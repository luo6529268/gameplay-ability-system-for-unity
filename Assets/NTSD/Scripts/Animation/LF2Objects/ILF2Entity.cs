using NTSD.Animation;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 战斗实体统一契约（对应反汇编 entity 结构体公共部分）
    ///
    /// 反汇编确认：
    ///   Entity_FrameAdvance (0x416240) 对所有 400 个 entity slot 统一调用
    ///   RenderDispatch (0x41D010) 对所有 active entity 统一画阴影和 sprite
    ///   所有 entity（角色/武器/技能）共用同一套物理字段和渲染逻辑
    ///
    /// 职责：
    ///   声明所有参与战斗的实体必须实现的公共成员
    ///   ILF2Object  → 帧驱动框架契约（对象池 + 模拟系统）
    ///   ILF2Entity  → 战斗实体公共契约（物理 + 渲染 + 战斗字段）
    /// </summary>
    public interface ILF2Entity : ILF2Object
    {
        // ── 标识 ──────────────────────────────────────────────────────────

        /// <summary>对象名称</summary>
        string Name { get; set; }

        /// <summary>队伍 ID（entity+2FCh）</summary>
        int Team { get; set; }

        /// <summary>队伍方向（entity+8h，0=右/1=左）</summary>
        int TeamSide { get; set; }

        /// <summary>所有者 entity slot index（entity+2F4h），-1 表示无</summary>
        int OwnerId { get; set; }

        /// <summary>被抓取状态（entity+98h grabbed_by）</summary>
        int GrabbedBy { get; set; }

        /// <summary>kind==2 tracker 标志（parent=1, child=-1）</summary>
        int TrackerFlag { get; set; }

        /// <summary>kind==2 tracker 子对象引用（entity+9Ch）</summary>
        ILF2Entity TrackerChild { get; set; }

        /// <summary>kind==2 tracker 父对象引用（entity+0A0h）</summary>
        ILF2Entity TrackerParent { get; set; }

        // ── 物理状态（Entity_FrameAdvance 共用）───────────────────────────

        /// <summary>物理状态（位置/速度，entity+58h/60h/68h 等）</summary>
        PhysicsState PS { get; }

        // ── 帧系统（Entity_FrameAdvance 共用）────────────────────────────

        /// <summary>当前帧信息（entity+70h frame index）</summary>
        LF2FrameInfo Frame { get; }

        /// <summary>dat 帧数据缓存（entity+368h）</summary>
        LF2FrameCache FrameCache { get; }

        /// <summary>帧转换器</summary>
        FrameTransistor Trans { get; }

        /// <summary>效果状态（TimeIn/Stuck 等）</summary>
        LF2EffectState Effect { get; }

        /// <summary>帧延迟计数器（entity+0B4h）</summary>
        int FrameDelay { get; set; }

        // ── 战斗字段（Entity_FrameAdvance/Entity_Collision 共用）─────────

        /// <summary>命中硬直标志（entity+88h hit_stun）</summary>
        int HitStun { get; set; }

        /// <summary>累积击退 X 速度（entity+28h knockback_vx）</summary>
        float KnockbackVx { get; set; }

        /// <summary>累积击退 Y 速度（entity+30h knockback_vy）</summary>
        float KnockbackVy { get; set; }

        /// <summary>累积击退 Z 速度（entity+38h knockback_vz）</summary>
        float KnockbackVz { get; set; }

        /// <summary>角色类型（entity+20h char_type）</summary>
        int CharType { get; set; }

        /// <summary>震屏计时器（entity+8h shake_timer）</summary>
        int ShakeTimer { get; set; }

        /// <summary>弹射计数（entity+308h）</summary>
        int ShotCount { get; set; }

        // ── 渲染（RenderDispatch 共用）────────────────────────────────────

        /// <summary>Sprite 资源引用</summary>
        LF2Sprite Sprite { get; }

        /// <summary>渲染器引用</summary>
        LF2ObjectRenderer Renderer { get; }

        /// <summary>阴影 SpriteRenderer（对应 RenderDispatch shadow blit）</summary>
        SpriteRenderer ShadowRenderer { get; }

        /// <summary>
        /// 更新阴影位置（对应反汇编 RenderDispatch shadow 公式）
        /// shadow_x = px, shadow_y = pz（只用地面深度，不含 py）
        /// </summary>
        void UpdateShadow();

        // ── Spark 系统（PostRender 共用，所有 entity 共用 slot）──────────

        /// <summary>当前激活的 spark slot 数量</summary>
        int SparkSlotCount { get; }

        /// <summary>读取指定 slot 的 timer 值</summary>
        int GetSparkTimer(int slotIndex);

        /// <summary>读取指定 slot 的世界坐标</summary>
        Vector3 GetSparkWorldPos(int slotIndex);

        /// <summary>每帧递增所有 spark timer</summary>
        void TickAllSparkTimers(int renderFrame);

        // ── 帧数据查询 ────────────────────────────────────────────────────

        /// <summary>按帧 ID 获取帧数据</summary>
        LF2FrameData GetFrameDataById(int frameId);

        /// <summary>跳转到指定帧</summary>
        void TransitionToFrame(int frameId, int wait = 0);

        /// <summary>获取碰撞用 sprite 宽度（像素）</summary>
        float GetSpriteWidthPxForCollision();

        // ── 模拟世界 ─────────────────────────────────────────────────────

        /// <summary>当前模拟世界引用</summary>
        SimulationWorld Match { get; }
    }
}
