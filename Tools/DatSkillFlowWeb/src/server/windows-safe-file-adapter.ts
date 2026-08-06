import { createHash } from "node:crypto";
import { spawn as nodeSpawn, type ChildProcessWithoutNullStreams } from "node:child_process";
import { open } from "node:fs/promises";
import { dirname, join, relative, resolve, sep } from "node:path";
import { fileURLToPath } from "node:url";

import { loadVerifiedRuntimeAsset } from "../../scripts/manifest-integrity.mjs";
import type { FileFingerprint } from "./workspace-registry.js";

const MAX_HELPER_BYTES = 1024 * 1024;
const MAX_PROTOCOL_OUTPUT_BYTES = 512 * 1024;
const MAX_READ_PROTOCOL_OUTPUT_BYTES = 24 * 1024 * 1024;
const sha256Pattern = /^[a-f0-9]{64}$/;

export interface NativeRootDescriptor {
    canonicalPath: string;
    volumeSerial: string;
    fileId: string;
}

export interface NativePathObservation {
    path: string;
    exists: boolean;
    size?: number;
    sha256?: string;
    inspectionError?: string;
}

export interface NativeRecoveryReport {
    target: NativePathObservation;
    replacement?: NativePathObservation;
    backup?: NativePathObservation;
}

export interface NativeReadResult {
    canonicalPath: string;
    bytes: Buffer;
    fingerprint: FileFingerprint;
}

export interface NativeWriteResult {
    canonicalPath: string;
    fingerprint: FileFingerprint;
    recovery: NativeRecoveryReport;
}

export interface NativeInspectRootRequest {
    absoluteRoot: string;
}

export interface NativeEnsureDirectoryRequest {
    root: NativeRootDescriptor;
    logicalPath: string;
}

export interface NativeReadRequest {
    root: NativeRootDescriptor;
    logicalPath: string;
    maximumBytes: number;
}

export interface NativeSaveAsRequest extends NativeReadRequest {
    bytes: Uint8Array;
}

export interface NativeOverwriteRequest extends NativeSaveAsRequest {
    expectedFingerprint: FileFingerprint;
    replacementName: string;
    backupName: string;
}

export interface NativeSafeFileClient {
    inspectRoot(request: NativeInspectRootRequest): Promise<NativeRootDescriptor>;
    ensureDirectory(request: NativeEnsureDirectoryRequest): Promise<{ canonicalPath: string }>;
    read(request: NativeReadRequest): Promise<NativeReadResult>;
    saveAs(request: NativeSaveAsRequest): Promise<NativeWriteResult>;
    overwrite(request: NativeOverwriteRequest): Promise<NativeWriteResult>;
}

export interface NativeBarrierEvent {
    name: "after-directory-handles" | "before-publish";
    targetPath: string;
    replacementPath: string;
    backupPath: string;
}

export interface NativeSafeFileHooks {
    onBarrier?(event: NativeBarrierEvent): Promise<void>;
}

export interface VerifiedNativeRuntimeAssetDescriptor {
    path: string;
    manifestPath: string;
    buildId: string;
    size: number;
    sha256: string;
}

export interface PowerShellWindowsSafeFileClientOptions {
    scriptPath?: string;
    runtimeAsset?: VerifiedNativeRuntimeAssetDescriptor;
    executable?: string;
    hooks?: NativeSafeFileHooks;
    spawn?: typeof nodeSpawn;
}

export class NativeSafeFileError extends Error {
    readonly code: string;
    readonly win32Code?: number;
    readonly recovery?: NativeRecoveryReport;

    constructor(
        code: string,
        message: string,
        options: ErrorOptions & { win32Code?: number; recovery?: NativeRecoveryReport } = {},
    ) {
        super(message, options);
        this.name = "NativeSafeFileError";
        this.code = code;
        this.win32Code = options.win32Code;
        this.recovery = options.recovery;
    }
}

interface NativeProtocolRequest {
    operation: "inspectRoot" | "ensureDirectory" | "read" | "saveAs" | "overwrite";
    absoluteRoot?: string;
    root?: NativeRootDescriptor;
    logicalPath?: string;
    maximumBytes?: number;
    expectedFingerprint?: FileFingerprint;
    replacementName?: string;
    backupName?: string;
    contentLength: number;
    barriers: readonly NativeBarrierEvent["name"][];
}

interface RawProtocolResult {
    type: "result";
    ok: boolean;
    code?: string;
    message?: string;
    win32Code?: number;
    root?: unknown;
    canonicalPath?: unknown;
    bytesBase64?: unknown;
    fingerprint?: unknown;
    recovery?: unknown;
}

const bootstrap = String.raw`
$ErrorActionPreference = 'Stop'
$protocolStream = [Console]::OpenStandardInput()
function Read-BootstrapExact([System.IO.Stream] $Stream, [byte[]] $Buffer) {
    $offset = 0
    while ($offset -lt $Buffer.Length) {
        $count = $Stream.Read($Buffer, $offset, $Buffer.Length - $offset)
        if ($count -le 0) { throw 'Unexpected EOF while loading the verified helper.' }
        $offset += $count
    }
}
$lengthBytes = New-Object byte[] 4
Read-BootstrapExact $protocolStream $lengthBytes
$sourceLength = [BitConverter]::ToInt32($lengthBytes, 0)
if ($sourceLength -lt 1 -or $sourceLength -gt 1048576) { throw 'Invalid verified helper length.' }
$sourceBytes = New-Object byte[] $sourceLength
Read-BootstrapExact $protocolStream $sourceBytes
$source = [Text.Encoding]::UTF8.GetString($sourceBytes)
& ([ScriptBlock]::Create($source)) $protocolStream
`;

function contentHash(bytes: Uint8Array): string {
    return createHash("sha256").update(bytes).digest("hex");
}

function assertRecord(value: unknown, label: string): Record<string, unknown> {
    if (value === null || typeof value !== "object" || Array.isArray(value)) {
        throw new Error(`${label} must be an object.`);
    }
    return value as Record<string, unknown>;
}

function assertString(value: unknown, label: string): string {
    if (typeof value !== "string" || value.length === 0 || value.includes("\0")) {
        throw new Error(`${label} must be a nonempty string.`);
    }
    return value;
}

function parseFingerprint(value: unknown): FileFingerprint {
    const record = assertRecord(value, "fingerprint");
    const sha256 = assertString(record.sha256, "fingerprint.sha256");
    const size = record.size;
    if (!sha256Pattern.test(sha256) || !Number.isSafeInteger(size) || (size as number) < 0) {
        throw new Error("Native helper returned an invalid fingerprint.");
    }
    return Object.freeze({
        sha256,
        size: size as number,
        modifiedNanoseconds: assertString(record.modifiedNanoseconds, "fingerprint.modifiedNanoseconds"),
        changedNanoseconds: assertString(record.changedNanoseconds, "fingerprint.changedNanoseconds"),
        device: assertString(record.device, "fingerprint.device"),
        inode: assertString(record.inode, "fingerprint.inode"),
    });
}

function parseObservation(value: unknown): NativePathObservation {
    const record = assertRecord(value, "path observation");
    const path = assertString(record.path, "path observation.path");
    if (typeof record.exists !== "boolean") {
        throw new Error("Native helper returned an invalid path existence value.");
    }
    const size = record.size;
    if (size !== undefined && (!Number.isSafeInteger(size) || (size as number) < 0)) {
        throw new Error("Native helper returned an invalid observed size.");
    }
    const sha256 = record.sha256;
    if (sha256 !== undefined && (typeof sha256 !== "string" || !sha256Pattern.test(sha256))) {
        throw new Error("Native helper returned an invalid observed digest.");
    }
    return {
        path,
        exists: record.exists,
        ...(size === undefined ? {} : { size: size as number }),
        ...(sha256 === undefined ? {} : { sha256 }),
        ...(typeof record.inspectionError === "string" ? { inspectionError: record.inspectionError } : {}),
    };
}

function parseRecovery(value: unknown): NativeRecoveryReport {
    const record = assertRecord(value, "recovery");
    return {
        target: parseObservation(record.target),
        ...(record.replacement === undefined ? {} : { replacement: parseObservation(record.replacement) }),
        ...(record.backup === undefined ? {} : { backup: parseObservation(record.backup) }),
    };
}

function parseRoot(value: unknown): NativeRootDescriptor {
    const record = assertRecord(value, "root descriptor");
    return Object.freeze({
        canonicalPath: assertString(record.canonicalPath, "root.canonicalPath"),
        volumeSerial: assertString(record.volumeSerial, "root.volumeSerial"),
        fileId: assertString(record.fileId, "root.fileId"),
    });
}

function uint32le(value: number): Buffer {
    const bytes = Buffer.allocUnsafe(4);
    bytes.writeUInt32LE(value, 0);
    return bytes;
}

async function readExactHelperBytes(filePath: string, expected?: { size: number; sha256: string }): Promise<Buffer> {
    const handle = await open(filePath, "r");
    try {
        const metadata = await handle.stat();
        if (!metadata.isFile() || metadata.size < 1 || metadata.size > MAX_HELPER_BYTES) {
            throw new Error("Windows safe-file helper is missing or exceeds its size limit.");
        }
        const bytes = await handle.readFile();
        if (bytes.length !== metadata.size) {
            throw new Error("Windows safe-file helper changed while it was loaded.");
        }
        if (expected !== undefined
            && (bytes.length !== expected.size || contentHash(bytes) !== expected.sha256)) {
            throw new Error("Windows safe-file helper does not match its pinned runtime digest.");
        }
        return bytes;
    } finally {
        await handle.close();
    }
}

function validateRuntimeAsset(asset: VerifiedNativeRuntimeAssetDescriptor): void {
    if (!Number.isSafeInteger(asset.size)
        || asset.size < 1
        || asset.size > MAX_HELPER_BYTES
        || !sha256Pattern.test(asset.sha256)
        || typeof asset.buildId !== "string"
        || asset.buildId.length === 0) {
        throw new TypeError("Invalid verified Windows safe-file runtime descriptor.");
    }
    const expectedRuntimeDirectory = join(dirname(asset.manifestPath), "runtime").toLowerCase();
    if (dirname(resolve(asset.path)).toLowerCase() !== resolve(expectedRuntimeDirectory).toLowerCase()) {
        throw new TypeError("Windows safe-file runtime asset is outside its pinned build runtime directory.");
    }
}

export class PowerShellWindowsSafeFileClient implements NativeSafeFileClient {
    readonly #executable: string;
    readonly #hooks: NativeSafeFileHooks;
    readonly #spawn: typeof nodeSpawn;
    readonly #helperBytes: Promise<Buffer>;

    constructor(options: PowerShellWindowsSafeFileClientOptions = {}) {
        if (options.scriptPath !== undefined && options.runtimeAsset !== undefined) {
            throw new TypeError("Specify either scriptPath or runtimeAsset, not both.");
        }
        this.#executable = options.executable ?? "powershell.exe";
        this.#hooks = options.hooks ?? {};
        this.#spawn = options.spawn ?? nodeSpawn;
        if (options.runtimeAsset !== undefined) {
            validateRuntimeAsset(options.runtimeAsset);
            this.#helperBytes = readExactHelperBytes(resolve(options.runtimeAsset.path), options.runtimeAsset);
        } else if (options.scriptPath !== undefined) {
            this.#helperBytes = readExactHelperBytes(resolve(options.scriptPath));
        } else {
            const defaultPath = fileURLToPath(new URL("../../runtime/windows-safe-file.ps1", import.meta.url));
            const staticRoot = fileURLToPath(new URL("../../../../", import.meta.url));
            this.#helperBytes = loadVerifiedRuntimeAsset({
                staticRoot,
                manifestPath: resolve(dirname(dirname(defaultPath)), "build-manifest.json"),
                outputPath: relative(staticRoot, defaultPath).split(sep).join("/"),
            }).then(async (asset) => {
                const entry = asset.manifest.runtimeAssets.find((candidate: { path: string }) => (
                    candidate.path.endsWith("/runtime/windows-safe-file.ps1")
                ));
                if (entry === undefined) {
                    throw new Error("Pinned Windows safe-file runtime asset is missing.");
                }
                return await readExactHelperBytes(asset.path, entry);
            });
        }
    }

    async inspectRoot(request: NativeInspectRootRequest): Promise<NativeRootDescriptor> {
        const result = await this.#invoke({
            operation: "inspectRoot",
            absoluteRoot: request.absoluteRoot,
            contentLength: 0,
            barriers: [],
        }, Buffer.alloc(0));
        return parseRoot(result.root);
    }

    async ensureDirectory(request: NativeEnsureDirectoryRequest): Promise<{ canonicalPath: string }> {
        const result = await this.#invoke({
            operation: "ensureDirectory",
            root: request.root,
            logicalPath: request.logicalPath,
            contentLength: 0,
            barriers: [],
        }, Buffer.alloc(0));
        return { canonicalPath: assertString(result.canonicalPath, "ensureDirectory.canonicalPath") };
    }

    async read(request: NativeReadRequest): Promise<NativeReadResult> {
        const result = await this.#invoke({
            operation: "read",
            root: request.root,
            logicalPath: request.logicalPath,
            maximumBytes: request.maximumBytes,
            contentLength: 0,
            barriers: this.#hooks.onBarrier === undefined ? [] : ["after-directory-handles"],
        }, Buffer.alloc(0));
        const bytesBase64 = assertString(result.bytesBase64, "read.bytesBase64");
        const bytes = Buffer.from(bytesBase64, "base64");
        const fingerprint = parseFingerprint(result.fingerprint);
        if (bytes.length !== fingerprint.size || contentHash(bytes) !== fingerprint.sha256) {
            throw new Error("Native helper returned read bytes inconsistent with its fingerprint.");
        }
        return {
            canonicalPath: assertString(result.canonicalPath, "read.canonicalPath"),
            bytes,
            fingerprint,
        };
    }

    async saveAs(request: NativeSaveAsRequest): Promise<NativeWriteResult> {
        return await this.#write("saveAs", request);
    }

    async overwrite(request: NativeOverwriteRequest): Promise<NativeWriteResult> {
        return await this.#write("overwrite", request);
    }

    async #write(
        operation: "saveAs" | "overwrite",
        request: NativeSaveAsRequest | NativeOverwriteRequest,
    ): Promise<NativeWriteResult> {
        const bytes = Buffer.from(request.bytes);
        const overwrite = operation === "overwrite" ? request as NativeOverwriteRequest : undefined;
        const result = await this.#invoke({
            operation,
            root: request.root,
            logicalPath: request.logicalPath,
            maximumBytes: request.maximumBytes,
            contentLength: bytes.length,
            ...(overwrite === undefined ? {} : {
                expectedFingerprint: overwrite.expectedFingerprint,
                replacementName: overwrite.replacementName,
                backupName: overwrite.backupName,
            }),
            barriers: this.#hooks.onBarrier === undefined
                ? []
                : operation === "overwrite"
                    ? ["after-directory-handles", "before-publish"]
                    : ["after-directory-handles"],
        }, bytes);
        return {
            canonicalPath: assertString(result.canonicalPath, `${operation}.canonicalPath`),
            fingerprint: parseFingerprint(result.fingerprint),
            recovery: parseRecovery(result.recovery),
        };
    }

    async #invoke(request: NativeProtocolRequest, content: Buffer): Promise<RawProtocolResult> {
        if (process.platform !== "win32") {
            throw new NativeSafeFileError("unsupported-platform", "Handle-safe workspace I/O is available only on Windows.");
        }
        const helperBytes = await this.#helperBytes;
        const requestBytes = Buffer.from(JSON.stringify(request), "utf8");
        const encodedBootstrap = Buffer.from(bootstrap, "utf16le").toString("base64");
        const child = this.#spawn(this.#executable, [
            "-NoProfile",
            "-NonInteractive",
            "-ExecutionPolicy",
            "Bypass",
            "-EncodedCommand",
            encodedBootstrap,
        ], {
            shell: false,
            windowsHide: true,
            stdio: ["pipe", "pipe", "pipe"],
        }) as ChildProcessWithoutNullStreams;
        const firstPayload = Buffer.concat([
            uint32le(helperBytes.length),
            helperBytes,
            uint32le(requestBytes.length),
            requestBytes,
            content,
        ]);
        let protocolFailure: unknown;
        child.stdin.on("error", (error) => {
            protocolFailure ??= error;
        });
        child.stdin.write(firstPayload);

        const outputLimit = request.operation === "read"
            ? Math.min(
                MAX_READ_PROTOCOL_OUTPUT_BYTES,
                Math.ceil((request.maximumBytes ?? 0) * 4 / 3) + MAX_PROTOCOL_OUTPUT_BYTES,
            )
            : MAX_PROTOCOL_OUTPUT_BYTES;
        let outputBytes = 0;
        let stdoutBuffer = "";
        let stderr = "";
        let protocolResult: RawProtocolResult | undefined;
        let eventTail = Promise.resolve();
        const acceptLine = (line: string): void => {
            if (line.trim().length === 0) {
                return;
            }
            let value: Record<string, unknown>;
            try {
                value = assertRecord(JSON.parse(line), "native protocol line");
            } catch (error) {
                protocolFailure ??= error;
                return;
            }
            if (value.type === "barrier") {
                eventTail = eventTail.then(async () => {
                    const name = value.name;
                    if (name !== "after-directory-handles" && name !== "before-publish") {
                        throw new Error("Native helper requested an unknown barrier.");
                    }
                    await this.#hooks.onBarrier?.({
                        name,
                        targetPath: assertString(value.targetPath, "barrier.targetPath"),
                        replacementPath: assertString(value.replacementPath, "barrier.replacementPath"),
                        backupPath: assertString(value.backupPath, "barrier.backupPath"),
                    });
                    child.stdin.write(Buffer.from([1]));
                }).catch((error: unknown) => {
                    protocolFailure ??= error;
                    child.kill();
                });
                return;
            }
            if (value.type === "result" && protocolResult === undefined) {
                protocolResult = value as unknown as RawProtocolResult;
                return;
            }
            protocolFailure ??= new Error("Native helper returned an unexpected protocol record.");
        };
        child.stdout.setEncoding("utf8");
        child.stdout.on("data", (chunk: string) => {
            outputBytes += Buffer.byteLength(chunk);
            if (outputBytes > outputLimit) {
                protocolFailure ??= new Error("Native helper exceeded its output limit.");
                child.kill();
                return;
            }
            stdoutBuffer += chunk;
            while (true) {
                const index = stdoutBuffer.indexOf("\n");
                if (index < 0) {
                    break;
                }
                acceptLine(stdoutBuffer.slice(0, index).replace(/\r$/, ""));
                stdoutBuffer = stdoutBuffer.slice(index + 1);
            }
        });
        child.stderr.setEncoding("utf8");
        child.stderr.on("data", (chunk: string) => {
            if (stderr.length < MAX_PROTOCOL_OUTPUT_BYTES) {
                stderr += chunk;
            }
        });
        const exit = await new Promise<{ code: number | null; signal: NodeJS.Signals | null }>((resolveExit, rejectExit) => {
            child.once("error", rejectExit);
            child.once("close", (code, signal) => resolveExit({ code, signal }));
        });
        await eventTail;
        if (stdoutBuffer.trim().length > 0) {
            acceptLine(stdoutBuffer.trim());
        }
        if (protocolFailure !== undefined) {
            throw new Error("Windows safe-file helper protocol failed.", { cause: protocolFailure });
        }
        if (protocolResult === undefined) {
            throw new Error(`Windows safe-file helper returned no result${stderr.trim().length === 0 ? "." : `: ${stderr.trim()}`}`);
        }
        if (typeof protocolResult.ok !== "boolean") {
            throw new Error("Windows safe-file helper returned an invalid result status.");
        }
        if (!protocolResult.ok) {
            if (protocolResult.win32Code !== undefined
                && (!Number.isSafeInteger(protocolResult.win32Code) || protocolResult.win32Code < 0)) {
                throw new Error("Windows safe-file helper returned an invalid Win32 error code.");
            }
            throw new NativeSafeFileError(
                assertString(protocolResult.code, "failure.code"),
                assertString(protocolResult.message, "failure.message"),
                {
                    ...(typeof protocolResult.win32Code === "number" ? { win32Code: protocolResult.win32Code } : {}),
                    ...(protocolResult.recovery === undefined ? {} : { recovery: parseRecovery(protocolResult.recovery) }),
                },
            );
        }
        if (exit.code !== 0 || exit.signal !== null) {
            throw new Error("Windows safe-file helper reported success with a failing process status.");
        }
        return protocolResult;
    }
}
