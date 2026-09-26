# Lee OID204 natural child camera pixel witness

Status: `VERIFIED / SCOPED_UNITY_ISOLATED_CENTRAL_CAMERA_PIXEL`. Q07/D-024 and full visual equivalence remain open.

The original Editor's saved `NTSD_Battle` Scene used the formal Logan content root and production complete Driver. The opt-in pixel request `lee-child-pixel-01` returned PASS. It saved a black-clear, layer-culled world-camera readback after completed tick11/Lee frame146 and another after completed tick12/Lee frame164, when five Lee-owned OID204/action20 children were naturally present. This isolates the central battle renderer while using the actual saved Scene camera and URP render path; it is not a normal-background screenshot or a root EXE GPU capture.

The materialized CentralOnly plan contained five child Entity commands. The selected first command was `VisualDataId=204`, `EffectivePic=28`, matching the root formal EXE trace at tick6, where all five OID204 have action20/pic28 and source position `(437,-35,651)`. The command resolved to formal `a/cha/cha.png` cell `(x=0,y=0,w=81,h=82)`. The Unity staged sheet and the formal root sheet `resources/runtime/vfs/a/cha/cha.png` have identical SHA-256 `BF69A255C2782014D430664BEDD23E41B961D4E40789B3E360ADAFA3715DF352`.

The command's projected camera ROI was bottom-left `(154,81,58,58)` in a 960×540 readback, or Pillow top-left box `(154,401,212,459)`. Independently decoding both PNGs produced:

| Measurement | Result |
|---|---:|
| Baseline nonblack ROI pixels | 0 |
| Post-birth nonblack ROI pixels | 574 |
| RGB-changed ROI pixels | 574 |
| Changed and nonblack ROI pixels | 574 |
| Distinct changed nonblack RGB colors | 19 |
| Post-birth pixels exactly in formal pic28 nonblack RGB set | 288 / 574 |
| Post-birth pixels within RGB Euclidean distance 5 of source set | 471 / 574 |
| Post-birth pixels within distance 20 | 546 / 574 |

The source cell was read with Unity's bottom-left pixel-rect origin against the formal PNG's height 663; its ten nonblack RGB values included eight of the 19 displayed values. The remaining shades include interpolation/blending and dark edge pixels; exact source color overlap plus an empty pre-birth ROI supports visible OID204 content in this bounded camera capture. It does not attribute every changed pixel exclusively among five overlapping children or prove root EXE same-camera pixels.

Artifact SHA-256:

| Artifact | SHA-256 |
|---|---|
| `lee-child-pixel-01.json` | `F931362B216636E89DF78B89B81C1651C9DD7A1143FE4DB4E37CE7E832F2024E` |
| `lee-child-pixel-01-before.png` | `2EE1F405C27D772999ABBF92262E3F92CB1AACFA675E60FB5D4F39761877A3DA` |
| `lee-child-pixel-01-after.png` | `5B00E98688DE7BFAB22C0C4CFA6B8C5734EE12367C84A16A345DF3BF9AA1AFE0` |

Camera culling mask, clear flags/color, HDR/MSAA, target texture and active RenderTexture were restored. Production Driver completed the usual 45 diagnostic ticks; ordered shutdown completed with pool borrowers0. Original Editor exited Play with Console errors0. Menu/Battle saved Scene SHA-256 stayed `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` / `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A` and no Scene/Build Settings Git diff. No production battle rule, DAT/image bytes, serialized Scene, ProjectSettings or nonbattle function changed.

Remaining gates: ordinary physical-key Play, full child interaction/lifespan, normal composed-background visibility/occlusion, and root formal EXE same-input/same-camera GPU comparison where presentation is not excluded. This package closes only the Unity isolated central-camera pixel question for this exact Lee child birth.
