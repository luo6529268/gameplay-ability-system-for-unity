// dat-skill-flow-build:20260801035004200-595a526ce90146be87ba0827ec56c335
import { diagnosticEnvelopeSchema } from "../diagnostics/envelope.js";
                                                                     
import {
    expectArray,
    expectEnum,
    expectFiniteNumber,
    expectLiteral,
    expectNonnegativeInteger,
    expectRecord,
    expectStrictKeys,
    expectString,
    fail,
    validator,
} from "../validation/strict.js";

                                                                                                      
                                                                        

                                
                     
                     
                     
                            
                        
                      
                                       
                                      
 

const traceCategories = new Set               (["system", "parser", "save", "simulation"]);
const traceKeys = new Set([
    "schemaVersion",
    "streamId",
    "sequence",
    "category",
    "tick",
    "ruleIds",
    "payload",
    "diagnostics",
]);

function parseJsonValue(value         , path                                )            {
    if (value === null || typeof value === "string" || typeof value === "boolean") {
        return value;
    }
    if (typeof value === "number") {
        return expectFiniteNumber(value, path);
    }
    if (Array.isArray(value)) {
        return value.map((entry, index) => parseJsonValue(entry, [...path, index]));
    }
    if (typeof value === "object") {
        const record = expectRecord(value, path);
        return Object.fromEntries(Object.entries(record).map(([key, entry]) => [
            key,
            parseJsonValue(entry, [...path, key]),
        ]));
    }
    return fail(path, "expected a JSON value");
}

export const traceEnvelopeSchema = validator               ((value) => {
    const record = expectRecord(value, []);
    expectStrictKeys(record, traceKeys, []);
    const tick = record.tick === null
        ? null
        : expectNonnegativeInteger(record.tick, ["tick"]);
    const payloadRecord = expectRecord(record.payload, ["payload"]);
    const payload = Object.fromEntries(Object.entries(payloadRecord).map(([key, entry]) => [
        key,
        parseJsonValue(entry, ["payload", key]),
    ]));
    return {
        schemaVersion: expectLiteral(record.schemaVersion, 1, ["schemaVersion"]),
        streamId: expectString(record.streamId, ["streamId"], 1),
        sequence: expectNonnegativeInteger(record.sequence, ["sequence"]),
        category: expectEnum(record.category, traceCategories, ["category"]),
        tick,
        ruleIds: expectArray(record.ruleIds, ["ruleIds"]).map((ruleId, index) => (
            expectString(ruleId, ["ruleIds", index], 1)
        )),
        payload,
        diagnostics: expectArray(record.diagnostics, ["diagnostics"]).map((diagnostic, index) => {
            try {
                return diagnosticEnvelopeSchema.parse(diagnostic);
            } catch (error) {
                fail(["diagnostics", index], error instanceof Error ? error.message : "invalid diagnostic");
            }
        }),
    };
});

function sortJson(value         )          {
    if (Array.isArray(value)) {
        return value.map(sortJson);
    }

    if (value !== null && typeof value === "object") {
        const record = value                           ;
        return Object.fromEntries(
            Object.keys(record)
                .sort((left, right) => left.localeCompare(right))
                .map((key) => [key, sortJson(record[key])]),
        );
    }

    return value;
}

export function canonicalizeTraceEnvelope(value               )         {
    const trace = traceEnvelopeSchema.parse(value);
    return JSON.stringify(sortJson(trace));
}
