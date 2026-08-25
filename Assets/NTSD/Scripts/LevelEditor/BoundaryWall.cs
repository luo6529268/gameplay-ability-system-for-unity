using System.Collections.Generic;
using UnityEngine;

namespace NTSD.LevelEditor
{
    /// <summary>
    /// 定义边界多边形类型的枚举
    /// 用于表示游戏中不同类型的多边形边界
    /// </summary>
    public enum BoundaryPolygonType
    {
        /// <summary>
        /// 可行走区域
        /// 角色或单位可以正常移动的区域
        /// </summary>
        Walkable = 0,

        /// <summary>
        /// 硬性阻挡区域
        /// 完全无法通过的区域，如墙壁等
        /// </summary>
        HardBlock = 1,

        /// <summary>
        /// 特殊区域
        /// 具有特殊属性的区域，可能需要特定条件才能通过
        /// </summary>
        Special = 2
    }


    /// <summary>
    /// 可行走区域多边形容器 - 支持多个凹多边形（并集）
    ///
    /// 职责：
    /// - 存储多个多边形（List of BoundaryPolygon）
    /// - 每个多边形可独立编辑（X/Y 平面）
    /// - 提供 Point-in-Polygon 检测（并集语义）
    ///
    /// 坐标系：
    /// - 顶点使用局部坐标 Vector2(x, y)，对应世界坐标的 X/Y 平面
    /// - Z 轴（深度）由 transform.position.z 统一定义
    /// - ⚠️ 项目规范：地面平面统一 X/Y（不是 X/Z）
    /// </summary>
    [ExecuteInEditMode]
    public class BoundaryWall : MonoBehaviour
    {
        // ==================== 序列化字段 ====================

        /// <summary>
        /// 多边形列表（并集语义：Rect 在任意一个多边形内即可行走）
        /// </summary>
        [SerializeField]
        private List<BoundaryPolygon> _polygons = new List<BoundaryPolygon>
        {
            // 默认创建一个矩形多边形
            new BoundaryPolygon
            {
                name = "多边形 1",
                vertices = new List<Vector2>
                {
                    new Vector2(-5f, -5f),
                    new Vector2(5f, -5f),
                    new Vector2(5f, 5f),
                    new Vector2(-5f, 5f)
                },
                color = new Color(0f, 1f, 0f, 0.3f)
            }
        };

        /// <summary>
        /// 当前激活的多边形索引（用于 Editor 选择编辑）
        /// </summary>
        [SerializeField]
        private int _activePolygonIndex = 0;

        /// <summary>
        /// 边界名称
        /// </summary>
        [SerializeField]
        private string _boundaryName = "可行走区域";

        /// <summary>
        /// 是否启用
        /// </summary>
        [SerializeField]
        private bool _isEnabled = true;

        private readonly List<Vector2> _worldVertexBuffer = new List<Vector2>(32);

        // ==================== 公共属性 ====================

        public string BoundaryName
        {
            get => _boundaryName;
            set => _boundaryName = value;
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public int ActivePolygonIndex
        {
            get => _activePolygonIndex;
            set => _activePolygonIndex = Mathf.Clamp(value, 0, _polygons.Count - 1);
        }

        public int PolygonCount => _polygons.Count;

        public IReadOnlyList<BoundaryPolygon> Polygons => _polygons;

        public BoundaryPolygon ActivePolygon
        {
            get
            {
                if (_activePolygonIndex >= 0 && _activePolygonIndex < _polygons.Count)
                    return _polygons[_activePolygonIndex];
                return null;
            }
        }

        // ==================== Editor Only 方法 ====================

#if UNITY_EDITOR
        /// <summary>
        /// 添加新多边形
        /// </summary>
        public void AddPolygon(string name = null)
        {
            var newPolygon = new BoundaryPolygon
            {
                name = name ?? $"多边形 {_polygons.Count + 1}",
                vertices = new List<Vector2>
                {
                    new Vector2(-2f, -2f),
                    new Vector2(2f, -2f),
                    new Vector2(2f, 2f),
                    new Vector2(-2f, 2f)
                },
                color = new Color(Random.value, Random.value, Random.value, 0.3f)
            };
            _polygons.Add(newPolygon);
            _activePolygonIndex = _polygons.Count - 1;
            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 删除多边形
        /// </summary>
        public void DeletePolygon(int index)
        {
            if (index < 0 || index >= _polygons.Count) return;
            if (_polygons.Count <= 1)
            {
                Debug.LogWarning("[BoundaryWall] 无法删除：至少保留一个多边形");
                return;
            }

            _polygons.RemoveAt(index);
            _activePolygonIndex = Mathf.Clamp(_activePolygonIndex, 0, _polygons.Count - 1);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 复制多边形
        /// </summary>
        public void DuplicatePolygon(int index)
        {
            if (index < 0 || index >= _polygons.Count) return;

            var original = _polygons[index];
            var duplicate = new BoundaryPolygon
            {
                name = original.name + " (副本)",
                vertices = new List<Vector2>(original.vertices),
                color = original.color
            };
            _polygons.Add(duplicate);
            _activePolygonIndex = _polygons.Count - 1;
            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 获取世界坐标顶点（X/Y 平面）
        /// </summary>
        public Vector3 GetWorldVertex(int polygonIndex, int vertexIndex)
        {
            if (polygonIndex < 0 || polygonIndex >= _polygons.Count) return transform.position;
            var polygon = _polygons[polygonIndex];
            if (vertexIndex < 0 || vertexIndex >= polygon.vertices.Count) return transform.position;

            Vector2 localVertex = polygon.vertices[vertexIndex];
            // X/Y 平面：local (x,y) -> world (x,y,z)
            return transform.TransformPoint(new Vector3(localVertex.x, localVertex.y, 0f));
        }

        /// <summary>
        /// 设置世界坐标顶点（X/Y 平面）
        /// </summary>
        public void SetWorldVertex(int polygonIndex, int vertexIndex, Vector3 worldPos)
        {
            if (polygonIndex < 0 || polygonIndex >= _polygons.Count) return;
            var polygon = _polygons[polygonIndex];
            if (vertexIndex < 0 || vertexIndex >= polygon.vertices.Count) return;

            // 世界坐标转局部坐标（投影到 Z=0 平面）
            Vector3 localPos = transform.InverseTransformPoint(worldPos);
            polygon.vertices[vertexIndex] = new Vector2(localPos.x, localPos.y);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 插入顶点
        /// </summary>
        public void InsertVertex(int polygonIndex, int afterEdgeIndex)
        {
            if (polygonIndex < 0 || polygonIndex >= _polygons.Count) return;
            var polygon = _polygons[polygonIndex];
            if (afterEdgeIndex < 0 || afterEdgeIndex >= polygon.vertices.Count) return;

            int nextIndex = (afterEdgeIndex + 1) % polygon.vertices.Count;
            Vector2 midPoint = (polygon.vertices[afterEdgeIndex] + polygon.vertices[nextIndex]) * 0.5f;
            polygon.vertices.Insert(nextIndex, midPoint);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 删除顶点
        /// </summary>
        public void RemoveVertex(int polygonIndex, int vertexIndex)
        {
            if (polygonIndex < 0 || polygonIndex >= _polygons.Count) return;
            var polygon = _polygons[polygonIndex];
            if (vertexIndex < 0 || vertexIndex >= polygon.vertices.Count) return;

            if (polygon.vertices.Count <= 3)
            {
                Debug.LogWarning("[BoundaryWall] 无法删除顶点：多边形至少需要 3 个顶点");
                return;
            }

            polygon.vertices.RemoveAt(vertexIndex);
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        // ==================== Runtime API ====================

        /// <summary>
        /// Copies an exported boundary's world X/Y vertices into this wall without
        /// changing the existing polygon query implementation.
        /// </summary>
        public bool TryApplyWorldBoundaryData(BoundaryData boundaryData, out string failure)
        {
            if (boundaryData == null || boundaryData.polygons == null || boundaryData.polygons.Count == 0)
            {
                failure = "Boundary data must contain at least one polygon.";
                return false;
            }

            var copiedPolygons = new List<BoundaryPolygon>(boundaryData.polygons.Count);
            for (int polygonIndex = 0; polygonIndex < boundaryData.polygons.Count; polygonIndex++)
            {
                PolygonData polygonData = boundaryData.polygons[polygonIndex];
                if (polygonData == null ||
                    polygonData.verticesWorld == null ||
                    polygonData.verticesWorld.Count < 3)
                {
                    failure = "Boundary data contains a polygon with fewer than three world vertices.";
                    return false;
                }

                var localVertices = new List<Vector2>(polygonData.verticesWorld.Count);
                for (int vertexIndex = 0; vertexIndex < polygonData.verticesWorld.Count; vertexIndex++)
                {
                    Vector2Data worldVertex = polygonData.verticesWorld[vertexIndex];
                    if (worldVertex == null ||
                        float.IsNaN(worldVertex.x) ||
                        float.IsInfinity(worldVertex.x) ||
                        float.IsNaN(worldVertex.y) ||
                        float.IsInfinity(worldVertex.y))
                    {
                        failure = "Boundary data contains a non-finite world vertex.";
                        return false;
                    }

                    Vector3 localVertex = transform.InverseTransformPoint(
                        new Vector3(worldVertex.x, worldVertex.y, transform.position.z));
                    localVertices.Add(new Vector2(localVertex.x, localVertex.y));
                }

                Color color = _polygons != null &&
                    polygonIndex < _polygons.Count && _polygons[polygonIndex] != null
                    ? _polygons[polygonIndex].color
                    : new Color(0f, 1f, 0f, 0.3f);
                copiedPolygons.Add(new BoundaryPolygon
                {
                    name = polygonData.name ?? string.Empty,
                    vertices = localVertices,
                    color = color,
                });
            }

            _boundaryName = boundaryData.boundaryName ?? string.Empty;
            _polygons = copiedPolygons;
            _activePolygonIndex = 0;
            _isEnabled = true;
            failure = string.Empty;
            return true;
        }

#if UNITY_EDITOR
        public bool TryCaptureWorldBoundaryData(
            out BoundaryData boundaryData,
            out string failure)
        {
            boundaryData = null;
            if (!_isEnabled)
            {
                failure = "Disabled BoundaryWall cannot be captured for authoring.";
                return false;
            }

            if (_polygons == null || _polygons.Count == 0)
            {
                failure = "BoundaryWall must contain at least one polygon.";
                return false;
            }

            var capturedBoundary = new BoundaryData
            {
                boundaryName = _boundaryName,
                polygons = new List<PolygonData>(_polygons.Count),
            };
            for (int polygonIndex = 0; polygonIndex < _polygons.Count; polygonIndex++)
            {
                BoundaryPolygon polygon = _polygons[polygonIndex];
                if (!TryGetWorldVertices(polygon, _worldVertexBuffer))
                {
                    failure = "BoundaryWall contains a polygon with fewer than three vertices.";
                    return false;
                }

                var capturedPolygon = new PolygonData
                {
                    name = polygon.name,
                    verticesWorld = new List<Vector2Data>(_worldVertexBuffer.Count),
                };
                for (int vertexIndex = 0; vertexIndex < _worldVertexBuffer.Count; vertexIndex++)
                {
                    capturedPolygon.verticesWorld.Add(new Vector2Data
                    {
                        x = _worldVertexBuffer[vertexIndex].x,
                        y = _worldVertexBuffer[vertexIndex].y,
                    });
                }

                capturedBoundary.polygons.Add(capturedPolygon);
            }

            boundaryData = capturedBoundary;
            failure = string.Empty;
            return true;
        }
#endif

        /// <summary>
        /// 检测点是否在任意一个多边形内（并集语义）
        /// </summary>
        public bool ContainsPoint(Vector2 worldPoint)
        {
            if (!_isEnabled) return false;

            foreach (var polygon in _polygons)
            {
                if (IsPolygonSimple(polygon) && ContainsPoint(worldPoint, polygon))
                    return true;
            }

            return false;
        }

        private bool ContainsPoint(Vector2 worldPoint, BoundaryPolygon polygon)
        {
            if (polygon == null || polygon.vertices.Count < 3) return false;

            Vector3 localPos = transform.InverseTransformPoint(new Vector3(worldPoint.x, worldPoint.y, transform.position.z));
            Vector2 localPoint = new Vector2(localPos.x, localPos.y);
            IReadOnlyList<Vector2> vertices = polygon.vertices;

            const float edgeEpsilon = 0.12f;
            int vertexCount = vertices.Count;
            for (int i = 0, j = vertexCount - 1; i < vertexCount; j = i++)
            {
                Vector2 vi = vertices[i];
                Vector2 vj = vertices[j];
                if (IsPointOnSegment(localPoint, vj, vi, edgeEpsilon))
                    return true;
            }

            bool inside = false;
            for (int i = 0, j = vertexCount - 1; i < vertexCount; j = i++)
            {
                Vector2 vi = vertices[i];
                Vector2 vj = vertices[j];
                if (((vi.y > localPoint.y) != (vj.y > localPoint.y)) &&
                    (localPoint.x < (vj.x - vi.x) * (localPoint.y - vi.y) / (vj.y - vi.y) + vi.x))
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        public bool TryGetWorldVertices(BoundaryPolygon polygon, List<Vector2> worldVertices)
        {
            if (polygon == null || worldVertices == null || polygon.vertices == null || polygon.vertices.Count < 3)
                return false;

            worldVertices.Clear();
            for (int i = 0; i < polygon.vertices.Count; i++)
            {
                Vector3 world = transform.TransformPoint(new Vector3(polygon.vertices[i].x, polygon.vertices[i].y, 0f));
                worldVertices.Add(new Vector2(world.x, world.y));
            }

            return worldVertices.Count >= 3;
        }

        private static bool IsPointOnSegment(Vector2 p, Vector2 a, Vector2 b, float epsilon)
        {
            float abx = b.x - a.x;
            float aby = b.y - a.y;
            float apx = p.x - a.x;
            float apy = p.y - a.y;

            float abLenSq = abx * abx + aby * aby;
            if (abLenSq <= Mathf.Epsilon)
                return Vector2.SqrMagnitude(p - a) <= epsilon * epsilon;

            float cross = abx * apy - aby * apx;
            if (Mathf.Abs(cross) > epsilon * Mathf.Sqrt(abLenSq))
                return false;

            float dot = apx * abx + apy * aby;
            if (dot < -epsilon || dot > abLenSq + epsilon)
                return false;

            return true;
        }

        /// <summary>
        /// 运行时 API：是否允许该 Rect 处于此 BoundaryWall 内（多层规则）
        /// - HardBlock：只要与 Rect 有交集就禁止
        /// - Walkable：Rect 需完全落入任意 Walkable polygon
        /// - Special：当 allowSpecial=true 时，Rect 可完全落入任意 Special polygon
        /// </summary>
        public bool IsRectAllowed(Rect worldRect)
        {
            if (!_isEnabled) return false;

            // 1) HardBlock 优先：任何重叠都直接禁止（用于左右/底部绝对不可越界区域）
            // Single-layer boundary: only checks whether Rect is fully inside any polygon.

            // 2) Walkable：Rect 完全在任意 Walkable polygon 内即允许
            foreach (var polygon in _polygons)
            {
                if (polygon == null) continue;
                if (!IsPolygonSimple(polygon)) continue;
                if (RectFullyInsidePolygon(worldRect, polygon))
                    return true;
            }

            // 3) Special：仅当 allowSpecial 时允许
            // No Special layer.

            return false;
        }

        /// <summary>
        /// 兼容旧 API：并集语义（只看 Walkable + Special，不含 HardBlock）
        /// </summary>
        public bool ContainsRect(Rect worldRect)
        {
            return IsRectAllowed(worldRect);
        }

        public bool ContainsPointWorld(Vector2 worldPoint)
        {
            if (!_isEnabled) return false;
            foreach (var polygon in _polygons)
            {
                if (polygon == null) continue;
                if (!IsPolygonSimple(polygon)) continue;
                if (ContainsPoint(worldPoint, polygon))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 外部校验：当前多边形是否为简单多边形（无自相交）。
        /// </summary>
        public bool IsPolygonSimple(BoundaryPolygon polygon) => IsSimplePolygon(polygon);

        /// <summary>
        /// 严谨检测：Rect 是否完全在多边形内
        /// 1. Rect 四角都在多边形内
        /// 2. 多边形边不与 Rect 边相交
        /// </summary>
        private bool RectFullyInsidePolygon(Rect worldRect, BoundaryPolygon polygon)
        {
            if (!IsSimplePolygon(polygon)) return false;

            // Rect 四个顶点
            Vector2[] rectCorners = new Vector2[4]
            {
                new Vector2(worldRect.xMin, worldRect.yMin),
                new Vector2(worldRect.xMax, worldRect.yMin),
                new Vector2(worldRect.xMax, worldRect.yMax),
                new Vector2(worldRect.xMin, worldRect.yMax)
            };

            // 1. 检测 Rect 四角是否都在多边形内
            foreach (var corner in rectCorners)
            {
                if (!ContainsPoint(corner, polygon))
                    return false;
            }

            // 2. 检测多边形边是否与 Rect 边相交
            Vector2[] rectEdges = new Vector2[4]
            {
                rectCorners[0], rectCorners[1], // 下边
                rectCorners[2], rectCorners[3]  // 上边（逆序用于闭合）
            };

            if (!TryGetWorldVertices(polygon, _worldVertexBuffer))
                return false;

            // 检测多边形每条边与 Rect 每条边是否相交
            for (int i = 0; i < _worldVertexBuffer.Count; i++)
            {
                int nextIndex = (i + 1) % _worldVertexBuffer.Count;
                Vector2 polyEdgeStart = _worldVertexBuffer[i];
                Vector2 polyEdgeEnd = _worldVertexBuffer[nextIndex];

                // Rect 的四条边
                if (SegmentIntersect(polyEdgeStart, polyEdgeEnd, rectCorners[0], rectCorners[1])) return false; // 下边
                if (SegmentIntersect(polyEdgeStart, polyEdgeEnd, rectCorners[1], rectCorners[2])) return false; // 右边
                if (SegmentIntersect(polyEdgeStart, polyEdgeEnd, rectCorners[2], rectCorners[3])) return false; // 上边
                if (SegmentIntersect(polyEdgeStart, polyEdgeEnd, rectCorners[3], rectCorners[0])) return false; // 左边
            }

            return true; // 四角都在内且无边相交
        }

        private bool PolygonOverlapsRect(Rect worldRect, BoundaryPolygon polygon)
        {
            if (!IsSimplePolygon(polygon)) return false;

            Vector2[] rectCorners = new Vector2[4]
            {
                new Vector2(worldRect.xMin, worldRect.yMin),
                new Vector2(worldRect.xMax, worldRect.yMin),
                new Vector2(worldRect.xMax, worldRect.yMax),
                new Vector2(worldRect.xMin, worldRect.yMax)
            };

            // A) 任意 Rect 角在多边形内 => overlap
            foreach (var corner in rectCorners)
            {
                if (ContainsPoint(corner, polygon))
                    return true;
            }

            // B) 任意多边形顶点在 Rect 内 => overlap
            if (!TryGetWorldVertices(polygon, _worldVertexBuffer))
                return false;

            for (int i = 0; i < _worldVertexBuffer.Count; i++)
            {
                if (worldRect.Contains(_worldVertexBuffer[i]))
                    return true;
            }

            // C) 任意边相交 => overlap
            for (int i = 0; i < _worldVertexBuffer.Count; i++)
            {
                int next = (i + 1) % _worldVertexBuffer.Count;
                Vector2 a1 = _worldVertexBuffer[i];
                Vector2 a2 = _worldVertexBuffer[next];

                if (SegmentIntersect(a1, a2, rectCorners[0], rectCorners[1])) return true;
                if (SegmentIntersect(a1, a2, rectCorners[1], rectCorners[2])) return true;
                if (SegmentIntersect(a1, a2, rectCorners[2], rectCorners[3])) return true;
                if (SegmentIntersect(a1, a2, rectCorners[3], rectCorners[0])) return true;
            }

            return false;
        }

        /// <summary>
        /// 检测两条线段是否相交
        /// </summary>
        private bool SegmentIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            float d1 = CrossProduct(p3 - p1, p2 - p1);
            float d2 = CrossProduct(p4 - p1, p2 - p1);
            float d3 = CrossProduct(p1 - p3, p4 - p3);
            float d4 = CrossProduct(p2 - p3, p4 - p3);

            if (d1 * d2 < 0 && d3 * d4 < 0)
                return true; // 相交

            return false;
        }

        private float CrossProduct(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        private bool IsSimplePolygon(BoundaryPolygon polygon)
        {
            if (polygon == null || polygon.vertices == null || polygon.vertices.Count < 3)
                return false;

            int count = polygon.vertices.Count;
            for (int i = 0; i < count; i++)
            {
                Vector2 a1 = polygon.vertices[i];
                Vector2 a2 = polygon.vertices[(i + 1) % count];

                for (int j = i + 1; j < count; j++)
                {
                    int nextJ = (j + 1) % count;
                    if (i == j || (i + 1) % count == j || i == nextJ)
                        continue;

                    Vector2 b1 = polygon.vertices[j];
                    Vector2 b2 = polygon.vertices[nextJ];
                    if (SegmentsIntersect(a1, a2, b1, b2))
                        return false;
                }
            }

            return true;
        }

        private static bool SegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            float d1 = Direction(p3, p4, p1);
            float d2 = Direction(p3, p4, p2);
            float d3 = Direction(p1, p2, p3);
            float d4 = Direction(p1, p2, p4);

            if (((d1 > 0f && d2 < 0f) || (d1 < 0f && d2 > 0f)) &&
                ((d3 > 0f && d4 < 0f) || (d3 < 0f && d4 > 0f)))
            {
                return true;
            }

            if (Mathf.Approximately(d1, 0f) && OnSegment(p3, p4, p1)) return true;
            if (Mathf.Approximately(d2, 0f) && OnSegment(p3, p4, p2)) return true;
            if (Mathf.Approximately(d3, 0f) && OnSegment(p1, p2, p3)) return true;
            if (Mathf.Approximately(d4, 0f) && OnSegment(p1, p2, p4)) return true;
            return false;
        }

        private static float Direction(Vector2 a, Vector2 b, Vector2 c)
        {
            return CrossProductStatic(c - a, b - a);
        }

        private static bool OnSegment(Vector2 a, Vector2 b, Vector2 p)
        {
            return p.x >= Mathf.Min(a.x, b.x) - 0.0001f && p.x <= Mathf.Max(a.x, b.x) + 0.0001f &&
                   p.y >= Mathf.Min(a.y, b.y) - 0.0001f && p.y <= Mathf.Max(a.y, b.y) + 0.0001f;
        }

        private static float CrossProductStatic(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        /// <summary>
        /// 检测世界坐标点是否在某个凹角顶点的 radius 范围内。
        /// 凹角（reflex vertex）：内角 > 180°，即多边形在该顶点处向内凹陷。
        /// </summary>
        public bool IsNearConcaveVertex(Vector2 worldPoint, float radius)
        {
            float radiusSq = radius * radius;
            foreach (var polygon in _polygons)
            {
                if (polygon == null || polygon.vertices.Count < 3) continue;
                if (!IsPolygonSimple(polygon)) continue;

                var verts = polygon.vertices;
                int count = verts.Count;

                // 计算有符号面积确定绕向（正=CCW，负=CW）
                float area = 0f;
                for (int i = 0; i < count; i++)
                {
                    int j = (i + 1) % count;
                    area += verts[i].x * verts[j].y - verts[j].x * verts[i].y;
                }
                float orientation = area > 0f ? 1f : -1f;

                for (int i = 0; i < count; i++)
                {
                    Vector2 prev = verts[(i - 1 + count) % count];
                    Vector2 curr = verts[i];
                    Vector2 next = verts[(i + 1) % count];

                    // 叉积：(curr-prev) × (next-curr)
                    Vector2 d1 = curr - prev;
                    Vector2 d2 = next - curr;
                    float cross = d1.x * d2.y - d1.y * d2.x;

                    // 凹角：叉积与绕向符号相反
                    if (cross * orientation >= 0f) continue;

                    // 转换到世界坐标
                    Vector3 worldV = transform.TransformPoint(new Vector3(curr.x, curr.y, 0f));
                    Vector2 worldVertex = new Vector2(worldV.x, worldV.y);

                    if ((worldPoint - worldVertex).sqrMagnitude <= radiusSq)
                        return true;
                }
            }
            return false;
        }

        // ==================== Unity 生命周期 ====================

        private void OnValidate()
        {
            if (_polygons.Count == 0)
            {
                Debug.LogWarning("[BoundaryWall] 至少需要一个多边形");
            }

            for (int i = 0; i < _polygons.Count; i++)
            {
                var polygon = _polygons[i];
                if (polygon != null && !IsSimplePolygon(polygon))
                {
                    Debug.LogWarning($"[BoundaryWall] 多边形 '{polygon.name}' 存在自相交或顺序错误，预览填充与运行时判定会失真。");
                }
            }

            _activePolygonIndex = Mathf.Clamp(_activePolygonIndex, 0, Mathf.Max(0, _polygons.Count - 1));
        }

        private void OnDrawGizmos()
        {
            if (_polygons.Count == 0) return;

            foreach (var polygon in _polygons)
            {
                if (polygon.vertices.Count < 3) continue;

                Gizmos.color = polygon.color;

                // 绘制边界线（闭合环）
                for (int i = 0; i < polygon.vertices.Count; i++)
                {
                    int nextIndex = (i + 1) % polygon.vertices.Count;
                    Vector3 worldStart = transform.TransformPoint(new Vector3(polygon.vertices[i].x, polygon.vertices[i].y, 0f));
                    Vector3 worldEnd = transform.TransformPoint(new Vector3(polygon.vertices[nextIndex].x, polygon.vertices[nextIndex].y, 0f));
                    Gizmos.DrawLine(worldStart, worldEnd);
                }

            }
        }

#if UNITY_EDITOR
        private static void DrawFilledPolygon(Vector3[] worldVertices)
        {
            if (worldVertices == null || worldVertices.Length < 3)
                return;

            List<Vector2> points = new List<Vector2>(worldVertices.Length);
            for (int i = 0; i < worldVertices.Length; i++)
                points.Add(new Vector2(worldVertices[i].x, worldVertices[i].y));

            if (!TryTriangulate(points, out var indices))
                return;

            for (int i = 0; i < indices.Count; i += 3)
            {
                Vector3[] tri = new Vector3[3]
                {
                    worldVertices[indices[i]],
                    worldVertices[indices[i + 1]],
                    worldVertices[indices[i + 2]]
                };
                UnityEditor.Handles.DrawAAConvexPolygon(tri);
            }
        }

        private static bool TryTriangulate(IReadOnlyList<Vector2> points, out List<int> indices)
        {
            indices = new List<int>();
            if (points == null || points.Count < 3)
                return false;

            List<int> polygon = new List<int>(points.Count);
            float area = SignedArea(points);
            if (Mathf.Approximately(area, 0f))
                return false;

            if (area > 0f)
            {
                for (int i = 0; i < points.Count; i++) polygon.Add(i);
            }
            else
            {
                for (int i = points.Count - 1; i >= 0; i--) polygon.Add(i);
            }

            int guard = 0;
            while (polygon.Count > 3 && guard++ < 2048)
            {
                bool earFound = false;
                for (int i = 0; i < polygon.Count; i++)
                {
                    int prevIndex = polygon[(i - 1 + polygon.Count) % polygon.Count];
                    int currIndex = polygon[i];
                    int nextIndex = polygon[(i + 1) % polygon.Count];

                    Vector2 a = points[prevIndex];
                    Vector2 b = points[currIndex];
                    Vector2 c = points[nextIndex];

                    if (!IsConvex(a, b, c))
                        continue;

                    bool hasPointInside = false;
                    for (int j = 0; j < polygon.Count; j++)
                    {
                        int testIndex = polygon[j];
                        if (testIndex == prevIndex || testIndex == currIndex || testIndex == nextIndex)
                            continue;

                        if (PointInTriangle(points[testIndex], a, b, c))
                        {
                            hasPointInside = true;
                            break;
                        }
                    }

                    if (hasPointInside)
                        continue;

                    indices.Add(prevIndex);
                    indices.Add(currIndex);
                    indices.Add(nextIndex);
                    polygon.RemoveAt(i);
                    earFound = true;
                    break;
                }

                if (!earFound)
                    return false;
            }

            if (polygon.Count == 3)
            {
                indices.Add(polygon[0]);
                indices.Add(polygon[1]);
                indices.Add(polygon[2]);
                return true;
            }

            return false;
        }

        private static float SignedArea(IReadOnlyList<Vector2> points)
        {
            float area = 0f;
            for (int i = 0; i < points.Count; i++)
            {
                int j = (i + 1) % points.Count;
                area += points[i].x * points[j].y - points[j].x * points[i].y;
            }
            return area * 0.5f;
        }

        private static bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
        {
            return CrossProductStatic(b - a, c - b) >= -0.0001f;
        }

        private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = CrossProductStatic(p - a, b - a);
            float d2 = CrossProductStatic(p - b, c - b);
            float d3 = CrossProductStatic(p - c, a - c);

            bool hasNeg = (d1 < -0.0001f) || (d2 < -0.0001f) || (d3 < -0.0001f);
            bool hasPos = (d1 > 0.0001f) || (d2 > 0.0001f) || (d3 > 0.0001f);
            return !(hasNeg && hasPos);
        }
#endif

    }

    // ==================== 数据结构 ====================

    /// <summary>
    /// 单个多边形数据
    /// </summary>
    [System.Serializable]
    public class BoundaryPolygon
    {
        public string name = "多边形";
        public List<Vector2> vertices = new List<Vector2>(); // 局部 X/Y 坐标
        public Color color = new Color(0f, 1f, 0f, 0.3f);
    }
}
