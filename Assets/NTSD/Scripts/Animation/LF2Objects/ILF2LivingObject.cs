using BeatEmUpTemplate2D;
using MoreMountains.TopDownEngine;
using NTSD.Extensions;
using NTSD.Simulation;
using System.Collections.Generic;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// LF2 活动对象公共接口
    /// 定义所有活动对象（角色、武器、特效）共享的属性和方法
    /// 
    /// 用于统一 LF2LivingObject（纯 C#）和 LF2CharacterAnimator（MonoBehaviour）的访问
    /// 
    /// 对应 FLF livingobject.js 的公共 API
    /// </summary>
    public interface ILF2LivingObject
    {
        // ========== 身份字段 ==========
        string Name { get; set; }
        int StableId { get; }
        int ObjectId { get; }
        int Team { get; set; }
        int OwnerId { get; set; }

        // ========== 核心模块 ==========
        PhysicsState PS { get; }
        FrameTransistor Trans { get; }
        LF2ItrRestTracker ItrRest { get; }
        LF2FrameInfo Frame { get; }
        LF2FrameCache FrameCache { get; }
        LF2CharacterDataWrapper _FrameDataWrapper { get; }
        Dictionary<string, object> StateMem { get; }

        // ========== 角色专用（可选，返回 null 表示不支持）==========
        NTSDCharacterStats CharacterStats { get; }
        LF2HitCountersModule HitCounters { get; }
        LF2ComboBufferModule ComboBuffer { get; }
        /// <summary>控制器（对应 FLF $.con）</summary>
        ILF2Controller Controller { get;}

        // ========== 方法 ==========
        void SetDirection(DIRECTION direction);
        void SetDirectionByString(string dir);
        void TransitionToFrame(int frameId, int wait = 0);
        void PlayFrameByID(int frameId);
        void FrameAniOscillate(int from, int to);
        void Transit_DynamicsAndWPoint();
        
        LF2FrameData GetFrameDataById(int frameId);
        int GetFirstFrameByState(int state);
        float GetSpriteWidthPxForCollision();

        bool GetStateMemory<T>(string key, out T value);
        void SetStateMemory<T>(string key, T value);
    }
}
