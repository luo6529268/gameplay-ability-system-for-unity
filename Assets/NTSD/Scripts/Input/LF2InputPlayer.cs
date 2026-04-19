using System.Collections.Generic;
using NTSD.Animation.LF2Objects;
using UnityEngine;

namespace NTSD.Input
{
    /// <summary>
    /// 输入回放控制器，对应 FLF core/controller-recorder.js control_player。
    ///
    /// 实现 ILF2Controller，可直接替换角色的 Controller 字段进行回放。
    ///
    /// 用法：
    ///   var events = LF2InputParser.ParseJson(jsonStr);
    ///   character.Controller = new LF2InputPlayer(events);
    ///   // 每 SimTick 调用 player.Tick() 推进
    /// </summary>
    public sealed class LF2InputPlayer : ILF2Controller
    {
        private readonly IReadOnlyList<LF2InputRecorder.InputEvent> _rec;
        private int _index;
        private int _time;

        // 当前按键状态
        private bool _up, _down, _left, _right, _attack, _jump, _defend;

        public LF2InputPlayer(IReadOnlyList<LF2InputRecorder.InputEvent> record)
        {
            _rec = record;
        }

        // ILF2Controller 读取当前回放状态
        public bool IsUp     => _up;
        public bool IsDown   => _down;
        public bool IsLeft   => _left;
        public bool IsRight  => _right;
        public bool IsAttack => _attack;
        public bool IsJump   => _jump;
        public bool IsDefend => _defend;

        public int Dirv()
        {
            if (_right) return 1;
            if (_left)  return -1;
            return 0;
        }

        public (int dx, int dz) GetMoveInput()
        {
            int dx = _right ? 1 : (_left  ? -1 : 0);
            int dz = _down  ? 1 : (_up    ? -1 : 0);
            return (dx, dz);
        }

        /// <summary>
        /// 每 SimTick 调用：把当前帧之前的所有录制事件应用到按键状态。
        /// 对应 FLF control_player.frame() + fetch()。
        /// </summary>
        public void Tick()
        {
            _time++;
            Fetch();
        }

        private void Fetch()
        {
            while (_index < _rec.Count && _rec[_index].t <= _time)
            {
                Apply(_rec[_index].k, _rec[_index].d);
                _index++;
            }
        }

        private void Apply(string key, bool down)
        {
            switch (key)
            {
                case "up":    _up     = down; break;
                case "down":  _down   = down; break;
                case "left":  _left   = down; break;
                case "right": _right  = down; break;
                case "att":   _attack = down; break;
                case "jump":  _jump   = down; break;
                case "def":   _defend = down; break;
                default:
                    Debug.LogWarning($"[LF2InputPlayer] Unknown key: {key}");
                    break;
            }
        }

        public void Reset()
        {
            _index = 0;
            _time  = 0;
            _up = _down = _left = _right = _attack = _jump = _defend = false;
        }

        /// <summary>是否已回放到末尾</summary>
        public bool IsFinished => _index >= _rec.Count;
    }

    /// <summary>
    /// JSON 解析工具（对应 FLF control_player 构造时传入的 record 数组）
    /// </summary>
    public static class LF2InputParser
    {
        /// <summary>
        /// 解析 LF2InputRecorder.ExportJson() 生成的 JSON 字符串。
        /// 格式：[{"t":0,"k":"right","d":true}, ...]
        /// </summary>
        public static List<LF2InputRecorder.InputEvent> ParseJson(string json)
        {
            var result = new List<LF2InputRecorder.InputEvent>(128);
            if (string.IsNullOrEmpty(json)) return result;

            // 简单手写解析，避免引入 JSON 库依赖
            // 每条记录格式固定：{"t":N,"k":"KEY","d":BOOL}
            int i = 0;
            while (i < json.Length)
            {
                int tStart = json.IndexOf("\"t\":", i);
                if (tStart < 0) break;
                tStart += 4;
                int tEnd = json.IndexOfAny(new[] { ',', '}' }, tStart);
                int t = int.Parse(json.Substring(tStart, tEnd - tStart).Trim());

                int kStart = json.IndexOf("\"k\":\"", tEnd) + 5;
                int kEnd   = json.IndexOf('"', kStart);
                string k   = json.Substring(kStart, kEnd - kStart);

                int dStart = json.IndexOf("\"d\":", kEnd) + 4;
                int dEnd   = json.IndexOfAny(new[] { ',', '}' }, dStart);
                bool d     = json.Substring(dStart, dEnd - dStart).Trim() == "true";

                result.Add(new LF2InputRecorder.InputEvent(t, k, d));
                i = dEnd + 1;
            }

            return result;
        }
    }
}
