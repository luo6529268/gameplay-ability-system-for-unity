using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace NTSD.UI
{
    /// <summary>
    /// 角色UI精灵数据
    /// 存储单个角色的头像(Head)和小图(Small)精灵
    /// </summary>
    [System.Serializable]
    public class CharacterUISprites
    {
        /// <summary>
        /// 头像精灵，来自 LF2CharacterData.head 路径加载
        /// 用于角色选择界面等需要显示角色头像的地方
        /// </summary>
        public Sprite HeadSprite;

        /// <summary>
        /// 小图精灵，来自 LF2CharacterData.small 路径加载
        /// 用于角色列表、小图标等场景
        /// </summary>
        public Sprite SmallSprite;
    }

    /// <summary>
    /// 角色UI资源管理器
    /// 
    /// 职责:
    /// 1. 存储所有角色的UI精灵资源（Head和Small）
    /// 2. 提供根据CharacterID获取对应精灵的接口
    /// 
    /// 加载时机:
    /// 由 CharacterAnimtorManager 在解析每个角色的 dat 文件后调用 SetCharacterUISprites
    /// 不需要单独的加载流程，属于 dat 文件加载的一部分
    /// 
    /// 使用方式:
    /// - SelectRoleItem 等UI组件通过 GetHeadSprite(characterId) 获取角色头像
    /// - 其他需要角色小图的地方通过 GetSmallSprite(characterId) 获取
    /// </summary>
    public class CharacterUIResourceManager : MMSingleton<CharacterUIResourceManager>
    {
        #region 数据存储

        /// <summary>
        /// 角色UI精灵字典
        /// Key: 角色ID (CharacterID)
        /// Value: 该角色的UI精灵数据 (Head和Small)
        /// </summary>
        private Dictionary<int, CharacterUISprites> characterUISprites = new Dictionary<int, CharacterUISprites>();

        #endregion

        #region 初始化

        protected override void InitializeSingleton()
        {
            base.InitializeSingleton();
            characterUISprites = new Dictionary<int, CharacterUISprites>();
        }

        #endregion

        #region 设置接口 (供 CharacterAnimtorManager 调用)

        /// <summary>
        /// 设置角色的UI精灵
        /// 由 CharacterAnimtorManager 在解析 dat 文件后调用
        /// </summary>
        /// <param name="characterId">角色ID</param>
        /// <param name="headSprite">头像精灵，可为null</param>
        /// <param name="smallSprite">小图精灵，可为null</param>
        public void SetCharacterUISprites(int characterId, Sprite headSprite, Sprite smallSprite)
        {
            if (headSprite == null && smallSprite == null)
            {
                return;
            }

            characterUISprites[characterId] = new CharacterUISprites
            {
                HeadSprite = headSprite,
                SmallSprite = smallSprite
            };
        }

        /// <summary>
        /// 清空所有已加载的UI精灵
        /// </summary>
        public void Clear()
        {
            characterUISprites.Clear();
        }

        #endregion

        #region 获取接口 (供UI组件调用)

        /// <summary>
        /// 获取角色的头像精灵
        /// </summary>
        /// <param name="characterId">角色ID</param>
        /// <returns>头像精灵，如果不存在返回null</returns>
        public Sprite GetHeadSprite(int characterId)
        {
            if (characterUISprites.TryGetValue(characterId, out CharacterUISprites sprites))
            {
                return sprites.HeadSprite;
            }
            return null;
        }

        /// <summary>
        /// 获取角色的小图精灵
        /// </summary>
        /// <param name="characterId">角色ID</param>
        /// <returns>小图精灵，如果不存在返回null</returns>
        public Sprite GetSmallSprite(int characterId)
        {
            if (characterUISprites.TryGetValue(characterId, out CharacterUISprites sprites))
            {
                return sprites.SmallSprite;
            }
            return null;
        }

        /// <summary>
        /// 获取角色的完整UI精灵数据
        /// </summary>
        /// <param name="characterId">角色ID</param>
        /// <returns>UI精灵数据，如果不存在返回null</returns>
        public CharacterUISprites GetCharacterUISprites(int characterId)
        {
            characterUISprites.TryGetValue(characterId, out CharacterUISprites sprites);
            return sprites;
        }

        /// <summary>
        /// 检查是否已加载指定角色的UI精灵
        /// </summary>
        /// <param name="characterId">角色ID</param>
        /// <returns>如果已加载返回true</returns>
        public bool HasCharacterUISprites(int characterId)
        {
            return characterUISprites.ContainsKey(characterId);
        }

        /// <summary>
        /// 获取已加载的角色数量
        /// </summary>
        public int LoadedCount => characterUISprites.Count;

        #endregion
    }
}
