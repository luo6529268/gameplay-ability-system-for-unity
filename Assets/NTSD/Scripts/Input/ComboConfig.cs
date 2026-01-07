namespace NTSD.Input
{
    /// <summary>
    /// 连招配置（全局常量脚本）- 对应 FLF 的 global.js（combo_list/combo_tag/combo_dir/combo_priority）。
    ///
    /// 约定：
    /// - 不使用 ScriptableObject/JSON，直接在 C# 中死写，便于完全复刻与版本控制
    /// - “新增连招” = 改代码增加一条定义
    /// </summary>
    public static class ComboConfig
    {
        public readonly struct ComboDefinition
        {
            public readonly string name;
            public readonly FuncKeyMask[] sequence;
            public readonly bool clearOnCombo;
            public readonly int maxTimeFrames;

            public ComboDefinition(string name, FuncKeyMask[] sequence, bool clearOnCombo = true, int maxTimeFrames = 0)
            {
                this.name = name;
                this.sequence = sequence;
                this.clearOnCombo = clearOnCombo;
                this.maxTimeFrames = maxTimeFrames;
            }
        }

        /// <summary>
        /// 全部连招定义（包含单键/双击/组合键）。
        /// </summary>
        public static readonly ComboDefinition[] ComboList =
        {
            // single-key "combos"
            new ComboDefinition("left", new[] { FuncKeyMask.Left }, clearOnCombo: false),
            new ComboDefinition("right", new[] { FuncKeyMask.Right }, clearOnCombo: false),
            new ComboDefinition("up", new[] { FuncKeyMask.Up }, clearOnCombo: false),
            new ComboDefinition("down", new[] { FuncKeyMask.Down }, clearOnCombo: false),
            new ComboDefinition("def", new[] { FuncKeyMask.Defend }, clearOnCombo: false),
            new ComboDefinition("jump", new[] { FuncKeyMask.Jump }, clearOnCombo: false),
            new ComboDefinition("att", new[] { FuncKeyMask.Attack }, clearOnCombo: false),

            // double-tap (LF2 maxtime = 9 frames)
            new ComboDefinition("left-left", new[] { FuncKeyMask.Left, FuncKeyMask.Left }, clearOnCombo: true, maxTimeFrames: 9),
            new ComboDefinition("right-right", new[] { FuncKeyMask.Right, FuncKeyMask.Right }, clearOnCombo: true, maxTimeFrames: 9),

            // 2-key
            new ComboDefinition("jump-att", new[] { FuncKeyMask.Jump, FuncKeyMask.Attack }, clearOnCombo: false),

            // 3-key combos (FLF global.js combo_list)
            new ComboDefinition("D<A", new[] { FuncKeyMask.Defend, FuncKeyMask.Left, FuncKeyMask.Attack }, clearOnCombo: false),
            new ComboDefinition("D>A", new[] { FuncKeyMask.Defend, FuncKeyMask.Right, FuncKeyMask.Attack }, clearOnCombo: false),
            new ComboDefinition("DvA", new[] { FuncKeyMask.Defend, FuncKeyMask.Down, FuncKeyMask.Attack }, clearOnCombo: true),
            new ComboDefinition("D^A", new[] { FuncKeyMask.Defend, FuncKeyMask.Up, FuncKeyMask.Attack }, clearOnCombo: true),

            new ComboDefinition("D<J", new[] { FuncKeyMask.Defend, FuncKeyMask.Left, FuncKeyMask.Jump }, clearOnCombo: true),
            new ComboDefinition("D>J", new[] { FuncKeyMask.Defend, FuncKeyMask.Right, FuncKeyMask.Jump }, clearOnCombo: true),
            new ComboDefinition("DvJ", new[] { FuncKeyMask.Defend, FuncKeyMask.Down, FuncKeyMask.Jump }, clearOnCombo: true),
            new ComboDefinition("D^J", new[] { FuncKeyMask.Defend, FuncKeyMask.Up, FuncKeyMask.Jump }, clearOnCombo: true),

            // 4-key combos
            new ComboDefinition("D<AJ", new[] { FuncKeyMask.Defend, FuncKeyMask.Left, FuncKeyMask.Attack, FuncKeyMask.Jump }, clearOnCombo: true),
            new ComboDefinition("D>AJ", new[] { FuncKeyMask.Defend, FuncKeyMask.Right, FuncKeyMask.Attack, FuncKeyMask.Jump }, clearOnCombo: true),

            // 3-key variant
            new ComboDefinition("DJA", new[] { FuncKeyMask.Defend, FuncKeyMask.Jump, FuncKeyMask.Attack }, clearOnCombo: true),
        };

        /// <summary>
        /// global.js: combo_tag (name -&gt; frame tag)
        /// </summary>
        public static string GetComboTag(string comboName)
        {
            switch (comboName)
            {
                case "def": return "hit_d";
                case "jump": return "hit_j";
                case "att": return "hit_a";
                case "D<A": return "hit_Fa";
                case "D>A": return "hit_Fa";
                case "DvA": return "hit_Da";
                case "D^A": return "hit_Ua";
                case "D<J": return "hit_Fj";
                case "D>J": return "hit_Fj";
                case "DvJ": return "hit_Dj";
                case "D^J": return "hit_Uj";
                case "D<AJ": return "hit_Fj";
                case "D>AJ": return "hit_Fj";
                case "DJA": return "hit_ja";
                default: return null;
            }
        }

        /// <summary>
        /// global.js: combo_dir (name -&gt; left/right)
        /// </summary>
        public static string GetComboDirection(string comboName)
        {
            switch (comboName)
            {
                case "D<A":
                case "D<J":
                case "D<AJ":
                    return "left";
                case "D>A":
                case "D>J":
                case "D>AJ":
                    return "right";
                default:
                    return null;
            }
        }
    }
}

