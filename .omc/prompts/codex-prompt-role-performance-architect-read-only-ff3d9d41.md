---
provider: "codex"
agent_role: "architect"
model: "gpt-5.3-codex"
files:
  - "Temp/NTSD_ProductionEntityStress.dispersed-ai-nosort-render-sort-20260725.json"
  - "Assets/NTSD/Scripts/Simulation/SimulationWorld.StageRender.partial.cs"
  - "Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs"
  - "Assets/NTSD/Scripts/Animation/Rendering/BattlePresentationShadowBuild.cs"
  - "Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectRenderer.cs"
  - "Assets/NTSD/Scripts/Animation/LF2Objects/LF2Sprite.cs"
timestamp: "2026-07-25T08:26:13.306Z"
---

--- File: Temp/NTSD_ProductionEntityStress.dispersed-ai-nosort-render-sort-20260725.json ---
{
    "schema": "ntsd-production-entity-stress/v1",
    "status": "StoppedCleanly",
    "mode": "Dispersed1000",
    "startedUtc": "2026-07-25T08:21:54.1464783Z",
    "updatedUtc": "2026-07-25T08:23:47.9588441Z",
    "unityVersion": "2022.3.34f1c1",
    "platform": "WindowsEditor",
    "scene": "NTSD_Battle",
    "stressRootName": "NTSD Production Entity Stress [Dispersed1000]",
    "outputPath": "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Temp\\NTSD_ProductionEntityStress.dispersed-ai-nosort-render-sort-20260725.json",
    "failure": "",
    "harnessValidity": true,
    "performanceVerdict": "EvidenceOnlyNoThreshold",
    "requestedEntityCount": 1000,
    "selectedCharacterOid": 1,
    "totalEntitiesCreated": 1000,
    "lifecycleReplacements": 0,
    "activeGameObjectCount": 0,
    "stressRootChildCount": 0,
    "worldObjectCount": 0,
    "worldEntityCount": 0,
    "peakWorldEntityCount": 1000,
    "claimedRuntimeSlotCount": 0,
    "runtimeProfile": "MobileExtended",
    "runtimeSlotCapacity": 1050,
    "broadphaseBackend": "LooseQuadtree",
    "logicTicksExecuted": 388,
    "warmupTicksCompleted": 5,
    "sampledLogicTicks": 383,
    "sampledUnityFrames": 97,
    "framesWithCatchUp": 97,
    "maximumCatchUpTicksInFrame": 4,
    "currentBacklogTicks": 4,
    "maximumBacklogTicks": 4,
    "droppedBacklogTicks": 2881,
    "aiControlledEntityTicks": 388000,
    "collisionCandidateCountSum": 17062,
    "collisionCandidateCountPeak": 735,
    "broadphasePairCountSum": 7625891,
    "broadphasePairCountPeak": 184181,
    "broadphaseFallbackParticipantPeak": 154,
    "broadphaseAbortedTicks": 0,
    "broadphaseLastIndexedCount": 996,
    "damageStatTotal": 0,
    "killStatTotal": 0,
    "opointCounterAvailable": true,
    "observedOpointCreates": 0,
    "opointCounterReason": "Runtime-derived observable proxy: unique active non-harness runtime handles observed after each logic tick. It is not a production opoint creation counter.",
    "logicTickMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Stopwatch around SimulationTickDriver.StepOneTick -> NTSDBattleTickSystem.RunReleaseTick",
        "unavailableReason": "",
        "sampleCount": 383,
        "average": 277.30811644908627,
        "maximum": 710.9017,
        "p95": 447.0875799999999,
        "p99": 576.819974
    },
    "unityFrameMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Time.unscaledDeltaTime for visible Play Mode frames",
        "unavailableReason": "",
        "sampleCount": 97,
        "average": 1142.293440005214,
        "maximum": 2097.487688064575,
        "p95": 1653.32453250885,
        "p99": 1996.4633655548087
    },
    "logicTickAllocatedBytes": {
        "available": true,
        "unit": "bytes",
        "source": "GC.GetAllocatedBytesForCurrentThread around production logic tick",
        "unavailableReason": "",
        "sampleCount": 383,
        "average": 0.0,
        "maximum": 0.0,
        "p95": 0.0,
        "p99": 0.0
    },
    "phaseTimingEnabled": true,
    "phaseTimingSource": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
    "phaseTimings": [
        {
            "phase": "BattleFlow",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.001123759791122716,
                "maximum": 0.0056,
                "p95": 0.0018899999999999979,
                "p99": 0.0023180000000000008
            }
        },
        {
            "phase": "Cooldown",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.8524926892950391,
                "maximum": 2.3381000000000005,
                "p95": 1.15035,
                "p99": 1.563018000000001
            }
        },
        {
            "phase": "HumanInput",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.1787386422976503,
                "maximum": 0.28350000000000005,
                "p95": 0.21813,
                "p99": 0.25677200000000008
            }
        },
        {
            "phase": "RuntimeMaintenance",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.05515248041775454,
                "maximum": 0.15030000000000003,
                "p95": 0.06523999999999998,
                "p99": 0.08710600000000013
            }
        },
        {
            "phase": "InputClear",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.0,
                "maximum": 0.0,
                "p95": 0.0,
                "p99": 0.0
            }
        },
        {
            "phase": "CharacterInput",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 84.50532741514367,
                "maximum": 234.1571,
                "p95": 206.46412999999994,
                "p99": 223.15703200000008
            }
        },
        {
            "phase": "EarlyFrameAdvance",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.7886206266318536,
                "maximum": 1.7825000000000003,
                "p95": 1.02588,
                "p99": 1.3863400000000015
            }
        },
        {
            "phase": "FrameLogic",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.2063013054830289,
                "maximum": 0.38230000000000005,
                "p95": 0.25158,
                "p99": 0.3182400000000001
            }
        },
        {
            "phase": "FrameAdvance",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 2.105249869451696,
                "maximum": 8.8215,
                "p95": 2.7652199999999995,
                "p99": 3.3124660000000016
            }
        },
        {
            "phase": "DeathCleanup",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.18793394255874663,
                "maximum": 0.5192,
                "p95": 0.24027999999999989,
                "p99": 0.32116600000000025
            }
        },
        {
            "phase": "StageBounds",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 1.6602498694516977,
                "maximum": 5.6694,
                "p95": 2.0003599999999994,
                "p99": 2.7226380000000005
            }
        },
        {
            "phase": "PreInteraction",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 2.2807524804177548,
                "maximum": 8.5838,
                "p95": 2.72096,
                "p99": 4.258308000000001
            }
        },
        {
            "phase": "HeldLinkValidation",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.09448825065274151,
                "maximum": 0.18280000000000003,
                "p95": 0.10989999999999996,
                "p99": 0.13495400000000006
            }
        },
        {
            "phase": "HeldProcess",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.07970365535248046,
                "maximum": 0.1826,
                "p95": 0.09731999999999996,
                "p99": 0.11642800000000003
            }
        },
        {
            "phase": "CollisionSnapshot",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.7147772845953001,
                "maximum": 1.7043000000000002,
                "p95": 0.8860899999999998,
                "p99": 1.0607260000000003
            }
        },
        {
            "phase": "PairVRest",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.1412814621409922,
                "maximum": 0.33,
                "p95": 0.18254999999999997,
                "p99": 0.24474
            }
        },
        {
            "phase": "CandidateCollect",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 39.71472375979117,
                "maximum": 271.5792,
                "p95": 155.43277999999999,
                "p99": 212.27134600000006
            }
        },
        {
            "phase": "CharacterHitConsumePostInteraction",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 1.5591537859007823,
                "maximum": 16.5423,
                "p95": 2.2641299999999999,
                "p99": 3.318210000000001
            }
        },
        {
            "phase": "RandomWeaponDrop",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.23201958224543094,
                "maximum": 0.755,
                "p95": 0.32289999999999999,
                "p99": 0.4366960000000001
            }
        },
        {
            "phase": "ObjectHitConsume",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.29963603133159269,
                "maximum": 0.5174,
                "p95": 0.3794399999999998,
                "p99": 0.4293580000000001
            }
        },
        {
            "phase": "CandidateConsumptionEnd",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.0024477806788511735,
                "maximum": 0.0082,
                "p95": 0.004,
                "p99": 0.005718000000000001
            }
        },
        {
            "phase": "PreFrameBounds",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 1.2777383812010444,
                "maximum": 2.782,
                "p95": 1.7585599999999995,
                "p99": 2.0339420000000008
            }
        },
        {
            "phase": "Stage",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.0011634464751958246,
                "maximum": 0.1032,
                "p95": 0.0015899999999999979,
                "p99": 0.0021540000000000024
            }
        },
        {
            "phase": "RenderDispatch",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 48.084583289817278,
                "maximum": 80.1949,
                "p95": 57.62239,
                "p99": 69.81298200000004
            }
        },
        {
            "phase": "FramePostProcess",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.7225080939947779,
                "maximum": 1.6394,
                "p95": 0.9699099999999999,
                "p99": 1.3158560000000006
            }
        },
        {
            "phase": "LateEntityUpdate",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 90.72906631853788,
                "maximum": 229.53310000000003,
                "p95": 180.48702999999999,
                "p99": 217.539704
            }
        },
        {
            "phase": "RandomWeaponDropTail",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.0005373368146214112,
                "maximum": 0.0028,
                "p95": 0.0008,
                "p99": 0.0011
            }
        },
        {
            "phase": "EntityPostFrameTail",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.7696002610966067,
                "maximum": 1.7313,
                "p95": 1.02711,
                "p99": 1.1971600000000005
            }
        },
        {
            "phase": "BattleResults",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.0006853785900783291,
                "maximum": 0.0021000000000000005,
                "p95": 0.001,
                "p99": 0.0013180000000000009
            }
        }
    ],
    "phaseTimingUnattributedMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Outer SimulationTickDriver.StepOneTick time minus the sum of attributed pass timings",
        "unavailableReason": "",
        "sampleCount": 383,
        "average": 0.062059268929487879,
        "maximum": 0.15119999999996026,
        "p95": 0.07919999999998595,
        "p99": 0.11374599999997098
    },
    "loggingPolicy": {
        "originalFilterLogType": "Log",
        "runningFilterLogType": "Error",
        "policy": "Suppress Log and Warning during the stress run while retaining Error.",
        "applied": false,
        "restored": true
    },
    "teardown": {
        "attempted": true,
        "restored": true,
        "activeStateRestored": true,
        "driverStateRestored": true,
        "loggingStateRestored": true,
        "activeGameObjectsBefore": 1000,
        "activeGameObjectsAfter": 0,
        "worldObjectsBefore": 2000,
        "worldObjectsAfter": 0,
        "worldEntitiesBefore": 1000,
        "worldEntitiesAfter": 0,
        "claimedSlotsBefore": 1000,
        "claimedSlotsAfter": 0,
        "objectPoolActiveBeforeRun": 0,
        "objectPoolActiveAfter": 0,
        "objectPoolAvailableBeforeRun": 10,
        "objectPoolAvailableAfter": 1001,
        "retainedInactiveObjectPoolCapacityBeforeRun": 10,
        "retainedInactiveObjectPoolCapacityAfter": 1001,
        "retainedInactiveObjectPoolCapacityDelta": 991,
        "retainedInactiveObjectPoolCapacityPolicy": "Informational inactive cache capacity only; it is not active cleanup residue and the stress harness does not trim it.",
        "referencePoolActiveBeforeRun": 0,
        "referencePoolActiveAfter": 0,
        "cleanupExceptionCount": 0,
        "cleanupExceptions": "",
        "evidence": "reason=stop-request; restored=True; activeCleanupRestored=True; driverRestored=True; loggerRestored=True; cleanupExceptions=0; activeGO=1000->0; worldObjects=2000->0; worldEntities=1000->0; claimed=1000->0; objectPoolActive=0->0; referencePoolActive=0->0; retainedInactiveObjectPoolCapacity=10->1001 (delta=991; doesNotAffectRestored=True)"
    }
}

--- File: Assets/NTSD/Scripts/Simulation/SimulationWorld.StageRender.partial.cs ---
﻿using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.LevelEditor;
using NTSD.Simulation.Presentation;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// SimulationWorld 的关卡边界、摄像机和渲染相关 pass。
    /// </summary>
    public partial class SimulationWorld
    {
        // These are presentation-only Unity sorting sub-orders. P1 renders Shadow,
        // Entity, and HitRecord; Overlay remains a reserved P3 slot until it has a
        // production consumer.
        internal const int PresentationShadowSubOrder = 0;
        internal const int PresentationEntitySubOrder = 1;
        internal const int PresentationReservedOverlaySubOrder = 2;
        internal const int PresentationHitRecordSubOrder = 3;
        private const int PresentationSubOrderCount = 4;

        // Legacy SpriteRenderer sortingOrder is a signed 16-bit value. Reserving
        // four contiguous presentation positions per entity leaves 8192 published
        // entities before a positive sorting order would overflow. Central rendering
        // removes this temporary legacy-backend limit.
        internal const int LegacySpriteRendererMaxPresentationEntities =
            (short.MaxValue + 1) / PresentationSubOrderCount;

        private readonly Dictionary<LF2Entity, PresentationRenderOrder> _presentationRenderOrders =
            new Dictionary<LF2Entity, PresentationRenderOrder>();
        private static readonly System.Comparison<LF2Entity> PresentationOrderComparison =
            ComparePresentationRenderOrder;
        private readonly List<LF2Entity> _presentationRenderScratch = new List<LF2Entity>(128);
        private readonly List<ISimObject> _rendererSnapshotScratch = new List<ISimObject>(128);
        private readonly BattlePresentationCoordinator _battlePresentation =
            new BattlePresentationCoordinator();
        private BattlePixelFramePlan _currentPixelFramePlan;

        public BattlePresentationCoordinator BattlePresentation => _battlePresentation;
        public BattlePixelFramePlan CurrentPixelFramePlan => _currentPixelFramePlan;

        internal void PublishPixelFramePlan(BattlePixelFramePlan plan)
        {
            _currentPixelFramePlan = plan;
        }

        public void SetBattlePresentationBackend(BattlePresentationBackendMode mode)
        {
            _battlePresentation.SetMode(mode);
        }

        private readonly struct PresentationRenderOrder
        {
            public PresentationRenderOrder(RuntimeEntityHandle handle, int rank)
            {
                Handle = handle;
                Rank = rank;
            }

            public RuntimeEntityHandle Handle { get; }
            public int Rank { get; }
        }

        private bool _hasExplicitStageRuntimeSnapshot;

        public void SetExplicitStageRuntimeSnapshotForTesting(
            int stageWidth,
            int zMin,
            int zMax,
            int perspectiveNear,
            int perspectiveFar)
        {
            Runtime?.Stage?.SetSceneSnapshot(stageWidth, zMin, zMax, perspectiveNear, perspectiveFar);
            _hasExplicitStageRuntimeSnapshot = true;
        }

        private static void ResolveUnityStageRuntime(out int stageWidth, out int zMin, out int zMax, out int perspectiveNear, out int perspectiveFar)
        {
            var cfg = NTSD.App.GameConfig.Instance;
            stageWidth = cfg != null ? Mathf.Max(cfg.BattleStageWidthPx, NTSDRenderSpace.SourceScreenWidth) : 800;
            zMin = cfg != null ? cfg.BattleStageZMinPx : 180;
            zMax = cfg != null ? Mathf.Max(cfg.BattleStageZMaxPx, zMin + 1) : 350;
            perspectiveNear = cfg != null ? cfg.BattlePerspectiveNear : 0;
            perspectiveFar = cfg != null ? cfg.BattlePerspectiveFar : 0;

            BoundaryWallManager manager = BoundaryWallManager.Instance;
            if (manager != null && manager.TryGetBattleStageRuntime(out int boundaryStageWidth, out int boundaryZMin, out int boundaryZMax))
            {
                stageWidth = boundaryStageWidth;
                zMin = boundaryZMin;
                zMax = boundaryZMax;
            }
        }

        public bool IsGroundPointWalkable(Vector2 pointXY)
        {
            BoundaryWallManager manager = BoundaryWallManager.Instance;
            if (manager == null)
                return true;

            return manager.IsPointWalkable(pointXY);
        }

        public void RefreshStageRuntimeSnapshotFromScene()
        {
            if (_hasExplicitStageRuntimeSnapshot)
                return;

            ResolveUnityStageRuntime(out int stageWidth, out int zMin, out int zMax, out int perspectiveNear, out int perspectiveFar);
            Runtime?.Stage?.SetSceneSnapshot(stageWidth, zMin, zMax, perspectiveNear, perspectiveFar);
        }

        public void ClampCharacterZToStageBoundsAll()
        {
            RefreshStageRuntimeSnapshotFromScene();
            float zMin = Runtime?.Stage?.ZMin ?? 180;
            float zMax = Runtime?.Stage?.ZMax ?? 350;
            if (zMax < zMin)
                return;

            ForEachEntityByRuntimeSlot(entity =>
            {
                if (!entity.IsStageBoundedCharacter() || entity.PS == null)
                    return;

                if (entity.PS.z > zMax) entity.PS.z = zMax;
                if (entity.PS.z < zMin) entity.PS.z = zMin;
                RefreshRuntimeSnapshot(entity);
            });
        }

        public void ApplyPreFrameBoundsAll()
        {
            RefreshStageRuntimeSnapshotFromScene();
            int stageWidthPx = Runtime?.Stage?.StageWidthPx ?? 800;
            int baseStageWidthPx = Runtime?.Stage?.BaseStageWidthPx ?? 800;
            int xMaxOverride = Runtime?.Stage?.XMaxOverride ?? 0;
            int stageZMin = Runtime?.Stage?.ZMin ?? 180;
            int stageZMax = Runtime?.Stage?.ZMax ?? 350;

            float zMin = stageZMin;
            float zMax = stageZMax;
            float baseStageWidth = baseStageWidthPx;
            if (zMax < zMin || baseStageWidth <= 0f)
                return;

            ForEachEntityByRuntimeSlot(entity =>
            {
                if (entity == null || entity.PS == null)
                    return;

                entity.ApplyPreFrameZBounds(zMin, zMax);

                bool destroyed = entity.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride);
                if (!destroyed)
                    RefreshRuntimeSnapshot(entity);
            });

            ResetUnityFixedWorldRenderOffsets();
        }

        public void RenderDispatchAll(int tickIndex)
        {
            BuildPresentationRenderOrder();
            _battlePresentation.BeginFrame(this, tickIndex);
            BattlePixelFramePlan plan = BattleCentralRenderSystem.PrepareFrame(this);
            if (RequiresLegacySpriteRendererCapacityGuard(plan))
                ValidateLegacySpriteRendererPresentationCapacity(_presentationRenderOrders.Count);
            LateRendererUpdateAll(tickIndex);
        }

        internal static bool RequiresLegacySpriteRendererCapacityGuard(BattlePixelFramePlan plan)
        {
            return !plan.SuppressesLegacyMaterializers;
        }

        internal void GetPresentationEntitiesNoAlloc(List<LF2Entity> destination)
        {
            if (destination == null)
                return;

            destination.Clear();
            for (int slot = 0; slot < RuntimeSlotCapacity; slot++)
            {
                LF2Entity entity = FindEntityByRuntimeSlotIncludingDormant(slot);
                if (entity != null && IsActiveForCurrentPassInternal(entity))
                    destination.Add(entity);
            }
        }

        internal void RecordLegacyShadowProbe(LF2Entity entity, SpriteRenderer renderer)
        {
            if (!_battlePresentation.IsCapturingLegacyProbes || entity?.Runtime == null ||
                renderer == null || !renderer.enabled)
            {
                return;
            }

            int slot = entity.Runtime.SlotIndex;
            if (!TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle))
                return;

            BattleCommonVisualBinding shadowBinding =
                CharacterAnimtorManager.Instance?.CommonVisualCatalog?.Shadow;
            bool matchesCommonShadow = shadowBinding != null &&
                                       shadowBinding.MatchesSprite(renderer.sprite);
            BattleSpriteValueDescriptor descriptor = CaptureRendererDescriptor(
                renderer,
                out BattleSpriteRenderState renderState,
                matchesCommonShadow,
                BattleVisualResourceKey.CommonShadow);

            _battlePresentation.RecordLegacyProbe(new LegacyPresentationProbe(
                BattleRenderCommandType.Shadow,
                handle,
                entity.Runtime.StableId,
                -1,
                -1,
                renderer.sortingOrder,
                renderer.sortingLayerID,
                0,
                renderer.transform.position,
                CaptureRendererSpriteSize(renderer),
                renderState,
                descriptor));
        }

        internal void RecordLegacyEntityProbe(LF2Entity entity, SpriteRenderer renderer)
        {
            if (!_battlePresentation.IsCapturingLegacyProbes || entity?.Runtime == null ||
                renderer == null || !renderer.enabled)
            {
                return;
            }

            int slot = entity.Runtime.SlotIndex;
            if (!TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle))
                return;

            BattleSpriteValueDescriptor descriptor = CaptureRendererDescriptor(
                renderer,
                out BattleSpriteRenderState renderState,
                true,
                BattleVisualResourceKey.FromEntity(new BattleSpriteKey(
                    LF2Entity.ResolveCurrentDataObjectId(entity),
                    entity.GetRenderPicIndex())));
            int visualDataId = descriptor.HasLogicalResourceKey &&
                               descriptor.LogicalResourceKey.IsEntitySprite
                ? descriptor.LogicalResourceKey.EntitySpriteKey.VisualDataId
                : -1;
            int effectivePic = descriptor.HasLogicalResourceKey &&
                               descriptor.LogicalResourceKey.IsEntitySprite
                ? descriptor.LogicalResourceKey.EntitySpriteKey.EffectivePic
                : -1;

            _battlePresentation.RecordLegacyProbe(new LegacyPresentationProbe(
                BattleRenderCommandType.Entity,
                handle,
                entity.Runtime.StableId,
                visualDataId,
                effectivePic,
                renderer.sortingOrder,
                renderer.sortingLayerID,
                0,
                renderer.transform.position,
                CaptureRendererSpriteSize(renderer),
                renderState,
                descriptor));
        }

        internal void RecordLegacyHitRecordProbe(
            LF2Entity entity,
            SpriteRenderer renderer,
            int hitRecordIndex)
        {
            if (!_battlePresentation.IsCapturingLegacyProbes || entity?.Runtime == null ||
                renderer == null || !renderer.enabled)
            {
                return;
            }

            int slot = entity.Runtime.SlotIndex;
            if (!TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle))
                return;

            BattleVisualResourceKey sparkKey = default;
            bool hasSparkKey = CharacterAnimtorManager.Instance?.CommonVisualCatalog?.TryGetSparkKey(
                renderer.sprite,
                out sparkKey) == true;
            BattleSpriteValueDescriptor descriptor = CaptureRendererDescriptor(
                renderer,
                out BattleSpriteRenderState renderState,
                hasSparkKey,
                sparkKey);

            _battlePresentation.RecordLegacyProbe(new LegacyPresentationProbe(
                BattleRenderCommandType.HitRecord,
                handle,
                entity.Runtime.StableId,
                -1,
                -1,
                renderer.sortingOrder,
                renderer.sortingLayerID,
                hitRecordIndex,
                renderer.transform.position,
                CaptureRendererSpriteSize(renderer),
                renderState,
                descriptor));
        }

        private static Vector2 CaptureRendererSpriteSize(SpriteRenderer renderer)
        {
            Sprite sprite = renderer != null ? renderer.sprite : null;
            return sprite != null ? sprite.rect.size : Vector2.zero;
        }

        private static BattleSpriteValueDescriptor CaptureRendererDescriptor(
            SpriteRenderer renderer,
            out BattleSpriteRenderState renderState,
            bool hasPreferredKey = false,
            BattleVisualResourceKey preferredKey = default)
        {
            Sprite sprite = renderer != null ? renderer.sprite : null;
            Rect rect = sprite != null ? sprite.rect : Rect.zero;
            Vector2 pivot = Vector2.zero;
            if (sprite != null && rect.width > 0f && rect.height > 0f)
            {
                pivot = new Vector2(
                    sprite.pivot.x / rect.width,
                    sprite.pivot.y / rect.height);
            }

            Texture2D texture = sprite != null ? sprite.texture : null;
            Material material = renderer != null ? renderer.sharedMaterial : null;
            BattleSpriteCatalog catalog = CharacterAnimtorManager.Instance?.SpriteCatalog ??
                                          BattleSpriteCatalog.Empty;
            BattleVisualResourceKey logicalResourceKey = default;
            bool hasLogicalResourceKey;
            if (hasPreferredKey &&
                (preferredKey.Kind == BattleVisualResourceKind.CommonShadow || preferredKey.IsCommonSpark))
            {
                logicalResourceKey = preferredKey;
                hasLogicalResourceKey = true;
            }
            else
            {
                BattleSpriteKey preferredEntityKey = preferredKey.EntitySpriteKey;
                bool foundEntityKey = hasPreferredKey && preferredKey.IsEntitySprite
                    ? catalog.TryGetKey(sprite, preferredEntityKey, out BattleSpriteKey entityKey)
                    : catalog.TryGetKey(sprite, out entityKey);
                logicalResourceKey = foundEntityKey
                    ? BattleVisualResourceKey.FromEntity(entityKey)
                    : default;
                hasLogicalResourceKey = foundEntityKey;
            }
            renderState = renderer != null
                ? new BattleSpriteRenderState(
                    renderer.color,
                    renderer.flipX,
                    renderer.flipY,
                    renderer.maskInteraction,
                    BattleSpriteMaterialContract.Classify(material))
                : default;
            return hasLogicalResourceKey
                ? new BattleSpriteValueDescriptor(
                    true,
                    sprite != null,
                    sprite != null ? sprite.GetInstanceID() : 0,
                    texture != null ? texture.GetInstanceID() : 0,
                    material != null ? material.GetInstanceID() : 0,
                    rect,
                    pivot,
                    logicalResourceKey)
                : new BattleSpriteValueDescriptor(
                    true,
                    sprite != null,
                    sprite != null ? sprite.GetInstanceID() : 0,
                    texture != null ? texture.GetInstanceID() : 0,
                    material != null ? material.GetInstanceID() : 0,
                    rect,
                    pivot);
        }

        /// <summary>
        /// Publishes a dense Unity presentation order from the release renderer's
        /// active (ZInt, runtime slot) ordering. This is intentionally not part of
        /// runtime state, checksums, or collision behavior.
        /// </summary>
        internal void BuildPresentationRenderOrder()
        {
            GetPresentationEntitiesNoAlloc(_presentationRenderScratch);
            _presentationRenderScratch.Sort(PresentationOrderComparison);
            _presentationRenderOrders.Clear();

            int rank = 0;
            for (int i = 0; i < _presentationRenderScratch.Count; i++)
            {
                LF2Entity entity = _presentationRenderScratch[i];
                int slot = entity?.Runtime?.SlotIndex ?? -1;
                if (entity == null || slot < 0 ||
                    !TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle))
                {
                    continue;
                }

                _presentationRenderOrders[entity] = new PresentationRenderOrder(handle, rank);
                rank++;
            }

            _presentationRenderScratch.Clear();
        }

        internal static void ValidateLegacySpriteRendererPresentationCapacity(
            int materializedEntityCount)
        {
            if (materializedEntityCount <= LegacySpriteRendererMaxPresentationEntities)
                return;

            throw new System.InvalidOperationException(
                "Legacy SpriteRenderer presentation supports at most " +
                LegacySpriteRendererMaxPresentationEntities +
                " materialized battle entities because it reserves four sorting orders per entity. " +
                "Use the central battle renderer before exceeding this temporary legacy limit.");
        }

        internal int GetPresentationRenderSortingOrder(LF2Entity entity, int subOrder)
        {
            if (entity != null &&
                _presentationRenderOrders.TryGetValue(entity, out PresentationRenderOrder published) &&
                TryResolveRuntimeHandle(published.Handle, out LF2Entity current) &&
                ReferenceEquals(current, entity))
            {
                return checked(published.Rank * PresentationSubOrderCount +
                               Mathf.Clamp(subOrder, PresentationShadowSubOrder, PresentationHitRecordSubOrder));
            }

            // ForceRefreshPresentation can run before the normal render pass. Build
            // the same active map on demand rather than deriving a Unity order from a
            // sparse runtime slot. An unregistered/stale entity remains isolated at
            // its requested sub-order until it is published by a later render pass.
            if (entity != null && IsActiveForCurrentPass(entity))
            {
                BuildPresentationRenderOrder();
                if (_presentationRenderOrders.TryGetValue(entity, out published) &&
                    TryResolveRuntimeHandle(published.Handle, out current) &&
                    ReferenceEquals(current, entity))
                {
                    return checked(published.Rank * PresentationSubOrderCount +
                                   Mathf.Clamp(subOrder, PresentationShadowSubOrder, PresentationHitRecordSubOrder));
                }
            }

            return Mathf.Clamp(subOrder, PresentationShadowSubOrder, PresentationHitRecordSubOrder);
        }

        private static int ComparePresentationRenderOrder(LF2Entity left, LF2Entity right)
        {
            int zComparison = (left?.GetRenderZInt() ?? int.MaxValue)
                .CompareTo(right?.GetRenderZInt() ?? int.MaxValue);
            if (zComparison != 0)
                return zComparison;

            int leftSlot = left?.Runtime?.SlotIndex ?? int.MaxValue;
            int rightSlot = right?.Runtime?.SlotIndex ?? int.MaxValue;
            int slotComparison = leftSlot.CompareTo(rightSlot);
            if (slotComparison != 0)
                return slotComparison;

            return (left?.StableId ?? int.MaxValue).CompareTo(right?.StableId ?? int.MaxValue);
        }

        internal void ResetUnityFixedWorldRenderOffsets()
        {
            // Unity battle scenes use fixed world coordinates. Keep entity, shadow,
            // and spark presentation independent from character-driven camera math.
            _cameraX = 0;
            _cameraVel = 0;
            GetAllEntities(_entityScratch);
            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                if (entity?.Runtime == null)
                    continue;

                entity.Runtime.RenderOffsetX = 0f;
            }

            _entityScratch.Clear();
        }

        private List<ISimObject> BuildRendererSnapshot()
        {
            _rendererSnapshotScratch.Clear();
            if (_buckets.TryGetValue(SimOrderConstants.Renderer, out Bucket bucket))
            {
                bucket.EnsureSorted(GetRuntimeStableId);
                for (int i = 0; i < bucket.items.Count; i++)
                {
                    if (bucket.items[i] is LF2Entity) continue;
                    if (bucket.items[i] is LF2ObjectRenderer)
                        _rendererSnapshotScratch.Add(bucket.items[i]);
                }
            }

            return _rendererSnapshotScratch;
        }

        private void LateRendererUpdateAll(int tickIndex)
        {
            var snapshot = BuildRendererSnapshot();
            for (int i = 0; i < snapshot.Count; i++)
            {
                ISimObject obj = snapshot[i];
                if (obj == null || !IsActiveForCurrentPass(obj))
                    continue;

                obj.SimLateTick(tickIndex);
            }
        }

        public void UpdateBattleResultsFlow()
        {
            BattleRuntimeState battle = Runtime;
            if (battle?.Match?.BattleGameModeId != 1)
                return;

            battle.Results ??= new BattleResultsRuntimeState();
            BattleResultsRuntimeState results = battle.Results;
            if (results.IsActive)
                return;

            BattleSlotRuntimeState[] rosterSlots = battle.Roster?.Slots;
            if (rosterSlots == null)
                return;

            int[] teamIds = { -1, -1 };
            int[] alive = new int[2];
            int teamCount = 0;
            int slotCount = rosterSlots.Length < 8 ? rosterSlots.Length : 8;

            for (int slotIndex = 0; slotIndex < slotCount; slotIndex++)
            {
                BattleSlotRuntimeState rosterSlot = rosterSlots[slotIndex];
                if (rosterSlot == null)
                    continue;

                LF2Entity entity = FindEntityByRuntimeSlotIncludingDormant(rosterSlot.RuntimeSlotIndex);
                if (entity == null ||
                    entity.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                {
                    continue;
                }

                // Authority GameTick keeps a fixed 0..7 slot in the result scan and
                // skips only when the slot state is dormant and the bound entity is
                // inactive. An active entity must remain eligible even when its
                // roster metadata has already been marked inactive.
                if (!rosterSlot.Active && !IsActiveForCurrentPass(entity))
                    continue;

                int team = entity.RelationTeam != 0 ? entity.RelationTeam : rosterSlot.Team;
                if (team == 0)
                    continue;

                int bucket = -1;
                for (int i = 0; i < teamCount; i++)
                {
                    if (teamIds[i] == team)
                    {
                        bucket = i;
                        break;
                    }
                }

                if (bucket < 0 && teamCount < teamIds.Length)
                {
                    bucket = teamCount;
                    teamIds[teamCount++] = team;
                }

                if (bucket >= 0 && IsActiveForCurrentPass(entity) && entity.Health != null && entity.Health.HP > 0)
                    alive[bucket]++;
            }

            if (alive[0] > 0 && alive[1] > 0)
                results.HadBoth = true;

            if (!results.HadBoth || teamCount < 2)
                return;

            results.EnsureTeamIds();
            if (alive[0] > 0 && alive[1] > 0)
            {
                results.BattleEndPhase = 0;
                results.PendingWinner = -2;
                results.TeamCount = teamCount;
                results.TeamIds[0] = teamIds[0];
                results.TeamIds[1] = teamIds[1];
                return;
            }

            int decidedWinner = alive[0] > 0 ? 0 : alive[1] > 0 ? 1 : -1;
            if (results.BattleEndPhase == 0)
            {
                results.BattleEndPhase = 1;
                results.PendingWinner = decidedWinner;
            }
            else
            {
                results.BattleEndPhase++;
            }

            results.TeamCount = teamCount;
            results.TeamIds[0] = teamIds[0];
            results.TeamIds[1] = teamIds[1];

            if (results.BattleEndPhase >= 11)
                results.ActivateSummary(results.PendingWinner, teamCount, teamIds[0], teamIds[1]);
        }
    }
}


--- File: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs ---
using System;
using System.Threading;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NTSD.Animation.Rendering
{
    public sealed class BattleCentralRuntimeDiagnostics
    {
        public BattlePresentationBackendMode RequestedMode { get; internal set; }
        public BattlePresentationBackendMode EffectivePixelMode { get; internal set; }
        public bool FeatureAvailable { get; internal set; }
        public bool MaterialAvailable { get; internal set; }
        public bool FrameAvailable { get; internal set; }
        public bool AllCategoryOwnershipReady { get; internal set; }
        public bool CommonShadowBindingReady { get; internal set; }
        public bool CommonSparkBindingReady { get; internal set; }
        public bool SubmissionReady { get; internal set; }
        public bool SubmittedPixelsLastFrame { get; internal set; }
        public int SubmissionCount { get; internal set; }
        public int LastSubmissionDrawCount { get; internal set; }
        public int SimulationTick { get; internal set; }
        public int DisplayTick { get; internal set; }
        public bool IsStale { get; internal set; }
        public string Reason { get; internal set; } = string.Empty;
        public string RefusalReason { get; internal set; } = string.Empty;
    }

    public static class BattleCentralRenderSystem
    {
        private const int RendererObservationMaxAgeFrames = 2;

        private static readonly BattleDynamicMeshBackend[] Backends =
        {
            new BattleDynamicMeshBackend(),
            new BattleDynamicMeshBackend(),
        };
        private static readonly BattleCentralSubmission[] SlotSubmissions =
        {
            new BattleCentralSubmission(Backends[0]),
            new BattleCentralSubmission(Backends[1]),
        };
        private static readonly BattleDynamicMeshBackend EmptyBackend = new BattleDynamicMeshBackend();
        private static readonly BattleCatalogCentralResourceResolver CatalogResolver =
            new BattleCatalogCentralResourceResolver();
        private static readonly BattleCatalogCentralResourceResolver DiagnosticCatalogResolver =
            new BattleCatalogCentralResourceResolver();
        private static readonly BattleCentralRuntimeDiagnostics RuntimeDiagnostics =
            new BattleCentralRuntimeDiagnostics();

        private static FeatureRegistration[] featureRegistrations = new FeatureRegistration[4];
        private static int featureRegistrationCount;
        private static BattleRenderFeature featureOwner;
        private static Material featureMaterial;
        private static Material featureArrayMaterial;
        private static BattleRenderFeature observedFeatureOwner;
        private static ScriptableRenderer observedRenderer;
        private static Camera observedWorldCamera;
        private static int observedUnityFrame = -1;
        private static BattlePresentationBackendMode requestedMode = BattlePresentationBackendMode.CentralOnly;
        private static BattleCentralDrawMode drawMode = BattleCentralDrawMode.OrderedChunks;
        private static BattleCentralDrawMode serializedDrawMode = BattleCentralDrawMode.OrderedChunks;
        private static BattleDrawPolicyDecision drawPolicyDecision = new BattleDrawPolicyDecision(
            BattleDrawPolicyMode.Auto,
            BattleCentralDrawMode.OrderedChunks,
            string.Empty);
        private static SimulationWorld publishedPlanWorld;
        private static int publishedPlanGeneration;
        private static BattleDynamicMeshBackend lastBuiltBackend = Backends[0];
        private static CharacterAnimtorManager diagnosticCatalogManager;
        private static BattleSpriteCatalog diagnosticCatalog = BattleSpriteCatalog.Empty;
        private static int nextGeneration;
        private static AttemptedBuildDiagnostics lastAttemptedBuildDiagnostics;

        public static BattleDynamicMeshBackend MeshBackend => lastBuiltBackend;
        public static BattleCentralRuntimeDiagnostics Diagnostics => RuntimeDiagnostics;
        public static BattlePixelFramePlan CurrentPixelFramePlan
        {
            get
            {
                SimulationWorld world = Volatile.Read(ref publishedPlanWorld);
                BattlePixelFramePlan plan = world != null
                    ? world.CurrentPixelFramePlan
                    : default;
                return plan.IsValid && plan.Generation == Volatile.Read(ref publishedPlanGeneration)
                    ? plan
                    : default;
            }
        }
        internal static int RegisteredFeatureCount => featureRegistrationCount;
        internal static BattleRenderFeature RegisteredFeature => featureOwner;
        public static Material RegisteredFeatureMaterialForAcceptance => featureMaterial;
        public static Material RegisteredFeatureArrayMaterialForAcceptance => featureArrayMaterial;
        internal static Material RegisteredFeatureMaterial => featureMaterial;
        internal static Material RegisteredFeatureArrayMaterial => featureArrayMaterial;
        internal static BattleCentralDrawMode RegisteredFeatureDrawMode => drawMode;
        public static BattleDrawPolicyDecision DrawPolicyDecision => drawPolicyDecision;

        internal static void RegisterFeature(
            BattleRenderFeature owner,
            Material material,
            BattleCentralDrawMode mode)
        {
            RegisterFeature(owner, material, null, mode);
        }

        internal static void RegisterFeature(
            BattleRenderFeature owner,
            Material material,
            Material arrayMaterial,
            BattleCentralDrawMode mode)
        {
            if (owner == null)
                return;

            int existingIndex = FindRegistration(owner);
            if (existingIndex >= 0)
                RemoveRegistrationAt(existingIndex);
            EnsureRegistrationCapacity(featureRegistrationCount + 1);
            featureRegistrations[featureRegistrationCount++] =
                new FeatureRegistration(owner, material, arrayMaterial, mode);
            ApplyActiveRegistration();
        }

        internal static void UnregisterFeature(BattleRenderFeature owner)
        {
            int index = FindRegistration(owner);
            if (index < 0)
                return;
            RemoveRegistrationAt(index);
            ApplyActiveRegistration();
        }

        internal static void RecordFeatureCameraAvailability(
            BattleRenderFeature owner,
            ScriptableRenderer renderer,
            Camera camera,
            CameraRenderType renderType)
        {
            if (owner == null || owner != featureOwner || renderer == null ||
                !IsWorldRenderCamera(camera, renderType, NTSDRenderSpace.WorldCamera))
            {
                return;
            }

            observedFeatureOwner = owner;
            observedRenderer = renderer;
            observedWorldCamera = camera;
            observedUnityFrame = Time.frameCount;
        }

        public static BattlePixelFramePlan PrepareFrame(SimulationWorld world)
        {
            BattlePresentationBackendMode mode =
                world?.BattlePresentation?.Mode ?? BattlePresentationBackendMode.CentralOnly;
            BattlePresentationFrame frame = world?.BattlePresentation?.PublishedFrame;
            int simulationTick = frame?.TickIndex ?? world?.CurrentTickIndex ?? 0;
            BattlePixelFramePlan current = world != null ? world.CurrentPixelFramePlan : default;
            if (current.IsValid && ReferenceEquals(current.World, world) &&
                current.SimulationTick == simulationTick &&
                current.RequestedMode == mode && CurrentPixelFramePlan.Generation == current.Generation)
            {
                return current;
            }

            requestedMode = mode;
            ResetPerFrameDiagnostics(mode, frame != null);
            lastAttemptedBuildDiagnostics = default;

            if (world == null)
            {
                return CommitCentralFailurePlan(
                    null,
                    simulationTick,
                    "SimulationWorld is unavailable.");
            }
            if (mode == BattlePresentationBackendMode.LegacyOnly)
            {
                return CommitLegacyPlan(
                    world,
                    frame,
                    mode,
                    simulationTick,
                    "LegacyOnly does not build or submit central geometry.");
            }

            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            BattleSpriteCatalog catalog = manager != null
                ? manager.SpriteCatalog
                : BattleSpriteCatalog.Empty;
            BattleCommonVisualCatalog commonVisualCatalog = manager != null
                ? manager.CommonVisualCatalog
                : BattleCommonVisualCatalog.Empty;
            RuntimeDiagnostics.CommonShadowBindingReady = commonVisualCatalog.IsShadowValid;
            RuntimeDiagnostics.CommonSparkBindingReady = commonVisualCatalog.IsSparkValid;

            if (!TryGetReusableBackend(out int backendIndex, out BattleDynamicMeshBackend stagingBackend))
            {
                const string reason =
                    "No central staging backend is available because the previous submission is still leased.";
                return mode == BattlePresentationBackendMode.CentralOnly
                    ? CommitCentralFailurePlan(world, simulationTick, reason)
                    : CommitLegacyPlan(world, frame, mode, simulationTick, reason);
            }

            bool rendererReady = TryValidateActiveRenderer(out string rendererReason);
            bool frameReady = frame != null;
            bool commonReady = commonVisualCatalog.IsComplete;
            if (mode == BattlePresentationBackendMode.CentralOnly &&
                (!rendererReady || !frameReady || !commonReady))
            {
                string reason = !rendererReady
                    ? rendererReason
                    : !frameReady
                        ? "No current immutable presentation frame is available."
                        : "The common shadow, spark, or WORDS catalog is incomplete.";
                return CommitCentralFailurePlan(world, simulationTick, reason);
            }

            try
            {
                BattleCentralSubmission stagingSubmission = SlotSubmissions[backendIndex];
                BattlePresentationFrame buildFrame = frame != null
                    ? stagingSubmission.CaptureFrame(frame)
                    : null;
                BattleSpriteCatalog buildCatalog = buildFrame?.BoundCatalog ?? catalog;
                BattleCommonVisualCatalog buildCommonVisualCatalog =
                    buildFrame?.CommonVisualCatalog ?? commonVisualCatalog;
                CatalogResolver.Configure(
                    buildCatalog,
                    buildCommonVisualCatalog,
                    featureMaterial,
                    featureArrayMaterial);
                stagingBackend.Build(buildFrame, CatalogResolver, drawMode);
                lastBuiltBackend = stagingBackend;
                lastAttemptedBuildDiagnostics = AttemptedBuildDiagnostics.Capture(
                    stagingBackend,
                    simulationTick);
            }
            catch (Exception exception)
            {
                stagingBackend.Clear();
                string reason =
                    $"Central geometry build failed: {exception.GetType().Name}: {exception.Message}";
                return mode == BattlePresentationBackendMode.CentralOnly
                    ? CommitCentralFailurePlan(world, simulationTick, reason)
                    : CommitLegacyPlan(world, frame, mode, simulationTick, reason);
            }

            bool allCategoryOwnershipReady = frameReady && commonReady &&
                                             frame.OverlayUnsupportedCount == 0 &&
                                             stagingBackend.Diagnostics.UnsupportedCategoryCount == 0 &&
                                             stagingBackend.Diagnostics.UnsupportedRenderStateCount == 0 &&
                                             stagingBackend.Diagnostics.UnresolvedCommandCount == 0;
            RuntimeDiagnostics.AllCategoryOwnershipReady = allCategoryOwnershipReady;

            if (mode == BattlePresentationBackendMode.CentralShadowBuild)
            {
                BindDiagnosticCatalog(manager, stagingBackend.BuiltFrame?.BoundCatalog ?? catalog);
                return CommitLegacyPlan(
                    world,
                    frame,
                    mode,
                    simulationTick,
                    "CentralShadowBuild builds diagnostics but fixes pixel ownership to Legacy.",
                    true);
            }

            if (!allCategoryOwnershipReady)
            {
                return CommitCentralFailurePlan(
                    world,
                    simulationTick,
                    BuildOwnershipRefusalReason(stagingBackend));
            }

            ReleaseDiagnosticCatalogBinding();
            int generation = NextGeneration();
            BattleCentralSubmission submission = SlotSubmissions[backendIndex];
            BattlePresentationFrame capturedFrame = stagingBackend.BuiltFrame;
            submission.Publish(
                world,
                capturedFrame,
                simulationTick,
                generation,
                manager,
                capturedFrame.BoundCatalog);
            var plan = new BattlePixelFramePlan(
                world,
                capturedFrame,
                mode,
                BattlePixelFrameOwner.Central,
                simulationTick,
                simulationTick,
                generation,
                false,
                string.Empty,
                submission);
            PublishPlan(world, plan);
            RuntimeDiagnostics.SubmissionReady = true;
            RuntimeDiagnostics.EffectivePixelMode = BattlePresentationBackendMode.CentralOnly;
            SetPlanDiagnostics(plan);
            RuntimeDiagnostics.RefusalReason = string.Empty;
            return plan;
        }

        public static bool CentralOnlyOwnsPixels(SimulationWorld world)
        {
            return world != null &&
                   world.BattlePresentation.Mode == BattlePresentationBackendMode.CentralOnly;
        }

        public static bool ShouldSuppressLegacyMaterializers(SimulationWorld world)
        {
            return CentralOnlyOwnsPixels(world);
        }

        public static bool ShouldUseCentralPixels(SimulationWorld world)
        {
            BattlePixelFramePlan plan = world != null ? world.CurrentPixelFramePlan : default;
            BattlePixelFramePlan globalPlan = CurrentPixelFramePlan;
            BattleCentralSubmission submission = plan.Submission;
            return plan.IsValid && globalPlan.IsValid && plan.Generation == globalPlan.Generation &&
                   ReferenceEquals(plan.World, world) &&
                   plan.Owner == BattlePixelFrameOwner.Central &&
                   plan.RequestedMode == BattlePresentationBackendMode.CentralOnly &&
                   submission != null &&
                   !submission.IsRetired && ReferenceEquals(submission.World, world) &&
                   ReferenceEquals(submission.CapturedFrame, plan.CapturedFrame) &&
                   submission.IsBackendBuildCurrent &&
                   submission.TickIndex == plan.DisplayTick &&
                   submission.Generation == plan.Generation;
        }

        internal static bool TryAcquireSubmission(
            Camera camera,
            CameraRenderType renderType,
            out BattleCentralSubmission.BattleCentralSubmissionLease lease)
        {
            lease = default;
            BattlePixelFramePlan plan = CurrentPixelFramePlan;
            SimulationWorld world = plan.World;
            if (!CanRenderCamera(camera, renderType, NTSDRenderSpace.WorldCamera) ||
                !ShouldUseCentralPixels(world))
            {
                return false;
            }

            if (!plan.Submission.TryAcquire(out lease))
                return false;
            if (ShouldUseCentralPixels(world) &&
                lease.Generation == plan.Generation && lease.TickIndex == plan.TickIndex)
            {
                return true;
            }

            lease.Dispose();
            lease = default;
            return false;
        }

        internal static bool IsSubmissionLeaseCurrent(
            BattleCentralSubmission.BattleCentralSubmissionLease lease)
        {
            BattleCentralSubmission submission = lease.Submission;
            BattlePixelFramePlan plan = CurrentPixelFramePlan;
            return submission != null && plan.IsValid &&
                   ReferenceEquals(plan.Submission, submission) &&
                   plan.Generation == lease.Generation && plan.TickIndex == lease.TickIndex &&
                   ShouldUseCentralPixels(plan.World);
        }

        internal static BattlePixelFramePlan PublishReadyCentralPlanForSelfCheck(
            SimulationWorld world)
        {
            if (!Application.isEditor)
                throw new InvalidOperationException("Central publication self-check hook is editor-only.");
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            BattlePresentationFrame frame = world.BattlePresentation.PublishedFrame;
            int tickIndex = frame?.TickIndex ?? 0;
            if (world.BattlePresentation.Mode != BattlePresentationBackendMode.CentralOnly || frame == null)
            {
                return world.BattlePresentation.Mode == BattlePresentationBackendMode.CentralOnly
                    ? CommitCentralFailurePlan(
                        world,
                        tickIndex,
                        "Self-check central publication requires a current CentralOnly frame.")
                    : CommitLegacyPlan(
                        world,
                        frame,
                        world.BattlePresentation.Mode,
                        tickIndex,
                        "Self-check central publication requires a current CentralOnly frame.");
            }
            if (!TryGetReusableBackend(out int backendIndex, out BattleDynamicMeshBackend backend))
            {
                return CommitCentralFailurePlan(
                    world,
                    tickIndex,
                    "Self-check central publication found no reusable backend slot.");
            }

            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            BattleCentralSubmission submission = SlotSubmissions[backendIndex];
            BattlePresentationFrame capturedFrame = submission.CaptureFrame(frame);
            CatalogResolver.Configure(
                capturedFrame.BoundCatalog,
                capturedFrame.CommonVisualCatalog,
                featureMaterial,
                featureArrayMaterial);
            backend.Build(capturedFrame, CatalogResolver, drawMode);
            lastBuiltBackend = backend;
            lastAttemptedBuildDiagnostics = AttemptedBuildDiagnostics.Capture(backend, tickIndex);
            int generation = NextGeneration();
            submission.Publish(
                world,
                capturedFrame,
                tickIndex,
                generation,
                manager,
                capturedFrame.BoundCatalog);
            var plan = new BattlePixelFramePlan(
                world,
                capturedFrame,
                BattlePresentationBackendMode.CentralOnly,
                BattlePixelFrameOwner.Central,
                tickIndex,
                tickIndex,
                generation,
                false,
                string.Empty,
                submission);
            PublishPlan(world, plan);
            RuntimeDiagnostics.RequestedMode = BattlePresentationBackendMode.CentralOnly;
            RuntimeDiagnostics.EffectivePixelMode = BattlePresentationBackendMode.CentralOnly;
            RuntimeDiagnostics.FrameAvailable = true;
            RuntimeDiagnostics.AllCategoryOwnershipReady = true;
            RuntimeDiagnostics.SubmissionReady = true;
            SetPlanDiagnostics(plan);
            RuntimeDiagnostics.RefusalReason = string.Empty;
            return plan;
        }

        internal static BattlePixelFramePlan PublishBuiltCentralPlanForSelfCheck(
            SimulationWorld world)
        {
            if (!Application.isEditor)
                throw new InvalidOperationException("Built central publication self-check hook is editor-only.");
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            BattlePresentationFrame frame = world.BattlePresentation.PublishedFrame;
            BattlePresentationFrame builtFrame = lastBuiltBackend.BuiltFrame;
            if (frame == null || builtFrame == null || frame.TickIndex != builtFrame.TickIndex)
                throw new InvalidOperationException("The self-check requires the current immutable frame tick to be built.");

            int backendIndex = Array.IndexOf(Backends, lastBuiltBackend);
            if (backendIndex < 0)
                throw new InvalidOperationException("The built backend is not a publishable central slot.");
            BattleCentralSubmission submission = SlotSubmissions[backendIndex];
            if (!submission.IsReusable)
                throw new InvalidOperationException("The built backend submission slot is still leased.");

            int generation = NextGeneration();
            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            submission.Publish(
                world,
                builtFrame,
                builtFrame.TickIndex,
                generation,
                manager,
                builtFrame.BoundCatalog);
            var plan = new BattlePixelFramePlan(
                world,
                builtFrame,
                BattlePresentationBackendMode.CentralOnly,
                BattlePixelFrameOwner.Central,
                builtFrame.TickIndex,
                builtFrame.TickIndex,
                generation,
                false,
                string.Empty,
                submission);
            PublishPlan(world, plan);
            RuntimeDiagnostics.RequestedMode = BattlePresentationBackendMode.CentralOnly;
            RuntimeDiagnostics.EffectivePixelMode = BattlePresentationBackendMode.CentralOnly;
            RuntimeDiagnostics.FrameAvailable = true;
            RuntimeDiagnostics.AllCategoryOwnershipReady = true;
            RuntimeDiagnostics.SubmissionReady = true;
            SetPlanDiagnostics(plan);
            RuntimeDiagnostics.RefusalReason = string.Empty;
            return plan;
        }

        internal static BattlePixelFramePlan PublishStaleCentralPlanForSelfCheck(
            SimulationWorld world,
            int simulationTick)
        {
            if (!Application.isEditor)
                throw new InvalidOperationException("Stale central publication self-check hook is editor-only.");
            if (world == null)
                throw new ArgumentNullException(nameof(world));
            BattlePixelFramePlan current = world.CurrentPixelFramePlan;
            if (!current.IsValid || current.Owner != BattlePixelFrameOwner.Central ||
                current.Submission == null || current.Submission.IsRetired)
            {
                throw new InvalidOperationException("The self-check requires a live central submission.");
            }
            return CommitCentralFailurePlan(world, simulationTick, "Self-check retained last-good frame.");
        }

        public static bool CanRenderCamera(Camera camera, CameraRenderType renderType, Camera worldCamera)
        {
            return CanRenderCamera(
                camera,
                renderType,
                worldCamera,
                camera != null ? camera.cameraType : CameraType.Game,
                Application.isPlaying);
        }

        internal static bool CanRenderCamera(
            Camera camera,
            CameraRenderType renderType,
            Camera worldCamera,
            CameraType cameraType,
            bool isPlaying)
        {
            if (renderType != CameraRenderType.Base || camera == null || worldCamera == null)
                return false;
            if (camera == worldCamera)
                return true;
#if UNITY_EDITOR
            return isPlaying && cameraType == CameraType.SceneView;
#else
            return false;
#endif
        }

        private static bool IsWorldRenderCamera(
            Camera camera,
            CameraRenderType renderType,
            Camera worldCamera)
        {
            return camera != null && worldCamera != null && camera == worldCamera &&
                   renderType == CameraRenderType.Base;
        }

        internal static void RecordSubmission(
            BattleCentralSubmission.BattleCentralSubmissionLease lease,
            int drawCount)
        {
            if (!lease.IsValid)
                return;
            RecordSubmission(lease.Submission, lease.Generation, lease.TickIndex, drawCount);
        }

#if UNITY_EDITOR
        internal static void RecordSubmissionForSelfCheck(
            BattlePixelFramePlan plan,
            int drawCount)
        {
            if (!Application.isEditor)
                throw new InvalidOperationException("Central submission recording self-check hook is editor-only.");
            BattlePixelFramePlan current = CurrentPixelFramePlan;
            if (!plan.IsValid || plan.Submission == null ||
                !current.IsValid || current.Generation != plan.Generation ||
                !ReferenceEquals(current.Submission, plan.Submission))
            {
                throw new InvalidOperationException(
                    "The self-check can record only the current central submission generation.");
            }
            RecordSubmission(plan.Submission, plan.Generation, plan.DisplayTick, drawCount);
        }
#endif

        private static void RecordSubmission(
            BattleCentralSubmission submission,
            int generation,
            int tickIndex,
            int drawCount)
        {
            if (submission == null ||
                !submission.TryRecordExecutedDraws(generation, tickIndex, drawCount))
            {
                return;
            }

            RuntimeDiagnostics.SubmissionCount += drawCount;
            BattlePixelFramePlan plan = CurrentPixelFramePlan;
            if (!plan.IsValid || !ReferenceEquals(plan.Submission, submission) ||
                plan.Generation != generation || plan.DisplayTick != tickIndex)
            {
                return;
            }

            int executedDrawCount = submission.GetExecutedDrawCount(generation, tickIndex);
            RuntimeDiagnostics.SubmittedPixelsLastFrame = executedDrawCount > 0;
            RuntimeDiagnostics.LastSubmissionDrawCount = executedDrawCount;
        }

        public static BattleRenderingDiagnosticReport CaptureDiagnosticReport()
        {
            BattleAtlasDiagnosticInputs atlasInputs = CharacterAnimtorManager.Instance?.LastAtlasDiagnosticInputs;
            if (atlasInputs == null)
                return null;

            return CaptureDiagnosticReportForSelfCheck(atlasInputs);
        }

        internal static BattleRenderingDiagnosticReport CaptureDiagnosticReportForSelfCheck(
            BattleAtlasDiagnosticInputs atlasInputs)
        {
            if (atlasInputs == null)
                throw new ArgumentNullException(nameof(atlasInputs));
            BattlePixelFramePlan plan = CurrentPixelFramePlan;
            BattleCentralBuildDiagnostics build = null;
            AttemptedBuildDiagnostics attempted = default;
            BattlePresentationFrame reportFrame = null;
            int submissionDrawCount = 0;
            bool submissionBuildCurrent = false;

            if (plan.IsValid && plan.Submission != null && !plan.Submission.IsRetired &&
                plan.Submission.Generation == plan.Generation &&
                plan.Submission.TickIndex == plan.DisplayTick &&
                ReferenceEquals(plan.Submission.CapturedFrame, plan.CapturedFrame))
            {
                reportFrame = plan.Submission.CapturedFrame;
                submissionBuildCurrent = plan.Submission.IsBackendBuildCurrent;
                if (submissionBuildCurrent)
                {
                    build = plan.Submission.Backend.Diagnostics;
                    submissionDrawCount = plan.Submission.GetExecutedDrawCount(
                        plan.Generation,
                        plan.DisplayTick);
                }
            }
            else if (plan.IsValid &&
                     plan.RequestedMode == BattlePresentationBackendMode.CentralShadowBuild &&
                     lastBuiltBackend?.BuiltFrame != null &&
                     lastBuiltBackend.Diagnostics.TickIndex == plan.DisplayTick)
            {
                build = lastBuiltBackend.Diagnostics;
                reportFrame = lastBuiltBackend.BuiltFrame;
                submissionBuildCurrent = true;
            }
            else if (plan.IsValid && lastAttemptedBuildDiagnostics.IsValid &&
                     lastAttemptedBuildDiagnostics.SimulationTick == plan.SimulationTick)
            {
                attempted = lastAttemptedBuildDiagnostics;
                reportFrame = attempted.Frame;
                submissionBuildCurrent = attempted.IsValid;
            }

            int sourceCommandCount = build != null
                ? build.SourceCommandCount
                : attempted.IsValid ? attempted.SourceCommandCount : 0;
            int resolvedCommandCount = build != null
                ? build.ResolvedCommandCount
                : attempted.IsValid ? attempted.ResolvedCommandCount : 0;
            int unresolvedCommandCount = build != null
                ? build.UnresolvedCommandCount
                : attempted.IsValid ? attempted.UnresolvedCommandCount : 0;
            int unsupportedCategoryCount = build != null
                ? build.UnsupportedCategoryCount
                : attempted.IsValid ? attempted.UnsupportedCategoryCount : 0;
            int unsupportedRenderStateCount = build != null
                ? build.UnsupportedRenderStateCount
                : attempted.IsValid ? attempted.UnsupportedRenderStateCount : 0;
            int activeChunkCount = build != null
                ? build.ActiveChunkCount
                : attempted.IsValid ? attempted.ActiveChunkCount : 0;
            int segmentCount = build != null
                ? build.SegmentCount
                : attempted.IsValid ? attempted.SegmentCount : 0;
            int buildTick = build != null
                ? build.TickIndex
                : attempted.IsValid ? attempted.BuildTick : -1;
            int firstUnresolvedCommandIndex = build != null
                ? build.FirstUnresolvedCommandIndex
                : attempted.IsValid ? attempted.FirstUnresolvedCommandIndex : -1;
            BattleRenderCommandType firstUnresolvedCommandType = build != null
                ? build.FirstUnresolvedCommandType
                : attempted.FirstUnresolvedCommandType;
            BattleCentralResourceStatus firstUnresolvedStatus = build != null
                ? build.FirstUnresolvedStatus
                : attempted.FirstUnresolvedStatus;
            return new BattleRenderingDiagnosticReport(
                atlasInputs,
                drawPolicyDecision,
                sourceCommandCount,
                resolvedCommandCount,
                unresolvedCommandCount,
                unsupportedCategoryCount,
                activeChunkCount,
                segmentCount,
                submissionDrawCount,
                plan.IsValid ? plan.RequestedMode : RuntimeDiagnostics.RequestedMode,
                RuntimeDiagnostics.EffectivePixelMode,
                reportFrame?.EntityCount ?? 0,
                plan.IsValid ? plan.Generation : 0,
                buildTick,
                plan.IsValid ? plan.SimulationTick : -1,
                plan.IsValid ? plan.DisplayTick : -1,
                plan.IsValid && plan.IsStale,
                plan.IsValid ? plan.Reason : RuntimeDiagnostics.RefusalReason,
                submissionBuildCurrent,
                unsupportedRenderStateCount,
                firstUnresolvedCommandIndex,
                firstUnresolvedCommandType,
                firstUnresolvedStatus);
        }

        public static BattleCentralEntityDiagnostic CaptureEntityDiagnostic(
            SimulationWorld world,
            RuntimeEntityHandle handle,
            BattleRenderCommandType commandType = BattleRenderCommandType.Entity)
        {
            if (world == null || !handle.IsValid ||
                !world.TryGetRuntimeSlotReadOnlyView(handle.Slot, out RuntimeSlotTable.ReadOnlySlotView slotView))
            {
                return CreateEntityDiagnostic(
                    BattleCentralEntityDiagnosticReason.InvalidRuntimeHandle,
                    handle,
                    commandType);
            }
            if (!slotView.Claimed || slotView.Generation != handle.Generation)
            {
                return CreateEntityDiagnostic(
                    BattleCentralEntityDiagnosticReason.GenerationMismatch,
                    handle,
                    commandType);
            }

            BattlePixelFramePlan plan = world.CurrentPixelFramePlan;
            BattlePresentationFrame frame = plan.RequestedMode ==
                                                BattlePresentationBackendMode.CentralShadowBuild &&
                                            lastBuiltBackend.BuiltFrame != null
                ? lastBuiltBackend.BuiltFrame
                : plan.CapturedFrame ?? world.BattlePresentation.PublishedFrame;
            if (frame == null || !TryFindSnapshot(frame, handle, out BattlePresentationEntitySnapshot snapshot))
            {
                return CreateEntityDiagnostic(
                    BattleCentralEntityDiagnosticReason.MissingSnapshotEntity,
                    handle,
                    commandType);
            }

            if (!TryFindCommand(frame, handle, commandType, out int commandIndex, out BattleRenderCommand command))
            {
                BattleCentralEntityDiagnosticReason reason =
                    commandType == BattleRenderCommandType.Entity && !snapshot.EntityVisible ||
                    commandType == BattleRenderCommandType.Shadow && !snapshot.ShadowVisible
                        ? BattleCentralEntityDiagnosticReason.PresentationVisibilityFalse
                        : BattleCentralEntityDiagnosticReason.CommandSuppressed;
                return CreateEntityDiagnostic(reason, handle, commandType, snapshot, true);
            }

            if (!command.RenderState.IsSupported)
            {
                return CreateEntityDiagnostic(
                    BattleCentralEntityDiagnosticReason.UnsupportedRenderState,
                    handle,
                    commandType,
                    snapshot,
                    true,
                    command,
                    true,
                    commandIndex);
            }

            BattleCentralEntityDiagnosticReason resourceReason = ResolveDiagnosticResource(
                frame,
                command,
                out BattleCentralResolvedResource resource);
            if (resourceReason != BattleCentralEntityDiagnosticReason.None)
            {
                return CreateEntityDiagnostic(
                    resourceReason,
                    handle,
                    commandType,
                    snapshot,
                    true,
                    command,
                    true,
                    commandIndex);
            }

            BattleDynamicMeshBackend backend = plan.Submission != null &&
                                                ReferenceEquals(plan.Submission.CapturedFrame, frame)
                ? plan.Submission.Backend
                : ReferenceEquals(lastBuiltBackend.BuiltFrame, frame)
                    ? lastBuiltBackend
                    : null;
            int segmentIndex = FindSegmentIndex(backend, commandIndex);
            int chunkIndex = segmentIndex >= 0 ? backend.GetSegment(segmentIndex).ChunkIndex : -1;
            bool backendBuildCurrent = plan.Submission == null ||
                                       plan.Submission.IsBackendBuildCurrent;
            bool submissionStructurallyCurrent = backendBuildCurrent &&
                                                 plan.Owner == BattlePixelFrameOwner.Central &&
                                                 plan.Submission != null &&
                                                 !plan.Submission.IsRetired &&
                                                 ReferenceEquals(plan.CapturedFrame, frame) &&
                                                 ReferenceEquals(plan.Submission.Backend, backend) &&
                                                 segmentIndex >= 0;
            bool submitted = submissionStructurallyCurrent &&
                             plan.Submission.GetExecutedDrawCount(
                                 plan.Generation,
                                 plan.DisplayTick) > 0;
            return CreateEntityDiagnostic(
                !backendBuildCurrent
                    ? BattleCentralEntityDiagnosticReason.BackendMutationMismatch
                    : !submitted
                        ? BattleCentralEntityDiagnosticReason.NotSubmitted
                        : plan.IsStale
                            ? BattleCentralEntityDiagnosticReason.StalePlan
                            : BattleCentralEntityDiagnosticReason.None,
                handle,
                commandType,
                snapshot,
                true,
                command,
                true,
                commandIndex,
                resource,
                true,
                segmentIndex,
                chunkIndex,
                submitted);
        }

#if UNITY_EDITOR
        internal static BattleCentralEntityDiagnosticReason CaptureResourceReasonForSelfCheck(
            BattlePresentationFrame frame,
            in BattleRenderCommand command)
        {
            if (!command.RenderState.IsSupported)
                return BattleCentralEntityDiagnosticReason.UnsupportedRenderState;
            return ResolveDiagnosticResource(frame, command, out _);
        }
#endif

        public static BattleCentralEntityDiagnostic CaptureEntityDiagnosticBySlot(
            SimulationWorld world,
            int runtimeSlot,
            BattleRenderCommandType commandType = BattleRenderCommandType.Entity)
        {
            if (world == null ||
                !world.TryGetRuntimeSlotReadOnlyView(runtimeSlot, out RuntimeSlotTable.ReadOnlySlotView view) ||
                !view.Claimed || view.Entity == null ||
                !world.TryGetCurrentRuntimeHandle(runtimeSlot, view.Entity, out RuntimeEntityHandle handle))
            {
                return CreateEntityDiagnostic(
                    BattleCentralEntityDiagnosticReason.InvalidRuntimeHandle,
                    RuntimeEntityHandle.Invalid,
                    commandType);
            }

            return CaptureEntityDiagnostic(world, handle, commandType);
        }

        private static BattleCentralEntityDiagnosticReason ResolveDiagnosticResource(
            BattlePresentationFrame frame,
            in BattleRenderCommand command,
            out BattleCentralResolvedResource resource)
        {
            resource = default;
            if (!command.SpriteDescriptor.HasLogicalResourceKey)
                return BattleCentralEntityDiagnosticReason.MissingCatalogKey;

            if (command.Type == BattleRenderCommandType.Entity)
            {
                BattleVisualResourceKey logicalKey = command.SpriteDescriptor.LogicalResourceKey;
                if (!logicalKey.IsEntitySprite ||
                    !frame.BoundCatalog.TryGet(logicalKey.EntitySpriteKey, out BattleSpriteEntry entry) ||
                    entry.Key.VisualDataId != command.VisualDataId ||
                    entry.Key.EffectivePic != command.EffectivePic)
                {
                    return BattleCentralEntityDiagnosticReason.MissingCatalogKey;
                }

                BattleSpriteCentralBinding binding = entry.CentralBinding;
                if (binding.Texture == null)
                    return BattleCentralEntityDiagnosticReason.MissingTextureOrMaterial;
                if (!binding.IsValid)
                    return BattleCentralEntityDiagnosticReason.InvalidCentralBinding;
                Material material = binding.Mode == BattleSpriteCentralBindingMode.AtlasTextureArray
                    ? featureArrayMaterial
                    : featureMaterial;
                bool expectsArray = binding.Mode == BattleSpriteCentralBindingMode.AtlasTextureArray;
                if (!BattleSpriteMaterialContract.IsDeclaredCentralMaterial(material, expectsArray))
                    return BattleCentralEntityDiagnosticReason.MissingTextureOrMaterial;

                resource = new BattleCentralResolvedResource(
                    binding.Texture,
                    material,
                    binding.NormalizedUv,
                    new Vector2(entry.PixelWidth, entry.PixelHeight),
                    entry.Pivot,
                    command.Color,
                    (int)command.RenderState.MaterialSemantic,
                    binding.AtlasSlice,
                    binding.Mode,
                    binding.AtlasPageIndex);
                return BattleCentralEntityDiagnosticReason.None;
            }

            DiagnosticCatalogResolver.Configure(
                frame.BoundCatalog,
                frame.CommonVisualCatalog,
                featureMaterial,
                featureArrayMaterial);
            BattleCentralResourceStatus status = DiagnosticCatalogResolver.Resolve(command, out resource);
            return status switch
            {
                BattleCentralResourceStatus.Resolved => BattleCentralEntityDiagnosticReason.None,
                BattleCentralResourceStatus.UnsupportedRenderState =>
                    BattleCentralEntityDiagnosticReason.UnsupportedRenderState,
                BattleCentralResourceStatus.UnsupportedCategory =>
                    BattleCentralEntityDiagnosticReason.UnresolvedResource,
                _ => BattleCentralEntityDiagnosticReason.UnresolvedResource,
            };
        }

        private static bool TryFindSnapshot(
            BattlePresentationFrame frame,
            RuntimeEntityHandle handle,
            out BattlePresentationEntitySnapshot snapshot)
        {
            for (int index = 0; index < frame.EntityCount; index++)
            {
                BattlePresentationEntitySnapshot candidate = frame.GetEntity(index);
                if (candidate.Handle == handle)
                {
                    snapshot = candidate;
                    return true;
                }
            }

            snapshot = default;
            return false;
        }

        private static bool TryFindCommand(
            BattlePresentationFrame frame,
            RuntimeEntityHandle handle,
            BattleRenderCommandType commandType,
            out int commandIndex,
            out BattleRenderCommand command)
        {
            for (int index = 0; index < frame.CommandCount; index++)
            {
                BattleRenderCommand candidate = frame.GetCommand(index);
                if (candidate.Handle == handle && candidate.Type == commandType)
                {
                    commandIndex = index;
                    command = candidate;
                    return true;
                }
            }

            commandIndex = -1;
            command = default;
            return false;
        }

        private static int FindSegmentIndex(BattleDynamicMeshBackend backend, int commandIndex)
        {
            if (backend == null)
                return -1;
            for (int index = 0; index < backend.SegmentCount; index++)
            {
                BattleCentralRenderSegment segment = backend.GetSegment(index);
                if (commandIndex >= segment.FirstCommandIndex &&
                    commandIndex < segment.FirstCommandIndex + segment.CommandCount)
                {
                    return index;
                }
            }

            return -1;
        }

        private static BattleCentralEntityDiagnostic CreateEntityDiagnostic(
            BattleCentralEntityDiagnosticReason reason,
            RuntimeEntityHandle handle,
            BattleRenderCommandType commandType,
            BattlePresentationEntitySnapshot snapshot = default,
            bool hasSnapshot = false,
            BattleRenderCommand command = default,
            bool hasCommand = false,
            int commandIndex = -1,
            BattleCentralResolvedResource resource = default,
            bool hasResolvedResource = false,
            int segmentIndex = -1,
            int chunkIndex = -1,
            bool submitted = false)
        {
            return new BattleCentralEntityDiagnostic(
                reason,
                handle,
                commandType,
                snapshot,
                hasSnapshot,
                command,
                hasCommand,
                resource,
                hasResolvedResource,
                commandIndex,
                segmentIndex,
                chunkIndex,
                submitted);
        }

        internal static void ResolveDrawPolicyForPublication(
            GameConfig config,
            string[] commandLineArguments = null)
        {
            drawPolicyDecision = BattleRenderingPolicyResolver.ResolveDraw(
                config,
                serializedDrawMode,
                commandLineArguments);
            drawMode = drawPolicyDecision.EffectiveMode;
        }

        public static void ResetRuntime()
        {
            BattleCentralPresentationMountRegistry.ResetAllRuntimeBindings();
            BattlePixelFramePlan previous = CurrentPixelFramePlan;
            Volatile.Write(ref publishedPlanGeneration, 0);
            Volatile.Write(ref publishedPlanWorld, null);
            previous.Submission?.Retire();
            previous.World?.PublishPixelFramePlan(default);
            ReleaseDiagnosticCatalogBinding();
            for (int index = 0; index < Backends.Length; index++)
            {
                BattleCentralSubmission submission = SlotSubmissions[index];
                submission.Retire();
                if (submission.IsReusable)
                    Backends[index].Clear();
            }
            lastBuiltBackend = Backends[0];
            lastAttemptedBuildDiagnostics = default;
            requestedMode = BattlePresentationBackendMode.CentralOnly;
            ResetPerFrameDiagnostics(BattlePresentationBackendMode.CentralOnly, false);
            RuntimeDiagnostics.RefusalReason = string.Empty;
        }

        private static BattlePixelFramePlan CommitLegacyPlan(
            SimulationWorld world,
            BattlePresentationFrame frame,
            BattlePresentationBackendMode mode,
            int tickIndex,
            string reason,
            bool preserveBuildDiagnostics = false)
        {
            if (!preserveBuildDiagnostics)
            {
                ReleaseDiagnosticCatalogBinding();
                EmptyBackend.Clear();
                lastBuiltBackend = EmptyBackend;
            }
            var plan = new BattlePixelFramePlan(
                world,
                frame,
                mode,
                BattlePixelFrameOwner.Legacy,
                tickIndex,
                tickIndex,
                NextGeneration(),
                false,
                reason,
                null);
            PublishPlan(world, plan);
            RuntimeDiagnostics.SubmissionReady = false;
            RuntimeDiagnostics.EffectivePixelMode = BattlePresentationBackendMode.LegacyOnly;
            SetPlanDiagnostics(plan);
            RuntimeDiagnostics.RefusalReason = reason ?? string.Empty;
            return plan;
        }

        private static BattlePixelFramePlan CommitCentralFailurePlan(
            SimulationWorld world,
            int simulationTick,
            string reason)
        {
            ReleaseDiagnosticCatalogBinding();
            BattlePixelFramePlan previous = CurrentPixelFramePlan;
            BattleCentralSubmission submission = previous.IsValid &&
                                                   ReferenceEquals(previous.World, world) &&
                                                   previous.Owner == BattlePixelFrameOwner.Central &&
                                                   previous.Submission != null &&
                                                   !previous.Submission.IsRetired
                ? previous.Submission
                : null;
            BattlePresentationFrame displayFrame = submission?.CapturedFrame;
            int displayTick = submission?.TickIndex ?? -1;
            int generation = submission?.Generation ?? NextGeneration();
            var plan = new BattlePixelFramePlan(
                world,
                displayFrame,
                BattlePresentationBackendMode.CentralOnly,
                BattlePixelFrameOwner.Central,
                simulationTick,
                displayTick,
                generation,
                true,
                reason,
                submission);
            PublishPlan(world, plan);
            RuntimeDiagnostics.SubmissionReady = submission != null;
            RuntimeDiagnostics.EffectivePixelMode = BattlePresentationBackendMode.CentralOnly;
            SetPlanDiagnostics(plan);
            int retainedDrawCount = submission?.GetExecutedDrawCount(generation, displayTick) ?? 0;
            RuntimeDiagnostics.SubmittedPixelsLastFrame = retainedDrawCount > 0;
            RuntimeDiagnostics.LastSubmissionDrawCount = retainedDrawCount;
            RuntimeDiagnostics.RefusalReason = reason ?? string.Empty;
            if (submission == null)
            {
                EmptyBackend.Clear();
                lastBuiltBackend = EmptyBackend;
            }
            return plan;
        }

        private static void PublishPlan(SimulationWorld world, BattlePixelFramePlan plan)
        {
            BattlePixelFramePlan previous = CurrentPixelFramePlan;
            world?.PublishPixelFramePlan(plan);
            Volatile.Write(ref publishedPlanWorld, world);
            Volatile.Write(ref publishedPlanGeneration, plan.Generation);
            if (previous.IsValid && !ReferenceEquals(previous.World, world))
                previous.World?.PublishPixelFramePlan(default);
            if (plan.Submission != null && !ReferenceEquals(previous.Submission, plan.Submission))
            {
                RuntimeDiagnostics.SubmittedPixelsLastFrame = false;
                RuntimeDiagnostics.LastSubmissionDrawCount = 0;
            }
            if (!ReferenceEquals(previous.Submission, plan.Submission))
                previous.Submission?.Retire();
        }

        private static bool TryGetReusableBackend(
            out int backendIndex,
            out BattleDynamicMeshBackend backend)
        {
            BattleCentralSubmission currentSubmission = CurrentPixelFramePlan.Submission;
            for (int index = 0; index < Backends.Length; index++)
            {
                BattleCentralSubmission slotSubmission = SlotSubmissions[index];
                if (ReferenceEquals(slotSubmission, currentSubmission))
                    continue;
                if (!slotSubmission.IsReusable)
                    continue;

                backendIndex = index;
                backend = Backends[index];
                return true;
            }

            backendIndex = -1;
            backend = null;
            return false;
        }

        private static bool TryValidateActiveRenderer(out string reason)
        {
            Camera worldCamera = NTSDRenderSpace.WorldCamera;
            RuntimeDiagnostics.FeatureAvailable = featureOwner != null;
            RuntimeDiagnostics.MaterialAvailable =
                BattleSpriteMaterialContract.IsDeclaredCentralMaterial(featureMaterial, false);
            if (featureOwner == null || !featureOwner.isActive)
            {
                reason = "BattleRenderFeature is not registered and active; CentralOnly output is fail-closed.";
                return false;
            }
            if (!RuntimeDiagnostics.MaterialAvailable)
            {
                reason = "The central battle material is missing or violates the declared alpha contract.";
                return false;
            }
            if (worldCamera == null || !worldCamera.enabled || !worldCamera.gameObject.activeInHierarchy)
            {
                reason = "The bound battle world camera is unavailable or disabled.";
                return false;
            }
            try
            {
                if (!worldCamera.TryGetComponent(out UniversalAdditionalCameraData cameraData) ||
                    cameraData.scriptableRenderer == null ||
                    !ReferenceEquals(cameraData.scriptableRenderer, observedRenderer))
                {
                    reason = "The battle world camera is not using the renderer that invoked BattleRenderFeature.";
                    return false;
                }
            }
            catch (Exception exception)
            {
                reason = $"The battle world-camera renderer could not be validated: {exception.GetType().Name}.";
                return false;
            }
            int observationAge = observedUnityFrame < 0 ? int.MaxValue : Time.frameCount - observedUnityFrame;
            if (observedFeatureOwner != featureOwner || observedWorldCamera != worldCamera ||
                observationAge < 0 || observationAge > RendererObservationMaxAgeFrames)
            {
                reason = "The active world-camera renderer has not recently invoked the registered BattleRenderFeature.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        private static string BuildOwnershipRefusalReason(BattleDynamicMeshBackend backend)
        {
            BattleCentralBuildDiagnostics diagnostics = backend.Diagnostics;
            return "Central frame ownership is incomplete: " +
                   $"unresolved={diagnostics.UnresolvedCommandCount}, " +
                   $"unsupportedCategory={diagnostics.UnsupportedCategoryCount}, " +
                   $"unsupportedState={diagnostics.UnsupportedRenderStateCount}.";
        }

        private static void BindDiagnosticCatalog(
            CharacterAnimtorManager manager,
            BattleSpriteCatalog catalog)
        {
            BattleSpriteCatalog nextCatalog = catalog ?? BattleSpriteCatalog.Empty;
            if (ReferenceEquals(diagnosticCatalogManager, manager) &&
                ReferenceEquals(diagnosticCatalog, nextCatalog))
            {
                return;
            }

            ReleaseDiagnosticCatalogBinding();
            diagnosticCatalogManager = manager;
            diagnosticCatalog = nextCatalog;
            diagnosticCatalogManager?.RegisterRendererCatalogBinding(diagnosticCatalog);
        }

        private static void ReleaseDiagnosticCatalogBinding()
        {
            CharacterAnimtorManager manager = diagnosticCatalogManager;
            BattleSpriteCatalog catalog = diagnosticCatalog;
            diagnosticCatalogManager = null;
            diagnosticCatalog = BattleSpriteCatalog.Empty;
            manager?.UnregisterRendererCatalogBinding(catalog);
        }

        private static void ResetPerFrameDiagnostics(
            BattlePresentationBackendMode mode,
            bool frameAvailable)
        {
            RuntimeDiagnostics.RequestedMode = mode;
            RuntimeDiagnostics.EffectivePixelMode = mode == BattlePresentationBackendMode.CentralOnly
                ? BattlePresentationBackendMode.CentralOnly
                : BattlePresentationBackendMode.LegacyOnly;
            RuntimeDiagnostics.FeatureAvailable = featureOwner != null;
            RuntimeDiagnostics.MaterialAvailable = featureMaterial != null;
            RuntimeDiagnostics.FrameAvailable = frameAvailable;
            RuntimeDiagnostics.AllCategoryOwnershipReady = false;
            RuntimeDiagnostics.CommonShadowBindingReady = false;
            RuntimeDiagnostics.CommonSparkBindingReady = false;
            RuntimeDiagnostics.SubmissionReady = false;
            RuntimeDiagnostics.SubmittedPixelsLastFrame = false;
            RuntimeDiagnostics.LastSubmissionDrawCount = 0;
            RuntimeDiagnostics.SimulationTick = 0;
            RuntimeDiagnostics.DisplayTick = -1;
            RuntimeDiagnostics.IsStale = false;
            RuntimeDiagnostics.Reason = string.Empty;
            RuntimeDiagnostics.RefusalReason = string.Empty;
        }

        private static void SetPlanDiagnostics(BattlePixelFramePlan plan)
        {
            RuntimeDiagnostics.SimulationTick = plan.SimulationTick;
            RuntimeDiagnostics.DisplayTick = plan.DisplayTick;
            RuntimeDiagnostics.IsStale = plan.IsStale;
            RuntimeDiagnostics.Reason = plan.Reason;
        }

        private static int NextGeneration()
        {
            int generation = Interlocked.Increment(ref nextGeneration);
            if (generation > 0)
                return generation;
            Interlocked.Exchange(ref nextGeneration, 1);
            return 1;
        }

        private readonly struct AttemptedBuildDiagnostics
        {
            private AttemptedBuildDiagnostics(
                int simulationTick,
                BattlePresentationFrame frame,
                BattleCentralBuildDiagnostics diagnostics)
            {
                SimulationTick = simulationTick;
                Frame = frame;
                BuildTick = diagnostics.TickIndex;
                SourceCommandCount = diagnostics.SourceCommandCount;
                ResolvedCommandCount = diagnostics.ResolvedCommandCount;
                UnresolvedCommandCount = diagnostics.UnresolvedCommandCount;
                UnsupportedCategoryCount = diagnostics.UnsupportedCategoryCount;
                UnsupportedRenderStateCount = diagnostics.UnsupportedRenderStateCount;
                ActiveChunkCount = diagnostics.ActiveChunkCount;
                SegmentCount = diagnostics.SegmentCount;
                FirstUnresolvedCommandIndex = diagnostics.FirstUnresolvedCommandIndex;
                FirstUnresolvedCommandType = diagnostics.FirstUnresolvedCommandType;
                FirstUnresolvedStatus = diagnostics.FirstUnresolvedStatus;
                IsValid = true;
            }

            public bool IsValid { get; }
            public int SimulationTick { get; }
            public BattlePresentationFrame Frame { get; }
            public int BuildTick { get; }
            public int SourceCommandCount { get; }
            public int ResolvedCommandCount { get; }
            public int UnresolvedCommandCount { get; }
            public int UnsupportedCategoryCount { get; }
            public int UnsupportedRenderStateCount { get; }
            public int ActiveChunkCount { get; }
            public int SegmentCount { get; }
            public int FirstUnresolvedCommandIndex { get; }
            public BattleRenderCommandType FirstUnresolvedCommandType { get; }
            public BattleCentralResourceStatus FirstUnresolvedStatus { get; }

            public static AttemptedBuildDiagnostics Capture(
                BattleDynamicMeshBackend backend,
                int simulationTick)
            {
                return backend == null
                    ? default
                    : new AttemptedBuildDiagnostics(
                        simulationTick,
                        backend.BuiltFrame,
                        backend.Diagnostics);
            }
        }

        private static int FindRegistration(BattleRenderFeature owner)
        {
            if (owner == null)
                return -1;
            for (int index = featureRegistrationCount - 1; index >= 0; index--)
            {
                if (featureRegistrations[index].Owner == owner)
                    return index;
            }
            return -1;
        }

        private static void RemoveRegistrationAt(int index)
        {
            for (int source = index + 1; source < featureRegistrationCount; source++)
                featureRegistrations[source - 1] = featureRegistrations[source];
            featureRegistrationCount--;
            featureRegistrations[featureRegistrationCount] = default;
        }

        private static void EnsureRegistrationCapacity(int required)
        {
            if (required <= featureRegistrations.Length)
                return;
            int next = featureRegistrations.Length;
            while (next < required)
                next = checked(next * 2);
            Array.Resize(ref featureRegistrations, next);
        }

        private static void ApplyActiveRegistration()
        {
            FeatureRegistration active = featureRegistrationCount > 0
                ? featureRegistrations[featureRegistrationCount - 1]
                : default;
            featureOwner = active.Owner;
            featureMaterial = active.Material;
            featureArrayMaterial = active.ArrayMaterial;
            serializedDrawMode = featureOwner != null
                ? active.DrawMode
                : BattleCentralDrawMode.OrderedChunks;
            drawPolicyDecision = featureOwner != null
                ? BattleRenderingPolicyResolver.ResolveDraw(GameConfig.Instance, serializedDrawMode)
                : new BattleDrawPolicyDecision(
                    BattleDrawPolicyMode.Auto,
                    BattleCentralDrawMode.OrderedChunks,
                    string.Empty);
            drawMode = drawPolicyDecision.EffectiveMode;
            observedFeatureOwner = null;
            observedRenderer = null;
            observedWorldCamera = null;
            observedUnityFrame = -1;
            RuntimeDiagnostics.FeatureAvailable = featureOwner != null;
            RuntimeDiagnostics.MaterialAvailable = featureMaterial != null;
        }

        private readonly struct FeatureRegistration
        {
            public FeatureRegistration(
                BattleRenderFeature owner,
                Material material,
                Material arrayMaterial,
                BattleCentralDrawMode drawMode)
            {
                Owner = owner;
                Material = material;
                ArrayMaterial = arrayMaterial;
                DrawMode = drawMode;
            }

            public BattleRenderFeature Owner { get; }
            public Material Material { get; }
            public Material ArrayMaterial { get; }
            public BattleCentralDrawMode DrawMode { get; }
        }
    }
}


--- File: Assets/NTSD/Scripts/Animation/Rendering/BattlePresentationShadowBuild.cs --- (Error reading file)

--- File: Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectRenderer.cs ---
﻿using UnityEngine;
using System.Collections.Generic;
using NTSD.Animation.LF2Tasks;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using MoreMountains.Tools;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// LF2 对象渲染器，负责把逻辑层实体的当前帧、朝向和 C++ 像素坐标同步到 Unity SpriteRenderer。
    /// </summary>
    public class LF2ObjectRenderer : MonoBehaviour, ISimObject
    {
        // ========== 组件引用 ==========
        private SpriteRenderer _spriteRenderer;
        private SpriteRenderer _shadowRenderer;
        private Transform _visualTransform;
        private Material _defaultSpriteSharedMaterial;
        private Material _defaultShadowSharedMaterial;

        // ========== 逻辑层引用 ==========
        private LF2Entity _logicObject;
        private int _boundSpriteObjectId = int.MinValue;
        private BattleSpriteCatalog _boundSpriteCatalog;
        private CharacterAnimtorManager _catalogBindingManager;

        // 渲染帧计数器，对齐 C++ release 的 dword_449098。
        private int _renderFrameCount = 0;

        // 缓存稳定 ID，AllocateStableId 只调用一次。
        [SerializeField][MMReadOnly]private int _stableId = 0;

        // ========== 公开属性 ==========
        public ILF2Object LogicObject => _logicObject;

        // ========== ISimObject 实现 ==========

        /// <summary>
        /// 渲染层固定在所有逻辑对象之后执行。
        /// </summary>
        public int SimOrder => SimOrderConstants.Renderer;

        public int StableId
        {
            get
            {
                if (_stableId == 0)
                {
                    _stableId = SimulationTickDriver.Instance?.World?.AllocateStableId() ?? GetInstanceID();
                }

                return _stableId;
            }
        }

        // ========== 生命周期 ==========

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _defaultSpriteSharedMaterial = ResolveBorrowedDefaultSharedMaterial(_spriteRenderer);
            NormalizeSpriteRendererState(_spriteRenderer, _defaultSpriteSharedMaterial);
            _visualTransform = this.transform;
        }

        private void OnEnable()
        {
            SimulationTickDriver.Instance?.World?.Register(this);
        }

        private void OnDisable()
        {
            SimulationTickDriver.Instance?.World?.Unregister(this);
        }

        public void SimLateTick(int tickIndex)
        {
            if (_logicObject == null) return;

            bool suppressLegacyMaterializers =
                BattleCentralRenderSystem.ShouldSuppressLegacyMaterializers(_logicObject.Match);
            _logicObject.Sprite?.SetLegacyRendererSuppressed(suppressLegacyMaterializers);

            int firstPresentationTick = _logicObject.Runtime?.FirstPresentationTick ?? 0;
            bool presentationBlocked = _logicObject.Runtime?.OidMergeDormant == true ||
                                       tickIndex < firstPresentationTick;
            if (suppressLegacyMaterializers)
            {
                _logicObject.Sprite?.SetPresentationSuppressed(presentationBlocked);
                if (!presentationBlocked)
                {
                    UpdateSprite();
                    _logicObject.UpdateShadow(_renderFrameCount);
                }
                _logicObject.ReleaseForcedRuntimeIntPositionAfterFirstPresentation(tickIndex);
                ApplyVisualShake();
                return;
            }

            if (_logicObject.Runtime?.OidMergeDormant == true)
            {
                _logicObject.Sprite?.SetPresentationSuppressed(true);
                return;
            }

            if (tickIndex < firstPresentationTick)
            {
                _logicObject.Sprite?.SetPresentationSuppressed(true);
                return;
            }

            _logicObject.Sprite?.SetPresentationSuppressed(false);
            UpdateSprite();
            UpdatePosition(tickIndex);
            _logicObject.Match?.RecordLegacyEntityProbe(_logicObject, _spriteRenderer);
            _logicObject.ReleaseForcedRuntimeIntPositionAfterFirstPresentation(tickIndex);
            ApplyVisualShake();
        }

        /// <summary>
        /// opoint 刚生成对象时，逻辑帧和表现对象需要在同一个模拟时刻完成同步。
        /// </summary>
        public void ForceRefreshPresentation()
        {
            if (_logicObject == null) return;
            int currentTick = _logicObject?.Match?.CurrentTickIndex ?? 0;
            bool suppressLegacyMaterializers =
                BattleCentralRenderSystem.ShouldSuppressLegacyMaterializers(_logicObject.Match);
            _logicObject.Sprite?.SetLegacyRendererSuppressed(suppressLegacyMaterializers);

            int firstPresentationTick = _logicObject.Runtime?.FirstPresentationTick ?? 0;
            bool presentationBlocked = _logicObject.Runtime?.OidMergeDormant == true ||
                                       currentTick < firstPresentationTick;
            if (suppressLegacyMaterializers)
            {
                _logicObject.Sprite?.SetPresentationSuppressed(presentationBlocked);
                if (!presentationBlocked)
                {
                    UpdateSprite();
                    _logicObject.UpdateShadow(_renderFrameCount);
                }
                _logicObject.ReleaseForcedRuntimeIntPositionAfterFirstPresentation(currentTick);
                return;
            }

            if (_logicObject.Runtime?.OidMergeDormant == true)
            {
                _logicObject.Sprite?.SetPresentationSuppressed(true);
                return;
            }
            if (currentTick < firstPresentationTick)
            {
                _logicObject.Sprite?.SetPresentationSuppressed(true);
                return;
            }

            _logicObject.Sprite?.SetPresentationSuppressed(false);
            UpdateSprite();
            UpdatePosition(currentTick);
            _logicObject.Match?.RecordLegacyEntityProbe(_logicObject, _spriteRenderer);
            _logicObject.ReleaseForcedRuntimeIntPositionAfterFirstPresentation(currentTick);
        }

        // ========== 核心方法 ==========

        /// <summary>
        /// 设置逻辑对象并初始化对应的 Sprite 资源。
        /// </summary>
        public void SetLogicObject(ILF2Object logicObject, LF2TaskBase task)
        {
            ReleaseCatalogBinding();
            RestorePooledVisualState();
            _logicObject = logicObject as LF2Entity;
            _renderFrameCount = 0;
            _logicObject?.Init(task, this);
            BattleCentralPresentationMountRegistry.BindOwnerRuntime(
                this,
                ResolveCurrentRuntimeHandle(_logicObject));

            List<Sprite> sprites = null;
            int startFrame = 0;
            int visualDataId = int.MinValue;
            CharacterAnimtorManager animatorManager = CharacterAnimtorManager.Instance;
            if (_logicObject != null)
            {
                visualDataId = LF2Entity.ResolveCurrentDataObjectId(_logicObject);
                animatorManager?.TryGetSprites(visualDataId, out sprites);
                startFrame = animatorManager?.GetStartFrame(visualDataId) ?? 0;
            }
            if (sprites != null)
            {
                _logicObject?.Sprite?.Initialize(
                    _spriteRenderer,
                    sprites,
                    startFrame,
                    animatorManager?.SpriteCatalog ?? BattleSpriteCatalog.Empty,
                    visualDataId);
                _boundSpriteObjectId = visualDataId;
                UpdateCatalogBinding(animatorManager, animatorManager?.SpriteCatalog);
            }
            else
            {
                _logicObject?.Sprite?.Initialize(
                    _spriteRenderer,
                    null,
                    0,
                    animatorManager?.SpriteCatalog ?? BattleSpriteCatalog.Empty,
                    visualDataId);
                _logicObject?.Sprite?.ClearCurrentSprite();
                _boundSpriteObjectId = int.MinValue;
                UpdateCatalogBinding(animatorManager, animatorManager?.SpriteCatalog);
            }
            _logicObject?.SetShadowRenderer(_shadowRenderer);

            // 新生成对象先压住表现。
            // C++ release 的 late opoint / transition smoke 不允许在创建当拍先露一帧，
            // 必须等 FirstPresentationTick 到达后再由 ForceRefresh/SimLateTick 放行。
            _logicObject?.Sprite?.SetPresentationSuppressed(true);

            var frame = _logicObject?.Frame?.D;
            if (frame != null && _logicObject.Sprite != null)
                _logicObject.Sprite.ShowPic(_logicObject.GetRenderPicIndex());
        }

        /// <summary>
        /// 对象池复用时恢复 Unity 渲染组件状态，避免上一轮 Hide/Reset 留下不可见状态。
        /// </summary>
        public void RestorePooledVisualState()
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_defaultSpriteSharedMaterial == null)
                _defaultSpriteSharedMaterial = ResolveBorrowedDefaultSharedMaterial(_spriteRenderer);

            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = true;
                NormalizeSpriteRendererState(_spriteRenderer, _defaultSpriteSharedMaterial);
            }
            NormalizeSpriteRendererState(_shadowRenderer, _defaultShadowSharedMaterial);
        }

        public void SetShadowRenderer(SpriteRenderer shadowRenderer)
        {
            _shadowRenderer = shadowRenderer;
            _defaultShadowSharedMaterial = ResolveBorrowedDefaultSharedMaterial(shadowRenderer);
            if (_shadowRenderer != null)
                _shadowRenderer.sortingLayerName = "Object";
            _logicObject?.SetShadowRenderer(shadowRenderer);
        }

        /// <summary>
        /// 重置状态，归还对象池前调用。
        /// </summary>
        public void ResetState()
        {
            BattleCentralPresentationMountRegistry.ResetOwnerRuntimeBinding(this);
            _logicObject?.Sprite?.Reset();
            _logicObject?.SetShadowRenderer(null);
            if (_defaultSpriteSharedMaterial == null)
                _defaultSpriteSharedMaterial = ResolveBorrowedDefaultSharedMaterial(_spriteRenderer);
            NormalizeSpriteRendererState(_spriteRenderer, _defaultSpriteSharedMaterial);
            NormalizeSpriteRendererState(_shadowRenderer, _defaultShadowSharedMaterial);
            ReleaseCatalogBinding();
            _logicObject?.UnregisterFromWorld();
            _logicObject?.Reset();
            _logicObject = null;
            _boundSpriteObjectId = int.MinValue;
            gameObject.SetActive(false);
        }

        private static RuntimeEntityHandle ResolveCurrentRuntimeHandle(LF2Entity logicObject)
        {
            if (logicObject == null || logicObject.Runtime?.SlotIndex < 0)
                return RuntimeEntityHandle.Invalid;

            SimulationWorld world = logicObject.Match;
            return world != null && world.TryGetCurrentRuntimeHandle(
                logicObject.Runtime.SlotIndex,
                logicObject,
                out RuntimeEntityHandle handle)
                ? handle
                : RuntimeEntityHandle.Invalid;
        }

        internal static void NormalizeSpriteRendererState(
            SpriteRenderer renderer,
            Material borrowedDefaultSharedMaterial)
        {
            if (renderer == null)
                return;

            renderer.color = Color.white;
            renderer.flipX = false;
            renderer.flipY = false;
            renderer.maskInteraction = SpriteMaskInteraction.None;
            if (borrowedDefaultSharedMaterial != null)
                renderer.sharedMaterial = borrowedDefaultSharedMaterial;
        }

        internal static Material ResolveBorrowedDefaultSharedMaterial(SpriteRenderer renderer)
        {
            Material sharedMaterial = renderer != null ? renderer.sharedMaterial : null;
            return sharedMaterial != null && sharedMaterial.shader != null &&
                   sharedMaterial.shader.name == "Sprites/Default"
                ? sharedMaterial
                : null;
        }

        /// <summary>
        /// 按当前帧 pic 和运行时方向刷新 Unity SpriteRenderer。
        /// </summary>
        private void UpdateSprite()
        {
            if (_logicObject == null) return;
            bool shouldDrawForHitStop = ShouldDrawEntityForHitStop(_logicObject.Runtime?.HitStop ?? 0);
            _logicObject.Sprite?.SetLegacyEntityVisible(shouldDrawForHitStop);
            if (!shouldDrawForHitStop)
            {
                // C# release DrawEntity hides the entity for the negative HitStop
                // threshold and four-tick blink phase. This only changes presentation;
                // the runtime entity continues advancing normally.
                return;
            }

            EnsureRuntimeIdentitySprites();
            var frame = _logicObject.Frame?.D;
            if (frame == null)
            {
                // C++ 侧 frame 已经切到 1000/无效帧时，不应继续保留上一张图。
                _logicObject.Sprite?.Hide();
                _logicObject.Sprite?.HideShadow();
                return;
            }
            if (_logicObject.Sprite == null) return;
            _logicObject.Sprite.ShowPic(_logicObject.GetRenderPicIndex());
            var ps = _logicObject.PS;
            if (ps != null)
                _logicObject.Sprite.SwitchLR(ps.dir);
        }

        internal static bool ShouldDrawEntityForHitStop(int hitStop)
        {
            return hitStop > -25 && (System.Math.Abs((long)hitStop) % 4) < 2;
        }

        internal static bool ShouldDrawShadowForHitStop(int hitStop)
        {
            return hitStop > -70 && (System.Math.Abs((long)hitStop) % 4) < 2;
        }

        private void EnsureRuntimeIdentitySprites()
        {
            if (_logicObject == null || _logicObject.Sprite == null)
                return;

            int visualDataId = LF2Entity.ResolveCurrentDataObjectId(_logicObject);
            CharacterAnimtorManager animatorManager = CharacterAnimtorManager.Instance;
            BattleSpriteCatalog currentCatalog = animatorManager?.SpriteCatalog ?? BattleSpriteCatalog.Empty;
            if (_boundSpriteObjectId == visualDataId &&
                ReferenceEquals(_boundSpriteCatalog, currentCatalog))
                return;

            if (animatorManager != null &&
                animatorManager.TryGetSprites(visualDataId, out List<Sprite> sprites) &&
                sprites != null)
            {
                int startFrame = animatorManager.GetStartFrame(visualDataId);
                _logicObject.Sprite.SetSprites(sprites, startFrame);
                _logicObject.Sprite.SetCatalogBinding(animatorManager.SpriteCatalog, visualDataId);
                _boundSpriteObjectId = visualDataId;
                UpdateCatalogBinding(animatorManager, animatorManager.SpriteCatalog);
                return;
            }

            // Never render the previous identity's catalog while the new one is still unavailable.
            _logicObject.Sprite.SetSprites(null);
            _logicObject.Sprite.SetCatalogBinding(
                animatorManager?.SpriteCatalog ?? BattleSpriteCatalog.Empty,
                visualDataId);
            _boundSpriteObjectId = int.MinValue;
            UpdateCatalogBinding(animatorManager, currentCatalog);
        }

        private void UpdateCatalogBinding(
            CharacterAnimtorManager manager,
            BattleSpriteCatalog catalog)
        {
            if (ReferenceEquals(_catalogBindingManager, manager) &&
                ReferenceEquals(_boundSpriteCatalog, catalog))
                return;

            ReleaseCatalogBinding();
            _catalogBindingManager = manager;
            _boundSpriteCatalog = catalog;
            manager?.RegisterRendererCatalogBinding(catalog);
        }

        private void ReleaseCatalogBinding()
        {
            _catalogBindingManager?.UnregisterRendererCatalogBinding(_boundSpriteCatalog);
            _catalogBindingManager = null;
            _boundSpriteCatalog = null;
        }

        private void OnDestroy()
        {
            BattleCentralPresentationMountRegistry.RemoveOwnerRuntimeBinding(this);
            ReleaseCatalogBinding();
        }

        /// <summary>
        /// 同步 Transform 位置。
        /// C++ release draw_entity 使用绘制矩形：
        /// 朝右 dst.x = x - centerx，朝左 dst.x = x - (frame_w - centerx)，dst.y = z + y - centery。
        /// Unity 运行时 Sprite 的 pivot 是底部中心，因此这里把 C++ 绘制矩形换算为底部中心点。
        /// </summary>
        private void UpdatePosition(int tickIndex)
        {
            if (_logicObject == null) return;
            var ps = _logicObject.PS;
            if (ps == null) return;

            ApplyCppDrawEntityPosition(ps, tickIndex);
            _logicObject.Sprite?.SetZ(_logicObject.GetDisplayRenderSortingOrder(
                _logicObject.GetDisplayZ(), ps.zz));

            // 阴影按 C++ 逻辑坐标 x/z 独立更新，不跟随图片 pivot。
            _logicObject.UpdateShadow(_renderFrameCount);
        }

        private void ApplyCppDrawEntityPosition(PhysicsState ps, int tickIndex)
        {
            var frame = _logicObject.Frame?.D;

            float spriteWidth = _logicObject.GetSpriteWidthPxForRender();
            float spriteHeight = _logicObject.GetSpriteHeightPxForRender();
            float centerx = frame?.centerx ?? 0f;
            float centery = frame?.centery ?? 0f;

            // C++ release draw_entity 使用的是 x_int / y_int / z_int。
            // 这里不能直接吃 Unity 侧的浮点逻辑坐标，否则同一实体在出生后续拍、
            // 摩擦衰减和 type=3/oid=999 这类路径上会出现和正式版不一致的像素漂移。
            int cameraX = _logicObject.Match?.ReleaseCameraX ?? 0;
            Vector2 pivot = ComputeEntityBottomCenterPivotPixels(
                _logicObject.GetRuntimeXInt(),
                _logicObject.GetRuntimeYInt(),
                _logicObject.GetDisplayZ(),
                _logicObject.GetRenderOffsetX(),
                cameraX,
                _logicObject.FrameDelay,
                tickIndex,
                ps.dir == "left",
                spriteWidth,
                spriteHeight,
                centerx,
                centery,
                NTSDRenderSpace.BattleVisualScale);
            pivot += ResolveHeldVisualAttachmentOffsetPixels(frame);

            Transform rootTransform = transform.parent != null ? transform.parent : transform;
            rootTransform.localScale = NTSDRenderSpace.RenderScale;
            Vector3 worldPos = NTSDRenderSpace.ScreenPixelToWorld(pivot.x, pivot.y, rootTransform.position.z);
            rootTransform.position = worldPos;

            if (_visualTransform != null && _visualTransform != rootTransform)
                _visualTransform.localPosition = Vector3.zero;
        }

        internal static Vector2 ComputeEntityBottomCenterPivotPixels(
            int xInt,
            int yInt,
            float displayZ,
            float renderOffsetX,
            int cameraX,
            int frameDelay,
            int tickIndex,
            bool facingLeft,
            float spriteWidth,
            float spriteHeight,
            float centerx,
            float centery,
            float visualScale)
        {
            int extraX = frameDelay < 0 ? 6 * (tickIndex & 1) - 3 : 0;
            int screenX = xInt + (int)renderOffsetX - cameraX + extraX;
            int screenY = (int)displayZ + yInt;
            float pivotX = facingLeft
                ? screenX + visualScale * (centerx - spriteWidth * 0.5f)
                : screenX + visualScale * (spriteWidth * 0.5f - centerx);
            float pivotY = screenY + visualScale * (spriteHeight - centery);
            return new Vector2(pivotX, pivotY);
        }

        private Vector2 ResolveHeldVisualAttachmentOffsetPixels(LF2FrameData heldFrame)
        {
            NTSDEntityRuntime heldRuntime = _logicObject.Runtime;
            int holderSlot = heldRuntime?.HolderStableId ?? -1;
            LF2Entity holder = _logicObject.Match?.FindEntityByRuntimeSlotForQuery(holderSlot);
            return ResolveHeldVisualAttachmentOffsetPixels(
                heldRuntime,
                heldFrame,
                holder,
                NTSDRenderSpace.BattleVisualScale);
        }

        internal static Vector2 ResolveHeldVisualAttachmentOffsetPixels(
            NTSDEntityRuntime heldRuntime,
            LF2FrameData heldFrame,
            LF2Entity holder,
            float visualScale)
        {
            NTSDEntityRuntime holderRuntime = holder?.Runtime;
            LF2FrameData holderFrame = holder?.Frame?.D;
            if (heldRuntime == null || heldRuntime.LinkState >= 0 || heldRuntime.SlotIndex < 0 ||
                holderRuntime == null || holderRuntime.SlotIndex != heldRuntime.HolderStableId ||
                holderRuntime.TargetSlotIndex != heldRuntime.SlotIndex ||
                holderFrame?.wpoints == null || holderFrame.wpoints.Count == 0 ||
                heldFrame?.wpoints == null || heldFrame.wpoints.Count == 0)
            {
                return Vector2.zero;
            }

            WeaponPoint holderWPoint = holderFrame.wpoints[0];
            WeaponPoint heldWPoint = heldFrame.wpoints[0];
            if (holderWPoint == null || heldWPoint == null)
                return Vector2.zero;

            return ComputeHeldVisualAttachmentOffsetPixels(
                holderRuntime.Dir == "left",
                holderFrame.centerx,
                holderFrame.centery,
                holderWPoint.x,
                holderWPoint.y,
                heldFrame.centerx,
                heldFrame.centery,
                heldWPoint.x,
                heldWPoint.y,
                visualScale);
        }

        internal static Vector2 ComputeHeldVisualAttachmentOffsetPixels(
            bool facingLeft,
            float holderCenterX,
            float holderCenterY,
            float holderWPointX,
            float holderWPointY,
            float heldCenterX,
            float heldCenterY,
            float heldWPointX,
            float heldWPointY,
            float visualScale)
        {
            float scaleDelta = visualScale - 1f;
            float holderDeltaX = holderWPointX - holderCenterX;
            float heldDeltaX = heldWPointX - heldCenterX;
            float x = scaleDelta * (holderDeltaX - heldDeltaX);
            if (facingLeft)
                x = -x;

            float holderDeltaY = holderWPointY - holderCenterY;
            float heldDeltaY = heldWPointY - heldCenterY;
            float y = scaleDelta * (holderDeltaY - heldDeltaY);
            return new Vector2(x, y);
        }

        private void ApplyVisualShake()
        {
            _renderFrameCount++;
        }

        // ========== 辅助方法 ==========

        /// <summary>
        /// 获取当前 Sprite 宽度，单位为像素。
        /// </summary>
        public float GetSpriteWidth()
        {
            return _logicObject?.GetSpriteWidthPxForRender() ?? 0f;
        }
    }
}


--- File: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Sprite.cs ---
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 精灵动画模块，封装 Unity SpriteRenderer 操作。
    /// 当前模块只负责 Unity 渲染适配，不作为战斗逻辑复刻依据。
    /// </summary>
    public class LF2Sprite
    {
        private SpriteRenderer _renderer;
        private List<Sprite> _sprites;
        private BattleSpriteCatalog _catalog;
        private int _visualDataId = int.MinValue;
        private BattleSpriteEntry _currentEntry;
        private int _currentPic = 999;
        private string _dir = "right";

        private SpriteRenderer _shadowRenderer;
        private bool _hasShadow;
        private bool _entityVisible = true;
        private bool _shadowVisible = true;
        private bool _presentationSuppressed;
        private bool _legacyRendererSuppressed;
        private bool _legacyEntityVisible = true;
        private Vector2 _localOffsetPixels;

        // SortingGroup 用于角色根节点，优先控制层级；武器/SA 无 SortingGroup 则回退到 SpriteRenderer.sortingOrder
        private SortingGroup _sortingGroup;

        /// <summary>
        /// 当前方向
        /// </summary>
        public string Dir => _dir;

        public bool EntityVisible => _entityVisible;

        public bool ShadowVisible => _shadowVisible;

        public bool PresentationSuppressed => _presentationSuppressed;

        public Vector2 LocalOffsetPixels => _localOffsetPixels;

        public int CurrentPic => _currentPic;

        private int _startFrame;

        /// <summary>
        /// 初始化精灵模块
        /// </summary>
        /// <param name="renderer">SpriteRenderer 组件引用</param>
        /// <param name="sprites">精灵列表</param>
        /// <param name="startFrame">精灵列表中的起始偏移（对应 SpriteFileInfo.startFrame）</param>
        public void Initialize(
            SpriteRenderer renderer,
            List<Sprite> sprites,
            int startFrame = 0,
            BattleSpriteCatalog catalog = null,
            int visualDataId = int.MinValue)
        {
            _renderer = renderer;
            _sprites = sprites;
            _startFrame = startFrame;
            _catalog = catalog;
            _visualDataId = visualDataId;
            _currentEntry = null;
            _currentPic = 999;
            _dir = "right";
            _entityVisible = true;
            _shadowVisible = true;
            _presentationSuppressed = false;
            _legacyRendererSuppressed = false;
            _legacyEntityVisible = true;
            _localOffsetPixels = Vector2.zero;

            // 从根节点查找 SortingGroup（角色有，武器/SA 无）
            _sortingGroup = renderer != null
                ? renderer.GetComponentInParent<SortingGroup>()
                : null;

            if (_renderer != null)
            {
                _renderer.sortingLayerName = "Object";
                _renderer.color = Color.white;
                _renderer.sprite = null;
                _renderer.flipX = false;
                Vector3 localPosition = _renderer.transform.localPosition;
                _renderer.transform.localPosition = new Vector3(0f, 0f, localPosition.z);
                ApplyEntityRendererVisibility();
            }
            if(_sortingGroup != null)
                _sortingGroup.sortingLayerName = "Object";

        }

        /// <summary>
        /// 初始化阴影渲染器。
        /// </summary>
        public void InitializeShadow(SpriteRenderer shadowRenderer)
        {
            _shadowRenderer = shadowRenderer;
            _hasShadow = shadowRenderer != null;
            if (_shadowRenderer != null)
            {
                _shadowRenderer.sortingLayerName = "Object";
                ApplyShadowRendererVisibility();
            }
        }

        public bool HasShadow => _hasShadow;

        /// <summary>
        /// 更新精灵列表（用于运行时切换角色）
        /// </summary>
        public void SetSprites(List<Sprite> sprites, int startFrame = 0)
        {
            _sprites = sprites;
            _startFrame = startFrame;

            if (sprites == null)
                ClearCurrentSprite();
        }

        public void SetCatalogBinding(BattleSpriteCatalog catalog, int visualDataId)
        {
            _catalog = catalog;
            _visualDataId = visualDataId;
            _currentEntry = null;
            ClearCurrentSprite();
        }

        /// <summary>
        /// 显示指定图片。
        /// </summary>
        /// <param name="picIndex">图片索引</param>
        public bool HasRenderer => _renderer != null;

        public void ShowPic(int picIndex)
        {
            _currentPic = picIndex;
            if (picIndex == 999)
            {
                ClearCurrentSprite();
                return;
            }

            if (_catalog != null)
            {
                if (!_catalog.TryGet(_visualDataId, picIndex, out BattleSpriteEntry entry) ||
                    entry == null)
                {
                    ClearResolvedSprite();
                    return;
                }

                _currentEntry = entry;
                _entityVisible = true;
                if (_renderer != null)
                    _renderer.sprite = entry.LegacySprite;
                ApplyEntityRendererVisibility();
                return;
            }

            // Editor previews and isolated legacy tests may still bind only a
            // sprite list. Production battle renderers always bind the catalog.
            if (_sprites == null)
            {
                ClearResolvedSprite();
                return;
            }

            // 运行时 MergedSprites 已按绝对 pic 编号展开；正常路径直接用 picIndex 取图。
            // 仅在传入的是局部表索引时，才回退到 startFrame 偏移，避免把 oid=999 等多文件对象二次偏移。
            int actualIndex = picIndex;
            if ((actualIndex < 0 || actualIndex >= _sprites.Count || _sprites[actualIndex] == null) &&
                _startFrame != 0)
            {
                actualIndex = _startFrame + picIndex;
            }

            if (actualIndex < 0 || actualIndex >= _sprites.Count)
            {
                ClearResolvedSprite();
                return;
            }
            if (_sprites[actualIndex] == null)
            {
                ClearResolvedSprite();
                return;
            }

            _currentEntry = null;
            _entityVisible = true;
            if (_renderer != null)
                _renderer.sprite = _sprites[actualIndex];
            ApplyEntityRendererVisibility();
        }

        public void ClearCurrentSprite()
        {
            _currentPic = 999;
            ClearResolvedSprite();
        }

        private void ClearResolvedSprite()
        {
            _currentEntry = null;
            if (_renderer == null)
                return;

            _renderer.sprite = null;
            _renderer.enabled = false;
        }

        /// <summary>
        /// 切换左右方向。
        /// </summary>
        /// <param name="dir">"left" 或 "right"</param>
        public void SwitchLR(string dir)
        {
            _dir = dir;
            if (_renderer != null)
            {
                _renderer.flipX = (dir == "left");
            }
        }

        /// <summary>
        /// 设置本地显示位置。
        /// </summary>
        public void SetXY(float x, float y)
        {
            _localOffsetPixels = new Vector2(x, y);
            if (_renderer == null) return;
            const float ppu = 100f;
            _renderer.transform.localPosition = new Vector3(x / ppu, -y / ppu, _renderer.transform.localPosition.z);
        }

        /// <summary>
        /// 设置 Z 排序。
        /// 角色有 SortingGroup → 改 SortingGroup.sortingOrder（控制整个角色层级）
        /// 武器/SA 无 SortingGroup → 回退改 SpriteRenderer.sortingOrder
        /// </summary>
        public void SetZ(int order)
        {
            if (_sortingGroup != null)
            {
                _sortingGroup.sortingLayerName = "Object";
                _sortingGroup.sortingOrder = order;
            }

            if (_renderer != null)
            {
                _renderer.sortingLayerName = "Object";
                _renderer.sortingOrder = order;
            }
        }

        public void SetZ(float z)
        {
            SetZ((int)z);
        }

        /// <summary>
        /// 显示精灵。
        /// </summary>
        public void Show()
        {
            _entityVisible = true;
            ApplyEntityRendererVisibility();
        }

        /// <summary>
        /// 隐藏精灵。
        /// </summary>
        public void Hide()
        {
            _entityVisible = false;
            ApplyEntityRendererVisibility();
        }

        public void SetPresentationSuppressed(bool suppressed)
        {
            _presentationSuppressed = suppressed;
            ApplyEntityRendererVisibility();
            ApplyShadowRendererVisibility();
        }

        public void SetLegacyRendererSuppressed(bool suppressed)
        {
            _legacyRendererSuppressed = suppressed;
            ApplyEntityRendererVisibility();
            ApplyShadowRendererVisibility();
        }

        public void SetLegacyEntityVisible(bool visible)
        {
            _legacyEntityVisible = visible;
            ApplyEntityRendererVisibility();
        }

        /// <summary>
        /// 显示阴影
        /// </summary>
        public void ShowShadow()
        {
            _shadowVisible = true;
            ApplyShadowRendererVisibility();
        }

        /// <summary>
        /// 隐藏阴影
        /// </summary>
        public void HideShadow()
        {
            _shadowVisible = false;
            ApplyShadowRendererVisibility();
        }

        /// <summary>
        /// 更新阴影位置（阴影始终在地面）
        /// </summary>
        public void UpdateShadowPosition(float groundX, float groundZ)
        {
            if (_shadowRenderer == null) return;
            var t = _shadowRenderer.transform;
            t.localPosition = new Vector3(groundX, groundZ, t.localPosition.z);
        }

        /// <summary>
        /// 隐藏精灵和阴影。
        /// </summary>
        public void Destroy()
        {
            ClearCurrentSprite();
            Hide();
            HideShadow();
        }

        public void Reset()
        {
            _sprites = null;
            _catalog = null;
            _visualDataId = int.MinValue;
            _startFrame = 0;
            _currentPic = 999;
            _currentEntry = null;
            _dir = "right";
            _entityVisible = false;
            _shadowVisible = false;
            _presentationSuppressed = false;
            _legacyRendererSuppressed = false;
            _legacyEntityVisible = true;
            _localOffsetPixels = Vector2.zero;
            _sortingGroup = null;

            if (_renderer != null)
            {
                _renderer.sprite = null;
                _renderer.flipX = false;
                Vector3 localPosition = _renderer.transform.localPosition;
                _renderer.transform.localPosition = new Vector3(0f, 0f, localPosition.z);
                _renderer.enabled = false;
            }
            if (_shadowRenderer != null)
                _shadowRenderer.enabled = false;

            _renderer = null;
            _shadowRenderer = null;
            _hasShadow = false;
        }

        private void ApplyEntityRendererVisibility()
        {
            if (_renderer == null)
                return;

            _renderer.enabled = _entityVisible &&
                                _legacyEntityVisible &&
                                !_presentationSuppressed &&
                                !_legacyRendererSuppressed &&
                                _renderer.sprite != null;
        }

        private void ApplyShadowRendererVisibility()
        {
            if (_shadowRenderer == null)
                return;

            _shadowRenderer.enabled = _shadowVisible &&
                                      !_presentationSuppressed &&
                                      !_legacyRendererSuppressed;
        }

        /// <summary>
        /// 获取当前精灵宽度（像素）
        /// </summary>
        public float GetWidthPx()
        {
            return _currentEntry?.PixelWidth ?? 0f;
        }

        /// <summary>
        /// 获取当前精灵宽度（像素）- 别名，用于碰撞检测
        /// </summary>
        public float GetCurrentSpriteWidthPx() => GetWidthPx();

        /// <summary>
        /// 获取当前精灵高度（像素）
        /// </summary>
        public float GetHeightPx()
        {
            return _currentEntry?.PixelHeight ?? 0f;
        }

        public BattleSpriteEntry CurrentEntry => _currentEntry;
    }
}


[HEADLESS SESSION] You are running non-interactively in a headless pipeline. Produce your FULL, comprehensive analysis directly in your response. Do NOT ask for clarification or confirmation - work thoroughly with all provided context. Do NOT write brief acknowledgments - your response IS the deliverable.

Role: performance architect, read-only analysis.

Analyze RenderDispatch after removing the redundant renderer snapshot sort. Current 1000 dispersed result:
`Temp/NTSD_ProductionEntityStress.dispersed-ai-nosort-render-sort-20260725.json`
with RenderDispatch average ~48.09 ms.

Trace and quantify the remaining work across:
- `SimulationWorld.RenderDispatchAll`
- `BattlePresentation.BeginFrame`
- `BattleCentralRenderSystem.PrepareFrame`
- `LateRendererUpdateAll`
- `LF2ObjectRenderer.SimLateTick`

The stress run is expected to use the project's actual active presentation mode. Verify the mode from code/report rather than assuming it.

Rank behavior-equivalent optimizations, especially reuse of a single ordered entity view, HitRecord-only sorting, caching immutable sprite/catalog/binding data, and change guards for Unity native property writes. Explain which changes are safe for pooled objects, identity swaps, opoint first-presentation timing, shadows, held weapons, and hit-stop blinking.

Do not edit files. Give exact files/methods/lines and required focused tests.
