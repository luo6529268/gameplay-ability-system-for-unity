// dat-skill-flow-build:20260811034854877-dbcee6f9cfde4d6b8294265c5b3e1b4d
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    applyDatStructureEdit,
    canCopyBlock,
    canCopyFrame,
    canDeleteBlock,
    canDeleteFrame,
} from "../../src/model/dat-structure-edit.js";
import { parseDatCst } from "../../src/syntax/byte-cst.js";

const source = Buffer.from([
    "# prefix remains byte-identical\r\n",
    "<frame> 7 unusual label # header comment\r\n",
    " pic: 1  wait: 2 next: 8 # formatting\r\n",
    " itr:\r\n",
    "  kind: 0 x: 1 y: 2 w: 3 h: 4 unknown: keep\r\n",
    " itr_end:\r\n",
    " bdy:\r\n",
    "  x: 5 y: 6 w: 7 h: 8\r\n",
    " bdy_end:\r\n",
    "<frame_end>\r\n",
    "# suffix remains byte-identical\r\n",
].join(""), "latin1");

describe("lossless DAT structure edits", () => {
    it("records exact, closed frame and block spans", () => {
        const cst = parseDatCst(source);
        const frame = cst.frames[0] ;
        assert.equal(frame.closed, true);
        assert.equal(source.subarray(frame.frameIdSpan.start, frame.frameIdSpan.end).toString("ascii"), "7");
        assert.equal(canDeleteFrame(frame), true);
        assert.equal(canCopyFrame(cst, frame), true);
        assert.equal(frame.blocks.every(canDeleteBlock), true);
        assert.equal(frame.blocks.every((block) => canCopyBlock(cst, block)), true);
    });

    it("copies the complete frame after itself and rewrites only the copied header ID", () => {
        const cst = parseDatCst(source);
        const frame = cst.frames[0] ;
        const result = applyDatStructureEdit(cst, {
            operation: "copy-frame",
            target: { kind: "frame", frameOccurrence: 0 },
            newFrameId: 17,
        });
        const original = source.subarray(frame.span.start, frame.span.end);
        const relativeStart = frame.frameIdSpan.start - frame.span.start;
        const relativeEnd = frame.frameIdSpan.end - frame.span.start;
        const copy = Buffer.concat([
            original.subarray(0, relativeStart),
            Buffer.from("17", "ascii"),
            original.subarray(relativeEnd),
        ]);
        assert.deepEqual(result, Buffer.concat([
            source.subarray(0, frame.span.end),
            copy,
            source.subarray(frame.span.end),
        ]));
        assert.deepEqual(result.subarray(0, frame.span.end), source.subarray(0, frame.span.end));
    });

    it("copies, template-creates, and deletes complete blocks without normalizing unknown bytes", () => {
        const cst = parseDatCst(source);
        const frame = cst.frames[0] ;
        const itr = frame.blocks[0] ;
        const itrBytes = source.subarray(itr.span.start, itr.span.end);
        const copied = applyDatStructureEdit(cst, {
            operation: "copy-block",
            target: { kind: "block", frameOccurrence: 0, blockType: "itr", blockIndex: 0 },
        });
        assert.deepEqual(copied, Buffer.concat([
            source.subarray(0, itr.span.end),
            itrBytes,
            source.subarray(itr.span.end),
        ]));
        const created = applyDatStructureEdit(cst, {
            operation: "create-block",
            target: { kind: "block", frameOccurrence: 0, blockType: "itr", blockIndex: 0 },
        });
        assert.deepEqual(created, copied);
        const deleted = applyDatStructureEdit(cst, {
            operation: "delete-block",
            target: { kind: "block", frameOccurrence: 0, blockType: "itr", blockIndex: 0 },
        });
        assert.deepEqual(deleted, Buffer.concat([
            source.subarray(0, itr.span.start),
            source.subarray(itr.span.end),
        ]));
        assert.match(copied.toString("latin1"), /unknown: keep/u);
    });

    it("deletes a complete frame without repairing references", () => {
        const cst = parseDatCst(source);
        const frame = cst.frames[0] ;
        const deleted = applyDatStructureEdit(cst, {
            operation: "delete-frame",
            target: { kind: "frame", frameOccurrence: 0 },
        });
        assert.deepEqual(deleted, Buffer.concat([
            source.subarray(0, frame.span.start),
            source.subarray(frame.span.end),
        ]));
        assert.match(deleted.toString("latin1"), /# prefix remains byte-identical/u);
        assert.match(deleted.toString("latin1"), /# suffix remains byte-identical/u);
    });

    it("rejects malformed spans, unsafe encodings, and invalid new IDs", () => {
        const malformed = parseDatCst(Buffer.from("<frame> 1\n pic: 2\n itr:\n x: 1\n", "ascii"));
        assert.equal(canDeleteFrame(malformed.frames[0] ), false);
        assert.throws(() => applyDatStructureEdit(malformed, {
            operation: "delete-frame",
            target: { kind: "frame", frameOccurrence: 0 },
        }));
        assert.throws(() => applyDatStructureEdit(parseDatCst(Buffer.from([0xff, 0xfe, 0x3c, 0])), {
            operation: "delete-frame",
            target: { kind: "frame", frameOccurrence: 0 },
        }));
        assert.throws(() => applyDatStructureEdit(parseDatCst(source), {
            operation: "copy-frame",
            target: { kind: "frame", frameOccurrence: 0 },
            newFrameId: 600,
        }));
    });
});
