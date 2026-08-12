// dat-skill-flow-build:20260811063630750-677833bd60c640bd969625b2a827b8a9
                               
                           
                               
                                
                              
                                     
                                     
                                 
                           
                                         
 

const valueFlags = new Set([
    "--root",
    "--manifest",
    "--workspace",
    "--data-txt",
    "--asset-workspace",
    "--patch-workspace",
    "--patch-index",
    "--port",
]);

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

    if (!values.has("--workspace") && (values.has("--data-txt") || values.has("--asset-workspace") || values.has("--patch-workspace") || values.has("--patch-index"))) {
        throw new Error("Each project-data option requires --workspace.");
    }
    if (values.has("--patch-workspace") !== values.has("--patch-index")) {
        throw new Error("--patch-workspace and --patch-index must be provided together.");
    }

    return Object.freeze({
        ...(values.has("--root") ? { root: values.get("--root")  } : {}),
        ...(values.has("--manifest") ? { manifest: values.get("--manifest")  } : {}),
        ...(values.has("--workspace") ? { workspace: values.get("--workspace")  } : {}),
        ...(values.has("--data-txt") ? { dataTxt: values.get("--data-txt")  } : {}),
        ...(values.has("--asset-workspace") ? { assetWorkspace: values.get("--asset-workspace")  } : {}),
        ...(values.has("--patch-workspace") ? { patchWorkspace: values.get("--patch-workspace")  } : {}),
        ...(values.has("--patch-index") ? { patchIndex: values.get("--patch-index")  } : {}),
        ...(values.has("--port") ? { port: values.get("--port")  } : {}),
        allowTestRootGrant,
    });
}
