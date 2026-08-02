// dat-skill-flow-build:20260801083408896-9389c9bc05e24c0e9a7d853e8087e132
import { readFile, writeFile } from "node:fs/promises";
import { resolve } from "node:path";

import {
    assertPreserved,
    createPreservationManifest,
                              
} from "../src/server/preservation-manifest.js";

function argumentValue(name        )                     {
    const index = process.argv.indexOf(name);
    return index >= 0 ? process.argv[index + 1] : undefined;
}

const root = resolve(argumentValue("--root") ?? ".");
const output = resolve(argumentValue("--output") ?? "audit/preservation-after.json");
const ignored = new Set((argumentValue("--ignore") ?? "node_modules,dist,dist-server,test-results,playwright-report")
    .split(",")
    .map((value) => value.trim())
    .filter(Boolean));

const manifest = await createPreservationManifest(root, { ignoreDirectoryNames: ignored });
await writeFile(output, `${JSON.stringify(manifest, null, 2)}\n`, { encoding: "utf8", flag: "wx" });

const comparePath = argumentValue("--compare");
if (comparePath !== undefined) {
    const baseline = JSON.parse(await readFile(resolve(comparePath), "utf8"))                        ;
    const failures = assertPreserved(baseline, manifest);
    if (failures.length > 0) {
        process.stderr.write(`${JSON.stringify({ preserved: false, failures }, null, 2)}\n`);
        process.exitCode = 1;
    } else {
        process.stdout.write(`${JSON.stringify({ preserved: true, entries: baseline.entries.length })}\n`);
    }
}
