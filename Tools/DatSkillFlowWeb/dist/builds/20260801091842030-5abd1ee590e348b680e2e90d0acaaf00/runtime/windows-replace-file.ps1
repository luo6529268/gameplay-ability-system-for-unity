[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$TargetPath,

    [Parameter(Mandatory = $true)]
    [string]$ReplacementPath,

    [Parameter(Mandatory = $true)]
    [string]$BackupPath
)

$ErrorActionPreference = 'Stop'

$nativeSource = @'
using System;
using System.Runtime.InteropServices;

public static class DatSkillFlowNativeMethods
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ReplaceFileW(
        string replacedFileName,
        string replacementFileName,
        string backupFileName,
        uint replaceFlags,
        IntPtr exclude,
        IntPtr reserved);
}
'@

try {
    Add-Type -TypeDefinition $nativeSource -Language CSharp
    $succeeded = [DatSkillFlowNativeMethods]::ReplaceFileW(
        $TargetPath,
        $ReplacementPath,
        $BackupPath,
        0,
        [IntPtr]::Zero,
        [IntPtr]::Zero)

    if ($succeeded) {
        [Console]::Out.WriteLine((@{
            ok = $true
            win32Code = 0
        } | ConvertTo-Json -Compress))
        exit 0
    }

    $win32Code = [Runtime.InteropServices.Marshal]::GetLastWin32Error()
    $message = switch ($win32Code) {
        1175 { 'ERROR_UNABLE_TO_REMOVE_REPLACED: target and replacement are expected at their original paths.' }
        1176 { 'ERROR_UNABLE_TO_MOVE_REPLACEMENT: replacement remains at temp and target is expected restored.' }
        1177 { 'ERROR_UNABLE_TO_MOVE_REPLACEMENT_2: replacement remains at temp; original may be at backup and target may be absent.' }
        default { "ReplaceFileW failed with Win32 error $win32Code." }
    }
    [Console]::Out.WriteLine((@{
        ok = $false
        win32Code = $win32Code
        message = $message
    } | ConvertTo-Json -Compress))
    exit 1
}
catch {
    [Console]::Out.WriteLine((@{
        ok = $false
        win32Code = 1
        message = $_.Exception.Message
    } | ConvertTo-Json -Compress))
    exit 1
}
