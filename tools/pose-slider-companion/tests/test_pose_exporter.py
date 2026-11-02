import importlib.util
import tempfile
import unittest
from pathlib import Path


MODULE_PATH = Path(__file__).resolve().parents[1] / "pose_exporter.py"
SPEC = importlib.util.spec_from_file_location("pose_exporter", MODULE_PATH)
pose_exporter = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(pose_exporter)


class FakeLandmark:
    def __init__(self, x, y, visibility=1.0):
        self.x = x
        self.y = y
        self.visibility = visibility


class PoseExporterTests(unittest.TestCase):
    def test_maps_model_joints_to_canonical_skeleton_without_face_data(self):
        landmarks = [FakeLandmark(0.5, 0.5) for _ in range(33)]
        landmarks[7] = FakeLandmark(0.46, 0.18, 0.8)
        landmarks[8] = FakeLandmark(0.54, 0.18, 0.9)
        landmarks[11] = FakeLandmark(0.42, 0.3, 0.95)
        landmarks[12] = FakeLandmark(0.58, 0.3, 0.93)
        landmarks[23] = FakeLandmark(0.46, 0.55, 0.91)
        landmarks[24] = FakeLandmark(0.54, 0.55, 0.90)

        joints = pose_exporter.map_pose_landmarks(landmarks)

        self.assertEqual(set(joints), set((
            "head", "neck", "shoulderLeft", "shoulderRight", "elbowLeft", "elbowRight",
            "wristLeft", "wristRight", "hipLeft", "hipRight", "kneeLeft", "kneeRight",
            "ankleLeft", "ankleRight",
        )))
        self.assertAlmostEqual(joints["head"]["x"], 0.5)
        self.assertAlmostEqual(joints["head"]["y"], 0.18)
        self.assertAlmostEqual(joints["neck"]["x"], 0.5)
        self.assertEqual(joints["neck"]["confidence"], 0.93)

    def test_falls_back_to_body_axis_for_unavailable_head_points(self):
        landmarks = [FakeLandmark(0.5, 0.5, 0.0) for _ in range(33)]
        landmarks[11] = FakeLandmark(0.4, 0.3, 0.9)
        landmarks[12] = FakeLandmark(0.6, 0.3, 0.9)
        landmarks[23] = FakeLandmark(0.45, 0.6, 0.8)
        landmarks[24] = FakeLandmark(0.55, 0.6, 0.8)

        joints = pose_exporter.map_pose_landmarks(landmarks)

        self.assertLess(joints["head"]["y"], joints["neck"]["y"])
        self.assertEqual(joints["head"]["confidence"], 0.8)

    def test_rejects_incompatible_model_topology(self):
        with self.assertRaisesRegex(ValueError, "Expected the 33-point"):
            pose_exporter.map_pose_landmarks([FakeLandmark(0, 0) for _ in range(17)])

    def test_preview_statistics_flag_missing_and_low_confidence_joints(self):
        frame = {
            "joints": {
                "head": {"x": 0.5, "y": 0.2, "confidence": 0.9},
                "neck": {"x": 0.5, "y": 0.3, "confidence": 0.05},
                "wristLeft": {"x": 0.4, "y": 0.5, "confidence": 0.8},
            }
        }

        usable, missing, confidence = pose_exporter.preview_frame_statistics(frame)
        self.assertEqual((usable, missing), (2, 12))
        self.assertAlmostEqual(confidence, 0.85)

    def test_refuses_to_overwrite_source_video(self):
        with tempfile.TemporaryDirectory() as directory:
            video_path = Path(directory) / "dance.mp4"
            video_path.write_bytes(b"not a real video")

            with self.assertRaisesRegex(ValueError, "must not overwrite"):
                pose_exporter.extract_pose_sequence(video_path, video_path)


if __name__ == "__main__":
    unittest.main()
