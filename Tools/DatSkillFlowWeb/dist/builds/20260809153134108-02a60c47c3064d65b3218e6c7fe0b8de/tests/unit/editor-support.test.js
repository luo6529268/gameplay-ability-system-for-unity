// dat-skill-flow-build:20260809153134108-02a60c47c3064d65b3218e6c7fe0b8de
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    localizedRequestError,
    localizedResponseError,
    projectResponseCode,
    projectSessionRecoveryDecision,
} from "../../src/client/editor-support.js";

describe("editor request errors", () => {
    it("keeps project-open errors generic for every type-0 character", () => {
        const message = localizedRequestError(404, "/api/project/open");
        assert.match(message, /所选角色/);
        assert.doesNotMatch(message, /Naruto|OID 2/);
    });

    it("includes the server diagnostic that explains an HTTP 400", () => {
        assert.equal(
            localizedResponseError(400, "/api/project/preview", {
                diagnostics: [{ message: "startFrame must reference an existing frame." }],
            }),
            "请求失败（HTTP 400）。 startFrame must reference an existing frame.",
        );
    });

    it("ignores malformed diagnostics", () => {
        assert.equal(
            localizedResponseError(400, "/api/project/preview", { diagnostics: [null, { message: 7 }] }),
            "请求失败（HTTP 400）。",
        );
    });

    it("extracts the structured project error code used for session recovery", () => {
        assert.equal(projectResponseCode({
            diagnostics: [{ details: { projectCode: "unknown-session" } }],
        }), "unknown-session");
        assert.equal(projectResponseCode({ diagnostics: [{ details: null }] }), "");
    });

    it("recovers only a clean expired project and preserves unsaved work", () => {
        assert.equal(projectSessionRecoveryDecision("unknown-session", false, false, "object-key"), "retry");
        assert.equal(projectSessionRecoveryDecision("unknown-session", true, false, "object-key"), "preserve-dirty");
        assert.equal(projectSessionRecoveryDecision("unknown-session", false, true, "object-key"), "preserve-dirty");
        assert.equal(projectSessionRecoveryDecision("unknown-session", false, false, ""), "none");
        assert.equal(projectSessionRecoveryDecision("preview-failed", false, false, "object-key"), "none");
    });
});
