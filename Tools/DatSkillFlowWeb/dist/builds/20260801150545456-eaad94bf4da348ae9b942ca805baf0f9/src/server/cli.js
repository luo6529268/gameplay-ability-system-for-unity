// dat-skill-flow-build:20260801150545456-eaad94bf4da348ae9b942ca805baf0f9
import { resolve } from "node:path";

import {
    loadVerifiedBuildManifest,
    loadVerifiedRuntimeAsset,
} from "../../scripts/manifest-integrity.mjs";
import { SafeSaveService } from "./safe-save.js";
import { createApplicationServer, listenLoopback, loadPinnedStaticConfiguration } from "./server.js";
import { PowerShellWindowsSafeFileClient } from "./windows-safe-file-adapter.js";
import { WorkspaceRegistry } from "./workspace-registry.js";
import { parseCliArguments, parsePortValue } from "./cli-args.js";

const PINNED_MANIFEST_PATH_ENV = "DAT_SKILL_FLOW_PINNED_MANIFEST_PATH";

const cliArguments = parseCliArguments(process.argv.slice(2));
const staticRoot = resolve(cliArguments.root ?? "dist");
const startupWorkspace = cliArguments.workspace;
const manifestPath = resolve(process.env[PINNED_MANIFEST_PATH_ENV] ?? cliArguments.manifest ?? "dist/build-manifest.json");
const allowAbsoluteRootGrant = cliArguments.allowTestRootGrant;
const rawPort = cliArguments.port ?? process.env.PORT ?? "4173";
const port = parsePortValue(rawPort);

const verified = await loadVerifiedBuildManifest({ staticRoot, manifestPath });
const staticConfiguration = await loadPinnedStaticConfiguration(verified.staticRoot, verified.manifest);
const safeRuntimePath = `${verified.manifest.clientRoot}/runtime/windows-safe-file.ps1`;
const safeRuntimeAsset = await loadVerifiedRuntimeAsset({
    staticRoot: verified.staticRoot,
    manifestPath: verified.manifestPath,
    outputPath: safeRuntimePath,
});
const safeRuntimeEntry = safeRuntimeAsset.manifest.runtimeAssets.find((entry) => entry.path === safeRuntimePath);
if (safeRuntimeEntry === undefined) {
    throw new Error("Pinned Windows safe-file runtime asset is missing from the manifest.");
}
const nativeClient = new PowerShellWindowsSafeFileClient({
    runtimeAsset: {
        path: safeRuntimeAsset.path,
        manifestPath: safeRuntimeAsset.manifestPath,
        buildId: safeRuntimeAsset.manifest.buildId,
        size: safeRuntimeEntry.size,
        sha256: safeRuntimeEntry.sha256,
    },
});
const workspace = new WorkspaceRegistry({ allowAbsoluteRootGrant, nativeClient });
if (startupWorkspace !== undefined) {
    await workspace.authorizeStartupRoot(startupWorkspace);
}
workspace.sealStartupAuthorization();
const safeSave = new SafeSaveService(workspace, { nativeClient });
const server = createApplicationServer({
    staticRoot: verified.staticRoot,
    manifestPath: verified.manifestPath,
    staticConfiguration: Promise.resolve(staticConfiguration),
    workspace,
    safeSave,
});
const origin = await listenLoopback(server, port);
process.stdout.write(`Dat Skill Flow server listening at ${origin}\n`);

function shutdown()       {
    server.close((error) => {
        process.exitCode = error === undefined ? 0 : 1;
    });
}

process.once("SIGINT", shutdown);
process.once("SIGTERM", shutdown);
