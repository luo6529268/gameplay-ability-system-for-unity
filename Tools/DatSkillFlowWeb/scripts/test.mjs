import { spawn } from "node:child_process";
import { resolve } from "node:path";
import { pathToFileURL } from "node:url";

import { loadVerifiedBuildManifest } from "./manifest-integrity.mjs";
import {
    VERIFIED_LOADER_MANIFEST_ENV,
    VERIFIED_LOADER_STATIC_ROOT_ENV,
} from "./verified-loader.mjs";

const projectRoot = process.cwd();
const verified = await loadVerifiedBuildManifest({
    staticRoot: resolve(projectRoot, "dist"),
    manifestPath: resolve(projectRoot, "dist/build-manifest.json"),
});
const testFiles = verified.manifest.testFiles.map((filePath) => {
    const verifiedPath = verified.verifiedPaths.get(filePath);
    if (verifiedPath === undefined) {
        throw new Error(`Test was not verified before execution: ${filePath}`);
    }
    return verifiedPath;
});
if (testFiles.length === 0) {
    throw new Error("Build manifest contains no required tests.");
}

const registerWrapper = resolve(projectRoot, "scripts/register-verified-loader.mjs");
const child = spawn(process.execPath, [
    "--import",
    pathToFileURL(registerWrapper).href,
    "--test",
    "--test-isolation=none",
    ...testFiles,
], {
    cwd: projectRoot,
    env: {
        ...process.env,
        [VERIFIED_LOADER_STATIC_ROOT_ENV]: verified.staticRoot,
        [VERIFIED_LOADER_MANIFEST_ENV]: verified.manifestPath,
    },
    stdio: "inherit",
    shell: false,
});
child.on("error", (error) => {
    throw error;
});
const exitCode = await new Promise((resolveExit) => child.once("exit", resolveExit));
if (exitCode !== 0) {
    process.exitCode = typeof exitCode === "number" ? exitCode : 1;
}
