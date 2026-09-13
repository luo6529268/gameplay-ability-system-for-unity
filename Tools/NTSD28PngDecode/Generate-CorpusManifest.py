"""Read-only PNG reference corpus using the installed Pillow decoder."""
import argparse
import hashlib
from pathlib import Path
from PIL import Image

parser = argparse.ArgumentParser()
parser.add_argument("--root", type=Path, required=True)
parser.add_argument("--output", type=Path, required=True)
args = parser.parse_args()
root = args.root.resolve()
output = args.output.resolve()
if output.is_relative_to(root):
    raise ValueError("Reference output must not be inside the authority resource root")
output.parent.mkdir(parents=True, exist_ok=True)
count = 0
with output.open("w", encoding="utf-8", newline="\n") as result:
    result.write("path\tinputSha256\twidth\theight\trgbaBottomUpSha256\n")
    for path in sorted(root.rglob("*.png")):
        if "\t" in str(path) or "\n" in str(path):
            raise ValueError("Unsupported manifest path delimiter")
        input_sha = hashlib.sha256(path.read_bytes()).hexdigest().upper()
        with Image.open(path) as image:
            width, height = image.size
            with image.convert("RGBA") as rgba:
                with rgba.transpose(Image.Transpose.FLIP_TOP_BOTTOM) as flipped:
                    pixel_sha = hashlib.sha256(flipped.tobytes()).hexdigest().upper()
        result.write(f"{path}\t{input_sha}\t{width}\t{height}\t{pixel_sha}\n")
        count += 1
print(f"PILLOW_REFERENCE_ONLY files={count} manifest={output}")
