using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

namespace BeatEmUpTemplate2D
{

    /**
     * UIButton类 - 用于通过InputManager（手柄和键盘）导航UI按钮的类
     * 实现了ISelectHandler、IPointerDownHandler和ISubmitHandler接口
     * 提供按钮选择、点击和提交的功能
     */
    public class UIButton : MonoBehaviour, ISelectHandler, IPointerDownHandler, ISubmitHandler
    {

        public bool SelectOnStart; // 是否在开始时自动选择此按钮

        [Header("选中时改变按钮文本")]
        public Text buttonText; // 按钮文本组件
        private Color buttonTextDefaultColor = Color.white; // 默认按钮文本颜色
        public Color buttonTextSelectedColor = Color.black; // 按钮选中时的文本颜色

        [Header("选中时显示/隐藏图像（可选）")]
        public Image imageTarget; // 目标图像组件

        [Header("按钮音效")]
        [SerializeField] private string sfxOnClick = "UIButtonClick"; // 按钮点击时播放的音效
        [SerializeField] private string sfxOnSelect = "UIButtonSelect"; // 按钮选中时播放的音效
        [HideInInspector] public bool waitForButtonRelease; // 等待按钮释放的标志

        public static GameObject lastSelectedButton; // 最后选中的按钮
        private bool LoadSceneInProgress; // 场景加载是否进行中
        private EventSystem eventSystem => EventSystem.current; // 事件系统引用
        private InputManager input; // 输入管理器引用

        private Button thisButton; // 当前按钮组件
        private float timeAlive; // 对象存活时间

        void OnEnable()
        {
            timeAlive = Time.time; // 记录启用时间
        }

        void Start()
        {
            input = GetInputManager(); // 获取输入管理器实例

            // 如果设置了在开始时选中，且按钮组件存在，则选中此按钮
            if (SelectOnStart && GetComponent<Button>() != null) GetComponent<Button>().Select();

            thisButton = GetComponent<Button>(); // 获取按钮组件

            // 保存默认文本颜色
            if (buttonText != null) buttonTextDefaultColor = buttonText.color;
        }

        //---
        // 通过InputManager进行键盘/手柄导航
        //---

        void Update()
        {
            // 设置按钮文本颜色
            bool selected = (eventSystem.currentSelectedGameObject == gameObject && thisButton.interactable);
            if (buttonText != null) buttonText.color = selected ? buttonTextSelectedColor : buttonTextDefaultColor;

            // 显示/隐藏图像
            if (imageTarget != null) imageTarget.enabled = selected;

            // 按钮导航的条件检查
            if (InputManager.Instance == null) return; // 没有输入管理器时不执行操作
            if (InputManager.JoypadDirInputDetected()) return; // 忽略手柄输入（由Unity内置事件管理器处理）
            if (eventSystem.currentSelectedGameObject == null && UIButton.lastSelectedButton != null)
                eventSystem.SetSelectedGameObject(UIButton.lastSelectedButton); // 修复Unity中鼠标点击可交互区域外导致所有按钮取消选择的问题
            if (EventSystem.current.currentSelectedGameObject != gameObject) return; // 只响应选中的按钮
            if (InputManager.GetInputVector() == Vector2.zero) waitForButtonRelease = false; // 当前没有按下输入按钮，重置waitForButtonRelease
            if (waitForButtonRelease) return; // 在用户释放按钮之前等待

            // 获取键盘/手柄输入方向并相应导航
            Vector2 dir = InputManager.GetInputVector();
            if (dir != Vector2.zero) NavigateToNextSelectable(dir);
        }

        // 在指定方向（vector2）查找下一个可选择按钮并选中它
        void NavigateToNextSelectable(Vector2 dir)
        {
            Selectable current = eventSystem.currentSelectedGameObject?.GetComponent<Selectable>();
            if (current != null)
            {
                Selectable next = current.FindSelectable(dir);
                if (next != null) next.Select();
            }
        }

        // 查找输入管理器
        InputManager GetInputManager()
        {
            InputManager im = GameObject.FindObjectOfType<InputManager>();
            if (im == null) Debug.LogError("场景中未找到InputManager");
            return im;
        }

        // 此按钮被选中时调用
        public void OnSelect(BaseEventData eventData)
        {
            UIButton.lastSelectedButton = gameObject; // 设置此按钮为选中状态
            if (sfxOnSelect.Length > 0 && Time.time - timeAlive > Time.deltaTime)
                BeatEmUpTemplate2D.AudioController.PlaySFX(sfxOnSelect, Camera.main.transform.position); // 播放选中音效（跳过按钮首次出现时的音效）
            waitForButtonRelease = true; // 等待用户释放按钮后继续
        }

        // 鼠标点击此按钮时调用
        public void OnPointerDown(PointerEventData eventData)
        {
            if (sfxOnClick.Length > 0)
                BeatEmUpTemplate2D.AudioController.PlaySFX(sfxOnClick, Camera.main.transform.position); // 播放点击音效
        }

        // 通过键盘按下此按钮时调用
        public void OnSubmit(BaseEventData eventData)
        {
            if (sfxOnClick.Length > 0)
                BeatEmUpTemplate2D.AudioController.PlaySFX(sfxOnClick, Camera.main.transform.position); // 播放提交音效
        }

        // 禁用所有按钮交互
        void DisableAllButtons()
        {
            foreach (Button button in FindObjectsOfType<Button>()) button.interactable = false;
        }

        //----
        // 常用按钮操作
        //----

        // 退出应用程序
        public void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
        }

        // 加载指定场景
        public void LoadScene(string sceneName)
        {
            float sfxDuration = BeatEmUpTemplate2D.AudioController.GetSFXDuration(sfxOnClick);
            StartCoroutine(LoadSceneRoutine(sceneName, sfxDuration));
        }

        // 重新加载当前场景
        public void ReloadCurrentScene()
        {
            float sfxDuration = BeatEmUpTemplate2D.AudioController.GetSFXDuration(sfxOnClick);
            StartCoroutine(LoadSceneRoutine(SceneManager.GetActiveScene().name, sfxDuration));
        }

        // 延迟加载下一个场景的协程
        IEnumerator LoadSceneRoutine(string sceneName, float delay)
        {
            if (LoadSceneInProgress) yield break; // 如果场景加载正在进行中，则退出
            DisableAllButtons(); // 禁用所有按钮
            LoadSceneInProgress = true;
            yield return new WaitForSeconds(delay); // 等待指定时间
            SceneManager.LoadScene(sceneName); // 加载场景
            LoadSceneInProgress = false;
        }
    }

}
