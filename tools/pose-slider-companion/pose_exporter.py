"""Extract one person's dance pose from a video into the Slider Toolbox 3 JSON format."""

from __future__ import annotations

import argparse
import json
import math
import os
import queue
import threading
import urllib.request
from pathlib import Path
from typing import Callable, Optional


SCHEMA = "osu-pose-sequence"
SCHEMA_VERSION = 1
SKELETON = "osu.simple-body.v1"
COORDINATE_SYSTEM = "normalized-top-left"
MAX_FRAMES = 20_000
MIN_JOINT_CONFIDENCE = 0.15
CANONICAL_JOINTS = (
    "head", "neck", "shoulderLeft", "shoulderRight", "elbowLeft", "elbowRight",
    "wristLeft", "wristRight", "hipLeft", "hipRight", "kneeLeft", "kneeRight",
    "ankleLeft", "ankleRight",
)
MODEL_URLS = {
    "lite": "https://storage.googleapis.com/mediapipe-models/pose_landmarker/pose_landmarker_lite/float16/1/pose_landmarker_lite.task",
    "full": "https://storage.googleapis.com/mediapipe-models/pose_landmarker/pose_landmarker_full/float16/1/pose_landmarker_full.task",
    "heavy": "https://storage.googleapis.com/mediapipe-models/pose_landmarker/pose_landmarker_heavy/float16/1/pose_landmarker_heavy.task",
}
MODEL_FILENAMES = {name: f"pose_landmarker_{name}.task" for name in MODEL_URLS}


def _confidence(landmark) -> float:
    visibility = getattr(landmark, "visibility", None)
    presence = getattr(landmark, "presence", None)
    values = [float(value) for value in (visibility, presence) if value is not None and math.isfinite(float(value))]
    return min(1.0, max(0.0, min(values))) if values else 1.0


def _point(landmark) -> dict:
    return {"x": float(landmark.x), "y": float(landmark.y), "confidence": _confidence(landmark)}


def _midpoint(first: dict, second: dict) -> dict:
    return {
        "x": (first["x"] + second["x"]) / 2,
        "y": (first["y"] + second["y"]) / 2,
        "confidence": min(first["confidence"], second["confidence"]),
    }


def map_pose_landmarks(landmarks: list) -> dict:
    """Map MediaPipe's 33 landmarks to the editor's small, model-independent skeleton."""
    if len(landmarks) < 29:
        raise ValueError(f"Expected the 33-point MediaPipe pose profile, received {len(landmarks)} landmarks.")

    indexed = {name: _point(landmarks[index]) for name, index in {
        "nose": 0,
        "left_ear": 7,
        "right_ear": 8,
        "shoulderLeft": 11,
        "shoulderRight": 12,
        "elbowLeft": 13,
        "elbowRight": 14,
        "wristLeft": 15,
        "wristRight": 16,
        "hipLeft": 23,
        "hipRight": 24,
        "kneeLeft": 25,
        "kneeRight": 26,
        "ankleLeft": 27,
        "ankleRight": 28,
    }.items()}

    neck = _midpoint(indexed["shoulderLeft"], indexed["shoulderRight"])
    hips = _midpoint(indexed["hipLeft"], indexed["hipRight"])
    ears = [indexed["left_ear"], indexed["right_ear"]]
    visible_ears = [point for point in ears if point["confidence"] >= 0.15]

    if len(visible_ears) == 2:
        head = _midpoint(visible_ears[0], visible_ears[1])
    elif visible_ears:
        head = visible_ears[0]
    elif indexed["nose"]["confidence"] >= 0.15:
        # The pose model's nose point is used only as a head-location fallback; no face is identified or rendered.
        head = indexed["nose"]
    else:
        head = {
            "x": neck["x"] + (neck["x"] - hips["x"]) * 0.28,
            "y": neck["y"] + (neck["y"] - hips["y"]) * 0.28,
            "confidence": min(neck["confidence"], hips["confidence"]),
        }

    joints = {
        "head": head,
        "neck": neck,
        "shoulderLeft": indexed["shoulderLeft"],
        "shoulderRight": indexed["shoulderRight"],
        "elbowLeft": indexed["elbowLeft"],
        "elbowRight": indexed["elbowRight"],
        "wristLeft": indexed["wristLeft"],
        "wristRight": indexed["wristRight"],
        "hipLeft": indexed["hipLeft"],
        "hipRight": indexed["hipRight"],
        "kneeLeft": indexed["kneeLeft"],
        "kneeRight": indexed["kneeRight"],
        "ankleLeft": indexed["ankleLeft"],
        "ankleRight": indexed["ankleRight"],
    }
    return joints


def preview_frame_statistics(frame: dict) -> tuple[int, int, float]:
    """Return usable/missing canonical joints and their mean confidence for preview diagnostics."""
    joints = frame.get("joints") or {}
    confidences = []
    for name in CANONICAL_JOINTS:
        point = joints.get(name)
        if not isinstance(point, dict):
            continue
        try:
            confidence = float(point.get("confidence", 1.0))
        except (TypeError, ValueError):
            continue
        if math.isfinite(confidence) and MIN_JOINT_CONFIDENCE <= confidence <= 1:
            confidences.append(confidence)
    return len(confidences), len(CANONICAL_JOINTS) - len(confidences), sum(confidences) / len(confidences) if confidences else 0.0


def ensure_model(model_name: str, model_directory: Path, progress: Optional[Callable[[str], None]] = None) -> Path:
    if model_name not in MODEL_URLS:
        raise ValueError(f"Unknown model '{model_name}'. Choose lite, full, or heavy.")

    model_directory.mkdir(parents=True, exist_ok=True)
    model_path = model_directory / MODEL_FILENAMES[model_name]
    if model_path.is_file() and model_path.stat().st_size > 1024:
        return model_path

    if progress:
        progress(f"Downloading the MediaPipe {model_name} pose model (first run only)…")

    temporary_path = model_path.with_suffix(model_path.suffix + ".download")
    try:
        request = urllib.request.Request(MODEL_URLS[model_name], headers={"User-Agent": "SliderToolbox3PoseCompanion/1.0"})
        with urllib.request.urlopen(request, timeout=60) as response, temporary_path.open("wb") as target:
            while True:
                chunk = response.read(1024 * 1024)
                if not chunk:
                    break
                target.write(chunk)
        if temporary_path.stat().st_size <= 1024:
            raise RuntimeError("The downloaded pose model is unexpectedly small.")
        os.replace(temporary_path, model_path)
    finally:
        if temporary_path.exists():
            temporary_path.unlink()

    return model_path


def extract_pose_sequence(
    video_path: Path,
    output_path: Path,
    model_name: str = "full",
    frame_stride: int = 1,
    progress: Optional[Callable[[int, int, str], None]] = None,
    model_directory: Optional[Path] = None,
) -> Path:
    if frame_stride < 1 or frame_stride > 30:
        raise ValueError("Frame interval must be between 1 and 30 source frames.")
    if not video_path.is_file():
        raise FileNotFoundError(f"Video file does not exist: {video_path}")
    if video_path.resolve() == output_path.resolve():
        raise ValueError("The output JSON path must not overwrite the source video.")

    try:
        import cv2
        import mediapipe as mp
        from mediapipe.tasks import python
        from mediapipe.tasks.python import vision
    except ImportError as error:
        raise RuntimeError("Dependencies are missing. Run setup.bat in the companion tool folder first.") from error

    model_root = model_directory or Path(__file__).resolve().parent / "models"
    model_path = ensure_model(model_name, model_root, lambda message: progress(0, 0, message) if progress else None)
    capture = cv2.VideoCapture(str(video_path))
    if not capture.isOpened():
        raise RuntimeError(f"Could not open video: {video_path}")

    fps = float(capture.get(cv2.CAP_PROP_FPS))
    if not math.isfinite(fps) or fps <= 0:
        capture.release()
        raise RuntimeError("The video does not report a usable frame rate.")

    total_frames = int(capture.get(cv2.CAP_PROP_FRAME_COUNT))
    width = int(capture.get(cv2.CAP_PROP_FRAME_WIDTH))
    height = int(capture.get(cv2.CAP_PROP_FRAME_HEIGHT))
    if total_frames <= 0 or width <= 0 or height <= 0:
        capture.release()
        raise RuntimeError("The video has invalid duration or image dimensions.")

    sampled_count = (total_frames + frame_stride - 1) // frame_stride
    if sampled_count > MAX_FRAMES:
        capture.release()
        raise RuntimeError(f"This video would export {sampled_count} poses. Increase the frame interval so the result stays below {MAX_FRAMES} frames.")

    options = vision.PoseLandmarkerOptions(
        base_options=python.BaseOptions(model_asset_path=str(model_path)),
        running_mode=vision.RunningMode.VIDEO,
        num_poses=1,
        min_pose_detection_confidence=0.35,
        min_pose_presence_confidence=0.35,
        min_tracking_confidence=0.35,
        output_segmentation_masks=False,
    )

    frames = []
    found_poses = 0
    frame_index = 0
    last_timestamp_ms = -1

    try:
        with vision.PoseLandmarker.create_from_options(options) as landmarker:
            while True:
                ok, bgr_image = capture.read()
                if not ok:
                    break

                current_index = frame_index
                frame_index += 1
                if current_index % frame_stride != 0:
                    continue

                timestamp_ms = max(last_timestamp_ms + 1, round(current_index * 1000 / fps))
                last_timestamp_ms = timestamp_ms
                rgb_image = cv2.cvtColor(bgr_image, cv2.COLOR_BGR2RGB)
                image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb_image)
                result = landmarker.detect_for_video(image, timestamp_ms)

                joints = map_pose_landmarks(result.pose_landmarks[0]) if result.pose_landmarks else {}
                found_poses += bool(joints)
                frames.append({"timestampMs": timestamp_ms, "joints": joints})

                if progress and (len(frames) == 1 or len(frames) % 4 == 0 or frame_index >= total_frames):
                    progress(len(frames), sampled_count, f"Recognised {len(frames)} / {sampled_count} source samples…")
    finally:
        capture.release()

    if len(frames) < 2 or found_poses < 2:
        raise RuntimeError("Fewer than two poses were detected. Try a clearer, full-body video or a different model quality.")

    document = {
        "schema": SCHEMA,
        "version": SCHEMA_VERSION,
        "skeleton": SKELETON,
        "coordinateSystem": COORDINATE_SYSTEM,
        "source": {
            "fileName": video_path.name,
            "model": f"mediapipe-pose-landmarker-{model_name}",
            "width": width,
            "height": height,
            "frameRate": fps,
            "durationMs": total_frames * 1000 / fps,
        },
        "frames": frames,
    }

    output_path.parent.mkdir(parents=True, exist_ok=True)
    temporary_output = output_path.with_suffix(output_path.suffix + ".tmp")
    try:
        with temporary_output.open("w", encoding="utf-8", newline="\n") as output:
            json.dump(document, output, ensure_ascii=False, separators=(",", ":"))
            output.write("\n")
        os.replace(temporary_output, output_path)
    finally:
        if temporary_output.exists():
            temporary_output.unlink()

    if progress:
        progress(sampled_count, sampled_count, f"Export complete: {output_path}")
    return output_path


def launch_gui() -> None:
    import tkinter as tk
    from tkinter import filedialog, messagebox, ttk

    root = tk.Tk()
    root.title("Slider Toolbox 3 — Pose Companion")
    root.geometry("1120x720")
    root.minsize(900, 600)

    video_var = tk.StringVar()
    output_var = tk.StringVar()
    model_var = tk.StringVar(value="full")
    stride_var = tk.IntVar(value=1)
    status_var = tk.StringVar(value="Choose a dance video, or open an existing pose JSON to inspect it.")
    preview_info_var = tk.StringVar(value="No pose sequence loaded.")
    messages: queue.Queue = queue.Queue()
    preview_document: dict = {}
    preview_frames: list[dict] = []
    preview_index = 0
    playing = False
    playback_job = None
    busy = False

    main = ttk.Frame(root, padding=16)
    main.pack(fill="both", expand=True)
    ttk.Label(main, text="Pose recognition companion", font=("Segoe UI", 16, "bold")).pack(anchor="w")
    ttk.Label(main, text="Inspect the recognised stick figure frame by frame before importing it into osu! Editor.", wraplength=1000).pack(anchor="w", pady=(4, 12))

    workspace = ttk.Frame(main)
    workspace.pack(fill="both", expand=True)
    controls = ttk.Frame(workspace, width=390)
    controls.pack(side="left", fill="y", padx=(0, 12))
    controls.pack_propagate(False)
    preview = ttk.LabelFrame(workspace, text="Pose result preview", padding=8)
    preview.pack(side="left", fill="both", expand=True)

    def file_row(parent, label: str, variable: tk.StringVar, command: Callable[[], None]) -> None:
        row = ttk.Frame(parent)
        row.pack(fill="x", pady=4)
        ttk.Label(row, text=label, width=12).pack(side="left")
        ttk.Entry(row, textvariable=variable).pack(side="left", fill="x", expand=True, padx=6)
        ttk.Button(row, text="Browse…", command=command).pack(side="right")

    def choose_video() -> None:
        path = filedialog.askopenfilename(title="Choose a dance video", filetypes=[("Video files", "*.mp4 *.mov *.mkv *.avi *.webm"), ("All files", "*.*")])
        if path:
            video_var.set(path)
            output_var.set(str(Path(path).with_suffix(".osu-pose.json")))

    def choose_output() -> None:
        path = filedialog.asksaveasfilename(title="Save pose sequence", defaultextension=".json", filetypes=[("Pose sequence", "*.json")], initialfile=Path(output_var.get() or "pose-sequence.json").name)
        if path:
            output_var.set(path)

    file_row(controls, "Video", video_var, choose_video)
    file_row(controls, "Output JSON", output_var, choose_output)

    options = ttk.LabelFrame(controls, text="Recognition", padding=10)
    options.pack(fill="x", pady=(10, 8))
    ttk.Label(options, text="Model quality").grid(row=0, column=0, sticky="w", padx=(0, 8))
    ttk.Combobox(options, textvariable=model_var, values=("lite", "full", "heavy"), state="readonly", width=10).grid(row=0, column=1, sticky="w")
    ttk.Label(options, text="Nth frame").grid(row=0, column=2, sticky="w", padx=(16, 6))
    tk.Spinbox(options, from_=1, to=30, textvariable=stride_var, width=5).grid(row=0, column=3, sticky="w")
    ttk.Label(options, text="Higher quality costs more processing time. The model is downloaded once and reused.", wraplength=340).grid(row=1, column=0, columnspan=4, sticky="w", pady=(8, 0))

    ttk.Button(controls, text="Open pose JSON for preview…", command=lambda: choose_preview_json()).pack(fill="x", pady=(2, 8))
    export_button = ttk.Button(controls, text="Recognise and export", width=28)
    export_button.pack(fill="x", pady=(0, 6))

    preview_canvas = tk.Canvas(preview, background="#111821", highlightthickness=0, height=450)
    preview_canvas.pack(fill="both", expand=True)
    preview_canvas.bind("<Configure>", lambda _event: draw_preview())
    ttk.Label(preview, textvariable=preview_info_var).pack(fill="x", pady=(6, 2))
    scrub_row = ttk.Frame(preview)
    scrub_row.pack(fill="x")
    play_button = ttk.Button(scrub_row, text="Play", width=8)
    play_button.pack(side="left", padx=(0, 8))
    frame_scale = ttk.Scale(scrub_row, from_=0, to=0, orient="horizontal", command=lambda value: draw_preview(int(float(value))))
    frame_scale.pack(side="left", fill="x", expand=True)
    frame_counter = ttk.Label(scrub_row, text="0 / 0", width=11, anchor="e")
    frame_counter.pack(side="right", padx=(8, 0))

    progress_bar = ttk.Progressbar(main, mode="determinate", maximum=100)
    progress_bar.pack(fill="x", pady=(10, 4))
    ttk.Label(main, textvariable=status_var, wraplength=1000).pack(fill="x", pady=(2, 0))

    def draw_preview(index: Optional[int] = None) -> None:
        nonlocal preview_index
        preview_canvas.delete("all")
        canvas_width = max(1, preview_canvas.winfo_width())
        canvas_height = max(1, preview_canvas.winfo_height())

        if not preview_frames:
            preview_canvas.create_text(canvas_width / 2, canvas_height / 2, text="Choose a video and run recognition,\nor open a previously exported pose JSON.", fill="#aeb9c6", justify="center", font=("Segoe UI", 12))
            return

        preview_index = min(max(0, int(index if index is not None else preview_index)), len(preview_frames) - 1)
        source = preview_document.get("source") or {}
        source_width = max(1, int(source.get("width", 1)))
        source_height = max(1, int(source.get("height", 1)))
        margin = 18
        scale = min((canvas_width - margin * 2) / source_width, (canvas_height - margin * 2) / source_height)
        if not math.isfinite(scale) or scale <= 0:
            return
        rendered_width = source_width * scale
        rendered_height = source_height * scale
        origin_x = (canvas_width - rendered_width) / 2
        origin_y = (canvas_height - rendered_height) / 2

        def to_canvas(point: dict) -> tuple[float, float]:
            return origin_x + float(point["x"]) * source_width * scale, origin_y + float(point["y"]) * source_height * scale

        preview_canvas.create_rectangle(origin_x, origin_y, origin_x + rendered_width, origin_y + rendered_height, fill="#18232e", outline="#344453")
        frame = preview_frames[preview_index]
        joints = frame.get("joints") or {}
        usable_count, missing_count, confidence = preview_frame_statistics(frame)

        hip_left = joints.get("hipLeft")
        hip_right = joints.get("hipRight")
        shoulder_left = joints.get("shoulderLeft")
        shoulder_right = joints.get("shoulderRight")
        if all(point is not None for point in (hip_left, hip_right, shoulder_left, shoulder_right)):
            torso = {
                "x": (float(shoulder_left["x"]) + float(shoulder_right["x"])) * 0.175 + (float(hip_left["x"]) + float(hip_right["x"])) * 0.325,
                "y": (float(shoulder_left["y"]) + float(shoulder_right["y"])) * 0.175 + (float(hip_left["y"]) + float(hip_right["y"])) * 0.325,
                "confidence": min(float(point.get("confidence", 1)) for point in (hip_left, hip_right, shoulder_left, shoulder_right)),
            }
            joints_to_draw = dict(joints)
            joints_to_draw["torso"] = torso
        else:
            joints_to_draw = joints

        def draw_bone(first_name: str, second_name: str) -> None:
            first = joints_to_draw.get(first_name)
            second = joints_to_draw.get(second_name)
            if first is None or second is None:
                return
            x1, y1 = to_canvas(first)
            x2, y2 = to_canvas(second)
            confidence_low = min(float(first.get("confidence", 1)), float(second.get("confidence", 1))) < MIN_JOINT_CONFIDENCE
            options = {"fill": "#657e92" if not confidence_low else "#bb6575", "width": 3}
            if confidence_low:
                options["dash"] = (4, 3)
            preview_canvas.create_line(x1, y1, x2, y2, **options)

        for side in ("Left", "Right"):
            draw_bone("neck", f"shoulder{side}")
            draw_bone(f"shoulder{side}", f"elbow{side}")
            draw_bone(f"elbow{side}", f"wrist{side}")
            draw_bone("torso", f"hip{side}")
            draw_bone(f"hip{side}", f"knee{side}")
            draw_bone(f"knee{side}", f"ankle{side}")
        draw_bone("neck", "torso")

        head = joints.get("head")
        neck = joints.get("neck")
        if head is not None and neck is not None:
            hx, hy = to_canvas(head)
            nx, ny = to_canvas(neck)
            shoulder_distance = 0.0
            if shoulder_left is not None and shoulder_right is not None:
                left_xy = to_canvas(shoulder_left)
                right_xy = to_canvas(shoulder_right)
                shoulder_distance = math.dist(left_xy, right_xy)
            radius = max(math.dist((hx, hy), (nx, ny)) * 0.62, shoulder_distance * 0.1, 3)
            low = min(float(head.get("confidence", 1)), float(neck.get("confidence", 1))) < MIN_JOINT_CONFIDENCE
            ring_options = {"outline": "#72c9ee" if not low else "#e07989", "width": 2}
            if low:
                ring_options["dash"] = (4, 3)
            preview_canvas.create_oval(hx - radius, hy - radius, hx + radius, hy + radius, **ring_options)
            preview_canvas.create_line(hx, hy + radius, nx, ny, fill="#657e92" if not low else "#bb6575", width=3)

        for name in CANONICAL_JOINTS:
            point = joints.get(name)
            if point is None:
                continue
            x, y = to_canvas(point)
            low = float(point.get("confidence", 1)) < MIN_JOINT_CONFIDENCE
            radius = 4 if not low else 3
            colour = "#7bd8ff" if not low else "#ff6f83"
            preview_canvas.create_oval(x - radius, y - radius, x + radius, y + radius, fill=colour, outline="#111821")

        if not joints:
            preview_canvas.create_text(canvas_width / 2, canvas_height / 2, text="No pose detected in this frame", fill="#ff9aa8", font=("Segoe UI", 12, "bold"))

        timestamp = float(frame.get("timestampMs", 0))
        preview_info_var.set(f"{timestamp:.1f} ms   •   usable joints {usable_count}/{len(CANONICAL_JOINTS)}   •   missing/low confidence {missing_count}   •   mean confidence {confidence:.2f}")
        frame_counter.configure(text=f"{preview_index + 1} / {len(preview_frames)}")

    def load_preview_json(path: Path) -> None:
        nonlocal preview_document, preview_frames, preview_index, playing
        try:
            with path.open("r", encoding="utf-8") as file:
                document = json.load(file)
            if not isinstance(document, dict):
                raise ValueError("The pose sequence root must be a JSON object.")
            if document.get("schema") != SCHEMA or document.get("version") != SCHEMA_VERSION:
                raise ValueError("This is not a supported osu-pose-sequence v1 file.")
            frames = document.get("frames")
            if not isinstance(frames, list) or not frames:
                raise ValueError("The pose sequence contains no frames.")
            source = document.get("source") or {}
            if not isinstance(source, dict):
                raise ValueError("The pose sequence has invalid source metadata.")
            if int(source.get("width", 0)) <= 0 or int(source.get("height", 0)) <= 0:
                raise ValueError("The pose sequence has invalid source dimensions.")
            previous_timestamp = -math.inf
            for frame in frames:
                if not isinstance(frame, dict):
                    raise ValueError("The pose sequence contains a frame with an invalid timestamp.")
                timestamp = float(frame.get("timestampMs", float("nan")))
                if not math.isfinite(timestamp) or timestamp < 0 or timestamp <= previous_timestamp:
                    raise ValueError("Frame timestamps must be finite, non-negative, and strictly increasing.")
                previous_timestamp = timestamp
                joints = frame.get("joints") or {}
                if not isinstance(joints, dict):
                    raise ValueError("A pose frame has an invalid joints object.")
                for point in joints.values():
                    if not isinstance(point, dict):
                        raise ValueError("A pose joint is not an object.")
                    x = float(point.get("x", float("nan")))
                    y = float(point.get("y", float("nan")))
                    confidence = float(point.get("confidence", 1))
                    if not all(math.isfinite(value) for value in (x, y, confidence)) or not 0 <= confidence <= 1:
                        raise ValueError("A pose joint contains invalid coordinates or confidence.")
            preview_document = document
            preview_frames = frames
            preview_index = 0
            stop_playback()
            frame_scale.configure(to=max(0, len(frames) - 1))
            frame_scale.set(0)
            draw_preview(0)
            status_var.set(f"Preview loaded: {path}")
        except (OSError, ValueError, TypeError, OverflowError, json.JSONDecodeError) as error:
            messagebox.showerror("Could not preview pose JSON", str(error), parent=root)

    def choose_preview_json() -> None:
        path = filedialog.askopenfilename(title="Open pose sequence preview", filetypes=[("Pose sequence JSON", "*.json"), ("All files", "*.*")])
        if path:
            load_preview_json(Path(path))

    def advance_preview() -> None:
        nonlocal playback_job
        playback_job = None
        if not playing or not preview_frames:
            return
        next_index = (preview_index + 1) % len(preview_frames)
        previous_time = float(preview_frames[preview_index].get("timestampMs", 0))
        next_time = float(preview_frames[next_index].get("timestampMs", previous_time + 100))
        delay = max(20, min(1000, int(next_time - previous_time)))
        frame_scale.set(next_index)
        draw_preview(next_index)
        playback_job = root.after(delay, advance_preview)

    def stop_playback() -> None:
        nonlocal playing, playback_job
        playing = False
        if playback_job is not None:
            root.after_cancel(playback_job)
            playback_job = None
        play_button.configure(text="Play")

    def toggle_playback() -> None:
        nonlocal playing
        if playing:
            stop_playback()
            return
        if preview_frames:
            playing = True
            play_button.configure(text="Pause")
            advance_preview()

    play_button.configure(command=toggle_playback)

    def begin() -> None:
        nonlocal busy
        if busy:
            return
        if not video_var.get() or not output_var.get():
            messagebox.showerror("Missing file", "Choose both the source video and output JSON path.", parent=root)
            return
        try:
            stride = int(stride_var.get())
        except (ValueError, tk.TclError):
            messagebox.showerror("Invalid frame interval", "Frame interval must be an integer from 1 to 30.", parent=root)
            return

        busy = True
        selected_video = Path(video_var.get())
        selected_output = Path(output_var.get())
        selected_model = model_var.get()
        export_button.configure(state="disabled")
        progress_bar.configure(value=0)
        status_var.set("Preparing local pose recognition…")

        def report(done: int, total: int, message: str) -> None:
            messages.put((done, total, message, None))

        def work() -> None:
            try:
                path = extract_pose_sequence(selected_video, selected_output, selected_model, stride, report)
                messages.put((0, 0, f"Pose sequence exported to {path}", path))
            except Exception as error:  # surfaced in the desktop UI
                messages.put((0, 0, str(error), "error"))

        threading.Thread(target=work, daemon=True).start()

    export_button.configure(command=begin)

    def drain_messages() -> None:
        nonlocal busy
        try:
            while True:
                done, total, message, terminal = messages.get_nowait()
                status_var.set(message)
                if total > 0:
                    progress_bar.configure(value=100 * done / total)
                if terminal:
                    busy = False
                    export_button.configure(state="normal")
                    if isinstance(terminal, Path):
                        load_preview_json(terminal)
                        messagebox.showinfo("Export complete", message, parent=root)
                    else:
                        messagebox.showerror("Pose export failed", message, parent=root)
        except queue.Empty:
            pass
        root.after(120, drain_messages)

    root.after(120, drain_messages)
    root.mainloop()


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--video", type=Path, help="Video to analyse. Omit to open the desktop UI.")
    parser.add_argument("--output", type=Path, help="Output pose-sequence JSON path.")
    parser.add_argument("--model", choices=tuple(MODEL_URLS), default="full")
    parser.add_argument("--frame-stride", type=int, default=1, help="Run recognition on every Nth source frame (1-30).")
    arguments = parser.parse_args()

    if arguments.video is None:
        launch_gui()
        return
    if arguments.output is None:
        parser.error("--output is required when --video is specified")

    destination = extract_pose_sequence(arguments.video, arguments.output, arguments.model, arguments.frame_stride, lambda done, total, message: print(message))
    print(f"Exported: {destination}")


if __name__ == "__main__":
    main()
