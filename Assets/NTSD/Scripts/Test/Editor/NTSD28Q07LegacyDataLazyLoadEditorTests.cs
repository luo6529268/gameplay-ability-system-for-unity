#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Animation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class NTSD28Q07LegacyDataLazyLoadEditorTests
    {
        [Test]
        public void SingletonAwake_DefersLegacyIndexUntilExplicitLoad()
        {
            var gameObject = new GameObject("Q07 Legacy Data Lazy Load")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            try
            {
                var manager = gameObject.AddComponent<GameDataManager>();
                Assert.That(manager.IsLoaded(), Is.False,
                    "Singleton initialization must not read the pre-migration data.txt.");

                manager.LoadDataFile("Assets/NTSD/Config/data.txt");
                Assert.That(manager.IsLoaded(), Is.True,
                    "The empty-root legacy caller must retain explicit data.txt loading.");
                Assert.That(manager.GetAllObjects().Count, Is.GreaterThan(0));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
#endif
