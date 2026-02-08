import struct, os

d = r"I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Sprite\XueYuan"
for fn in os.listdir(d):
    if fn.endswith(".png"):
        path = os.path.join(d, fn)
        with open(path, "rb") as f:
            f.read(16)
            data = f.read(8)
            w, h = struct.unpack(">II", data)
            print(f"{fn}: {w} x {h}")
