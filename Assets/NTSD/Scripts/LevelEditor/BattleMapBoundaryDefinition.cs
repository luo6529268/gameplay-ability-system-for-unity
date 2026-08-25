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
        [SerializeField] private string mapId = "";
        [SerializeField] private string displayName = "";
        [SerializeField, Min(0)] private int revision;
        [SerializeField] private List<BoundaryData> boundaries = new List<BoundaryData>();

        public string MapId => mapId;
        public string DisplayName => displayName;
        public int Revision => revision;
        public IReadOnlyList<BoundaryData> Boundaries => boundaries;

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

            if (!TryCloneBoundaryCollection(sourceBoundaries, out List<BoundaryData> copiedBoundaries, out failure))
                return false;

            if (!TryValidateBoundaryCollection(copiedBoundaries, out failure))
                return false;

            boundaries = copiedBoundaries;
            failure = string.Empty;
            return true;
        }
#endif

        private static bool TryValidateBoundaryCollection(
            IReadOnlyList<BoundaryData> sourceBoundaries,
            out string failure)
        {
            if (sourceBoundaries == null || sourceBoundaries.Count == 0)
            {
                failure = "Boundary definition must contain at least one boundary.";
                return false;
            }

            for (int boundaryIndex = 0; boundaryIndex < sourceBoundaries.Count; boundaryIndex++)
            {
                BoundaryData boundary = sourceBoundaries[boundaryIndex];
                if (boundary == null)
                {
                    failure = "Boundary definition contains a null boundary.";
                    return false;
                }

                if (boundary.polygons == null || boundary.polygons.Count == 0)
                {
                    failure = "Boundary definition contains a boundary without polygons.";
                    return false;
                }

                for (int polygonIndex = 0; polygonIndex < boundary.polygons.Count; polygonIndex++)
                {
                    PolygonData polygon = boundary.polygons[polygonIndex];
                    if (polygon == null ||
                        polygon.verticesWorld == null ||
                        polygon.verticesWorld.Count < 3)
                    {
                        failure = "Boundary definition contains a polygon with fewer than three world vertices.";
                        return false;
                    }

                    for (int vertexIndex = 0; vertexIndex < polygon.verticesWorld.Count; vertexIndex++)
                    {
                        Vector2Data vertex = polygon.verticesWorld[vertexIndex];
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
            out List<BoundaryData> copiedBoundaries,
            out string failure)
        {
            copiedBoundaries = null;
            if (sourceBoundaries == null || sourceBoundaries.Count == 0)
            {
                failure = "Boundary authoring source must contain at least one boundary.";
                return false;
            }

            copiedBoundaries = new List<BoundaryData>(sourceBoundaries.Count);
            for (int boundaryIndex = 0; boundaryIndex < sourceBoundaries.Count; boundaryIndex++)
            {
                BoundaryData sourceBoundary = sourceBoundaries[boundaryIndex];
                if (sourceBoundary == null || sourceBoundary.polygons == null)
                {
                    failure = "Boundary authoring source contains an invalid boundary.";
                    return false;
                }

                var copiedBoundary = new BoundaryData
                {
                    boundaryName = sourceBoundary.boundaryName,
                    polygons = new List<PolygonData>(sourceBoundary.polygons.Count),
                };
                for (int polygonIndex = 0; polygonIndex < sourceBoundary.polygons.Count; polygonIndex++)
                {
                    PolygonData sourcePolygon = sourceBoundary.polygons[polygonIndex];
                    if (sourcePolygon == null || sourcePolygon.verticesWorld == null)
                    {
                        failure = "Boundary authoring source contains an invalid polygon.";
                        return false;
                    }

                    var copiedPolygon = new PolygonData
                    {
                        name = sourcePolygon.name,
                        verticesWorld = new List<Vector2Data>(sourcePolygon.verticesWorld.Count),
                    };
                    for (int vertexIndex = 0; vertexIndex < sourcePolygon.verticesWorld.Count; vertexIndex++)
                    {
                        Vector2Data sourceVertex = sourcePolygon.verticesWorld[vertexIndex];
                        if (sourceVertex == null)
                        {
                            failure = "Boundary authoring source contains a null world vertex.";
                            return false;
                        }

                        copiedPolygon.verticesWorld.Add(new Vector2Data
                        {
                            x = sourceVertex.x,
                            y = sourceVertex.y,
                        });
                    }

                    copiedBoundary.polygons.Add(copiedPolygon);
                }

                copiedBoundaries.Add(copiedBoundary);
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
