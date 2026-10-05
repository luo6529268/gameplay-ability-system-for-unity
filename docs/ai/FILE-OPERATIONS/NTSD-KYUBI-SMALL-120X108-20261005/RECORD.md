# NTSD-KYUBI-SMALL-120X108-20261005

Status: PLANNED
Type: overwrite one PNG; no project script changes.
User authorization: 2026-10-05 request to change small/4t_kyubi_s to120*108 for a clarity comparison.
Executor: root Codex task; PowerShell PID 77788.
Start UTC: 2026-10-04T20:45:25.4262087+00:00
Working directory: I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity
Exact target: I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Content\LoganRuntime\vfs\sprite\small\4t_kyubi_s.png
Preserved companion: I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Content\LoganRuntime\vfs\sprite\small\4t_kyubi_s.png.meta
Original: 60x54 RGBA. Target:120x108 RGBA using exact 2x nearest-neighbor pixel replication.
Before manifest: before.json. Target clean before edit; global preexisting changes recorded in git-before.txt.
Recovery source: 4t_kyubi_s.before.png and .meta backup, SHA verified. Restore only with user authorization.
Reference: decoded_dat/c/nar/4tk.dat small path remains unchanged. GUID a9f7182e7fbb6d04c84c2046039ff602; no serialized GUID consumers found in searched prefab/scene/asset files.
Exact operation: command.txt; invoke recorded PowerShell block with asset/opDir above.
Scope: PNG dimensions only. No importer, DAT, code or Scene changes. This is the user-requested local art experiment, not a change to battle rules.
Validation: decode final dimensions; compare every output RGBA pixel against source[floor(x/2),floor(y/2)]; verify .meta SHA unchanged.

## Result
Status: VERIFIED (file/pixel validation only). Recorded command executed successfully, no exception.12960/12960 RGBA pixels exactly match2x source replication. PNG120x108. Meta hash unchanged. Evidence:after.json; git-after.txt. Runtime clarity pending user comparison. End UTC:2026-10-04T20:45:25.5631763+00:00.

