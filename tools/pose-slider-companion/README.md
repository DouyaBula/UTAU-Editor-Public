# Slider Toolbox 3 Pose Companion

This separate Windows companion extracts a single dancer's pose track from a video and exports the versioned JSON consumed by osu! Editor's **Slider Toolbox 3**. The editor performs beat mapping and creates native sliders; this tool performs video recognition. Recognition runs locally after setup. The selected MediaPipe model is downloaded once into `models/` and is not included in this directory.

## Quick start

1. Install Python 3.12 (64-bit).
2. Run `setup.bat` once to create a local virtual environment and install MediaPipe.
3. Run `run.bat`, choose a video, select a model and frame interval, then click **Recognise and export**. The right-hand preview lets you scrub or play the exported stick-figure sequence and shows low-confidence/missing joints per frame; **Open pose JSON for preview…** can inspect an earlier export without rerunning recognition.
4. In osu! Editor, open **Generate → Slider Toolbox 3** and drop the exported `.osu-pose.json` into the editor while the panel is open, or use its built-in browser. Seek to the intended beginning and click **Lock start at cursor**, then seek to the ending and click **Lock end at cursor**. Set **Sample density (% of source frames)**, **Gap between sliders (snaps)** and placement, inspect the preview, then click **Apply**. Seeking alone never changes the locked range.

The current editor workflow and data boundaries are described in the [editor guide](../../docs/editor-guide.md). The companion preview shows the recognised skeleton; the editor preview shows how that skeleton is sampled and mapped to native sliders.

The companion supports **Lite**, **Full**, and **Heavy** Pose Landmarker models. Full is the default. The model bundle is fetched from Google's MediaPipe model storage on first use; videos are processed locally and are not uploaded by this tool.

For command-line use:

```powershell
run.bat --video "D:\dance.mp4" --output "D:\dance.osu-pose.json" --model full --frame-stride 1
```

`--frame-stride` runs recognition on every Nth source frame (1–30). Use a larger value for long clips; Slider Toolbox 3 interpolates missing source times before its BPM-aligned downsampling.

## Interchange format v1

Coordinates are normalised to the source frame (`x` and `y` use a top-left origin); confidence is in `[0, 1]`. Each frame has a source-video timestamp in milliseconds. The recogniser maps its native model joints into `osu.simple-body.v1`; model-specific joint layouts do not leak into the editor.

The JSON below is a shortened schema excerpt; an importable file must contain at least two frame entries.

```json
{
  "schema": "osu-pose-sequence",
  "version": 1,
  "skeleton": "osu.simple-body.v1",
  "coordinateSystem": "normalized-top-left",
  "source": {
    "fileName": "dance.mp4",
    "model": "mediapipe-pose-landmarker-full",
    "width": 1920,
    "height": 1080,
    "frameRate": 30,
    "durationMs": 5000
  },
  "frames": [
    {
      "timestampMs": 0,
      "joints": {
        "head": { "x": 0.50, "y": 0.16, "confidence": 0.91 },
        "neck": { "x": 0.50, "y": 0.28, "confidence": 0.94 },
        "shoulderLeft": { "x": 0.43, "y": 0.29, "confidence": 0.95 },
        "shoulderRight": { "x": 0.57, "y": 0.29, "confidence": 0.95 },
        "elbowLeft": { "x": 0.39, "y": 0.39, "confidence": 0.90 },
        "elbowRight": { "x": 0.61, "y": 0.39, "confidence": 0.90 },
        "wristLeft": { "x": 0.35, "y": 0.49, "confidence": 0.85 },
        "wristRight": { "x": 0.65, "y": 0.49, "confidence": 0.85 },
        "hipLeft": { "x": 0.46, "y": 0.53, "confidence": 0.94 },
        "hipRight": { "x": 0.54, "y": 0.53, "confidence": 0.94 },
        "kneeLeft": { "x": 0.45, "y": 0.70, "confidence": 0.91 },
        "kneeRight": { "x": 0.55, "y": 0.70, "confidence": 0.91 },
        "ankleLeft": { "x": 0.43, "y": 0.90, "confidence": 0.88 },
        "ankleRight": { "x": 0.57, "y": 0.90, "confidence": 0.88 }
      }
    }
  ]
}
```

Frames where no pose was detected may use an empty `joints` object. The editor interpolates missing/low-confidence joints between valid detections. The head ring is generated from the imported head/neck points; this format contains no face mesh or identity data.

## Development checks

The schema mapping tests use only the Python standard library:

```powershell
py -3.12 -m unittest discover -s tests -v
```
