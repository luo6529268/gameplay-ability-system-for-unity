using MoreMountains.TopDownEngine;
using NTSD.Simulation;
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

        public int timeoutFrames = 60;
        public int combooutFrames = 10;
        public bool clearOnCombo = true;
        public bool debugLog = false;

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
            Frame_Update();
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
                Debug.Log($"[ActionSequenceDetectorModule] Sequence cleared at time {_time}");
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
                    Debug.Log($"[ActionSequenceDetectorModule] Key {key} is already pressed, ignoring");
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
                    Debug.Log($"[ActionSequenceDetectorModule] Key pressed: {key}, Time: {_time}, Sequence: {GetSequenceString()}");
                }
            }

            if (_comboCandidates.Count > 0 && push)
            {
                if (TryFindBestMatch(out var bestCombo))
                {
                    if (debugLog)
                    {
                        Debug.Log($"[ActionSequenceDetectorModule] Combo detected: {bestCombo.name}");
                    }

                    OnComboDetected?.Invoke(bestCombo);

                    if (bestCombo.clearOnCombo && clearOnCombo)
                    {
                        ClearSequence();
                    }
                }
            }
        }

        private void Frame_Update()
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
                    Debug.Log($"[ActionSequenceDetectorModule] Combo interrupt inserted at time {_time}");
                }
            }

            _time++;
        }

        private bool TryFindBestMatch(out ComboConfig.ComboDefinition best)
        {
            bool hasBest = false;
            best = default;

            int bestLen = -1;
            int bestSourcePriority = -1;
            int bestOriginalIndex = int.MaxValue;

            for (int i = 0; i < _comboCandidates.Count; i++)
            {
                var candidate = _comboCandidates[i];
                var combo = candidate.combo;

                if (!MatchCombo(combo))
                {
                    continue;
                }

                int len = combo.sequence == null ? 0 : combo.sequence.Length;
                if (len > bestLen)
                {
                    hasBest = true;
                    best = combo;
                    bestLen = len;
                    bestSourcePriority = candidate.sourcePriority;
                    bestOriginalIndex = candidate.originalIndex;
                    continue;
                }

                if (len == bestLen)
                {
                    if (candidate.sourcePriority > bestSourcePriority)
                    {
                        hasBest = true;
                        best = combo;
                        bestLen = len;
                        bestSourcePriority = candidate.sourcePriority;
                        bestOriginalIndex = candidate.originalIndex;
                        continue;
                    }

                    if (candidate.sourcePriority == bestSourcePriority && candidate.originalIndex < bestOriginalIndex)
                    {
                        hasBest = true;
                        best = combo;
                        bestLen = len;
                        bestSourcePriority = candidate.sourcePriority;
                        bestOriginalIndex = candidate.originalIndex;
                    }
                }
            }

            return hasBest;
        }

        private bool MatchCombo(ComboConfig.ComboDefinition combo)
        {
            if (combo.sequence == null || combo.sequence.Length == 0) return false;

            int startIndex = _sequence.Count - combo.sequence.Length;
            if (startIndex < 0) return false;

            for (int i = 0; i < combo.sequence.Length; i++)
            {
                int seqIndex = startIndex + i;

                if (_sequence[seqIndex].key != combo.sequence[i])
                {
                    return false;
                }

                if (combo.maxTimeFrames > 0)
                {
                    int lastIndex = _sequence.Count - 1;
                    if (_sequence[lastIndex].time - _sequence[seqIndex].time > combo.maxTimeFrames)
                    {
                        return false;
                    }
                }
            }

            return true;
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
