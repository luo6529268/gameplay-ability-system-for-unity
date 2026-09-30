#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Reflection;
using MoreMountains.Tools;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.App;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q09LegacyShadowPrewarmEditorTests
    {
        [TestCase("LegacyOnly", true)]
        [TestCase("LegacyOnly", false)]
        [TestCase("CentralShadowBuild", true)]
        [TestCase("CentralOnly", true)]
        public void CreatedPoolObject_HasShadowOnlyWhenLegacyMaterializersRun(
            string backendName, bool usePrefab)
        {
            GameConfig savedConfig = GameConfig.Instance;
            FieldInfo configField = typeof(GameConfig).GetField(
                "_instance", BindingFlags.Static | BindingFlags.NonPublic);
            FieldInfo poolField = typeof(MMSingleton<LF2ObjectPool>).GetField(
                "_instance", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo create = typeof(LF2ObjectPool).GetMethod(
                "CreateNewObject", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo awake = typeof(LF2ObjectPool).GetMethod(
                "Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo shadowField = typeof(LF2ObjectRenderer).GetField(
                "_shadowRenderer", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(configField, Is.Not.Null);
            Assert.That(poolField, Is.Not.Null);
            Assert.That(create, Is.Not.Null);
            Assert.That(awake, Is.Not.Null);
            Assert.That(shadowField, Is.Not.Null);

            LF2ObjectPool savedPool = poolField.GetValue(null) as LF2ObjectPool;
            GameConfig config = null;
            GameObject host = null;
            try
            {
                GameConfig source = AssetDatabase.LoadAssetAtPath<GameConfig>(
                    "Assets/NTSD/Config/GameConfig/GameConfig.asset");
                GameObject entityPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/NTSD/Prefabs/Common/EntityObject.prefab");
                GameObject shadowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/NTSD/Prefabs/Common/Shadow.prefab");
                Assert.That(source, Is.Not.Null);
                Assert.That(entityPrefab, Is.Not.Null);
                Assert.That(shadowPrefab, Is.Not.Null);

                config = Object.Instantiate(source);
                config.BattlePresentationBackendName = backendName;
                config.PoolInitialSize = 0;
                config.LF2ObjectPrefab = usePrefab ? entityPrefab : null;
                config.ShadowPrefab = shadowPrefab;
                configField.SetValue(null, config);
                poolField.SetValue(null, null);

                host = new GameObject("Q09 Legacy Shadow Prewarm Test");
                LF2ObjectPool pool = host.AddComponent<LF2ObjectPool>();
                awake.Invoke(pool, null);
                LF2ObjectRenderer renderer =
                    create.Invoke(pool, null) as LF2ObjectRenderer;
                Assert.That(renderer, Is.Not.Null);

                BattleCentralPresentationMount shadowMount = null;
                foreach (BattleCentralPresentationMount mount in
                         renderer.transform.parent.GetComponentsInChildren<
                             BattleCentralPresentationMount>(true))
                {
                    if (mount.Role == BattleCentralPresentationMountRole.Shadow)
                    {
                        shadowMount = mount;
                        break;
                    }
                }
                Assert.That(shadowMount, Is.Not.Null);
                SpriteRenderer shadow = shadowMount.GetComponent<SpriteRenderer>();
                SpriteRenderer bound = shadowField.GetValue(renderer) as SpriteRenderer;
                if (backendName == "CentralOnly")
                {
                    Assert.That(shadow, Is.Null);
                    Assert.That(bound, Is.Null);
                    return;
                }

                BattleCommonShadowDescriptor descriptor =
                    shadowPrefab.GetComponent<BattleCommonShadowDescriptor>();
                Assert.That(descriptor, Is.Not.Null);
                Assert.That(shadow, Is.Not.Null);
                Assert.That(bound, Is.SameAs(shadow));
                Assert.That(shadow.sprite, Is.SameAs(descriptor.Sprite));
                Assert.That(shadow.sharedMaterial, Is.SameAs(descriptor.Material));
                Assert.That(shadow.color, Is.EqualTo(descriptor.Color));
                Assert.That(shadow.flipX, Is.EqualTo(descriptor.FlipX));
                Assert.That(shadow.flipY, Is.EqualTo(descriptor.FlipY));
                Assert.That(shadow.maskInteraction,
                    Is.EqualTo(descriptor.MaskInteraction));

                GameObject borrowed = pool.Get(out LF2ObjectRenderer firstBorrow);
                Assert.That(firstBorrow, Is.SameAs(renderer));
                Assert.That(borrowed, Is.SameAs(renderer.transform.parent.gameObject));
                pool.Release(firstBorrow);
                Assert.That(shadow.enabled, Is.False);
                Assert.That(shadowField.GetValue(renderer), Is.SameAs(shadow));
                GameObject reused = pool.Get(out LF2ObjectRenderer secondBorrow);
                Assert.That(reused, Is.SameAs(borrowed));
                Assert.That(secondBorrow, Is.SameAs(renderer));
                Assert.That(shadowField.GetValue(secondBorrow), Is.SameAs(shadow));
                Assert.That(shadow.sprite, Is.SameAs(descriptor.Sprite));
                Assert.That(shadow.sharedMaterial, Is.SameAs(descriptor.Material));
                pool.Release(secondBorrow);
                Assert.That(pool.ActiveObjectCountForAcceptance, Is.Zero);
            }
            finally
            {
                if (host != null)
                    Object.DestroyImmediate(host);
                poolField.SetValue(null, savedPool);
                configField.SetValue(null, savedConfig);
                if (config != null)
                    Object.DestroyImmediate(config);
            }
        }
    }
}
#endif
