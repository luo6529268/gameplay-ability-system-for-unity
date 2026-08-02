// dat-skill-flow-build:20260801032054397-c27419aacb4c4795ac1f58b93631b035
import { createHash, randomBytes, timingSafeEqual } from "node:crypto";
import { open, readFile, realpath } from "node:fs/promises";
import { createServer,                                                        } from "node:http";
                                            
import { extname, isAbsolute, relative, resolve, sep } from "node:path";

import { createDiagnostic } from "../diagnostics/envelope.js";
import {
    buildManifestSchema,
                        
                       
} from "./build-manifest.js";

export const LOOPBACK_HOST = "127.0.0.1"         ;
export const STATE_CHANGE_TOKEN_HEADER = "x-dat-skill-flow-token"         ;
export const MAX_STATIC_FILE_BYTES = 8 * 1024 * 1024;
export const MAX_STATIC_TOTAL_BYTES = 32 * 1024 * 1024;
export const HTTP_SERVER_LIMITS = Object.freeze({
    headersTimeoutMs: 5_000,
    requestTimeoutMs: 10_000,
    keepAliveTimeoutMs: 1_000,
    maxConnections: 32,
});

const processStateChangeToken = randomBytes(32).toString("base64url");
const applicationServers = new WeakSet        ();

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

                                           
                       
                         
 

                               
                            
                                                            
 

                            
                          
                 
 

                                            
                                                  
                  
 

export function getApplicationServerSecurity(server        )                            {
    if (!applicationServers.has(server)) {
        throw new TypeError("Security metadata is available only for an application server.");
    }
    return Object.freeze({
        tokenHeader: STATE_CHANGE_TOKEN_HEADER,
        token: processStateChangeToken,
    });
}

export function authorizeStateChangingRequest(server        , providedToken                    )          {
    if (!applicationServers.has(server) || typeof providedToken !== "string") {
        return false;
    }
    const expected = Buffer.from(processStateChangeToken);
    const provided = Buffer.from(providedToken);
    return expected.length === provided.length && timingSafeEqual(expected, provided);
}

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
    let totalSize = 0;
    for (const entry of manifest.clientFiles) {
        if (entry.size > MAX_STATIC_FILE_BYTES) {
            throw new Error(`Static file exceeds limit: ${entry.path}`);
        }
        totalSize += entry.size;
        if (!Number.isSafeInteger(totalSize) || totalSize > MAX_STATIC_TOTAL_BYTES) {
            throw new Error("Static client manifest exceeds aggregate size limit.");
        }
    }
    const requestedClientRoot = resolve(canonicalRoot, portablePathToNative(manifest.clientRoot));
    if (!isContained(canonicalRoot, requestedClientRoot)) {
        throw new Error("Manifest client root escapes the static root.");
    }
    const canonicalClientRoot = await realpath(requestedClientRoot);
    if (!isContained(canonicalRoot, canonicalClientRoot)) {
        throw new Error("Canonical client root escapes the static root.");
    }
    const clientFileByPath = new Map                          ();
    for (const entry of manifest.clientFiles) {
        const candidate = resolve(canonicalClientRoot, portablePathToNative(entry.path));
        if (!isContained(canonicalClientRoot, candidate)) {
            throw new Error(`Static file escapes the current client root: ${entry.path}`);
        }
        const firstCanonicalCandidate = await realpath(candidate);
        if (!isContained(canonicalClientRoot, firstCanonicalCandidate)) {
            throw new Error(`Static file resolves outside the current client root: ${entry.path}`);
        }
        const canonicalCandidate = await realpath(candidate);
        if (canonicalCandidate !== firstCanonicalCandidate || !isContained(canonicalClientRoot, canonicalCandidate)) {
            throw new Error(`Static file changed during canonicalization: ${entry.path}`);
        }
        const handle = await open(canonicalCandidate, "r");
        let body        ;
        try {
            const metadata = await handle.stat();
            if (!metadata.isFile() || metadata.size !== entry.size) {
                throw new Error(`Static file size does not match manifest: ${entry.path}`);
            }
            body = await handle.readFile();
        } finally {
            await handle.close();
        }
        if (body.length !== entry.size || createHash("sha256").update(body).digest("hex") !== entry.sha256) {
            throw new Error(`Static file SHA-256 does not match manifest: ${entry.path}`);
        }
        clientFileByPath.set(entry.path, Object.freeze({ entry, body }));
    }
    return {
        manifest,
        clientFileByPath,
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

function expectedRequestOrigin(server        )                     {
    const address = server.address()                      ;
    return address === null || address.address !== LOOPBACK_HOST
        ? undefined
        : `http://${LOOPBACK_HOST}:${address.port}`;
}

function hasTrustedRequestOrigin(server        , request                 )          {
    const expectedOrigin = expectedRequestOrigin(server);
    if (expectedOrigin === undefined || request.headers.host !== expectedOrigin.slice("http://".length)) {
        return false;
    }
    const origin = request.headers.origin;
    return origin === undefined || origin === expectedOrigin;
}

function advertisesRequestBody(request                 )          {
    if (request.headers["transfer-encoding"] !== undefined) {
        return true;
    }
    const contentLength = request.headers["content-length"];
    return contentLength !== undefined && contentLength !== "0";
}

async function serveStatic(
    request                 ,
    response                ,
    staticConfiguration                     ,
    pathname        ,
)                {
    const relativePath = pathname === "/" ? "index.html" : pathname.replace(/^\/+/, "");
    const cached = staticConfiguration.clientFileByPath.get(relativePath);
    if (cached === undefined || cached.entry.buildId !== staticConfiguration.manifest.buildId) {
        sendJson(request, response, 404, {
            ok: false,
            diagnostics: [createDiagnostic("not-found", "The requested asset was not found.")],
        });
        return;
    }

    sendBody(
        request,
        response,
        200,
        contentTypes[extname(relativePath).toLowerCase()] ?? "application/octet-stream",
        cached.body,
    );
}

export function createApplicationServer(options                          )         {
    let staticConfigurationPromise                                          ;
    const getStaticConfiguration = ()                               => {
        staticConfigurationPromise ??= loadStaticConfiguration(options);
        return staticConfigurationPromise;
    };

    const server = createServer((request, response) => {
        void (async () => {
            if (!hasTrustedRequestOrigin(server, request)) {
                sendJson(request, response, 403, {
                    ok: false,
                    diagnostics: [createDiagnostic("forbidden-request", "Host or Origin is not the active loopback origin.")],
                });
                return;
            }
            if (advertisesRequestBody(request)) {
                response.setHeader("Connection", "close");
                sendJson(request, response, 413, {
                    ok: false,
                    diagnostics: [createDiagnostic("request-body-not-allowed", "Request bodies are not accepted by the Gate 0 server.")],
                });
                return;
            }
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
    server.headersTimeout = HTTP_SERVER_LIMITS.headersTimeoutMs;
    server.requestTimeout = HTTP_SERVER_LIMITS.requestTimeoutMs;
    server.keepAliveTimeout = HTTP_SERVER_LIMITS.keepAliveTimeoutMs;
    server.maxConnections = HTTP_SERVER_LIMITS.maxConnections;
    server.maxHeadersCount = 64;
    applicationServers.add(server);
    return server;
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
