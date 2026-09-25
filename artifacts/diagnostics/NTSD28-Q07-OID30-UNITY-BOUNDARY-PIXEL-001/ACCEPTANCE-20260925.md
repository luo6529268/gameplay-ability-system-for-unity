# OID30 frame31 original-Editor catalog and camera witness

Status: `VERIFIED_SCOPED_CONTROLLED_UNITY_PIXEL`. The formal source/release precursor `NTSD28-Q07-NIN-FRAME31-VISIBILITY-WITNESS-001` proved a controlled OID30/action31/pic81 command on ticks1–7 and 36/36 source/release trace fields. This report is a separate Unity production-publication and camera observation; it does not combine the two into a same-world root-EXE GPU certificate.

The original project Editor 2022.3.62f3, PID11944, imported the single existing Editor-probe extension. Its `Assembly-CSharp-Editor.dll` timestamp followed the source edit, and the Editor log reported `*** Tundra build success` with no current C# error. Two serial saved-Battle-Scene Play requests were run after compilation, with no second Unity project or computer-use:

| Request | Observed result |
| --- | --- |
| `oid30-frame31-pixel-20260925-01` with explicit `target=oid30-frame31` | `PASS`; formal content fingerprint `27CAE014...E02D`; actual production catalog has OID30/pic81, 79×79 derived texture under `Temp/NTSD28NativeClampCells`, valid Legacy Sprite and Central binding. After one full production Driver tick, tick6 stable102/slot50/frame31/pic81 has exactly one matching Entity command among nine. WorldCamera black-clear readback projects a 56×57 ROI with 2,870 pure white and 3,080 nonclear pixels. The PNG was visually inspected; the white body square is visible. |
| `oid32-default-regression-20260925-01` with no `target` field | `PASS`; original default OID32/frame95/pic64 still has exactly one matching Entity command among nine, and 56×56 ROI has 2,835 pure white / 3,136 nonclear pixels. The legacy `oid32CommandSummary` remains `102:64;`. |

Both requests restored object count 4→4, claimed slots 2→2, active object pool 2→2, logic pool 2→2, with empty cleanup error. Each post-exit result observed non-Play and zero live `native_clamp_` Texture2D. Menu/Battle on-disk SHA remained `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` / `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`; the original Editor was observed idle/non-Play after completion. The script modification adds only a fixed opt-in target and catalog fields; no production combat logic, DAT, PNG, Scene, GameConfig or nonbattle module changed.

| Raw artifact | SHA-256 |
| --- | --- |
| OID30 report JSON | `5284D3EF622D81CCCACB84102F54661182DAD18646A1DE1564883ED2E3D6B477` |
| OID30 camera PNG | `A4613BD4D074FC7EDDA07B8DA2E41188AFDFFF98ACCC569C448C519E62DC7D67` |
| OID30 post-exit JSON | `33072052725EABAD21C5F2C572802796CEB3378EAFE4A2ED79650300179F06D3` |
| OID32 default regression JSON | `F1B890C8C174BA5036EA43562DB3893EC2806B538E2F6BEB0237A2986DC99BD8` |
| OID32 default regression PNG | `ADAB84126196772A362DDBCD6C464632EC114AE429F00A09EBDE38FB47FA4450` |
| OID32 post-exit JSON | `2C4A5BD15F627E0393BB88E16E126FC8BDCB1684646450FCA9CF261A06AEA043` |

The formal EXE trace only reports aggregate sprite count, while the per-OID sprite command is observed in the paired source snapshot. The Unity camera readback is from a controlled fixture after one full Driver tick, not a natural jump-with-weapon player input sequence. No direct root-EXE GUI framebuffer A/B was captured. OID31/nin2 and frame41/pic91 are not covered by this test; the generic publisher remains a common implementation, but this witness does not grant all-frame parity. Next Q07/R17 action should establish natural reachability or a more direct formal GUI comparison for a representative case before promoting broader visual alignment.
