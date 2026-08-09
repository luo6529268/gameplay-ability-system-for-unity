// dat-skill-flow-build:20260808015740587-250992ad1f8845fba4fb7101f86211bf
import { createHash, randomBytes, timingSafeEqual } from "node:crypto";
import { open, readFile, realpath } from "node:fs/promises";
import { createServer,                                                        } from "node:http";
                                            
import { basename, extname, isAbsolute, relative, resolve, sep } from "node:path";

import { createDiagnostic } from "../diagnostics/envelope.js";
import {
    buildManifestSchema,
                        
                       
} from "./build-manifest.js";
import {
    SafeSaveError,
    SafeSaveService,
                         
                                
} from "./safe-save.js";
import {
    MAX_DOCUMENT_BYTES,
    WorkspaceRegistry,
                                  
    WorkspaceSecurityError,
} from "./workspace-registry.js";
import {
    ProjectDatError,
                           
} from "./project-dat-service.js";
import {
    ProjectSkillError,
                             
} from "./project-skill-service.js";

export const LOOPBACK_HOST = "127.0.0.1"         ;
export const MAX_EPHEMERAL_LOOPBACK_LISTEN_ATTEMPTS = 8;
export const STATE_CHANGE_TOKEN_HEADER = "x-dat-skill-flow-token"         ;
export const MAX_STATIC_FILE_BYTES = 8 * 1024 * 1024;
export const MAX_STATIC_TOTAL_BYTES = 32 * 1024 * 1024;
export const MAX_JSON_BODY_BYTES = Math.ceil(MAX_DOCUMENT_BYTES * 4 / 3) + 64 * 1024;
export const HTTP_SERVER_LIMITS = Object.freeze({
    headersTimeoutMs: 5_000,
    requestTimeoutMs: 10_000,
    keepAliveTimeoutMs: 1_000,
    maxConnections: 32,
});

const processStateChangeToken = randomBytes(32).toString("base64url");
const applicationServers = new WeakSet        ();
const applicationServerWorkspaces = new WeakMap                           ();
const startupWorkspaceCapabilities = Object.freeze([
    "documents.open",
    "documents.read",
    "documents.save-as",
    "documents.overwrite",
]);

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

export async function loadPinnedStaticConfiguration(
    staticRoot        ,
    manifest               ,
)                               {
    const canonicalRoot = await realpath(resolve(staticRoot));
    const verifiedManifest = buildManifestSchema.parse(manifest);
    let totalSize = 0;
    for (const entry of verifiedManifest.clientFiles) {
        if (entry.size > MAX_STATIC_FILE_BYTES) {
            throw new Error(`Static file exceeds limit: ${entry.path}`);
        }
        totalSize += entry.size;
        if (!Number.isSafeInteger(totalSize) || totalSize > MAX_STATIC_TOTAL_BYTES) {
            throw new Error("Static client manifest exceeds aggregate size limit.");
        }
    }
    const requestedClientRoot = resolve(canonicalRoot, portablePathToNative(verifiedManifest.clientRoot));
    if (!isContained(canonicalRoot, requestedClientRoot)) {
        throw new Error("Manifest client root escapes the static root.");
    }
    const canonicalClientRoot = await realpath(requestedClientRoot);
    if (!isContained(canonicalRoot, canonicalClientRoot)) {
        throw new Error("Canonical client root escapes the static root.");
    }
    const clientFileByPath = new Map                          ();
    for (const entry of verifiedManifest.clientFiles) {
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
        manifest: verifiedManifest,
        clientFileByPath,
    };
}

async function loadStaticConfiguration(options                          )                               {
    const canonicalRoot = await realpath(resolve(options.staticRoot));
    const canonicalManifestPath = await realpath(resolve(options.manifestPath));
    if (!isContained(canonicalRoot, canonicalManifestPath)) {
        throw new Error("Build manifest escapes the static root.");
    }
    const manifest = buildManifestSchema.parse(JSON.parse(await readFile(canonicalManifestPath, "utf8")));
    return await loadPinnedStaticConfiguration(canonicalRoot, manifest);
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

function isRecord(value         )                                   {
    return typeof value === "object" && value !== null && !Array.isArray(value);
}

function requireString(record                         , name        )         {
    const value = record[name];
    if (typeof value !== "string" || value.length === 0) {
        throw new TypeError(`${name} must be a nonempty string.`);
    }
    return value;
}

function requireExactKeys(record                         , keys                   )       {
    const allowed = new Set(keys);
    if (Object.keys(record).some((key) => !allowed.has(key)) || keys.some((key) => !(key in record))) {
        throw new TypeError("The JSON object has missing or unknown fields.");
    }
}

function decodeBase64Content(value        )         {
    if (!/^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$/.test(value)) {
        throw new TypeError("contentBase64 must use canonical base64 encoding.");
    }
    const bytes = Buffer.from(value, "base64");
    if (bytes.length > MAX_DOCUMENT_BYTES) {
        throw new SafeSaveError("content-too-large", "Save content exceeds the document limit.");
    }
    return bytes;
}

async function readJsonBody(request                 )                                   {
    const contentType = request.headers["content-type"];
    if (typeof contentType !== "string" || contentType.split(";", 1)[0]?.trim().toLowerCase() !== "application/json") {
        throw Object.assign(new TypeError("Content-Type must be application/json."), { httpStatus: 415 });
    }
    const declaredLength = request.headers["content-length"];
    if (declaredLength !== undefined) {
        const length = Number(declaredLength);
        if (!Number.isSafeInteger(length) || length < 0 || length > MAX_JSON_BODY_BYTES) {
            throw Object.assign(new RangeError("The JSON request body exceeds the configured limit."), { httpStatus: 413 });
        }
    }
    const chunks           = [];
    let total = 0;
    for await (const rawChunk of request) {
        const chunk = Buffer.isBuffer(rawChunk) ? rawChunk : Buffer.from(rawChunk);
        total += chunk.length;
        if (total > MAX_JSON_BODY_BYTES) {
            request.resume();
            throw Object.assign(new RangeError("The JSON request body exceeds the configured limit."), { httpStatus: 413 });
        }
        chunks.push(chunk);
    }
    let value         ;
    try {
        value = JSON.parse(Buffer.concat(chunks, total).toString("utf8"));
    } catch (error) {
        throw new TypeError("The request body is not valid JSON.", { cause: error });
    }
    if (!isRecord(value)) {
        throw new TypeError("The request body must be a JSON object.");
    }
    return value;
}

function exactStateChangingOrigin(server        , request                 )          {
    const expectedOrigin = expectedRequestOrigin(server);
    return expectedOrigin !== undefined
        && request.headers.host === expectedOrigin.slice("http://".length)
        && request.headers.origin === expectedOrigin;
}

function safeRecoveryDetails(error               )                          {
    const observation = (value                             )                                      => (
        value === undefined
            ? undefined
            : {
                name: basename(value.path),
                exists: value.exists,
                ...(value.size === undefined ? {} : { size: value.size }),
                ...(value.sha256 === undefined ? {} : { sha256: value.sha256 }),
                ...(value.inspectionError === undefined ? {} : { inspectionError: value.inspectionError }),
            }
    );
    return {
        saveCode: error.code,
        ...(error.win32Code === undefined ? {} : { win32Code: error.win32Code }),
        ...(error.recovery === undefined ? {} : {
            recovery: {
                target: observation(error.recovery.target),
                replacement: observation(error.recovery.replacement),
                backup: observation(error.recovery.backup),
            },
        }),
    };
}

function sendApiError(request                 , response                , error         )       {
    if (error instanceof SafeSaveError) {
        const status = error.code === "content-too-large"
            ? 413
            : error.code === "challenge-expired"
                ? 410
                : [
                    "overwrite-required",
                    "challenge-invalid",
                    "challenge-content-mismatch",
                    "challenge-target-mismatch",
                    "external-change",
                ].includes(error.code)
                    ? 409
                    : 500;
        sendJson(request, response, status, {
            ok: false,
            diagnostics: [createDiagnostic("unsafe-save", error.message, safeRecoveryDetails(error))],
        });
        return;
    }
    if (error instanceof WorkspaceSecurityError) {
        const status = error.code === "root-grant-disabled" || error.code === "root-escape"
            ? 403
            : error.code === "unknown-root" || error.code === "unknown-document" || error.code === "not-a-file"
                ? 404
                : error.code === "read-too-large"
                    ? 413
                    : 400;
        sendJson(request, response, status, {
            ok: false,
            diagnostics: [createDiagnostic("unsafe-save", error.message, { workspaceCode: error.code })],
        });
        return;
    }
    if (error instanceof ProjectDatError) {
        const status = error.code === "unknown-session" || error.code === "unknown-object" || error.code === "object-unavailable" || error.code === "unknown-asset"
            ? 404
            : error.code === "invalid-asset" || error.code === "preview-failed"
                ? 422
                : error.code === "revision-conflict" || error.code === "read-only-session"
                    ? 409
                    : error.code === "invalid-request"
                        ? 400
                        : error.code === "project-disabled"
                            ? 503
                            : 500;
        sendJson(request, response, status, {
            ok: false,
            diagnostics: [createDiagnostic(
                status === 404
                    ? "not-found"
                    : error.code === "invalid-asset"
                        ? "missing-asset"
                        : error.code === "preview-failed"
                            ? "parse-failure"
                            : status === 400 || status === 409 ? "unsafe-save" : "internal-error",
                error.message,
                { projectCode: error.code },
            )],
        });
        return;
    }
    if (error instanceof ProjectSkillError) {
        const status = error.code === "schema-invalid"
            ? 422
            : error.code === "revision-conflict"
                ? 409
                : error.code === "invalid-request"
                    ? 400
                    : error.code === "project-disabled"
                        ? 503
                        : 500;
        sendJson(request, response, status, {
            ok: false,
            diagnostics: [createDiagnostic(
                status === 422 ? "invalid-sidecar" : status === 409 ? "unsafe-save" : status === 400 ? "invalid-request" : "internal-error",
                error.message,
                { projectSkillCode: error.code },
            )],
        });
        return;
    }
    const requestedStatus = (error                            )?.httpStatus;
    const status = typeof requestedStatus === "number" ? requestedStatus : 400;
    sendJson(request, response, status, {
        ok: false,
        diagnostics: [createDiagnostic(
            status === 413 ? "request-body-not-allowed" : "unsafe-save",
            error instanceof Error ? error.message : "The API request is invalid.",
        )],
    });
}

function rejectMethod(request                 , response                , allowedMethods = "GET, HEAD")       {
    response.setHeader("Allow", allowedMethods);
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

async function handleApiRequest(
    server        ,
    request                 ,
    response                ,
    pathname        ,
    workspace                   ,
    safeSave                 ,
    projectDatService                               ,
    projectSkillService                                 ,
)                   {
    if (pathname === "/api/health") {
        if (advertisesRequestBody(request)) {
            response.setHeader("Connection", "close");
            sendJson(request, response, 413, {
                ok: false,
                diagnostics: [createDiagnostic("request-body-not-allowed", "The health route does not accept a request body.")],
            });
            return true;
        }
        if (request.method !== "GET" && request.method !== "HEAD") {
            rejectMethod(request, response);
            return true;
        }
        sendJson(request, response, 200, {
            ok: true,
            data: {
                status: "ok",
                host: LOOPBACK_HOST,
                authorityEntries: 0,
            },
            diagnostics: [],
        });
        return true;
    }

    if (pathname === "/api/bootstrap") {
        if (advertisesRequestBody(request)) {
            response.setHeader("Connection", "close");
            sendJson(request, response, 413, {
                ok: false,
                diagnostics: [createDiagnostic("request-body-not-allowed", "The bootstrap route does not accept a request body.")],
            });
            return true;
        }
        if (request.method !== "GET") {
            rejectMethod(request, response, "GET");
            return true;
        }
        const startupRoot = workspace.getStartupRootGrant();
        response.setHeader("Cache-Control", "no-store");
        sendJson(request, response, 200, {
            ok: true,
            data: {
                ...(startupRoot === undefined ? {} : { rootId: startupRoot.rootId }),
                capabilities: startupRoot === undefined ? [] : startupWorkspaceCapabilities,
                stateToken: getApplicationServerSecurity(server).token,
            },
            diagnostics: [],
        });
        return true;
    }

    const readMatch = /^\/api\/documents\/([A-Za-z0-9_-]{32,})\/read$/.exec(pathname);
    if (readMatch !== null) {
        if (request.method !== "GET") {
            rejectMethod(request, response, "GET");
            return true;
        }
        try {
            const result = await workspace.readDocument(readMatch[1]          );
            sendJson(request, response, 200, {
                ok: true,
                data: {
                    documentId: result.documentId,
                    rootId: result.rootId,
                    path: result.logicalPath,
                    contentBase64: result.bytes.toString("base64"),
                    fingerprint: result.fingerprint,
                    externallyModified: result.externallyModified,
                },
                diagnostics: [],
            });
        } catch (error) {
            sendApiError(request, response, error);
        }
        return true;
    }

    const projectCatalogMatch = pathname === "/api/project";
    if (projectCatalogMatch) {
        if (advertisesRequestBody(request)) {
            response.setHeader("Connection", "close");
            sendJson(request, response, 413, {
                ok: false,
                diagnostics: [createDiagnostic("request-body-not-allowed", "The project catalog route does not accept a request body.")],
            });
            return true;
        }
        if (request.method !== "GET" && request.method !== "HEAD") {
            rejectMethod(request, response);
            return true;
        }
        if (projectDatService === undefined) {
            sendJson(request, response, 503, {
                ok: false,
                diagnostics: [createDiagnostic("internal-error", "The project service is not available.")],
            });
            return true;
        }
        try {
            response.setHeader("Cache-Control", "no-store");
            sendJson(request, response, 200, {
                ok: true,
                data: await projectDatService.catalog(),
                diagnostics: [],
            });
        } catch (error) {
            sendApiError(request, response, error);
        }
        return true;
    }

    if (pathname === "/api/project/skills" && (request.method === "GET" || request.method === "HEAD")) {
        if (advertisesRequestBody(request)) {
            response.setHeader("Connection", "close");
            sendJson(request, response, 413, {
                ok: false,
                diagnostics: [createDiagnostic("request-body-not-allowed", "The project skills route does not accept a request body for GET.")],
            });
            return true;
        }
        if (request.method !== "GET" && request.method !== "HEAD") {
            rejectMethod(request, response);
            return true;
        }
        if (projectSkillService === undefined) {
            sendJson(request, response, 503, {
                ok: false,
                diagnostics: [createDiagnostic("internal-error", "The project skill service is not available.")],
            });
            return true;
        }
        try {
            response.setHeader("Cache-Control", "no-store");
            sendJson(request, response, 200, {
                ok: true,
                data: await projectSkillService.get(),
                diagnostics: [],
            });
        } catch (error) {
            sendApiError(request, response, error);
        }
        return true;
    }

    const assetMatch = /^\/api\/assets\/([A-Za-z0-9_-]{32,})$/.exec(pathname);
    if (assetMatch !== null) {
        if (request.method !== "GET" && request.method !== "HEAD") {
            rejectMethod(request, response);
            return true;
        }
        const requestPath = new URL(request.url ?? "/", "http://127.0.0.1");
        if (requestPath.searchParams.size > 0) {
            sendJson(request, response, 400, {
                ok: false,
                diagnostics: [createDiagnostic("request-body-not-allowed", "The project asset route does not support query parameters.")],
            });
            return true;
        }
        if (projectDatService === undefined) {
            sendJson(request, response, 503, {
                ok: false,
                diagnostics: [createDiagnostic("internal-error", "The project service is not available.")],
            });
            return true;
        }
        try {
            const asset = await projectDatService.asset(assetMatch[1]          );
            response.setHeader("Cache-Control", "no-store");
            sendBody(request, response, 200, "image/bmp", asset.bytes);
        } catch (error) {
            sendApiError(request, response, error);
        }
        return true;
    }

    const stateRoute = pathname === "/api/workspace/grant"
        || pathname === "/api/documents/open"
        || pathname === "/api/project/open"
        || pathname === "/api/project/edit"
        || pathname === "/api/project/edit-batch"
        || pathname === "/api/project/edit-structure"
        || pathname === "/api/project/preview"
        || pathname === "/api/project/save"
        || pathname === "/api/project/close"
        || pathname === "/api/project/skills"
        || /^\/api\/documents\/[A-Za-z0-9_-]{32,}\/(?:save-as|overwrite-challenge|overwrite)$/.test(pathname);
    if (!stateRoute) {
        return false;
    }
    if (request.method !== "POST") {
        rejectMethod(request, response, "POST");
        return true;
    }
    if (!exactStateChangingOrigin(server, request)
        || !authorizeStateChangingRequest(server, request.headers[STATE_CHANGE_TOKEN_HEADER]                      )) {
        sendJson(request, response, 403, {
            ok: false,
            diagnostics: [createDiagnostic("forbidden-request", "State-changing requests require the exact active Origin, Host, and process token.")],
        });
        return true;
    }

    try {
        const body = await readJsonBody(request);
        if (pathname === "/api/workspace/grant") {
            requireExactKeys(body, ["absoluteRoot"]);
            const grant = await workspace.grantAbsoluteRoot(requireString(body, "absoluteRoot"));
            sendJson(request, response, 200, { ok: true, data: grant, diagnostics: [] });
            return true;
        }
        if (pathname === "/api/documents/open") {
            requireExactKeys(body, ["rootId", "path"]);
            const document = await workspace.openDocument(requireString(body, "rootId"), requireString(body, "path"));
            sendJson(request, response, 200, { ok: true, data: document, diagnostics: [] });
            return true;
        }
        if (pathname === "/api/project/open") {
            requireExactKeys(body, ["objectKey"]);
            if (projectDatService === undefined) {
                sendJson(request, response, 503, {
                    ok: false,
                    diagnostics: [createDiagnostic("internal-error", "The project service is not available.")],
                });
                return true;
            }
            const result = await projectDatService.open(requireString(body, "objectKey"));
            sendJson(request, response, 200, { ok: true, data: result, diagnostics: [] });
            return true;
        }
        if (pathname === "/api/project/edit") {
            if (projectDatService === undefined) {
                sendJson(request, response, 503, {
                    ok: false,
                    diagnostics: [createDiagnostic("internal-error", "The project service is not available.")],
                });
                return true;
            }
            const result = await projectDatService.edit(body);
            sendJson(request, response, 200, { ok: true, data: result, diagnostics: [] });
            return true;
        }
        if (pathname === "/api/project/edit-batch") {
            if (projectDatService === undefined) {
                sendJson(request, response, 503, {
                    ok: false,
                    diagnostics: [createDiagnostic("internal-error", "The project service is not available.")],
                });
                return true;
            }
            const result = await projectDatService.editBatch(body);
            sendJson(request, response, 200, { ok: true, data: result, diagnostics: [] });
            return true;
        }
        if (pathname === "/api/project/edit-structure") {
            if (projectDatService === undefined) {
                sendJson(request, response, 503, {
                    ok: false,
                    diagnostics: [createDiagnostic("internal-error", "The project service is not available.")],
                });
                return true;
            }
            const result = await projectDatService.editStructure(body);
            sendJson(request, response, 200, { ok: true, data: result, diagnostics: [] });
            return true;
        }
        if (pathname === "/api/project/preview") {
            if (projectDatService === undefined) {
                sendJson(request, response, 503, {
                    ok: false,
                    diagnostics: [createDiagnostic("internal-error", "The project service is not available.")],
                });
                return true;
            }
            const result = await projectDatService.preview(body);
            sendJson(request, response, 200, { ok: true, data: result, diagnostics: [] });
            return true;
        }
        if (pathname === "/api/project/save") {
            if (projectDatService === undefined) {
                sendJson(request, response, 503, {
                    ok: false,
                    diagnostics: [createDiagnostic("internal-error", "The project service is not available.")],
                });
                return true;
            }
            const result = await projectDatService.save(body);
            sendJson(request, response, 200, { ok: true, data: result, diagnostics: [] });
            return true;
        }
        if (pathname === "/api/project/close") {
            requireExactKeys(body, ["sessionId"]);
            if (projectDatService === undefined) {
                sendJson(request, response, 503, {
                    ok: false,
                    diagnostics: [createDiagnostic("internal-error", "The project service is not available.")],
                });
                return true;
            }
            const result = await projectDatService.close(body);
            sendJson(request, response, 200, { ok: true, data: result, diagnostics: [] });
            return true;
        }
        if (pathname === "/api/project/skills") {
            if (projectSkillService === undefined) {
                sendJson(request, response, 503, {
                    ok: false,
                    diagnostics: [createDiagnostic("internal-error", "The project skill service is not available.")],
                });
                return true;
            }
            const result = await projectSkillService.save(body);
            sendJson(request, response, 200, { ok: true, data: result, diagnostics: [] });
            return true;
        }

        const match = /^\/api\/documents\/([A-Za-z0-9_-]{32,})\/(save-as|overwrite-challenge|overwrite)$/.exec(pathname);
        if (match === null) {
            return false;
        }
        const documentId = match[1]          ;
        const operation = match[2]          ;
        if (operation === "save-as") {
            requireExactKeys(body, ["rootId", "path", "contentBase64"]);
            const result = await safeSave.saveAs(
                documentId,
                requireString(body, "rootId"),
                requireString(body, "path"),
                decodeBase64Content(requireString(body, "contentBase64")),
            );
            sendJson(request, response, 201, { ok: true, data: result, diagnostics: [] });
            return true;
        }
        if (operation === "overwrite-challenge") {
            requireExactKeys(body, ["rootId", "path", "contentBase64"]);
            const challenge = await safeSave.issueOverwriteChallenge(
                documentId,
                requireString(body, "rootId"),
                requireString(body, "path"),
                decodeBase64Content(requireString(body, "contentBase64")),
            );
            sendJson(request, response, 200, { ok: true, data: challenge, diagnostics: [] });
            return true;
        }
        requireExactKeys(body, ["challengeId", "contentBase64"]);
        const result = await safeSave.overwrite(
            documentId,
            requireString(body, "challengeId"),
            decodeBase64Content(requireString(body, "contentBase64")),
        );
        sendJson(request, response, 200, {
            ok: true,
            data: {
                status: result.status,
                document: result.document,
                recovery: {
                    target: {
                        name: basename(result.recovery.target.path),
                        exists: result.recovery.target.exists,
                        size: result.recovery.target.size,
                        sha256: result.recovery.target.sha256,
                    },
                    replacement: {
                        name: basename(result.recovery.replacement.path),
                        exists: result.recovery.replacement.exists,
                        size: result.recovery.replacement.size,
                        sha256: result.recovery.replacement.sha256,
                    },
                    backup: {
                        name: basename(result.recovery.backup.path),
                        exists: result.recovery.backup.exists,
                        size: result.recovery.backup.size,
                        sha256: result.recovery.backup.sha256,
                    },
                },
            },
            diagnostics: [],
        });
        return true;
    } catch (error) {
        sendApiError(request, response, error);
        return true;
    }
}

export function createApplicationServer(options                          )         {
    const workspace = options.workspace instanceof WorkspaceRegistry
        ? options.workspace
        : new WorkspaceRegistry(options.workspace);
    const safeSave = options.safeSave ?? new SafeSaveService(workspace, options.safeSaveOptions);
    let staticConfigurationPromise = options.staticConfiguration;
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
            if (await handleApiRequest(
                server,
                request,
                response,
                pathname,
                workspace,
                safeSave,
                options.projectDatService,
                options.projectSkillService,
            )) {
                return;
            }

            if (pathname.startsWith("/api/")) {
                sendJson(request, response, 404, {
                    ok: false,
                    diagnostics: [createDiagnostic("not-found", "The requested API route was not found.")],
                });
                return;
            }

            if (advertisesRequestBody(request)) {
                response.setHeader("Connection", "close");
                sendJson(request, response, 413, {
                    ok: false,
                    diagnostics: [createDiagnostic("request-body-not-allowed", "Static routes do not accept request bodies.")],
                });
                return;
            }
            const method = request.method ?? "GET";
            if (method !== "GET" && method !== "HEAD") {
                rejectMethod(request, response);
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
    applicationServerWorkspaces.set(server, workspace);
    return server;
}

                                        
                                        
                                                   
                                                                           
                                              
 

const fetchBlockedPorts = new Set([
    1, 7, 9, 11, 13, 15, 17, 19, 20, 21, 22, 23, 25, 37, 42, 43, 53, 69,
    77, 79, 87, 95, 101, 102, 103, 104, 109, 110, 111, 113, 115, 117, 119,
    123, 135, 137, 139, 143, 161, 179, 389, 427, 465, 512, 513, 514, 515,
    526, 530, 531, 532, 540, 548, 554, 556, 563, 587, 601, 636, 989, 990,
    993, 995, 1719, 1720, 1723, 2049, 3659, 4045, 4190, 6000, 6566, 6665,
    6666, 6667, 6668, 6669, 6697, 10080,
]);

function isFetchBlockedPort(port        )          {
    return fetchBlockedPorts.has(port);
}

async function listenOnLoopback(server        , port        )                              {
    await new Promise      ((resolveListen, rejectListen) => {
        const onError = (error       )       => rejectListen(error);
        server.once("error", onError);
        server.listen(port, LOOPBACK_HOST, () => {
            server.off("error", onError);
            resolveListen();
        });
    });

    return server.address()                      ;
}

async function closeServer(server        )                {
    await new Promise      ((resolveClose, rejectClose) => {
        server.close((error) => error === undefined ? resolveClose() : rejectClose(error));
    });
}

export async function listenLoopback(
    server        ,
    port        ,
    options                        = {},
)                  {
    const maxAttempts = options.maxEphemeralListenAttempts ?? MAX_EPHEMERAL_LOOPBACK_LISTEN_ATTEMPTS;
    if (!Number.isInteger(maxAttempts) || maxAttempts < 1) {
        throw new RangeError("maxEphemeralListenAttempts must be a positive integer.");
    }
    const listen = options.listen ?? listenOnLoopback;
    const close = options.close ?? closeServer;
    const isBlockedPort = options.isFetchBlockedPort ?? isFetchBlockedPort;
    const attempts = port === 0 ? maxAttempts : 1;

    for (let attempt = 1; attempt <= attempts; attempt += 1) {
        if (attempt === 1) applicationServerWorkspaces.get(server)?.sealStartupAuthorization();
        const address = await listen(server, port);
        if (address === null || address.address !== LOOPBACK_HOST) {
            await close(server);
            throw new Error("Server did not bind to the required loopback address.");
        }
        if (!isBlockedPort(address.port)) {
            return `http://${LOOPBACK_HOST}:${address.port}`;
        }

        await close(server);
        if (port !== 0) {
            throw new Error(`Configured port ${address.port} is blocked by browser Fetch.`);
        }
    }

    throw new Error(`Unable to allocate a browser-safe loopback port after ${attempts} attempts.`);
}
