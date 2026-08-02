// dat-skill-flow-build:20260801131023002-16ee7b31622f44128704a6dccbeca365
                               
                           
                               
                                
                           
                                         
 

const valueFlags = new Set(["--root", "--manifest", "--workspace", "--port"]);

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
        if (argument === "--port" && (!/^(?:0|[1-9]\d*)$/.test(value) || Number(value) > 65_535)) {
            throw new Error(`Invalid CLI argument value: --port`);
        }
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
