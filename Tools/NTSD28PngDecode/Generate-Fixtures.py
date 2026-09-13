"""Generate isolated decoder fixtures and independent expected RGBA; never touch assets."""
import binascii
import io
import json
import struct
import zlib
from pathlib import Path
from PIL import Image

REPO = Path(__file__).resolve().parents[2]
ROOT = REPO / "Temp/NTSD28PngDecode/generated"
ROOT.mkdir(parents=True, exist_ok=True)
SIGNATURE = b"\x89PNG\r\n\x1a\n"


def chunk(kind, data):
    return struct.pack(">I", len(data)) + kind + data + struct.pack(">I", binascii.crc32(kind + data) & 0xffffffff)


def paeth(a, b, c):
    p = a + b - c
    distances = (abs(p - a), abs(p - b), abs(p - c))
    return (a, b, c)[distances.index(min(distances))]


def filtered(rows, bpp, filter_type):
    result = bytearray()
    previous = bytes(len(rows[0]))
    for row in rows:
        result.append(filter_type)
        for i, value in enumerate(row):
            a = row[i - bpp] if i >= bpp else 0
            b = previous[i]
            c = previous[i - bpp] if i >= bpp else 0
            predictor = (0, a, b, (a + b) // 2, paeth(a, b, c))[filter_type]
            result.append((value - predictor) & 255)
        previous = row
    return bytes(result)


def png(width, height, depth, color_type, rows, filter_type, palette=b"", alpha=b""):
    ihdr = struct.pack(">IIBBBBB", width, height, depth, color_type, 0, 0, 0)
    data = SIGNATURE + chunk(b"IHDR", ihdr)
    if palette:
        data += chunk(b"PLTE", palette)
    if alpha:
        data += chunk(b"tRNS", alpha)
    raw = filtered(rows, 4 if color_type == 6 else 1, filter_type)
    compressed = zlib.compress(raw)
    cut = max(1, len(compressed) // 3)
    for begin in range(0, len(compressed), cut):
        data += chunk(b"IDAT", compressed[begin:begin + cut])
    return data + chunk(b"IEND", b"")


valid, invalid = [], []


def save(name, data, rgba=None, width=0, height=0, category=""):
    path = ROOT / name
    path.write_bytes(data)
    entry = {"path": str(path), "width": width, "height": height, "category": category}
    if rgba is None:
        invalid.append(entry)
    else:
        expected = ROOT / (name + ".rgba")
        expected.write_bytes(rgba)
        entry["rgbaPath"] = str(expected)
        valid.append(entry)


width, height = 13, 7
for depth in (1, 2, 4, 8):
    count = 1 << depth
    palette = bytes(component for i in range(count) for component in ((i * 73) % 256, (i * 37 + 19) % 256, (i * 11 + 133) % 256))
    # Omit tail alpha entries to exercise implicit 255.
    alpha = bytes((0, 31, 127, 255)[i % 4] for i in range(max(1, count - 1)))
    rows, rgba_rows = [], []
    for y in range(height):
        row = bytearray((width * depth + 7) // 8)
        rgba = bytearray()
        for x in range(width):
            index = (x * 3 + y * 5) % count
            row[x * depth // 8] |= index << (8 - depth - (x * depth % 8))
            rgba.extend(palette[index * 3:index * 3 + 3])
            rgba.append(alpha[index] if index < len(alpha) else 255)
        rows.append(bytes(row))
        rgba_rows.append(bytes(rgba))
    for filter_type in range(5):
        data = png(width, height, depth, 3, rows, filter_type, palette, alpha)
        assert Image.open(io.BytesIO(data)).convert("RGBA").tobytes() == b"".join(rgba_rows)
        save(f"palette-{depth}-filter-{filter_type}.png", data, b"".join(reversed(rgba_rows)), width, height, "palette")

opaque_rows = [bytes(value if index % 4 != 3 else 255 for index, value in enumerate(row)) for row in rgba_rows]
save("palette-no-trns.png", png(width, height, 8, 3, rows, 1, palette),
     b"".join(reversed(opaque_rows)), width, height, "palette")

rgba_rows = [bytes(v for x in range(width) for v in ((x * 19) % 256, (y * 39) % 256, ((x + y) * 17) % 256, (0, 17, 128, 255)[(x + y) % 4])) for y in range(height)]
for filter_type in range(5):
    data = png(width, height, 8, 6, rgba_rows, filter_type)
    assert Image.open(io.BytesIO(data)).convert("RGBA").tobytes() == b"".join(rgba_rows)
    save(f"rgba-filter-{filter_type}.png", data, b"".join(reversed(rgba_rows)), width, height, "rgba")

# BMP fixture covers the unchanged worker path without requiring Unity's BMP decoder.
bw, bh = 5, 3
rgb_rows = [bytes(v for x in range(bw) for v in ((x * 41) % 256, y * 91, 213)) for y in range(bh)]
packed_rows = [b"".join(row[x:x + 3][::-1] for x in range(0, len(row), 3)) + bytes((-len(row)) % 4) for row in rgb_rows]
payload = b"".join(packed_rows)
bmp = b"BM" + struct.pack("<IHHI", 54 + len(payload), 0, 0, 54) + struct.pack("<IiiHHIIiiII", 40, bw, bh, 1, 24, 0, len(payload), 0, 0, 0, 0) + payload
expected = bytes(v for row in rgb_rows for i in range(0, len(row), 3) for v in (*row[i:i + 3], 255))
save("unchanged-24bit.bmp", bmp, expected, bw, bh, "bmp")
spark = REPO / "Assets/NTSD/Sprite/UIPanels/SPARK.bmp"
with Image.open(spark) as image:
    with image.convert("RGBA") as rgba:
        with rgba.transpose(Image.Transpose.FLIP_TOP_BOTTOM) as flipped:
            save("unchanged-production-spark.bmp", spark.read_bytes(), flipped.tobytes(), image.width, image.height, "bmp")

base = (ROOT / "rgba-filter-4.png").read_bytes()
save("truncated.png", base[:-7])
corrupt = bytearray(base)
corrupt[29] ^= 1
save("crc-mismatch.png", bytes(corrupt))
ihdr = struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 1)
save("unsupported-interlace.png", SIGNATURE + chunk(b"IHDR", ihdr) + base[33:])
ihdr = struct.pack(">IIBBBBB", 0x7fffffff, 0x7fffffff, 8, 6, 0, 0, 0)
save("overflow-dimensions.png", SIGNATURE + chunk(b"IHDR", ihdr) + base[33:])
raw = b"\x05" + bytes(width * 4) + (b"\x00" + bytes(width * 4)) * (height - 1)
save("invalid-filter.png", base[:33] + chunk(b"IDAT", zlib.compress(raw)) + chunk(b"IEND", b""))
compressed = bytearray(zlib.compress(filtered(rgba_rows, 4, 0)))
compressed[-1] ^= 1
save("adler-mismatch.png", base[:33] + chunk(b"IDAT", bytes(compressed)) + chunk(b"IEND", b""))
save("extra-inflated-byte.png", base[:33] + chunk(b"IDAT", zlib.compress(filtered(rgba_rows, 4, 0) + b"\x00")) + chunk(b"IEND", b""))
save("short-inflated-data.png", base[:33] + chunk(b"IDAT", zlib.compress(b"\x00")) + chunk(b"IEND", b""))
save("unknown-critical-chunk.png", base[:33] + chunk(b"NOPE", b"") + base[33:])
save("duplicate-ihdr.png", base[:33] + base[8:33] + base[33:])
save("missing-iend.png", base[:-12])
indexed_header = SIGNATURE + chunk(b"IHDR", struct.pack(">IIBBBBB", 1, 1, 1, 3, 0, 0, 0))
save("missing-palette.png", indexed_header + chunk(b"IDAT", zlib.compress(b"\x00\x00")) + chunk(b"IEND", b""))
save("palette-index-out-of-range.png", indexed_header + chunk(b"PLTE", b"\x00\x00\x00") + chunk(b"IDAT", zlib.compress(b"\x00\x80")) + chunk(b"IEND", b""))
save("palette-transparency-too-long.png", indexed_header + chunk(b"PLTE", b"\x00\x00\x00") + chunk(b"tRNS", b"\x00\xff") + chunk(b"IDAT", zlib.compress(b"\x00\x00")) + chunk(b"IEND", b""))
compressed = zlib.compress(filtered(rgba_rows, 4, 0))
cut = len(compressed) // 2
save("nonconsecutive-idat.png", base[:33] + chunk(b"IDAT", compressed[:cut]) + chunk(b"tEXt", b"note\x00gap") + chunk(b"IDAT", compressed[cut:]) + chunk(b"IEND", b""))
save("trailing-data-after-iend.png", base + b"extra")
manifest = {"valid": valid, "invalid": invalid, "reference": "Known generated pixels independently checked with Pillow; expected RGBA is bottom-up"}
(ROOT / "manifest.json").write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
print(json.dumps({"valid": len(valid), "invalid": len(invalid), "manifest": str(ROOT / "manifest.json")}))
