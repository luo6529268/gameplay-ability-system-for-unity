import { resolve } from "node:path";

import { loadVerifiedBuildManifest } from "./manifest-integrity.mjs";
import { registerVerifiedLoader } from "./verified-loader.mjs";

function argumentValue(name) {
    const index = process.argv.indexOf(name);
    return index >= 0 ? process.argv[index + 1] : undefined;
}

const staticRoot = resolve(argumentValue("--root") ?? "dist");
const manifestPath = resolve(argumentValue("--manifest") ?? "dist/build-manifest.json");
const pinned = await loadVerifiedBuildManifest({ staticRoot, manifestPath });
process.env.DAT_SKILL_FLOW_PINNED_MANIFEST_PATH = pinned.manifestPath;
const verified = registerVerifiedLoader({
    staticRoot: pinned.staticRoot,
    manifestPath: pinned.manifestPath,
});
await import(verified.entryUrl);
