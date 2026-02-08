using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

namespace NTSD.Test
{
    public class ProCamera2DTestPanel : MonoBehaviour
    {
        [Header("World Boundaries (可活动区域)")]
        public float BoundaryLeft   = -20.69f;
        public float BoundaryRight  =  20.69f;
        public float BoundaryTop    =  7.49f;
        public float BoundaryBottom = -3.91f;

        [Header("Player")]
        public float PlayerSpeed  = 8f;
        public float DashSpeed    = 24f;
        public float DashDuration = 0.15f;

        [Header("Extra Targets")]
        public int ExtraTargetCount = 3;

        [Header("Multi-Target Framing")]
        public float FramingPadding = 2f;
        public float MinOrthoSize   = 2f;
        public float ZoomOutSpeed   = 20f;
        public float ZoomInSpeed    = 0.4f;

        Camera    _cam;
        Transform _playerTransform;
        readonly List<Transform> _allTargets   = new List<Transform>();
        readonly List<Transform> _extraTargets = new List<Transform>();
        int _controlledIndex;

        // Cinemachine
        CinemachineVirtualCamera _vcam;
        CinemachineConfiner2D    _confiner;
        Transform                _followTarget;

        // Dynamic zoom state
        float _maxOrthoSize;
        float _currentOrthoSize;
        float _sizeVelocity;

        // Shake
        bool  _enableShake = true;
        float _shakeTimer;
        float _shakeDuration;
        float _shakeIntensity;

        // Dash
        bool    _isDashing;
        float   _dashTimer;
        Vector2 _dashDir;

        // Hit
        float _hitCooldown;
        float _timeStopTimer;
        float _savedTimeScale = 1f;

        // Input
        InputActionMap _actionMap;
        InputAction    _moveAction;
        InputAction    _dashAction;
        InputAction    _hitAction;
        Vector2        _moveInput;

        // GUI
        bool    _showPanel = true;
        Vector2 _scrollPos;

        // ================================================================
        //  LIFECYCLE
        // ================================================================

        void Awake()
        {
            BuildInputActions();
            BuildCamera();
            BuildCinemachine();
            BuildPlayer();
            BuildExtraTargets();
            BuildBackgroundGrid();
            BuildBoundaryLines();

            float boundsW = BoundaryRight - BoundaryLeft;
            float boundsH = BoundaryTop - BoundaryBottom;
            _maxOrthoSize = (boundsW * 0.5f) / _cam.aspect;
            _currentOrthoSize = boundsH * 0.5f;
        }

        void OnEnable()
        {
            _actionMap?.Enable();
            CinemachineCore.CameraUpdatedEvent.AddListener(OnCinemachineCameraUpdated);
        }

        void OnDisable()
        {
            _actionMap?.Disable();
            CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCinemachineCameraUpdated);
        }

        void OnDestroy()
        {
            if (_moveAction != null)
            {
                _moveAction.performed -= OnMovePerformed;
                _moveAction.canceled  -= OnMoveCanceled;
            }
            _actionMap?.Disable();
            _actionMap?.Dispose();
        }

        void Update()
        {
            UpdateTimeStop();
            HandleTargetSwitch();
            HandleControlledMovement();
            HandleDash();
            HandleHitSimulation();
            UpdateFollowTargetPosition();
        }

        void LateUpdate()
        {
            ApplyShake();
        }

        void OnCinemachineCameraUpdated(CinemachineBrain brain)
        {
            if (_cam == null) return;
            UpdateOrthoSize();
            _cam.orthographicSize = _currentOrthoSize;
            UpdateViewportRect();
        }

        void UpdateViewportRect()
        {
            float camH = _currentOrthoSize * 2f;
            float camW = camH * _cam.aspect;
            float bgW = BoundaryRight - BoundaryLeft;
            float bgH = BoundaryTop - BoundaryBottom;

            // How much of the camera view the background occupies (0~1)
            float ratioW = Mathf.Clamp01(bgW / camW);
            float ratioH = Mathf.Clamp01(bgH / camH);

            // Center the viewport rect on screen
            float rx = (1f - ratioW) * 0.5f;
            float ry = (1f - ratioH) * 0.5f;
            _cam.rect = new Rect(rx, ry, ratioW, ratioH);
        }

        // ================================================================
        //  INPUT
        // ================================================================

        void BuildInputActions()
        {
            _actionMap = new InputActionMap("CamTest");

            _moveAction = _actionMap.AddAction("Move", type: InputActionType.Value);
            _moveAction.expectedControlType = "Vector2";
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up",    "<Keyboard>/w")
                .With("Down",  "<Keyboard>/s")
                .With("Left",  "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up",    "<Keyboard>/upArrow")
                .With("Down",  "<Keyboard>/downArrow")
                .With("Left",  "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            _moveAction.performed += OnMovePerformed;
            _moveAction.canceled  += OnMoveCanceled;

            _dashAction = _actionMap.AddAction("Dash", type: InputActionType.Button);
            _dashAction.AddBinding("<Keyboard>/leftShift");
            _dashAction.AddBinding("<Keyboard>/rightShift");

            _hitAction = _actionMap.AddAction("Hit", type: InputActionType.Button);
            _hitAction.AddBinding("<Keyboard>/space");

            _actionMap.Enable();
        }

        void OnMovePerformed(InputAction.CallbackContext ctx) { _moveInput = ctx.ReadValue<Vector2>(); }
        void OnMoveCanceled(InputAction.CallbackContext ctx)  { _moveInput = Vector2.zero; }

        // ================================================================
        //  CAMERA + CINEMACHINE
        // ================================================================

        void BuildCamera()
        {
            // Background camera: fills entire screen with black, renders nothing
            var bgCamGo = new GameObject("BackgroundCamera");
            bgCamGo.transform.position = new Vector3(0, 0, -100);
            var bgCam = bgCamGo.AddComponent<Camera>();
            bgCam.orthographic = true;
            bgCam.clearFlags = CameraClearFlags.SolidColor;
            bgCam.backgroundColor = Color.black;
            bgCam.cullingMask = 0; // render nothing
            bgCam.depth = -10;

            // Main camera
            var camGo = new GameObject("MainCamera");
            camGo.transform.position = new Vector3(0, 0, -10);
            _cam = camGo.AddComponent<Camera>();
            _cam.orthographic     = true;
            _cam.orthographicSize = 5f;
            _cam.backgroundColor  = new Color(0.12f, 0.12f, 0.18f);
            _cam.clearFlags       = CameraClearFlags.SolidColor;
            _cam.nearClipPlane    = 0.1f;
            _cam.farClipPlane     = 100f;
            _cam.depth            = 0;

            if (Camera.main != null && Camera.main != _cam)
                Destroy(Camera.main.gameObject);
            _cam.tag = "MainCamera";

            var brain = camGo.AddComponent<CinemachineBrain>();
            brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.LateUpdate;
            brain.m_DefaultBlend = new CinemachineBlendDefinition(
                CinemachineBlendDefinition.Style.EaseInOut, 0.5f);
        }

        void BuildCinemachine()
        {
            // Follow target: empty transform whose position = center of all targets
            var followGo = new GameObject("FollowTarget");
            _followTarget = followGo.transform;

            // VirtualCamera with simple Transposer body (just follows the target)
            var vcamGo = new GameObject("CM_VCam");
            _vcam = vcamGo.AddComponent<CinemachineVirtualCamera>();
            _vcam.m_Lens.Orthographic = true;
            _vcam.m_Lens.OrthographicSize = 5f;
            _vcam.Follow = _followTarget;
            _vcam.LookAt = null;

            // Body: Transposer for smooth follow
            var transposer = _vcam.AddCinemachineComponent<CinemachineTransposer>();
            transposer.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
            transposer.m_FollowOffset = new Vector3(0, 0, -10);
            transposer.m_XDamping = 1f;
            transposer.m_YDamping = 1f;
            transposer.m_ZDamping = 0f;

            // Confiner2D: keep camera within boundaries
            _confiner = vcamGo.AddComponent<CinemachineConfiner2D>();
            _confiner.m_BoundingShape2D = CreateBoundaryCollider();
            _confiner.m_Damping = 0f;
            float boundsH = BoundaryTop - BoundaryBottom;
            _confiner.m_MaxWindowSize = boundsH * 0.5f;
        }

        PolygonCollider2D CreateBoundaryCollider()
        {
            var go = new GameObject("CameraBounds");
            go.layer = LayerMask.NameToLayer("Ignore Raycast");
            var col = go.AddComponent<PolygonCollider2D>();
            col.isTrigger = true;
            col.points = new Vector2[]
            {
                new Vector2(BoundaryLeft,  BoundaryBottom),
                new Vector2(BoundaryRight, BoundaryBottom),
                new Vector2(BoundaryRight, BoundaryTop),
                new Vector2(BoundaryLeft,  BoundaryTop),
            };
            return col;
        }

        // ================================================================
        //  MULTI-TARGET FRAMING (core logic)
        // ================================================================

        float _requiredOrthoSize;
        bool _callbackFired;

        void UpdateFollowTargetPosition()
        {
            if (_allTargets.Count == 0) return;

            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            for (int i = 0; i < _allTargets.Count; i++)
            {
                if (_allTargets[i] == null) continue;
                Vector3 p = _allTargets[i].position;
                if (p.x < minX) minX = p.x;
                if (p.x > maxX) maxX = p.x;
                if (p.y < minY) minY = p.y;
                if (p.y > maxY) maxY = p.y;
            }

            float cx = (minX + maxX) * 0.5f;
            float cy = (minY + maxY) * 0.5f;
            _followTarget.position = new Vector3(cx, cy, 0f);

            // Pre-compute required size
            float aspect = _cam.aspect;
            float spanX = (maxX - minX) + FramingPadding * 2f;
            float spanY = (maxY - minY) + FramingPadding * 2f;
            float sizeForW = (spanX * 0.5f) / aspect;
            float sizeForH = spanY * 0.5f;
            _requiredOrthoSize = Mathf.Clamp(Mathf.Max(sizeForW, sizeForH), MinOrthoSize, _maxOrthoSize);
        }

        void UpdateOrthoSize()
        {
            _callbackFired = true;
            float required = _requiredOrthoSize;

            if (required > _currentOrthoSize)
            {
                _currentOrthoSize = Mathf.MoveTowards(_currentOrthoSize, required, ZoomOutSpeed * Time.deltaTime);
                _sizeVelocity = 0f;
            }
            else
            {
                _currentOrthoSize = Mathf.SmoothDamp(_currentOrthoSize, required, ref _sizeVelocity, ZoomInSpeed);
            }
        }

        // ================================================================
        //  SHAKE
        // ================================================================

        public void TriggerShake(float duration, float intensity)
        {
            if (!_enableShake) return;
            _shakeDuration = duration;
            _shakeTimer = duration;
            _shakeIntensity = intensity;
        }

        void ApplyShake()
        {
            if (_shakeTimer <= 0f) return;

            _shakeTimer -= Time.unscaledDeltaTime;
            float decay = Mathf.Clamp01(_shakeTimer / _shakeDuration);
            float ox = Random.Range(-1f, 1f) * _shakeIntensity * decay;
            float oy = Random.Range(-1f, 1f) * _shakeIntensity * decay;

            Vector3 pos = _cam.transform.position;
            _cam.transform.position = new Vector3(pos.x + ox, pos.y + oy, pos.z);
        }

        // ================================================================
        //  PLAYER & TARGETS
        // ================================================================

        void BuildPlayer()
        {
            var go = CreateSquare("Player", Vector3.zero, 0.8f, Color.cyan, 10);
            _playerTransform = go.transform;
            _allTargets.Add(_playerTransform);
        }

        void BuildExtraTargets()
        {
            Color[] palette = { Color.red, Color.green, Color.yellow, Color.magenta, new Color(1f, 0.5f, 0f) };
            for (int i = 0; i < ExtraTargetCount; i++)
            {
                Vector3 pos = RandomPointInBounds(2f);
                Color col = palette[i % palette.Length];
                var go = CreateSquare($"Target_{i + 1}", pos, 0.6f, col, 5);
                _extraTargets.Add(go.transform);
                _allTargets.Add(go.transform);
            }
        }

        Transform ControlledTransform => _controlledIndex == 0
            ? _playerTransform
            : (_controlledIndex - 1 < _extraTargets.Count ? _extraTargets[_controlledIndex - 1] : _playerTransform);

        void HandleTargetSwitch()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.digit1Key.wasPressedThisFrame) _controlledIndex = 0;
            if (kb.digit2Key.wasPressedThisFrame && _extraTargets.Count >= 1) _controlledIndex = 1;
            if (kb.digit3Key.wasPressedThisFrame && _extraTargets.Count >= 2) _controlledIndex = 2;
            if (kb.digit4Key.wasPressedThisFrame && _extraTargets.Count >= 3) _controlledIndex = 3;
        }

        void HandleControlledMovement()
        {
            if (_isDashing) return;

            var target = ControlledTransform;
            Vector3 move = new Vector3(_moveInput.x, _moveInput.y, 0f).normalized * PlayerSpeed * Time.deltaTime;
            Vector3 next = target.position + move;
            next.x = Mathf.Clamp(next.x, BoundaryLeft, BoundaryRight);
            next.y = Mathf.Clamp(next.y, BoundaryBottom, BoundaryTop);
            target.position = next;

            if (_dashAction.WasPressedThisFrame() && _moveInput.sqrMagnitude > 0.01f)
            {
                _isDashing = true;
                _dashTimer = DashDuration;
                _dashDir   = _moveInput.normalized;
            }
        }

        void HandleDash()
        {
            if (!_isDashing) return;

            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0f) { _isDashing = false; return; }

            var target = ControlledTransform;
            Vector3 move = new Vector3(_dashDir.x, _dashDir.y, 0f) * DashSpeed * Time.deltaTime;
            Vector3 next = target.position + move;
            next.x = Mathf.Clamp(next.x, BoundaryLeft, BoundaryRight);
            next.y = Mathf.Clamp(next.y, BoundaryBottom, BoundaryTop);
            target.position = next;
        }

        // ================================================================
        //  HIT SIMULATION
        // ================================================================

        void HandleHitSimulation()
        {
            _hitCooldown -= Time.unscaledDeltaTime;

            if (_hitAction.WasPressedThisFrame() && _hitCooldown <= 0f)
            {
                _hitCooldown = 0.5f;
                TriggerHitEffect();
            }
        }

        void TriggerHitEffect()
        {
            TriggerShake(0.3f, 0.5f);

            _savedTimeScale = Time.timeScale;
            Time.timeScale = 0.05f;
            _timeStopTimer = 0.12f;

            StartCoroutine(HitFlashCoroutine());
        }

        System.Collections.IEnumerator HitFlashCoroutine()
        {
            var sr = ControlledTransform.GetComponent<SpriteRenderer>();
            if (sr == null) yield break;

            var original = sr.color;
            sr.color = Color.white;
            yield return new WaitForSecondsRealtime(0.06f);
            sr.color = Color.red;
            yield return new WaitForSecondsRealtime(0.06f);
            sr.color = original;
        }

        void UpdateTimeStop()
        {
            if (_timeStopTimer > 0f)
            {
                _timeStopTimer -= Time.unscaledDeltaTime;
                if (_timeStopTimer <= 0f)
                    Time.timeScale = _savedTimeScale;
            }
        }

        // ================================================================
        //  VISUAL HELPERS
        // ================================================================

        void BuildBackgroundGrid()
        {
            float step = 2f;
            Color gridColor = new Color(0.25f, 0.25f, 0.35f, 0.5f);
            var gridParent = new GameObject("Grid").transform;

            for (float x = Mathf.Ceil(BoundaryLeft / step) * step; x <= BoundaryRight; x += step)
                CreateLine($"GridV_{x}", gridParent, gridColor, 0.03f,
                    new Vector3(x, BoundaryBottom, 1f), new Vector3(x, BoundaryTop, 1f));

            for (float y = Mathf.Ceil(BoundaryBottom / step) * step; y <= BoundaryTop; y += step)
                CreateLine($"GridH_{y}", gridParent, gridColor, 0.03f,
                    new Vector3(BoundaryLeft, y, 1f), new Vector3(BoundaryRight, y, 1f));
        }

        void BuildBoundaryLines()
        {
            Color c = new Color(1f, 0.3f, 0.3f, 0.8f);
            float w = 0.08f;
            var parent = new GameObject("Boundaries").transform;
            CreateLine("BoundTop",    parent, c, w, new Vector3(BoundaryLeft, BoundaryTop, 0.5f),    new Vector3(BoundaryRight, BoundaryTop, 0.5f));
            CreateLine("BoundBottom", parent, c, w, new Vector3(BoundaryLeft, BoundaryBottom, 0.5f), new Vector3(BoundaryRight, BoundaryBottom, 0.5f));
            CreateLine("BoundLeft",   parent, c, w, new Vector3(BoundaryLeft, BoundaryBottom, 0.5f), new Vector3(BoundaryLeft, BoundaryTop, 0.5f));
            CreateLine("BoundRight",  parent, c, w, new Vector3(BoundaryRight, BoundaryBottom, 0.5f), new Vector3(BoundaryRight, BoundaryTop, 0.5f));
        }

        Vector3 RandomPointInBounds(float margin)
        {
            return new Vector3(
                Random.Range(BoundaryLeft + margin, BoundaryRight - margin),
                Random.Range(BoundaryBottom + margin, BoundaryTop - margin),
                0f);
        }

        // ================================================================
        //  PRIMITIVE FACTORIES
        // ================================================================

        static GameObject CreateSquare(string name, Vector3 pos, float size, Color color, int sortingOrder = 0)
        {
            var go = new GameObject(name);
            go.transform.position   = pos;
            go.transform.localScale = Vector3.one * size;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeWhiteSquareSprite();
            sr.color  = color;
            sr.sortingOrder = sortingOrder;
            return go;
        }

        static Sprite _cachedSquareSprite;
        static Sprite MakeWhiteSquareSprite()
        {
            if (_cachedSquareSprite != null) return _cachedSquareSprite;
            var tex = new Texture2D(4, 4);
            var pixels = new Color[16];
            for (int i = 0; i < 16; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Point;
            _cachedSquareSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            return _cachedSquareSprite;
        }

        static LineRenderer CreateLine(string name, Transform parent, Color color, float width, Vector3 a, Vector3 b)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.SetPosition(0, a);
            lr.SetPosition(1, b);
            lr.startWidth = width;
            lr.endWidth   = width;
            lr.material   = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = color;
            lr.endColor   = color;
            lr.sortingOrder = 1;
            return lr;
        }

        // ================================================================
        //  IMGUI HUD + CONTROL PANEL
        // ================================================================

        void OnGUI()
        {
            DrawHUD();
            DrawControlPanel();
        }

        void DrawHUD()
        {
            var style = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold };
            style.normal.textColor = Color.white;

            float y = 10f;
            float x = 10f;
            GUI.Label(new Rect(x, y, 500, 24), "LF2 多目标摄像机测试 (Cinemachine)", style); y += 22f;

            style.fontSize  = 12;
            style.fontStyle = FontStyle.Normal;

            string[] targetNames = { "玩家(青)", "目标1(红)", "目标2(绿)", "目标3(黄)" };
            string ctrlName = _controlledIndex < targetNames.Length ? targetNames[_controlledIndex] : "?";

            GUI.Label(new Rect(x, y, 700, 20), "WASD/方向键=移动  Shift=冲刺  Space=受击  1234=切换控制  Tab=面板", style); y += 20f;
            GUI.Label(new Rect(x, y, 700, 20),
                $"控制: {ctrlName}  " +
                $"位置: ({ControlledTransform.position.x:F1}, {ControlledTransform.position.y:F1})  " +
                $"中心: ({_followTarget.position.x:F1}, {_followTarget.position.y:F1})  " +
                $"视野: {_cam.orthographicSize:F2}", style); y += 20f;

            style.normal.textColor = Color.yellow;
            GUI.Label(new Rect(x, y, 700, 20),
                $"[DEBUG] required={_requiredOrthoSize:F2}  current={_currentOrthoSize:F2}  max={_maxOrthoSize:F2}  callback={_callbackFired}", style); y += 20f;
            style.normal.textColor = Color.white;

            if (_isDashing)
            {
                style.normal.textColor = Color.yellow;
                GUI.Label(new Rect(x, y, 200, 20), "冲刺中!", style);
                style.normal.textColor = Color.white;
            }

            if (_timeStopTimer > 0f)
            {
                style.normal.textColor = Color.red;
                GUI.Label(new Rect(x + 80, y, 200, 20), "受击定格!", style);
                style.normal.textColor = Color.white;
            }
        }

        void DrawControlPanel()
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame)
                _showPanel = !_showPanel;

            if (!_showPanel) return;

            float panelW = 320f;
            float panelH = 480f;
            float panelX = Screen.width - panelW - 10f;
            float panelY = 10f;

            GUI.Box(new Rect(panelX, panelY, panelW, panelH), "");

            GUILayout.BeginArea(new Rect(panelX + 8, panelY + 8, panelW - 16, panelH - 16));
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);

            var headerStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold };
            headerStyle.normal.textColor = Color.cyan;
            GUILayout.Label("多目标摄像机控制", headerStyle);
            GUILayout.Space(6);

            // ── Multi-target framing ──
            GUILayout.Label("多目标适配", headerStyle);
            GUILayout.Label($"  边距: {FramingPadding:F1}");
            FramingPadding = GUILayout.HorizontalSlider(FramingPadding, 0.5f, 6f);
            GUILayout.Label($"  最小视野: {MinOrthoSize:F1}  最大视野: {_maxOrthoSize:F1}");
            MinOrthoSize = GUILayout.HorizontalSlider(MinOrthoSize, 1f, _maxOrthoSize);
            GUILayout.Label($"  当前视野: {_currentOrthoSize:F2}");
            GUILayout.Space(4);

            // ── Shake ──
            _enableShake = GUILayout.Toggle(_enableShake, "震动 (受击冲击)");
            if (_enableShake && GUILayout.Button("测试震动 (轻击)"))
                TriggerShake(0.2f, 0.2f);
            if (_enableShake && GUILayout.Button("测试震动 (重击)"))
                TriggerShake(0.5f, 0.8f);
            GUILayout.Space(4);

            // ── Hit Simulation ──
            GUILayout.Label("受击模拟", headerStyle);
            if (GUILayout.Button("触发受击 (Space)"))
            {
                if (_hitCooldown <= 0f)
                {
                    _hitCooldown = 0.5f;
                    TriggerHitEffect();
                }
            }
            GUILayout.Space(4);

            // ── Info ──
            GUILayout.Label("信息", headerStyle);
            string[] names = { "玩家(青)", "目标1(红)", "目标2(绿)", "目标3(黄)" };
            string cn = _controlledIndex < names.Length ? names[_controlledIndex] : "?";
            GUILayout.Label($"当前控制: {cn}  (按1234切换)");
            GUILayout.Label($"目标数: {_allTargets.Count}");
            GUILayout.Label($"跟随中心: ({_followTarget.position.x:F1}, {_followTarget.position.y:F1})");
            GUILayout.Label($"边界: {BoundaryLeft:F1}~{BoundaryRight:F1} x {BoundaryBottom:F1}~{BoundaryTop:F1}");
            GUILayout.Label($"时间缩放: {Time.timeScale:F2}");

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
