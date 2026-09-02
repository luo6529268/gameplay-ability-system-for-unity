using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// Owns normal and mode-2 random weapon drop selection and materialization.
    /// </summary>
    internal sealed class BattleRandomWeaponDropModule
    {
        private readonly SimulationWorld world;
        private readonly SimulationRandomWeaponDropBuffer candidates =
            new SimulationRandomWeaponDropBuffer();

        internal BattleRandomWeaponDropModule(SimulationWorld world)
        {
            this.world = world;
        }

        internal void RunNormalDrop(int tickIndex)
        {
            int weaponCount = 0;
            foreach (LF2Entity entity in
                     world.ActiveEntitiesByRuntimeSlotForModule)
            {
                if (entity.CountsAsRandomWeaponDropCandidate())
                    weaponCount++;
            }
            if (weaponCount >= 4)
                return;
            if (world.Rng.NextInt(0, 200) != 0)
                return;

            int freeSlot = world.FindFirstFreeRuntimeSlotForModule(
                world.DynamicRuntimeSlotStartForServices,
                world.RuntimeSlotCapacity);
            if (freeSlot < 0)
                return;

            IReadOnlyList<ObjectDefinition> loadedObjects =
                world.GetRandomWeaponLoadedObjectsForModule();
            if (loadedObjects == null)
                return;

            candidates.Reset();
            for (int i = 0; i < loadedObjects.Count; i++)
            {
                ObjectDefinition definition = loadedObjects[i];
                if (definition == null)
                    continue;
                int oid = definition.id;
                if (!candidates.TryMarkUnique(oid))
                    continue;
                LF2CharacterDataWrapper wrapper =
                    world.ResolveRandomWeaponCharacterConfigForModule(oid);
                if (wrapper == null)
                    continue;
                if (oid == 122 || oid == 123)
                {
                    if (world.Rng.NextInt(0, 2) == 0 ||
                        (world.BattleGameModeId >= 1 &&
                         world.BattleGameModeId <= 4))
                    {
                        continue;
                    }
                }
                candidates.TryAdd(oid);
            }
            if (candidates.Count == 0)
                return;

            int selectedOid =
                candidates[world.Rng.NextInt(0, candidates.Count)];
            ILF2ObjectPointFactory factory =
                world.ResolveObjectPointFactoryForSimulation();
            BattleLogicReferencePool referencePool = world.LogicReferencePool;
            if (factory == null || referencePool == null)
                return;

            BattleStageRuntimeState stage = world.Runtime?.Stage;
            int xMaxOverride = stage?.XMaxOverride ?? 0;
            int stageWidth = stage?.BaseStageWidthPx ?? 800;
            int zMin = stage?.ZMin ?? 180;
            int zMax = stage?.ZMax ?? 350;
            int r1 = world.Rng.NextInt(0, 30);
            int xBase =
                xMaxOverride == 0 ? stageWidth - 60 : xMaxOverride - 60;
            int xStep = xBase / 30;
            int r2 = world.Rng.NextInt(0, 30);
            int r3 = world.Rng.NextInt(0, 30);
            int zBase = zMax - zMin - 60;
            int zStep = zBase / 30;
            int r4 = world.Rng.NextInt(0, 30);
            double lf2X = r1 * xStep + r2 + 30;
            double lf2Z = r3 * zStep + r4 + zMin + 30;
            const double lf2Y = -500.0;

            OPointCreateTask spawnTask =
                referencePool.Fetch<OPointCreateTask>();
            if (spawnTask == null)
                return;
            spawnTask.opoint = new ObjectPoint
            {
                oid = selectedOid,
                kind = 0,
                action = 0,
                x = (int)lf2X,
                y = (int)lf2Y,
                dvx = 0,
                dvy = 0,
                facing = 0,
            };
            spawnTask.parent = null;
            spawnTask.team = 0;
            spawnTask.requiredRuntimeSlot = freeSlot;
            spawnTask.pos = new Vector3((float)lf2X, (float)lf2Y, 0f);
            spawnTask.z = (float)lf2Z;
            spawnTask.dir = "right";
            spawnTask.dvz = 0f;
            spawnTask.preserveActionZero = true;
            spawnTask.skipPostInitZOffset = true;
            spawnTask.useDirectRuntimePosition = true;
            spawnTask.directX = lf2X;
            spawnTask.directY = lf2Y;
            spawnTask.directZ = lf2Z;
            spawnTask.useDirectVelocity = true;
            spawnTask.directVx = 0.0;
            spawnTask.directVy = 0.0;
            spawnTask.directVz = 0.0;
            spawnTask.useInitialRuntimeIntPosition = true;
            spawnTask.initialRuntimeX = (int)lf2X;
            spawnTask.initialRuntimeY = (int)lf2Y;
            spawnTask.initialRuntimeZ = (int)lf2Z;
            spawnTask.targetWorld = world;

            LF2Entity spawned;
            try
            {
                spawned = factory.CreateObjectImmediate(spawnTask);
            }
            finally
            {
                referencePool.Recycle(spawnTask);
            }

            if (spawned == null || spawned.Runtime?.SlotIndex != freeSlot)
                return;

            spawned.Health.HP = selectedOid == 122 ? 200 : 500;
            spawned.Health.HPBound = 500;
            spawned.Health.HP3 = 500;
            spawned.Health.PP = 500;
            spawned.KillCount = -1;
            world.ResetCooldownsForRuntimeSlot(freeSlot, spawned);
            spawned.RefreshRuntimeSnapshot();
        }

        internal void RunMode2Tail(int tickIndex)
        {
            int mode2Request = world.Mode2Request;
            if (mode2Request == 0)
                return;

            if (mode2Request == 1)
            {
                SpawnMode2RandomWeapons();
            }
            else if (mode2Request == 2)
            {
                foreach (LF2Entity entity in
                         world.ActiveEntitiesByRuntimeSlotForModule)
                {
                    if (!entity.CountsAsRandomWeaponDropCandidate())
                        continue;

                    entity.Runtime.WeaponFlightCounter = -1;
                    world.RefreshRuntimeSnapshotForModule(entity);
                }
            }
        }

        private void SpawnMode2RandomWeapons()
        {
            if (!world.HasRandomWeaponCharacterConfigSourceForModule())
                return;

            candidates.Reset();
            for (int oid = 100; oid < 200; oid++)
            {
                LF2CharacterDataWrapper wrapper =
                    world.ResolveRandomWeaponCharacterConfigForModule(oid);
                if (wrapper == null)
                    continue;

                if (oid == 122 && world.Rng.NextInt(0, 2) == 0)
                    continue;

                candidates.TryAdd(oid);
            }

            if (candidates.Count == 0)
                return;

            BattleStageRuntimeState stage = world.Runtime?.Stage;
            int stageWidth = stage?.BaseStageWidthPx ?? 800;
            int zMin = stage?.ZMin ?? 180;
            int zMax = stage?.ZMax ?? 350;
            if (stageWidth <= 60 || zMax - zMin <= 60)
                return;

            ILF2ObjectPointFactory factory =
                world.ResolveObjectPointFactoryForSimulation();
            if (factory == null)
                return;

            for (int chooseIndex = 0;
                 chooseIndex < candidates.Count;
                 chooseIndex++)
            {
                int oid = candidates[chooseIndex];

                bool hasFreeSlot = false;
                for (int slot = world.DynamicRuntimeSlotStartForServices;
                     slot < world.RuntimeSlotCapacity;
                     slot++)
                {
                    if (!world.IsRuntimeSlotClaimedForRandomWeaponModule(slot))
                    {
                        hasFreeSlot = true;
                        break;
                    }
                }

                if (!hasFreeSlot)
                    break;

                int r1 = world.Rng.NextInt(0, 30);
                int r2 = world.Rng.NextInt(0, 30);
                int r3 = world.Rng.NextInt(0, 30);
                int r4 = world.Rng.NextInt(0, 30);
                float lf2X =
                    r1 * ((stageWidth - 60) / 30) + r2 + 30;
                float lf2Z =
                    r3 * ((zMax - zMin - 60) / 30) + r4 + zMin + 30;
                const float lf2Y = -500f;

                LF2CharacterData charData =
                    world.ResolveRandomWeaponCharacterDataForModule(oid);
                int flyFrame = -1;
                int minFrame = int.MaxValue;
                if (charData?.frames != null)
                {
                    foreach (LF2FrameData frame in charData.frames)
                    {
                        if (frame == null)
                            continue;
                        if (frame.frameId > 0 && frame.frameId < minFrame)
                            minFrame = frame.frameId;
                        if (flyFrame < 0 && frame.frameId > 0 &&
                            (frame.state == LF2States.WeaponInSky ||
                             frame.state == LF2States.WeaponThrowing ||
                             frame.state == LF2States.HeavyWeaponInSky))
                        {
                            flyFrame = frame.frameId;
                        }
                    }
                }

                if (flyFrame < 0)
                    flyFrame = minFrame != int.MaxValue ? minFrame : 0;

                BattleLogicReferencePool referencePool = world.LogicReferencePool;
                if (referencePool == null)
                    break;

                OPointCreateTask spawnTask =
                    referencePool.Fetch<OPointCreateTask>();
                if (spawnTask == null)
                    break;
                spawnTask.opoint = new ObjectPoint
                {
                    oid = oid,
                    kind = 0,
                    action = flyFrame,
                    x = Mathf.RoundToInt(lf2X),
                    y = Mathf.RoundToInt(lf2Y),
                    dvx = 0,
                    dvy = 0,
                    facing = 0,
                };
                spawnTask.parent = null;
                spawnTask.team = 0;
                spawnTask.pos = new Vector3(lf2X, lf2Y, 0f);
                spawnTask.z = lf2Z;
                spawnTask.dir = "right";
                spawnTask.dvz = 0f;
                spawnTask.targetWorld = world;
                try
                {
                    factory.CreateObjectImmediate(spawnTask);
                }
                finally
                {
                    referencePool.Recycle(spawnTask);
                }
            }
        }
    }
}
