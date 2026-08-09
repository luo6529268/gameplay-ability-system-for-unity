// dat-skill-flow-build:20260808012629739-490e8c62d3fe4d5ba541d99d9b17f3fa
export const GATE1A_DAT_KEY = Buffer.from("SiuHungIsAGoodBearBecauseHeIsVeryGood", "ascii");
export const GATE1A_DAT_PREFIX_LENGTH = 123;

export function syntheticPrefix()         {
    return Buffer.from(Array.from(
        { length: GATE1A_DAT_PREFIX_LENGTH },
        (_unused, index) => (index * 29 + 7) & 0xff,
    ));
}

export function independentlyEncryptDat(plaintext            , prefix = syntheticPrefix())         {
    if (prefix.length !== GATE1A_DAT_PREFIX_LENGTH) {
        throw new RangeError("synthetic DAT prefix must be exactly 123 bytes");
    }
    const result = Buffer.alloc(prefix.length + plaintext.length);
    result.set(prefix);
    for (let index = 0; index < plaintext.length; index += 1) {
        const absoluteOffset = GATE1A_DAT_PREFIX_LENGTH + index;
        result[absoluteOffset] = (
            (plaintext[index] ?? 0) + (GATE1A_DAT_KEY[absoluteOffset % GATE1A_DAT_KEY.length] ?? 0)
        ) & 0xff;
    }
    return result;
}

export function syntheticDatPlaintext()         {
    return Buffer.concat([
        Buffer.from([
            0xef, 0xbb, 0xbf,
            ...Buffer.from("name: Synthetic Hero\r\n", "ascii"),
            ...Buffer.from("walking_speed: 4.5\r\n", "ascii"),
            ...Buffer.from("file(0-5): sprite\\hero.bmp w: 3 h: 2 row: 3 col: 2\r\n", "ascii"),
            ...Buffer.from("unknown_top: keep-me # untouched\n", "ascii"),
            ...Buffer.from("<frame> 7 duplicate-first\r\n", "ascii"),
            ...Buffer.from("pic: 0 state: 1 next: 8\r\n", "ascii"),
            ...Buffer.from("<frame_end>\r\n", "ascii"),
            ...Buffer.from("<frame> 7 duplicate-last\n", "ascii"),
            ...Buffer.from("pic: 2 state: 3 next: 9 sound: data\\001.wav\n", "ascii"),
            ...Buffer.from("itr:\n kind: 0 x: 1 y: 2 w: 3 h: 4\n itr_end:\n", "ascii"),
            ...Buffer.from("bdy:\n x: 5 y: 6 w: 7 h: 230 zwidth: 10\n bdy_end:\n", "ascii"),
            ...Buffer.from("opoint:\n kind: 1 x: 2 y: 3 action: 4 oid: 200 facing: 1\n opoint_end:\n", "ascii"),
            ...Buffer.from("wpoint:\n kind: 1 x: 8 y: 9 weaponact: 10\n wpoint_end:\n", "ascii"),
            ...Buffer.from("bpoint:\n x: 11 y: 12\n bpoint_end:\n", "ascii"),
            ...Buffer.from("cpoint:\n injury: 1 cover: 2 fronthurtact: 70 backhurtact: 71\n cpoint_end:\n", "ascii"),
            ...Buffer.from("mystery: ", "ascii"),
            0x00,
            0xff,
            0xc3,
            0x28,
            ...Buffer.from(" preserve\n", "ascii"),
            ...Buffer.from("<frame_end>\n", "ascii"),
        ]),
    ]);
}

export function syntheticDataTxt()         {
    return Buffer.concat([
        Buffer.from("# exact comments and order stay here\r\n<object>\r\n", "ascii"),
        Buffer.from("id: 10 type: 0 file: chars\\missing.dat # first fails\r\n", "ascii"),
        Buffer.from("id: 10 type: 3 file: chars\\working.dat # duplicate succeeds\n", "ascii"),
        Buffer.from("id: 10 type: 5 file: chars\\ignored.dat\r\n", "ascii"),
        Buffer.from("id: 11 type: 0 file: ..\\escape.dat\r\n", "ascii"),
        Buffer.from("malformed object line ", "ascii"),
        Buffer.from([0xff, 0x00]),
        Buffer.from("\r\n<background>\r\n", "ascii"),
        Buffer.from("id: 2 file: bg\\district.dat # stage\r\n", "ascii"),
        Buffer.from("id: 3 file: C:\\outside\\stage.dat\n", "ascii"),
        Buffer.from("<unknown>\nleave: everything\n", "ascii"),
    ]);
}

export const cppDuplicateLoadFixture = Object.freeze([
    { id: 10, file: "chars\\missing.dat", outcome: "decrypt-failed"          },
    { id: 10, file: "chars\\malformed.dat", outcome: "parse-failed"          },
    { id: 10, file: "chars\\working-but-skipped.dat", outcome: "loaded"          },
    { id: 11, file: "chars\\missing.dat", outcome: "decrypt-failed"          },
    { id: 11, file: "chars\\working.dat", outcome: "loaded"          },
]);

export function syntheticBmp(bitDepth             , width = 12, height = 6)         {
    const paletteBytes = bitDepth === 8 ? 256 * 4 : 0;
    const pixelOffset = 14 + 40 + paletteBytes;
    const rowStride = Math.floor((bitDepth * width + 31) / 32) * 4;
    const pixelBytes = rowStride * height;
    const bytes = Buffer.alloc(pixelOffset + pixelBytes);
    bytes.write("BM", 0, "ascii");
    bytes.writeUInt32LE(bytes.length, 2);
    bytes.writeUInt32LE(pixelOffset, 10);
    bytes.writeUInt32LE(40, 14);
    bytes.writeInt32LE(width, 18);
    bytes.writeInt32LE(height, 22);
    bytes.writeUInt16LE(1, 26);
    bytes.writeUInt16LE(bitDepth, 28);
    bytes.writeUInt32LE(0, 30);
    bytes.writeUInt32LE(pixelBytes, 34);
    if (bitDepth === 8) {
        bytes.writeUInt32LE(256, 46);
    }
    return bytes;
}
