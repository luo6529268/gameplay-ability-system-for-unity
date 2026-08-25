// dat-skill-flow-build:20260823081507882-da4b5972637d433fbefeaa8865bf044e
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import { previewDatPlaintext, previewDatProjection } from "../../src/server/project-dat-service.js";
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

    it("does not mistake a Data Changer envelope banner containing file text for plaintext DAT", () => {
        const banner = Buffer.from(
            "This data file was created with Jiquera Mondilano's Data Changer, download for free at http://jiquera.web1000.com          ",
            "latin1",
        );
        assert.equal(banner.length, 123);
        const encrypted = encryptDatPayload(banner, plaintext);

        assert.equal(previewDatProjection(encrypted).frames.length, 1);
        assert.deepEqual(previewDatPlaintext(encrypted), plaintext);
    });
});
