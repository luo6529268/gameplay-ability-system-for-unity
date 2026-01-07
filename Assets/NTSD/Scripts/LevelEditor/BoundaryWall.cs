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
        /// 检测点是否在任意一个多边形内（并集语义）
        /// </summary>
        public bool ContainsPoint(Vector2 worldPoint)
        {
            if (!_isEnabled) return false;

            foreach (var polygon in _polygons)
            {
                if (ContainsPoint(worldPoint, polygon))
                    return true; // 在任意一个多边形内即可
            }

            return false;
        }

        /// <summary>
        /// 检测点是否在指定多边形内（Ray Casting 算法）
        /// </summary>
        private bool ContainsPoint(Vector2 worldPoint, BoundaryPolygon polygon)
        {
            if (polygon == null || polygon.vertices.Count < 3) return false;

            // 世界坐标转局部坐标（X/Y 平面）
            Vector3 localPos = transform.InverseTransformPoint(new Vector3(worldPoint.x, worldPoint.y, transform.position.z));
            Vector2 localPoint = new Vector2(localPos.x, localPos.y);

            // Ray Casting 算法
            bool inside = false;
            int vertexCount = polygon.vertices.Count;

            for (int i = 0, j = vertexCount - 1; i < vertexCount; j = i++)
            {
                Vector2 vi = polygon.vertices[i];
                Vector2 vj = polygon.vertices[j];

                if (((vi.y > localPoint.y) != (vj.y > localPoint.y)) &&
                    (localPoint.x < (vj.x - vi.x) * (localPoint.y - vi.y) / (vj.y - vi.y) + vi.x))
                {
                    inside = !inside;
                }
            }

            return inside;
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
                if (ContainsPoint(worldPoint, polygon))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 严谨检测：Rect 是否完全在多边形内
        /// 1. Rect 四角都在多边形内
        /// 2. 多边形边不与 Rect 边相交
        /// </summary>
        private bool RectFullyInsidePolygon(Rect worldRect, BoundaryPolygon polygon)
        {
            if (polygon == null || polygon.vertices.Count < 3) return false;

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

            // 获取多边形世界坐标顶点
            List<Vector2> worldPolygonVertices = new List<Vector2>();
            foreach (var localVertex in polygon.vertices)
            {
                Vector3 worldPos = transform.TransformPoint(new Vector3(localVertex.x, localVertex.y, 0f));
                worldPolygonVertices.Add(new Vector2(worldPos.x, worldPos.y));
            }

            // 检测多边形每条边与 Rect 每条边是否相交
            for (int i = 0; i < worldPolygonVertices.Count; i++)
            {
                int nextIndex = (i + 1) % worldPolygonVertices.Count;
                Vector2 polyEdgeStart = worldPolygonVertices[i];
                Vector2 polyEdgeEnd = worldPolygonVertices[nextIndex];

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
            if (polygon == null || polygon.vertices == null || polygon.vertices.Count < 3) return false;

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
            for (int i = 0; i < polygon.vertices.Count; i++)
            {
                Vector3 worldPos = transform.TransformPoint(new Vector3(polygon.vertices[i].x, polygon.vertices[i].y, 0f));
                if (worldRect.Contains(new Vector2(worldPos.x, worldPos.y)))
                    return true;
            }

            // C) 任意边相交 => overlap
            List<Vector2> worldPolygonVertices = new List<Vector2>(polygon.vertices.Count);
            foreach (var localVertex in polygon.vertices)
            {
                Vector3 worldPos = transform.TransformPoint(new Vector3(localVertex.x, localVertex.y, 0f));
                worldPolygonVertices.Add(new Vector2(worldPos.x, worldPos.y));
            }

            for (int i = 0; i < worldPolygonVertices.Count; i++)
            {
                int next = (i + 1) % worldPolygonVertices.Count;
                Vector2 a1 = worldPolygonVertices[i];
                Vector2 a2 = worldPolygonVertices[next];

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

        // ==================== Unity 生命周期 ====================

        private void OnValidate()
        {
            if (_polygons.Count == 0)
            {
                Debug.LogWarning("[BoundaryWall] 至少需要一个多边形");
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

        private void OnDrawGizmosSelected()
        {
            if (_polygons.Count == 0) return;

#if UNITY_EDITOR
            foreach (var polygon in _polygons)
            {
                if (polygon.vertices.Count < 3) continue;

                UnityEditor.Handles.color = polygon.color;

                Vector3[] worldVertices = new Vector3[polygon.vertices.Count];
                for (int i = 0; i < polygon.vertices.Count; i++)
                {
                    worldVertices[i] = transform.TransformPoint(new Vector3(polygon.vertices[i].x, polygon.vertices[i].y, 0f));
                }

                UnityEditor.Handles.DrawAAConvexPolygon(worldVertices);
            }
#endif
        }
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
