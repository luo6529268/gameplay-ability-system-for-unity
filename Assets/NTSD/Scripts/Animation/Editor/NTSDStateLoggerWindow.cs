#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using NTSD.Tools;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace NTSD.Animation.Editor
{
    /// <summary>
    /// 状态日志调试窗口 v2
    ///
    /// 功能：
    ///   1. Category 过滤勾选（Combo / Trans / Frame / Lock）
    ///   2. 日志级别过滤（All / Warn+ / Error only）
    ///   3. 角色下拉选择
    ///   4. Folded 视图（连续重复折叠 + 可展开）
    ///   5. Timeline 视图（按 Tick 分组，每帧一个可折叠标题）
    ///   6. 触发式捕获（按下按钮后仅记录 N 帧，自动停止）
    ///   7. Pause on Error 自动冻结
    ///   8. buffer 2000 条
    /// </summary>
    public class NTSDStateLoggerWindow : OdinEditorWindow
    {
        [MenuItem("NTSD/State Logger")]
        private static void OpenWindow()
        {
            var w = GetWindow<NTSDStateLoggerWindow>("📋 NTSD State Logger");
            w.minSize = new Vector2(720, 520);
            w.Show();
        }

        // ── 折叠条目（Folded 视图用）────────────────────────────────────────

        private class FoldedEntry
        {
            public Log.StateLogEntry Representative;
            public int  Count;
            public int  FirstFrame;
            public int  LastFrame;
            public bool IsExpanded;
            public readonly List<Log.StateLogEntry> SubEntries = new List<Log.StateLogEntry>();
            public string Key =>
                $"{FirstFrame}|{Representative.ObjectName}|{Representative.Category}|{Representative.Message}";
        }

        // ── Tick 分组（Timeline 视图用）──────────────────────────────────────

        private class TickGroup
        {
            public int  FrameCount;
            public bool IsExpanded = true;
            public readonly List<Log.StateLogEntry> Entries = new List<Log.StateLogEntry>();
        }

        // ── 运行时状态 ────────────────────────────────────────────────────────

        private int  _lastVersion = -1;
        private bool _paused      = false;

        private Log.StateLogEntry[] _rawEntries = null;
        private List<FoldedEntry>   _foldedList = new List<FoldedEntry>();
        private List<TickGroup>     _tickGroups = new List<TickGroup>();

        private Dictionary<string, bool> _expandMap      = new Dictionary<string, bool>();
        private Dictionary<int, bool>    _tickExpandMap  = new Dictionary<int, bool>();

        // STUCK 预捕获面板（Priority 7）
        private Log.StateLogEntry[] _stuckSnapshot      = null;
        private int                 _stuckFrameCount    = -1;
        private bool                _stuckPanelExpanded = true;

        // ── 过滤器状态 ────────────────────────────────────────────────────────

        // Category 勾选
        private bool _showCombo  = true;
        private bool _showTrans  = true;
        private bool _showFrame  = true;
        private bool _showLock   = true;

        // 日志级别
        private enum MinLevel { All, WarnAndAbove, ErrorOnly }
        private MinLevel _minLevel = MinLevel.All;
        private static readonly string[] MinLevelLabels = { "All", "Warn+", "Error" };

        // 角色选择
        private string[] _charOptions = { "All" };
        private int      _charIndex   = 0;

        // 视图模式
        private enum ViewMode { Folded, Timeline }
        private ViewMode _viewMode = ViewMode.Folded;

        // 触发式捕获输入
        private int _captureFrameInput = 60;

        // 滚动
        private Vector2 _scrollPos;

        // GUIStyle 缓存
        private GUIStyle _rowStyle;
        private GUIStyle _subStyle;
        private GUIStyle _tickHeaderStyle;

        // ── 配色方案（文字色 / 背景色） ───────────────────────────────────────
        //  文字色：鲜艳，在深色背景上清晰可辨
        private static readonly Color ColFrame   = new Color(0.20f, 0.95f, 0.60f);  // 薄荷绿  — Frame 转换成功
        private static readonly Color ColCombo   = new Color(1.00f, 0.78f, 0.10f);  // 琥珀黄  — Combo 输入
        private static readonly Color ColTrans   = new Color(0.30f, 0.80f, 1.00f);  // 天蓝    — Trans 状态机
        private static readonly Color ColLock    = new Color(0.80f, 0.55f, 1.00f);  // 薰衣草紫 — Lock 锁定
        private static readonly Color ColError   = new Color(1.00f, 0.28f, 0.28f);  // 鲜红    — Error / STUCK
        private static readonly Color ColWarn    = new Color(1.00f, 0.60f, 0.15f);  // 橙      — Warn
        private static readonly Color ColSub     = new Color(0.45f, 0.45f, 0.45f);  // 暗灰    — 展开子项
        private static readonly Color ColDefault = new Color(0.78f, 0.78f, 0.78f);  // 浅灰    — 其他

        //  Tick 标题背景：深蓝调，与编辑器灰色明显区分
        private static readonly Color ColTickEven = new Color(0.10f, 0.15f, 0.22f); // 深海蓝（偶数帧）
        private static readonly Color ColTickOdd  = new Color(0.15f, 0.20f, 0.28f); // 略浅海蓝（奇数帧）

        // ── 生命周期 ──────────────────────────────────────────────────────────

        protected override void OnEnable()
        {
            base.OnEnable();
            EditorApplication.update += OnEditorUpdate;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (_paused) return;
            int ver = Log.StateVersion;
            if (ver != _lastVersion)
            {
                _lastVersion = ver;
                RefreshEntries();
                Repaint();
            }
        }

        // ── 数据刷新 ──────────────────────────────────────────────────────────

        private void RefreshEntries()
        {
            _rawEntries = Log.GetStateSnapshot();
            RebuildCharOptions();
            RebuildFolded();
            RebuildTickGroups();
            DetectStuck();
        }

        private void RebuildCharOptions()
        {
            var seen = new HashSet<string>();
            if (_rawEntries != null)
                foreach (var e in _rawEntries)
                    seen.Add(e.ObjectName);

            string prev = (_charIndex < _charOptions.Length) ? _charOptions[_charIndex] : "All";
            var list = new List<string> { "All" };
            list.AddRange(seen);
            _charOptions = list.ToArray();

            _charIndex = 0;
            for (int i = 0; i < _charOptions.Length; i++)
                if (_charOptions[i] == prev) { _charIndex = i; break; }
        }

        private void RebuildFolded()
        {
            _foldedList.Clear();
            if (_rawEntries == null) return;

            FoldedEntry cur = null;
            foreach (var e in _rawEntries)
            {
                if (!IsVisible(e)) continue;

                bool same = cur != null
                    && cur.Representative.ObjectName == e.ObjectName
                    && cur.Representative.Category   == e.Category
                    && cur.Representative.Message    == e.Message;

                if (same)
                {
                    cur.Count++;
                    cur.LastFrame = e.FrameCount;
                    cur.SubEntries.Add(e);
                }
                else
                {
                    cur = new FoldedEntry
                    {
                        Representative = e,
                        Count      = 1,
                        FirstFrame = e.FrameCount,
                        LastFrame  = e.FrameCount,
                    };
                    cur.SubEntries.Add(e);
                    _expandMap.TryGetValue(cur.Key, out cur.IsExpanded);
                    _foldedList.Add(cur);
                }
            }
        }

        private void RebuildTickGroups()
        {
            _tickGroups.Clear();
            if (_rawEntries == null) return;

            TickGroup cur = null;
            foreach (var e in _rawEntries)
            {
                if (!IsVisible(e)) continue;

                if (cur == null || cur.FrameCount != e.FrameCount)
                {
                    cur = new TickGroup { FrameCount = e.FrameCount };
                    _tickExpandMap.TryGetValue(e.FrameCount, out cur.IsExpanded);
                    if (!_tickExpandMap.ContainsKey(e.FrameCount)) cur.IsExpanded = true;
                    _tickGroups.Add(cur);
                }
                cur.Entries.Add(e);
            }
        }

        // ── 可见性判断（统一过滤入口）────────────────────────────────────────

        private bool IsVisible(in Log.StateLogEntry e)
        {
            // 角色过滤
            string charFilter = CurrentCharFilter();
            if (charFilter != "All" && e.ObjectName != charFilter) return false;

            // Category 过滤
            switch (e.Category)
            {
                case "Combo": if (!_showCombo) return false; break;
                case "Trans": if (!_showTrans) return false; break;
                case "Frame": if (!_showFrame) return false; break;
                case "Lock":  if (!_showLock)  return false; break;
            }

            // 日志级别过滤
            if (_minLevel == MinLevel.WarnAndAbove && e.Level == Log.StateLogLevel.Info) return false;
            if (_minLevel == MinLevel.ErrorOnly    && e.Level != Log.StateLogLevel.Error
                && e.Message.IndexOf("STUCK",   StringComparison.OrdinalIgnoreCase) < 0
                && e.Message.IndexOf("BLOCKED", StringComparison.OrdinalIgnoreCase) < 0) return false;

            return true;
        }

        private string CurrentCharFilter()
            => (_charOptions.Length > 0 && _charIndex < _charOptions.Length)
               ? _charOptions[_charIndex] : "All";

        // ── GUI 入口 ──────────────────────────────────────────────────────────

        protected override void OnImGUI()
        {
            DrawMainToolbar();
            DrawFilterBar();
            DrawStatusBar();
            GUILayout.Space(1);
            DrawStuckPanel();
            DrawLogArea();
        }

        // ── 主工具栏 ──────────────────────────────────────────────────────────

        private void DrawMainToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            Log.StateLogEnabled = GUILayout.Toggle(
                Log.StateLogEnabled, "Enable", EditorStyles.toolbarButton, GUILayout.Width(54));

            Log.AutoPauseOnError = GUILayout.Toggle(
                Log.AutoPauseOnError, "Pause on Error", EditorStyles.toolbarButton, GUILayout.Width(104));

            GUILayout.Space(4);

            if (_paused)
            {
                if (GUILayout.Button("▶ Resume", EditorStyles.toolbarButton, GUILayout.Width(74)))
                {
                    _paused = false;
                    Log.StateLogEnabled = true;
                }
            }
            else
            {
                if (GUILayout.Button("⏸ Pause", EditorStyles.toolbarButton, GUILayout.Width(74)))
                    _paused = true;
            }

            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(44)))
            {
                Log.ClearStateLog();
                _rawEntries = null;
                _foldedList.Clear();
                _tickGroups.Clear();
                _expandMap.Clear();
                _tickExpandMap.Clear();
                _stuckSnapshot   = null;
                _stuckFrameCount = -1;
                Repaint();
            }

            if (GUILayout.Button("Export", EditorStyles.toolbarButton, GUILayout.Width(50)))
                ExportToFile();

            GUILayout.Space(8);

            // ── 触发式捕获 ──
            GUILayout.Label("Capture:", EditorStyles.miniLabel);
            _captureFrameInput = EditorGUILayout.IntField(
                _captureFrameInput, EditorStyles.toolbarTextField, GUILayout.Width(36));
            GUILayout.Label("f", EditorStyles.miniLabel);
            if (GUILayout.Button("▶ Go", EditorStyles.toolbarButton, GUILayout.Width(40)))
            {
                Log.ClearStateLog();
                Log.CaptureFrames(_captureFrameInput);
                _paused = false;
                _rawEntries = null;
                _foldedList.Clear();
                _tickGroups.Clear();
                _stuckSnapshot   = null;
                _stuckFrameCount = -1;
            }
            if (GUILayout.Button("✕", EditorStyles.toolbarButton, GUILayout.Width(22)))
                Log.CancelCapture();

            GUILayout.FlexibleSpace();

            // 角色选择
            GUILayout.Label("Char:", EditorStyles.miniLabel);
            int newIdx = EditorGUILayout.Popup(
                _charIndex, _charOptions, EditorStyles.toolbarPopup, GUILayout.Width(110));
            if (newIdx != _charIndex)
            {
                _charIndex = newIdx;
                RebuildFolded();
                RebuildTickGroups();
            }

            GUILayout.Space(6);

            ViewMode newMode = (ViewMode)GUILayout.Toolbar(
                (int)_viewMode,
                new[] { "Folded", "Timeline" },
                EditorStyles.toolbarButton,
                GUILayout.Width(140));
            if (newMode != _viewMode)
                _viewMode = newMode;

            EditorGUILayout.EndHorizontal();
        }

        // ── 过滤栏（第二行）──────────────────────────────────────────────────

        private void DrawFilterBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label("Show:", EditorStyles.miniLabel);

            bool newCombo = GUILayout.Toggle(_showCombo, "Combo", EditorStyles.toolbarButton, GUILayout.Width(52));
            bool newTrans = GUILayout.Toggle(_showTrans, "Trans", EditorStyles.toolbarButton, GUILayout.Width(48));
            bool newFrame = GUILayout.Toggle(_showFrame, "Frame", EditorStyles.toolbarButton, GUILayout.Width(48));
            bool newLock  = GUILayout.Toggle(_showLock,  "Lock",  EditorStyles.toolbarButton, GUILayout.Width(44));

            bool filterChanged = newCombo != _showCombo || newTrans != _showTrans
                              || newFrame != _showFrame  || newLock  != _showLock;
            _showCombo = newCombo;
            _showTrans = newTrans;
            _showFrame = newFrame;
            _showLock  = newLock;

            GUILayout.Space(12);
            GUILayout.Label("Level:", EditorStyles.miniLabel);

            MinLevel newLevel = (MinLevel)EditorGUILayout.Popup(
                (int)_minLevel, MinLevelLabels, EditorStyles.toolbarPopup, GUILayout.Width(68));
            if (newLevel != _minLevel) { _minLevel = newLevel; filterChanged = true; }

            if (filterChanged)
            {
                RebuildFolded();
                RebuildTickGroups();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        // ── 状态栏 ────────────────────────────────────────────────────────────

        private void DrawStatusBar()
        {
            int total = _rawEntries?.Length ?? 0;
            int rem   = Log.CaptureFramesRemaining;
            string captureStr = rem >= 0 ? $"  ⏺ Capturing {rem}f left" : "";
            string liveStr    = _paused              ? "[PAUSED]"
                              : !Log.StateLogEnabled ? "[DISABLED]"
                                                     : "[LIVE]";
            EditorGUILayout.LabelField(
                $"Buffer: {total}/2000   {liveStr}{captureStr}",
                EditorStyles.miniLabel);
        }

        // ── 日志区域 ──────────────────────────────────────────────────────────

        private void DrawLogArea()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandHeight(true));

            if (_viewMode == ViewMode.Folded)
                DrawFolded();
            else
                DrawTimeline();

            EditorGUILayout.EndScrollView();
        }

        // ── Folded 视图 ───────────────────────────────────────────────────────

        private void DrawFolded()
        {
            if (_foldedList.Count == 0)
            {
                EditorGUILayout.HelpBox("No entries matching current filters.", MessageType.Info);
                return;
            }

            foreach (var fe in _foldedList)
            {
                DrawFoldedRow(fe);
                if (fe.IsExpanded)
                    DrawSubEntries(fe);
            }
        }

        private void DrawFoldedRow(FoldedEntry fe)
        {
            string frameLabel  = fe.Count > 1 ? $"[F{fe.FirstFrame}–{fe.LastFrame}]" : $"[F{fe.FirstFrame}]";
            string countBadge  = fe.Count > 1 ? $"  ×{fe.Count}" : "";
            string line = $"{frameLabel} [{fe.Representative.ObjectName}] [{fe.Representative.Category}] {fe.Representative.Message}{countBadge}";

            EditorGUILayout.BeginHorizontal();
            GUI.color = PickColor(fe.Representative);

            if (fe.Count > 1)
            {
                string arrow = fe.IsExpanded ? "▼" : "▶";
                if (GUILayout.Button(arrow, EditorStyles.label, GUILayout.Width(14), GUILayout.Height(16)))
                {
                    fe.IsExpanded = !fe.IsExpanded;
                    _expandMap[fe.Key] = fe.IsExpanded;
                }
            }
            else
            {
                GUILayout.Space(18);
            }

            EditorGUILayout.LabelField(line, RowStyle());
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSubEntries(FoldedEntry fe)
        {
            GUI.color = ColSub;
            foreach (var sub in fe.SubEntries)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(28);
                EditorGUILayout.LabelField(
                    $"  [F{sub.FrameCount}] [{sub.ObjectName}] [{sub.Category}] {sub.Message}",
                    SubStyle());
                EditorGUILayout.EndHorizontal();
            }
            GUI.color = Color.white;
        }

        // ── Timeline 视图（按 Tick 分组）─────────────────────────────────────

        private void DrawTimeline()
        {
            if (_tickGroups.Count == 0)
            {
                EditorGUILayout.HelpBox("No entries matching current filters.", MessageType.Info);
                return;
            }

            for (int gi = 0; gi < _tickGroups.Count; gi++)
            {
                var tg = _tickGroups[gi];
                DrawTickHeader(tg, gi);
                if (tg.IsExpanded)
                {
                    // 行背景与对应 Tick 标题同色系，透明度 0.45 使色带清晰可见
                    Color rowBg = gi % 2 == 0
                        ? new Color(0.10f, 0.15f, 0.22f, 0.45f)
                        : new Color(0.15f, 0.20f, 0.28f, 0.45f);

                    foreach (var e in tg.Entries)
                    {
                        Rect rowRect = EditorGUILayout.BeginHorizontal();
                        if (Event.current.type == EventType.Repaint)
                            EditorGUI.DrawRect(rowRect, rowBg);
                        GUILayout.Space(20);
                        GUI.color = PickColor(e);
                        EditorGUILayout.LabelField(
                            $"[{e.ObjectName}] [{e.Category}] {e.Message}",
                            RowStyle());
                        GUI.color = Color.white;
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }

        private void DrawTickHeader(TickGroup tg, int index)
        {
            // 奇偶帧交替底色
            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, TickHeaderStyle(),
                GUILayout.ExpandWidth(true), GUILayout.Height(16));
            EditorGUI.DrawRect(rect, index % 2 == 0 ? ColTickEven : ColTickOdd);

            string arrow = tg.IsExpanded ? "▼" : "▶";
            string label = $"{arrow}  Tick F{tg.FrameCount}  ({tg.Entries.Count} entries)";

            GUI.color = Color.white;
            if (GUI.Button(rect, label, TickHeaderStyle()))
            {
                tg.IsExpanded = !tg.IsExpanded;
                _tickExpandMap[tg.FrameCount] = tg.IsExpanded;
            }
        }

        // ── GUIStyle 缓存 ─────────────────────────────────────────────────────

        private GUIStyle RowStyle()
        {
            if (_rowStyle == null)
                _rowStyle = new GUIStyle(EditorStyles.label)
                {
                    richText = true,
                    wordWrap = false,
                    fontSize = 11,
                    font     = EditorStyles.miniLabel.font,
                };
            return _rowStyle;
        }

        private GUIStyle SubStyle()
        {
            if (_subStyle == null)
                _subStyle = new GUIStyle(RowStyle())
                {
                    normal = { textColor = ColSub },
                };
            return _subStyle;
        }

        private GUIStyle TickHeaderStyle()
        {
            if (_tickHeaderStyle == null)
                _tickHeaderStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize  = 10,
                    padding   = new RectOffset(6, 0, 0, 0),
                    normal    = { textColor = new Color(0.85f, 0.85f, 0.85f) },
                    hover     = { textColor = Color.white },
                };
            return _tickHeaderStyle;
        }

        // ── 颜色选取 ──────────────────────────────────────────────────────────

        private static Color PickColor(in Log.StateLogEntry e)
        {
            // Error 与关键词优先
            if (e.Level == Log.StateLogLevel.Error) return ColError;
            if (e.Message.IndexOf("STUCK",   StringComparison.OrdinalIgnoreCase) >= 0) return ColError;
            if (e.Message.IndexOf("BLOCKED", StringComparison.OrdinalIgnoreCase) >= 0) return ColError;
            if (e.Level == Log.StateLogLevel.Warn) return ColWarn;

            // 按 Category 分色
            switch (e.Category)
            {
                case "Frame": return ColFrame;
                case "Combo": return ColCombo;
                case "Trans": return ColTrans;
                case "Lock":  return ColLock;
            }
            return ColDefault;
        }

        // ── STUCK 预捕获面板（Priority 7）────────────────────────────────────

        /// <summary>
        /// 扫描 raw entries，找到第一条 Error / STUCK / BLOCKED 条目，
        /// 锁定其前 50 条（含自身）存入 _stuckSnapshot。
        /// 已有 snapshot 时不重复锁定（需手动 Dismiss 后才重新检测）。
        /// </summary>
        private void DetectStuck()
        {
            if (_rawEntries == null || _rawEntries.Length == 0) return;
            if (_stuckSnapshot != null) return; // 已锁定，等用户 Dismiss

            for (int i = 0; i < _rawEntries.Length; i++)
            {
                var e = _rawEntries[i];
                bool isStuck = e.Level == Log.StateLogLevel.Error
                    || e.Message.IndexOf("STUCK",   StringComparison.OrdinalIgnoreCase) >= 0
                    || e.Message.IndexOf("BLOCKED", StringComparison.OrdinalIgnoreCase) >= 0;

                if (!isStuck) continue;

                int start = Mathf.Max(0, i - 49);
                int count = i - start + 1;
                _stuckSnapshot      = new Log.StateLogEntry[count];
                Array.Copy(_rawEntries, start, _stuckSnapshot, 0, count);
                _stuckFrameCount    = e.FrameCount;
                _stuckPanelExpanded = true;
                break;
            }
        }

        private void DrawStuckPanel()
        {
            if (_stuckSnapshot == null) return;

            // 红色标题行（可折叠）
            Rect headerRect = GUILayoutUtility.GetRect(
                GUIContent.none, EditorStyles.boldLabel,
                GUILayout.ExpandWidth(true), GUILayout.Height(20));
            EditorGUI.DrawRect(headerRect, new Color(0.72f, 0.05f, 0.05f, 1.0f));

            string arrow = _stuckPanelExpanded ? "▼" : "▶";
            if (GUI.Button(headerRect,
                    $"{arrow}  ⚠ STUCK Context — F{_stuckFrameCount}  (last {_stuckSnapshot.Length} entries before error)",
                    TickHeaderStyle()))
                _stuckPanelExpanded = !_stuckPanelExpanded;

            if (!_stuckPanelExpanded) return;

            // Dismiss 按钮
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("✕ Dismiss", EditorStyles.miniButton, GUILayout.Width(70)))
            {
                _stuckSnapshot   = null;
                _stuckFrameCount = -1;
                EditorGUILayout.EndHorizontal();
                return;
            }
            EditorGUILayout.EndHorizontal();

            // 条目列表（深红底色，比正常行更显眼）
            Color bg = new Color(0.30f, 0.06f, 0.06f, 0.55f);
            foreach (var e in _stuckSnapshot)
            {
                Rect rowRect = EditorGUILayout.BeginHorizontal();
                if (Event.current.type == EventType.Repaint)
                    EditorGUI.DrawRect(rowRect, bg);
                GUI.color = PickColor(e);
                EditorGUILayout.LabelField(
                    $"[F{e.FrameCount}] [{e.ObjectName}] [{e.Category}] {e.Message}",
                    RowStyle());
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
            }

            // 分隔线
            GUILayout.Space(2);
            EditorGUILayout.LabelField("─────────────────────────────────────",
                EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(2);
        }

        // ── 导出到文件 ────────────────────────────────────────────────────────

        private void ExportToFile()
        {
            var snapshot = Log.GetStateSnapshot();
            if (snapshot == null || snapshot.Length == 0)
            {
                EditorUtility.DisplayDialog("Export", "No log entries to export.", "OK");
                return;
            }

            // 输出到项目根目录下的 Logs/ 文件夹
            string logsDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs"));
            Directory.CreateDirectory(logsDir);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string path      = Path.Combine(logsDir, $"NTSDStateLog_{timestamp}.txt");

            var lines = new List<string>
            {
                $"# NTSD State Log — {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                $"# Filter: Char={CurrentCharFilter()}  Level={_minLevel}" +
                $"  Combo={_showCombo}  Trans={_showTrans}  Frame={_showFrame}  Lock={_showLock}",
                $"# Buffer entries (raw): {snapshot.Length}",
                "",
            };

            int written = 0;
            foreach (var e in snapshot)
            {
                if (!IsVisible(e)) continue;
                lines.Add($"[F{e.FrameCount}] [{e.ObjectName}] [{e.Category}] [{e.Level}] {e.Message}");
                written++;
            }

            File.WriteAllLines(path, lines);

            Debug.Log($"[NTSD State Logger] Exported {written} entries → {path}");
            EditorUtility.RevealInFinder(path);
        }
    }
}
#endif
