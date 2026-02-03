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
        }

        private async void Update()
        {
            if (resourceLoader == null)
            {
                resourceLoader = NTSD_ResourceLoader.Instance;
            }

            if (resourceLoader == null)
            {
                return;
            }

            await resourceLoader.ProcessFrame();
        }
    }
}
