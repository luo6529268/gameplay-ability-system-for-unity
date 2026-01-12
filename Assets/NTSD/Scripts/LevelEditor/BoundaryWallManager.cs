using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MoreMountains.Tools;
using NTSD.Animation;
using NTSD.Simulation;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NTSD.LevelEditor
{
    /// <summary>
    /// 关卡边界管理器 - 管理场景中的边界多边形
    ///
    /// 职责：
    /// - 自动收集场景中所有 BoundaryWall 组件
    /// - 提供 API：检测 Rect 是否在边界内
    /// - 提供 JSON 导出功能（每关卡一个 JSON 文件）
    ///
    /// 使用方式：
    /// 1. 在场景中创建 GameObject，挂载此脚本
    /// 2. 场景中添加 BoundaryWall 定义边界
    /// 3. 通过 BoundaryWallManager.Instance 访问 API
    /// 4. Inspector 点击"导出边界 JSON"按钮导出配置
    ///
    /// API 示例：
    /// ```csharp
    /// // 检测 Rect 是否允许移动（多层规则：Walkable/HardBlock/Special）
    /// bool ok = BoundaryWallManager.Instance.IsRectWalkable(footprintRect);
    /// if (!ok) { /* 阻止移动 */ }
    /// ```
    ///
    /// ⚠️ 项目规范：
    /// - 所有坐标都是 X/Y 平面（不是 X/Z）
    /// - Rect 参数的 x/y 与 transform.position 单位一致
    /// </summary>
    public class BoundaryWallManager : MMSingleton<BoundaryWallManager>, ILF2StageBoundsProvider
    {
        // ==================== 序列化字段（Inspector 可配置）====================

        /// <summary>
        /// 是否自动刷新边界列表（每帧查找场景中所有 BoundaryWall）
        /// </summary>
        [SerializeField]
        [Tooltip("是否自动刷新边界列表（性能开销较大，建议编辑器模式启用，运行时禁用）")]
        private bool _autoRefresh = false;

        /// <summary>
        /// 调试模式（输出详细日志）
        /// </summary>
        [SerializeField]
        [Tooltip("调试模式（输出详细日志）")]
        private bool _debugMode = false;

        [Header("FLF Dynamics Bounds")]
        [SerializeField]
        [Tooltip("对齐 FLF mechanics.js: floor_xbound；开启后会对 X 进行 clamp（Z 仍然永远 clamp）。")]
        private bool _flfFloorXBound = true;

        // ==================== 私有字段 ====================

        /// <summary>
        /// 缓存的所有 BoundaryWall
        /// </summary>
        private List<BoundaryWall> _boundaries = new List<BoundaryWall>();

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool _initialized = false;

        private bool _boundsCacheValid = false;
        private LF2StageBoundsPx _boundsCachePx;

        // ==================== Unity 生命周期 ====================

        protected override void Awake()
        {
            base.Awake();
            RefreshBoundaries();
        }

        private void Start()
        {
            if (!_initialized)
            {
                RefreshBoundaries();
            }
        }

        private void Update()
        {
            // 编辑器模式下支持自动刷新
            if (_autoRefresh && Application.isEditor)
            {
                RefreshBoundaries();
            }
        }

        // ==================== 公共 API ====================

        /// <summary>
        /// 运行时 API：单层边界（Walkable union）。
        /// - Rect 完全位于任意 polygon 内 => true
        /// - 否则 => false
        /// - ps.y 为视觉高度，不参与边界
        /// </summary>
        public bool IsRectWalkable(Rect rectXY)
        {
            if (!_initialized)
            {
                RefreshBoundaries();
            }

            if (_boundaries.Count == 0)
            {
                return true;
            }

            foreach (var boundary in _boundaries)
            {
                if (boundary == null || !boundary.IsEnabled) continue;

                if (boundary.IsRectAllowed(rectXY))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// å•å±‚è¾¹ç•Œï¼špoint åœ¨ä»»æ„?polygon å†…å³å…è®¸ï¼ˆå¹¶é›†ï¼‰
        /// </summary>
        public bool IsPointWalkable(Vector2 pointXY)
        {
            if (!_initialized)
            {
                RefreshBoundaries();
            }

            if (_boundaries.Count == 0)
            {
                return true;
            }

            foreach (var boundary in _boundaries)
            {
                if (boundary == null || !boundary.IsEnabled) continue;
                if (boundary.ContainsPointWorld(pointXY))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 手动刷新边界列表（查找场景中所有 BoundaryWall）
        /// </summary>
        public void RefreshBoundaries()
        {
            _boundaries.Clear();
            _boundsCacheValid = false;

            // 查找场景中所有 BoundaryWall
            BoundaryWall[] foundBoundaries = FindObjectsOfType<BoundaryWall>();
            _boundaries.AddRange(foundBoundaries);

            _initialized = true;
            RebuildBoundsCache();

            if (_debugMode)
            {
                Debug.Log($"[BoundaryWallManager] 刷新边界列表，找到 {_boundaries.Count} 个 BoundaryWall");
            }
        }

        /// <summary>
        /// 检测 Rect 是否完全在边界内（X/Y 平面）
        /// </summary>
        /// <param name="rectXY">世界坐标矩形（X/Y 平面）</param>
        /// <returns>true = Rect 完全在边界内，可移动</returns>
        /// <summary>
        /// 运行时 API：提供对齐 FLF dynamics 的关卡边界（像素坐标）。
        /// 数据来源：BoundaryWallEditor 维护的多边形集合；这里取其 AABB 作为 FLF 的 bg.width / zboundary 等价输入。
        /// </summary>
        public bool TryGetStageBoundsPx(out LF2StageBoundsPx bounds)
        {
            if (!_initialized)
            {
                RefreshBoundaries();
            }

            if (!_boundsCacheValid)
            {
                RebuildBoundsCache();
            }

            bounds = _boundsCachePx;
            return _boundsCacheValid;
        }

        private void RebuildBoundsCache()
        {
            _boundsCacheValid = false;
            _boundsCachePx = default;

            if (_boundaries == null || _boundaries.Count == 0) return;

            bool hasAnyVertex = false;
            float minX = 0f, maxX = 0f, minY = 0f, maxY = 0f;

            for (int b = 0; b < _boundaries.Count; b++)
            {
                var boundary = _boundaries[b];
                if (boundary == null || !boundary.IsEnabled) continue;

                var polygons = boundary.Polygons;
                if (polygons == null) continue;

                for (int p = 0; p < polygons.Count; p++)
                {
                    var poly = polygons[p];
                    if (poly == null || poly.vertices == null || poly.vertices.Count < 3) continue;

                    for (int i = 0; i < poly.vertices.Count; i++)
                    {
                        Vector2 v = poly.vertices[i];
                        Vector3 world = boundary.transform.TransformPoint(new Vector3(v.x, v.y, 0f));
                        float x = world.x;
                        float y = world.y;

                        if (!hasAnyVertex)
                        {
                            hasAnyVertex = true;
                            minX = maxX = x;
                            minY = maxY = y;
                        }
                        else
                        {
                            if (x < minX) minX = x;
                            if (x > maxX) maxX = x;
                            if (y < minY) minY = y;
                            if (y > maxY) maxY = y;
                        }
                    }
                }
            }

            if (!hasAnyVertex) return;

            float xMinPx = minX * SimulationConstants.PIXELS_PER_UNIT;
            float xMaxPx = maxX * SimulationConstants.PIXELS_PER_UNIT;
            float zMinPx = minY * SimulationConstants.PIXELS_PER_UNIT;
            float zMaxPx = maxY * SimulationConstants.PIXELS_PER_UNIT;

            _boundsCachePx = new LF2StageBoundsPx(_flfFloorXBound, xMinPx, xMaxPx, zMinPx, zMaxPx);
            _boundsCacheValid = true;
        }

        /// <summary>
        /// 检测 Rect 是否完全在边界内（X/Y 平面）。
        /// </summary>
        public bool IsRectInsideBoundary(Rect rectXY)
        {
            if (!_initialized)
            {
                RefreshBoundaries();
            }

            // 如果没有边界，默认允许（无限制）
            if (_boundaries.Count == 0)
            {
                return true;
            }

            // 检测 Rect 是否在任意一个启用的边界内
            // 注意：通常只有一个边界（outer boundary），但支持多个
            foreach (var boundary in _boundaries)
            {
                if (boundary != null && boundary.IsEnabled)
                {
                    bool isInside = boundary.ContainsRect(rectXY);

                    if (_debugMode)
                    {
                        Debug.Log($"[BoundaryWallManager] Rect {rectXY} 在边界 {boundary.BoundaryName} 内: {isInside}");
                    }

                    // 只要在任意一个边界内就允许
                    if (isInside)
                    {
                        return true;
                    }
                }
            }

            // 不在任何边界内 → 越界
            if (_debugMode)
            {
                Debug.Log($"[BoundaryWallManager] Rect {rectXY} 越界（不在任何边界内）");
            }

            return false;
        }

        /// <summary>
        /// 检测点是否在边界内（X/Y 平面）
        /// </summary>
        /// <param name="pointXY">世界坐标点（X/Y 平面）</param>
        /// <returns>true = 点在边界内</returns>
        public bool IsPointInsideBoundary(Vector2 pointXY)
        {
            if (!_initialized)
            {
                RefreshBoundaries();
            }

            if (_boundaries.Count == 0)
            {
                return true; // 无边界限制
            }

            foreach (var boundary in _boundaries)
            {
                if (boundary != null && boundary.IsEnabled && boundary.ContainsPoint(pointXY))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取所有启用的边界（只读）
        /// </summary>
        public IReadOnlyList<BoundaryWall> EnabledBoundaries
        {
            get
            {
                if (!_initialized)
                {
                    RefreshBoundaries();
                }

                return _boundaries.FindAll(b => b != null && b.IsEnabled);
            }
        }

        /// <summary>
        /// 获取所有边界（只读）
        /// </summary>
        public IReadOnlyList<BoundaryWall> AllBoundaries
        {
            get
            {
                if (!_initialized)
                {
                    RefreshBoundaries();
                }

                return _boundaries;
            }
        }

        // ==================== JSON 导出 ====================

#if UNITY_EDITOR
        /// <summary>
        /// 导出边界数据到 JSON 文件
        /// </summary>
        public void ExportToJson()
        {
            RefreshBoundaries();

            if (_boundaries.Count == 0)
            {
                EditorUtility.DisplayDialog("导出失败", "场景中没有找到任何 BoundaryWall", "确定");
                return;
            }

            // 打开保存文件对话框
            string defaultFileName = $"{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}_Boundary.json";
            string filePath = EditorUtility.SaveFilePanel(
                "导出边界 JSON",
                Application.dataPath,
                defaultFileName,
                "json"
            );

            if (string.IsNullOrEmpty(filePath))
            {
                return; // 用户取消
            }

            // 构建导出数据
            BoundaryExportData exportData = new BoundaryExportData();
            exportData.boundaries = new List<BoundaryData>();

            foreach (var boundary in _boundaries)
            {
                if (boundary == null || !boundary.IsEnabled)
                    continue;

                BoundaryData boundaryData = new BoundaryData();
                boundaryData.boundaryName = boundary.BoundaryName;
                boundaryData.polygons = new List<PolygonData>();

                // 导出所有多边形（X/Y 平面）
                for (int polygonIndex = 0; polygonIndex < boundary.PolygonCount; polygonIndex++)
                {
                    var polygon = boundary.Polygons[polygonIndex];
                    PolygonData polygonData = new PolygonData();
                    polygonData.name = polygon.name;
                    polygonData.verticesWorld = new List<Vector2Data>();

                    // 导出该多边形的所有顶点（世界坐标）
                    for (int vertexIndex = 0; vertexIndex < polygon.vertices.Count; vertexIndex++)
                    {
                        Vector3 worldPos = boundary.GetWorldVertex(polygonIndex, vertexIndex);
                        polygonData.verticesWorld.Add(new Vector2Data
                        {
                            x = worldPos.x,
                            y = worldPos.y
                        });
                    }

                    boundaryData.polygons.Add(polygonData);
                }

                exportData.boundaries.Add(boundaryData);
            }

            // 序列化为 JSON
            string json = JsonUtility.ToJson(exportData, true);

            // 写入文件
            try
            {
                File.WriteAllText(filePath, json);
                Debug.Log($"[BoundaryWallManager] 成功导出边界 JSON: {filePath}");
                EditorUtility.DisplayDialog("导出成功", $"边界数据已导出到:\n{filePath}", "确定");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BoundaryWallManager] 导出失败: {e.Message}");
                EditorUtility.DisplayDialog("导出失败", e.Message, "确定");
            }
        }
#endif

        // ==================== 调试辅助 ====================

        /// <summary>
        /// 获取调试信息字符串
        /// </summary>
        public string GetDebugInfo()
        {
            if (!_initialized)
            {
                RefreshBoundaries();
            }

            int enabledCount = _boundaries.FindAll(b => b != null && b.IsEnabled).Count;
            return $"BoundaryWallManager: {enabledCount}/{_boundaries.Count} boundaries enabled";
        }

        private void OnDrawGizmos()
        {
            // 在 Scene 视图显示管理器状态（可选）
            if (_debugMode && _initialized)
            {
                // 可以在这里绘制全局调试信息
            }
        }
    }

    // ==================== JSON 导出数据结构 ====================

    /// <summary>
    /// 导出数据包装器（支持多个边界）
    /// </summary>
    [System.Serializable]
    public class BoundaryExportData
    {
        public List<BoundaryData> boundaries;
    }

    /// <summary>
    /// 单个边界数据（包含多个多边形）
    /// </summary>
    [System.Serializable]
    public class BoundaryData
    {
        public string boundaryName;
        public List<PolygonData> polygons; // 多边形列表
    }

    /// <summary>
    /// 单个多边形数据
    /// </summary>
    [System.Serializable]
    public class PolygonData
    {
        public string name;
        public List<Vector2Data> verticesWorld; // 世界坐标顶点（X/Y）
    }

    /// <summary>
    /// Vector2 数据（JsonUtility 不支持 Vector2，需要自定义）
    /// </summary>
    [System.Serializable]
    public class Vector2Data
    {
        public float x;
        public float y;
    }

    // ==================== Custom Editor（Inspector 按钮）====================

#if UNITY_EDITOR
    [CustomEditor(typeof(BoundaryWallManager))]
    public class BoundaryWallManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            BoundaryWallManager manager = (BoundaryWallManager)target;

            // 导出 JSON 按钮
            if (GUILayout.Button("导出边界 JSON", GUILayout.Height(30)))
            {
                manager.ExportToJson();
            }

            EditorGUILayout.Space();

            // 刷新按钮
            if (GUILayout.Button("手动刷新边界列表"))
            {
                manager.RefreshBoundaries();
            }

            EditorGUILayout.Space();

            // 显示调试信息
            EditorGUILayout.HelpBox(manager.GetDebugInfo(), MessageType.Info);
        }
    }
#endif
}
