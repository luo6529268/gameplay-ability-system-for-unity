using UnityEngine;
using UnityEditor;

namespace BeatEmUpTemplate2D
{

    /*
     * 自定义窗口类，用于显示帮助和文档信息
     * 继承自Unity的EditorWindow，用于创建编辑器窗口
     */
    public class CustomWindow : EditorWindow
    {

        // 窗口标题
        private string windowTitle;
        // 窗口内容
        private string content;
        // 文档链接URL
        private string url = "https://www.osarion.com/BeatEmUpTemplate2D/documentation.html";
        // 内边距大小
        private int padding = 25;

        /*
         * 显示窗口的静态方法
         * @param title 窗口标题
         * @param content 窗口内容
         * @param size 窗口大小
         */
        public static void ShowWindow(string title, string content, Vector2 size)
        {
            // 获取或创建窗口实例
            CustomWindow window = GetWindow<CustomWindow>(title);

            // 设置窗口标题和内容
            window.windowTitle = title;
            window.content = content;
            window.Repaint();

            // 设置窗口大小范围
            window.minSize = size;
            window.maxSize = new Vector2(1024, 1024);

            // 计算窗口位置（屏幕中心）
            Vector2 screenCenter = new Vector2(Screen.currentResolution.width / 2, Screen.currentResolution.height / 2);
            Vector2 windowSize = size;
            Vector2 windowPosition = screenCenter - (windowSize / 2);

            // 设置窗口的位置和大小
            window.position = new Rect(windowPosition.x, windowPosition.y, windowSize.x, windowSize.y);
        }

        // 绘制GUI界面的方法
        private void OnGUI()
        {
            // 显示标题
            ShowTitle(windowTitle);

            // 如果内容存在，显示内容文本区域
            if (!string.IsNullOrEmpty(content))
            {
                EditorGUILayout.TextArea(content, labelStyle(), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            }

            // 显示文档链接部分
            EditorGUILayout.Space(10);
            ShowTitle("Documentation");
            GUILayout.Label("For detailed documentation, FAQ, tutorials, and videos, please visit the website:", labelStyle());

            // 显示打开文档的按钮
            if (GUILayout.Button(new GUIContent("Online Documentation", "Open link"), buttonStyle()))
            {
                Application.OpenURL(url);
            }
        }

        // 按钮样式设置方法
        GUIStyle buttonStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.normal.textColor = new Color(1, 1, 1, .6f);  // 正常状态文本颜色
            style.hover.textColor = Color.white;               // 悬停状态文本颜色
            style.alignment = TextAnchor.MiddleCenter;         // 文本居中对齐
            style.richText = true;                            // 启用富文本
            style.margin = new RectOffset(padding, padding, 0, 10);  // 设置边距
            style.fixedHeight = 40;                           // 固定高度
            return style;
        }

        // 标签样式设置方法
        // @param bold 是否使用粗体
        GUIStyle labelStyle(bool bold = false)
        {
            // 根据参数选择使用粗体或普通标签样式
            GUIStyle style = bold ? new GUIStyle(EditorStyles.boldLabel) : new GUIStyle(EditorStyles.label);
            style.wordWrap = true;                            // 启用自动换行
            style.richText = true;                            // 启用富文本
            style.padding = new RectOffset(padding, padding, 0, 0);  // 设置内边距
            style.alignment = TextAnchor.UpperLeft;           // 文本左上对齐
            return style;
        }

        // 显示标题的方法
        // @param label 要显示的标题文本
        void ShowTitle(string label)
        {
            // 设置富文本格式：粗体、14号字、白色
            string richText = $"<b><size=14><color=#FFFFFF>{label}</color></size></b>";
            GUILayout.Label(richText, titleStyle());
        }

        // 标题样式设置方法
        GUIStyle titleStyle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.wordWrap = true;                            // 启用自动换行
            style.richText = true;                            // 启用富文本
            style.padding = new RectOffset(padding, padding, padding, 0);  // 设置内边距
            style.alignment = TextAnchor.UpperLeft;           // 文本左上对齐
            style.fontSize = 14;                              // 字体大小
            style.richText = true;                            // 启用富文本
            return style;
        }
    }

}