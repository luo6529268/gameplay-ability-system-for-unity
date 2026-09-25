# Q07 six formal out-of-image frame catalog publication

Status: `VERIFIED_SCOPED_CATALOG_PUBLICATION`. This is an opt-in read-only diagnostic in the existing original Unity Editor, not Q07/R17 completion.

The saved `NTSD_Battle` Scene and serialized formal root were used. The editor recompiled the extended probe; `Library/ScriptAssemblies/Assembly-CSharp-Editor.dll` was newer than its source, the request was consumed once, and the resulting current `read_console` error filter returned zero entries. At production driver tick 5, the published visual key was nonempty and the SpriteCatalog contained 29,034 entries. OID32/pic0 remained a formal `m/nin/hun.png` control with valid central binding.

The opt-in six-key result recorded all six as present. Each was a derived `Temp/NTSD28NativeClampCells/*.rgba` source with a 79×79 shared texture, a non-null Legacy Sprite and valid central binding:

| Formal indexed object / base pic | Original DAT frame | Result |
| --- | ---: | --- |
| OID55 / pic44 | `c/saso/pup.dat` frame105 | present, derived, 79×79, Legacy + central |
| OID32 / pic64 | `m/nin/hun.dat` frame95 | present, derived, 79×79, Legacy + central |
| OID30 / pic81 | `m/nin/nin.dat` frame31 | present, derived, 79×79, Legacy + central |
| OID30 / pic91 | `m/nin/nin.dat` frame41 | present, derived, 79×79, Legacy + central |
| OID31 / pic81 | `m/nin/nin2.dat` frame31 | present, derived, 79×79, Legacy + central |
| OID31 / pic91 | `m/nin/nin2.dat` frame41 | present, derived, 79×79, Legacy + central |

Raw JSON: `../NTSD28-Q07-OID32-PUBLISHED-CATALOG-001/six-boundary-catalog-20260926-01.json`, SHA-256 `D4397B8AC4F4371BCD6E416C38DA1627337F4BF2104C5BBA8EEF0212592E3901`. The observed content key, first four catalog controls, and six per-key results are preserved there. This extends the existing OID32 catalog witness without changing its default request path.

After the request, `requested=false`, `running=false`; original Editor was idle and non-Play on `NTSD_Battle`. Menu Scene SHA remained `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`; Battle Scene SHA remained `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`; neither Scene has a Git diff. This run did not spawn any of the six objects, draw entity pixels, establish natural action selection, or capture root formal EXE GPU output. Those remain their own Q07/R17 gates. No DAT, PNG, importer, Scene, production battle rule or nonbattle file changed.

Governance checks: `Tools/Validate-ChangeLedger.ps1` exited 0 with 841 Records and 32 governed code files in the current diff; `git diff --check` exited 0; all three live progress documents remained NUL-free. No NUnit or aggregate SelfCheck was run for this read-only diagnostic extension.
