using System.Collections.Generic;
using NTSD.Animation.LF2Objects;

namespace NTSD.Input
{
    /// <summary>
    /// AI 输入控制器，对应 FLF AI.js AIcon。
    ///
    /// 职责：
    /// - AI 脚本通过 Key/Keypress/Keyseq 将按键事件写入缓冲区
    /// - 每帧由 Fetch() 将缓冲区内容推送给实际按键状态
    /// - 实现 ILF2Controller，可替换角色的 Controller 字段
    ///
    /// 对应 FLF AI.js: AIcon.key / keypress / keyseq / fetch / flush
    /// </summary>
    public class LF2AIController : ILF2Controller
    {
        // 当前按键状态（对应 FLF AIcon.state）
        private readonly Dictionary<string, bool> _state = new Dictionary<string, bool>();

        // 待 fetch 的缓冲队列（对应 FLF AIcon.buf）
        private readonly List<(string key, bool down)> _buf = new List<(string, bool)>(8);

        // ==================== ILF2Controller 实现 ====================

        public bool IsUp    => GetState("up");
        public bool IsDown  => GetState("down");
        public bool IsLeft  => GetState("left");
        public bool IsRight => GetState("right");
        public bool IsAttack  => GetState("att");
        public bool IsJump    => GetState("jump");
        public bool IsDefend  => GetState("def");

        public int Dirv()
        {
            if (IsRight) return 1;
            if (IsLeft)  return -1;
            return 0;
        }

        public (int dx, int dz) GetMoveInput()
        {
            int dx = IsRight ? 1 : (IsLeft ? -1 : 0);
            int dz = IsDown  ? 1 : (IsUp   ? -1 : 0);
            return (dx, dz);
        }

        // ==================== AI 脚本 API ====================

        /// <summary>
        /// 设置按键状态（对应 FLF AIcon.key）
        /// sync=true 时写入缓冲区，由 Fetch() 统一推送
        /// </summary>
        public void Key(string key, bool down)
        {
            _buf.Add((key, down));
        }

        /// <summary>
        /// 模拟按下/释放一次按键（对应 FLF AIcon.keypress）
        /// keypress(key)       → down=1, up=0
        /// keypress(key,1,1)   → hold down
        /// keypress(key,0,0)   → release
        /// </summary>
        public void Keypress(string key, int x = 1, int y = 0)
        {
            if (x == 1 && y == 0)
            {
                if (GetState(key)) Key(key, false);
                Key(key, true);
                Key(key, false);
            }
            else if (x == 1 && y == 1)
            {
                if (!GetState(key)) Key(key, true);
            }
            else if (x == 0 && y == 0)
            {
                if (GetState(key)) Key(key, false);
            }
        }

        /// <summary>
        /// 顺序按下一组按键（对应 FLF AIcon.keyseq）
        /// </summary>
        public void Keyseq(IList<string> seq)
        {
            for (int i = 0; i < seq.Count; i++)
                Keypress(seq[i]);
        }

        /// <summary>
        /// 将缓冲区内容推送到按键状态（对应 FLF AIcon.fetch）
        /// 每帧 TU 开始前调用
        /// </summary>
        public void Fetch()
        {
            for (int i = 0; i < _buf.Count; i++)
            {
                _state[_buf[i].key] = _buf[i].down;
            }
            _buf.Clear();
        }

        /// <summary>
        /// 丢弃所有缓冲区内容（对应 FLF AIcon.flush）
        /// </summary>
        public void Flush()
        {
            _buf.Clear();
        }

        /// <summary>
        /// 清除所有按键状态（对应 FLF AIcon.clear_states）
        /// </summary>
        public void ClearStates()
        {
            foreach (var key in new List<string>(_state.Keys))
                _state[key] = false;
        }

        private bool GetState(string key)
        {
            return _state.TryGetValue(key, out bool v) && v;
        }
    }
}
