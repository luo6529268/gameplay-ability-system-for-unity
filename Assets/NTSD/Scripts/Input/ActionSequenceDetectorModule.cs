using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Tools;
using System;
using System.Collections.Generic;

namespace NTSD.Input
{
    /// <summary>
    /// �������м�������� C#��������Ҫ���ص�Ԥ�����ϣ���
    ///
    /// �����ϸ�����ʵ�֣�ActionSequenceDetector.cs����
    /// - ������Դ��CharacterInputModule.InputBuffer���� tick ���룩
    /// - �жϷ��ţ�ʹ�� FuncKeyMask.None ��Ӧ FLF combodec.js �� '_' ���
    /// - best-match������ƥ����� sequence����� custom > basic
    ///
    /// SimOrder=5 (Input): �������ڽ�ɫ�߼�֮ǰִ��
    /// </summary>
    public sealed class ActionSequenceDetectorModule : ISimObject
    {
        [Serializable]
        public struct KeyInput
        {
            public FuncKeyMask key;
            public int time;
        }

        private sealed class ComboCandidate
        {
            public readonly ComboConfig.ComboDefinition combo;
            public readonly int sourcePriority;
            public readonly int originalIndex;

            public ComboCandidate(ComboConfig.ComboDefinition combo, int sourcePriority, int originalIndex)
            {
                this.combo = combo;
                this.sourcePriority = sourcePriority;
                this.originalIndex = originalIndex;
            }
        }

        public int timeoutFrames = 0;
        public int combooutFrames = 0;
        public bool clearOnCombo = true;
        public bool debugLog = true;

        private readonly List<KeyInput> _sequence = new List<KeyInput>();
        private readonly Dictionary<FuncKeyMask, bool> _keyState = new Dictionary<FuncKeyMask, bool>();

        private readonly List<ComboCandidate> _comboCandidates = new List<ComboCandidate>();

        private int _time = 0;
        private int _timeout = -1;
        private int _comboout = -1;

        private SimInputBuffer _inputBuffer;
        private LF2Character _lf2Character;

        public void Initialize(SimInputBuffer buffer, LF2Character lf2Character)
        {
            _inputBuffer = buffer;
            _lf2Character = lf2Character;
            InitializeCombos();
        }

        public int SimOrder => SimOrderConstants.Input;
        public int StableId { get; private set; }

        public void SetStableId(int stableId) => StableId = stableId;

        public void OnAdded(SimContext ctx) { }
        public void OnRemoved(SimContext ctx) { }

        public void SimTransit(int tickIndex)
        {
            if (_inputBuffer != null && _inputBuffer.TryDequeueAll(tickIndex, out var events))
            {
                foreach (var evt in events)
                {
                    if (evt.down) RecordAction(evt.key);
                    else OnKeyUp(evt.key);
                }
            }
            Frame_Update(tickIndex);
        }

        public void SimTU(int tickIndex) { }

        public void SimLateTick(int tickIndex) { }

        public void RecordAction(FuncKeyMask key)
        {
            // 反汇编 sub_414D80: 每次按键时推入 NTSD 编码到输入序列
            _lf2Character?.RecordInputKey(FuncKeyMaskToNtsdCode(key));
            ProcessKeyDown(key);
        }

        public void OnKeyUp(FuncKeyMask key)
        {
            if (_keyState.ContainsKey(key))
            {
                _keyState[key] = false;
            }
        }

        public void ClearSequence()
        {
            _sequence.Clear();
            _timeout = _time - 1;
            _comboout = _time - 1;

            if (debugLog)
            {
                Log.Info($"[ActionSequenceDetectorModule] Sequence cleared at time {_time}");
            }
        }

        /// <summary>
        /// 将 FuncKeyMask 转换为 NTSD 输入序列编码（对应反汇编 sub_414D80 调用参数）
        /// att→9, jump→6, down→5, def→0, left→8, right→2, up→4
        /// </summary>
        private static int FuncKeyMaskToNtsdCode(FuncKeyMask key)
        {
            switch (key)
            {
                case FuncKeyMask.att:   return 9;
                case FuncKeyMask.jump:  return 6;
                case FuncKeyMask.down:  return 5;
                case FuncKeyMask.def:   return 0;
                case FuncKeyMask.left:  return 8;
                case FuncKeyMask.right: return 2;
                case FuncKeyMask.up:    return 4;
                default:                return -1;
            }
        }

        private void ProcessKeyDown(FuncKeyMask key)
        {
            bool push = true;

            if (_keyState.ContainsKey(key) && _keyState[key])
            {
                push = false;
                if (debugLog)
                {
                    Log.Info($"[ActionSequenceDetectorModule] Key {key} is already pressed, ignoring");
                }
            }

            _keyState[key] = true;

            if (timeoutFrames > 0)
            {
                _timeout = _time + timeoutFrames;
            }
            if (combooutFrames > 0)
            {
                _comboout = _time + combooutFrames;
            }

            if (push)
            {
                _sequence.Add(new KeyInput { key = key, time = _time });

                if (debugLog)
                {
                    Log.Info($"[ActionSequenceDetectorModule] Key pressed: {key}, Time: {_time}, Sequence: {GetSequenceString()}");
                }
            }

            if (_comboCandidates.Count > 0 && push)
            {
                foreach (var candidate in _comboCandidates)
                {
                    bool detected = true;
                    int j = _sequence.Count - candidate.combo.sequence.Length;
                    if (j < 0) detected = false;
                    else
                    {
                        for (int k = 0; j < _sequence.Count; j++, k++)
                        {
                            if (candidate.combo.sequence[k] != _sequence[j].key ||
                              (candidate.combo.maxTimeFrames != 0 && _sequence[_sequence.Count - 1].time - _sequence[j].time > candidate.combo.maxTimeFrames))
                            {
                                detected = false;
                                break;
                            }
                        }
                    }

                    if (detected)
                    {
                        _lf2Character?.OnComboDetected(candidate.combo);
                        if (candidate.combo.clearOnCombo && clearOnCombo)
                        {
                            ClearSequence();
                        }
                    }
                }
            }
        }

        private void Frame_Update(int tickIndex)
        {
            if (_time == _timeout)
            {
                ClearSequence();
            }

            if (_time == _comboout)
            {
                _sequence.Add(new KeyInput
                {
                    key = FuncKeyMask.None,
                    time = _time
                });

                if (debugLog)
                {
                    Log.Info($"[ActionSequenceDetectorModule] Combo interrupt inserted at time {_time}");
                }
            }

            _time = tickIndex;
        }

        private string GetSequenceString()
        {
            if (_sequence.Count == 0) return "";
            var parts = new string[_sequence.Count];
            for (int i = 0; i < _sequence.Count; i++)
            {
                parts[i] = _sequence[i].key.ToString();
            }
            return string.Join(",", parts);
        }

        private void InitializeCombos()
        {
            _comboCandidates.Clear();
            int index = 0;

            for (int i = 0; i < ComboConfig.ComboList.Length; i++)
            {
                _comboCandidates.Add(new ComboCandidate(ComboConfig.ComboList[i], sourcePriority: 1, originalIndex: index++));
            }
        }
    }
}
