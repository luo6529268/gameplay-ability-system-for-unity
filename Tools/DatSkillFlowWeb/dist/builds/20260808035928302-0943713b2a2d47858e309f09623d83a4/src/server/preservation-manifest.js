// dat-skill-flow-build:20260808035928302-0943713b2a2d47858e309f09623d83a4
import { createHash } from "node:crypto";
import { opendir, readFile, stat } from "node:fs/promises";
import { relative, resolve, sep } from "node:path";

                                    
                 
                 
                   
 

                                       
                     
              
                                 
 

                                      
                 
                                  
                                
                               
 

                                  
                                               
                                              
 

// Generated dependency/build/report trees are not repository inputs. The after
// manifest is self-generated output and cannot be evidence for its own preservation.
export const REPOSITORY_PRESERVATION_OPTIONS                  = Object.freeze({
    ignoreDirectoryNames: new Set([
        "node_modules",
        "dist",
        "test-results",
        "playwright-report",
        ".vite",
    ]),
    ignoreRelativePaths: new Set(["audit/preservation-after.json"]),
});

function toPortableRelativePath(root        , filePath        )         {
    return relative(root, filePath).split(sep).join("/");
}

async function collectFiles(
    root        ,
    directory        ,
    output          ,
    ignored                     ,
    ignoredRelativePaths                     ,
)                {
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
    rootPath        ,
    options                  = {},
)                                {
    const root = resolve(rootPath);
    const files           = [];
    await collectFiles(
        root,
        root,
        files,
        options.ignoreDirectoryNames ?? new Set(),
        options.ignoreRelativePaths ?? new Set(),
    );

    const entries = await Promise.all(files.map(async (filePath)                             => {
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
    before                      ,
    after                      ,
)                        {
    const afterByPath = new Map(after.entries.map((entry) => [entry.path, entry]));
    const failures                        = [];

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
