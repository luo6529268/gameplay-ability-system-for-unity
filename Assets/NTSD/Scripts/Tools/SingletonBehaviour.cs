using UnityEngine;

namespace NTSD.Tools
{
    /// <summary>
    /// Simple scene-owned singleton base.
    /// - Does NOT auto-create.
    /// - Duplicate instances are destroyed by default.
    /// - Optional DontDestroyOnLoad for app-level singletons.
    /// </summary>
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        protected virtual bool DestroyDuplicate => true;
        protected virtual bool PersistAcrossScenes => false;

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (DestroyDuplicate)
                {
                    Destroy(gameObject);
                }
                else
                {
                    enabled = false;
                }
                return;
            }

            Instance = this as T;
            if (Instance == null)
            {
                Debug.LogError($"[SingletonBehaviour] {GetType().Name} is not assignable to {typeof(T).Name}");
                enabled = false;
                return;
            }

            if (PersistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }

            OnSingletonAwake();
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            OnSingletonDestroyed();
        }

        protected virtual void OnSingletonAwake() { }
        protected virtual void OnSingletonDestroyed() { }
    }
}
