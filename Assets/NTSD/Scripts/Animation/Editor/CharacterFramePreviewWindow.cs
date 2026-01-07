#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using NTSD.DatParser;

namespace NTSD.Animation.Editor
{
    /// <summary>
    /// 角色动画帧预览编辑器窗口
    /// 支持按帧播放、循环播放、wait延迟、next跳转等完整LF2动画逻辑
    /// </summary>
    public class CharacterFramePreviewWindow : OdinEditorWindow
    {
        #region 窗口打开

        [MenuItem("NTSD/Animation/Frame Preview Window")]
        private static void OpenWindow()
        {
            var window = GetWindow<CharacterFramePreviewWindow>("🎬 LF2 帧预览器");
            window.minSize = new Vector2(700, 800);
            window.Show();
        }

        #endregion

        #region 数据成员

        // data.txt 对象定义缓存
        private Dictionary<int, DataFileParser.ObjectData> dataObjectMap = null;
        private Dictionary<int, LF2CharacterDataWrapper> objectDataCache = new Dictionary<int, LF2CharacterDataWrapper>();

        // 对象精灵缓存 (oid -> sprites)
        private Dictionary<int, List<Sprite>> objectSpritesCache = new Dictionary<int, List<Sprite>>();

        /// <summary>
        /// opoint 对象状态跟踪类
        /// 用于实现与 FLF livingobject.prototype.TU_update 相同的帧播放逻辑
        ///
        /// ⭐ 递归生成逻辑：
        /// 1. 角色帧可以生成 opoint 对象
        /// 2. opoint 对象的帧也可以生成子 opoint 对象
        /// 3. 子 opoint 对象的帧也可以生成孙 opoint 对象
        /// 4. 无限递归，每个对象都有独立的生命周期
        /// </summary>
        private class OpointObjectState
        {
            public int oid;                    // 对象 ID（对应 opoint.oid）
            public ObjectPoint opoint;         // 原始 opoint 数据（保存位置、速度等信息）
            public int currentFrameId;         // 当前帧 ID（对应 $.frame.N）
            public int waitCount;              // wait 计数器（每帧递减）
            public LF2FrameData currentFrame;  // 当前帧数据（对应 $.frame.D）
            public bool isPlaying;             // 是否正在播放
        }

        // 所有活跃的 opoint 对象（支持多个对象同时存在）
        // ⭐ 这个列表是扁平化的，包含所有层级的 opoint 对象（父、子、孙...）
        // 每个对象都独立更新，互不干扰
        private List<OpointObjectState> activeOpointObjects = new List<OpointObjectState>();

        // ==================== 主界面标签页 ====================

        [TabGroup("Main", "🎬 播放器")]
        [BoxGroup("Main/🎬 播放器/角色选择")]
        [HorizontalGroup("Main/🎬 播放器/角色选择/Row1")]
        [LabelText("角色ID")]
        [ValueDropdown("GetLoadedCharacterIds", DropdownTitle = "选择角色", DropdownWidth = 250)]
        [OnValueChanged("OnCharacterChanged")]
        public int selectedCharacterId = 2;

        [BoxGroup("Main/🎬 播放器/角色选择")]
        [HorizontalGroup("Main/🎬 播放器/角色选择/Row1")]
        [LabelText("角色名称")]
        [ReadOnly]
        [ShowInInspector]
        [DisplayAsString(false)]
        private string CharacterName => CharacterAnimtorManager.Instance?.GetCharacterName(selectedCharacterId) ?? "Unknown";

        [PropertySpace(10)]
        [TabGroup("Main", "🎬 播放器")]
        [BoxGroup("Main/🎬 播放器/帧控制")]
        [HorizontalGroup("Main/🎬 播放器/帧控制/StateFilter")]
        [LabelText("状态筛选")]
        [ValueDropdown("GetAvailableStates", DropdownTitle = "选择状态", DropdownWidth = 250)]
        [OnValueChanged("OnStateFilterChanged")]
        public int? selectedStateFilter = null;

        [BoxGroup("Main/🎬 播放器/帧控制")]
        [HorizontalGroup("Main/🎬 播放器/帧控制/StateFilter")]
        [Button("🔄 清空筛选")]
        [EnableIf("@selectedStateFilter.HasValue")]
        [GUIColor(1f, 0.8f, 0.3f)]
        private void ClearStateFilter()
        {
            selectedStateFilter = null;
            Debug.Log("<color=cyan>状态筛选已清空，显示所有帧</color>");
        }

        [BoxGroup("Main/🎬 播放器/帧控制")]
        [HorizontalGroup("Main/🎬 播放器/帧控制/Row1")]
        [LabelText("起始帧ID")]
        [ValueDropdown("GetAvailableFrameIds", DropdownTitle = "选择起始帧", DropdownWidth = 350)]
        [OnValueChanged("OnStartFrameChanged")]
        public int startFrameId = 0;

        [BoxGroup("Main/🎬 播放器/帧控制")]
        [HorizontalGroup("Main/🎬 播放器/帧控制/Row1")]
        [LabelText("当前帧ID")]
        [ReadOnly]
        [ShowInInspector]
        [DisplayAsString(false)]
        private int CurrentFrameId => currentPlayingFrameId;

        [BoxGroup("Main/🎬 播放器/帧控制")]
        [HorizontalGroup("Main/🎬 播放器/帧控制/Row2")]
        [LabelText("帧名称")]
        [ReadOnly]
        [ShowInInspector]
        [DisplayAsString(false)]
        private string CurrentFrameName => GetCurrentFrameData()?.frameName ?? "N/A";

        [BoxGroup("Main/🎬 播放器/帧控制")]
        [HorizontalGroup("Main/🎬 播放器/帧控制/Row2")]
        [LabelText("当前状态")]
        [ReadOnly]
        [ShowInInspector]
        [DisplayAsString(false)]
        private int CurrentState => GetCurrentFrameData()?.state ?? -1;

        [PropertySpace(10)]
        [TabGroup("Main", "🎬 播放器")]
        [BoxGroup("Main/🎬 播放器/播放控制")]
        [ResponsiveButtonGroup("Main/🎬 播放器/播放控制/Buttons1")]
        [Button("▶ 播放", ButtonSizes.Large)]
        [GUIColor(0.3f, 1f, 0.3f)]
        [EnableIf("@!isPlaying")]
        private void Play()
        {
            StartPlayback(false);
        }

        [ResponsiveButtonGroup("Main/🎬 播放器/播放控制/Buttons1")]
        [Button("🔁 循环播放", ButtonSizes.Large)]
        [GUIColor(0.3f, 0.8f, 1f)]
        [EnableIf("@!isPlaying")]
        private void PlayLoop()
        {
            StartPlayback(true);
        }

        [ResponsiveButtonGroup("Main/🎬 播放器/播放控制/Buttons1")]
        [Button("⏹ 停止", ButtonSizes.Large)]
        [GUIColor(1f, 0.3f, 0.3f)]
        [EnableIf("isPlaying")]
        private void Stop()
        {
            StopPlayback();
        }

        [ResponsiveButtonGroup("Main/🎬 播放器/播放控制/Buttons1")]
        [Button("🔄 重置", ButtonSizes.Large)]
        [GUIColor(1f, 0.8f, 0.3f)]
        private void Reset()
        {
            ResetToStartFrame();
        }

        [BoxGroup("Main/🎬 播放器/播放控制")]
        [Button("⏭ 下一帧", ButtonSizes.Large)]
        [GUIColor(0.8f, 0.8f, 1f)]
        [EnableIf("@!isPlaying")]
        private void NextFrame()
        {
            ManualNextFrame();
        }

        [PropertySpace(10)]
        [TabGroup("Main", "🎬 播放器")]
        [BoxGroup("Main/🎬 播放器/播放状态")]
        [HorizontalGroup("Main/🎬 播放器/播放状态/Row1")]
        [LabelText("播放中")]
        [ReadOnly]
        [ShowInInspector]
        [ProgressBar(0, 1, ColorGetter = "GetPlayingColor")]
        private float IsPlayingProgress => isPlaying ? 1f : 0f;

        private Color GetPlayingColor(float value)
        {
            return value > 0.5f ? new Color(0.3f, 1f, 0.3f) : new Color(0.5f, 0.5f, 0.5f);
        }

        [BoxGroup("Main/🎬 播放器/播放状态")]
        [HorizontalGroup("Main/🎬 播放器/播放状态/Row1")]
        [LabelText("循环模式")]
        [ReadOnly]
        [ShowInInspector]
        [DisplayAsString(false)]
        private bool isPlaying = false;

        [BoxGroup("Main/🎬 播放器/播放状态")]
        [HorizontalGroup("Main/🎬 播放器/播放状态/Row1")]
        [LabelText("循环")]
        [ReadOnly]
        [ShowInInspector]
        [DisplayAsString(false)]
        private bool isLooping = false;

        [BoxGroup("Main/🎬 播放器/播放状态")]
        [HorizontalGroup("Main/🎬 播放器/播放状态/Row2")]
        [LabelText("等待计数")]
        [ReadOnly]
        [ShowInInspector]
        [ProgressBar(0, "@WaitTime", ColorGetter = "GetWaitColor")]
        private int CurrentWaitProgress => currentWaitCount;

        private Color GetWaitColor(int value)
        {
            return new Color(1f, 0.8f, 0.3f);
        }

        private int currentWaitCount = 0;

        [BoxGroup("Main/🎬 播放器/播放状态")]
        [HorizontalGroup("Main/🎬 播放器/播放状态/Row2")]
        [LabelText("下一帧")]
        [ReadOnly]
        [ShowInInspector]
        [DisplayAsString(false)]
        private int NextFrameId => GetCurrentFrameData()?.next ?? 999;

        [BoxGroup("Main/🎬 播放器/播放状态")]
        [HorizontalGroup("Main/🎬 播放器/播放状态/Row3")]
        [LabelText("等待时间")]
        [ReadOnly]
        [ShowInInspector]
        [DisplayAsString(false)]
        private int WaitTime => GetCurrentFrameData()?.wait ?? 0;

        [BoxGroup("Main/🎬 播放器/播放状态")]
        [HorizontalGroup("Main/🎬 播放器/播放状态/Row3")]
        [LabelText("精灵索引")]
        [ReadOnly]
        [ShowInInspector]
        [DisplayAsString(false)]
        private int PicIndex => GetCurrentFrameData()?.pic ?? 0;

        private Vector2 previewScrollPos;
        private const float PREVIEW_WIDTH = 600f;
        private const float PREVIEW_HEIGHT = 400f;
        private const float PIXEL_SCALE = 2f; // 放大倍数

        /// <summary>
        /// 自定义绘制精灵预览（角色 + opoint 对象）
        /// </summary>
        [PropertySpace(15)]
        [TabGroup("Main", "🎬 播放器")]
        [BoxGroup("Main/🎬 播放器/精灵预览")]
        [OnInspectorGUI]
        private void DrawSpritePreview()
        {
            var frameData = GetCurrentFrameData();
            if (frameData == null)
            {
                GUILayout.Label($"❌ 无帧数据 (角色ID={selectedCharacterId}, 帧ID={currentPlayingFrameId})", GUILayout.Height(PREVIEW_HEIGHT));
                return;
            }

            var sprites = CharacterAnimtorManager.Instance?.GetCharacterSpriteByID(selectedCharacterId);
            if (sprites == null)
            {
                GUILayout.Label($"❌ CharacterAnimtorManager 未加载角色 ID={selectedCharacterId} 的精灵数据\n请在 CharacterAnimtorManager 中点击【刷新所有数据】按钮", GUILayout.Height(PREVIEW_HEIGHT));
                return;
            }

            if (sprites.Count == 0)
            {
                GUILayout.Label($"❌ 角色 ID={selectedCharacterId} 的精灵列表为空\n精灵可能未加载成功，请查看 Console 日志", GUILayout.Height(PREVIEW_HEIGHT));
                return;
            }

            if (frameData.pic < 0 || frameData.pic >= sprites.Count)
            {
                GUILayout.Label($"❌ 精灵索引超出范围\npic={frameData.pic}, 精灵总数={sprites.Count}\n请检查帧数据配置", GUILayout.Height(PREVIEW_HEIGHT));
                return;
            }

            Sprite characterSprite = sprites[frameData.pic];
            if (characterSprite == null)
            {
                GUILayout.Label($"❌ 精灵为空 (pic={frameData.pic})\n精灵可能未正确加载，请检查 BMP 文件路径\n查看 Console 中的 [BMP 文件] 日志", GUILayout.Height(PREVIEW_HEIGHT));
                return;
            }

            // 创建预览区域
            Rect previewRect = GUILayoutUtility.GetRect(PREVIEW_WIDTH, PREVIEW_HEIGHT);

            // 绘制背景框
            GUI.Box(previewRect, GUIContent.none, EditorStyles.helpBox);

            // 开始 Handles 绘制（必须在 GUI 绘制中包裹 Handles 调用）
            Handles.BeginGUI();

            // 绘制背景网格
            DrawGrid(previewRect);

            // 计算角色精灵的绘制位置（居中）
            Vector2 characterCenter = previewRect.center;
            Rect characterSpriteRect = GetSpriteRect(characterSprite, characterCenter);

            // 绘制角色精灵（需要使用 sprite.rect 来处理图集裁剪）
            DrawSprite(characterSpriteRect, characterSprite);

            // 绘制所有活跃的 opoint 对象（独立于角色当前帧）
            DrawOpointObjects(characterSpriteRect, characterSprite);

            // 结束 Handles 绘制
            Handles.EndGUI();

            // 绘制信息（在 Handles 之外）
            DrawPreviewInfo(previewRect, characterSprite, frameData);
        }

        /// <summary>
        /// 绘制背景网格
        /// </summary>
        private void DrawGrid(Rect rect)
        {
            Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);

            // 垂直线
            for (float x = rect.x; x < rect.xMax; x += 50f)
            {
                Handles.DrawLine(new Vector3(x, rect.y), new Vector3(x, rect.yMax));
            }

            // 水平线
            for (float y = rect.y; y < rect.yMax; y += 50f)
            {
                Handles.DrawLine(new Vector3(rect.x, y), new Vector3(rect.xMax, y));
            }

            // 中心十字线
            Handles.color = new Color(1f, 0f, 0f, 0.5f);
            Handles.DrawLine(new Vector3(rect.center.x, rect.y), new Vector3(rect.center.x, rect.yMax));
            Handles.DrawLine(new Vector3(rect.x, rect.center.y), new Vector3(rect.xMax, rect.center.y));
        }

        /// <summary>
        /// 获取精灵绘制矩形（中心点锚点）
        /// </summary>
        private Rect GetSpriteRect(Sprite sprite, Vector2 center)
        {
            float width = sprite.rect.width * PIXEL_SCALE;
            float height = sprite.rect.height * PIXEL_SCALE;
            return new Rect(center.x - width * 0.5f, center.y - height * 0.5f, width, height);
        }

        /// <summary>
        /// 获取 opoint 对象精灵绘制矩形（底部中心点锚点）
        /// 在 LF2/FLF 中，opoint.x/y 指的是对象的脚底位置
        /// </summary>
        private Rect GetOpointSpriteRect(Sprite sprite, Vector2 bottomCenter)
        {
            float width = sprite.rect.width * PIXEL_SCALE;
            float height = sprite.rect.height * PIXEL_SCALE;
            // 底部中心点：X 轴居中，Y 轴从 bottomCenter.y 向上延伸 height
            return new Rect(bottomCenter.x - width * 0.5f, bottomCenter.y - height, width, height);
        }

        /// <summary>
        /// 绘制精灵（正确处理图集裁剪）
        /// </summary>
        private void DrawSprite(Rect position, Sprite sprite)
        {
            if (sprite == null || sprite.texture == null)
                return;

            // 获取精灵在纹理中的位置和大小
            Rect spriteRect = sprite.rect;
            Rect texCoords = new Rect(
                spriteRect.x / sprite.texture.width,
                spriteRect.y / sprite.texture.height,
                spriteRect.width / sprite.texture.width,
                spriteRect.height / sprite.texture.height
            );

            // 使用 GUI.DrawTextureWithTexCoords 来正确绘制图集中的精灵
            GUI.DrawTextureWithTexCoords(position, sprite.texture, texCoords);
        }

        /// <summary>
        /// 绘制所有活跃的 opoint 对象精灵
        /// 基于 FLF 源码：specialattack.prototype (LF/specialattack.js)
        /// </summary>
        /// <remarks>
        /// 重要：opoint 对象的生命周期完全独立于角色当前帧！
        ///
        /// FLF 逻辑：
        /// 1. 创建对象时，根据 opoint.action 设置起始帧
        /// 2. 每帧调用 TU_update() 更新对象状态
        /// 3. wait 计数器递减，为 0 时切换到 next 帧
        /// 4. 支持多个 opoint 对象同时存在和独立播放
        /// 5. 对象的显示和更新不受角色当前帧影响
        /// </remarks>
        private void DrawOpointObjects(Rect characterRect, Sprite characterSprite)
        {
            // 绘制所有活跃的 opoint 对象（不依赖角色当前帧数据）
            foreach (var opointState in activeOpointObjects)
            {
                if (!opointState.isPlaying) continue;

                // 使用对象自己的当前帧数据（完全独立于角色帧）
                var objectFrame = opointState.currentFrame;
                if (objectFrame == null) continue;

                // 尝试获取对象精灵
                var objectSprite = GetObjectSprite(opointState.oid, objectFrame.pic);

                if (objectSprite != null)
                {
                    // 绘制真实精灵
                    DrawOpointSprite(opointState.opoint, characterRect, characterSprite, objectSprite);
                }
                else
                {
                    // 绘制占位框
                    DrawOpointPlaceholder(opointState.opoint, characterRect, characterSprite, objectFrame);
                }
            }
        }

        /// <summary>
        /// 创建新的 opoint 对象并添加到活跃列表
        /// 基于 FLF 源码：specialattack.prototype.born (LF/specialattack.js:295-325)
        /// </summary>
        private void CreateOpointObject(ObjectPoint opoint)
        {
            // 加载对象数据
            var objectData = LoadObjectData(opoint.oid);
            if (objectData == null)
            {
                Debug.LogWarning($"<color=yellow>[opoint 创建失败] 无法加载对象数据: oid={opoint.oid}</color>");
                return;
            }

            // 查找起始帧（opoint.action）
            // FLF: $.trans.frame(opoint.action === 0 ? 999 : opoint.action)
            int startFrameId = opoint.action == 0 ? 999 : opoint.action;
            var startFrame = objectData.characterData.frames.Find(f => f.frameId == startFrameId);
            if (startFrame == null)
            {
                Debug.LogWarning($"<color=yellow>[opoint 创建失败] 未找到起始帧: oid={opoint.oid}, frameId={startFrameId}</color>");
                return;
            }

            // 创建新的 opoint 对象状态
            var newOpointState = new OpointObjectState
            {
                oid = opoint.oid,
                opoint = opoint,  // 保存原始 opoint 数据（位置、速度等）
                currentFrameId = startFrameId,
                currentFrame = startFrame,
                waitCount = startFrame.wait,
                isPlaying = true
            };

            // 添加到活跃列表
            activeOpointObjects.Add(newOpointState);

            Debug.Log($"<color=cyan>[opoint 创建] oid={opoint.oid}, 起始帧={startFrameId}, wait={startFrame.wait}, 当前活跃数量={activeOpointObjects.Count}</color>");
        }

        /// <summary>
        /// 更新所有 opoint 对象的帧（模拟 FLF 的 trans.trans 逻辑）
        /// </summary>
        private void UpdateOpointFrame()
        {
            // 倒序遍历，方便移除已停止的对象
            for (int i = activeOpointObjects.Count - 1; i >= 0; i--)
            {
                var opointState = activeOpointObjects[i];

                if (!opointState.isPlaying)
                {
                    // 移除已停止的对象
                    activeOpointObjects.RemoveAt(i);
                    Debug.Log($"<color=yellow>[opoint 移除] oid={opointState.oid}, 剩余活跃数量={activeOpointObjects.Count}</color>");
                    continue;
                }

                // 模拟 FLF 的 trans.trans() 逻辑
                if (opointState.waitCount > 0)
                {
                    // wait 递减
                    opointState.waitCount--;
                }
                else
                {
                    // wait = 0 时切换帧
                    int nextFrameId = opointState.currentFrame.next;

                    // 检查是否为终止帧
                    if (IsEndFrame(nextFrameId))
                    {
                        Debug.Log($"<color=yellow>[opoint 播放结束] oid={opointState.oid}, 终止帧={nextFrameId}</color>");
                        // 立即移除对象
                        activeOpointObjects.RemoveAt(i);
                        Debug.Log($"<color=yellow>[opoint 移除] oid={opointState.oid}, 剩余活跃数量={activeOpointObjects.Count}</color>");
                        continue;
                    }

                    // 加载下一帧数据
                    var objectData = LoadObjectData(opointState.oid);
                    if (objectData != null)
                    {
                        var nextFrame = objectData.characterData.frames.Find(f => f.frameId == nextFrameId);
                        if (nextFrame != null)
                        {
                            // 切换帧
                            opointState.currentFrameId = nextFrameId;
                            opointState.currentFrame = nextFrame;
                            opointState.waitCount = nextFrame.wait;

                            Debug.Log($"<color=gray>[opoint 帧切换] oid={opointState.oid}, 帧={nextFrameId}, wait={nextFrame.wait}, next={nextFrame.next}</color>");

                            // ⭐ 检查新帧是否有 opoint，如果有则创建子 opoint 对象（递归生成）
                            // 这样就能实现 opoint → 子 opoint → 孙 opoint 的多层嵌套
                            if (nextFrame.opoint != null && nextFrame.opoint.oid > 0)
                            {
                                CreateOpointObject(nextFrame.opoint);
                                Debug.Log($"<color=magenta>[opoint 递归创建] 父 oid={opointState.oid} 在帧 {nextFrameId} 创建了子 opoint oid={nextFrame.opoint.oid}</color>");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"<color=yellow>[opoint] 未找到下一帧: oid={opointState.oid}, frameId={nextFrameId}</color>");
                            opointState.isPlaying = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 绘制 opoint 真实精灵
        /// </summary>
        private void DrawOpointSprite(ObjectPoint opoint, Rect characterRect, Sprite characterSprite, Sprite objectSprite)
        {
            // 根据 FLF 逻辑计算对象位置（底部中心点）
            Vector2 objectBottomCenter = CalculateOpointPosition(opoint, characterRect, characterSprite);

            // 计算对象精灵矩形（使用底部中心点锚点）
            Rect objectRect = GetOpointSpriteRect(objectSprite, objectBottomCenter);

            // 绘制对象精灵（半透明以便区分）
            Color originalColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.8f);
            DrawSprite(objectRect, objectSprite);
            GUI.color = originalColor;

            // 绘制边框
            // Handles.color = Color.clear;
            // Handles.DrawSolidRectangleWithOutline(objectRect, Color.clear, Color.clear);

            // 绘制速度矢量（从底部中心点开始）
            DrawVelocityVector(opoint, objectBottomCenter);

            // 绘制标签（在精灵矩形的底部中心）
            Rect labelRect = new Rect(objectBottomCenter.x - 40, objectBottomCenter.y, 80, 40);
            DrawOpointLabel(opoint, labelRect, Color.green);
        }

        /// <summary>
        /// 绘制 opoint 占位框（当没有精灵时）
        /// </summary>
        private void DrawOpointPlaceholder(ObjectPoint opoint, Rect characterRect, Sprite characterSprite, LF2FrameData objectFrame)
        {
            // 计算对象位置（底部中心点）
            Vector2 objectBottomCenter = CalculateOpointPosition(opoint, characterRect, characterSprite);

            // 绘制占位框（30x30像素，以底部中心点为锚点）
            float boxSize = 30;
            Rect objRect = new Rect(objectBottomCenter.x - boxSize * 0.5f, objectBottomCenter.y - boxSize, boxSize, boxSize);

            // 半透明红框
            Handles.color = new Color(1f, 0f, 0f, 0.5f);
            Handles.DrawSolidRectangleWithOutline(objRect, new Color(1f, 0f, 0f, 0.2f), new Color(1f, 0f, 0f, 0.8f));

            // 绘制速度矢量（从底部中心点开始）
            DrawVelocityVector(opoint, objectBottomCenter);

            // 绘制标签（在底部中心）
            string labelText = objectFrame != null
                ? $"oid:{opoint.oid}\nframe:{opoint.action}\n{objectFrame.frameName}"
                : $"oid:{opoint.oid}\nLoading...";
            Rect labelRect = new Rect(objectBottomCenter.x - 40, objectBottomCenter.y, 80, 60);
            DrawOpointLabel(opoint, labelRect, Color.red, labelText);
        }

        /// <summary>
        /// 计算 opoint 对象的屏幕位置（返回底部中心点）
        /// 基于 FLF 源码：mech.prototype.make_point (LF/mechanics.js:223-243)
        /// </summary>
        /// <remarks>
        /// FLF 公式：
        /// 朝右: x = ps.sx + opoint.x, y = ps.sy + opoint.y
        /// 朝左: x = ps.sx + sp.w - opoint.x, y = ps.sy + opoint.y
        ///
        /// Unity 对照：
        /// - characterRect.x = 精灵左边缘 (对应 ps.sx)
        /// - characterRect.y = 精灵顶部 (对应 ps.sy)
        /// - characterRect.width = 精灵宽度 (对应 sp.w)
        ///
        /// 返回值：对象的底部中心点位置（与 LF2/FLF 一致，opoint.x/y 指的是脚底位置）
        /// </remarks>
        private Vector2 CalculateOpointPosition(ObjectPoint opoint, Rect characterRect, Sprite characterSprite)
        {
            // opoint.x/y 已经是像素坐标，不需要再乘以 PIXEL_SCALE
            // PIXEL_SCALE 应该用于整体预览区域的缩放
            float offsetX = opoint.x * PIXEL_SCALE;
            float offsetY = opoint.y * PIXEL_SCALE;

            // TODO: 需要从角色状态获取真实朝向
            // 当前假设朝右，后续需要根据角色的 ps.dir 或其他状态判断
            bool facingRight = true;

            float objX, objY;
            if (facingRight)
            {
                // FLF: x = ps.sx + opoint.x
                objX = characterRect.x + offsetX;
            }
            else
            {
                // FLF: x = ps.sx + sp.w - opoint.x
                // characterRect.width 对应 sp.w（精灵宽度）
                objX = characterRect.x + characterRect.width * PIXEL_SCALE - offsetX;
            }

            // FLF: y = ps.sy + opoint.y
            // Y 轴向下为正，与 LF2 一致
            objY = characterRect.y + offsetY;

            return new Vector2(objX, objY);
        }

        /// <summary>
        /// 绘制速度矢量
        /// </summary>
        private void DrawVelocityVector(ObjectPoint opoint, Vector2 center)
        {
            if (opoint.dvx == 0 && opoint.dvy == 0)
                return;

            Vector2 velocityDir = new Vector2(opoint.dvx, -opoint.dvy).normalized; // y轴反转
            Vector2 arrowStart = center;
            Vector2 arrowEnd = arrowStart + velocityDir * 40f;

            Handles.color = Color.yellow;
            Handles.DrawLine(arrowStart, arrowEnd);
            Handles.DrawSolidDisc(arrowEnd, Vector3.forward, 3f);

            // 绘制速度文本
            GUIStyle velocityStyle = new GUIStyle(EditorStyles.miniLabel);
            velocityStyle.normal.textColor = Color.yellow;
            GUI.Label(new Rect(arrowEnd.x + 5, arrowEnd.y - 10, 100, 20),
                $"({opoint.dvx},{opoint.dvy})", velocityStyle);
        }

        /// <summary>
        /// 绘制标签
        /// </summary>
        private void DrawOpointLabel(ObjectPoint opoint, Rect rect, Color color, string customText = null)
        {
            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
            labelStyle.normal.textColor = color;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.fontSize = 9;

            string labelText = customText ?? $"oid:{opoint.oid}";
            GUI.Label(rect, labelText, labelStyle);
        }

        /// <summary>
        /// 绘制预览信息
        /// </summary>
        private void DrawPreviewInfo(Rect previewRect, Sprite sprite, LF2FrameData frameData)
        {
            GUIStyle infoStyle = new GUIStyle(EditorStyles.miniLabel);
            infoStyle.normal.textColor = Color.white;

            bool hasOpoint = frameData.opoint != null && frameData.opoint.oid > 0;
            string opointInfo = hasOpoint ? $"oid:{frameData.opoint.oid}" : "无";

            string info = $"精灵: {sprite.name} | 尺寸: {sprite.rect.width}x{sprite.rect.height} | " +
                         $"帧: {frameData.frameId} | opoint: {opointInfo}";

            Rect infoRect = new Rect(previewRect.x + 5, previewRect.y + 5, previewRect.width - 10, 20);
            EditorGUI.DrawRect(infoRect, new Color(0, 0, 0, 0.7f));
            GUI.Label(infoRect, info, infoStyle);
        }

        // ==================== 帧信息标签页 ====================

        [TabGroup("Main", "📊 帧信息")]
        [FoldoutGroup("Main/📊 帧信息/🎯 基本参数", expanded: true)]
        [HorizontalGroup("Main/📊 帧信息/🎯 基本参数/Row1")]
        [ShowInInspector, ReadOnly]
        [LabelText("帧ID")]
        [LabelWidth(80)]
        private int FrameId => GetCurrentFrameData()?.frameId ?? -1;

        [FoldoutGroup("Main/📊 帧信息/🎯 基本参数")]
        [HorizontalGroup("Main/📊 帧信息/🎯 基本参数/Row1")]
        [ShowInInspector, ReadOnly]
        [LabelText("帧名称")]
        [LabelWidth(80)]
        private string FrameName => GetCurrentFrameData()?.frameName ?? "N/A";

        [FoldoutGroup("Main/📊 帧信息/🎯 基本参数")]
        [HorizontalGroup("Main/📊 帧信息/🎯 基本参数/Row1")]
        [ShowInInspector, ReadOnly]
        [LabelText("状态")]
        [LabelWidth(80)]
        private int FrameState => GetCurrentFrameData()?.state ?? 0;

        [FoldoutGroup("Main/📊 帧信息/🎯 基本参数")]
        [HorizontalGroup("Main/📊 帧信息/🎯 基本参数/Row2")]
        [ShowInInspector, ReadOnly]
        [LabelText("图片索引")]
        [LabelWidth(80)]
        private int FramePic => GetCurrentFrameData()?.pic ?? 0;

        [FoldoutGroup("Main/📊 帧信息/🎯 基本参数")]
        [HorizontalGroup("Main/📊 帧信息/🎯 基本参数/Row2")]
        [ShowInInspector, ReadOnly]
        [LabelText("等待时间")]
        [LabelWidth(80)]
        private int FrameWait => GetCurrentFrameData()?.wait ?? 0;

        [FoldoutGroup("Main/📊 帧信息/🎯 基本参数")]
        [HorizontalGroup("Main/📊 帧信息/🎯 基本参数/Row2")]
        [ShowInInspector, ReadOnly]
        [LabelText("下一帧")]
        [LabelWidth(80)]
        private int FrameNext => GetCurrentFrameData()?.next ?? 0;

        [FoldoutGroup("Main/📊 帧信息/🎯 基本参数")]
        [HorizontalGroup("Main/📊 帧信息/🎯 基本参数/Row3")]
        [ShowInInspector, ReadOnly]
        [LabelText("速度 X")]
        [LabelWidth(80)]
        private int FrameDvx => GetCurrentFrameData()?.dvx ?? 0;

        [FoldoutGroup("Main/📊 帧信息/🎯 基本参数")]
        [HorizontalGroup("Main/📊 帧信息/🎯 基本参数/Row3")]
        [ShowInInspector, ReadOnly]
        [LabelText("速度 Y")]
        [LabelWidth(80)]
        private int FrameDvy => GetCurrentFrameData()?.dvy ?? 0;

        [FoldoutGroup("Main/📊 帧信息/🎯 基本参数")]
        [HorizontalGroup("Main/📊 帧信息/🎯 基本参数/Row3")]
        [ShowInInspector, ReadOnly]
        [LabelText("速度 Z")]
        [LabelWidth(80)]
        private int FrameDvz => GetCurrentFrameData()?.dvz ?? 0;

        [FoldoutGroup("Main/📊 帧信息/🎯 基本参数")]
        [HorizontalGroup("Main/📊 帧信息/🎯 基本参数/Row4")]
        [ShowInInspector, ReadOnly]
        [LabelText("中心 X")]
        [LabelWidth(80)]
        private int FrameCenterx => GetCurrentFrameData()?.centerx ?? 0;

        [FoldoutGroup("Main/📊 帧信息/🎯 基本参数")]
        [HorizontalGroup("Main/📊 帧信息/🎯 基本参数/Row4")]
        [ShowInInspector, ReadOnly]
        [LabelText("中心 Y")]
        [LabelWidth(80)]
        private int FrameCentery => GetCurrentFrameData()?.centery ?? 0;

        [FoldoutGroup("Main/📊 帧信息/🎯 基本参数")]
        [HorizontalGroup("Main/📊 帧信息/🎯 基本参数/Row4")]
        [ShowInInspector, ReadOnly]
        [LabelText("MP 消耗")]
        [LabelWidth(80)]
        private int FrameMp => GetCurrentFrameData()?.mp ?? 0;

        [FoldoutGroup("Main/📊 帧信息/🎯 基本参数")]
        [ShowInInspector, ReadOnly]
        [LabelText("声音")]
        [LabelWidth(80)]
        private string FrameSound => GetCurrentFrameData()?.sound ?? "";

        // ==================== 按键响应 ====================

        [TabGroup("Main", "📊 帧信息")]
        [FoldoutGroup("Main/📊 帧信息/⌨️ 按键响应", expanded: false)]
        [HorizontalGroup("Main/📊 帧信息/⌨️ 按键响应/Row1")]
        [ShowInInspector, ReadOnly]
        [LabelText("A")]
        [LabelWidth(40)]
        private int Hit_a => GetCurrentFrameData()?.hit_a ?? 0;

        [FoldoutGroup("Main/📊 帧信息/⌨️ 按键响应")]
        [HorizontalGroup("Main/📊 帧信息/⌨️ 按键响应/Row1")]
        [ShowInInspector, ReadOnly]
        [LabelText("D")]
        [LabelWidth(40)]
        private int Hit_d => GetCurrentFrameData()?.hit_d ?? 0;

        [FoldoutGroup("Main/📊 帧信息/⌨️ 按键响应")]
        [HorizontalGroup("Main/📊 帧信息/⌨️ 按键响应/Row1")]
        [ShowInInspector, ReadOnly]
        [LabelText("J")]
        [LabelWidth(40)]
        private int Hit_j => GetCurrentFrameData()?.hit_j ?? 0;

        [FoldoutGroup("Main/📊 帧信息/⌨️ 按键响应")]
        [HorizontalGroup("Main/📊 帧信息/⌨️ 按键响应/Row2")]
        [ShowInInspector, ReadOnly]
        [LabelText("F+J")]
        [LabelWidth(40)]
        private int Hit_Fj => GetCurrentFrameData()?.hit_Fj ?? 0;

        [FoldoutGroup("Main/📊 帧信息/⌨️ 按键响应")]
        [HorizontalGroup("Main/📊 帧信息/⌨️ 按键响应/Row2")]
        [ShowInInspector, ReadOnly]
        [LabelText("F+A")]
        [LabelWidth(40)]
        private int Hit_Fa => GetCurrentFrameData()?.hit_Fa ?? 0;

        [FoldoutGroup("Main/📊 帧信息/⌨️ 按键响应")]
        [HorizontalGroup("Main/📊 帧信息/⌨️ 按键响应/Row2")]
        [ShowInInspector, ReadOnly]
        [LabelText("D+A")]
        [LabelWidth(40)]
        private int Hit_Da => GetCurrentFrameData()?.hit_Da ?? 0;

        [FoldoutGroup("Main/📊 帧信息/⌨️ 按键响应")]
        [HorizontalGroup("Main/📊 帧信息/⌨️ 按键响应/Row3")]
        [ShowInInspector, ReadOnly]
        [LabelText("U+A")]
        [LabelWidth(40)]
        private int Hit_Ua => GetCurrentFrameData()?.hit_Ua ?? 0;

        [FoldoutGroup("Main/📊 帧信息/⌨️ 按键响应")]
        [HorizontalGroup("Main/📊 帧信息/⌨️ 按键响应/Row3")]
        [ShowInInspector, ReadOnly]
        [LabelText("J+A")]
        [LabelWidth(40)]
        private int Hit_ja => GetCurrentFrameData()?.hit_ja ?? 0;

        [FoldoutGroup("Main/📊 帧信息/⌨️ 按键响应")]
        [HorizontalGroup("Main/📊 帧信息/⌨️ 按键响应/Row3")]
        [ShowInInspector, ReadOnly]
        [LabelText("D+J")]
        [LabelWidth(40)]
        private int Hit_Dj => GetCurrentFrameData()?.hit_Dj ?? 0;

        [FoldoutGroup("Main/📊 帧信息/⌨️ 按键响应")]
        [HorizontalGroup("Main/📊 帧信息/⌨️ 按键响应/Row3")]
        [ShowInInspector, ReadOnly]
        [LabelText("U+J")]
        [LabelWidth(40)]
        private int Hit_Uj => GetCurrentFrameData()?.hit_Uj ?? 0;

        // ==================== 碰撞盒 ====================

        [TabGroup("Main", "📊 帧信息")]
        [FoldoutGroup("Main/📊 帧信息/🔵 碰撞盒 (bdy)", expanded: true)]
        [ShowInInspector, ReadOnly]
        [LabelText("碰撞盒列表")]
        [ListDrawerSettings(ShowIndexLabels = true, ShowPaging = false, DraggableItems = false)]
        private List<BodyBox> FrameBodies => GetCurrentFrameData()?.bodies ?? new List<BodyBox>();

        // ==================== 攻击判定 ====================

        [TabGroup("Main", "📊 帧信息")]
        [FoldoutGroup("Main/📊 帧信息/🔴 攻击判定 (itr)", expanded: true)]
        [ShowInInspector, ReadOnly]
        [LabelText("攻击判定列表")]
        [ListDrawerSettings(ShowIndexLabels = true, ShowPaging = false, DraggableItems = false)]
        private List<InteractionArea> FrameItrs => GetCurrentFrameData()?.itrs ?? new List<InteractionArea>();

        // ==================== 武器点 ====================

        [TabGroup("Main", "📊 帧信息")]
        [FoldoutGroup("Main/📊 帧信息/⚔️ 武器点 (wpoint)", expanded: false)]
        [ShowInInspector, ReadOnly]
        [LabelText("武器点列表")]
        [ListDrawerSettings(ShowIndexLabels = true, ShowPaging = false, DraggableItems = false)]
        private List<WeaponPoint> FrameWpoints => GetCurrentFrameData()?.wpoints ?? new List<WeaponPoint>();

        // ==================== 对象点 ====================

        [TabGroup("Main", "📊 帧信息")]
        [FoldoutGroup("Main/📊 帧信息/🎯 对象点 (opoint)", expanded: true)]
        [ShowInInspector, ReadOnly]
        [LabelText("对象点数据")]
        [HideIf("@GetCurrentFrameData()?.opoint == null")]
        private ObjectPoint FrameOpoint => GetCurrentFrameData()?.opoint;

        [FoldoutGroup("Main/📊 帧信息/🎯 对象点 (opoint)")]
        [ShowInInspector, ReadOnly]
        [LabelText("播放状态")]
        [MultiLineProperty(5)]
        [HideIf("@activeOpointObjects.Count == 0")]
        private string OpointPlaybackInfo
        {
            get
            {
                if (activeOpointObjects == null || activeOpointObjects.Count == 0)
                    return "无活跃对象";

                var statusList = new System.Text.StringBuilder();
                statusList.AppendLine($"活跃对象数量: {activeOpointObjects.Count}");

                for (int i = 0; i < activeOpointObjects.Count; i++)
                {
                    var opointState = activeOpointObjects[i];
                    string status = opointState.isPlaying
                        ? $"播放中 - 帧={opointState.currentFrameId}, wait={opointState.waitCount}, next={opointState.currentFrame?.next ?? 0}"
                        : "已停止";
                    statusList.AppendLine($"  [{i}] oid={opointState.oid}: {status}");
                }

                return statusList.ToString();
            }
        }

        // ==================== 血点 ====================

        [TabGroup("Main", "📊 帧信息")]
        [FoldoutGroup("Main/📊 帧信息/🩸 血点 (bpoint)", expanded: false)]
        [ShowInInspector, ReadOnly]
        [LabelText("血点数据")]
        [HideIf("@GetCurrentFrameData()?.bpoint == null")]
        private BloodPoint FrameBpoint => GetCurrentFrameData()?.bpoint;

        // ==================== 抓取点 ====================

        [TabGroup("Main", "📊 帧信息")]
        [FoldoutGroup("Main/📊 帧信息/🤜 抓取点 (cpoint)", expanded: false)]
        [ShowInInspector, ReadOnly]
        [LabelText("抓取点数据")]
        [HideIf("@GetCurrentFrameData()?.cpoint == null")]
        private CatchPoint FrameCpoint => GetCurrentFrameData()?.cpoint;

        [PropertySpace(10)]
        [TabGroup("Main", "⚙️ 设置")]
        [BoxGroup("Main/⚙️ 设置/播放速度")]
        [LabelText("播放速度倍率")]
        [Range(0.1f, 5f)]
        [OnValueChanged("Repaint")]
        public float playbackSpeed = 1f;

        [BoxGroup("Main/⚙️ 设置/播放速度")]
        [LabelText("FPS (每秒帧数)")]
        [ReadOnly]
        [ShowInInspector]
        [DisplayAsString(false)]
        [SuffixLabel("fps", true)]
        private float CurrentFPS => 30f * playbackSpeed;

        #endregion

        #region 内部状态

        private int currentPlayingFrameId = 0;
        private double lastUpdateTime = 0;
        private const double TARGET_FRAME_TIME = 1.0 / 30.0; // 30 FPS

        #endregion

        #region 初始化

        protected override void OnEnable()
        {
            base.OnEnable();
            EditorApplication.update += OnEditorUpdate;
            ResetToStartFrame();
            CheckDataAvailability();
            LoadDataFile();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EditorApplication.update -= OnEditorUpdate;
            StopPlayback();
        }

        /// <summary>
        /// 检查数据是否可用
        /// </summary>
        private void CheckDataAvailability()
        {
            if (CharacterAnimtorManager.Instance == null)
            {
                Debug.LogWarning("<color=yellow>⚠️ CharacterAnimtorManager 未找到！请确保场景中有 CharacterAnimtorManager 对象。</color>");
                return;
            }

            var ids = CharacterAnimtorManager.Instance.GetAllLoadedCharacterIds();
            if (ids == null || ids.Count == 0)
            {
                Debug.LogWarning("<color=yellow>⚠️ 角色数据未加载！\n" +
                    "请在场景中找到 CharacterAnimtorManager 对象，\n" +
                    "在 Inspector 中点击【刷新所有数据】按钮来加载角色配置。</color>");
            }
            else
            {
                Debug.Log($"<color=green>✅ 已加载 {ids.Count} 个角色的数据</color>");
            }
        }

        /// <summary>
        /// 加载 data.txt 文件
        /// </summary>
        private void LoadDataFile()
        {
            string dataFilePath = "Assets/NTSD/Config/data.txt";
            string fullPath = System.IO.Path.GetFullPath(dataFilePath);

            if (!System.IO.File.Exists(fullPath))
            {
                Debug.LogWarning($"<color=yellow>⚠️ data.txt 文件不存在: {fullPath}</color>");
                return;
            }

            dataObjectMap = DataFileParser.ParseDataFile(fullPath);
        }

        #endregion

        #region 核心播放逻辑

        /// <summary>
        /// 开始播放（从当前帧开始）
        /// </summary>
        private void StartPlayback(bool loop)
        {
            isPlaying = true;
            isLooping = loop;
            // 从当前帧开始播放，而不是从起始帧
            currentWaitCount = GetCurrentFrameData()?.wait ?? 0;
            lastUpdateTime = EditorApplication.timeSinceStartup;

            Debug.Log($"<color=green>开始播放：角色ID={selectedCharacterId}, 当前帧={currentPlayingFrameId}, 循环={loop}</color>");
        }

        /// <summary>
        /// 手动切换到下一帧
        /// </summary>
        private void ManualNextFrame()
        {
            var currentFrame = GetCurrentFrameData();
            if (currentFrame == null)
            {
                Debug.LogWarning($"未找到帧数据：角色ID={selectedCharacterId}, 帧ID={currentPlayingFrameId}");
                return;
            }

            int nextFrameId = currentFrame.next;

            // 检查是否到达终点
            if (IsEndFrame(nextFrameId))
            {
                Debug.Log($"<color=cyan>已到达终点帧：next={nextFrameId}，回到起始帧={startFrameId}</color>");
                currentPlayingFrameId = startFrameId;
                currentWaitCount = GetCurrentFrameData()?.wait ?? 0;
            }
            else
            {
                // 切换到下一帧
                currentPlayingFrameId = nextFrameId;
                var newFrame = GetCurrentFrameData();
                currentWaitCount = newFrame?.wait ?? 0;

                Debug.Log($"<color=gray>手动切换帧：{currentPlayingFrameId} (wait={currentWaitCount}, next={newFrame?.next})</color>");

                // 检查新帧是否有 opoint，如果有则创建新的 opoint 对象
                // 与自动播放保持一致（FrameUpdate 中的逻辑）
                if (newFrame != null && newFrame.opoint != null && newFrame.opoint.oid > 0)
                {
                    CreateOpointObject(newFrame.opoint);
                }
            }

            // 更新所有 opoint 对象的帧（与自动播放保持一致）
            UpdateOpointFrame();

            Repaint();
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        private void StopPlayback()
        {
            isPlaying = false;

            // 清空所有 opoint 对象
            if (activeOpointObjects.Count > 0)
            {
                Debug.Log($"<color=yellow>清空所有 opoint 对象，共 {activeOpointObjects.Count} 个</color>");
                activeOpointObjects.Clear();
            }

            Debug.Log($"<color=yellow>停止播放：当前帧={currentPlayingFrameId}</color>");
        }

        /// <summary>
        /// 重置到起始帧
        /// </summary>
        private void ResetToStartFrame()
        {
            StopPlayback();
            currentPlayingFrameId = startFrameId;
            currentWaitCount = 0;
            Repaint();
        }

        /// <summary>
        /// Editor Update - 实现帧更新逻辑（完全复刻 LF2CharacterAnimator 的逻辑）
        /// </summary>
        private void OnEditorUpdate()
        {
            if (!isPlaying) return;

            double currentTime = EditorApplication.timeSinceStartup;
            double deltaTime = currentTime - lastUpdateTime;

            // 应用播放速度
            double targetTime = TARGET_FRAME_TIME / playbackSpeed;

            if (deltaTime >= targetTime)
            {
                lastUpdateTime = currentTime;
                UpdateFrame();
                Repaint();
            }
        }

        /// <summary>
        /// 更新帧（对应 LF2CharacterAnimator.TU_Update）
        /// 基于 FLF 源码：livingobject.prototype.TU_update (LF/livingobject.js:200-333)
        /// </summary>
        private void UpdateFrame()
        {
            // 1. 更新角色帧
            if (currentWaitCount > 0)
            {
                // 等待计时器递减
                currentWaitCount--;
            }
            else
            {
                // wait=0 时触发帧更新（对应 Frame_Update）
                FrameUpdate();
            }

            // 2. 更新 opoint 对象帧（模拟 specialattack.prototype.TU_update）
            UpdateOpointFrame();
        }

        /// <summary>
        /// 帧更新 - 切换到下一帧（对应 LF2CharacterAnimator.Frame_Update）
        /// </summary>
        private void FrameUpdate()
        {
            var currentFrame = GetCurrentFrameData();
            if (currentFrame == null)
            {
                Debug.LogWarning($"未找到帧数据：角色ID={selectedCharacterId}, 帧ID={currentPlayingFrameId}");
                StopPlayback();
                return;
            }

            int nextFrameId = currentFrame.next;

            // 检查是否到达终点（next为0, 999, 1000时停止）
            if (IsEndFrame(nextFrameId))
            {
                Debug.Log($"<color=cyan>到达终点帧：next={nextFrameId}</color>");

                if (isLooping)
                {
                    // 循环模式：回到起始帧
                    Debug.Log($"<color=cyan>循环：回到起始帧={startFrameId}</color>");
                    currentPlayingFrameId = startFrameId;
                    currentWaitCount = GetCurrentFrameData()?.wait ?? 0;
                }
                else
                {
                    // 非循环模式：停止播放
                    StopPlayback();
                }
            }
            else
            {
                // 切换到下一帧
                currentPlayingFrameId = nextFrameId;

                // 重新设置等待时间
                var newFrame = GetCurrentFrameData();
                currentWaitCount = newFrame?.wait ?? 0;

                Debug.Log($"<color=gray>帧切换：{currentPlayingFrameId} → wait={currentWaitCount}, next={newFrame?.next}</color>");

                // 检查新帧是否有 opoint，如果有则创建新的 opoint 对象
                // 基于 FLF 源码：character.prototype.opoint (LF/character.js:2339-2380)
                if (newFrame != null && newFrame.opoint != null && newFrame.opoint.oid > 0)
                {
                    CreateOpointObject(newFrame.opoint);
                }
            }
        }

        /// <summary>
        /// 判断是否为终止帧
        /// </summary>
        private bool IsEndFrame(int nextId)
        {
            return nextId == 0 || nextId == 999 || nextId == 1000;
        }

        #endregion

        #region 数据获取

        /// <summary>
        /// 获取当前播放帧的数据
        /// </summary>
        private LF2FrameData GetCurrentFrameData()
        {
            return CharacterAnimtorManager.Instance?.GetFrameData(selectedCharacterId, currentPlayingFrameId);
        }

        /// <summary>
        /// 获取已加载的角色ID列表
        /// </summary>
        private IEnumerable<int> GetLoadedCharacterIds()
        {
            if (CharacterAnimtorManager.Instance == null)
                return new List<int>();

            return CharacterAnimtorManager.Instance.GetAllLoadedCharacterIds().OrderBy(x => x);
        }

        /// <summary>
        /// 加载 opoint 对象的数据
        /// ⚠️ 优先从 CharacterAnimtorManager 获取已加载的数据，避免重复解析
        /// </summary>
        private LF2CharacterDataWrapper LoadObjectData(int oid)
        {
            // 1. 检查本地缓存
            if (objectDataCache.ContainsKey(oid))
            {
                return objectDataCache[oid];
            }

            // 2. 优先从 CharacterAnimtorManager 获取已加载的数据
            if (CharacterAnimtorManager.Instance != null)
            {
                var wrapper = CharacterAnimtorManager.Instance.GetCharacterConfig(oid);
                if (wrapper != null)
                {
                    // 存入本地缓存
                    objectDataCache[oid] = wrapper;
                    Debug.Log($"<color=green>✅ 从 CharacterAnimtorManager 获取对象数据: oid={oid}, 帧数={wrapper.characterData.frames.Count}</color>");
                    return wrapper;
                }
            }

            // 3. 如果 CharacterAnimtorManager 中没有，提示用户刷新数据
            Debug.LogWarning($"<color=yellow>⚠️ CharacterAnimtorManager 未加载 oid={oid} 的数据</color>");
            Debug.LogWarning($"<color=yellow>   请在 CharacterAnimtorManager 中点击【刷新所有数据】按钮</color>");
            return null;
        }

        /// <summary>
        /// 获取对象精灵
        /// </summary>
        private Sprite GetObjectSprite(int oid, int picIndex)
        {
            // 先检查缓存
            if (objectSpritesCache.ContainsKey(oid))
            {
                var sprites = objectSpritesCache[oid];
                if (picIndex >= 0 && picIndex < sprites.Count)
                {
                    return sprites[picIndex];
                }
                else
                {
                    Debug.LogWarning($"<color=yellow>⚠️ 对象精灵索引超出范围: oid={oid}, pic={picIndex}, 总数={sprites.Count}</color>");
                    return null;
                }
            }

            // 如果没有缓存，加载对象精灵
            LoadObjectSprites(oid);

            // 再次尝试从缓存获取
            if (objectSpritesCache.ContainsKey(oid))
            {
                var sprites = objectSpritesCache[oid];
                if (picIndex >= 0 && picIndex < sprites.Count)
                {
                    return sprites[picIndex];
                }
            }

            return null;
        }

        /// <summary>
        /// 加载对象的所有精灵
        /// ⚠️ 优先从 CharacterAnimtorManager 获取已加载的精灵，避免重复加载
        /// </summary>
        private void LoadObjectSprites(int oid)
        {
            // 1. 优先从 CharacterAnimtorManager 获取已加载的精灵
            if (CharacterAnimtorManager.Instance != null)
            {
                var sprites = CharacterAnimtorManager.Instance.GetCharacterSpriteByID(oid);
                if (sprites != null && sprites.Count > 0)
                {
                    // 存储到缓存
                    objectSpritesCache[oid] = sprites;
                    Debug.Log($"<color=green>✅ 从 CharacterAnimtorManager 获取对象精灵: oid={oid}, 总数={sprites.Count}</color>");
                    return;
                }
            }

            // 2. 如果 CharacterAnimtorManager 中没有，提示用户刷新数据
            Debug.LogWarning($"<color=yellow>⚠️ CharacterAnimtorManager 未加载 oid={oid} 的精灵数据</color>");
            Debug.LogWarning($"<color=yellow>   请在 CharacterAnimtorManager 中点击【刷新所有数据】按钮</color>");
        }

        #endregion

        #region 事件回调

        /// <summary>
        /// 角色切换回调
        /// </summary>
        private void OnCharacterChanged()
        {
            StopPlayback();
            selectedStateFilter = null;  // 清空状态筛选
            startFrameId = 0;
            currentPlayingFrameId = 0;
            Debug.Log($"切换角色：ID={selectedCharacterId}, 名称={CharacterName}");
        }

        /// <summary>
        /// 状态筛选切换回调
        /// </summary>
        private void OnStateFilterChanged()
        {
            // 当状态筛选改变时，重置起始帧为筛选后列表的第一个帧
            var availableFrames = GetAvailableFrameIds();
            if (availableFrames.Any())
            {
                startFrameId = availableFrames.First().Value;
            }

            if (selectedStateFilter.HasValue)
            {
                Debug.Log($"<color=cyan>状态筛选已设置: 状态={selectedStateFilter.Value}</color>");
            }
        }

        /// <summary>
        /// 起始帧切换回调
        /// </summary>
        private void OnStartFrameChanged()
        {
            if (!isPlaying)
            {
                currentPlayingFrameId = startFrameId;
            }
        }

        /// <summary>
        /// 获取可用的帧ID列表（支持状态筛选）
        /// </summary>
        private IEnumerable<ValueDropdownItem<int>> GetAvailableFrameIds()
        {
            var frames = CharacterAnimtorManager.Instance?.GetCharacterFrames(selectedCharacterId);
            if (frames == null || frames.Count == 0)
            {
                return new List<ValueDropdownItem<int>>
                {
                    new ValueDropdownItem<int>("无可用帧", 0)
                };
            }

            // 根据状态筛选过滤帧
            IEnumerable<LF2FrameData> filteredFrames = frames;
            if (selectedStateFilter.HasValue)
            {
                filteredFrames = frames.Where(f => f.state == selectedStateFilter.Value);
            }

            var result = filteredFrames
                .OrderBy(f => f.frameId)
                .Select(f => new ValueDropdownItem<int>(
                    $"帧 {f.frameId}: {f.frameName} (状态={f.state})",
                    f.frameId
                ))
                .ToList();

            // 如果筛选后没有帧，返回提示
            if (result.Count == 0)
            {
                return new List<ValueDropdownItem<int>>
                {
                    new ValueDropdownItem<int>($"状态 {selectedStateFilter.Value} 无可用帧", 0)
                };
            }

            return result;
        }

        /// <summary>
        /// 获取可用的状态列表
        /// </summary>
        private IEnumerable<ValueDropdownItem<int?>> GetAvailableStates()
        {
            var frames = CharacterAnimtorManager.Instance?.GetCharacterFrames(selectedCharacterId);
            if (frames == null || frames.Count == 0)
            {
                return new List<ValueDropdownItem<int?>>
                {
                    new ValueDropdownItem<int?>("无可用状态", null)
                };
            }

            // 添加"全部"选项
            var result = new List<ValueDropdownItem<int?>>
            {
                new ValueDropdownItem<int?>("🔍 全部状态", null)
            };

            // 获取所有唯一状态并排序
            var states = frames
                .Select(f => f.state)
                .Distinct()
                .OrderBy(s => s)
                .Select(s =>
                {
                    // 统计该状态的帧数量
                    int count = frames.Count(f => f.state == s);
                    return new ValueDropdownItem<int?>($"状态 {s} ({count} 帧)", s);
                });

            result.AddRange(states);
            return result;
        }

        #endregion

        #region 调试工具

        [TabGroup("Main", "🔧 调试")]
        [BoxGroup("Main/🔧 调试/帧调试")]
        [ResponsiveButtonGroup("Main/🔧 调试/帧调试/Buttons")]
        [Button("🖼️ 检查精灵状态", ButtonSizes.Medium)]
        [GUIColor(1f, 0.7f, 0.9f)]
        private void CheckSpriteStatus()
        {
            var sprites = CharacterAnimtorManager.Instance?.GetCharacterSpriteByID(selectedCharacterId);
            var frameData = GetCurrentFrameData();

            if (sprites == null)
            {
                Debug.LogError($"<color=red>❌ 精灵列表为 null！角色ID={selectedCharacterId}</color>");
                return;
            }

            if (sprites.Count == 0)
            {
                Debug.LogError($"<color=red>❌ 精灵列表为空！角色ID={selectedCharacterId}</color>");
                return;
            }

            if (frameData == null)
            {
                Debug.LogError($"<color=red>❌ 帧数据为 null！角色ID={selectedCharacterId}, 帧ID={currentPlayingFrameId}</color>");
                return;
            }

            Debug.Log($"<color=cyan>===== 精灵状态检查 =====</color>");
            Debug.Log($"<color=yellow>角色ID:</color> {selectedCharacterId}");
            Debug.Log($"<color=yellow>精灵总数:</color> {sprites.Count}");
            Debug.Log($"<color=yellow>当前帧ID:</color> {currentPlayingFrameId}");
            Debug.Log($"<color=yellow>帧数据 pic:</color> {frameData.pic}");

            if (frameData.pic < 0 || frameData.pic >= sprites.Count)
            {
                Debug.LogError($"<color=red>❌ pic 索引超出范围！pic={frameData.pic}, 总数={sprites.Count}</color>");
            }
            else
            {
                var sprite = sprites[frameData.pic];
                if (sprite == null)
                {
                    Debug.LogError($"<color=red>❌ sprites[{frameData.pic}] 为 null！</color>");
                    Debug.LogWarning($"<color=yellow>提示：检查 BMP 文件定义的帧范围是否包含 pic={frameData.pic}</color>");

                    // 查找第一个非 null 的精灵
                    for (int i = 0; i < sprites.Count; i++)
                    {
                        if (sprites[i] != null)
                        {
                            Debug.Log($"<color=green>✅ sprites[{i}] 有精灵：{sprites[i].name}</color>");
                            break;
                        }
                    }
                }
                else
                {
                    Debug.Log($"<color=green>✅ 精灵正常！</color>");
                    Debug.Log($"<color=yellow>精灵名称:</color> {sprite.name}");
                    Debug.Log($"<color=yellow>精灵尺寸:</color> {sprite.rect.width}x{sprite.rect.height}");
                    Debug.Log($"<color=yellow>纹理尺寸:</color> {sprite.texture.width}x{sprite.texture.height}");
                }
            }
        }

        [ResponsiveButtonGroup("Main/🔧 调试/帧调试/Buttons")]
        [Button("📋 打印当前帧", ButtonSizes.Medium)]
        [GUIColor(0.7f, 0.9f, 1f)]
        private void PrintCurrentFrameInfo()
        {
            var frame = GetCurrentFrameData();
            if (frame != null)
            {
                Debug.Log($"<color=cyan>===== 帧信息 =====</color>\n" +
                         $"<color=yellow>帧ID:</color> {frame.frameId}\n" +
                         $"<color=yellow>帧名:</color> {frame.frameName}\n" +
                         $"<color=yellow>状态:</color> {frame.state}\n" +
                         $"<color=yellow>图片:</color> {frame.pic}\n" +
                         $"<color=yellow>等待:</color> {frame.wait}\n" +
                         $"<color=yellow>下一帧:</color> {frame.next}\n" +
                         $"<color=yellow>碰撞盒:</color> {frame.bodies?.Count ?? 0}\n" +
                         $"<color=yellow>攻击判定:</color> {frame.itrs?.Count ?? 0}\n" +
                         $"<color=yellow>MP:</color> {frame.mp}");
            }
            else
            {
                Debug.LogWarning($"未找到帧：角色ID={selectedCharacterId}, 帧ID={currentPlayingFrameId}");
            }
        }

        [ResponsiveButtonGroup("Main/🔧 调试/帧调试/Buttons")]
        [Button("🗺️ 打印播放路径", ButtonSizes.Medium)]
        [GUIColor(0.7f, 1f, 0.9f)]
        private void PrintPlaybackPath()
        {
            List<int> path = new List<int>();
            HashSet<int> visited = new HashSet<int>();
            int frameId = startFrameId;
            int maxSteps = 100; // 防止死循环

            while (maxSteps-- > 0)
            {
                if (visited.Contains(frameId))
                {
                    Debug.Log($"<color=yellow>检测到循环：帧{frameId}已访问过</color>");
                    break;
                }

                visited.Add(frameId);
                path.Add(frameId);

                var frame = CharacterAnimtorManager.Instance?.GetFrameData(selectedCharacterId, frameId);
                if (frame == null)
                {
                    Debug.LogWarning($"未找到帧：{frameId}");
                    break;
                }

                int nextId = frame.next;
                if (IsEndFrame(nextId))
                {
                    Debug.Log($"<color=green>到达终点：next={nextId}</color>");
                    break;
                }

                frameId = nextId;
            }

            string pathString = string.Join(" → ", path);
            Debug.Log($"<color=cyan>播放路径 ({path.Count}帧):</color>\n{pathString}");
        }

        [ResponsiveButtonGroup("Main/🔧 调试/帧调试/Buttons")]
        [Button("📝 列出所有状态", ButtonSizes.Medium)]
        [GUIColor(1f, 0.9f, 0.7f)]
        private void ListAllStateFrames()
        {
            var frames = CharacterAnimtorManager.Instance?.GetCharacterFrames(selectedCharacterId);
            if (frames == null)
            {
                Debug.LogWarning($"未找到角色帧：ID={selectedCharacterId}");
                return;
            }

            var stateGroups = frames.GroupBy(f => f.state).OrderBy(g => g.Key);
            Debug.Log($"<color=cyan>===== 角色 {selectedCharacterId} 的状态列表 =====</color>");

            foreach (var group in stateGroups)
            {
                var firstFrame = group.First();
                Debug.Log($"<color=yellow>状态 {group.Key}:</color> {firstFrame.frameName} " +
                         $"(帧 {string.Join(", ", group.Select(f => f.frameId))})");
            }
        }

        #endregion
    }
}
#endif
