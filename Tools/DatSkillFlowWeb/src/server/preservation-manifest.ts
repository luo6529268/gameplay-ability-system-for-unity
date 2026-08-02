import { createHash } from "node:crypto";
import { opendir, readFile, stat } from "node:fs/promises";
import { relative, resolve, sep } from "node:path";

export interface PreservationEntry {
    path: string;
    size: number;
    sha256: string;
}

export interface PreservationManifest {
    schemaVersion: 1;
    root: ".";
    entries: PreservationEntry[];
}

export interface PreservationFailure {
    path: string;
    reason: "missing" | "changed";
    expected: PreservationEntry;
    actual?: PreservationEntry;
}

export interface ManifestOptions {
    ignoreDirectoryNames?: ReadonlySet<string>;
    ignoreRelativePaths?: ReadonlySet<string>;
}

// Generated dependency/build/report trees are not repository inputs. The after
// manifest is self-generated output and cannot be evidence for its own preservation.
export const REPOSITORY_PRESERVATION_OPTIONS: ManifestOptions = Object.freeze({
    ignoreDirectoryNames: new Set([
        "node_modules",
        "dist",
        "test-results",
        "playwright-report",
        ".vite",
    ]),
    ignoreRelativePaths: new Set(["audit/preservation-after.json"]),
});

function toPortableRelativePath(root: string, filePath: string): string {
    return relative(root, filePath).split(sep).join("/");
}

async function collectFiles(
    root: string,
    directory: string,
    output: string[],
    ignored: ReadonlySet<string>,
    ignoredRelativePaths: ReadonlySet<string>,
): Promise<void> {
    const handle = await opendir(directory);
    for await (const entry of handle) {
        if (entry.isDirectory() && ignored.has(entry.name)) {
            continue;
        }

        const absolutePath = resolve(directory, entry.name);
        const relativePath = toPortableRelativePath(root, absolutePath);
        if (ignoredRelativePaths.has(relativePath)) {
            continue;
        }
        if (entry.isDirectory()) {
            await collectFiles(root, absolutePath, output, ignored, ignoredRelativePaths);
        } else if (entry.isFile()) {
            output.push(absolutePath);
        }
    }
}

export async function createPreservationManifest(
    rootPath: string,
    options: ManifestOptions = {},
): Promise<PreservationManifest> {
    const root = resolve(rootPath);
    const files: string[] = [];
    await collectFiles(
        root,
        root,
        files,
        options.ignoreDirectoryNames ?? new Set(),
        options.ignoreRelativePaths ?? new Set(),
    );

    const entries = await Promise.all(files.map(async (filePath): Promise<PreservationEntry> => {
        const [bytes, metadata] = await Promise.all([readFile(filePath), stat(filePath)]);
        return {
            path: toPortableRelativePath(root, filePath),
            size: metadata.size,
            sha256: createHash("sha256").update(bytes).digest("hex"),
        };
    }));

    entries.sort((left, right) => left.path < right.path ? -1 : left.path > right.path ? 1 : 0);
    return { schemaVersion: 1, root: ".", entries };
}

export function assertPreserved(
    before: PreservationManifest,
    after: PreservationManifest,
): PreservationFailure[] {
    const afterByPath = new Map(after.entries.map((entry) => [entry.path, entry]));
    const failures: PreservationFailure[] = [];

    for (const expected of before.entries) {
        const actual = afterByPath.get(expected.path);
        if (actual === undefined) {
            failures.push({ path: expected.path, reason: "missing", expected });
        } else if (actual.size !== expected.size || actual.sha256 !== expected.sha256) {
            failures.push({ path: expected.path, reason: "changed", expected, actual });
        }
    }

    return failures;
}
