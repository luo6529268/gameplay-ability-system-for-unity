import { execFile } from "node:child_process";
import { createServer } from "node:http";
import { readFile, stat } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const labRoot = path.dirname(fileURLToPath(import.meta.url));
const defaultPort = 41731;
const openBrowser = process.argv.includes("--open");
const portArgumentIndex = process.argv.indexOf("--port");
const hasExplicitPort = portArgumentIndex >= 0;
const requestedPort = portArgumentIndex >= 0 ? Number(process.argv[portArgumentIndex + 1]) : defaultPort;
const port = Number.isInteger(requestedPort) && requestedPort > 0 && requestedPort < 65536
    ? requestedPort
    : defaultPort;

const contentTypes = new Map([
    [".html", "text/html; charset=utf-8"],
    [".js", "text/javascript; charset=utf-8"],
    [".mjs", "text/javascript; charset=utf-8"],
    [".png", "image/png"],
    [".css", "text/css; charset=utf-8"]
]);

function resolveLabFile(requestUrl) {
    const url = new URL(requestUrl, "http://127.0.0.1");
    const pathname = url.pathname === "/" ? "/index.html" : decodeURIComponent(url.pathname);
    const filePath = path.resolve(labRoot, `.${pathname}`);
    const relativePath = path.relative(labRoot, filePath);
    if (relativePath.startsWith("..") || path.isAbsolute(relativePath)) return undefined;
    return filePath;
}

const server = createServer(async (request, response) => {
    if (request.method !== "GET" && request.method !== "HEAD") {
        response.writeHead(405, { Allow: "GET, HEAD" });
        response.end();
        return;
    }

    const filePath = resolveLabFile(request.url ?? "/");
    if (!filePath) {
        response.writeHead(403, { "Content-Type": "text/plain; charset=utf-8" });
        response.end("Forbidden");
        return;
    }

    try {
        const details = await stat(filePath);
        if (!details.isFile()) throw new Error("Not a file");
        const headers = {
            "Cache-Control": "no-store",
            "Content-Type": contentTypes.get(path.extname(filePath).toLowerCase()) ?? "application/octet-stream"
        };
        response.writeHead(200, headers);
        if (request.method === "HEAD") {
            response.end();
            return;
        }
        response.end(await readFile(filePath));
    } catch {
        response.writeHead(404, { "Content-Type": "text/plain; charset=utf-8" });
        response.end("Not found");
    }
});

function reportStartedServer() {
    const address = server.address();
    const actualPort = typeof address === "object" && address !== null ? address.port : port;
    const url = `http://127.0.0.1:${actualPort}/`;
    console.log(`Sprite Cadence Lab is running at ${url}`);
    console.log("Keep this window open while using the lab. Press Ctrl+C to stop it.");

    if (openBrowser && process.platform === "win32") {
        execFile("cmd.exe", ["/c", "start", "", url], () => {});
    }
}

server.once("error", (error) => {
    if (error.code === "EADDRINUSE" && !hasExplicitPort) {
        console.warn(`Port ${port} is already in use; selecting a free local port instead.`);
        server.listen(0, "127.0.0.1", reportStartedServer);
        return;
    }
    console.error(`Sprite Cadence Lab server failed: ${error.message}`);
    process.exitCode = 1;
});

server.listen(port, "127.0.0.1", reportStartedServer);
