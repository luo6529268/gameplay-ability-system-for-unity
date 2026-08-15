using System;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation.Ecs;

namespace NTSD.Simulation
{
    /// <summary>
    /// World-owned object-point playback for the pure simulation path. It keeps
    /// the authority's queue and cursor-local spawn boundaries without touching
    /// MonoBehaviour, GameObject, prefab, renderer, or a Unity singleton.
    /// </summary>
    internal sealed class BattleLogicObjectPointRuntime :
        ILF2ObjectPointFactory,
        IBattleObjectPointStructuralMaterializer
    {
        private readonly SimulationWorld world;
        private readonly LF2TaskRingBuffer taskQueue;
        private readonly LF2Entity[] spawnedBuffer;
        private int spawnedCount;

        internal BattleLogicObjectPointRuntime(
            SimulationWorld world,
            int taskCapacity)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
            if (taskCapacity < 1)
                throw new ArgumentOutOfRangeException(nameof(taskCapacity));

            taskQueue = new LF2TaskRingBuffer(taskCapacity);
            spawnedBuffer = new LF2Entity[taskCapacity];
        }

        internal int PendingTaskCountForDiagnostics => taskQueue.Count;
        internal int TaskQueueCapacityForDiagnostics => taskQueue.Capacity;
        internal long RejectedTaskCountForDiagnostics =>
            taskQueue.RejectedEnqueueCount;
        internal long UnknownTaskTypeCountForDiagnostics { get; private set; }
        internal long MissingObjectDefinitionCountForDiagnostics
        {
            get;
            private set;
        }

        public void EnqueueCreateObject(OPointCreateTask task)
        {
            if (!taskQueue.TryEnqueue(task))
                world.LogicReferencePool?.Recycle(task);
        }

        public void EnqueueCreateMultipleObjects(OPointCreateMultipleTask task)
        {
            if (!taskQueue.TryEnqueue(task))
                world.LogicReferencePool?.Recycle(task);
        }

        internal void PrepareTaskQueueCapacity(int capacity)
        {
            taskQueue.EnsureCapacity(capacity);
        }

        internal void SealBattleTaskCapacity()
        {
            taskQueue.SealCapacity();
        }

        internal void UnsealBattleTaskCapacity()
        {
            taskQueue.UnsealCapacity();
        }

        public void FlushTasks()
        {
            int taskCount = taskQueue.Count;
            for (int index = 0; index < taskCount; index++)
            {
                if (!taskQueue.TryDequeue(out LF2TaskBase task))
                    break;

                ProcessTask(task);
            }
        }

        public LF2Entity CreateObjectImmediate(OPointCreateTask task)
        {
            return ProcessCreateObject(
                task,
                BattleStructuralPlaybackBoundary.CurrentEntityImmediate);
        }

        private void ProcessTask(LF2TaskBase task)
        {
            switch (task?.TaskType)
            {
                case LF2TaskType.CreateObject:
                    ProcessCreateObject(
                        (OPointCreateTask)task,
                        BattleStructuralPlaybackBoundary.CurrentPassSegment);
                    break;
                case LF2TaskType.CreateMultipleObjects:
                    ProcessCreateMultipleObjects(
                        (OPointCreateMultipleTask)task,
                        BattleStructuralPlaybackBoundary.CurrentPassSegment);
                    break;
                default:
                    UnknownTaskTypeCountForDiagnostics++;
                    break;
            }

            if (task is ILF2Recyclable recyclable)
                world.LogicReferencePool?.Recycle(recyclable);
        }

        public void ProcessOpointSpawnCoreForStructuralWriter(LF2Entity spawner)
        {
            if (spawner?.PS == null || spawner.Runtime == null)
                return;

            LF2FrameData frame = spawner.Frame?.D;
            if (frame == null)
                return;

            bool hasList = frame.opoints != null && frame.opoints.Count > 0;
            bool hasSingle = frame.opoint.HasValue;
            if (!hasList && !hasSingle)
                return;

            ObjectPoint firstOpoint = hasList
                ? frame.opoints[0]
                : frame.opoint.Value;
            if (firstOpoint.kind <= 0 ||
                firstOpoint.oid <= 0 ||
                spawner.AttackingCounter != 0)
            {
                return;
            }
            if (spawner.FrameDelay != 0 &&
                spawner.GetCurrentDataObjectTypeForSimulation() ==
                    (int)LF2ObjectType.Character)
            {
                return;
            }

            if (hasList)
            {
                for (int index = 0; index < frame.opoints.Count; index++)
                {
                    ClearSpawnedBuffer();
                    ProcessOneLateOpoint(spawner, frame, frame.opoints[index]);
                    ApplyMultiSpawnExemptAndVrest();
                }
            }
            else
            {
                ClearSpawnedBuffer();
                ProcessOneLateOpoint(spawner, frame, frame.opoint.Value);
                ApplyMultiSpawnExemptAndVrest();
            }

            ClearSpawnedBuffer();
        }

        public LF2Entity MaterializeObjectForStructuralWriter(
            OPointCreateTask task)
        {
            LF2Entity entity = world.LogicEntityFactory.Create(
                task,
                out BattleLogicEntityCreationFailure failure);
            if (failure == BattleLogicEntityCreationFailure.MissingObjectDefinition)
                MissingObjectDefinitionCountForDiagnostics++;
            return entity;
        }

        public void MaterializeMultipleObjectsForStructuralWriter(
            OPointCreateMultipleTask task)
        {
            if (task == null || task.number <= 0)
                return;

            BattleLogicReferencePool referencePool = world.LogicReferencePool;
            for (int spawnIndex = 0; spawnIndex < task.number; spawnIndex++)
            {
                float spreadDvz = task.number == 1
                    ? 0f
                    : spawnIndex * 10f / (task.number - 1) - 5f;
                OPointCreateTask singleTask =
                    referencePool?.Fetch<OPointCreateTask>();
                if (singleTask == null)
                    break;

                CopyMultipleTaskToSingle(task, singleTask, spreadDvz);
                world.LogicEntityFactory.Create(
                    singleTask,
                    out BattleLogicEntityCreationFailure failure,
                    spreadDvz);
                if (failure ==
                    BattleLogicEntityCreationFailure.MissingObjectDefinition)
                {
                    MissingObjectDefinitionCountForDiagnostics++;
                }
                referencePool.Recycle(singleTask);
            }
        }

        private void ProcessOneLateOpoint(
            LF2Entity spawner,
            LF2FrameData frame,
            ObjectPoint opoint)
        {
            if (opoint.kind <= 0 || opoint.oid <= 0)
                return;

            int spawnCount = 1;
            int facingMode = opoint.facing;
            if (opoint.facing > 10)
            {
                spawnCount = opoint.facing / 10;
                facingMode = opoint.facing % 10;
            }

            for (int spawnIndex = 0; spawnIndex < spawnCount; spawnIndex++)
            {
                int requiredRuntimeSlot =
                    world.FindFirstFreeFrameLogicRuntimeSlot();
                if (requiredRuntimeSlot < 0)
                    continue;

                OPointCreateTask task =
                    world.LogicReferencePool?.Fetch<OPointCreateTask>();
                if (task == null)
                    break;

                ObjectPoint spawnOpoint = opoint;
                spawnOpoint.facing = facingMode;
                task.opoint = spawnOpoint;
                task.parent = spawner;
                task.targetWorld = world;
                task.team = spawner.Team;
                ConfigureLateOpointPosition(task, spawner, frame, opoint);
                task.dir = spawner.PS.dir;
                task.dvz = 0f;
                task.preserveActionZero = true;
                task.releaseSpawnSemantic = ReleaseSpawnSemantic.LateOpoint;
                task.releaseOpointSpawn = true;
                task.requiredRuntimeSlot = requiredRuntimeSlot;

                LF2Entity spawned = ProcessCreateObject(
                    task,
                    BattleStructuralPlaybackBoundary.CurrentEntityImmediate);
                world.LogicReferencePool.Recycle(task);
                if (spawned == null)
                    continue;

                if (opoint.kind != 2)
                    spawned.Runtime.HolderStableId = 0;

                if (spawnCount > 1)
                {
                    float spread =
                        spawnIndex * 10f / (spawnCount - 1) - 5f;
                    spawned.PS.vz += spread;
                    float absoluteSpread = Math.Abs(spread);
                    if (spawned.PS.vx > 0f)
                        spawned.PS.vx -= absoluteSpread;
                    else if (spawned.PS.vx < 0f)
                        spawned.PS.vx += absoluteSpread;
                    else
                        spawned.PS.vx += spread;
                }

                if (spawner.GetCurrentDataObjectTypeForSimulation() == 3 &&
                    frame.state == 3003)
                {
                    ApplyState3003LinkedVrest(spawner, spawned);
                }

                spawned.AttackExempt = 0;
                AddSpawned(spawned);
            }
        }

        private LF2Entity ProcessCreateObject(
            OPointCreateTask task,
            BattleStructuralPlaybackBoundary boundary)
        {
            return world.StructuralWriter.Spawn(this, task, boundary);
        }

        private void ProcessCreateMultipleObjects(
            OPointCreateMultipleTask task,
            BattleStructuralPlaybackBoundary boundary)
        {
            world.StructuralWriter.SpawnMultiple(this, task, boundary);
        }

        private static void ConfigureLateOpointPosition(
            OPointCreateTask task,
            LF2Entity spawner,
            LF2FrameData frame,
            ObjectPoint opoint)
        {
            int spawnX = spawner.Runtime.Dir == "right"
                ? spawner.Runtime.XInt - frame.centerx + opoint.x
                : spawner.Runtime.XInt + frame.centerx - opoint.x;
            int spawnY = spawner.Runtime.YInt - frame.centery + opoint.y;
            double spawnZ = spawner.Runtime.Z + 1.0;

            task.z = (float)spawnZ;
            task.useDirectRuntimePosition = true;
            task.directX = spawnX;
            task.directY = spawnY;
            task.directZ = spawnZ;
            task.skipPostInitZOffset = true;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = spawnX;
            task.initialRuntimeY = spawnY;
            task.initialRuntimeZ = (int)spawnZ;
        }

        private static void CopyMultipleTaskToSingle(
            OPointCreateMultipleTask source,
            OPointCreateTask destination,
            float spreadDvz)
        {
            destination.opoint = source.opoint;
            destination.parent = source.parent;
            destination.targetWorld = source.targetWorld;
            destination.team = source.team;
            destination.pos = source.pos;
            destination.z = source.z;
            destination.dir = source.dir;
            destination.dvz = spreadDvz;
            destination.useDirectVelocity = source.useDirectVelocity;
            destination.directVx = source.directVx;
            destination.directVy = source.directVy;
            destination.directVz = source.directVz;
            destination.preserveActionZero = source.preserveActionZero;
            destination.skipPostInitZOffset = false;
            destination.ownerEntityIndex = source.ownerEntityIndex;
            destination.frameDelay = source.frameDelay;
            destination.attackExempt = source.attackExempt;
            destination.releaseOpointSpawn = source.releaseOpointSpawn;
        }

        private void AddSpawned(LF2Entity entity)
        {
            if (entity == null || spawnedCount >= spawnedBuffer.Length)
                return;
            spawnedBuffer[spawnedCount++] = entity;
        }

        private void ClearSpawnedBuffer()
        {
            for (int index = 0; index < spawnedCount; index++)
                spawnedBuffer[index] = null;
            spawnedCount = 0;
        }

        private void ApplyMultiSpawnExemptAndVrest()
        {
            if (spawnedCount <= 1)
                return;

            int center = spawnedCount / 2;
            for (int index = 0; index < spawnedCount; index++)
            {
                LF2Entity entity = spawnedBuffer[index];
                if (entity == null)
                    continue;

                if ((spawnedCount & 1) == 0)
                {
                    if (index < center - 1)
                        entity.AttackExempt = (center - index - 1) * 2;
                    else if (index > center)
                        entity.AttackExempt = (index - center) * 2;
                }
                else
                {
                    if (index < center)
                        entity.AttackExempt = (center - index) * 2;
                    else if (index > center)
                        entity.AttackExempt = (index - center) * 2;
                }

                for (int previousIndex = 0;
                     previousIndex < index;
                     previousIndex++)
                {
                    LF2Entity other = spawnedBuffer[previousIndex];
                    if (other == null)
                        continue;
                    int entitySlot = entity.Runtime?.SlotIndex ?? -1;
                    int otherSlot = other.Runtime?.SlotIndex ?? -1;
                    if (entitySlot < 0 || otherSlot < 0)
                        continue;
                    entity.ItrRest?.SetVrest(otherSlot, 40);
                    other.ItrRest?.SetVrest(entitySlot, 40);
                }
            }
        }

        private static void ApplyState3003LinkedVrest(
            LF2Entity spawner,
            LF2Entity spawned)
        {
            int linkedSlot = spawner?.Runtime?.AnimCounter ?? -1;
            int spawnedSlot = spawned?.Runtime?.SlotIndex ?? -1;
            if (linkedSlot < 0 || spawnedSlot < 0)
                return;

            LF2Entity linked =
                spawner.Match?.FindEntityByRuntimeSlotForQuery(linkedSlot);
            if (linked == null)
                return;
            linked.ItrRest?.SetVrest(spawnedSlot, 10);
            spawned.ItrRest?.SetVrest(linkedSlot, 10);
        }
    }
}
