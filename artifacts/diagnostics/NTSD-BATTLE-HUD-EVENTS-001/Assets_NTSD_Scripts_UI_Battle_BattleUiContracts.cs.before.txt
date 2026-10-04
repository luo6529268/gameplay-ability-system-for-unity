using System;
using UnityEngine;

namespace NTSD.UI.Battle
{
    [Serializable]
    public sealed class BattleHudState
    {
        public bool IsVisible = true;
        public int CharacterId;
        public string DisplayName = string.Empty;
        public Sprite HeadSprite;
        [Range(0f, 1f)] public float Hp01 = 1f;
        [Range(0f, 1f)] public float HpPreview01 = 1f;
        [Range(0f, 1f)] public float Mp01 = 1f;
    }

    [Serializable]
    public sealed class BattleComboState
    {
        public bool IsVisible;
        public int HitCount;
        public string DisplayText = string.Empty;
    }

    [Serializable]
    public sealed class BattleControlsState
    {
        public bool IsVisible = true;
        public bool IsAttackHeld;
        public bool IsJumpHeld;
        public bool IsDefendHeld;
    }

    /// <summary>
    /// 当前 Battle HUD 的可复用表现快照，不包含角色对象引用或输入写入逻辑。
    /// </summary>
    public sealed class BattleUiSnapshot
    {
        public BattleUiSnapshot()
        {
            Clear();
        }

        public int Version { get; set; }

        public BattleHudState Hud { get; } = new BattleHudState();

        public BattleComboState Combo { get; } = new BattleComboState();

        public BattleControlsState Controls { get; } = new BattleControlsState();

        public void Clear()
        {
            Version = 0;
            Hud.IsVisible = false;
            Hud.CharacterId = 0;
            Hud.DisplayName = string.Empty;
            Hud.HeadSprite = null;
            Hud.Hp01 = 1f;
            Hud.HpPreview01 = 1f;
            Hud.Mp01 = 1f;

            Combo.IsVisible = false;
            Combo.HitCount = 0;
            Combo.DisplayText = string.Empty;

            Controls.IsVisible = false;
            Controls.IsAttackHeld = false;
            Controls.IsJumpHeld = false;
            Controls.IsDefendHeld = false;
        }
    }

    /// <summary>
    /// Runtime/presentation 到 Canvas UI 的数据边界。
    /// UI 只消费目标快照，不直接读取角色对象。
    /// </summary>
    public interface IBattleUiSnapshotSource
    {
        bool TryCopySnapshot(BattleUiSnapshot destination);
    }

    /// <summary>
    /// 可供动态 TEngine UI 通过 UserData 传入的轻量上下文。
    /// 当前 NTSD_Battle 的场景常驻 HUD 不依赖此上下文。
    /// </summary>
    public sealed class BattleUiContext
    {
        public BattleUiContext(IBattleUiSnapshotSource snapshotSource)
        {
            SnapshotSource = snapshotSource;
        }

        public IBattleUiSnapshotSource SnapshotSource { get; }
    }
}
