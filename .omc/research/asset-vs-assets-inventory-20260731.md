# Asset vs Assets file-system inventory (2026-07-31)

Read-only SHA-256 comparison: recovery `Asset` vs clean baseline `Assets`. Neither directory was modified.

## Logical-path normalization

- `Asset/NTSD/$Folder100002ECA/...` maps to logical `NTSD/...`.
- `Asset/NTSD/00040000001DF94F27659311/...` maps to logical `NTSD/Sprite/...`.
- Both are recovery container names, not source-tree differences. Timestamps are retained only as auxiliary evidence and do not determine recency.

Second-container verification: 688 files = 320 SHA-identical + 368 content-different + 0 recovery-only.

## Corrected SHA-256 results

| Category | Files |
|---|---:|
| Common and SHA-256 identical | 4710 |
| Common path but content different | 1592 |
| Only recovery Asset | 29 |
| Only clean Assets | 1441 |
| Logical union | 7772 |

## Unity metadata

| Check | Asset | Assets |
|---|---:|---:|
| Non-meta files without adjacent .meta | 7 | 13 |
| Orphan .meta | 42 | 23 |
| Duplicate GUID groups | 0 | 0 |

For differing common non-meta files: meta same=38; meta different=311; missing only in Asset=0; missing only in Assets=0; both missing=0.

## Decision boundary

This is a corrected content inventory, not a freshness verdict. Git history must classify each differing or one-sided path before any adoption decision. Do not bulk-copy `Asset`.

## First 40 different common paths

| Logical path | Asset physical path | Asset bytes | Assets bytes | Meta pair |
|---|---|---:|---:|---|
| `InputSystem.inputsettings.asset` | `InputSystem.inputsettings.asset` | 1024 | 1060 | different |
| `InputSystem.inputsettings.asset.meta` | `InputSystem.inputsettings.asset.meta` | 189 | 197 | n/a |
| `KAKO.meta` | `KAKO.meta` | 172 | 180 | n/a |
| `KAKO/CameraFit.meta` | `KAKO/CameraFit.meta` | 172 | 180 | n/a |
| `KAKO/Common.meta` | `KAKO/Common.meta` | 172 | 180 | n/a |
| `KAKO/Utilities.meta` | `KAKO/Utilities.meta` | 172 | 180 | n/a |
| `NTSD.meta` | `NTSD.meta` | 172 | 180 | n/a |
| `NTSD/BeatEmUp.meta` | `NTSD/$Folder100002ECA/BeatEmUp.meta` | 172 | 180 | n/a |
| `NTSD/BeatEmUp/Resources.meta` | `NTSD/$Folder100002ECA/BeatEmUp/Resources.meta` | 172 | 180 | n/a |
| `NTSD/BeatEmUp/Resources/IconArrowClose.png.meta` | `NTSD/$Folder100002ECA/BeatEmUp/Resources/IconArrowClose.png.meta` | 2623 | 2737 | n/a |
| `NTSD/BeatEmUp/Resources/IconArrowOpen.png.meta` | `NTSD/$Folder100002ECA/BeatEmUp/Resources/IconArrowOpen.png.meta` | 2623 | 2737 | n/a |
| `NTSD/BeatEmUp/Resources/iconInfo.png.meta` | `NTSD/$Folder100002ECA/BeatEmUp/Resources/iconInfo.png.meta` | 2623 | 2737 | n/a |
| `NTSD/BeatEmUp/Scripts.meta` | `NTSD/$Folder100002ECA/BeatEmUp/Scripts.meta` | 172 | 180 | n/a |
| `NTSD/BeatEmUp/Scripts/Attributes.meta` | `NTSD/$Folder100002ECA/BeatEmUp/Scripts/Attributes.meta` | 172 | 180 | n/a |
| `NTSD/BeatEmUp/Scripts/Attributes/Editor.meta` | `NTSD/$Folder100002ECA/BeatEmUp/Scripts/Attributes/Editor.meta` | 172 | 180 | n/a |
| `NTSD/BeatEmUp/Scripts/Attributes/Editor/HelpAttribute.cs.meta` | `NTSD/$Folder100002ECA/BeatEmUp/Scripts/Attributes/Editor/HelpAttribute.cs.meta` | 243 | 254 | n/a |
| `NTSD/BeatEmUp/Scripts/Attributes/Editor/ReadOnlyAttribute.cs.meta` | `NTSD/$Folder100002ECA/BeatEmUp/Scripts/Attributes/Editor/ReadOnlyAttribute.cs.meta` | 243 | 254 | n/a |
| `NTSD/BeatEmUp/Scripts/Attributes/HelpAttribute.cs.meta` | `NTSD/$Folder100002ECA/BeatEmUp/Scripts/Attributes/HelpAttribute.cs.meta` | 243 | 254 | n/a |
| `NTSD/BeatEmUp/Scripts/Attributes/ReadOnlyAttribute.cs.meta` | `NTSD/$Folder100002ECA/BeatEmUp/Scripts/Attributes/ReadOnlyAttribute.cs.meta` | 243 | 254 | n/a |
| `NTSD/BeatEmUp/Scripts/Tools.meta` | `NTSD/$Folder100002ECA/BeatEmUp/Scripts/Tools.meta` | 215 | 225 | n/a |
| `NTSD/BeatEmUp/Scripts/Tools/AnimationCurveAnim.cs.meta` | `NTSD/$Folder100002ECA/BeatEmUp/Scripts/Tools/AnimationCurveAnim.cs.meta` | 243 | 254 | n/a |
| `NTSD/BeatEmUp/Scripts/Tools/MathUtilities.cs.meta` | `NTSD/$Folder100002ECA/BeatEmUp/Scripts/Tools/MathUtilities.cs.meta` | 286 | 299 | n/a |
| `NTSD/BeatEmUp/Scripts/Tools/ObjectSorting.cs.meta` | `NTSD/$Folder100002ECA/BeatEmUp/Scripts/Tools/ObjectSorting.cs.meta` | 243 | 254 | n/a |
| `NTSD/BeatEmUp/Scripts/Tools/TimeToLive.cs` | `NTSD/$Folder100002ECA/BeatEmUp/Scripts/Tools/TimeToLive.cs` | 860 | 867 | different |
| `NTSD/BeatEmUp/Scripts/Tools/TimeToLive.cs.meta` | `NTSD/$Folder100002ECA/BeatEmUp/Scripts/Tools/TimeToLive.cs.meta` | 243 | 254 | n/a |
| `NTSD/BeatEmUp/Scripts/UI.meta` | `NTSD/$Folder100002ECA/BeatEmUp/Scripts/UI.meta` | 172 | 180 | n/a |
| `NTSD/CONFIG.md.meta` | `NTSD/$Folder100002ECA/CONFIG.md.meta` | 158 | 165 | n/a |
| `NTSD/Config.meta` | `NTSD/$Folder100002ECA/Config.meta` | 172 | 180 | n/a |
| `NTSD/Config/AnimationConfig.meta` | `NTSD/$Folder100002ECA/Config/AnimationConfig.meta` | 172 | 180 | n/a |
| `NTSD/Config/AnimationConfig/Kakashi.meta` | `NTSD/$Folder100002ECA/Config/AnimationConfig/Kakashi.meta` | 172 | 180 | n/a |
| `NTSD/Config/AnimationConfig/Kakashi/kakashi.dat.meta` | `NTSD/$Folder100002ECA/Config/AnimationConfig/Kakashi/kakashi.dat.meta` | 155 | 162 | n/a |
| `NTSD/Config/AnimationConfig/Mingren.meta` | `NTSD/$Folder100002ECA/Config/AnimationConfig/Mingren.meta` | 172 | 180 | n/a |
| `NTSD/Config/AnimationConfig/Mingren/naruto.dat.meta` | `NTSD/$Folder100002ECA/Config/AnimationConfig/Mingren/naruto.dat.meta` | 155 | 162 | n/a |
| `NTSD/Config/AnimationConfig/XiaoYing.meta` | `NTSD/$Folder100002ECA/Config/AnimationConfig/XiaoYing.meta` | 172 | 180 | n/a |
| `NTSD/Config/AnimationConfig/XiaoYing/sakura.dat.meta` | `NTSD/$Folder100002ECA/Config/AnimationConfig/XiaoYing/sakura.dat.meta` | 155 | 162 | n/a |
| `NTSD/Config/AnimationConfig/ZuoZhu.meta` | `NTSD/$Folder100002ECA/Config/AnimationConfig/ZuoZhu.meta` | 172 | 180 | n/a |
| `NTSD/Config/AnimationConfig/ZuoZhu/sasuke.dat.meta` | `NTSD/$Folder100002ECA/Config/AnimationConfig/ZuoZhu/sasuke.dat.meta` | 155 | 162 | n/a |
| `NTSD/Config/chars.meta` | `NTSD/$Folder100002ECA/Config/chars.meta` | 172 | 180 | n/a |
| `NTSD/Config/chars/criminal.dat.meta` | `NTSD/$Folder100002ECA/Config/chars/criminal.dat.meta` | 155 | 162 | n/a |
| `NTSD/Config/chars/ex_ball.dat.meta` | `NTSD/$Folder100002ECA/Config/chars/ex_ball.dat.meta` | 155 | 162 | n/a |

Machine manifest: `asset-vs-assets-inventory-20260731.json` contains every normalized logical path, both physical relative paths, SHA-256, size, mtimeUtc, status, aggregates, and metadata diagnostics.
