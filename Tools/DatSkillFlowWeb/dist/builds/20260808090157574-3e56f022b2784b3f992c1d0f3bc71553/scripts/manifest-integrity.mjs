// dat-skill-flow-build:20260808090157574-3e56f022b2784b3f992c1d0f3bc71553
import { createHash } from "node:crypto";
import { open, readFile, realpath } from "node:fs/promises";
import { isAbsolute, join, relative, resolve, sep } from "node:path";

const manifestKeys = new Set([
    "schemaVersion",
    "buildId",
    "clientRoot",
    "serverEntry",
    "testFiles",
    "runtimeAssets",
    "outputs",
    "clientFiles",
]);
const fileKeys = new Set(["path", "buildId", "size", "sha256"]);
const buildIdPattern = /^[a-z0-9]+(?:-[a-z0-9]+)*$/;
const sha256Pattern = /^[a-f0-9]{64}$/;
const CURRENT_MANIFEST_READ_ATTEMPTS = 32;
const transientCurrentManifestCodes = new Set(["ENOENT", "EBUSY", "EPERM", "EACCES"]);

function fail(path, message) {
    throw new TypeError(`${path}: ${message}`);
}

function requireRecord(value, path) {
    if (value === null || typeof value !== "object" || Array.isArray(value)) {
        fail(path, "expected object");
    }
    return value;
}

function requireKeys(record, keys, path) {
    for (const key of Object.keys(record)) {
        if (!keys.has(key)) {
            fail(`${path}.${key}`, `unknown key ${key}`);
        }
    }
    for (const key of keys) {
        if (!Object.hasOwn(record, key)) {
            fail(`${path}.${key}`, `missing key ${key}`);
        }
    }
}

function requireString(value, path) {
    if (typeof value !== "string" || value.length === 0) {
        fail(path, "expected non-empty string");
    }
    return value;
}

function requireArray(value, path) {
    if (!Array.isArray(value)) {
        fail(path, "expected array");
    }
    return value;
}

function requirePortablePath(value, path) {
    const filePath = requireString(value, path);
    const segments = filePath.split("/");
    if (filePath.startsWith("/") || filePath.includes("\\") || segments.some((segment) => (
        segment.length === 0 || segment === "." || segment === ".."
    ))) {
        fail(path, "expected normalized portable relative path");
    }
    return filePath;
}

function requireSafeInteger(value, path) {
    if (!Number.isSafeInteger(value) || value < 0) {
        fail(path, "expected nonnegative safe integer");
    }
    return value;
}

function parseFileEntry(value, path, buildId) {
    const record = requireRecord(value, path);
    requireKeys(record, fileKeys, path);
    const entryBuildId = requireString(record.buildId, `${path}.buildId`);
    if (entryBuildId !== buildId) {
        fail(`${path}.buildId`, "stale build ID");
    }
    const sha256 = requireString(record.sha256, `${path}.sha256`);
    if (!sha256Pattern.test(sha256)) {
        fail(`${path}.sha256`, "invalid SHA-256 digest");
    }
    return {
        path: requirePortablePath(record.path, `${path}.path`),
        buildId: entryBuildId,
        size: requireSafeInteger(record.size, `${path}.size`),
        sha256,
    };
}

export function parseBuildManifest(value) {
    const record = requireRecord(value, "manifest");
    requireKeys(record, manifestKeys, "manifest");
    if (record.schemaVersion !== 1) {
        fail("manifest.schemaVersion", "expected literal 1");
    }
    const buildId = requireString(record.buildId, "manifest.buildId");
    if (!buildIdPattern.test(buildId)) {
        fail("manifest.buildId", "invalid build ID");
    }
    const clientRoot = requirePortablePath(record.clientRoot, "manifest.clientRoot");
    if (clientRoot !== `builds/${buildId}`) {
        fail("manifest.clientRoot", "does not identify current build ID");
    }
    const outputs = requireArray(record.outputs, "manifest.outputs").map((entry, index) => (
        parseFileEntry(entry, `manifest.outputs[${index}]`, buildId)
    ));
    const outputByPath = new Map();
    for (const [index, output] of outputs.entries()) {
        if (!output.path.startsWith(`${clientRoot}/`)) {
            fail(`manifest.outputs[${index}].path`, "output is outside current build root");
        }
        if (outputByPath.has(output.path)) {
            fail(`manifest.outputs[${index}].path`, `duplicate output ${output.path}`);
        }
        outputByPath.set(output.path, output);
    }

    const runtimeAssets = requireArray(record.runtimeAssets, "manifest.runtimeAssets").map((entry, index) => (
        parseFileEntry(entry, `manifest.runtimeAssets[${index}]`, buildId)
    ));
    const uniqueRuntimeAssets = new Set();
    for (const [index, runtimeAsset] of runtimeAssets.entries()) {
        if (!runtimeAsset.path.startsWith(`${clientRoot}/runtime/`)) {
            fail(`manifest.runtimeAssets[${index}].path`, "runtime asset is outside current runtime root");
        }
        if (uniqueRuntimeAssets.has(runtimeAsset.path)) {
            fail(`manifest.runtimeAssets[${index}].path`, `duplicate runtime asset ${runtimeAsset.path}`);
        }
        uniqueRuntimeAssets.add(runtimeAsset.path);
        const output = outputByPath.get(runtimeAsset.path);
        if (output === undefined || output.size !== runtimeAsset.size || output.sha256 !== runtimeAsset.sha256) {
            fail(`manifest.runtimeAssets[${index}]`, "runtime asset does not match a current output");
        }
    }

    const serverEntry = requirePortablePath(record.serverEntry, "manifest.serverEntry");
    if (serverEntry !== `${clientRoot}/src/server/cli.js` || !outputByPath.has(serverEntry)) {
        fail("manifest.serverEntry", "serverEntry does not correspond to a current output");
    }
    const testFiles = requireArray(record.testFiles, "manifest.testFiles").map((entry, index) => (
        requirePortablePath(entry, `manifest.testFiles[${index}]`)
    ));
    const uniqueTests = new Set();
    for (const [index, testFile] of testFiles.entries()) {
        if (!testFile.startsWith(`${clientRoot}/tests/`) || !outputByPath.has(testFile)) {
            fail(`manifest.testFiles[${index}]`, "testFiles entry does not correspond to a current output");
        }
        if (uniqueTests.has(testFile)) {
            fail(`manifest.testFiles[${index}]`, `duplicate test file ${testFile}`);
        }
        uniqueTests.add(testFile);
    }

    const clientFiles = requireArray(record.clientFiles, "manifest.clientFiles").map((entry, index) => (
        parseFileEntry(entry, `manifest.clientFiles[${index}]`, buildId)
    ));
    const uniqueClientFiles = new Set();
    for (const [index, clientFile] of clientFiles.entries()) {
        if (clientFile.path.startsWith(`${clientRoot}/`)) {
            fail(`manifest.clientFiles[${index}].path`, "client path must be relative to client root");
        }
        if (uniqueClientFiles.has(clientFile.path)) {
            fail(`manifest.clientFiles[${index}].path`, `duplicate client file ${clientFile.path}`);
        }
        uniqueClientFiles.add(clientFile.path);
        const output = outputByPath.get(`${clientRoot}/${clientFile.path}`);
        if (output === undefined || output.size !== clientFile.size || output.sha256 !== clientFile.sha256) {
            fail(`manifest.clientFiles[${index}]`, "client file does not match a current output");
        }
    }
    return {
        schemaVersion: 1,
        buildId,
        clientRoot,
        serverEntry,
        testFiles,
        runtimeAssets,
        outputs,
        clientFiles,
    };
}

function requireContained(root, candidate, path) {
    const relativePath = relative(root, candidate);
    if (relativePath === "" || relativePath === ".." || relativePath.startsWith(`..${sep}`) || isAbsolute(relativePath)) {
        fail(path, "path escapes canonical static root");
    }
    return candidate;
}

async function verifyOutput(canonicalRoot, entry) {
    const requestedPath = requireContained(
        canonicalRoot,
        resolve(canonicalRoot, ...entry.path.split("/")),
        entry.path,
    );
    const firstCanonicalPath = requireContained(canonicalRoot, await realpath(requestedPath), entry.path);
    const canonicalPath = requireContained(canonicalRoot, await realpath(requestedPath), entry.path);
    if (canonicalPath !== firstCanonicalPath) {
        fail(entry.path, "path changed during canonicalization");
    }
    const handle = await open(canonicalPath, "r");
    let bytes;
    try {
        const metadata = await handle.stat();
        if (!metadata.isFile() || metadata.size !== entry.size) {
            fail(entry.path, "size does not match current output manifest");
        }
        bytes = await handle.readFile();
    } finally {
        await handle.close();
    }
    const digest = createHash("sha256").update(bytes).digest("hex");
    if (bytes.length !== entry.size || digest !== entry.sha256) {
        fail(entry.path, "SHA-256 digest does not match current output manifest");
    }
    return canonicalPath;
}

function isCurrentManifestPointer(canonicalRoot, manifestPath) {
    return resolve(manifestPath) === resolve(canonicalRoot, "build-manifest.json");
}

export function isTransientCurrentManifestPointerError(error) {
    return error !== null
        && typeof error === "object"
        && transientCurrentManifestCodes.has(error.code);
}

async function readCurrentManifestSnapshot(
    canonicalRoot,
    manifestPath,
    {
        readFileImpl = readFile,
        realpathImpl = realpath,
        retryDelay = async () => await new Promise((resolveRetry) => setTimeout(resolveRetry, 2)),
    } = {},
) {
    const attempts = isCurrentManifestPointer(canonicalRoot, manifestPath)
        ? CURRENT_MANIFEST_READ_ATTEMPTS
        : 1;
    let lastError;
    for (let attempt = 0; attempt < attempts; attempt += 1) {
        try {
            const canonicalManifestPath = requireContained(
                canonicalRoot,
                await realpathImpl(resolve(manifestPath)),
                "manifestPath",
            );
            return {
                canonicalManifestPath,
                manifest: parseBuildManifest(JSON.parse(await readFileImpl(canonicalManifestPath, "utf8"))),
            };
        } catch (error) {
            if (!isTransientCurrentManifestPointerError(error) || attempt === attempts - 1) {
                throw error;
            }
            lastError = error;
            await retryDelay();
        }
    }
    throw lastError;
}

export async function verifyManifestOutput({ staticRoot, manifestPath, outputPath, readFileImpl, realpathImpl, retryDelay }) {
    const canonicalRoot = await realpath(resolve(staticRoot));
    const { manifest } = await readCurrentManifestSnapshot(canonicalRoot, manifestPath, {
        readFileImpl,
        realpathImpl,
        retryDelay,
    });
    const entry = manifest.runtimeAssets.find((runtimeAsset) => runtimeAsset.path === outputPath);
    if (entry === undefined) {
        fail(outputPath, "output is missing from the current-build runtime asset allowlist");
    }
    return verifyOutput(canonicalRoot, entry);
}

export async function loadVerifiedBuildManifest({ staticRoot, manifestPath, readFileImpl, realpathImpl, retryDelay }) {
    const canonicalRoot = await realpath(resolve(staticRoot));
    const io = { readFileImpl, realpathImpl, retryDelay };
    const { manifest } = await readCurrentManifestSnapshot(canonicalRoot, manifestPath, io);
    const canonicalPinnedManifestPath = requireContained(
        canonicalRoot,
        await (realpathImpl ?? realpath)(join(canonicalRoot, ...manifest.clientRoot.split("/"), "build-manifest.json")),
        "pinnedManifestPath",
    );
    const pinnedManifest = parseBuildManifest(JSON.parse(await (readFileImpl ?? readFile)(canonicalPinnedManifestPath, "utf8")));
    if (JSON.stringify(pinnedManifest) !== JSON.stringify(manifest)) {
        fail("pinnedManifestPath", "does not exactly match the published current manifest");
    }
    const outputByPath = new Map(manifest.outputs.map((entry) => [entry.path, entry]));
    const executablePaths = [manifest.serverEntry, ...manifest.testFiles];
    const verifiedPaths = new Map();
    for (const executablePath of executablePaths) {
        const entry = outputByPath.get(executablePath);
        if (entry === undefined) {
            fail(executablePath, "executable is missing from outputs");
        }
        verifiedPaths.set(executablePath, await verifyOutput(canonicalRoot, entry));
    }
    return Object.freeze({
        staticRoot: canonicalRoot,
        manifestPath: canonicalPinnedManifestPath,
        manifest,
        verifiedPaths,
    });
}

export async function loadVerifiedRuntimeAsset({ staticRoot, manifestPath, outputPath, readFileImpl, realpathImpl, retryDelay }) {
    const verified = await loadVerifiedBuildManifest({ staticRoot, manifestPath, readFileImpl, realpathImpl, retryDelay });
    const entry = verified.manifest.runtimeAssets.find((runtimeAsset) => runtimeAsset.path === outputPath);
    if (entry === undefined) {
        fail(outputPath, "output is missing from the current-build runtime asset allowlist");
    }
    return Object.freeze({
        staticRoot: verified.staticRoot,
        manifestPath: verified.manifestPath,
        manifest: verified.manifest,
        outputPath,
        path: await verifyOutput(verified.staticRoot, entry),
    });
}
