using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;

namespace NTSD.Simulation.Ecs
{
    internal static class BattleNativeWeaponPieceWriter
    {
        internal static void Materialize(LF2Entity source)
        {
            SimulationWorld world = source?.Match;
            if (world == null || !world.StructuralWriter.AcceptingStructuralCreatesForDiagnostics)
                return;
            var factory = world.ResolveLateObjectPointStructuralMaterializerForModule();
            if (factory == null || world.LogicReferencePool == null)
                return;

            // Alignment contract: NTSD28-Q06-NATIVE-WEAPON-PIECE-TRANSACTION-001.
            int oid = source.ObjectId;
            int x = source.Runtime.XInt, y = source.Runtime.YInt, z = source.Runtime.ZInt;
            var random = world.NativeRandom;
            int count = LF2Entity.BrokenWeaponFragmentCount(oid);
            if (HasDefinition(world, 999))
            {
                for (int ordinal = 0; ordinal < count; ordinal++)
                {
                    int slot = world.FindFirstFreeRuntimeSlotForModule(50, world.RuntimeSlotCapacity);
                    if (slot < 0) break;
                    int dx = random.SynchronizedNext(0x00420581u, 7) - 3;
                    int dy = random.SynchronizedNext(0x004205A5u, 7) - 3;
                    int vy = oid == 150 || oid == 151 || oid == 213
                        ? -(random.SynchronizedNext(0x0042062Fu, 20) / 2) - 8
                        : -(random.SynchronizedNext(0x004206A8u, 8) / 2) - 6;
                    int vx = random.SynchronizedNext(0x004206D6u, 11) - 5;
                    int action = BuiltinAction(random, oid, ordinal, ref vy);
                    if (Spawn(world, factory, slot, 999, action, x + dx, y + dy, z,
                        vx, vy, 0, -1, 0, false) == null) break;
                }
            }

            var block = source.FrameCache?.Wrapper?.characterData?.NativeMetadata?.WeaponPiece;
            if (block == null) return;
            int groupId = block.Fields.Int32OrDefault("team", 0) != 0 ? source.RelationTeam : 0;
            foreach (var group in block.Groups)
            {
                int amount = group.Fields.Int32OrDefault("amount", 0);
                for (int ordinal = 0; ordinal < amount; ordinal++)
                {
                    if (group.Variants.Count == 0) continue;
                    int variant = group.Variants.Count > 1
                        ? random.SynchronizedNext(0x004162F6u, group.Variants.Count) : 0;
                    var fields = group.Variants[variant].Fields;
                    int target = fields.Int32OrDefault("oid", -1);
                    if (target == -1) target = 999;
                    if (!HasDefinition(world, target)) continue;
                    int slot = world.FindFirstFreeRuntimeSlotForModule(50, world.RuntimeSlotCapacity);
                    if (slot < 0) continue;
                    int dz = random.SynchronizedNext(0x00416362u, 7) - 3;
                    int dy = random.SynchronizedNext(0x00416370u, 7) - 3;
                    int dx = random.SynchronizedNext(0x0041637Eu, 7) - 3;
                    int action = fields.Int32OrDefault("act", 0) +
                        random.SynchronizedNext(0x004163EBu, fields.Int32OrDefault("framea", 0));
                    bool negateX = random.SynchronizedNext(0x004163F9u, 2) != 0;
                    int vx = random.SynchronizedNext(0x00416401u, fields.Int32OrDefault("dvx", 0));
                    if (negateX) vx = -vx;
                    int dvy = fields.Int32OrDefault("dvy", 0);
                    int vy = random.SynchronizedNext(0x00416424u, dvy < 0 ? -dvy : dvy);
                    if (dvy < 0) vy = -vy;
                    bool negateZ = random.SynchronizedNext(0x00416436u, 2) != 0;
                    int vz = random.SynchronizedNext(0x0041643Eu, fields.Int32OrDefault("dvz", 0));
                    if (negateZ) vz = -vz;
                    Spawn(world, factory, slot, target, action, x + dx, y + dy, z + dz,
                        vx, vy, vz, source.OwnerEntityIndex, groupId, source.Runtime.IsFacingLeft);
                }
            }
        }

        private static bool HasDefinition(SimulationWorld world, int oid)
        {
            return world.RuntimeDataCatalog.GetObjectDefinition(oid) != null &&
                world.RuntimeDataCatalog.GetCharacterConfig(oid)?.characterData != null;
        }

        private static LF2Entity Spawn(SimulationWorld world,
            IBattleObjectPointStructuralMaterializer factory, int slot, int oid, int action,
            int x, int y, int z, int vx, int vy, int vz, int owner, int group, bool facingLeft)
        {
            var wrapper = world.RuntimeDataCatalog.GetCharacterConfig(oid);
            if (!IsInitialActionAdmitted(wrapper, action))
                return null;
            var pool = world.LogicReferencePool;
            var task = pool.Fetch<OPointCreateTask>();
            if (task == null) return null;
            try
            {
                task.targetWorld = world;
                task.opoint = new ObjectPoint { oid = oid, action = action, kind = 1 };
                task.nativeWeaponPieceSpawn = true;
                task.releaseSpawnSemantic = ReleaseSpawnSemantic.BrokenFragment;
                task.requiredRuntimeSlot = slot;
                task.preserveActionZero = true;
                task.useDirectRuntimePosition = true;
                task.directX = x; task.directY = y; task.directZ = z;
                task.useInitialRuntimeIntPosition = true;
                task.initialRuntimeX = x; task.initialRuntimeY = y; task.initialRuntimeZ = z;
                task.skipPostInitZOffset = true;
                task.useDirectVelocity = true;
                task.directVx = vx; task.directVy = vy; task.directVz = vz;
                task.ownerEntityIndex = owner;
                task.useExplicitRelationIdentity = true;
                task.relationTeam = group;
                task.dir = facingLeft ? "left" : "right";
                return world.StructuralWriter.Spawn(factory, task,
                    BattleStructuralPlaybackBoundary.CurrentEntityImmediate);
            }
            finally { pool.Recycle(task); }
        }

        internal static bool IsInitialActionAdmitted(LF2CharacterDataWrapper wrapper, int action)
        {
            // Alignment contract: NTSD28-Q06-WEAPON-PIECE-SPAWN-ADMISSION-EDGE-001.
            return wrapper?.characterData != null && action >= 0 &&
                action < LF2FrameCache.NativeMaxFrameIdExclusive &&
                (action != 999 || wrapper.characterData.frames.Exists(frame => frame.frameId == action));
        }

        internal static void InitializeBirth(LF2Entity entity, OPointCreateTask task)
        {
            var data = entity.FrameCache.Wrapper.characterData;
            var runtime = entity.Runtime;
            entity.RelationTeam = task.relationTeam;
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.Health.HP3 = 500;
            entity.Health.PP = 500;
            entity.Health.MaxMP = data.NativeMetadata?.Stats.Int32OrDefault("max_mp", 500) ?? 500;
            runtime.HP2Orig = 1;
            runtime.HPOrig = 0;
            runtime.RespawnCount = 0;
            runtime.DisplayCurrentHp200 = 500;
            runtime.DisplayEffectiveMaxHp208 = 500;
            runtime.DisplayScore1F0 = 0;
            runtime.DisplayDamageTotal1F8 = 0;
            runtime.DisplayScoreStep1F4 = 0;
            runtime.DisplayDamageStep1FC = 0;
            runtime.DisplayCurrentHpStep204 = 0;
            runtime.DisplayEffectiveMaxHpStep20C = 0;
            runtime.WeaponFlightCounter = data.NativeMetadata?.Bmp.Int32OrDefault("weapon_hp", 0) ?? data.weapon_hp;
            entity.WriteCurrentFrameId(task.opoint.action);
            entity.Frame.D = entity.FrameCache.GetNativeFrameDataById(task.opoint.action);
            entity.Trans.SyncDirectFrameData(entity.Frame.D.wait, entity.Frame.D.next, task.opoint.action);
            entity.Frame.Prev = task.opoint.action;
            entity.AttackingCounter = 0;
            entity.SyncCollisionSnapshotToCurrentFrame();
            runtime.NativeSoundActionLatch = -1;
            runtime.NativeLifecycleResolutionPending = false;
            runtime.NativeLifecycleCode = 0;
            runtime.NativeRuntimeStateCode = 0;
        }

        private static int BuiltinAction(NTSD28NativeRandom random, int oid, int ordinal, ref int vy)
        {
            switch (oid)
            {
                case 150: return ordinal < 5 ? random.SynchronizedNext(0x0042071Du, 4) : random.SynchronizedNext(0x00420736u, 4) + 4;
                case 100:
                case 201: return ordinal < 2 ? random.SynchronizedNext(0x0042076Fu, 4) + 10 : random.SynchronizedNext(0x0042078Bu, 4) + 14;
                case 213: return ordinal < 2 ? random.SynchronizedNext(0x004207C7u, 4) + 150 : random.SynchronizedNext(0x004207E5u, 4) + 154;
                case 101:
                    if (ordinal < 5)
                    {
                        int basis = random.SynchronizedNext(0x00420820u, 2) * 4 + 20;
                        return basis + random.SynchronizedNext(0x00420837u, 4);
                    }
                    return random.SynchronizedNext(0x00420856u, 4) + 30;
                case 151:
                    if (ordinal < 2) return random.SynchronizedNext(0x00420898u, 4) + 40;
                    if (ordinal < 5) return random.SynchronizedNext(0x004208B9u, 4) + 44;
                    if (ordinal < 8) return random.SynchronizedNext(0x004208D0u, 4) + 50;
                    return random.SynchronizedNext(0x004208ECu, 4) + 54;
                case 120: return ordinal < 2 ? random.SynchronizedNext(0x00420927u, 4) + 54 : random.SynchronizedNext(0x0042094Au, 4) + 30;
                case 124: return random.SynchronizedNext(0x0042097Cu, 4) + 170;
                case 121: return random.SynchronizedNext(0x004209B0u, 4) + 60;
                case 122:
                    if (ordinal < 1) return random.SynchronizedNext(0x004209EFu, 4) + 70;
                    if (ordinal < 3) return random.SynchronizedNext(0x00420A10u, 4) + 80;
                    int action122 = random.SynchronizedNext(0x00420A2Cu, 4) + 74;
                    vy = -(random.SynchronizedNext(0x00420A45u, 18) / 2) - 4;
                    return action122;
                case 123:
                    if (ordinal < 1) return random.SynchronizedNext(0x00420A96u, 4) + 160;
                    if (ordinal < 3) return random.SynchronizedNext(0x00420AB9u, 4) + 164;
                    int action123 = random.SynchronizedNext(0x00420AD7u, 4) + 74;
                    vy = -(random.SynchronizedNext(0x00420AF0u, 18) / 2) - 4;
                    return action123;
                case 217:
                case 218: return random.SynchronizedNext(0x00420B3Fu, 4) + 174;
                default: return 0;
            }
        }
    }
}
