// dat-skill-flow-build:20260801023815489-208dde217c444553bb5f59d9637e1aaa
import { createHash } from "node:crypto";
import { open, readFile, realpath } from "node:fs/promises";
import { createServer,                                                        } from "node:http";
                                            
import { extname, isAbsolute, relative, resolve, sep } from "node:path";

import { createDiagnostic } from "../diagnostics/envelope.js";
import {
    buildManifestSchema,
                        
                       
} from "./build-manifest.js";

export const LOOPBACK_HOST = "127.0.0.1"         ;

const securityHeaders = {
    "Content-Security-Policy": "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data: blob:; connect-src 'self'; object-src 'none'; base-uri 'none'; frame-ancestors 'none'",
    "Cross-Origin-Opener-Policy": "same-origin",
    "Referrer-Policy": "no-referrer",
    "X-Content-Type-Options": "nosniff",
}         ;

const contentTypes                         = {
    ".css": "text/css; charset=utf-8",
    ".html": "text/html; charset=utf-8",
    ".js": "text/javascript; charset=utf-8",
    ".json": "application/json; charset=utf-8",
    ".map": "application/json; charset=utf-8",
    ".svg": "image/svg+xml",
};

                                           
                       
                         
 

                               
                          
                                
                            
                                                          
 

function applySecurityHeaders(response                )       {
    for (const [name, value] of Object.entries(securityHeaders)) {
        response.setHeader(name, value);
    }
}

function sendBody(
    request                 ,
    response                ,
    status        ,
    contentType        ,
    body                 ,
)       {
    applySecurityHeaders(response);
    response.statusCode = status;
    response.setHeader("Content-Type", contentType);
    response.setHeader("Content-Length", Buffer.byteLength(body));
    if (request.method === "HEAD") {
        response.end();
    } else {
        response.end(body);
    }
}

function isContained(root        , candidate        )          {
    const relativePath = relative(root, candidate);
    return relativePath !== ""
        && relativePath !== ".."
        && !relativePath.startsWith(`..${sep}`)
        && !isAbsolute(relativePath);
}

function portablePathToNative(filePath        )         {
    return filePath.split("/").join(sep);
}

async function loadStaticConfiguration(options                          )                               {
    const canonicalRoot = await realpath(resolve(options.staticRoot));
    const canonicalManifestPath = await realpath(resolve(options.manifestPath));
    if (!isContained(canonicalRoot, canonicalManifestPath)) {
        throw new Error("Build manifest escapes the static root.");
    }
    const manifest = buildManifestSchema.parse(JSON.parse(await readFile(canonicalManifestPath, "utf8")));
    const requestedClientRoot = resolve(canonicalRoot, portablePathToNative(manifest.clientRoot));
    if (!isContained(canonicalRoot, requestedClientRoot)) {
        throw new Error("Manifest client root escapes the static root.");
    }
    const canonicalClientRoot = await realpath(requestedClientRoot);
    if (!isContained(canonicalRoot, canonicalClientRoot)) {
        throw new Error("Canonical client root escapes the static root.");
    }
    return {
        canonicalRoot,
        canonicalClientRoot,
        manifest,
        clientFileByPath: new Map(manifest.clientFiles.map((entry) => [entry.path, entry])),
    };
}

function decodeRequestPath(rawTarget        )         {
    const queryIndex = rawTarget.indexOf("?");
    const rawPath = queryIndex < 0 ? rawTarget : rawTarget.slice(0, queryIndex);
    if (!rawPath.startsWith("/")) {
        throw new Error("Request target must be origin-form.");
    }

    let decoded = rawPath;
    for (let pass = 0; pass < 8; pass += 1) {
        const next = decodeURIComponent(decoded);
        if (next.includes("\0") || next.includes("\\")) {
            throw new Error("Unsafe path separator.");
        }
        const segments = next.split("/");
        if (segments.some((segment) => segment === "." || segment === "..")) {
            throw new Error("Unsafe traversal segment.");
        }
        if (next === decoded) {
            return next;
        }
        decoded = next;
    }
    throw new Error("Excessively encoded request path.");
}

function sendJson(request                 , response                , status        , value         )       {
    sendBody(request, response, status, "application/json; charset=utf-8", JSON.stringify(value));
}

function rejectMethod(request                 , response                )       {
    response.setHeader("Allow", "GET, HEAD");
    sendJson(request, response, 405, {
        ok: false,
        diagnostics: [createDiagnostic("method-not-allowed", `Method ${request.method ?? "UNKNOWN"} is not allowed.`)],
    });
}

async function serveStatic(
    request                 ,
    response                ,
    staticConfiguration                     ,
    pathname        ,
)                {
    const relativePath = pathname === "/" ? "index.html" : pathname.replace(/^\/+/, "");
    const expected = staticConfiguration.clientFileByPath.get(relativePath);
    if (expected === undefined || expected.buildId !== staticConfiguration.manifest.buildId) {
        sendJson(request, response, 404, {
            ok: false,
            diagnostics: [createDiagnostic("not-found", "The requested asset was not found.")],
        });
        return;
    }

    try {
        const candidate = resolve(staticConfiguration.canonicalClientRoot, portablePathToNative(relativePath));
        if (!isContained(staticConfiguration.canonicalClientRoot, candidate)) {
            throw new Error("Requested path escapes the current client root.");
        }
        const firstCanonicalCandidate = await realpath(candidate);
        if (!isContained(staticConfiguration.canonicalClientRoot, firstCanonicalCandidate)) {
            throw new Error("Requested path resolves outside the current client root.");
        }

        // Re-resolve immediately before open. Reads use the resulting handle so a
        // later pathname substitution cannot redirect the open file descriptor.
        const canonicalCandidate = await realpath(candidate);
        if (canonicalCandidate !== firstCanonicalCandidate
            || !isContained(staticConfiguration.canonicalClientRoot, canonicalCandidate)) {
            throw new Error("Requested path changed during canonicalization.");
        }
        const handle = await open(canonicalCandidate, "r");
        let body        ;
        try {
            const metadata = await handle.stat();
            if (!metadata.isFile() || metadata.size !== expected.size) {
                throw new Error("Current-build asset metadata does not match its manifest.");
            }
            body = await handle.readFile();
        } finally {
            await handle.close();
        }
        if (createHash("sha256").update(body).digest("hex") !== expected.sha256) {
            throw new Error("Current-build asset digest does not match its manifest.");
        }
        if (body.length !== expected.size) {
            throw new Error("Not a file");
        }
        sendBody(
            request,
            response,
            200,
            contentTypes[extname(relativePath).toLowerCase()] ?? "application/octet-stream",
            body,
        );
    } catch {
        sendJson(request, response, 404, {
            ok: false,
            diagnostics: [createDiagnostic("not-found", "The requested asset was not found.")],
        });
    }
}

export function createApplicationServer(options                          )         {
    let staticConfigurationPromise                                          ;
    const getStaticConfiguration = ()                               => {
        staticConfigurationPromise ??= loadStaticConfiguration(options);
        return staticConfigurationPromise;
    };

    return createServer((request, response) => {
        void (async () => {
            const method = request.method ?? "GET";
            if (method !== "GET" && method !== "HEAD") {
                rejectMethod(request, response);
                return;
            }

            let pathname        ;
            try {
                pathname = decodeRequestPath(request.url ?? "/");
            } catch {
                sendJson(request, response, 400, {
                    ok: false,
                    diagnostics: [createDiagnostic("not-found", "The requested path is malformed or unsafe.")],
                });
                return;
            }
            if (pathname === "/api/health") {
                sendJson(request, response, 200, {
                    ok: true,
                    data: {
                        status: "ok",
                        host: LOOPBACK_HOST,
                        authorityEntries: 0,
                    },
                    diagnostics: [],
                });
                return;
            }

            if (pathname.startsWith("/api/")) {
                sendJson(request, response, 404, {
                    ok: false,
                    diagnostics: [createDiagnostic("not-found", "The requested API route was not found.")],
                });
                return;
            }

            await serveStatic(request, response, await getStaticConfiguration(), pathname);
        })().catch(() => {
            if (!response.headersSent) {
                sendJson(request, response, 500, {
                    ok: false,
                    diagnostics: [createDiagnostic("internal-error", "The local server encountered an error.")],
                });
            } else {
                response.destroy();
            }
        });
    });
}

export async function listenLoopback(server        , port        )                  {
    await new Promise      ((resolveListen, rejectListen) => {
        const onError = (error       )       => rejectListen(error);
        server.once("error", onError);
        server.listen(port, LOOPBACK_HOST, () => {
            server.off("error", onError);
            resolveListen();
        });
    });

    const address = server.address()                      ;
    if (address === null || address.address !== LOOPBACK_HOST) {
        await new Promise      ((resolveClose) => server.close(() => resolveClose()));
        throw new Error("Server did not bind to the required loopback address.");
    }
    return `http://${LOOPBACK_HOST}:${address.port}`;
}
