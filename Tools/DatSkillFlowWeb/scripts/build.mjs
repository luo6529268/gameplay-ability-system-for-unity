import { createHash, randomUUID } from "node:crypto";
import { opendir, mkdir, readFile, rename, stat, writeFile } from "node:fs/promises";
import { execFile } from "node:child_process";
import { stripTypeScriptTypes } from "node:module";
import { dirname, extname, join, resolve, sep } from "node:path";
import { fileURLToPath } from "node:url";

const projectRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const distRoot = join(projectRoot, "dist");
const buildsRoot = join(distRoot, "builds");
const timestamp = new Date().toISOString().toLowerCase().replace(/[^0-9]/g, "");
const buildId = `${timestamp}-${randomUUID().replaceAll("-", "")}`;
const buildRoot = join(buildsRoot, buildId);
const sourceRoots = ["src", "tests/unit", "tests/integration", "scripts"];
const emittedPaths = [];

process.stderr.write(
    "NOTICE: build uses Node 24's release-candidate stripTypeScriptTypes API; ExperimentalWarning is intentionally suppressed.\n",
);

function toPortable(filePath) {
    return filePath.split(sep).join("/");
}

async function collectTypeScriptFiles(relativeDirectory, output) {
    const absoluteDirectory = join(projectRoot, relativeDirectory);
    const directory = await opendir(absoluteDirectory);
    for await (const entry of directory) {
        const relativePath = join(relativeDirectory, entry.name);
        if (entry.isDirectory()) {
            await collectTypeScriptFiles(relativePath, output);
        } else if (entry.isFile() && extname(entry.name) === ".ts") {
            output.push(relativePath);
        }
    }
}

async function emit(relativeOutputPath, bytes) {
    const absoluteOutputPath = join(buildRoot, relativeOutputPath);
    await mkdir(dirname(absoluteOutputPath), { recursive: true });
    await writeFile(absoluteOutputPath, bytes, { flag: "wx" });
    emittedPaths.push(relativeOutputPath);
}

await mkdir(buildsRoot, { recursive: true });
await mkdir(buildRoot);

const typeScriptFiles = [];
for (const sourceRoot of sourceRoots) {
    await collectTypeScriptFiles(sourceRoot, typeScriptFiles);
}
typeScriptFiles.sort((left, right) => left.localeCompare(right));

for (const relativeSourcePath of typeScriptFiles) {
    const source = await readFile(join(projectRoot, relativeSourcePath), "utf8");
    const javaScript = stripTypeScriptTypes(source, { mode: "strip" });
    const relativeOutputPath = relativeSourcePath.slice(0, -3) + ".js";
    await emit(relativeOutputPath, `// dat-skill-flow-build:${buildId}\n${javaScript}`);
}

const manifestIntegritySource = await readFile(join(projectRoot, "scripts/manifest-integrity.mjs"), "utf8");
await emit(
    "scripts/manifest-integrity.mjs",
    `// dat-skill-flow-build:${buildId}\n${manifestIntegritySource}`,
);

const windowsReplaceFileBytes = await readFile(join(projectRoot, "scripts/windows-replace-file.ps1"));
await emit("runtime/windows-replace-file.ps1", windowsReplaceFileBytes);
const windowsSafeFileBytes = await readFile(join(projectRoot, "scripts/windows-safe-file.ps1"));
await emit("runtime/windows-safe-file.ps1", windowsSafeFileBytes);

const sourceHtml = await readFile(join(projectRoot, "index.html"), "utf8");
const stampedHtml = sourceHtml.replace(
    "<head>",
    `<head>\n    <meta name="dat-skill-flow-build-id" content="${buildId}" />`,
);
await emit("index.html", stampedHtml);

const sourceRenderCadenceHtml = await readFile(join(projectRoot, "render-cadence.html"), "utf8");
const stampedRenderCadenceHtml = sourceRenderCadenceHtml.replace(
    "<head>",
    `<head>\n    <meta name="dat-skill-flow-build-id" content="${buildId}" />`,
);
await emit("render-cadence.html", stampedRenderCadenceHtml);

const sourceCss = await readFile(join(projectRoot, "src/client/styles.css"));
await emit("src/client/styles.css", Buffer.concat([
    Buffer.from(`/* dat-skill-flow-build:${buildId} */\n`),
    sourceCss,
]));
const renderCadenceStyles = await readFile(join(projectRoot, "src/client/render-cadence-styles.css"));
await emit("src/client/render-cadence-styles.css", Buffer.concat([
    Buffer.from(`/* dat-skill-flow-build:${buildId} */\n`),
    renderCadenceStyles,
]));

async function describeOutput(relativeBuildPath, manifestPath = toPortable(relativeBuildPath)) {
    const bytes = await readFile(join(buildRoot, relativeBuildPath));
    return {
        path: manifestPath,
        buildId,
        size: bytes.length,
        sha256: createHash("sha256").update(bytes).digest("hex"),
    };
}

emittedPaths.sort((left, right) => left.localeCompare(right));
const clientPaths = [
    "index.html", "render-cadence.html", "src/client/main.js", "src/client/render-cadence-main.js", "src/client/render-cadence-sampler.js", "src/client/render-cadence-styles.css", "src/client/canvas-geometry-edit.js", "src/client/complete-action-selection.js", "src/client/editor-support.js", "src/client/flow-layout.js", "src/client/flow-svg.js", "src/client/latest-task-scheduler.js", "src/client/panel-layout.js", "src/client/preview-renderer.js", "src/client/project-client.js", "src/client/runtime-frame-timeline.js", "src/client/skill-entries.js", "src/client/skill-flow.js", "src/client/skill-timeline.js", "src/client/overlay-geometry.js", "src/client/styles.css", "src/client/timeline-controller.js",
    "src/presentation/camera.js", "src/presentation/index.js", "src/presentation/projection.js",
    "src/sim/canonical.js", "src/sim/catalog.js", "src/sim/constants.js", "src/sim/core.js", "src/sim/frame-tick.js",
    "src/sim/index.js", "src/sim/input.js", "src/sim/motion.js", "src/sim/opoint.js", "src/sim/rng.js", "src/sim/rules.js", "src/sim/timeline.js", "src/sim/types.js",
    "src/sim/wpoint.js",
    "src/sim/world.js", "src/authority/gate2-sim-ledger.js", "src/authority/gate4-motion-ledger.js", "src/authority/gate4b-presentation-ledger.js", "src/authority/ledger.js", "src/trace/envelope.js",
    "src/diagnostics/envelope.js", "src/validation/strict.js",
];
const clientFiles = await Promise.all(clientPaths.map((filePath) => describeOutput(filePath)));
const clientRoot = `builds/${buildId}`;
const outputs = await Promise.all(emittedPaths.map((filePath) => (
    describeOutput(filePath, `${clientRoot}/${toPortable(filePath)}`)
)));
const runtimeAssetPaths = [
    "runtime/windows-replace-file.ps1",
    "runtime/windows-safe-file.ps1",
];
const runtimeAssets = await Promise.all(runtimeAssetPaths.map((filePath) => (
    describeOutput(filePath, `${clientRoot}/${toPortable(filePath)}`)
)));
const testFiles = emittedPaths
    .filter((filePath) => /^tests[\\/](?:unit|integration)[\\/].+\.test\.js$/.test(filePath))
    .map((filePath) => `${clientRoot}/${toPortable(filePath)}`);
const manifest = {
    schemaVersion: 1,
    buildId,
    clientRoot,
    serverEntry: `${clientRoot}/src/server/cli.js`,
    testFiles,
    runtimeAssets,
    outputs,
    clientFiles,
};

const manifestBytes = Buffer.from(`${JSON.stringify(manifest, null, 2)}\n`);
const pinnedManifestPath = join(buildRoot, "build-manifest.json");
await writeFile(pinnedManifestPath, manifestBytes, { flag: "wx" });

async function publishCurrentManifest() {
    const currentManifestPath = join(distRoot, "build-manifest.json");
    const replacementPath = join(distRoot, `.build-manifest-${buildId}.pending.json`);
    const backupPath = join(distRoot, `.build-manifest-${buildId}.backup.json`);
    await writeFile(replacementPath, manifestBytes, { flag: "wx" });
    try {
        await stat(currentManifestPath);
    } catch (error) {
        if (error?.code !== "ENOENT") {
            throw error;
        }
        try {
            await rename(replacementPath, currentManifestPath);
            return;
        } catch (renameError) {
            if (renameError?.code !== "EEXIST") {
                throw renameError;
            }
        }
    }
    if (process.platform !== "win32") {
        await rename(replacementPath, currentManifestPath);
        return;
    }
    const helperPath = join(projectRoot, "scripts", "windows-replace-file.ps1");
    const result = await new Promise((resolveResult, rejectResult) => {
        execFile("powershell.exe", [
            "-NoProfile",
            "-NonInteractive",
            "-ExecutionPolicy",
            "Bypass",
            "-File",
            helperPath,
            "-TargetPath",
            currentManifestPath,
            "-ReplacementPath",
            replacementPath,
            "-BackupPath",
            backupPath,
        ], { windowsHide: true, encoding: "utf8" }, (error, stdout, stderr) => {
            if (error !== null) {
                rejectResult(new Error(`Atomic build manifest publication failed: ${stderr.trim()}`, { cause: error }));
                return;
            }
            try {
                const status = JSON.parse(stdout.trim().split(/\r?\n/).at(-1));
                if (status.ok !== true) {
                    rejectResult(new Error(`Atomic build manifest publication failed: ${stdout.trim()}`));
                    return;
                }
                resolveResult();
            } catch (parseError) {
                rejectResult(new Error("Atomic build manifest publication returned invalid output.", { cause: parseError }));
            }
        });
    });
    await result;
}

await publishCurrentManifest();
process.stdout.write(`Built ${outputs.length} files for ${buildId}\n`);
