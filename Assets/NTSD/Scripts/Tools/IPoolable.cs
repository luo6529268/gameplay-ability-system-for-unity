
namespace NTSD.Tools
{
    public interface IPoolable
    {
        /// <summary> 当对象从池中取出时调用（相当于 Awake/OnEnable） </summary>
        void OnSpawned();

        /// <summary> 当对象被回收到池中时调用（相当于 OnDisable/Clear） </summary>
        void OnRecycled();
    }
}
