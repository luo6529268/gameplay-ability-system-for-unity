import { createHash } from "node:crypto";
import {
    closeSync,
    fstatSync,
    openSync,
    readFileSync,
    realpathSync,
} from "node:fs";
import { registerHooks } from "node:module";
import { extname, isAbsolute, relative, resolve, sep } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

import { parseBuildManifest } from "./manifest-integrity.mjs";

export const VERIFIED_LOADER_STATIC_ROOT_ENV = "DAT_SKILL_FLOW_STATIC_ROOT";
export const VERIFIED_LOADER_MANIFEST_ENV = "DAT_SKILL_FLOW_MANIFEST_PATH";

function fail(path, message) {
    throw new Error(`${path}: ${message}`);
}

function requireContained(root, candidate, path) {
    const relativePath = relative(root, candidate);
    if (relativePath === "" || relativePath === ".." || relativePath.startsWith(`..${sep}`) || isAbsolute(relativePath)) {
        fail(path, "path escapes the verified root");
    }
    return candidate;
}

function toPortable(filePath) {
    return filePath.split(sep).join("/");
}

function isJavaScriptModuleOutput(entry) {
    const extension = extname(entry.path).toLowerCase();
    return extension === ".js" || extension === ".mjs";
}

export function createVerifiedModuleHooks({ staticRoot, manifestPath }) {
    const canonicalRoot = realpathSync(resolve(staticRoot));
    const canonicalManifestPath = requireContained(
        canonicalRoot,
        realpathSync(resolve(manifestPath)),
        "manifestPath",
    );
    const manifest = parseBuildManifest(JSON.parse(readFileSync(canonicalManifestPath, "utf8")));
    const canonicalManagedBuildsRoot = requireContained(
        canonicalRoot,
        realpathSync(resolve(canonicalRoot, "builds")),
        "managedBuildsRoot",
    );
    const canonicalBuildRoot = requireContained(
        canonicalManagedBuildsRoot,
        realpathSync(resolve(canonicalRoot, ...manifest.clientRoot.split("/"))),
        "manifest.clientRoot",
    );
    const moduleOutputByPath = new Map();
    for (const output of manifest.outputs) {
        if (isJavaScriptModuleOutput(output)) {
            moduleOutputByPath.set(output.path, output);
        }
    }

    function isFileUrlWithin(root, url) {
        if (typeof url !== "string" || !url.startsWith("file:")) {
            return false;
        }
        try {
            const filePath = fileURLToPath(url);
            const relativePath = relative(root, filePath);
            return relativePath !== ""
                && relativePath !== ".."
                && !relativePath.startsWith(`..${sep}`)
                && !isAbsolute(relativePath);
        } catch {
            return false;
        }
    }

    function isCurrentBuildFileUrl(url) {
        return isFileUrlWithin(canonicalBuildRoot, url);
    }

    function isManagedBuildFileUrl(url) {
        return isFileUrlWithin(canonicalManagedBuildsRoot, url);
    }

    function readVerifiedSource(url) {
        const requestedPath = fileURLToPath(url);
        requireContained(canonicalBuildRoot, requestedPath, url);
        const firstCanonicalPath = requireContained(
            canonicalBuildRoot,
            realpathSync(requestedPath),
            url,
        );
        const canonicalPath = requireContained(
            canonicalBuildRoot,
            realpathSync(requestedPath),
            url,
        );
        if (canonicalPath !== firstCanonicalPath) {
            fail(url, "module path changed during canonicalization");
        }
        const outputPath = toPortable(relative(canonicalRoot, canonicalPath));
        const expected = moduleOutputByPath.get(outputPath);
        if (expected === undefined) {
            fail(outputPath, "module is not in the current-build allowlist");
        }
        if (expected.buildId !== manifest.buildId) {
            fail(outputPath, "module has a stale build ID");
        }

        const descriptor = openSync(canonicalPath, "r");
        let source;
        try {
            const metadata = fstatSync(descriptor);
            if (!metadata.isFile() || metadata.size !== expected.size) {
                fail(outputPath, "module size does not match the current-build manifest");
            }
            source = readFileSync(descriptor);
        } finally {
            closeSync(descriptor);
        }
        if (source.length !== expected.size
            || createHash("sha256").update(source).digest("hex") !== expected.sha256) {
            fail(outputPath, "module SHA-256 digest does not match the current-build manifest");
        }
        return source;
    }

    const hooks = {
        resolve(specifier, context, nextResolve) {
            const resolved = nextResolve(specifier, context);
            if (isManagedBuildFileUrl(resolved.url) && !isCurrentBuildFileUrl(resolved.url)) {
                fail(resolved.url, "stale module is outside the current verified build");
            }
            if (!isCurrentBuildFileUrl(context.parentURL)) {
                return resolved;
            }
            if (resolved.url.startsWith("node:")) {
                return resolved;
            }
            if (!isCurrentBuildFileUrl(resolved.url)) {
                fail(resolved.url, "transitive import leaves the current verified build root");
            }
            return resolved;
        },
        load(url, context, nextLoad) {
            if (isManagedBuildFileUrl(url) && !isCurrentBuildFileUrl(url)) {
                fail(url, "stale module is outside the current verified build");
            }
            if (!isCurrentBuildFileUrl(url)) {
                return nextLoad(url, context);
            }
            return {
                format: "module",
                shortCircuit: true,
                source: readVerifiedSource(url),
            };
        },
    };

    return Object.freeze({
        staticRoot: canonicalRoot,
        manifestPath: canonicalManifestPath,
        manifest,
        entryUrl: pathToFileURL(resolve(canonicalRoot, ...manifest.serverEntry.split("/"))).href,
        hooks: Object.freeze(hooks),
    });
}

export function registerVerifiedLoader(options) {
    const configuration = createVerifiedModuleHooks(options);
    const registration = registerHooks(configuration.hooks);
    return Object.freeze({ ...configuration, registration });
}
