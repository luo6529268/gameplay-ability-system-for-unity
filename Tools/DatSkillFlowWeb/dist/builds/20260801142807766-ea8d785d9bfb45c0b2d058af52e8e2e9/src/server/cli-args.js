// dat-skill-flow-build:20260801142807766-ea8d785d9bfb45c0b2d058af52e8e2e9
                               
                           
                               
                                
                           
                                         
 

const valueFlags = new Set(["--root", "--manifest", "--workspace", "--port"]);

export function parsePortValue(raw        )         {
    if (!/^(?:0|[1-9]\d*)$/.test(raw)) throw new Error(`Invalid port: ${raw}`);
    const port = Number(raw);
    if (!Number.isSafeInteger(port) || port > 65_535) throw new Error(`Invalid port: ${raw}`);
    return port;
}

export function parseCliArguments(argv                   )               {
    const values = new Map                ();
    let allowTestRootGrant = false;

    for (let index = 0; index < argv.length; index++) {
        const argument = argv[index] ;
        if (argument === "--allow-test-root-grant") {
            if (allowTestRootGrant) throw new Error("Duplicate CLI argument: --allow-test-root-grant");
            allowTestRootGrant = true;
            continue;
        }
        if (!valueFlags.has(argument)) throw new Error(`Unknown CLI argument: ${argument}`);
        if (values.has(argument)) throw new Error(`Duplicate CLI argument: ${argument}`);
        const value = argv[++index];
        if (value === undefined || value.startsWith("--")) throw new Error(`Missing CLI argument value: ${argument}`);
        if (argument === "--port") parsePortValue(value);
        values.set(argument, value);
    }

    return Object.freeze({
        ...(values.has("--root") ? { root: values.get("--root")  } : {}),
        ...(values.has("--manifest") ? { manifest: values.get("--manifest")  } : {}),
        ...(values.has("--workspace") ? { workspace: values.get("--workspace")  } : {}),
        ...(values.has("--port") ? { port: values.get("--port")  } : {}),
        allowTestRootGrant,
    });
}
