// dat-skill-flow-build:20260808104752039-9d7255dbd5514ced96a8d46ec2294102
                               
                           
                               
                                
                              
                                     
                           
                                         
 

const valueFlags = new Set(["--root", "--manifest", "--workspace", "--data-txt", "--asset-workspace", "--port"]);

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

    if (!values.has("--workspace") && (values.has("--data-txt") || values.has("--asset-workspace"))) {
        throw new Error("Each of --data-txt and --asset-workspace requires --workspace.");
    }

    return Object.freeze({
        ...(values.has("--root") ? { root: values.get("--root")  } : {}),
        ...(values.has("--manifest") ? { manifest: values.get("--manifest")  } : {}),
        ...(values.has("--workspace") ? { workspace: values.get("--workspace")  } : {}),
        ...(values.has("--data-txt") ? { dataTxt: values.get("--data-txt")  } : {}),
        ...(values.has("--asset-workspace") ? { assetWorkspace: values.get("--asset-workspace")  } : {}),
        ...(values.has("--port") ? { port: values.get("--port")  } : {}),
        allowTestRootGrant,
    });
}
