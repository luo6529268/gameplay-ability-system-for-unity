# Lee OID204 over the retained Battle background

Status: `VERIFIED / SCOPED_UNITY_COMPOSED_WORLD_CAMERA_PIXEL`. Q07/D-024 and the full battle-alignment goal remain open.

The original Unity Editor saved `NTSD_Battle` Scene and formal Logan content root ran a unique `lee-child-composed-01` Play request. Ordinary discrete J at completed tick8 and L at ticks9–10 reached Lee frame146 at tick11 and frame164 at tick12; five owned OID204/action20 children naturally entered the complete production Driver and materialized CentralOnly plan. The first child Entity command was `VisualDataId=204/EffectivePic=28` with a valid formal `a/cha/cha.png` catalog cell. This is the same J→L event and projected command ROI as the earlier isolated-camera run.

This capture set only a temporary RenderTexture on the saved world camera. It retained the camera's scene culling mask, clear flags/color, HDR and MSAA instead of forcing black/cullingMask0. Both 960×540 PNGs visibly contain the project's retained Battle background and actors. Independent Pillow decoding of the before/after images gives:

| Measurement | Result |
|---|---:|
| First child projected ROI, top-left box | `(154,401,212,459)` / 58×58 |
| Composed world-camera changed RGB pixels within ROI | 600 |
| Pixels changed both here and in the earlier isolated capture, within ROI | 574 |
| Pixels changed only in this composed capture, within ROI | 26 |
| Pixels changed only in the isolated capture, within ROI | 0 |
| Before-frame nonblack pixels outside ROI | 515004 / 515036 |
| Composed changed pixels outside ROI | 1636 |
| Before/after top-left background pixel | RGB `(63,139,211)` / same |

The 574-pixel change-mask overlap plus visible full background supports that the natural child effect is visible over this Battle background in the world-camera pass. The 26 additional ROI changes and 1636 outside changes show that other animation/composition also changed between ticks; they are not assigned to a specific child. The previous isolated readback independently found 288/574 post pixels exactly matching the formal pic28 source RGB set, and staged/formal `cha.png` whole-file SHA identity; those are provenance support, not a claim of exact composited-pixel identity.

| Artifact | SHA-256 |
|---|---|
| `lee-child-composed-01.json` | `BB1CC4F25B0CECC93EA4A5454C97FE136BE877EFA188F0DF02E2D68165CDD71E` |
| `lee-child-composed-01-before.png` | `D678E9C4132AF915EC6E2D8B65058D7857FE71FF8BE380643DC216A1DB678D6F` |
| `lee-child-composed-01-after.png` | `5743509F0893FC651BFCE2D4CA65F9A9A09730A85A2E7E05EF728F121486C12B` |

The request returned PASS, camera parameters/active RenderTexture were restored, and ordered shutdown finished with pool borrowers0. The original Editor exited Play with Console errors0. Saved Menu/Battle Scene SHA-256 stayed `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` / `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`; neither Scene nor Build Settings has a Git diff. No production battle rule, DAT/image bytes, serialized Scene, ProjectSettings or nonbattle behavior was edited.

This is one composited **world-camera** readback from a scripted ordinary-input fixture. It does not include the whole Game view/HUD stack, physical keyboard, all five children independently resolved through their lifetimes, or a formal EXE same-camera GPU A/B. The user-approved fixed full-background framing remains intact. Q07/D-024 and the aggregate parity claim cannot close from this one case.
