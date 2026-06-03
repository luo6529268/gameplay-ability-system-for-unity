using NTSD.Extensions;
using NTSD.Simulation;
using NTSD.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    public partial class LF2Character
    {
        /// <summary>
        /// 攻击状态处理器 (State 3)
        /// 对应 FLF character.js:489-549
        /// 处理所有攻击动作 (普通、跳跃、冲刺攻击) 的通用逻辑
        /// </summary>
        private bool State_Attack(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    // 空中攻击保持逻辑: 如果攻击结束时还在空中，强制切回跳跃状态
                    var D = Frame.D;
                    if (D.next == LF2StandardFrames.LoopToStart && PS.vy < 0)
                    {
                        Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 3, "Attack", LF2StandardFrames.JumpingAir, "空中攻击结束 -> 返回跳跃");
                        Trans.SetNext(LF2StandardFrames.JumpingAir);
                    }
                    return false;

                case "frame_force":
                    return false;

                case "hit_stop":
                    // 通用命中停顿 (卡肉) 效果
                    if (CurrentFrameId == 86 || CurrentFrameId == 87 || CurrentFrameId == 91)
                    {
                        Trans.IncWait(1, 10);
                        return true;
                    }
                    return false;

                case "TU":
                    // FLF character.js:516-548
                    // if (frame.D.itr && (kind==10||11) && match.time.t % 2 === 0)
                    //   椭圆范围 x²+4z²<150² 内所有目标执行 hit(frame[251].itr[0])
                    //   target.ps.y<0 || type=='character' || random()<0.15 才攻击
                    int tickTU = SimulationTickDriver.Instance != null
                        ? SimulationTickDriver.Instance.CurrentTickIndex
                        : 0;
                    var frameDataTU = Frame.D;
                    if (frameDataTU.itrs != null)
                    {
                        foreach (var itr in frameDataTU.itrs)
                        {
                            if ((itr.kind == 10 || itr.kind == 11) && tickTU % 2 == 0)
                            {
                                var sceneQueryTU = Match?.SceneQuery;
                                if (sceneQueryTU == null) break;

                                var frame251 = FrameCache.GetFrameDataById(LF2StandardFrames.FluteAttackDamage);
                                if (frame251?.itrs == null || frame251.itrs.Count == 0) break;

                                var itr251 = frame251.itrs[0];
                                float spriteWTU = GetSpriteWidthPxForCollision();
                                var vol251 = PS.GetItrVolume(itr251, frame251.centerx, frame251.centery, spriteWTU);

                                List<LF2LivingObject> allObjects = new List<LF2LivingObject>();
                                Match.GetAllLivingObjects(allObjects);

                                for (int i = 0; i < allObjects.Count; i++)
                                {
                                    var target = allObjects[i];
                                    if (target == this) continue;
                                    if (target.PS == null) continue;

                                    float zDiff = Mathf.Abs(target.PS.z - PS.z);
                                    float xDiff = Mathf.Abs(target.PS.x - PS.x);
                                    if (xDiff * xDiff + 4 * zDiff * zDiff < 150 * 150)
                                    {
                                        // FLF character.js:556 - $.match.random() < 0.15 随机攻击地面对象
                                        bool randHit = Match.Rng.Next() < 0.15f;
                                        if (target.PS.y < 0 ||
                                            target.Type == LF2ObjectType.Character ||
                                            (target.PS.y >= 0 && randHit))
                                        {
                                            if (target is LF2Character targetChar && targetChar.GetHeldWeapon() != null)
                                                targetChar.DropWeapon(0, 0);

                                            if (target.Hit(itr251, this, new Vector3(PS.x, PS.y, PS.z), vol251))
                                            {
                                                target.Attacked(itr251, this);
                                                target.ItrArestUpdate(itr251);
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    return false;

                default:
                    return false;
            }
        }
    }
}
