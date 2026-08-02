// dat-skill-flow-build:20260801031421576-28f00178e2ed430f87dfd56fa4b88f64
import { resolve } from "node:path";

import { createApplicationServer, listenLoopback } from "./server.js";

function argumentValue(name        )                     {
    const index = process.argv.indexOf(name);
    return index >= 0 ? process.argv[index + 1] : undefined;
}

const staticRoot = resolve(argumentValue("--root") ?? "dist");
const manifestPath = resolve(argumentValue("--manifest") ?? "dist/build-manifest.json");
const rawPort = argumentValue("--port") ?? process.env.PORT ?? "4173";
const port = Number.parseInt(rawPort, 10);
if (!Number.isInteger(port) || port < 0 || port > 65_535) {
    throw new Error(`Invalid port: ${rawPort}`);
}

const server = createApplicationServer({ staticRoot, manifestPath });
const origin = await listenLoopback(server, port);
process.stdout.write(`Dat Skill Flow server listening at ${origin}\n`);

function shutdown()       {
    server.close((error) => {
        process.exitCode = error === undefined ? 0 : 1;
    });
}

process.once("SIGINT", shutdown);
process.once("SIGTERM", shutdown);
