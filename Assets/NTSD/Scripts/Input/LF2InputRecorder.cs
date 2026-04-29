using System.Collections.Generic;
using System.Text;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Input
{
    /// <summary>
    /// 输入录制器，对应 FLF core/controller-recorder.js control_recorder。
    ///
    /// 用法：
    ///   recorder = new LF2InputRecorder();
    ///   character.Controller = new LF2RecordingController(realController, recorder);
    ///   // 战斗结束后
    ///   string json = recorder.ExportJson();
    /// </summary>
    public class LF2InputRecorder
    {
        public readonly struct InputEvent
        {
            public readonly int  t;    // 帧时间
            public readonly string k;  // 按键名
            public readonly bool d;    // 按下=true 释放=false

            public InputEvent(int t, string k, bool d)
            {
                this.t = t; this.k = k; this.d = d;
            }
        }

        private int _time;
        private readonly List<InputEvent> _rec = new List<InputEvent>(256);

        // 由 LF2RecordingController 每帧调用
        internal void Advance() => _time++;

        // 由 LF2RecordingController 每次按键时调用
        internal void Record(string key, bool down)
        {
            _rec.Add(new InputEvent(_time, key, down));
        }

        public IReadOnlyList<InputEvent> Events => _rec;

        /// <summary>
        /// 导出 JSON 字符串（对应 FLF control_recorder.export_str）
        /// 格式：[{"t":0,"k":"right","d":true}, ...]
        /// </summary>
        public string ExportJson()
        {
            var sb = new StringBuilder();
            sb.Append("[\n");
            for (int i = 0; i < _rec.Count; i++)
            {
                if (i != 0) sb.Append(',');
                var e = _rec[i];
                sb.Append($"{{\"t\":{e.t},\"k\":\"{e.k}\",\"d\":{(e.d ? "true" : "false")}}}");
            }
            sb.Append("\n]");
            _rec.Clear();
            _time = 0;
            return sb.ToString();
        }

        public void Reset()
        {
            _rec.Clear();
            _time = 0;
        }
    }

    /// <summary>
    /// 录制包装控制器：透传真实输入，同时旁录到 LF2InputRecorder。
    /// 对应 FLF 将 control_recorder 加入 controller.child 的模式。
    /// </summary>
    public sealed class LF2RecordingController : ILF2Controller
    {
        private readonly ILF2Controller  _inner;
        private readonly LF2InputRecorder _recorder;

        // 上一帧的按键状态（用于检测边沿）
        private bool _prevUp, _prevDown, _prevLeft, _prevRight;
        private bool _prevAttack, _prevJump, _prevDefend;

        public LF2RecordingController(ILF2Controller inner, LF2InputRecorder recorder)
        {
            _inner    = inner;
            _recorder = recorder;
        }

        // ILF2Controller 直接透传
        public bool IsUp     => _inner.IsUp;
        public bool IsDown   => _inner.IsDown;
        public bool IsLeft   => _inner.IsLeft;
        public bool IsRight  => _inner.IsRight;
        public bool IsAttack => _inner.IsAttack;
        public bool IsJump   => _inner.IsJump;
        public bool IsDefend => _inner.IsDefend;

        public SimInputBuffer InputBuffer { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public int Dirv()                     => _inner.Dirv();
        public (int dx, int dz) GetMoveInput() => _inner.GetMoveInput();

        public void SetInputID(int inputId)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 每 SimTick 调用一次：检测按键边沿并录制，然后推进录制器时钟。
        /// 对应 FLF control_recorder.frame() + key() 调用时机。
        /// </summary>
        public void Tick()
        {
            RecordEdge("up",     _inner.IsUp,     ref _prevUp);
            RecordEdge("down",   _inner.IsDown,   ref _prevDown);
            RecordEdge("left",   _inner.IsLeft,   ref _prevLeft);
            RecordEdge("right",  _inner.IsRight,  ref _prevRight);
            RecordEdge("att",    _inner.IsAttack, ref _prevAttack);
            RecordEdge("jump",   _inner.IsJump,   ref _prevJump);
            RecordEdge("def",    _inner.IsDefend, ref _prevDefend);
            _recorder.Advance();
        }

        private void RecordEdge(string key, bool current, ref bool prev)
        {
            if (current != prev)
            {
                _recorder.Record(key, current);
                prev = current;
            }
        }
    }
}
