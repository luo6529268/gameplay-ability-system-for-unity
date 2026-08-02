// dat-skill-flow-build:20260801045021694-55c032f73ea348fabb37f6992569ec25
import { resolve } from "node:path";

import {
    loadVerifiedBuildManifest,
    loadVerifiedRuntimeAsset,
} from "../../scripts/manifest-integrity.mjs";
import { SafeSaveService } from "./safe-save.js";
import { createApplicationServer, listenLoopback, loadPinnedStaticConfiguration } from "./server.js";
import { WindowsReplaceFilePublisher } from "./windows-replace-adapter.js";
import { WorkspaceRegistry } from "./workspace-registry.js";

const PINNED_MANIFEST_PATH_ENV = "DAT_SKILL_FLOW_PINNED_MANIFEST_PATH";

function argumentValue(name        )                     {
    const index = process.argv.indexOf(name);
    return index >= 0 ? process.argv[index + 1] : undefined;
}

const staticRoot = resolve(argumentValue("--root") ?? "dist");
const manifestPath = resolve(process.env[PINNED_MANIFEST_PATH_ENV] ?? argumentValue("--manifest") ?? "dist/build-manifest.json");
const allowAbsoluteRootGrant = process.argv.includes("--allow-test-root-grant");
const rawPort = argumentValue("--port") ?? process.env.PORT ?? "4173";
const port = Number.parseInt(rawPort, 10);
if (!Number.isInteger(port) || port < 0 || port > 65_535) {
    throw new Error(`Invalid port: ${rawPort}`);
}

const verified = await loadVerifiedBuildManifest({ staticRoot, manifestPath });
const staticConfiguration = await loadPinnedStaticConfiguration(verified.staticRoot, verified.manifest);
const runtimeAsset = await loadVerifiedRuntimeAsset({
    staticRoot: verified.staticRoot,
    manifestPath: verified.manifestPath,
    outputPath: `${verified.manifest.clientRoot}/runtime/windows-replace-file.ps1`,
});
const workspace = new WorkspaceRegistry({ allowAbsoluteRootGrant });
const safeSave = new SafeSaveService(workspace, {
    publisher: new WindowsReplaceFilePublisher({
        runtimeAsset: {
            path: runtimeAsset.path,
            manifestPath: runtimeAsset.manifestPath,
            buildId: runtimeAsset.manifest.buildId,
        },
    }),
});
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
