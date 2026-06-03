using NTSD.App;
using UnityEngine.Pool;

namespace NTSD.Animation.LF2Objects
{
    public partial class LF2Character : LF2LivingObject
    {
        private void ApplyFrozenBurningParticles()
        {
            var fD = Frame?.D;
            if (fD == null) return;
            var prevFD = FrameCache.GetFrameDataById(Frame.PN);
            int curState = fD.state;
            int prevState = prevFD?.state ?? curState;

            bool entersFrozenOrCut = (curState == LF2States.Frozen || Frame.N == 200)
                                   && !(prevState == LF2States.Frozen || Frame.PN == 200);
            if (entersFrozenOrCut)
            {
                PlaySound("15");
                SpawnOid999Particles(15, new int[] { 120, 125, 130, 135 });
            }

            bool isBurning = (curState == LF2States.Burning || curState == LF2States.FirenSpecific);
            bool wasBurning = (prevState == LF2States.Burning || prevState == LF2States.FirenSpecific);
            if (isBurning)
            {
                int count = (!wasBurning) ? 7 : (UnityEngine.Random.Range(0, 4) == 0 ? 1 : 0);
                if (count > 0)
                    SpawnOid999Particles(count, new int[] { 140 });
            }
        }

        private void PlaySound(string soundId)
        {
            if (string.IsNullOrEmpty(soundId)) return;
            AppManager.Instance?.SoundPlayer?.PlaySfx(soundId);
        }

        private void SpawnOid999Particles(int count, int[] framePicks)
        {
            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;
            for (int i = 0; i < count; i++)
            {
                int frame = framePicks[i < framePicks.Length ? i : (i % framePicks.Length)];
                var op = new ObjectPoint
                {
                    oid = 999, kind = 0, action = frame,
                    dvx = UnityEngine.Random.Range(0, 11) - 5,
                    dvy = -(UnityEngine.Random.Range(0, 20) + 8),
                    dvz = UnityEngine.Random.Range(0, 11) - 5,
                    x = UnityEngine.Random.Range(0, 29) - 14,
                    y = -(UnityEngine.Random.Range(0, 39) - 19),
                    facing = UnityEngine.Random.Range(0, 2)
                };
                factory.EnqueueCreateObject(new LF2Tasks.OPointCreateTask
                {
                    opoint = op, parent = null, team = Team,
                    pos = new UnityEngine.Vector3(PS.x + op.x, PS.y + op.y, PS.z),
                    z = PS.z, dir = PS.dir, dvz = op.dvz
                });
            }
        }

        private void SpawnFragments9996Character()
        {
            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;
            for (int i = 0; i < 5; i++)
            {
                int oid = (i < 4) ? 217 : 218;
                float vy = -(UnityEngine.Random.Range(0, 15) / 2f + 5f);
                float vx, vz;
                int rnd2 = UnityEngine.Random.Range(0, 2);
                int rnd3 = UnityEngine.Random.Range(0, 3);
                if (i < 2)       { vx = (i == 0 ? 1 : -1) * (rnd3 + 10f); vz = (i == 0 ? 1 : -1) * (rnd2 + 3f); }
                else if (i < 4)  { vx = UnityEngine.Random.Range(0, 7) - 3f; vz = (i % 2 == 0 ? 1 : -1) * (rnd2 + 3f); }
                else             { vx = UnityEngine.Random.Range(0, 7) - 3f; vz = (UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1) * (rnd3 + 10f); }
                int frame = UnityEngine.Random.Range(0, 4);
                string dir = UnityEngine.Random.Range(0, 2) == 0 ? "left" : "right";
                var syntheticOpoint = new ObjectPoint
                {
                    oid = oid, kind = 0, action = frame,
                    dvx = (int)vx, dvy = (int)vy, dvz = (int)vz,
                    x = UnityEngine.Random.Range(0, 7) - 3,
                    y = UnityEngine.Random.Range(0, 7) - 9,
                    facing = (dir == "right") ? 0 : 1
                };
                factory.EnqueueCreateObject(new LF2Tasks.OPointCreateTask
                {
                    opoint = syntheticOpoint, parent = null, team = Team,
                    pos = new UnityEngine.Vector3(PS.x + syntheticOpoint.x, PS.y + syntheticOpoint.y, PS.z + 1),
                    z = PS.z + 1, dir = dir, dvz = vz
                });
            }
            HitStun = 6;
        }

        private void ApplyMergeLogic()
        {
            if (FrameCache?.Wrapper == null) return;
            int oid = FrameCache.Wrapper.characterId;

            if ((oid == 7 || oid == 8) && Health.HP > 0 && MergeTimer == 0
                && Frame.D?.state == 2 && Health.HP < 177)
            {
                int partnerOid = 15 - oid;
                var allObjs = ListPool<LF2LivingObject>.Get();
                Match?.GetAllLivingObjects(allObjs);

                LF2Character partner = null;
                int candidateCount = 0;
                for (int i = 0; i < allObjs.Count && candidateCount < 10; i++)
                {
                    if (!(allObjs[i] is LF2Character ch)) continue;
                    if (ch == this) continue;
                    if (ch.FrameCache?.Wrapper?.characterId != partnerOid) continue;
                    if (ch.Team != Team) continue;
                    if (ch.Health.HP <= 0) continue;
                    if (ch.MergeTimer != 0) continue;
                    int pState = ch.Frame.D?.state ?? -1;
                    bool stateOk = pState == 2 || (pState != 0x0E && ch.MergeTimer == 0 && candidateCount > 9);
                    if (!stateOk) continue;
                    float dx = PS.x - ch.PS.x;
                    float dz = PS.z - ch.PS.z;
                    if (System.Math.Abs(dx) >= 50f || System.Math.Abs(dz) >= 8f) continue;
                    if (PS.x <= ch.PS.x && candidateCount <= 9) continue;
                    candidateCount++;
                    partner = ch;
                }
                ListPool<LF2LivingObject>.Release(allObjs);

                if (partner == null) return;

                var wrapper51 = CharacterAnimtorManager.Instance?.GetCharacterConfig(0x33);
                if (wrapper51 == null) return;

                int newHp = System.Math.Min(Health.HP + partner.Health.HP, Health.HPBound);
                int newPp = System.Math.Min(Health.PP + partner.Health.PP, Health.PPBound);

                MergeSelfObjectId = oid;
                MergePartnerObjectId = partnerOid;
                MergePartnerSlotIndex = partner.StableId;

                PS.x = (PS.x + partner.PS.x) * 0.5f;
                PS.z = (PS.z + partner.PS.z) * 0.5f;
                partner.PS.x = PS.x;
                partner.PS.z = PS.z;

                FrameCache.Load(wrapper51);
                Frame.N = 0x7A;
                Frame.D = FrameCache.GetFrameDataById(0x7A);
                MergeFlag = 1;
                PS.vx = 0; PS.vy = 0; PS.vz = 0;
                Health.HP = newHp;
                Health.PP = newPp;
                ShotCount = 500;
                MergeTimer = 4500;

                partner.MergeFlag = -1;
                partner.MergeTimer = 4500;
                partner.Health.HP = 0;
                return;
            }

            if (oid == 0x33 && MergeFlag == 1 && MergeTimer <= 0
                && (Frame.N < 9 || Frame.N > 0x104))
            {
                var wrapperSelf = CharacterAnimtorManager.Instance?.GetCharacterConfig(MergeSelfObjectId);
                if (wrapperSelf == null) return;

                int halfHp = Health.HP / 2;
                int halfPp = Health.PP / 2;

                FrameCache.Load(wrapperSelf);
                MergeFlag = -1;
                MergeTimer = 900;
                Health.HP = halfHp;
                Health.PP = halfPp;
                PS.vx = 0; PS.vy = 0; PS.vz = 0;

                var allObjs = ListPool<LF2LivingObject>.Get();
                Match?.GetAllLivingObjects(allObjs);
                for (int i = 0; i < allObjs.Count; i++)
                {
                    if (!(allObjs[i] is LF2Character ch)) continue;
                    if (ch.StableId != MergePartnerSlotIndex) continue;
                    var wrapperPartner = CharacterAnimtorManager.Instance?.GetCharacterConfig(MergePartnerObjectId);
                    if (wrapperPartner == null) break;
                    ch.FrameCache.Load(wrapperPartner);
                    ch.MergeFlag = -1;
                    ch.MergeTimer = 900;
                    ch.Health.HP = halfHp;
                    ch.Health.PP = halfPp;
                    ch.PS.x = PS.x;
                    ch.PS.z = PS.z;
                    ch.Team = Team;
                    ch.Frame.N = 0x70;
                    ch.Frame.D = ch.FrameCache.GetFrameDataById(0x70);
                    ch.PS.dir = PS.dir == "right" ? "left" : "right";
                    ch.PS.vx = 4.066f;
                    ch.PS.vy = -3.5625f;
                    ch.PS.vz = 3.671875f;
                    break;
                }
                ListPool<LF2LivingObject>.Release(allObjs);
            }
        }

        private void ApplyDeathRespawn()
        {
            if (Frame.D?.state != 0x0E) return;
            if (Health.HP > 0) return;
            if (OwnerEntityIndex >= 0 && Team != 5) return;
            if (StableId < 0x14 && OwnerEntityIndex >= 0 && Team != 5) return;
            if (ShakeTimer <= 0 || ShakeTimer >= 5) return;

            if (RespawnCount > 0)
            {
                RespawnCountdown = ShotCount;
                Health.HP = RespawnCount;
                RespawnCount = 0;
                ShotCount = 0;

                Team = 1;
                Frame.N = 0xDB;
                Frame.D = FrameCache.GetFrameDataById(0xDB);
                HitStun = 0;
                FrameDelay = 10;

                var factory = LF2ObjectPointFactory.Instance;
                if (factory != null)
                {
                    var op = new ObjectPoint { oid = 0x3E6, kind = 0, action = 6, dvx = 0, dvy = 0, dvz = 0, x = 0, y = 0, facing = 0 };
                    factory.EnqueueCreateObject(new LF2Tasks.OPointCreateTask
                    {
                        opoint = op, parent = null, team = Team,
                        pos = new UnityEngine.Vector3(PS.x, PS.y, PS.z),
                        z = PS.z, dir = PS.dir, dvz = 0
                    });
                }

                PropagateRespawnFrameToAllies();
            }
            else
            {
                if (RespawnCountdown >= 2)
                {
                    RespawnCountdown--;
                }
                else
                {
                    Health.HP = 0;
                    var allObjs = ListPool<LF2LivingObject>.Get();
                    Match?.GetAllLivingObjects(allObjs);
                    float sumX = 0, sumZ = 0;
                    int count = 0;
                    for (int i = 0; i < allObjs.Count; i++)
                    {
                        if (!(allObjs[i] is LF2Character ch)) continue;
                        if (ch == this) continue;
                        if (ch.Team != Team) continue;
                        if (ch.Health.HP <= 0) continue;
                        sumX += ch.PS.x;
                        sumZ += ch.PS.z;
                        count++;
                    }
                    ListPool<LF2LivingObject>.Release(allObjs);
                    if (count > 0)
                    {
                        PS.x = sumX / count + 31f;
                        PS.z = sumZ / count + 31f;
                        ShotCount = 500;
                        Health.HP = RespawnCountdown;
                        Frame.N = 0xD4;
                        Frame.D = FrameCache.GetFrameDataById(0xD4);
                        PS.vx = (int)PS.vx;
                        PS.vy = 0;
                        PS.vz = 0;
                    }
                }
            }
        }

        private void ApplyInputSequenceRespawn()
        {
            if (StableId >= 0x32) return;
            if (Health.HP <= 0) return;

            int[] inputSeq = Runtime.InputHistory;
            if (inputSeq[1] != 9) return;
            int spawnFrame = -1;
            if (inputSeq[2] == 0 && inputSeq[3] == 9 && inputSeq[4] == 0) spawnFrame = 100;
            else if (inputSeq[2] == 9 && inputSeq[3] == 9 && inputSeq[4] == 9) spawnFrame = 102;
            else if (inputSeq[2] == 5 && inputSeq[3] == 9 && inputSeq[4] == 5) spawnFrame = 104;
            if (spawnFrame < 0) return;

            System.Array.Clear(inputSeq, 0, inputSeq.Length);

            int spawnAction = spawnFrame - 100;
            var factory = LF2ObjectPointFactory.Instance;
            if (factory != null)
            {
                var op = new ObjectPoint { oid = 0x3E6, kind = 0, action = spawnAction, dvx = 0, dvy = 0, dvz = 0, x = 0, y = 0, facing = 0 };
                factory.EnqueueCreateObject(new LF2Tasks.OPointCreateTask
                {
                    opoint = op, parent = null, team = Team,
                    pos = new UnityEngine.Vector3(PS.x, PS.y, PS.z),
                    z = PS.z, dir = PS.dir, dvz = 0
                });
            }

            PropagateRespawnFrameToAllies();
        }

        private void PropagateRespawnFrameToAllies()
        {
            var allObjs = ListPool<LF2LivingObject>.Get();
            Match?.GetAllLivingObjects(allObjs);
            for (int i = 0; i < allObjs.Count; i++)
            {
                if (!(allObjs[i] is LF2Character ch)) continue;
                if (ch == this) continue;
                if (ch.Team != Team) continue;
                if (ch.Health.HP <= 0) continue;
                ch.FrameCache.Load(FrameCache.Wrapper);
                int chVy = (int)ch.PS.vy;
                ch.Frame.N = chVy != 0 ? 0xD4 : 0;
                ch.Frame.D = ch.FrameCache.GetFrameDataById(ch.Frame.N);
            }
            ListPool<LF2LivingObject>.Release(allObjs);
        }

        internal void RecordInputKey(int ntsdCode)
        {
            int[] inputSeq = Runtime.InputHistory;
            inputSeq[0] = inputSeq[1];
            inputSeq[1] = inputSeq[2];
            inputSeq[2] = inputSeq[3];
            inputSeq[3] = inputSeq[4];
            inputSeq[4] = ntsdCode;
        }
    }
}
