# NTSD28-Q09-SMALLB-EXCEPTION-CORRECTION-001 acceptance

Status: VERIFIED for scope correction only; Q09 and the overall battle-alignment goal remain open.

The approved alignment table §0.2, §1.2 and P-17 exclude the complete native character HUD. The prior smallb publication package mistakenly treated that HUD consumer as required Q09 work. Its smallb-only candidate, decode, publication and portrait API additions have been surgically removed. Production candidates again cover the 906 distinct file/head/small indexed images. The 104 distinct smallb files remain part of the 1,010/1,010 byte-identical formal/staged content inventory; staging evidence is retained, but no native HUD consumption is required. The independently valid ProjectBattleModeConfig test snapshots remain because original mode DAT consumption is separately excluded.

Validation in the original Unity Editor: refresh compiled with zero Console errors; focused EditMode job `52c98c44e9c74c49a07162c70faca838` passed 5/5, including 330-object publication/recycle and adjacent kill-icon/WORDS checks. Candidate job `27a96589503945deac8a62efc9947165` passed 7/7. Ledger validator passed with 784 Records and 21 current code diffs covered; `git -c core.safecrlf=false diff --check` passed. Production search found no `smallb` or `BattlePortraitSprite` in the three corrected runtime scripts, and `CharacterUIResourceManager.cs` has no remaining Git diff.

The Battle Scene SHA-256 remains `9409F2BCFE3E657A6C3C88A7527045CC384D50AAACC99197D53AACA38F3B3A39`; the Menu Scene remains `3B0F58AA88BEC495AA999D014CB2779E935B21F0374826357B4DC64AE5B80228`. This correction did not edit Scenes, DAT data, image files, Prefabs or ProjectSettings, did not delete user content and did not use computer-use or a second Unity project. `HeadImg` and the user-confirmed `HUDBg x30` remain as before.

The superseded package's earlier tests remain a historical record, not an instruction to restore its runtime code. Continue only with nonexcluded Q09 battle presentation and Q07 content/ref ownership work. This package does not claim full battle visual parity or full Q09 closure.
