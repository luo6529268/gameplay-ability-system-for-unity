namespace NTSD.Animation
{
    /// <summary>
    /// NTSD 战斗 DAT 常用帧 ID。
    /// 这些常量只描述数据约定，具体帧切换仍以 C++ release 战斗逻辑和当前 DAT 为准。
    /// </summary>
    public static class LF2StandardFrames
    {
        // ==================== 基础移动 (0-19) ====================

        /// <summary>站立第一帧 (100.0%)</summary>
        public const int Standing = 0;

        /// <summary>站立变体1 (96.2%)</summary>
        public const int Standing1 = 1;

        /// <summary>站立变体2 (94.9%)</summary>
        public const int Standing2 = 2;

        /// <summary>站立变体3 (93.7%)</summary>
        public const int Standing3 = 3;

        /// <summary>行走起始帧 (88.6%)</summary>
        public const int WalkingStart = 5;

        /// <summary>行走帧1 (87.3%)</summary>
        public const int Walking1 = 6;

        /// <summary>行走帧2 (83.5%)</summary>
        public const int Walking2 = 7;

        /// <summary>行走结束帧 (82.3%)</summary>
        public const int WalkingEnd = 8;

        /// <summary>奔跑起始帧 (79.7%)</summary>
        public const int RunningStart = 9;

        /// <summary>奔跑帧1 (97.5%)</summary>
        public const int Running1 = 10;

        /// <summary>奔跑结束帧 (91.1%)</summary>
        public const int RunningEnd = 11;

        /// <summary>持重武器行走0 (83.5%)</summary>
        public const int HeavyObjWalk0 = 12;

        /// <summary>持重武器行走1 (83.5%)</summary>
        public const int HeavyObjWalk1 = 13;

        /// <summary>持重武器行走2 (78.5%)</summary>
        public const int HeavyObjWalk2 = 14;

        /// <summary>持重武器行走3 (78.5%)</summary>
        public const int HeavyObjWalk3 = 15;

        /// <summary>持重武器奔跑 (77.2%)</summary>
        public const int HeavyObjRun = 16;

        /// <summary>树跳跃0 (75.9%) - NTSD 特有</summary>
        public const int TreeJump0 = 17;

        /// <summary>树跳跃1 (75.9%) - NTSD 特有</summary>
        public const int TreeJump1 = 18;

        /// <summary>树跳跃2 (75.9%) - NTSD 特有</summary>
        public const int TreeJump2 = 19;

        // ==================== 武器攻击 (20-54) ====================

        /// <summary>站立武器攻击 (79.7%)</summary>
        public const int NormalWeaponAtck = 20;

        /// <summary>站立武器攻击2 (62.0%)</summary>
        public const int NormalWeaponAtck2 = 25;

        /// <summary>跳跃武器攻击 (77.2%)</summary>
        public const int JumpWeaponAtck = 30;

        /// <summary>奔跑武器攻击 (63.3%)</summary>
        public const int RunWeaponAtck = 35;

        /// <summary>冲刺武器攻击 (89.9%)</summary>
        public const int DashWeaponAtck = 40;

        /// <summary>轻武器投掷 (67.1%)</summary>
        public const int LightWeaponThw = 45;

        /// <summary>轻武器投掷2 (63.3%)</summary>
        public const int LightWeaponThw2 = 46;

        /// <summary>轻武器投掷3 (62.0%)</summary>
        public const int LightWeaponThw3 = 47;

        /// <summary>重武器投掷 (74.7%)</summary>
        public const int HeavyWeaponThw = 50;

        /// <summary>重武器投掷2 (73.4%)</summary>
        public const int HeavyWeaponThw2 = 51;

        /// <summary>空中轻武器投掷 (69.6%)</summary>
        public const int SkyLgtWpThw = 52;

        /// <summary>空中轻武器投掷2 (65.8%)</summary>
        public const int SkyLgtWpThw2 = 53;

        /// <summary>空中轻武器投掷3 (64.6%)</summary>
        public const int SkyLgtWpThw3 = 54;

        // ==================== 拳击/攻击 (60-95) ====================

        /// <summary>普通拳击 (88.6%)</summary>
        public const int Punch = 60;

        /// <summary>拳击变体1 (73.4%)</summary>
        public const int Punch1 = 61;

        /// <summary>拳击变体2 (69.6%)</summary>
        public const int Punch2 = 62;

        /// <summary>拳击变体3 (60.8%)</summary>
        public const int Punch3 = 63;

        /// <summary>拳击变体4 (68.4%)</summary>
        public const int Punch4 = 65;

        /// <summary>拳击变体5 (65.8%)</summary>
        public const int Punch5 = 66;

        /// <summary>拳击变体6 (62.0%)</summary>
        public const int Punch6 = 67;

        /// <summary>重拳 (88.6%)</summary>
        public const int SuperPunch = 70;

        /// <summary>重拳变体1 (62.0%)</summary>
        public const int SuperPunch1 = 71;

        /// <summary>重拳变体2 (59.5%)</summary>
        public const int SuperPunch2 = 72;

        /// <summary>跳跃攻击 (65.8%)</summary>
        public const int JumpAttack = 80;

        /// <summary>奔跑攻击 (65.8%)</summary>
        public const int RunAttack = 85;

        /// <summary>冲刺攻击 (64.6%)</summary>
        public const int DashAttack = 90;

        /// <summary>冲刺防御 (63.3%)</summary>
        public const int DashDefend = 95;

        // ==================== 防御移动 (Rowing) (100-109) ====================

        /// <summary>前防御移动0 (69.6%)</summary>
        public const int Rowing = 100;

        /// <summary>前防御移动1 (83.5%)</summary>
        public const int Rowing1 = 101;

        /// <summary>奔跑防御 (83.5%)</summary>
        public const int Rowing2 = 102;

        /// <summary>前防御移动3 (81.0%)</summary>
        public const int Rowing3 = 103;

        /// <summary>前防御移动4 (75.9%)</summary>
        public const int Rowing4 = 104;

        /// <summary>前防御移动5 (75.9%)</summary>
        public const int Rowing5 = 105;

        /// <summary>后防御移动0 (78.5%)</summary>
        public const int RowingBack = 108;

        /// <summary>后防御移动1 (79.7%)</summary>
        public const int RowingBack1 = 109;

        // ==================== 防御 (110-114) ====================

        /// <summary>站立防御 (82.3%)</summary>
        public const int Defend = 110;

        /// <summary>站立防御2 (83.5%)</summary>
        public const int Defend1 = 111;

        /// <summary>防御被破 (82.3%)</summary>
        public const int DefendBroken = 112;

        /// <summary>防御被破2 (81.0%)</summary>
        public const int DefendBroken1 = 113;

        /// <summary>防御被破3 (79.7%)</summary>
        public const int DefendBroken2 = 114;

        // ==================== 拾取/抓取 (115-120) ====================

        /// <summary>拾取轻武器 (82.3%)</summary>
        public const int PickingLight = 115;

        /// <summary>拾取重武器 (82.3%)</summary>
        public const int PickingHeavy = 116;

        /// <summary>拾取重武器2 (81.0%)</summary>
        public const int PickingHeavy2 = 117;

        /// <summary>抓取 (60.8%)</summary>
        public const int Catching = 120;

        /// <summary>抓取攻击1。</summary>
        public const int CatchingAttack1 = 121;

        /// <summary>抓取攻击2。</summary>
        public const int CatchingAttack2 = 122;

        /// <summary>抓取成功。</summary>
        public const int CatchingSuccess = 123;

        // ==================== 被抓状态 (130-144) ====================

        /// <summary>被抓住悬空 (84.8%)</summary>
        public const int PickedCaught = 130;

        /// <summary>被抓1 (83.5%)</summary>
        public const int PickedCaught1 = 131;

        /// <summary>被抓2 (81.0%)</summary>
        public const int PickedCaught2 = 132;

        /// <summary>被抓3 (79.7%)</summary>
        public const int PickedCaught3 = 133;

        /// <summary>被抓4 (81.0%)</summary>
        public const int PickedCaught4 = 134;

        /// <summary>被抓5 (83.5%)</summary>
        public const int PickedCaught5 = 135;

        /// <summary>被抓6 (83.5%)</summary>
        public const int PickedCaught6 = 136;

        /// <summary>被抓7 (81.0%)</summary>
        public const int PickedCaught7 = 137;

        /// <summary>被抓8 (81.0%)</summary>
        public const int PickedCaught8 = 138;

        /// <summary>被抓9 (81.0%)</summary>
        public const int PickedCaught9 = 139;

        /// <summary>被抓10 (82.3%)</summary>
        public const int PickedCaught10 = 140;

        /// <summary>被抓11 (81.0%)</summary>
        public const int PickedCaught11 = 141;

        /// <summary>被抓12 (79.7%)</summary>
        public const int PickedCaught12 = 142;

        /// <summary>被抓13 (79.7%)</summary>
        public const int PickedCaught13 = 143;

        /// <summary>被抓14 (77.2%)</summary>
        public const int PickedCaught14 = 144;

        // ==================== 倒地 (180-191) ====================

        /// <summary>正面倒地起始 (60.8%)</summary>
        public const int FallingFront = 180;

        /// <summary>正面倒地上升 (60.8%)</summary>
        public const int FallingFront1 = 181;

        /// <summary>正面倒地空中 (60.8%)</summary>
        public const int FallingFront2 = 182;

        /// <summary>正面倒地下落 (60.8%)</summary>
        public const int FallingFront3 = 183;

        /// <summary>正面倒地4 (60.8%)</summary>
        public const int FallingFront4 = 184;

        /// <summary>正面倒地弹起 (59.5%)</summary>
        public const int FallingFront5 = 185;

        /// <summary>背面倒地起始 (59.5%)</summary>
        public const int FallingBack = 186;

        /// <summary>背面倒地上升 (59.5%)</summary>
        public const int FallingBack1 = 187;

        /// <summary>背面倒地空中 (59.5%)</summary>
        public const int FallingBack2 = 188;

        /// <summary>背面倒地下落 (59.5%)</summary>
        public const int FallingBack3 = 189;

        /// <summary>背面倒地4 (59.5%)</summary>
        public const int FallingBack4 = 190;

        /// <summary>背面倒地弹起 (59.5%)</summary>
        public const int FallingBack5 = 191;

        // ==================== 特殊效果 (200-206) ====================

        /// <summary>MP消耗 (59.5%)</summary>
        public const int MpDrain = 200;

        /// <summary>燃烧0 (59.5%)</summary>
        public const int Fire = 203;

        /// <summary>燃烧1 (59.5%)</summary>
        public const int Fire1 = 204;

        /// <summary>燃烧2 (59.5%)</summary>
        public const int Fire2 = 205;

        /// <summary>燃烧3 (59.5%)</summary>
        public const int Fire3 = 206;

        // ==================== 跳跃/冲刺 (210-219) ====================

        /// <summary>跳跃起始 (59.5%)</summary>
        public const int Jumping = 210;

        /// <summary>跳跃上升 (59.5%)</summary>
        public const int JumpingUp = 211;

        /// <summary>空中状态 (59.5%)</summary>
        public const int JumpingAir = 212;

        /// <summary>前冲刺 (59.5%)</summary>
        public const int DashForward = 213;

        /// <summary>前冲刺2 (59.5%)</summary>
        public const int DashForward2 = 214;

        /// <summary>下蹲着陆 (59.5%)</summary>
        public const int Crouch = 215;

        /// <summary>后冲刺 (59.5%)</summary>
        public const int DashBack = 216;

        /// <summary>后冲刺2 (59.5%)</summary>
        public const int DashBack2 = 217;

        /// <summary>停止奔跑 (59.5%)</summary>
        public const int StopRunning = 218;

        /// <summary>下蹲2 (59.5%)</summary>
        public const int Crouch2 = 219;

        // ==================== 受伤/躺地 (220-231) ====================

        /// <summary>受伤0 (59.5%)</summary>
        public const int Injured = 220;

        /// <summary>受伤1 (59.5%)</summary>
        public const int Injured1 = 221;

        /// <summary>受伤2 (59.5%)</summary>
        public const int Injured2 = 222;

        /// <summary>受伤3 (59.5%)</summary>
        public const int Injured3 = 223;

        /// <summary>受伤4 (59.5%)</summary>
        public const int Injured4 = 224;

        /// <summary>受伤5 (59.5%)</summary>
        public const int Injured5 = 225;

        /// <summary>受伤6 (59.5%)</summary>
        public const int Injured6 = 226;

        /// <summary>受伤7 (59.5%)</summary>
        public const int Injured7 = 227;

        /// <summary>受伤8 (59.5%)</summary>
        public const int Injured8 = 228;

        /// <summary>受伤9 (59.5%)</summary>
        public const int Injured9 = 229;

        /// <summary>正面躺地 (59.5%)</summary>
        public const int Lying = 230;

        /// <summary>背面躺地 (59.5%)</summary>
        public const int LyingBack = 231;

        /// <summary>投掷躺地者0。</summary>
        public const int ThrowLyingDown = 233;

        /// <summary>投掷躺地者1。</summary>
        public const int ThrowLyingDown1 = 234;

        /// <summary>变身帧。</summary>
        public const int RudolfTransform = 240;

        /// <summary>笛子攻击伤害帧。</summary>
        public const int FluteAttackDamage = 251;

        /// <summary>飞行撞击帧。</summary>
        public const int FlyingCrash = 253;

        /// <summary>消失帧。</summary>
        public const int Disappear = 257;

        // ==================== NTSD 特有帧 (395-399) ====================

        /// <summary>NTSD 抓取2 (68.4%) - NTSD 特有</summary>
        public const int Catching2 = 395;

        /// <summary>NTSD 被抓2 (68.4%) - NTSD 特有</summary>
        public const int PickedCaught2_Alt = 396;

        /// <summary>NTSD 受伤变体 (70.9%) - NTSD 特有</summary>
        public const int Injured_Alt = 397;

        /// <summary>NTSD 受伤变体2 (68.4%) - NTSD 特有</summary>
        public const int Injured_Alt2 = 398;

        /// <summary>NTSD 虚拟帧 (78.5%) - NTSD 特有</summary>
        public const int Dummy = 399;

        // ==================== 特殊值 ====================

        /// <summary>next 哨兵值：停止当前帧推进。</summary>
        public const int StopSpeed = 550;

        /// <summary>next 哨兵值：返回站立帧。</summary>
        public const int LoopToStart = 999;

        // ==================== 辅助方法 ====================

        /// <summary>判断是否为站立帧 (0-3)</summary>
        public static bool IsStanding(int frameId)
        {
            return frameId >= 0 && frameId <= 3;
        }

        /// <summary>判断是否为行走帧 (5-8)</summary>
        public static bool IsWalking(int frameId)
        {
            return frameId >= WalkingStart && frameId <= WalkingEnd;
        }

        /// <summary>判断是否为奔跑帧 (9-11)</summary>
        public static bool IsRunning(int frameId)
        {
            return frameId >= RunningStart && frameId <= RunningEnd;
        }

        /// <summary>判断是否为持重武器移动帧 (12-16)</summary>
        public static bool IsHeavyObjMovement(int frameId)
        {
            return frameId >= 12 && frameId <= 16;
        }

        /// <summary>判断是否为树跳跃帧 (17-19) - NTSD 特有</summary>
        public static bool IsTreeJump(int frameId)
        {
            return frameId >= 17 && frameId <= 19;
        }

        /// <summary>判断是否为武器攻击帧 (20, 25, 30, 35, 40, 45-54)</summary>
        public static bool IsWeaponAttack(int frameId)
        {
            return (frameId == 20 || frameId == 25 || frameId == 30 ||
                    frameId == 35 || frameId == 40 ||
                    (frameId >= 45 && frameId <= 47) ||
                    (frameId >= 50 && frameId <= 54));
        }

        /// <summary>判断是否为拳击帧 (60-63, 65-67, 70-72)</summary>
        public static bool IsPunch(int frameId)
        {
            return frameId == 60 || (frameId >= 61 && frameId <= 63) ||
                   (frameId >= 65 && frameId <= 67) ||
                   (frameId >= 70 && frameId <= 72);
        }

        /// <summary>判断是否为跳跃/冲刺攻击帧 (80, 85, 90, 95)</summary>
        public static bool IsJumpDashAttack(int frameId)
        {
            return frameId == 80 || frameId == 85 || frameId == 90 || frameId == 95;
        }

        /// <summary>判断是否为防御移动帧 (100-105, 108-109)</summary>
        public static bool IsRowing(int frameId)
        {
            return (frameId >= 100 && frameId <= 105) ||
                   (frameId >= 108 && frameId <= 109);
        }

        /// <summary>判断是否为防御帧 (110-114)</summary>
        public static bool IsDefending(int frameId)
        {
            return (frameId >= 110 && frameId <= 111) ||
                   (frameId >= 112 && frameId <= 114);
        }

        /// <summary>判断是否为拾取/抓取帧 (115-120)</summary>
        public static bool IsPicking(int frameId)
        {
            return (frameId >= 115 && frameId <= 117) || frameId == 120;
        }

        /// <summary>判断是否为被抓状态帧 (130-144)</summary>
        public static bool IsBeingCaught(int frameId)
        {
            return frameId >= 130 && frameId <= 144;
        }

        /// <summary>判断是否为倒地帧 (180-191)</summary>
        public static bool IsFalling(int frameId)
        {
            return frameId >= 180 && frameId <= 191;
        }

        /// <summary>判断是否为特殊效果帧 (200, 203-206)</summary>
        public static bool IsSpecialEffect(int frameId)
        {
            return frameId == 200 || (frameId >= 203 && frameId <= 206);
        }

        /// <summary>判断是否为跳跃/冲刺帧 (210-219)</summary>
        public static bool IsJumpingOrDashing(int frameId)
        {
            return frameId >= 210 && frameId <= 219;
        }

        /// <summary>判断是否为受伤帧 (220-229, 397-398)</summary>
        public static bool IsInjured(int frameId)
        {
            return (frameId >= 220 && frameId <= 229) ||
                   frameId == 397 || frameId == 398; // 包含 NTSD 变体
        }

        /// <summary>判断是否为躺地帧 (230-231)</summary>
        public static bool IsLying(int frameId)
        {
            return frameId == 230 || frameId == 231;
        }

        /// <summary>判断是否为 NTSD 特有帧 (17-19, 395-399)</summary>
        public static bool IsNTSDSpecific(int frameId)
        {
            return (frameId >= 17 && frameId <= 19) ||  // tree_jump
                   (frameId >= 395 && frameId <= 399);   // NTSD 扩展
        }
    }
}
