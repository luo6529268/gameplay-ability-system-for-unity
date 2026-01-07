using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using NTSD.LevelEditor;

namespace NTSD.LevelEditor.Editor
{
    /// <summary>
    /// BoundaryWall custom editor:
    /// - Inspector: polygon list management (select/duplicate/delete/add)
    /// - Scene view: edit vertices on XY plane (drag / Shift+Click insert / Ctrl+Click delete)
    /// </summary>
    [CustomEditor(typeof(BoundaryWall))]
    public class BoundaryWallEditor : OdinEditor
    {
        private const float VertexHandleSize = 0.3f;
        private const float EdgeClickThreshold = 0.5f;
        private const float VertexClickThreshold = 1.0f;

        private BoundaryWall _target;
        private int _hoveredVertexIndex = -1;
        private int _hoveredEdgeIndex = -1;
        private Plane _editPlane;

        protected override void OnEnable()
        {
            base.OnEnable();
            _target = (BoundaryWall)target;
            UpdateEditPlane();
        }

        private void UpdateEditPlane()
        {
            if (_target == null) return;
            _editPlane = new Plane(Vector3.forward, _target.transform.position);
        }

        protected virtual void OnSceneGUI()
        {
            if (_target == null) return;
            if (_target.ActivePolygon == null) return;
            if (_target.ActivePolygon.vertices.Count < 3) return;

            UpdateEditPlane();
            SceneView.RepaintAll();

            HandleInput();
            DrawVertexHandles();
            DrawEdges();
        }

        private void DrawVertexHandles()
        {
            int polygonIndex = _target.ActivePolygonIndex;
            var polygon = _target.ActivePolygon;

            for (int i = 0; i < polygon.vertices.Count; i++)
            {
                Vector3 worldPos = _target.GetWorldVertex(polygonIndex, i);

                Handles.color = (i == _hoveredVertexIndex) ? Color.red : Color.yellow;
                Handles.SphereHandleCap(0, worldPos, Quaternion.identity, VertexHandleSize * 2f, EventType.Repaint);

                EditorGUI.BeginChangeCheck();

                Vector3 newWorldPos = Handles.Slider2D(
                    worldPos,
                    Vector3.forward,
                    Vector3.right,
                    Vector3.up,
                    VertexHandleSize,
                    Handles.SphereHandleCap,
                    0f
                );

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_target, "Move Vertex");
                    _target.SetWorldVertex(polygonIndex, i, newWorldPos);
                }

                Handles.Label(worldPos + Vector3.up * 0.5f, $"V{i}", EditorStyles.whiteMiniLabel);
            }
        }

        private void DrawEdges()
        {
            _hoveredEdgeIndex = -1;
            Vector2 mousePos = GetMouseWorldPoint2D();

            int polygonIndex = _target.ActivePolygonIndex;
            var polygon = _target.ActivePolygon;

            for (int i = 0; i < polygon.vertices.Count; i++)
            {
                int nextIndex = (i + 1) % polygon.vertices.Count;
                Vector3 worldStart = _target.GetWorldVertex(polygonIndex, i);
                Vector3 worldEnd = _target.GetWorldVertex(polygonIndex, nextIndex);

                bool isHovered = IsMouseNearEdge(mousePos, worldStart, worldEnd, EdgeClickThreshold);
                if (isHovered) _hoveredEdgeIndex = i;

                Handles.color = isHovered ? Color.cyan : Color.green;
                Handles.DrawLine(worldStart, worldEnd);

                if (isHovered && Event.current.shift)
                {
                    Vector3 midPoint = (worldStart + worldEnd) * 0.5f;
                    Handles.color = Color.white;
                    Handles.SphereHandleCap(0, midPoint, Quaternion.identity, VertexHandleSize * 0.5f, EventType.Repaint);
                    Handles.Label(midPoint + Vector3.up * 0.5f, "Shift+Click: Insert Vertex", EditorStyles.helpBox);
                }
            }
        }

        private void HandleInput()
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Vector2 mousePos = GetMouseWorldPoint2D();

                if (e.shift && _hoveredEdgeIndex >= 0)
                {
                    Undo.RecordObject(_target, "Insert Vertex");
                    _target.InsertVertex(_target.ActivePolygonIndex, _hoveredEdgeIndex);
                    e.Use();
                    return;
                }

                if (e.control)
                {
                    int clickedVertex = GetClickedVertex(mousePos);
                    if (clickedVertex >= 0)
                    {
                        Undo.RecordObject(_target, "Delete Vertex");
                        _target.RemoveVertex(_target.ActivePolygonIndex, clickedVertex);
                        e.Use();
                        return;
                    }
                }
            }

            if (e.type == EventType.MouseMove)
            {
                Vector2 mousePos = GetMouseWorldPoint2D();
                _hoveredVertexIndex = GetHoveredVertex(mousePos);
                SceneView.RepaintAll();
            }
        }

        private Vector2 GetMouseWorldPoint2D()
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (_editPlane.Raycast(ray, out float enter))
            {
                Vector3 worldPoint = ray.GetPoint(enter);
                return new Vector2(worldPoint.x, worldPoint.y);
            }

            return new Vector2(_target.transform.position.x, _target.transform.position.y);
        }

        private static bool IsMouseNearEdge(Vector2 mousePos, Vector3 worldStart, Vector3 worldEnd, float threshold)
        {
            Vector2 start2D = new Vector2(worldStart.x, worldStart.y);
            Vector2 end2D = new Vector2(worldEnd.x, worldEnd.y);
            float distance = DistanceToLineSegment(mousePos, start2D, end2D);
            return distance < threshold;
        }

        private static float DistanceToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 lineVec = lineEnd - lineStart;
            Vector2 pointVec = point - lineStart;

            float lineLengthSq = lineVec.sqrMagnitude;
            if (lineLengthSq < 0.0001f)
                return Vector2.Distance(point, lineStart);

            float t = Mathf.Clamp01(Vector2.Dot(pointVec, lineVec) / lineLengthSq);
            Vector2 projection = lineStart + t * lineVec;
            return Vector2.Distance(point, projection);
        }

        private int GetHoveredVertex(Vector2 mousePos)
        {
            int polygonIndex = _target.ActivePolygonIndex;
            var polygon = _target.ActivePolygon;

            for (int i = 0; i < polygon.vertices.Count; i++)
            {
                Vector3 worldPos = _target.GetWorldVertex(polygonIndex, i);
                Vector2 vertex2D = new Vector2(worldPos.x, worldPos.y);
                if (Vector2.Distance(mousePos, vertex2D) < VertexClickThreshold)
                    return i;
            }

            return -1;
        }

        private int GetClickedVertex(Vector2 mousePos)
        {
            return GetHoveredVertex(mousePos);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (_target == null) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Polygons", EditorStyles.boldLabel);

            for (int i = 0; i < _target.PolygonCount; i++)
            {
                EditorGUILayout.BeginHorizontal();

                var polygon = _target.Polygons[i];
                bool isActive = (i == _target.ActivePolygonIndex);

                GUI.backgroundColor = isActive ? Color.green : Color.white;
                if (GUILayout.Button($"{polygon.name} ({polygon.vertices.Count})", GUILayout.Height(25)))
                {
                    _target.ActivePolygonIndex = i;
                    SceneView.RepaintAll();
                }
                GUI.backgroundColor = Color.white;

                if (GUILayout.Button("Duplicate", GUILayout.Width(80)))
                {
                    _target.DuplicatePolygon(i);
                }

                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    _target.DeletePolygon(i);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("+ Add Polygon", GUILayout.Height(30)))
            {
                _target.AddPolygon();
                SceneView.RepaintAll();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Scene editing:\n" +
                "- Drag yellow handles to move vertices (XY plane)\n" +
                "- Shift+Click an edge to insert a vertex\n" +
                "- Ctrl+Click a vertex to delete\n" +
                "\n" +
                "Rule: every polygon is walkable; runtime uses union (inside any polygon = allowed).",
                MessageType.Info
            );

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Active Polygon", EditorStyles.boldLabel);
            if (_target.ActivePolygon != null)
            {
                EditorGUILayout.LabelField($"Name: {_target.ActivePolygon.name}");
                EditorGUILayout.LabelField($"Vertices: {_target.ActivePolygon.vertices.Count}");
                EditorGUILayout.LabelField($"Edit plane: XY (wall Z = {_target.transform.position.z:F2})");
            }
        }
    }
}

