using BeatEmUpTemplate2D;
using GAS.General;
using GAS.Runtime;
using MoreMountains.TopDownEngine;
using UnityEngine;
using UnityEngine.Events;

namespace GAS.Cue
{
    public class CuePlayAnimationOfFightUnit : GameplayCueInstant
    {
        [SerializeField] private string animName;
        public string AnimName => animName;

        public override GameplayCueInstantSpec CreateSpec(GameplayCueParameters parameters)
        {
            return new CuePlayAnimationOfFightUnitSpec(this, parameters);
        }

#if UNITY_EDITOR
        public override void OnEditorPreview(GameObject preview, int frame, int startFrame)
        {
            var unit = preview.GetComponent<UnitActions>();
            if (startFrame <= frame)
            {
                var animatorObject = unit.animator.gameObject;
                var animator = unit.animator;
                var stateMap = animator.GetAllAnimationState();
                if (stateMap.TryGetValue(animName, out var clip))
                {
                    float clipFrameCount = (int)(clip.frameRate * clip.length);
                    if (frame < clipFrameCount + startFrame)
                    {
                        var progress = (frame - startFrame) / clipFrameCount;
                        if (progress > 1 && clip.isLooping) progress -= (int)progress;
                        clip.SampleAnimation(animatorObject.gameObject, progress * clip.length);
                    }
                }
            }
        }
#endif
    }

    public class CuePlayAnimationOfFightUnitSpec : GameplayCueInstantSpec
    {
        private readonly CuePlayAnimationOfFightUnit _cuePlayAnimationOfFightUnit;

        public CuePlayAnimationOfFightUnitSpec(CuePlayAnimationOfFightUnit cue, GameplayCueParameters parameters) :
            base(cue, parameters)
        {
            _cuePlayAnimationOfFightUnit = _cue as CuePlayAnimationOfFightUnit;
        }

        public override void Trigger()
        {
        }
    }
}