using System;
#if UNITY_EDITOR
using GAS.Editor;
#endif
using GAS.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Demo.Script.GAS.AbilityTask
{
    [Serializable]
    public class TeleportToBeamPoint : InstantAbilityTask
    {
        public Vector2 BeamPointLeft;
        public Vector2 BeamPointRight;

        public override void OnExecute()
        {
        }
    }

#if UNITY_EDITOR
    public class TeleportToBeamPointInspector : InstantTaskInspector<TeleportToBeamPoint>
    {
        [Delayed] [LabelText("左侧发射激光位点")] [OnValueChanged("OnBeamPointLeftChanged")]
        public Vector2 BeamPointLeft;

        [Delayed] [LabelText("右侧发射激光位点")] [OnValueChanged("OnBeamPointRightChanged")]
        public Vector2 BeamPointRight;

        public override void Init(InstantAbilityTask task)
        {
            base.Init(task);
            BeamPointLeft = _task.BeamPointLeft;
            BeamPointRight = _task.BeamPointRight;
        }

        void OnBeamPointLeftChanged()
        {
            _task.BeamPointLeft = BeamPointLeft;
            Save();
        }

        void OnBeamPointRightChanged()
        {
            _task.BeamPointRight = BeamPointRight;
            Save();
        }
    }
#endif
}