#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using NTSD.LevelEditor;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class BattleMapBoundaryRuntimeSourceEditorTests
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
        public void LoadedBoundaryDefinition_PreservesExistingManagerQueryResults()
        {
            BoundaryData leftBoundary = CreateRectangleBoundary("Left", -10f, -4f, -2f, 4f);
            BoundaryData rightBoundary = CreateRectangleBoundary("Right", 2f, -2f, 8f, 6f);
            BoundaryWall sourceLeft = CreateSourceWall(leftBoundary);
            BoundaryWall sourceRight = CreateSourceWall(rightBoundary);
            BoundaryWallManager manager = CreateManager(sourceLeft, sourceRight);
            BattleMapBoundaryDefinition definition = CreateDefinition(
                "desert_01",
                CreateMapRectangleBoundary(-10f, -4f, -2f, 4f),
                CreateMapRectangleBoundary(2f, -2f, 8f, 6f));

            Vector2[] points =
            {
                new Vector2(-6f, 0f),
                new Vector2(0f, 0f),
                new Vector2(-2f, 4f),
                new Vector2(6f, 2f),
            };
            Rect[] rectangles =
            {
                Rect.MinMaxRect(-9f, -3f, -7f, -1f),
                Rect.MinMaxRect(-3f, -1f, 3f, 1f),
                Rect.MinMaxRect(3f, -1f, 5f, 1f),
            };
            bool[] pointResults = CapturePointResults(manager, points);
            bool[] rectResults = CaptureRectResults(manager, rectangles);
            Assert.That(manager.TryGetBattleStageRuntime(
                out int expectedStageWidth,
                out int expectedZMin,
                out int expectedZMax), Is.True);
            var sourceRng = new DeterministicRng(0x10305070u);
            bool expectedRandomResult = manager.TryGetRandomWalkablePoint(
                sourceRng,
                out Vector2 expectedRandomPoint,
                0.25f,
                256);

            Assert.That(manager.TryLoadBoundaryDefinition(definition, out string failure), Is.True, failure);
            manager.RefreshBoundaries();

            Assert.That(manager.UsesLoadedBoundaryDefinition, Is.True);
            Assert.That(manager.LoadedBoundaryDefinition, Is.SameAs(definition));
            Assert.That(manager.AllBoundaries.Count, Is.EqualTo(2));
            Assert.That(CapturePointResults(manager, points), Is.EqualTo(pointResults));
            Assert.That(CaptureRectResults(manager, rectangles), Is.EqualTo(rectResults));
            Assert.That(manager.TryGetBattleStageRuntime(
                out int actualStageWidth,
                out int actualZMin,
                out int actualZMax), Is.True);
            Assert.That(actualStageWidth, Is.EqualTo(expectedStageWidth));
            Assert.That(actualZMin, Is.EqualTo(expectedZMin));
            Assert.That(actualZMax, Is.EqualTo(expectedZMax));

            var loadedRng = new DeterministicRng(0x10305070u);
            bool actualRandomResult = manager.TryGetRandomWalkablePoint(
                loadedRng,
                out Vector2 actualRandomPoint,
                0.25f,
                256);
            Assert.That(actualRandomResult, Is.EqualTo(expectedRandomResult));
            Assert.That(actualRandomPoint, Is.EqualTo(expectedRandomPoint));
            Assert.That(loadedRng.CallCount, Is.EqualTo(sourceRng.CallCount));
            Assert.That(
                definition.Boundaries[0].Polygons[0].VerticesWorld[0].x,
                Is.EqualTo(-10f));
            Assert.That(sourceLeft.ContainsPointWorld(new Vector2(-6f, 0f)), Is.True);
        }

        [Test]
        public void FailedLoad_LeavesTheCurrentAssetSourceUntouched()
        {
            BattleMapBoundaryDefinition activeDefinition = CreateDefinition(
                "desert_01",
                CreateMapRectangleBoundary(20f, 20f, 28f, 28f));
            BattleMapBoundaryDefinition invalidDefinition = CreateDefinition(
                "broken_01",
                CreateMapBoundary(
                    new Vector2Data { x = 40f, y = 40f },
                    new Vector2Data { x = 44f, y = 40f }));
            BoundaryWallManager manager = CreateManager();

            Assert.That(manager.TryLoadBoundaryDefinition(activeDefinition, out string activeFailure), Is.True, activeFailure);
            bool activePointResult = manager.IsPointWalkable(new Vector2(24f, 24f));
            int activeBoundaryCount = manager.AllBoundaries.Count;
            float originalInvalidVertexX =
                invalidDefinition.Boundaries[0].Polygons[0].VerticesWorld[0].x;

            Assert.That(manager.TryLoadBoundaryDefinition(invalidDefinition, out _), Is.False);

            Assert.That(manager.UsesLoadedBoundaryDefinition, Is.True);
            Assert.That(manager.LoadedBoundaryDefinition, Is.SameAs(activeDefinition));
            Assert.That(manager.AllBoundaries.Count, Is.EqualTo(activeBoundaryCount));
            Assert.That(manager.IsPointWalkable(new Vector2(24f, 24f)), Is.EqualTo(activePointResult));
            Assert.That(
                invalidDefinition.Boundaries[0].Polygons[0].VerticesWorld[0].x,
                Is.EqualTo(originalInvalidVertexX));
        }

        [Test]
        public void ClearLoadedBoundaryDefinition_RestoresExistingSceneFallbackOnlyWhenRequested()
        {
            BoundaryData sceneBoundary = CreateRectangleBoundary("Scene", -120f, -120f, -100f, -100f);
            BoundaryWall sceneWall = CreateSourceWall(sceneBoundary);
            BoundaryWallManager manager = CreateManager(sceneWall);
            BattleMapBoundaryDefinition definition = CreateDefinition(
                "asset_01",
                CreateMapRectangleBoundary(100f, 100f, 120f, 120f));

            Assert.That(manager.TryLoadBoundaryDefinition(definition, out string failure), Is.True, failure);
            Assert.That(manager.IsPointWalkable(new Vector2(-110f, -110f)), Is.False);
            Assert.That(ContainsBoundary(manager.AllBoundaries, sceneWall), Is.False);

            manager.ClearLoadedBoundaryDefinition();

            Assert.That(manager.UsesLoadedBoundaryDefinition, Is.False);
            Assert.That(manager.LoadedBoundaryDefinition, Is.Null);
            Assert.That(ContainsBoundary(manager.AllBoundaries, sceneWall), Is.True);
            Assert.That(manager.IsPointWalkable(new Vector2(-110f, -110f)), Is.True);
        }

        private BoundaryWallManager CreateManager(params BoundaryWall[] boundaries)
        {
            GameObject managerObject = Track(new GameObject("MAPCFG-002 Manager"));
            BoundaryWallManager manager = managerObject.AddComponent<BoundaryWallManager>();
            List<BoundaryWall> managerBoundaries = GetPrivateField<List<BoundaryWall>>(
                manager,
                "_boundaries");
            managerBoundaries.Clear();
            managerBoundaries.AddRange(boundaries);
            SetPrivateField(manager, "_initialized", true);
            return manager;
        }

        private BoundaryWall CreateSourceWall(BoundaryData boundaryData)
        {
            GameObject wallObject = Track(new GameObject("MAPCFG-002 Source " + boundaryData.boundaryName));
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
            return CreateMapBoundary(
                new Vector2Data { x = minX, y = minY },
                new Vector2Data { x = maxX, y = minY },
                new Vector2Data { x = maxX, y = maxY },
                new Vector2Data { x = minX, y = maxY });
        }

        private static BattleMapBoundaryDefinition.MapBoundaryData CreateMapBoundary(
            params Vector2Data[] vertices)
        {
            return new BattleMapBoundaryDefinition.MapBoundaryData(
                new List<BattleMapBoundaryDefinition.MapPolygonData>
                {
                    new BattleMapBoundaryDefinition.MapPolygonData(
                        new List<Vector2Data>(vertices)),
                });
        }

        private static BoundaryData CreateRectangleBoundary(
            string name,
            float minX,
            float minY,
            float maxX,
            float maxY)
        {
            return CreateBoundary(
                name,
                new Vector2Data { x = minX, y = minY },
                new Vector2Data { x = maxX, y = minY },
                new Vector2Data { x = maxX, y = maxY },
                new Vector2Data { x = minX, y = maxY });
        }

        private static BoundaryData CreateBoundary(string name, params Vector2Data[] vertices)
        {
            return new BoundaryData
            {
                boundaryName = name,
                polygons = new List<PolygonData>
                {
                    new PolygonData
                    {
                        name = name + " Polygon",
                        verticesWorld = new List<Vector2Data>(vertices),
                    },
                },
            };
        }

        private static bool[] CapturePointResults(
            BoundaryWallManager manager,
            IReadOnlyList<Vector2> points)
        {
            var results = new bool[points.Count];
            for (int index = 0; index < points.Count; index++)
                results[index] = manager.IsPointWalkable(points[index]);

            return results;
        }

        private static bool[] CaptureRectResults(
            BoundaryWallManager manager,
            IReadOnlyList<Rect> rectangles)
        {
            var results = new bool[rectangles.Count];
            for (int index = 0; index < rectangles.Count; index++)
                results[index] = manager.IsRectWalkable(rectangles[index]);

            return results;
        }

        private static bool ContainsBoundary(
            IReadOnlyList<BoundaryWall> boundaries,
            BoundaryWall expectedBoundary)
        {
            for (int index = 0; index < boundaries.Count; index++)
            {
                if (boundaries[index] == expectedBoundary)
                    return true;
            }

            return false;
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
            Assert.That(field, Is.Not.Null, "Missing serialized field: " + fieldName);
            return field.GetValue(target);
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
