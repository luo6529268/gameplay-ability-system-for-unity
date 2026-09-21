# Retained-shell DAT binding audit

## Binding audit update — 2026-09-21

Observed LF2FrameCache.Load clears the existing frame table, assigns Wrapper, fills authored native frame entries and invokes ILF2FrameCacheObserver.OnFrameCacheIdentityChanged. LF2Entity observer calls PublishIdentityMetadataForSimulation: invalidates data-type tick cache, writes Runtime.ObjType/EntityType, then registeredWorld.IdentityWriter.SyncFromEntity. Thus Load is a mutation and must not execute in ValidateBattleStateSnapshotRestore. The getter RuntimeDataCatalog.GetCharacterConfig only looks up the prepared dictionary; use it for preflight resolution without singleton/resource fallback. LF2LivingObject._FrameDataWrapper is already a FrameCache.Wrapper getter, not a second mutable wrapper field.

TryRestoreBattleStateSnapshot currently validates the complete snapshot first, unbinds rest, restores topology, copies raw/entity runtime, then restores base/living/character shells. Candidate binding placement is after saved runtime copy and before TryRestoreBaseShellForSnapshot, with explicit validation of identity metadata consistency. FrameCache observer may publish metadata, so verify final registry values against saved identity and preserve retained renderer ownership; no claim of implementation is made yet. Character-specific controller and weapon cache compatibility remains to be checked before broadening beyond the evidenced type3 transition. Do not silently narrow the overall mutable-definition recovery requirement to one passing case.

Existing IdentityMismatchFailsBeforeMutatingWorld mutates StableId, then checks X and AiRand15 remain unchanged on rejection. Retain this guard. Add a later-slot invalid-definition case to prove the proposed preflight never binds an earlier valid transformed entity before all checks pass.

Documentation verification: Tools/Validate-ChangeLedger.ps1 PASS609 records/40 governed code files; git diff --check passed (existing LF/CRLF advisory only). No dependency scripts changed or new Unity tests executed.
