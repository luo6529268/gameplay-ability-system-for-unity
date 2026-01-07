using UnityEditor;
using UnityEngine;

namespace BeatEmUpTemplate2D {

/**
 * InputManager的自定义编辑器脚本
 * 用于扩展InputManager组件在Inspector中的显示
 * 在底部添加了文档说明按钮
 */
[CustomEditor(typeof(InputManager))]
public class InputManagerEditor : Editor {

    // 使用双换行符提高可读性
    string newLine = "\n\n";

        /**
         * 重写Inspector界面的绘制方法
         * 在默认Inspector内容下方添加自定义按钮
         */
        public override void OnInspectorGUI()
        {

            // 绘制默认的Inspector内容
            DrawDefaultInspector();

            // 添加10像素的垂直间距
            GUILayout.Space(10);

            // 如果用户点击了"点击此处获取有关此组件的更多信息"按钮
            if (GUILayout.Button("点击此处获取有关此组件的更多信息", GUILayout.Height(30)))
            {
                string title = "输入管理器";
                // 组件说明内容
                string content = "输入管理器处理键盘和手柄输入。它还提供了自定义按键和按钮映射的选项。" + newLine;

                // 添加控制修改说明
                content += highlightItem("如何修改控制设置 \n");
                content += "前往 Osarion/BeatEmUpTemplate2D/Scripts/Input/PlayerControls。" + newLine;
                content += "在检视器窗口中，您会看到一个名为'编辑资源'的按钮，点击它可以打开一个窗口，您可以在其中为整个项目设置按钮映射。\n";

                // 显示自定义窗口
                CustomWindow.ShowWindow(title, content, new Vector2(700, 500));
            }

        }

    /**
     * 高亮显示文本的快捷方法
     * @param label 需要高亮显示的文本
     * @param size 文本大小，默认为13
     * @return 返回带有HTML标签的格式化文本
     */
    string highlightItem(string label, int size = 13){
        return "<b><size=" + size + "><color=#FFFFFF>" + label + "</color></size></b>";
    }
}

}