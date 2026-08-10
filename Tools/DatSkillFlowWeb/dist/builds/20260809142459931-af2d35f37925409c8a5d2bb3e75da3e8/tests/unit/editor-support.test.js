// dat-skill-flow-build:20260809142459931-af2d35f37925409c8a5d2bb3e75da3e8
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    localizedRequestError,
    localizedResponseError,
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
});
