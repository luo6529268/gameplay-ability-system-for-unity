import {
    expectArray,
    expectEnum,
    expectLiteral,
    expectRecord,
    expectStrictKeys,
    expectString,
    fail,
    validator,
} from "../validation/strict.js";

export const authorityStatuses = [
    "authoritative",
    "provisional",
    "unsupported",
    "unimplemented",
] as const;

export type AuthorityStatus = typeof authorityStatuses[number];

export interface AuthoritySource {
    file: string;
    function: string;
    region: string;
}

export interface AuthorityLedgerEntry {
    id: string;
    summary: string;
    status: AuthorityStatus;
    source?: AuthoritySource;
    note?: string;
}

export interface AuthorityLedger {
    schemaVersion: 1;
    entries: AuthorityLedgerEntry[];
}

const authorityStatusSet = new Set<AuthorityStatus>(authorityStatuses);
const sourceKeys = new Set(["file", "function", "region"]);
const entryKeys = new Set(["id", "summary", "status", "source", "note"]);
const ledgerKeys = new Set(["schemaVersion", "entries"]);
const authorityIdPattern = /^[a-z][a-z0-9]*(?:[.-][a-z0-9]+)*$/;

function parseAuthoritySource(value: unknown, path: ReadonlyArray<string | number>): AuthoritySource {
    const record = expectRecord(value, path);
    expectStrictKeys(record, sourceKeys, path);
    return {
        file: expectString(record.file, [...path, "file"], 1),
        function: expectString(record.function, [...path, "function"], 1),
        region: expectString(record.region, [...path, "region"], 1),
    };
}

function parseAuthorityEntry(value: unknown, path: ReadonlyArray<string | number>): AuthorityLedgerEntry {
    const record = expectRecord(value, path);
    expectStrictKeys(record, entryKeys, path);
    const id = expectString(record.id, [...path, "id"], 1);
    if (!authorityIdPattern.test(id)) {
        fail([...path, "id"], "invalid authority rule id");
    }
    const status = expectEnum(record.status, authorityStatusSet, [...path, "status"]);
    const source = record.source === undefined
        ? undefined
        : parseAuthoritySource(record.source, [...path, "source"]);
    if (status === "authoritative" && source === undefined) {
        fail([...path, "source"], "authoritative behavior requires a concrete C++ source citation");
    }
    return {
        id,
        summary: expectString(record.summary, [...path, "summary"], 1),
        status,
        ...(source === undefined ? {} : { source }),
        ...(record.note === undefined ? {} : { note: expectString(record.note, [...path, "note"], 1) }),
    };
}

export const authorityStatusSchema = validator<AuthorityStatus>((value) => (
    expectEnum(value, authorityStatusSet, ["status"])
));

export const authoritySourceSchema = validator<AuthoritySource>((value) => parseAuthoritySource(value, ["source"]));

export const authorityLedgerEntrySchema = validator<AuthorityLedgerEntry>((value) => parseAuthorityEntry(value, ["entry"]));

export const authorityLedgerSchema = validator<AuthorityLedger>((value) => {
    const record = expectRecord(value, []);
    expectStrictKeys(record, ledgerKeys, []);
    expectLiteral(record.schemaVersion, 1, ["schemaVersion"]);
    const entries = expectArray(record.entries, ["entries"]).map((entry, index) => (
        parseAuthorityEntry(entry, ["entries", index])
    ));
    const ids = new Set<string>();
    for (const [index, entry] of entries.entries()) {
        if (ids.has(entry.id)) {
            fail(["entries", index, "id"], `duplicate authority rule id: ${entry.id}`);
        }
        ids.add(entry.id);
    }
    return { schemaVersion: 1, entries };
});

export function createEmptyAuthorityLedger(): AuthorityLedger {
    return authorityLedgerSchema.parse({
        schemaVersion: 1,
        entries: [],
    });
}
