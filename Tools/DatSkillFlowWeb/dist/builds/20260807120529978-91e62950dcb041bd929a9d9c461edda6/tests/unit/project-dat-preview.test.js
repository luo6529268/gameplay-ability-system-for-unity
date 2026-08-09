// dat-skill-flow-build:20260807120529978-91e62950dcb041bd929a9d9c461edda6
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import { previewDatProjection } from "../../src/server/project-dat-service.js";
import { encryptDatPayload } from "../../src/syntax/dat-envelope.js";

const plaintext = Buffer.from([
    "name: Preview object\n",
    "<frame> 0 idle\n",
    "pic: 0 state: 0 wait: 1 next: 0\n",
    "<frame_end>\n",
], "latin1");

describe("native preview DAT format selection", () => {
    it("reads plaintext auxiliary DATs and encrypted DATs through the same projection boundary", () => {
        const plainProjection = previewDatProjection(plaintext);
        const encryptedProjection = previewDatProjection(encryptDatPayload(Buffer.alloc(123, 0x41), plaintext));

        assert.equal(plainProjection.top.name, "Preview object");
        assert.equal(plainProjection.frames.length, 1);
        assert.equal(encryptedProjection.top.name, "Preview object");
        assert.equal(encryptedProjection.frames.length, 1);
    });
});
