using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    public class CharacterAnimationEvent : MonoBehaviour
    {
        public UnityAction AnimationEndEvent;

        public void OnAnimationEndEvent()
        {
            AnimationEndEvent?.Invoke();
        }

    }
}
