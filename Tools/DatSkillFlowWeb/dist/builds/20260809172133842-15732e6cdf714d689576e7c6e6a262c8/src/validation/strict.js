// dat-skill-flow-build:20260809172133842-15732e6cdf714d689576e7c6e6a262c8
                                                            

                               
                             
 

export class ValidationError extends TypeError {
                    path                ;

           constructor(path                , message        ) {
        const location = path.length === 0
            ? "value"
            : path.map((part, index) => typeof part === "number" ? `[${part}]` : `${index === 0 ? "" : "."}${part}`).join("");
        super(`${location}: ${message}`);
        this.name = "ValidationError";
        this.path = [...path];
    }
}

export function validator   (parse                       )               {
    return Object.freeze({ parse });
}

export function fail(path                , message        )        {
    throw new ValidationError(path, message);
}

export function expectRecord(value         , path                )                          {
    if (value === null || typeof value !== "object" || Array.isArray(value)) {
        return fail(path, "expected an object");
    }
    return value                           ;
}

export function expectStrictKeys(
    record                         ,
    allowedKeys                     ,
    path                ,
)       {
    for (const key of Object.keys(record)) {
        if (!allowedKeys.has(key)) {
            fail([...path, key], `unknown field: ${key}`);
        }
    }
}

export function expectArray(value         , path                )            {
    if (!Array.isArray(value)) {
        return fail(path, "expected an array");
    }
    return value;
}

export function expectString(value         , path                , minimumLength = 0)         {
    if (typeof value !== "string" || value.length < minimumLength) {
        return fail(path, `expected a string with at least ${minimumLength} character(s)`);
    }
    return value;
}

export function expectEnum                        (
    value         ,
    allowedValues                ,
    path                ,
)    {
    if (typeof value !== "string" || !allowedValues.has(value     )) {
        return fail(path, `expected one of: ${[...allowedValues].join(", ")}`);
    }
    return value     ;
}

export function expectLiteral                                            (
    value         ,
    expected   ,
    path                ,
)    {
    if (value !== expected) {
        return fail(path, `expected literal ${JSON.stringify(expected)}`);
    }
    return expected;
}

export function expectNonnegativeInteger(value         , path                )         {
    if (typeof value !== "number" || !Number.isSafeInteger(value) || value < 0) {
        return fail(path, "expected a nonnegative safe integer");
    }
    return value;
}

export function expectFiniteNumber(value         , path                )         {
    if (typeof value !== "number" || !Number.isFinite(value)) {
        return fail(path, "expected a finite number");
    }
    return value;
}
