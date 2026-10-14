# OP-NTSD-M12-UNITY-FONT-SIDE-EFFECT-20261014

Status: VERIFIED
Task: NTSD-M12-MATCH-IMAGE-RESIDENCY-20261013
Change: NTSD-M12-MATCH-IMAGE-RESIDENCY-001

Only observed unrelated TMP dynamic font Asset differences; no font write/restore executed. Exact path, initial/observed SHA, preimages and timeline: artifacts/diagnostics/M12-match-image-residency-20261013/font-side-effect-01/audit.json. Initial baseline preserved preexisting dirty bytes. Scene05/06 baselines alreadyshowintermediateglyph additions; latestobservedmtimecoincideswithScene06exit. AutomaticEditorSaveAssets inference, no processaudit. Keepcurrentfontunchanged untilhumanclarifies. Proposedrollback exactinitialbyteimage afterEditorexits; newOperationrevisionmustcapture latestcurrent preimage/CAS if approved. NoPNG/DAT/pages/meta/Scene/settings deletion or overwrite.

2026-10-14 humanreply: 有新增字体没什么问题. ExistingTMPAsset automaticdynamicglyphadditions accepted; preserveobservedfont, no restore/overwrite/delete. Main shouldreportthisexception separately fromprotectedPNG/DAT/pages andtake finalfontidentity. Proposedrollbacknotexecuted, approvalpendingresolvedbyretainchoice.

Finalretained font aftersuccessfulScene08: SHA2563b9b60811360480498ddbfb079a591418fea6f6a7b3fe402e4f7626056f25eae,2136778bytes,63characters vsinitial44; total19added/+9626bytes. Userretainchoicecoversaddedglyphs; snapshotsretained-after-success.asset andretained-final.json exact. No originalfontrestoreoroverwrite bymain.
