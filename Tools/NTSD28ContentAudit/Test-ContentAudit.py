import importlib.util
import json
import struct
import tempfile
import unittest
from pathlib import Path

spec = importlib.util.spec_from_file_location("audit", Path(__file__).with_name("Audit-Content.py"))
audit = importlib.util.module_from_spec(spec)
spec.loader.exec_module(audit)


class ContentAuditTests(unittest.TestCase):
    def test_registry_preserves_duplicate_ids_and_rejects_unknown_lines(self):
        with tempfile.TemporaryDirectory() as temp:
            path = Path(temp) / "data.txt"
            path.write_text("<object>\nid: 2 type: 0 file: c\\nar\\nar.dat\nid: 2 type: 3 file: other.dat\nnot a record\n<object_end>\n<background>\nid: 2 file: bg.dat\n<background_end>", encoding="utf-8")
            rows, rejected = audit.registry(path)
            self.assertEqual(3, len(rows))
            self.assertEqual([2, 2, 2], [r["id"] for r in rows])
            self.assertEqual("background", rows[2]["section"])
            self.assertEqual(4, rejected[0]["line"])

    def test_png_metadata_and_native_bmp_fallback_are_separate_from_unity_path(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            image = root / "a.png"
            image.write_bytes(b"\x89PNG\r\n\x1a\n" + b"\x00\x00\x00\x0dIHDR" + struct.pack(">II", 79, 158))
            self.assertEqual({"format": "PNG", "width": 79, "height": 158}, audit.image_info(image))
            self.assertEqual("NATIVE_BMP_TO_PNG_FALLBACK", audit.resolve_image(root, "a.bmp")[1])
            self.assertEqual("MISSING", audit.resolve_image(root, "a.bmp", dat_dir=root)[1])

    def test_registry_trailing_comment_does_not_become_part_of_asset_path(self):
        with tempfile.TemporaryDirectory() as temp:
            path = Path(temp) / "data.txt"
            path.write_text("<object>\nid: 101 type: 1 file: chars/weapon2.dat #ex_tag\n<object_end>", encoding="utf-8")
            rows, rejected = audit.registry(path)
            self.assertEqual([], rejected)
            self.assertEqual("chars/weapon2.dat", rows[0]["sourcePath"])

    def test_missing_typed_zero_is_not_falsely_equal_and_explicit_count_is_kept(self):
        source = {"a.dat": {"frames": [{"id": 4, "subblocks": [{"kind": "opoint", "fields": [{"key": "dvz", "value": "0"}], "normalized": {"dvz": 0, "hp": 0}}]}]}}
        actual = {"a.dat": {"frames": [{"id": 4, "conversionError": "frame rejected", "subblocks": [{"kind": "opoint", "fields": [], "normalized": {}}]}]}}
        rows, issues, samples = audit.compare_capture(source, actual)
        self.assertEqual(2, rows[0]["DTO_FIELD_MISSING"])
        self.assertEqual(1, rows[0]["rejectedFrames"])
        self.assertEqual(1, next(i for i in issues if i["field"] == "dvz")["explicitOccurrences"])
        self.assertEqual(0, next(i for i in issues if i["field"] == "hp")["explicitOccurrences"])

    def test_duplicate_frames_are_not_collapsed(self):
        self.assertEqual([(1, 0), (1, 1)], list(audit.frame_map({"frames": [{"id": 1}, {"id": 1}]})))

    def test_raw_body_and_scalar_array_carriers_are_review_not_confirmed_semantic_gaps(self):
        def capture(kind, values):
            return {"a.dat": {"frames": [{"id": 1, "subblocks": [{"kind": kind, "normalized": values}]}]}}
        _, issues, _ = audit.compare_capture(capture("bdy", {"z": 0}), capture("bdy", {}))
        self.assertEqual("PARSED_BODY_FIELD_REVIEW", issues[0]["issue"])
        _, issues, _ = audit.compare_capture(capture("itr", {"caughtact": 10}), capture("itr", {"caughtact": [10, 11]}))
        self.assertEqual("REPRESENTATION_SHAPE_REVIEW", issues[0]["issue"])

    def test_sprite_effective_range_difference_is_not_hidden_by_identical_declared_range(self):
        native = {"a.dat": {"sprites": [{"path": "a.png", "declaredFirst": 100, "effectiveFirst": 0}]}}
        unity = {"a.dat": {"sprites": [{"path": "a.png", "declaredFirst": 100, "effectiveFirst": 100}]}}
        rows = audit.compare_sprite_layouts(native, unity)
        self.assertEqual(1, len(rows))
        self.assertEqual("effectiveFirst", rows[0]["field"])

    def test_output_cannot_overwrite_input_or_its_parent(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp) / "inputs"
            root.mkdir()
            with self.assertRaises(ValueError):
                audit.ensure_output(root / "reports", [root])
            with self.assertRaises(ValueError):
                audit.ensure_output(Path(temp), [root])

    def test_incomplete_capture_cannot_produce_successful_inventory(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            (root / "a.dat").write_text("fixture", encoding="utf-8")
            with self.assertRaises(ValueError):
                audit.verify_capture_coverage({}, root)
            self.assertEqual(1, audit.verify_capture_coverage({"a.dat": {}}, root))

    def test_logical_identity_rejects_traversal_instead_of_hiding_it(self):
        self.assertEqual("c/nar.dat", audit.key("./c\\nar.dat"))
        for unsafe in ("../c/nar.dat", "c/../../nar.dat", "C:/c/nar.dat", "/c/nar.dat"):
            with self.assertRaises(ValueError):
                audit.key(unsafe)

    def test_menu_face_reference_kept_separate_from_battle_images(self):
        document = {"menuFace": [{"fields": [{"key": "pic", "value": "c/0/M.png"}]}]}
        self.assertEqual([(0, "c/0/M.png")], audit.menu_image_references(document))
        self.assertEqual([], audit.image_references(document))

    def test_capture_identity_rejects_changed_source_input_and_wrong_mode(self):
        with tempfile.TemporaryDirectory() as temp:
            repo, authority, receipt = (Path(temp) / name for name in ("repo", "authority", "receipt"))
            native_root = authority / "resources/runtime/decoded_dat"
            old_root = repo / "Assets/NTSD/Config"
            for root in (native_root, old_root, receipt, authority / "source"):
                root.mkdir(parents=True)
            for root in (native_root, old_root):
                (root / "a.dat").write_text("data", encoding="utf-8")
            source = repo / "parser.cs"
            stub = repo / "stub.cs"
            source.write_text("parser", encoding="utf-8")
            stub.write_text("stub", encoding="utf-8")
            output = receipt / "capture.jsonl"
            native = {"a.dat": {"sourceModel": "SOURCE_MODEL_DIAGNOSTIC_ONLY"}}
            output.write_text(json.dumps(native), encoding="utf-8")
            audit.write_json(receipt / "capture-stability.json", {"stable": True, "firstSha256": audit.sha(output)})
            audit.write_json(receipt / "build-manifest.json", {"authoritySourceRoot": str(authority / "source"),
                "sources": [{"path": str(source), "sha256": audit.sha(source)}], "conservativeHeaders": []})
            for suffix in ("before", "after"):
                (receipt / f"authority-input-sha256-{suffix}.jsonl").write_text(json.dumps(
                    {"path": "a.dat", "sha256": audit.sha(native_root / "a.dat")}) + "\n", encoding="utf-8")
            def unity_capture(root, mode):
                return {"a.dat": {"auditId": "NTSD28-B11-CONTENT-ENTRY-INVENTORY-001", "inputMode": mode,
                    "inputIdentity": {"absoluteRoot": str(root), "sha256": audit.sha(root / "a.dat")},
                    "source": {"loadsLibraryScriptAssemblies": False, "linkedProductionSourceSha256": {"parser.cs": audit.sha(source)},
                        "stubBoundary": {"file": "stub.cs", "sha256": audit.sha(stub)}}}}
            unity, current = unity_capture(native_root, "plaintext"), unity_capture(old_root, "unity")
            verify = lambda: audit.verify_source_capture_identity(native, output, unity, current, authority, repo)
            self.assertTrue(verify()["inputsAndSourcesMatch"])
            unity["a.dat"]["inputMode"] = "unity"
            with self.assertRaises(ValueError):
                verify()
            unity["a.dat"]["inputMode"] = "plaintext"
            (old_root / "a.dat").write_text("changed content", encoding="utf-8")
            with self.assertRaises(ValueError):
                verify()
            (old_root / "a.dat").write_text("data", encoding="utf-8")
            source.write_text("changed parser", encoding="utf-8")
            with self.assertRaises(ValueError):
                verify()


if __name__ == "__main__":
    unittest.main()
