"""Review immutable Unity GPU captures; never edits source images or project assets.
Alignment contract: NTSD-M12-SPRITE-VISUAL-EQUIVALENCE-001.
"""
import argparse
import hashlib
import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def save_json(path, value):
    with path.open("x", encoding="utf-8") as out:
        json.dump(value, out, ensure_ascii=False, indent=2)


def save_png(image, path, **kwargs):
    with path.open("xb") as out:
        image.save(out, format="PNG", **kwargs)


def caption(image, text):
    result = Image.new("RGB", (image.width, image.height + 42), (28, 34, 42))
    result.paste(image.convert("RGB"), (0, 42))
    ImageDraw.Draw(result).text((12, 7), text, font=FONT, fill=(235, 239, 245))
    return result


def metrics(a, b):
    delta = np.abs(a.astype(np.int16) - b.astype(np.int16))
    mask = np.any(delta != 0, axis=2)
    ys, xs = np.where(mask)
    return {
        "changedPixels": int(mask.sum()),
        "screenPercent": float(mask.mean() * 100),
        "maxRGBA": int(delta.max()),
        "meanRgbDiff": float(delta[:, :, :3].mean()),
        "highContrastPixels": int((delta.max(axis=2) > 16).sum()),
        "bbox": None if not len(xs) else [int(xs.min()), int(ys.min()), int(xs.max() + 1), int(ys.max() + 1)],
        "changedRows": int(np.any(mask, axis=1).sum()),
        "changedColumns": int(np.any(mask, axis=0).sum()),
    }


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--scan-run", type=Path, required=True)
    parser.add_argument("--selected-run", type=Path, required=True)
    parser.add_argument("--out", type=Path, required=True)
    args = parser.parse_args()
    if args.out.exists():
        raise RuntimeError("Refusing existing output directory")
    inputs = [*args.scan_run.glob("*.json"), *args.scan_run.glob("*.png"),
              *args.selected_run.glob("*.json"), *args.selected_run.glob("*.png")]
    before = {str(p.resolve()): digest(p) for p in inputs}
    scan = json.loads((args.scan_run / "scan.json").read_text(encoding="utf-8-sig"))
    motion = json.loads((args.scan_run / "motion.json").read_text(encoding="utf-8-sig"))
    selected = json.loads((args.selected_run / "summary.json").read_text(encoding="utf-8-sig"))
    chosen = [x for x in selected["files"] if x["label"].startswith(("ranked-", "worst-", "stress-"))]
    patch = sorted([x for x in selected["files"] if x["label"].startswith("patch11-")],
                   key=lambda x: x["row"]["meanRgbDiff"], reverse=True)[0]
    chosen.append(patch)
    planned = ["planned-outputs.json", "review.json", "input-protection.json", "motion-kirin-page.png"]
    for item in chosen:
        planned.extend(item["label"] + suffix for suffix in
                       ("-compare.png", "-hotspot.png", "-blink.png"))
    args.out.mkdir(parents=True)
    save_json(args.out / "planned-outputs.json", {
        "inputDirectories": [str(args.scan_run.resolve()), str(args.selected_run.resolve())],
        "outputs": planned,
        "semantics": "GPU inputs unchanged; contact previews downsample to50%, hotspot nearest3x, APNG blink preserves native1080 pixels. No perceptual PASS threshold."
    })
    results = []
    for item in chosen:
        a = Image.open(args.selected_run / item["original"]).convert("RGBA")
        b = Image.open(args.selected_run / item["derived"]).convert("RGBA")
        aa, bb = np.array(a), np.array(b)
        observed = metrics(aa, bb)
        assert observed["changedPixels"] == item["row"]["changedPixels"]
        assert observed["maxRGBA"] == item["row"]["maxAbsDiff"]
        # Full-frame preview labels explicitly distinguish display resize from native evidence.
        panels = [caption(im.resize((960, 540), Image.Resampling.LANCZOS), label)
                  for im, label in [(a, "原效果 / 1080p回读，预览缩至50%"),
                                    (b, "调整后 / 同样缩至50%")]]
        board = Image.new("RGB", (960, sum(p.height for p in panels)))
        y = 0
        for panel in panels:
            board.paste(panel, (0, y))
            y += panel.height
        save_png(board, args.out / (item["label"] + "-compare.png"))
        # Select the most densely changed aligned128x128 window, not a handpicked clean area.
        mask = np.any(aa != bb, axis=2)
        candidates = []
        for y in range(0, a.height, 128):
            for x in range(0, a.width, 128):
                candidates.append((int(mask[y:y + 128, x:x + 128].sum()), x, y))
        _, x, y = max(candidates)
        x, y = min(x, a.width - 128), min(y, a.height - 128)
        box = (x, y, x + 128, y + 128)
        diff = np.minimum(np.abs(aa.astype(np.int16) - bb.astype(np.int16))[:, :, :3] * 8, 255).astype(np.uint8)
        crops = [caption(im.crop(box).resize((384, 384), Image.Resampling.NEAREST), label)
                 for im, label in [(a, "原效果 / 局部3倍"), (b, "调整后 / 局部3倍"),
                                   (Image.fromarray(diff), "RGB差异放大8倍")]]
        crop_board = Image.new("RGB", (1152, 426))
        for i, panel in enumerate(crops):
            crop_board.paste(panel, (i * 384, 0))
        save_png(crop_board, args.out / (item["label"] + "-hotspot.png"))
        # Lossless APNG, no palette quantization; header swaps with each source/candidate image.
        blink_a, blink_b = caption(a, "原效果"), caption(b, "调整后")
        save_png(blink_a, args.out / (item["label"] + "-blink.png"),
                 save_all=True, append_images=[blink_b], duration=[850, 850], loop=0, disposal=0, blend=0)
        with Image.open(args.out / (item["label"] + "-blink.png")) as check:
            assert check.n_frames == 2
            check.seek(0)
            assert np.array_equal(np.array(check.convert("RGB"))[42:], aa[:, :, :3])
            check.seek(1)
            assert np.array_equal(np.array(check.convert("RGB"))[42:], bb[:, :, :3])
        results.append({"label": item["label"], "row": item["row"], "readback": observed,
                        "hotspot128": box, "apngNativePixelsVerified": True})
    animation = []
    temporal = []
    previous_error = None
    for step in range(16):
        a = Image.open(args.scan_run / f"motion-kirin-page-{step:02d}-original.png").convert("RGB")
        b = Image.open(args.scan_run / f"motion-kirin-page-{step:02d}-derived.png").convert("RGB")
        aa, bb = np.array(a).astype(np.int16), np.array(b).astype(np.int16)
        error = bb - aa
        temporal.append({
            "phase": step / 16,
            "errorChangedPixelsFromPrevious": None if previous_error is None
            else int(np.any(error != previous_error, axis=2).sum()),
            "maxErrorChangeFromPrevious": None if previous_error is None
            else int(np.abs(error - previous_error).max()),
        })
        previous_error = error
        board = Image.new("RGB", (1920, 582))
        board.paste(caption(a.resize((960, 540), Image.Resampling.LANCZOS), "原效果 / 亚像素移动"), (0, 0))
        board.paste(caption(b.resize((960, 540), Image.Resampling.LANCZOS), "调整后 / 相同移动"), (960, 0))
        animation.append(board)
    save_png(animation[0], args.out / "motion-kirin-page.png", save_all=True,
             append_images=animation[1:], duration=80, loop=0, disposal=0, blend=0)
    ranked = []
    seen = set()
    for item in sorted(scan, key=lambda x: x["meanRgbDiff"], reverse=True):
        if item["sample"] not in seen:
            seen.add(item["sample"])
            ranked.append(item)
    motion_max = []
    for group in sorted({x["group"] for x in motion}):
        for scale in sorted({x["scale"] for x in motion}):
            subset = [x for x in motion if x["group"] == group and x["scale"] == scale]
            motion_max.append(max(subset, key=lambda x: x["meanRgbDiff"]))
    after = {p: digest(Path(p)) for p in before}
    assert before == after
    save_json(args.out / "input-protection.json", {"unchanged": True, "count": len(before), "files": before})
    save_json(args.out / "review.json", {
        "status": "METRICS_AND_PREVIEWS_READY_FOR_HUMAN_VISUAL_REVIEW",
        "sourceScan": str(args.scan_run.resolve()), "selectedCapture": str(args.selected_run.resolve()),
        "scanCases": len(scan), "scanChangedCases": sum(x["changedPixels"] > 0 for x in scan),
        "topDistinctFrames": ranked[:10], "selectedPreviews": results, "motionWorstByGroupAndScale": motion_max,
        "motionErrorVariation": temporal,
        "limits": [
            "Primary1.40625 uses staticScene5.76 at1920x1080, background framing can change actual ratio.",
            "First scan patch-motion selected000 because all30static patch cases equal; use selected011 followup for detailed composite evidence.",
            "Stress cases intentionally include exact half-pixel ties; this is not actual DAT timing or mobile hardware.",
            "No perceptual acceptance is inferred from a changed-pixel fraction alone."
        ],
    })
    print(json.dumps({"status": "REVIEW_READY", "previews": len(results), "inputFilesUnchanged": len(before),
                      "out": str(args.out.resolve())}, ensure_ascii=False))


FONT = ImageFont.truetype("C:/Windows/Fonts/msyh.ttc", 23)
if __name__ == "__main__":
    main()
