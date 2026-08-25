import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { describe, it } from "node:test";

import { parseCliArguments } from "../../src/server/cli-args.js";

describe("render cadence read-only server contract", () => {
    it("accepts an explicit read-only startup flag without changing normal startup defaults", () => {
        assert.equal(parseCliArguments([]).readOnly, false);
        assert.equal(parseCliArguments(["--read-only"]).readOnly, true);
    });

    it("keeps preview lifecycle routes available but blocks every mutation route in read-only mode", async () => {
        const source = await readFile(resolve("src/server/server.ts"), "utf8");
        assert.match(source, /options\.readOnly === true/);
        assert.match(source, /pathname === "\/api\/project\/open"/);
        assert.match(source, /pathname === "\/api\/project\/preview"/);
        assert.match(source, /pathname === "\/api\/project\/close"/);
        for (const path of [
            "/api/project/edit",
            "/api/project/edit-batch",
            "/api/project/edit-structure",
            "/api/project/save",
            "/api/project/skills",
        ]) {
            assert.match(source, new RegExp(`pathname === "${path.replaceAll("/", "\\/")}"`));
        }
        assert.match(source, /read-only-mode/);
    });
});
