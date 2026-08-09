// dat-skill-flow-build:20260808055418434-39f8e96b8c0f4d4b87503c75b5cbeda0
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import { previewDatProjection } from "../../src/server/project-dat-service.js";
import { encryptDatPayload } from "../../src/syntax/dat-envelope.js";

const plaintext = Buffer.from([
    "name: Preview object\n",
    "<frame> 0 idle\n",
    "pic: 0 state: 0 wait: 1 next: 0\n",
    "<frame_end>\n",
].join(""), "latin1");

describe("native preview DAT format selection", () => {
    it("reads plaintext auxiliary DATs and encrypted DATs through the same projection boundary", () => {
        const plainProjection = previewDatProjection(plaintext);
        const encryptedProjection = previewDatProjection(encryptDatPayload(Buffer.alloc(123, 0x41), plaintext));

        assert.equal(plainProjection.frames.length, 1);
        assert.equal(encryptedProjection.frames.length, 1);
    });
});
