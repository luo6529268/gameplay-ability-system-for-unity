using System;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.LevelEditor
{
    [CreateAssetMenu(
        fileName = "BattleMapBoundary",
        menuName = "NTSD/Maps/Boundary Definition")]
    public sealed class BattleMapBoundaryDefinition : ScriptableObject
    {
        [Serializable]
        public sealed class MapBoundaryData
        {
            [SerializeField] private List<MapPolygonData> polygons = new List<MapPolygonData>();

            public IReadOnlyList<MapPolygonData> Polygons => polygons;

            public MapBoundaryData()
            {
            }

            public MapBoundaryData(IReadOnlyList<MapPolygonData> sourcePolygons)
            {
                polygons = sourcePolygons == null
                    ? null
                    : new List<MapPolygonData>(sourcePolygons);
            }
        }

        [Serializable]
        public sealed class MapPolygonData
        {
            [SerializeField] private List<Vector2Data> verticesWorld = new List<Vector2Data>();

            public IReadOnlyList<Vector2Data> VerticesWorld => verticesWorld;

            public MapPolygonData()
            {
            }

            public MapPolygonData(IReadOnlyList<Vector2Data> sourceVerticesWorld)
            {
                verticesWorld = sourceVerticesWorld == null
                    ? null
                    : new List<Vector2Data>(sourceVerticesWorld);
            }
        }

        [SerializeField] private string mapId = "";
        [SerializeField] private string displayName = "";
        [SerializeField, Min(0)] private int revision;
        [SerializeField] private Sprite backgroundSprite;
        [SerializeField] private List<MapBoundaryData> boundaries = new List<MapBoundaryData>();

        public string MapId => mapId;
        public string DisplayName => displayName;
        public int Revision => revision;
        public Sprite BackgroundSprite => backgroundSprite;
        public IReadOnlyList<MapBoundaryData> Boundaries => boundaries;

        public bool TryValidate(out string failure)
        {
            if (!BattleMapDefinitionValidation.TryValidateMapId(mapId, out failure))
                return false;

            if (revision < 0)
            {
                failure = "Boundary definition revision must not be negative.";
                return false;
            }

            return TryValidateBoundaryCollection(boundaries, out failure);
        }

#if UNITY_EDITOR
        public bool TryReplaceBoundariesFromAuthoring(
            IReadOnlyList<BoundaryData> sourceBoundaries,
            out string failure)
        {
            if (!BattleMapDefinitionValidation.TryValidateMapId(mapId, out failure))
                return false;

            if (revision < 0)
            {
                failure = "Boundary definition revision must not be negative.";
                return false;
            }

            if (!TryCloneBoundaryCollection(sourceBoundaries, out List<MapBoundaryData> copiedBoundaries, out failure))
                return false;

            if (!TryValidateBoundaryCollection(copiedBoundaries, out failure))
                return false;

            boundaries = copiedBoundaries;
            failure = string.Empty;
            return true;
        }
#endif

        private static bool TryValidateBoundaryCollection(
            IReadOnlyList<MapBoundaryData> sourceBoundaries,
            out string failure)
        {
            if (sourceBoundaries == null || sourceBoundaries.Count == 0)
            {
                failure = "Boundary definition must contain at least one boundary.";
                return false;
            }

            for (int boundaryIndex = 0; boundaryIndex < sourceBoundaries.Count; boundaryIndex++)
            {
                MapBoundaryData boundary = sourceBoundaries[boundaryIndex];
                if (boundary == null)
                {
                    failure = "Boundary definition contains a null boundary.";
                    return false;
                }

                if (boundary.Polygons == null || boundary.Polygons.Count == 0)
                {
                    failure = "Boundary definition contains a boundary without polygons.";
                    return false;
                }

                for (int polygonIndex = 0; polygonIndex < boundary.Polygons.Count; polygonIndex++)
                {
                    MapPolygonData polygon = boundary.Polygons[polygonIndex];
                    if (polygon == null ||
                        polygon.VerticesWorld == null ||
                        polygon.VerticesWorld.Count < 3)
                    {
                        failure = "Boundary definition contains a polygon with fewer than three world vertices.";
                        return false;
                    }

                    for (int vertexIndex = 0; vertexIndex < polygon.VerticesWorld.Count; vertexIndex++)
                    {
                        Vector2Data vertex = polygon.VerticesWorld[vertexIndex];
                        if (vertex == null ||
                            !BattleMapDefinitionValidation.IsFinite(vertex.x) ||
                            !BattleMapDefinitionValidation.IsFinite(vertex.y))
                        {
                            failure = "Boundary definition contains a non-finite world vertex.";
                            return false;
                        }
                    }
                }
            }

            failure = string.Empty;
            return true;
        }

#if UNITY_EDITOR
        private static bool TryCloneBoundaryCollection(
            IReadOnlyList<BoundaryData> sourceBoundaries,
            out List<MapBoundaryData> copiedBoundaries,
            out string failure)
        {
            copiedBoundaries = null;
            if (sourceBoundaries == null || sourceBoundaries.Count == 0)
            {
                failure = "Boundary authoring source must contain at least one boundary.";
                return false;
            }

            copiedBoundaries = new List<MapBoundaryData>(sourceBoundaries.Count);
            for (int boundaryIndex = 0; boundaryIndex < sourceBoundaries.Count; boundaryIndex++)
            {
                BoundaryData sourceBoundary = sourceBoundaries[boundaryIndex];
                if (sourceBoundary == null || sourceBoundary.polygons == null)
                {
                    failure = "Boundary authoring source contains an invalid boundary.";
                    return false;
                }

                var copiedPolygons = new List<MapPolygonData>(sourceBoundary.polygons.Count);
                for (int polygonIndex = 0; polygonIndex < sourceBoundary.polygons.Count; polygonIndex++)
                {
                    PolygonData sourcePolygon = sourceBoundary.polygons[polygonIndex];
                    if (sourcePolygon == null || sourcePolygon.verticesWorld == null)
                    {
                        failure = "Boundary authoring source contains an invalid polygon.";
                        return false;
                    }

                    var copiedVertices = new List<Vector2Data>(sourcePolygon.verticesWorld.Count);
                    for (int vertexIndex = 0; vertexIndex < sourcePolygon.verticesWorld.Count; vertexIndex++)
                    {
                        Vector2Data sourceVertex = sourcePolygon.verticesWorld[vertexIndex];
                        if (sourceVertex == null)
                        {
                            failure = "Boundary authoring source contains a null world vertex.";
                            return false;
                        }

                        copiedVertices.Add(new Vector2Data
                        {
                            x = sourceVertex.x,
                            y = sourceVertex.y,
                        });
                    }

                    copiedPolygons.Add(new MapPolygonData(copiedVertices));
                }

                copiedBoundaries.Add(new MapBoundaryData(copiedPolygons));
            }

            failure = string.Empty;
            return true;
        }
#endif
    }

    internal static class BattleMapDefinitionValidation
    {
        internal static bool TryValidateMapId(string mapId, out string failure)
        {
            if (string.IsNullOrWhiteSpace(mapId))
            {
                failure = "MapId must not be empty.";
                return false;
            }

            if (!string.Equals(mapId, mapId.Trim(), StringComparison.Ordinal))
            {
                failure = "MapId must not have leading or trailing whitespace.";
                return false;
            }

            failure = string.Empty;
            return true;
        }

        internal static bool MapIdsMatch(string left, string right)
        {
            return string.Equals(left, right, StringComparison.Ordinal);
        }

        internal static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
