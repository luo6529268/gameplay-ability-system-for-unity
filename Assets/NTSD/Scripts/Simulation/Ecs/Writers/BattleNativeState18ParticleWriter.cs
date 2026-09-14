using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;

namespace NTSD.Simulation.Ecs
{
    internal static class BattleNativeState18ParticleWriter
    {
        internal static bool Materialize(LF2Entity source, int globalDelay = 0)
        {
            var world = source?.Match;
            if (world == null || !world.StructuralWriter.AcceptingStructuralCreatesForDiagnostics)
                return false;
            var previous = source.FrameCache?.GetNativeFrameDataById(source.Frame.Prev);
            var current = source.FrameCache?.GetNativeFrameDataById(source.Frame.N);
            if (previous == null || current == null)
                return false;

            int count = BattleNativeState18ParticleKernel.ResolvePreRollCount(previous.state, current.state, globalDelay);
            var random = world.NativeRandom;
            if (count < 0)
                count = random.SynchronizedNext(0x00416A40u, 4) == 0 ? 1 : 0;
            if (count <= 0 || world.RuntimeDataCatalog.GetObjectDefinition(999) == null ||
                world.RuntimeDataCatalog.GetCharacterConfig(999)?.characterData == null)
                return false;
            var factory = world.ResolveLateObjectPointStructuralMaterializerForModule();
            var pool = world.LogicReferencePool;
            if (factory == null || pool == null)
                return false;

            int x = source.Runtime.XInt, y = source.Runtime.YInt, z = source.Runtime.ZInt;
            double preciseX = source.Runtime.X, preciseY = source.Runtime.Y, preciseZ = source.Runtime.Z;
            double motionX = source.Runtime.Vx;
            bool spawned = false;
            for (int ordinal = 0; ordinal < count; ordinal++)
            {
                int slot = -1;
                // Alignment contract: NTSD28-Q06-C25L-STATE18-SPAWN-TRANSACTION-001.
                // Native scans absent entities, including reserved fusion slots; admission may then fail.
                for (int candidate = 50; candidate < world.RuntimeSlotCapacity; candidate++)
                {
                    if (world.FindEntityByRuntimeSlotForNativeDisplay(candidate) == null)
                    {
                        slot = candidate;
                        break;
                    }
                }
                if (slot < 0) break;
                int dy = random.SynchronizedNext(0x004212DEu, 29);
                int dx = random.SynchronizedNext(0x004212FEu, 59) - 29;
                int dvx = random.SynchronizedNext(0x00421339u, 11) - 5;
                int action = random.SynchronizedNext(0x00421367u, 1) + 140;
                var task = pool.Fetch<OPointCreateTask>();
                if (task == null) break;
                LF2Entity particle;
                try
                {
                    task.targetWorld = world;
                    task.opoint = new ObjectPoint { oid = 999, action = action, kind = 1 };
                    // Both broken-weapon producers use native generic birth, without OPoint percentages.
                    task.nativeWeaponPieceSpawn = true;
                    task.releaseSpawnSemantic = ReleaseSpawnSemantic.TransitionEffect;
                    task.requiredRuntimeSlot = slot;
                    task.preserveActionZero = true;
                    task.dir = "right";
                    task.ownerEntityIndex = -1;
                    task.useExplicitRelationIdentity = true;
                    task.relationTeam = 0;
                    task.useDirectRuntimePosition = true;
                    task.directX = preciseX + dx; task.directY = preciseY - dy; task.directZ = preciseZ;
                    task.useInitialRuntimeIntPosition = true;
                    task.initialRuntimeX = x; task.initialRuntimeY = y; task.initialRuntimeZ = z;
                    task.skipPostInitZOffset = true;
                    task.useDirectVelocity = true;
                    task.directVx = motionX + dvx; task.directVy = -1; task.directVz = 0;
                    particle = world.StructuralWriter.Spawn(factory, task, BattleStructuralPlaybackBoundary.CurrentEntityImmediate);
                }
                finally { pool.Recycle(task); }
                if (particle == null) break;
                spawned = true;
            }
            return spawned;
        }
    }
}
