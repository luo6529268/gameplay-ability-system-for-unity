#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using NTSD.App;
using NTSD.LevelEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class BattleMapStartupConfigurationEditorTests
    {
        private readonly List<Object> createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int index = createdObjects.Count - 1; index >= 0; index--)
            {
                if (createdObjects[index] != null)
                    Object.DestroyImmediate(createdObjects[index]);
            }

            createdObjects.Clear();
        }

        [Test]
        public void EmptyMapConfiguration_PreservesLegacyBoundaryAndBackgroundState()
        {
            BoundaryWallManager boundaryManager = CreateBoundaryManager();
            Sprite originalSprite = CreateSprite(Color.red);
            SpriteRenderer backgroundRenderer = CreateBackgroundRenderer(originalSprite);
            BattleBootstrap bootstrap = CreateBootstrap(
                null,
                string.Empty,
                boundaryManager,
                backgroundRenderer);

            Assert.That(bootstrap.TryPrepareMapConfiguration(out string failure), Is.True, failure);
            Assert.That(bootstrap.IsMapConfigurationPrepared, Is.False);
            Assert.That(boundaryManager.UsesLoadedBoundaryDefinition, Is.False);
            Assert.That(backgroundRenderer.sprite, Is.SameAs(originalSprite));
        }

        [Test]
        public void ValidMapConfiguration_LoadsBoundaryAndBackgroundThenRestoresOnClear()
        {
            BoundaryWallManager boundaryManager = CreateBoundaryManager();
            Sprite originalSprite = CreateSprite(Color.red);
            Sprite mapSprite = CreateSprite(Color.green);
            SpriteRenderer backgroundRenderer = CreateBackgroundRenderer(originalSprite);
            BattleMapBoundaryDefinition boundaryDefinition = CreateBoundaryDefinition(
                "desert_01",
                CreateRectangleBoundary(-8f, -4f, 8f, 4f));
            SetPrivateField(boundaryDefinition, "backgroundSprite", mapSprite);
            BattleMapCatalog catalog = CreateCatalog(
                CreateCatalogEntry("desert_01", boundaryDefinition));
            BattleBootstrap bootstrap = CreateBootstrap(
                catalog,
                "desert_01",
                boundaryManager,
                backgroundRenderer);

            Assert.That(bootstrap.TryPrepareMapConfiguration(out string failure), Is.True, failure);
            Assert.That(bootstrap.IsMapConfigurationPrepared, Is.True);
            Assert.That(bootstrap.PreparedMapId, Is.EqualTo("desert_01"));
            Assert.That(boundaryManager.UsesLoadedBoundaryDefinition, Is.True);
            Assert.That(boundaryManager.LoadedBoundaryDefinition, Is.SameAs(boundaryDefinition));
            Assert.That(backgroundRenderer.sprite, Is.SameAs(mapSprite));

            bootstrap.ClearPreparedMapConfiguration();

            Assert.That(bootstrap.IsMapConfigurationPrepared, Is.False);
            Assert.That(boundaryManager.UsesLoadedBoundaryDefinition, Is.False);
            Assert.That(backgroundRenderer.sprite, Is.SameAs(originalSprite));
        }

        [Test]
        public void InvalidConfiguredMap_FailsBeforeBoundaryOrBackgroundMutation()
        {
            BoundaryWallManager boundaryManager = CreateBoundaryManager();
            Sprite originalSprite = CreateSprite(Color.red);
            SpriteRenderer backgroundRenderer = CreateBackgroundRenderer(originalSprite);
            BattleMapBoundaryDefinition boundaryDefinition = CreateBoundaryDefinition(
                "desert_01",
                CreateRectangleBoundary(-8f, -4f, 8f, 4f));
            BattleMapCatalog catalog = CreateCatalog(
                CreateCatalogEntry("desert_01", boundaryDefinition));
            BattleBootstrap bootstrap = CreateBootstrap(
                catalog,
                "missing_01",
                boundaryManager,
                backgroundRenderer);

            Assert.That(bootstrap.TryPrepareMapConfiguration(out _), Is.False);
            Assert.That(bootstrap.IsMapConfigurationPrepared, Is.False);
            Assert.That(boundaryManager.UsesLoadedBoundaryDefinition, Is.False);
            Assert.That(backgroundRenderer.sprite, Is.SameAs(originalSprite));
        }

        [Test]
        public void PartialMapConfiguration_FailsBeforeBoundaryOrBackgroundMutation()
        {
            BoundaryWallManager boundaryManager = CreateBoundaryManager();
            Sprite originalSprite = CreateSprite(Color.red);
            SpriteRenderer backgroundRenderer = CreateBackgroundRenderer(originalSprite);
            BattleMapBoundaryDefinition boundaryDefinition = CreateBoundaryDefinition(
                "desert_01",
                CreateRectangleBoundary(-8f, -4f, 8f, 4f));
            BattleMapCatalog catalog = CreateCatalog(
                CreateCatalogEntry("desert_01", boundaryDefinition));
            BattleBootstrap missingCatalogBootstrap = CreateBootstrap(
                null,
                "desert_01",
                boundaryManager,
                backgroundRenderer);
            BattleBootstrap missingMapIdBootstrap = CreateBootstrap(
                catalog,
                string.Empty,
                boundaryManager,
                backgroundRenderer);

            Assert.That(missingCatalogBootstrap.TryPrepareMapConfiguration(out _), Is.False);
            Assert.That(missingMapIdBootstrap.TryPrepareMapConfiguration(out _), Is.False);
            Assert.That(boundaryManager.UsesLoadedBoundaryDefinition, Is.False);
            Assert.That(backgroundRenderer.sprite, Is.SameAs(originalSprite));
        }

        private BoundaryWallManager CreateBoundaryManager()
        {
            GameObject managerObject = Track(new GameObject("MAPCFG-004 Boundary Manager"));
            return managerObject.AddComponent<BoundaryWallManager>();
        }

        private BattleBootstrap CreateBootstrap(
            BattleMapCatalog catalog,
            string mapId,
            BoundaryWallManager boundaryManager,
            SpriteRenderer backgroundRenderer)
        {
            GameObject bootstrapObject = Track(new GameObject("MAPCFG-004 Bootstrap"));
            BattleBootstrap bootstrap = bootstrapObject.AddComponent<BattleBootstrap>();
            SetPrivateField(bootstrap, "mapCatalog", catalog);
            SetPrivateField(bootstrap, "mapId", mapId);
            SetPrivateField(bootstrap, "boundaryManager", boundaryManager);
            SetPrivateField(bootstrap, "backgroundRenderer", backgroundRenderer);
            return bootstrap;
        }

        private SpriteRenderer CreateBackgroundRenderer(Sprite sprite)
        {
            GameObject backgroundObject = Track(new GameObject("MAPCFG-004 Background"));
            SpriteRenderer renderer = backgroundObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            return renderer;
        }

        private Sprite CreateSprite(Color color)
        {
            Texture2D texture = Track(new Texture2D(2, 2));
            texture.SetPixels(new[] { color, color, color, color });
            texture.Apply();
            return Track(Sprite.Create(
                texture,
                new Rect(0f, 0f, 2f, 2f),
                new Vector2(0.5f, 0.5f)));
        }

        private BattleMapBoundaryDefinition CreateBoundaryDefinition(
            string mapId,
            params BattleMapBoundaryDefinition.MapBoundaryData[] boundaries)
        {
            BattleMapBoundaryDefinition definition =
                Track(ScriptableObject.CreateInstance<BattleMapBoundaryDefinition>());
            SetPrivateField(definition, "mapId", mapId);
            SetPrivateField(definition, "displayName", mapId + " display");
            SetPrivateField(definition, "revision", 1);
            SetPrivateField(
                definition,
                "boundaries",
                new List<BattleMapBoundaryDefinition.MapBoundaryData>(boundaries));
            return definition;
        }

        private BattleMapCatalog CreateCatalog(params BattleMapCatalog.Entry[] entries)
        {
            BattleMapCatalog catalog = Track(ScriptableObject.CreateInstance<BattleMapCatalog>());
            SetPrivateField(catalog, "entries", new List<BattleMapCatalog.Entry>(entries));
            return catalog;
        }

        private static BattleMapCatalog.Entry CreateCatalogEntry(
            string mapId,
            BattleMapBoundaryDefinition boundaryDefinition)
        {
            var entry = new BattleMapCatalog.Entry();
            SetPrivateField(entry, "mapId", mapId);
            SetPrivateField(entry, "boundaryDefinition", boundaryDefinition);
            return entry;
        }

        private static BattleMapBoundaryDefinition.MapBoundaryData CreateRectangleBoundary(
            float minX,
            float minY,
            float maxX,
            float maxY)
        {
            return new BattleMapBoundaryDefinition.MapBoundaryData(
                new List<BattleMapBoundaryDefinition.MapPolygonData>
                {
                    new BattleMapBoundaryDefinition.MapPolygonData(
                        new List<Vector2Data>
                        {
                            new Vector2Data { x = minX, y = minY },
                            new Vector2Data { x = maxX, y = minY },
                            new Vector2Data { x = maxX, y = maxY },
                            new Vector2Data { x = minX, y = maxY },
                        }),
                });
        }

        private T Track<T>(T target)
            where T : Object
        {
            createdObjects.Add(target);
            return target;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Missing private field: " + fieldName);
            field.SetValue(target, value);
        }
    }
}
#endif
