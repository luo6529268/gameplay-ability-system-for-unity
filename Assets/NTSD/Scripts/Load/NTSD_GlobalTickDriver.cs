using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NTSD.Load
{
    public sealed class NTSD_GlobalTickDriver : MonoBehaviour
    {
        private NTSD_ResourceLoader resourceLoader;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            resourceLoader = NTSD_ResourceLoader.Instance;
        }

        private void Update()
        {
            if (resourceLoader == null)
                resourceLoader = NTSD_ResourceLoader.Instance;

            if (resourceLoader == null || !resourceLoader.HasQueuedTasks)
                return;

            resourceLoader.ProcessFrame().Forget();
        }
    }
}
