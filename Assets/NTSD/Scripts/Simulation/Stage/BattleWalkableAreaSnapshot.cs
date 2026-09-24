using System;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>Immutable battle-ground polygon data captured before a worker tick.</summary>
    internal sealed class BattleWalkableAreaSnapshot
    {
        private const double EdgeEpsilonWorld = 0.12;
        private readonly Vector2[][] polygons;
        private readonly double originX;
        private readonly double originY;
        private readonly double unitsPerPixelX;
        private readonly double unitsPerPixelY;

        internal BattleWalkableAreaSnapshot(
            IReadOnlyList<Vector2[]> sourcePolygons,
            Vector2 pixelOriginWorld,
            double unitsPerPixelX,
            double unitsPerPixelY)
        {
            if (sourcePolygons == null || sourcePolygons.Count == 0)
                throw new ArgumentException("At least one polygon is required.", nameof(sourcePolygons));
            if (unitsPerPixelX <= 0 || unitsPerPixelY <= 0)
                throw new ArgumentOutOfRangeException(nameof(unitsPerPixelX));

            polygons = new Vector2[sourcePolygons.Count][];
            for (int index = 0; index < sourcePolygons.Count; index++)
            {
                Vector2[] source = sourcePolygons[index];
                if (source == null || source.Length < 3)
                    throw new ArgumentException("Every polygon needs three vertices.", nameof(sourcePolygons));
                polygons[index] = (Vector2[])source.Clone();
            }

            originX = pixelOriginWorld.x;
            originY = pixelOriginWorld.y;
            this.unitsPerPixelX = unitsPerPixelX;
            this.unitsPerPixelY = unitsPerPixelY;
        }

        internal bool ContainsGroundPixel(double battleX, double battleZ)
        {
            double x = originX + battleX * unitsPerPixelX;
            double y = originY - battleZ * unitsPerPixelY;
            for (int polygonIndex = 0; polygonIndex < polygons.Length; polygonIndex++)
            {
                Vector2[] vertices = polygons[polygonIndex];
                bool inside = false;
                for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
                {
                    Vector2 a = vertices[j];
                    Vector2 b = vertices[i];
                    double abx = b.x - a.x;
                    double aby = b.y - a.y;
                    double apx = x - a.x;
                    double apy = y - a.y;
                    double lengthSq = abx * abx + aby * aby;
                    if (lengthSq > 0)
                    {
                        double cross = abx * apy - aby * apx;
                        double dot = apx * abx + apy * aby;
                        if (Math.Abs(cross) <= EdgeEpsilonWorld * Math.Sqrt(lengthSq) &&
                            dot >= -EdgeEpsilonWorld &&
                            dot <= lengthSq + EdgeEpsilonWorld)
                            return true;
                    }

                    if ((a.y > y) != (b.y > y) &&
                        x < (b.x - a.x) * (y - a.y) / (b.y - a.y) + a.x)
                        inside = !inside;
                }

                if (inside)
                    return true;
            }

            return false;
        }
    }
}
