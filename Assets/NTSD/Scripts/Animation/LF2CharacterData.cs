using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// 完整的LF2角色配置数据
    /// </summary>
    [System.Serializable]
    public class LF2CharacterData
    {
        [Header("基本信息")]
        public string name = "Naruto";
        public string head = "";
        public string small = "";
        public List<SpriteFileInfo> files = new List<SpriteFileInfo>();

        [Header("行走参数")]
        public int walking_frame_rate = 3;
        public float walking_speed = 4.0f;
        public float walking_speedz = 2.0f;

        [Header("奔跑参数")]
        public int running_frame_rate = 3;
        public float running_speed = 8.0f;
        public float running_speedz = 3.3f;

        [Header("负重行走参数")]
        public float heavy_walking_speed = 3.0f;
        public float heavy_walking_speedz = 1.5f;

        [Header("负重奔跑参数")]
        public float heavy_running_speed = 5.0f;
        public float heavy_running_speedz = 0.8f;

        [Header("跳跃参数")]
        public float jump_height = -16.3f;
        public float jump_distance = 8.0f;
        public float jump_distancez = 3.0f;

        [Header("冲刺参数")]
        public float dash_height = -13.0f;
        public float dash_distance = 15.0f;
        public float dash_distancez = 3.75f;

        [Header("翻滚参数")]
        public float rowing_height = -2.0f;
        public float rowing_distance = 20.0f;

        [Header("帧数据")]
        public List<LF2FrameData> frames = new List<LF2FrameData>();

        [Header("武器参数（仅武器 DAT 有效）")]
        public int weapon_hp = 0;
        public int weapon_drop_hurt = 0;
        public string weapon_hit_sound = "";
        public string weapon_drop_sound = "";
        public string weapon_broken_sound = "";
        public List<WeaponStrengthEntry> weapon_strength_list = new List<WeaponStrengthEntry>();
        /// <summary>
        /// 武器/投射物子类型，对应 C++ release 实体的 oid/type_sub 运行时语义。
        /// 决定 state=1002 时的重力分级：
        ///   0x7C(124) = 极轻（气球），0x78(120) = 轻（苦无），0x65(101) = 中等
        ///   0x3E7(999) = 特殊落地（frame=101），其余走默认重力 0.5667
        /// </summary>
        public int type_sub = 0;
    }

    /// <summary>
    /// 精灵图文件信息
    /// </summary>
    [System.Serializable]
    public class SpriteFileInfo
    {
        public string filePath = "";
        public int startFrame = 0;
        public int endFrame = 0;
        public int width = 79;
        public int height = 79;
        public int row = 10;
        public int col = 7;

        public SpriteFileInfo() { }

        public SpriteFileInfo(string path, int start, int end, int w, int h, int r, int c)
        {
            filePath = path;
            startFrame = start;
            endFrame = end;
            width = w;
            height = h;
            row = r;
            col = c;
        }
    }

    /// <summary>
    /// 用于JSON序列化的包装器
    /// </summary>
    [System.Serializable]
    public class LF2CharacterDataWrapper
    {
        public int characterId;
        public LF2CharacterData characterData;

        public LF2CharacterDataWrapper(int id, LF2CharacterData data)
        {
            characterId = id;
            characterData = data;
        }
    }

    /// <summary>
    /// ScriptableObject版本的角色数据
    /// </summary>
    [CreateAssetMenu(fileName = "LF2CharacterData", menuName = "LF2/Character Data")]
    public class LF2CharacterDataAsset : ScriptableObject
    {
        public int characterId;
        public LF2CharacterData characterData = new LF2CharacterData();
    }
}
