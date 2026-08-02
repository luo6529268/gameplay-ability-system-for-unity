// dat-skill-flow-build:20260801034035657-e77b54972a8f44b28020241ce8859fad
import { execFile as nodeExecFile } from "node:child_process";
import { resolve } from "node:path";

                                 
                       
                            
                       
 

                                 
             
 

                                 
              
                      
                 
                    
 

                                                            

                                   
                                                             
 

                                  
                    
                          
                       
                              
 

                            
                 
                            
                             
                                                                            
             

                                                     
                        
                        
                            
 

const mappedErrors                                                              = Object.freeze({
    1175: {
        code: "unable-to-remove-replaced",
        message: "Windows could not remove the replaced file; inspect target and replacement recovery paths.",
    },
    1176: {
        code: "unable-to-move-replacement",
        message: "Windows could not move the replacement; inspect the restored target and replacement paths.",
    },
    1177: {
        code: "unable-to-move-replacement-2",
        message: "Windows could not finish moving the replacement; the original may be at the backup path.",
    },
});

export function mapReplaceFileError(win32Code        )                                    {
    return mappedErrors[win32Code] ?? {
        code: "replace-file-failed",
        message: `ReplaceFileW failed with Win32 error ${win32Code}.`,
    };
}

function parseHelperOutput(stdout        )                                                        {
    const lines = stdout.trim().split(/\r?\n/).filter((line) => line.length > 0);
    const lastLine = lines.at(-1);
    if (lastLine === undefined) {
        throw new Error("ReplaceFileW helper returned no structured output.");
    }
    const value = JSON.parse(lastLine)                           ;
    if (typeof value.ok !== "boolean") {
        throw new Error("ReplaceFileW helper returned an invalid result.");
    }
    if (!value.ok && (!Number.isSafeInteger(value.win32Code) || (value.win32Code          ) < 0)) {
        throw new Error("ReplaceFileW helper returned an invalid Win32 error.");
    }
    return {
        ok: value.ok,
        ...(typeof value.win32Code === "number" ? { win32Code: value.win32Code } : {}),
        ...(typeof value.message === "string" ? { message: value.message } : {}),
    };
}

export class WindowsReplaceFilePublisher                             {
             #scriptPath        ;
             #executable        ;
             #execFile              ;

    constructor(options                                     = {}) {
        this.#scriptPath = resolve(options.scriptPath ?? "scripts/windows-replace-file.ps1");
        this.#executable = options.executable ?? "powershell.exe";
        this.#execFile = options.execFile ?? (nodeExecFile                           );
    }

    async replace(request                )                         {
        const args = [
            "-NoProfile",
            "-NonInteractive",
            "-ExecutionPolicy",
            "Bypass",
            "-File",
            this.#scriptPath,
            "-TargetPath",
            request.targetPath,
            "-ReplacementPath",
            request.replacementPath,
            "-BackupPath",
            request.backupPath,
        ]         ;
        const result = await new Promise                                                         ((resolveResult) => {
            this.#execFile(this.#executable, args, {
                shell: false,
                windowsHide: true,
                maxBuffer: 256 * 1024,
                encoding: "utf8",
            }, (error, stdout, stderr) => resolveResult({ error, stdout, stderr }));
        });
        let helper                                      ;
        try {
            helper = parseHelperOutput(result.stdout);
        } catch (error) {
            throw new Error(
                `ReplaceFileW helper did not return valid structured output${result.stderr.length > 0 ? `: ${result.stderr.trim()}` : "."}`,
                { cause: error },
            );
        }
        if (helper.ok) {
            if (result.error !== null) {
                throw new Error("ReplaceFileW helper reported success with a failing process status.", { cause: result.error });
            }
            return { ok: true };
        }
        const win32Code = helper.win32Code          ;
        const mapped = mapReplaceFileError(win32Code);
        return {
            ok: false,
            win32Code,
            code: mapped.code,
            message: helper.message ?? mapped.message,
        };
    }
}
