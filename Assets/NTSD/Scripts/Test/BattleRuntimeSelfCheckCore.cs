using System;
using System.Collections.Generic;

namespace NTSD.Test
{
    /// <summary>
    /// 战斗运行时自检的纯 C# 核心。
    /// 不依赖 UnityEngine，也不实例化场景对象；用于验证 C++ release cpoint 公式和关键状态写入。
    /// </summary>
    public static class BattleRuntimeSelfCheckCore
    {
        public static void RunAllChecks()
        {
            CheckActionSelection();
            CheckThrow();
            CheckBeingCaughtPositionSync();
            CheckDecreaseEscape();
        }

        private static void CheckActionSelection()
        {
            var frames = BuildFrames();
            var attacker = Entity(frames, frame: 100, x: 0, y: 0, z: 0, facing: 0);
            var victim = Entity(frames, frame: 130, x: 0, y: 0, z: 0, facing: 0);
            attacker.Caught = victim;
            victim.Catcher = attacker;

            ProcessCatching(attacker, attack: true, jump: false, left: false, right: false, up: false, down: false);

            Expect(attacker.Frame == 120, "aaction 应写入抓取者帧 120");
            Expect(victim.Frame == 131, "aaction 目标帧 cpoint.vaction 应写入被抓者帧 131");
            Expect(attacker.Attacking == 0 && victim.Attacking == 0, "aaction 后双方 attacking 应清零");
        }

        private static void CheckThrow()
        {
            var frames = BuildFrames();
            var attacker = Entity(frames, frame: 110, x: 100, y: 20, z: 7, facing: 1);
            var victim = Entity(frames, frame: 130, x: 0, y: 0, z: 1, facing: 0);
            attacker.Caught = victim;
            victim.Catcher = attacker;

            ProcessCatching(attacker, attack: false, jump: false, left: false, right: false, up: false, down: true);

            Expect(attacker.Frame == 112, "throwvx 分支应让抓取者进入 next=112");
            Expect(victim.Frame == 132, "throwvx 分支应写入 victim vaction=132");
            Expect(Nearly(victim.Vx, -8), "左向投掷应反转 victim.vx");
            Expect(Nearly(victim.Vy, -4), "投掷应写入 victim.vy");
            Expect(Nearly(victim.Vz, 3), "按下方向投掷应写入正 throwvz");
            Expect(victim.WeaponCount == 25, "throwinjury>0 应写入 victim.WeaponCount");
            Expect(attacker.Caught == null && victim.Catcher == null, "投掷后双方抓取关系应清空");
        }

        private static void CheckBeingCaughtPositionSync()
        {
            var frames = BuildFrames();
            var catcher = Entity(frames, frame: 100, x: 50, y: 12, z: 4, facing: 0);
            var victim = Entity(frames, frame: 130, x: 0, y: 0, z: 0, facing: 0);
            catcher.Caught = victim;
            victim.Catcher = catcher;

            SyncBeingCaught(victim);

            Expect(victim.Frame == 131, "被抓者应按 vaction 进入帧 131");
            Expect(Nearly(victim.X, 56), "被抓者 x 应按 catcher/vaction cpoint 组合计算");
            Expect(Nearly(victim.Y, 20), "被抓者 y 应按垂直坐标计算并应用 cover 修正");
            Expect(Nearly(victim.Z, 3), "被抓者 z 应复制 catcher 深度并应用 cover 修正");
            Expect(victim.Facing == 0, "cover=10 应复制抓取者方向");
        }

        private static void CheckDecreaseEscape()
        {
            var frames = BuildFrames();
            var attacker = Entity(frames, frame: 140, x: 30, y: 0, z: 0, facing: 0);
            var victim = Entity(frames, frame: 130, x: 10, y: 0, z: 0, facing: 0);
            attacker.Caught = victim;
            victim.Catcher = attacker;
            attacker.CaughtDuration = 3;

            ProcessCatching(attacker, attack: false, jump: false, left: false, right: false, up: false, down: false);

            Expect(attacker.Frame == 0, "decrease<0 逃脱后抓取者应回 frame 0");
            Expect(victim.Frame == 181, "decrease<0 逃脱后被抓者应进入 frame 181");
            Expect(attacker.HitCount == 1 && victim.HitCount == 1, "decrease<0 逃脱后双方 HitCount 应为 1");
            Expect(Nearly(victim.KnockbackVx, -4), "抓取者在右侧时被抓者 knockback_vx 应为 -4");
            Expect(Nearly(victim.KnockbackVy, -3), "逃脱后被抓者 knockback_vy 应为 -3");
            Expect(attacker.Caught == null && victim.Catcher == null, "逃脱后双方抓取关系应清空");
        }

        private static void ProcessCatching(EntityState attacker, bool attack, bool jump, bool left, bool right, bool up, bool down)
        {
            var victim = attacker.Caught;
            if (victim == null) return;

            var cp = attacker.Current.CPoint;
            if (cp == null || cp.Kind != 1 || victim.Current.CPoint == null || victim.Current.CPoint.Kind != 2 || victim.Catcher != attacker)
            {
                attacker.Frame = 0;
                attacker.Caught = null;
                return;
            }

            if (cp.Decrease > 0)
            {
                attacker.CaughtDuration -= cp.Decrease;
            }
            else if (cp.Decrease < 0)
            {
                attacker.CaughtDuration += cp.Decrease;
                if (attacker.CaughtDuration < 0)
                {
                    attacker.Frame = 0;
                    victim.Frame = 181;
                    attacker.HitCount = 1;
                    victim.HitCount = 1;
                    victim.KnockbackVx = attacker.X > victim.X ? -4 : 4;
                    victim.KnockbackVy = -3;
                    attacker.Caught = null;
                    victim.Catcher = null;
                    attacker.CaughtDuration = 0;
                    return;
                }
            }

            bool hasDirection = left || right || up || down;
            if (attack && cp.AAction != 0 && ((!left && !right) || cp.TAction == 0))
            {
                ApplyAction(attacker, victim, cp.AAction);
                return;
            }

            if (attack && hasDirection && cp.TAction != 0)
            {
                ApplyAction(attacker, victim, cp.TAction);
                return;
            }

            if (jump && cp.JAction != 0)
            {
                ApplyAction(attacker, victim, cp.JAction);
                return;
            }

            if (cp.ThrowVx != 0)
                ApplyThrow(attacker, victim, cp, up, down);
        }

        private static void ApplyAction(EntityState attacker, EntityState victim, int actionFrame)
        {
            if (actionFrame < 0)
            {
                attacker.Facing = 1 - attacker.Facing;
                actionFrame = -actionFrame;
            }

            attacker.Frame = actionFrame;
            victim.Frame = attacker.Frames[actionFrame].CPoint?.VAction ?? 0;
            attacker.Attacking = 0;
            victim.Attacking = 0;
        }

        private static void ApplyThrow(EntityState attacker, EntityState victim, CPoint cp, bool up, bool down)
        {
            if (cp.ThrowInjury > 0)
                victim.WeaponCount = cp.ThrowInjury;

            var frame = attacker.Current;
            victim.X = attacker.Facing == 0
                ? attacker.X - frame.CenterX + cp.X
                : frame.CenterX - cp.X + attacker.X;
            victim.Y = attacker.Y - frame.CenterY + cp.Y;

            int dir = attacker.Facing == 0 ? 1 : -1;
            victim.Vx = cp.ThrowVx * dir;
            victim.Vy = cp.ThrowVy;
            if (up && !down)
                victim.Vz = -cp.ThrowVz;
            else if (!up && down)
                victim.Vz = cp.ThrowVz;

            victim.Frame = cp.VAction;
            attacker.Frame = frame.Next;
            attacker.Attacking = 0;
            attacker.Caught = null;
            victim.Catcher = null;
        }

        private static void SyncBeingCaught(EntityState victim)
        {
            var catcher = victim.Catcher;
            if (catcher == null) return;

            var cp = catcher.Current.CPoint;
            if (cp == null || cp.Kind != 1 || victim.Current.CPoint == null || victim.Current.CPoint.Kind != 2) return;

            if (cp.VAction != 0)
            {
                int target = cp.VAction;
                if (target < 0)
                {
                    victim.Facing = 1 - victim.Facing;
                    target = -target;
                }
                victim.Frame = target;
            }

            var catcherFrame = catcher.Current;
            var selfFrame = victim.Current;
            int catcherAdir = catcher.Facing == 0 ? 1 : -1;
            double attachX = catcher.X + (cp.X - catcherFrame.CenterX) * catcherAdir;
            double attachY = catcher.Y - catcherFrame.CenterY + cp.Y;

            var vactionCp = victim.Frames[Math.Abs(cp.VAction)].CPoint;
            double selfCpointX = vactionCp?.X ?? 0;
            double selfCpointY = vactionCp?.Y ?? 0;

            victim.X = victim.Facing == 0
                ? selfFrame.CenterX - selfCpointX + attachX
                : selfCpointX - selfFrame.CenterX + attachX;
            victim.Y = selfFrame.CenterY - selfCpointY + attachY;
            victim.Z = catcher.Z;

            if (cp.Cover % 10 != 0)
            {
                victim.Z += 1;
                victim.Y -= 1;
            }
            else
            {
                victim.Z -= 1;
                victim.Y += 1;
            }

            int coverDir = cp.Cover / 10;
            if (coverDir == 1)
                victim.Facing = catcher.Facing;
            else if (coverDir == 2)
                victim.Facing = 1 - catcher.Facing;
        }

        private static Dictionary<int, FrameData> BuildFrames()
        {
            return new Dictionary<int, FrameData>
            {
                [0] = Frame(0, 0, 0, 39, 79),
                [100] = Frame(100, 9, 100, 39, 79, new CPoint { Kind = 1, X = 20, Y = 30, VAction = 131, AAction = 120, Cover = 10 }),
                [110] = Frame(110, 9, 112, 40, 80, new CPoint { Kind = 1, X = 16, Y = 24, VAction = 132, ThrowVx = 8, ThrowVy = -4, ThrowVz = 3, ThrowInjury = 25, Cover = 10 }),
                [112] = Frame(112, 0, 0, 39, 79),
                [120] = Frame(120, 9, 120, 39, 79, new CPoint { Kind = 1, X = 20, Y = 30, VAction = 131, Cover = 10 }),
                [130] = Frame(130, 10, 130, 35, 70, new CPoint { Kind = 2, X = 8, Y = 12 }),
                [131] = Frame(131, 10, 131, 34, 69, new CPoint { Kind = 2, X = 9, Y = 13 }),
                [132] = Frame(132, 10, 132, 33, 68, new CPoint { Kind = 2, X = 6, Y = 10 }),
                [140] = Frame(140, 9, 140, 39, 79, new CPoint { Kind = 1, X = 20, Y = 30, VAction = 131, Decrease = -5, Cover = 10 }),
                [181] = Frame(181, 11, 181, 39, 79),
            };
        }

        private static FrameData Frame(int id, int state, int next, int centerX, int centerY, CPoint cpoint = null)
        {
            return new FrameData { Id = id, State = state, Next = next, CenterX = centerX, CenterY = centerY, CPoint = cpoint };
        }

        private static EntityState Entity(Dictionary<int, FrameData> frames, int frame, double x, double y, double z, int facing)
        {
            return new EntityState { Frames = frames, Frame = frame, X = x, Y = y, Z = z, Facing = facing };
        }

        private static void Expect(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static bool Nearly(double actual, double expected)
        {
            return Math.Abs(actual - expected) <= 0.001;
        }

        private sealed class EntityState
        {
            public Dictionary<int, FrameData> Frames;
            public int Frame;
            public int Facing;
            public int Attacking;
            public int CaughtDuration;
            public int HitCount;
            public int WeaponCount;
            public double X;
            public double Y;
            public double Z;
            public double Vx;
            public double Vy;
            public double Vz;
            public double KnockbackVx;
            public double KnockbackVy;
            public EntityState Caught;
            public EntityState Catcher;
            public FrameData Current => Frames[Frame];
        }

        private sealed class FrameData
        {
            public int Id;
            public int State;
            public int Next;
            public int CenterX;
            public int CenterY;
            public CPoint CPoint;
        }

        private sealed class CPoint
        {
            public int Kind;
            public int X;
            public int Y;
            public int VAction;
            public int AAction;
            public int TAction { get; set; }
            public int JAction { get; set; }
            public int ThrowVx;
            public int ThrowVy;
            public int ThrowVz;
            public int ThrowInjury;
            public int Decrease;
            public int Cover;
        }
    }
}
