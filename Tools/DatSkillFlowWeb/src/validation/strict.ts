export type ValidationPath = ReadonlyArray<string | number>;

export interface Validator<T> {
    parse(value: unknown): T;
}

export class ValidationError extends TypeError {
    public readonly path: ValidationPath;

    public constructor(path: ValidationPath, message: string) {
        const location = path.length === 0
            ? "value"
            : path.map((part, index) => typeof part === "number" ? `[${part}]` : `${index === 0 ? "" : "."}${part}`).join("");
        super(`${location}: ${message}`);
        this.name = "ValidationError";
        this.path = [...path];
    }
}

export function validator<T>(parse: (value: unknown) => T): Validator<T> {
    return Object.freeze({ parse });
}

export function fail(path: ValidationPath, message: string): never {
    throw new ValidationError(path, message);
}

export function expectRecord(value: unknown, path: ValidationPath): Record<string, unknown> {
    if (value === null || typeof value !== "object" || Array.isArray(value)) {
        return fail(path, "expected an object");
    }
    return value as Record<string, unknown>;
}

export function expectStrictKeys(
    record: Record<string, unknown>,
    allowedKeys: ReadonlySet<string>,
    path: ValidationPath,
): void {
    for (const key of Object.keys(record)) {
        if (!allowedKeys.has(key)) {
            fail([...path, key], `unknown field: ${key}`);
        }
    }
}

export function expectArray(value: unknown, path: ValidationPath): unknown[] {
    if (!Array.isArray(value)) {
        return fail(path, "expected an array");
    }
    return value;
}

export function expectString(value: unknown, path: ValidationPath, minimumLength = 0): string {
    if (typeof value !== "string" || value.length < minimumLength) {
        return fail(path, `expected a string with at least ${minimumLength} character(s)`);
    }
    return value;
}

export function expectEnum<const T extends string>(
    value: unknown,
    allowedValues: ReadonlySet<T>,
    path: ValidationPath,
): T {
    if (typeof value !== "string" || !allowedValues.has(value as T)) {
        return fail(path, `expected one of: ${[...allowedValues].join(", ")}`);
    }
    return value as T;
}

export function expectLiteral<T extends string | number | boolean | null>(
    value: unknown,
    expected: T,
    path: ValidationPath,
): T {
    if (value !== expected) {
        return fail(path, `expected literal ${JSON.stringify(expected)}`);
    }
    return expected;
}

export function expectNonnegativeInteger(value: unknown, path: ValidationPath): number {
    if (typeof value !== "number" || !Number.isSafeInteger(value) || value < 0) {
        return fail(path, "expected a nonnegative safe integer");
    }
    return value;
}

export function expectFiniteNumber(value: unknown, path: ValidationPath): number {
    if (typeof value !== "number" || !Number.isFinite(value)) {
        return fail(path, "expected a finite number");
    }
    return value;
}
