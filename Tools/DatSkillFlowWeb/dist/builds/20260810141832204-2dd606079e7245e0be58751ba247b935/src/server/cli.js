// dat-skill-flow-build:20260810141832204-2dd606079e7245e0be58751ba247b935
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
import { ProjectDatService } from "./project-dat-service.js";
import { ProjectSkillService } from "./project-skill-service.js";
import { loadPatchPackageIndex } from "./patch-package-index.js";

const PINNED_MANIFEST_PATH_ENV = "DAT_SKILL_FLOW_PINNED_MANIFEST_PATH";

const cliArguments = parseCliArguments(process.argv.slice(2));
const staticRoot = resolve(cliArguments.root ?? "dist");
const startupWorkspace = cliArguments.workspace;
const startupAssetWorkspace = cliArguments.assetWorkspace;
const startupPatchWorkspace = cliArguments.patchWorkspace;
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

const assetWorkspace = startupAssetWorkspace === undefined
    ? undefined
    : (() => {
        const registry = new WorkspaceRegistry({ allowAbsoluteRootGrant, nativeClient });
        return registry;
    })();
if (assetWorkspace !== undefined) {
    await assetWorkspace.authorizeStartupRoot(startupAssetWorkspace);
    assetWorkspace.sealStartupAuthorization();
}

const patchWorkspace = startupPatchWorkspace === undefined
    ? undefined
    : new WorkspaceRegistry({ allowAbsoluteRootGrant, nativeClient });
if (patchWorkspace !== undefined) {
    await patchWorkspace.authorizeStartupRoot(startupPatchWorkspace);
    patchWorkspace.sealStartupAuthorization();
}
const patchIndex = cliArguments.patchIndex === undefined
    ? undefined
    : await loadPatchPackageIndex(resolve(cliArguments.patchIndex));

const safeSave = new SafeSaveService(workspace, { nativeClient });
let projectDatService                               ;
let projectSkillService                                 ;
if (startupWorkspace !== undefined) {
    projectDatService = await ProjectDatService.initialize({
        primaryRegistry: workspace,
        assetRegistry: assetWorkspace,
        patchRegistry: patchWorkspace,
        patchIndex,
        dataTxtLogicalPath: cliArguments.dataTxt,
    });
    process.stdout.write("Preparing DAT catalog and editable character session...\n");
    let lastReported = 0;
    const prepared = await projectDatService.prepareDefaultSession((completed, total) => {
        const percent = total === 0 ? 100 : Math.floor(completed * 100 / total);
        if (completed === total || percent >= lastReported + 25) {
            lastReported = percent;
            process.stdout.write(`Native preview warmup: ${completed}/${total}\n`);
        }
    });
    process.stdout.write(
        `DAT project ready in ${prepared.elapsedMs} ms; `
        + `${prepared.scenarios} verified entry previews continue warming in the background.\n`,
    );
    void prepared.warmup.then((warmup) => {
        process.stdout.write(
            `DAT preparation complete: ${warmup.scenarios - warmup.failed}/${warmup.scenarios} previews, `
            + `${warmup.assets - warmup.assetFailures}/${warmup.assets} assets in ${warmup.elapsedMs} ms.\n`,
        );
    }).catch((error         ) => {
        process.stderr.write(`Background DAT preview warmup failed: ${error instanceof Error ? error.message : String(error)}\n`);
    });
    const startupRoot = workspace.getStartupRootGrant();
    if (startupRoot === undefined) {
        throw new Error("The project skill service requires an authorized startup workspace.");
    }
    projectSkillService = new ProjectSkillService({
        registry: workspace,
        rootId: startupRoot.rootId,
        safeSave,
    });
}
const server = createApplicationServer({
    staticRoot: verified.staticRoot,
    manifestPath: verified.manifestPath,
    staticConfiguration: Promise.resolve(staticConfiguration),
    workspace,
    safeSave,
    projectDatService,
    projectSkillService,
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
