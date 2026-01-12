using MoreMountains.TopDownEngine;
using NTSD.Simulation;
using NTSD.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Input
{
    /// <summary>
    /// 动作序列检测器（纯 C#，不再需要挂载到预制体上）。
    ///
    /// 语义严格对齐旧实现（ActionSequenceDetector.cs）：
    /// - 输入来源：CharacterInputModule.InputBuffer（按 tick 对齐）
    /// - 中断符号：使用 FuncKeyMask.None 对应 FLF combodec.js 的 '_' 标记
    /// - best-match：优先匹配更长 sequence，其次 custom > basic
    /// </summary>
    public sealed class ActionSequenceDetectorModule : ISimObject, ICharacterModule
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

        public int timeoutFrames = 1800;
        public int combooutFrames = 0;
        public bool clearOnCombo = true;
        public bool debugLog = true;

        private readonly List<KeyInput> _sequence = new List<KeyInput>();
        private readonly Dictionary<FuncKeyMask, bool> _keyState = new Dictionary<FuncKeyMask, bool>();

        private readonly List<ComboCandidate> _comboCandidates = new List<ComboCandidate>();

        private int _time = 0;
        private int _timeout = -1;
        private int _comboout = -1;

        private Character _hub;

        public event Action<ComboConfig.ComboDefinition> OnComboDetected;

        public int ModuleOrder => CharacterModuleOrder.ComboDetector;

        public void ModuleSetup(Character character)
        {
            _hub = character;
        }

        public void ModuleInitialize()
        {
            InitializeCombos();
        }

        public void ModuleBind() { }
        public void ModuleUnbind() { }

        public int SimOrder => 50;
        public int StableId { get; private set; }

        public void SetStableId(int stableId) => StableId = stableId;

        public void OnAdded(SimContext ctx) { }
        public void OnRemoved(SimContext ctx) { }

        public void SimTick(int tickIndex)
        {
            // 1) consume buffered input events for this tick
            if (_hub?._CharacterInput != null && _hub._CharacterInput.InputBuffer != null)
            {
                if (_hub._CharacterInput.InputBuffer.TryDequeueAll(tickIndex, out var events))
                {
                    foreach (var evt in events)
                    {
                        if (evt.down)
                        {
                            RecordAction(evt.key);
                        }
                        else
                        {
                            OnKeyUp(evt.key);
                        }
                    }
                }
            }

            // 2) time management + insert interrupt + clear
            Frame_Update(tickIndex);
        }

        public void SimLateTick(int tickIndex) { }

        public void RecordAction(FuncKeyMask key)
        {
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
                        // 逐个检查按键序列和时间间隔
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

                    // 如果检测到连招
                    if (detected)
                    {
                        OnComboDetected?.Invoke(candidate.combo);

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

            // Global combos (hardcoded in ComboConfig)
            for (int i = 0; i < ComboConfig.ComboList.Length; i++)
            {
                _comboCandidates.Add(new ComboCandidate(ComboConfig.ComboList[i], sourcePriority: 1, originalIndex: index++));
            }
        }
    }
}
