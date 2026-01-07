using System.Collections.Generic;

namespace BeatEmUpTemplate2D
{

    //state base class
    public abstract class StateNode
    {

        public abstract string animationName { get; }

        public virtual string AbilityName { get => animationName; }

        public List<Transition> Transitions = new List<Transition>(10);
        public virtual bool canGrab => true;
        public float stateStartTime = 0;
        public UnitActions unit;

        public virtual void Update() { }
        public virtual void LateUpdate() { }
        public virtual void FixedUpdate() { }
        public virtual void Enter(){}
        public virtual void Tick() { }
        public virtual void Exit()
        {
            unit.animator.speed = 1;
        }

    }
}