using System.Collections.Generic;
using NTSD.Simulation;
using UnityEngine.Pool;

namespace NTSD.Animation
{
    /// <summary>
    /// 记录正式战斗流程中的 arest 和按攻击者区分的 vrest 冷却。
    /// 冷却递减由 SimulationWorld.VrestTickAll() 在每个 tick 开头统一驱动。
    /// </summary>
    public sealed class LF2ItrRestTracker
    {
        public sealed class StateSnapshot
        {
            internal int Arest;
            internal Dictionary<int, int> VrestByAttacker;
        }

        private int _arest = 0;
        private readonly Dictionary<int, int> _vrestByAttacker = new Dictionary<int, int>();
        private RuntimeRestStore _boundStore;
        private RuntimeRestBindingHandle _bindingHandle;
        private int preserveStateAcrossOwnerResetDepth;

        public bool IsBound => EnsureActiveBinding();
        public int BoundVictimSlot => EnsureActiveBinding() ? _bindingHandle.BoundVictimSlot : -1;

        internal bool IsBoundTo(RuntimeRestStore store, int victimSlot)
        {
            return store != null &&
                   EnsureActiveBinding() &&
                   ReferenceEquals(_boundStore, store) &&
                   _bindingHandle.BoundVictimSlot == victimSlot;
        }

        /// <summary>
        /// 攻击者自身的命中冷却；大于 0 时不能再次执行 arest 门控的命中。
        /// </summary>
        public int Arest
        {
            get => EnsureActiveBinding()
                ? _boundStore.GetARest(_bindingHandle.BoundVictimSlot)
                : _arest;
            set
            {
                int storedValue = value > 0 ? value : 0;
                if (EnsureActiveBinding())
                    _boundStore.SetARest(_bindingHandle.BoundVictimSlot, storedValue);
                else
                    _arest = storedValue;
            }
        }

        public bool Bind(RuntimeRestStore store, int victimSlot, bool importLocal)
        {
            if (store == null || !store.IsAddressable(victimSlot))
                return false;

            if (_boundStore != null && EnsureActiveBinding())
            {
                return ReferenceEquals(_boundStore, store) &&
                       _bindingHandle.BoundVictimSlot == victimSlot;
            }

            if (!store.TryAcquireBinding(victimSlot, out RuntimeRestBindingHandle handle))
            {
                return false;
            }

            _boundStore = store;
            _bindingHandle = handle;
            if (!importLocal)
                return true;

            if (store.ReplaceVictimState(victimSlot, _arest, _vrestByAttacker))
                return true;

            store.ReleaseBinding(handle);
            ClearBinding();
            return false;
        }

        public bool Unbind(bool captureStoreState)
        {
            if (!EnsureActiveBinding())
                return false;

            RuntimeRestStore store = _boundStore;
            RuntimeRestBindingHandle handle = _bindingHandle;
            if (captureStoreState)
                CaptureBoundStateToLocal();
            bool released = store.ReleaseBinding(handle);
            ClearBinding();
            return released;
        }

        public void Reset()
        {
            if (preserveStateAcrossOwnerResetDepth > 0)
                return;

            if (EnsureActiveBinding())
            {
                _boundStore.SetARest(_bindingHandle.BoundVictimSlot, 0);
                _boundStore.ClearVictimRowOnly(_bindingHandle.BoundVictimSlot);
                return;
            }
            _arest = 0;
            _vrestByAttacker.Clear();
        }

        internal void BeginPreserveStateAcrossOwnerReset()
        {
            preserveStateAcrossOwnerResetDepth++;
        }

        internal void EndPreserveStateAcrossOwnerReset()
        {
            if (preserveStateAcrossOwnerResetDepth > 0)
                preserveStateAcrossOwnerResetDepth--;
        }

        public StateSnapshot CaptureState()
        {
            if (EnsureActiveBinding())
            {
                return new StateSnapshot
                {
                    Arest = _boundStore.GetARest(_bindingHandle.BoundVictimSlot),
                    VrestByAttacker = _boundStore.CaptureVictimRow(_bindingHandle.BoundVictimSlot),
                };
            }

            return new StateSnapshot
            {
                Arest = _arest,
                VrestByAttacker = new Dictionary<int, int>(_vrestByAttacker),
            };
        }

        public void RestoreState(StateSnapshot snapshot)
        {
            if (snapshot == null)
                return;

            int restoredArest = snapshot.Arest > 0 ? snapshot.Arest : 0;
            bool isBound = EnsureActiveBinding();
            if (isBound)
            {
                _boundStore.ReplaceVictimState(
                    _bindingHandle.BoundVictimSlot,
                    restoredArest,
                    snapshot.VrestByAttacker);
                return;
            }

            _arest = restoredArest;
            _vrestByAttacker.Clear();
            if (snapshot.VrestByAttacker != null)
            {
                foreach (KeyValuePair<int, int> pair in snapshot.VrestByAttacker)
                {
                    if (pair.Value > 0)
                        _vrestByAttacker[pair.Key] = pair.Value;
                }
            }
        }

        public bool ArestTest() => Arest <= 0;

        public bool VrestTest(int attackerKey)
        {
            if (EnsureActiveBinding())
                return _boundStore.GetVRest(_bindingHandle.BoundVictimSlot, attackerKey) <= 0;
            return !_vrestByAttacker.TryGetValue(attackerKey, out int v) || v <= 0;
        }

        /// <summary>
        /// 检查指定攻击者是否仍在受击者侧的 vrest 冷却中。
        /// </summary>
        public bool HasVrest(int attackerKey)
        {
            if (EnsureActiveBinding())
                return _boundStore.GetVRest(_bindingHandle.BoundVictimSlot, attackerKey) > 0;
            return _vrestByAttacker.TryGetValue(attackerKey, out int v) && v > 0;
        }

        public int GetVrest(int attackerKey)
        {
            if (EnsureActiveBinding())
                return _boundStore.GetVRest(_bindingHandle.BoundVictimSlot, attackerKey);
            return _vrestByAttacker.TryGetValue(attackerKey, out int value) ? value : 0;
        }

        /// <summary>
        /// 为指定攻击者设置受击者侧 vrest 冷却。
        /// </summary>
        public void SetVrest(int attackerKey, int value)
        {
            if (EnsureActiveBinding())
            {
                _boundStore.SetVRest(_bindingHandle.BoundVictimSlot, attackerKey, value);
                return;
            }

            if (value > 0)
                _vrestByAttacker[attackerKey] = value;
            else
                _vrestByAttacker.Remove(attackerKey);
        }

        public void RemoveVrest(int attackerKey)
        {
            if (EnsureActiveBinding())
            {
                _boundStore.SetVRest(_bindingHandle.BoundVictimSlot, attackerKey, 0);
                return;
            }
            _vrestByAttacker.Remove(attackerKey);
        }

        public void ArestUpdate(InteractionArea itr)
        {
            if (itr != null && itr.arest > 0)
            {
                Arest = itr.arest;
            }
            else if (itr == null || itr.vrest <= 0)
            {
                Arest = NTSDGlobal.Default.Character.ARest;
            }
        }

        public void VrestUpdate(int attackerKey, InteractionArea itr)
        {
            if (itr != null && itr.vrest > 0)
            {
                SetVrest(attackerKey, itr.vrest);
            }
        }

        public void TickArest()
        {
            if (EnsureActiveBinding())
            {
                _boundStore.TickARest(_bindingHandle.BoundVictimSlot);
                return;
            }
            if (_arest > 0) _arest--;
        }

        public void TickVrestForAttacker(int attackerKey)
        {
            if (EnsureActiveBinding())
            {
                _boundStore.TickVRestForAttacker(_bindingHandle.BoundVictimSlot, attackerKey);
                return;
            }

            if (!_vrestByAttacker.TryGetValue(attackerKey, out int value) || value <= 0)
                return;

            if (value == 1)
                _vrestByAttacker.Remove(attackerKey);
            else
                _vrestByAttacker[attackerKey] = value - 1;
        }

        public void Tick()
        {
            if (EnsureActiveBinding())
            {
                _boundStore.TickVictim(_bindingHandle.BoundVictimSlot);
                return;
            }

            TickArest();

            if (_vrestByAttacker.Count == 0) return;

            var keys = ListPool<int>.Get();
            keys.AddRange(_vrestByAttacker.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                int k = keys[i];
                int value = _vrestByAttacker[k];
                if (value <= 1)
                    _vrestByAttacker.Remove(k);
                else
                    _vrestByAttacker[k] = value - 1;
            }
            ListPool<int>.Release(keys);
        }

        private bool EnsureActiveBinding()
        {
            if (_boundStore == null)
                return false;
            if (_boundStore.IsBindingValid(_bindingHandle))
                return true;

            ClearBinding();
            return false;
        }

        private void CaptureBoundStateToLocal()
        {
            int victimSlot = _bindingHandle.BoundVictimSlot;
            _arest = _boundStore.GetARest(victimSlot);
            _vrestByAttacker.Clear();
            Dictionary<int, int> row = _boundStore.CaptureVictimRow(victimSlot);
            foreach (KeyValuePair<int, int> pair in row)
                _vrestByAttacker[pair.Key] = pair.Value;
        }

        private void ClearBinding()
        {
            _boundStore = null;
            _bindingHandle = default;
        }
    }
}
