#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using NTSD.LevelEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class BattleMapBoundaryAuthoringEditorTests
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
        public void ExplicitLoadAndApply_RoundTripsWorldDataWithoutSharingMutableVertices()
        {
            BattleMapBoundaryDefinition.MapBoundaryData assetAlpha =
                CreateMapRectangleBoundary(-8f, -4f, -2f, 4f);
            BattleMapBoundaryDefinition.MapBoundaryData assetBeta =
                CreateMapRectangleBoundary(3f, -3f, 9f, 5f);
            BattleMapBoundaryDefinition definition = CreateDefinition(
                "desert_01",
                assetAlpha,
                assetBeta);
            BoundaryWall sceneAlpha = CreateWall(
                CreateRectangleBoundary("Alpha", -30f, -30f, -20f, -20f),
                new Vector3(11f, -7f, 0f));
            BoundaryWall sceneBeta = CreateWall(
                CreateRectangleBoundary("Beta", 30f, 30f, 40f, 40f),
                new Vector3(-6f, 5f, 0f));
            BoundaryWallManager manager = CreateManager(definition, sceneAlpha, sceneBeta);

            Assert.That(manager.TryLoadAuthoringBoundaryDefinitionIntoScene(out string loadFailure), Is.True, loadFailure);
            Assert.That(sceneAlpha.TryCaptureWorldBoundaryData(out BoundaryData loadedAlpha, out string alphaFailure), Is.True, alphaFailure);
            Assert.That(sceneBeta.TryCaptureWorldBoundaryData(out BoundaryData loadedBeta, out string betaFailure), Is.True, betaFailure);
            AssertBoundaryEqual(assetAlpha, loadedAlpha);
            AssertBoundaryEqual(assetBeta, loadedBeta);

            Vector3 movedWorldVertex = sceneAlpha.GetWorldVertex(0, 0) + new Vector3(1.5f, -0.5f, 0f);
            sceneAlpha.SetWorldVertex(0, 0, movedWorldVertex);
            Assert.That(
                definition.Boundaries[0].Polygons[0].VerticesWorld[0].x,
                Is.EqualTo(-8f));

            Assert.That(manager.TryApplySceneBoundariesToAuthoringDefinition(out string applyFailure), Is.True, applyFailure);
            float appliedAssetX = definition.Boundaries[0].Polygons[0].VerticesWorld[0].x;
            float appliedAssetY = definition.Boundaries[0].Polygons[0].VerticesWorld[0].y;
            Assert.That(appliedAssetX, Is.EqualTo(movedWorldVertex.x));
            Assert.That(appliedAssetY, Is.EqualTo(movedWorldVertex.y));

            sceneAlpha.SetWorldVertex(0, 0, movedWorldVertex + new Vector3(2f, 2f, 0f));
            Assert.That(
                definition.Boundaries[0].Polygons[0].VerticesWorld[0].x,
                Is.EqualTo(appliedAssetX));
            Assert.That(
                definition.Boundaries[0].Polygons[0].VerticesWorld[0].y,
                Is.EqualTo(appliedAssetY));
        }

        [Test]
        public void ExplicitLoad_UsesStableBoundaryOrderWithoutNameMatching()
        {
            BattleMapBoundaryDefinition.MapBoundaryData assetBoundary =
                CreateMapRectangleBoundary(1f, 1f, 5f, 5f);
            BattleMapBoundaryDefinition definition = CreateDefinition("desert_01", assetBoundary);
            BoundaryWall sceneBoundary = CreateWall(
                CreateRectangleBoundary("Actual", -5f, -5f, -1f, -1f),
                Vector3.zero);
            BoundaryWallManager manager = CreateManager(definition, sceneBoundary);

            Assert.That(manager.TryLoadAuthoringBoundaryDefinitionIntoScene(out string failure), Is.True, failure);
            Assert.That(sceneBoundary.GetWorldVertex(0, 0), Is.EqualTo(new Vector3(1f, 1f, 0f)));
        }

        [Test]
        public void AuthoringOperations_FailClosedWhileRuntimeAssetSourceIsActive()
        {
            BattleMapBoundaryDefinition authoringDefinition = CreateDefinition(
                "authoring_01",
                CreateMapRectangleBoundary(-20f, -20f, -10f, -10f));
            BattleMapBoundaryDefinition runtimeDefinition = CreateDefinition(
                "runtime_01",
                CreateMapRectangleBoundary(20f, 20f, 30f, 30f));
            BoundaryWall sceneBoundary = CreateWall(
                CreateRectangleBoundary("Authoring", -30f, -30f, -25f, -25f),
                Vector3.zero);
            BoundaryWallManager manager = CreateManager(authoringDefinition, sceneBoundary);
            float authoringVertex = authoringDefinition.Boundaries[0].Polygons[0].VerticesWorld[0].x;

            Assert.That(manager.TryLoadBoundaryDefinition(runtimeDefinition, out string runtimeFailure), Is.True, runtimeFailure);
            Assert.That(manager.TryLoadAuthoringBoundaryDefinitionIntoScene(out _), Is.False);
            Assert.That(manager.TryApplySceneBoundariesToAuthoringDefinition(out _), Is.False);

            Assert.That(manager.UsesLoadedBoundaryDefinition, Is.True);
            Assert.That(manager.LoadedBoundaryDefinition, Is.SameAs(runtimeDefinition));
            Assert.That(
                authoringDefinition.Boundaries[0].Polygons[0].VerticesWorld[0].x,
                Is.EqualTo(authoringVertex));
            Assert.That(sceneBoundary.GetWorldVertex(0, 0).x, Is.EqualTo(-30f));
        }

        private BoundaryWallManager CreateManager(
            BattleMapBoundaryDefinition authoringDefinition,
            params BoundaryWall[] boundaries)
        {
            GameObject managerObject = Track(new GameObject("MAPCFG-003 Manager"));
            BoundaryWallManager manager = managerObject.AddComponent<BoundaryWallManager>();
            List<BoundaryWall> managerBoundaries = GetPrivateField<List<BoundaryWall>>(
                manager,
                "_boundaries");
            managerBoundaries.Clear();
            managerBoundaries.AddRange(boundaries);
            SetPrivateField(manager, "_initialized", true);
            SetPrivateField(manager, "_authoringBoundaryDefinition", authoringDefinition);
            return manager;
        }

        private BoundaryWall CreateWall(BoundaryData boundaryData, Vector3 position)
        {
            GameObject wallObject = Track(new GameObject("MAPCFG-003 " + boundaryData.boundaryName));
            wallObject.transform.position = position;
            BoundaryWall wall = wallObject.AddComponent<BoundaryWall>();
            Assert.That(wall.TryApplyWorldBoundaryData(boundaryData, out string failure), Is.True, failure);
            return wall;
        }

        private BattleMapBoundaryDefinition CreateDefinition(
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

        private static BattleMapBoundaryDefinition.MapBoundaryData CreateMapRectangleBoundary(
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

        private static BoundaryData CreateRectangleBoundary(
            string name,
            float minX,
            float minY,
            float maxX,
            float maxY)
        {
            return new BoundaryData
            {
                boundaryName = name,
                polygons = new List<PolygonData>
                {
                    new PolygonData
                    {
                        name = name + " Polygon",
                        verticesWorld = new List<Vector2Data>
                        {
                            new Vector2Data { x = minX, y = minY },
                            new Vector2Data { x = maxX, y = minY },
                            new Vector2Data { x = maxX, y = maxY },
                            new Vector2Data { x = minX, y = maxY },
                        },
                    },
                },
            };
        }

        private static void AssertBoundaryEqual(
            BattleMapBoundaryDefinition.MapBoundaryData expected,
            BoundaryData actual)
        {
            Assert.That(actual.polygons.Count, Is.EqualTo(expected.Polygons.Count));
            for (int polygonIndex = 0; polygonIndex < expected.Polygons.Count; polygonIndex++)
            {
                BattleMapBoundaryDefinition.MapPolygonData expectedPolygon =
                    expected.Polygons[polygonIndex];
                PolygonData actualPolygon = actual.polygons[polygonIndex];
                Assert.That(
                    actualPolygon.verticesWorld.Count,
                    Is.EqualTo(expectedPolygon.VerticesWorld.Count));
                for (int vertexIndex = 0; vertexIndex < expectedPolygon.VerticesWorld.Count; vertexIndex++)
                {
                    Assert.That(
                        actualPolygon.verticesWorld[vertexIndex].x,
                        Is.EqualTo(expectedPolygon.VerticesWorld[vertexIndex].x));
                    Assert.That(
                        actualPolygon.verticesWorld[vertexIndex].y,
                        Is.EqualTo(expectedPolygon.VerticesWorld[vertexIndex].y));
                }
            }
        }

        private T Track<T>(T target)
            where T : Object
        {
            createdObjects.Add(target);
            return target;
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            return (T)GetPrivateField(target, fieldName);
        }

        private static object GetPrivateField(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Missing private field: " + fieldName);
            return field.GetValue(target);
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
