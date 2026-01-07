using NTSD.Tools;

namespace BeatEmUpTemplate2D
{
    public class Transition: IPoolable
    {
        public string From;
        public string To;
        public ConditionHelper Condition;

        public Transition()
        {
        }

        public Transition(string from, string to, ConditionHelper condition)
        {
            From = from;
            To = to;
            Condition = condition;
        }

        public void OnInit(string from, string to, ConditionHelper condition) 
        {
            From = from;
            To = to;
            Condition = condition;
        }

        public bool CanTransit() => Condition != null && Condition.Evaluate();

        public void OnSpawned()
        {

        }

        public void OnRecycled()
        {

        }
    }
}