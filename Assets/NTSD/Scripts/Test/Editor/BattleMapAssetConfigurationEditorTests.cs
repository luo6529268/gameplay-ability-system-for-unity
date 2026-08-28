#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using NTSD.LevelEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class BattleMapAssetConfigurationEditorTests
    {
        private readonly List<Object> createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int index = createdObjects.Count - 1; index >= 0; index--)
            {
                Object.DestroyImmediate(createdObjects[index]);
            }

            createdObjects.Clear();
        }

        [Test]
        public void BoundaryDefinition_PreservesExistingWorldVertexExportShape()
        {
            BattleMapBoundaryDefinition definition =
                CreateBoundaryDefinition("desert_01", CreateValidBoundaryData(3f));

            Assert.That(definition.TryValidate(out string failure), Is.True, failure);
            Assert.That(definition.Boundaries.Count, Is.EqualTo(1));
            Assert.That(definition.Boundaries[0].Polygons.Count, Is.EqualTo(1));
            Assert.That(
                definition.Boundaries[0].Polygons[0].VerticesWorld[0].x,
                Is.EqualTo(3f));
            Assert.That(
                definition.Boundaries[0].Polygons[0].VerticesWorld[2].y,
                Is.EqualTo(6f));
        }

        [Test]
        public void BoundaryAssetGeometry_DoesNotSerializeLegacyNames()
        {
            Assert.That(
                typeof(BattleMapBoundaryDefinition.MapBoundaryData).GetField(
                    "boundaryName",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
                Is.Null);
            Assert.That(
                typeof(BattleMapBoundaryDefinition.MapPolygonData).GetField(
                    "name",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
                Is.Null);
        }

        [Test]
        public void BoundaryDefinition_RejectsMalformedWorldVertices()
        {
            BattleMapBoundaryDefinition tooShort =
                CreateBoundaryDefinition(
                    "desert_01",
                    CreateBoundaryData(
                        new Vector2Data { x = 0f, y = 0f },
                        new Vector2Data { x = 1f, y = 0f }));
            Assert.That(tooShort.TryValidate(out _), Is.False);

            BattleMapBoundaryDefinition nonFinite =
                CreateBoundaryDefinition(
                    "desert_02",
                    CreateBoundaryData(
                        new Vector2Data { x = float.NaN, y = 0f },
                        new Vector2Data { x = 1f, y = 0f },
                        new Vector2Data { x = 1f, y = 1f }));
            Assert.That(nonFinite.TryValidate(out _), Is.False);
        }

        [Test]
        public void Catalog_ResolvesBoundaryAssetWithoutPresentationPairing()
        {
            BattleMapBoundaryDefinition boundary =
                CreateBoundaryDefinition("desert_01", CreateValidBoundaryData(2f));
            BattleMapCatalog catalog =
                CreateCatalog(CreateEntry("desert_01", boundary));

            float originalX = boundary.Boundaries[0].Polygons[0].VerticesWorld[0].x;
            bool resolved = catalog.TryResolve(
                "desert_01",
                out BattleMapCatalog.Entry entry,
                out string failure);

            Assert.That(resolved, Is.True, failure);
            Assert.That(entry.BoundaryDefinition, Is.SameAs(boundary));
            Assert.That(
                boundary.Boundaries[0].Polygons[0].VerticesWorld[0].x,
                Is.EqualTo(originalX));
        }

        [Test]
        public void Catalog_FailsClosedForDuplicateAndMismatchedMapIds()
        {
            BattleMapBoundaryDefinition desertBoundary =
                CreateBoundaryDefinition("desert_01", CreateValidBoundaryData(0f));
            BattleMapBoundaryDefinition forestBoundary =
                CreateBoundaryDefinition("forest_01", CreateValidBoundaryData(10f));

            BattleMapCatalog duplicateCatalog = CreateCatalog(
                CreateEntry("desert_01", desertBoundary),
                CreateEntry("desert_01", forestBoundary));
            Assert.That(duplicateCatalog.TryValidate(out _), Is.False);
            Assert.That(
                duplicateCatalog.TryResolve("desert_01", out _, out _),
                Is.False);

            BattleMapCatalog mismatchCatalog = CreateCatalog(
                CreateEntry("desert_01", forestBoundary));
            Assert.That(mismatchCatalog.TryValidate(out _), Is.False);
            Assert.That(
                mismatchCatalog.TryResolve("desert_01", out _, out _),
                Is.False);
        }

        private BattleMapBoundaryDefinition CreateBoundaryDefinition(
            string mapId,
            List<BattleMapBoundaryDefinition.MapBoundaryData> boundaries)
        {
            BattleMapBoundaryDefinition definition =
                Track(ScriptableObject.CreateInstance<BattleMapBoundaryDefinition>());
            SetPrivateField(definition, "mapId", mapId);
            SetPrivateField(definition, "displayName", mapId + " display");
            SetPrivateField(definition, "revision", 1);
            SetPrivateField(definition, "boundaries", boundaries);
            return definition;
        }

        private BattleMapCatalog CreateCatalog(params BattleMapCatalog.Entry[] entries)
        {
            BattleMapCatalog catalog =
                Track(ScriptableObject.CreateInstance<BattleMapCatalog>());
            SetPrivateField(catalog, "entries", new List<BattleMapCatalog.Entry>(entries));
            return catalog;
        }

        private static BattleMapCatalog.Entry CreateEntry(
            string mapId,
            BattleMapBoundaryDefinition boundary)
        {
            var entry = new BattleMapCatalog.Entry();
            SetPrivateField(entry, "mapId", mapId);
            SetPrivateField(entry, "boundaryDefinition", boundary);
            return entry;
        }

        private static List<BattleMapBoundaryDefinition.MapBoundaryData> CreateValidBoundaryData(float xOffset)
        {
            return CreateBoundaryData(
                new Vector2Data { x = xOffset, y = 0f },
                new Vector2Data { x = xOffset + 4f, y = 0f },
                new Vector2Data { x = xOffset + 4f, y = 6f },
                new Vector2Data { x = xOffset, y = 6f });
        }

        private static List<BattleMapBoundaryDefinition.MapBoundaryData> CreateBoundaryData(
            params Vector2Data[] vertices)
        {
            return new List<BattleMapBoundaryDefinition.MapBoundaryData>
            {
                new BattleMapBoundaryDefinition.MapBoundaryData(
                    new List<BattleMapBoundaryDefinition.MapPolygonData>
                    {
                        new BattleMapBoundaryDefinition.MapPolygonData(
                            new List<Vector2Data>(vertices)),
                    }),
            };
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
            Assert.That(field, Is.Not.Null, "Missing serialized field: " + fieldName);
            field.SetValue(target, value);
        }
    }
}
#endif
