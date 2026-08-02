// dat-skill-flow-build:20260801034035657-e77b54972a8f44b28020241ce8859fad
import assert from "node:assert/strict";
import { readFile, stat } from "node:fs/promises";
import { resolve } from "node:path";
import { describe, it } from "node:test";

                         
                          
                    
                       
                        
                        
                                                                                    
                                                                                        
 

describe("native zero-dependency build", () => {
    it("publishes a unique build root and exact current output manifest", async () => {
        const manifestPath = resolve(process.cwd(), "dist/build-manifest.json");
        const manifest = JSON.parse(await readFile(manifestPath, "utf8"))                 ;

        assert.equal(manifest.schemaVersion, 1);
        assert.match(manifest.buildId, /^[a-z0-9-]+$/);
        assert.equal(manifest.clientRoot, `builds/${manifest.buildId}`);
        assert.equal(manifest.serverEntry, `${manifest.clientRoot}/src/server/cli.js`);
        assert.ok(manifest.testFiles.some((path) => path.endsWith("tests/integration/build.test.js")));
        assert.ok(manifest.outputs.length > manifest.clientFiles.length);

        for (const output of manifest.outputs) {
            assert.equal(output.buildId, manifest.buildId);
            assert.match(output.sha256, /^[a-f0-9]{64}$/);
            const metadata = await stat(resolve(process.cwd(), "dist", output.path));
            assert.equal(metadata.size, output.size);
        }
    });
});
