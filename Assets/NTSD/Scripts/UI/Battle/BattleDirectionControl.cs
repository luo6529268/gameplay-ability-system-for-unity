using MoreMountains.Tools;
using NTSD.App;
using NTSD.Simulation;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NTSD.UI.Battle
{
    public enum BattleDirectionMode
    {
        ClickOnly,
        JoystickOnly,
        Hybrid
    }

    public sealed class BattleDirectionControl : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IInitializePotentialDragHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, ICancelHandler,
        MMEventListener<BattleHudChangedEvent>
    {
        [SerializeField] private RectTransform directionRing;
        [SerializeField] private RectTransform knob;
        [SerializeField] private BattleDirectionMode mode = BattleDirectionMode.Hybrid;
        [Tooltip("Screen pixels from the initial press, independent of Canvas scale.")]
        [SerializeField, Min(1f)] private float dragThresholdPixels = 12f;
        [SerializeField, Range(0f, 0.9f)] private float clickInnerRadius = 0.45f;
        [SerializeField, Range(0f, 0.9f)] private float deadZone = 0.2f;
        [SerializeField, Range(0f, 0.2f)] private float deadZoneHysteresis = 0.05f;
        [SerializeField, Range(0f, 15f)] private float angularHysteresis = 5f;

        private InputModule inputModule;
        private BattleHudValues binding;
        private int playerId = -1;
        private bool ownsPointer;
        private int pointerId;
        private Vector2 pressPosition;
        private bool dragging;
        private int sector = -1;
        private Vector2 direction;
        private Vector2 knobOrigin;
        private bool focused = true;
        private bool paused;

        public BattleDirectionMode Mode => mode;
        public Vector2 Direction => direction;
        public bool IsDragging => dragging;
        public int PlayerId => playerId;

        private void Awake()
        {
            if (knob != null) knobOrigin = knob.anchoredPosition;
        }

        private void OnEnable()
        {
            this.MMEventStartListening<BattleHudChangedEvent>();
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            if (driver != null && driver.TryGetCurrentBattleHud(out BattleHudChangedEvent value))
                ApplyBinding(value.Values);
        }

        private void OnDisable()
        {
            this.MMEventStopListening<BattleHudChangedEvent>();
            UnbindPlayer();
        }

        private void OnApplicationFocus(bool value)
        {
            focused = value;
            if (!value) CancelGesture();
        }

        private void OnApplicationPause(bool value)
        {
            paused = value;
            if (value) CancelGesture();
        }

        public void OnMMEvent(BattleHudChangedEvent value)
        {
            if (!isActiveAndEnabled) return;
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            if (driver != null && driver.IsCurrentBattleHudEvent(value)) ApplyBinding(value.Values);
        }

        private void ApplyBinding(BattleHudValues value)
        {
            if (!value.IsVisible || value.InputId < 1)
            {
                UnbindPlayer();
                return;
            }
            if (playerId == value.InputId && binding.Session == value.Session &&
                binding.Handle.Equals(value.Handle) && binding.StableId == value.StableId) return;
            if (BindPlayer(value.InputId)) binding = value;
        }

        public bool BindPlayer(int inputId)
        {
            UnbindPlayer();
            if (inputId < 1) return false;
            AppManager manager = AppManager.Instance;
            InputModule candidate = manager != null ? manager.InputModule : null;
            if (candidate == null || candidate.GetActionMapByPlayerID(inputId)?.FindAction("Move") == null)
                return false;
            inputModule = candidate;
            playerId = inputId;
            return true;
        }

        public void UnbindPlayer()
        {
            CancelGesture();
            inputModule = null;
            playerId = -1;
            binding = default;
        }

        public void SetMode(BattleDirectionMode value)
        {
            if (value < BattleDirectionMode.ClickOnly || value > BattleDirectionMode.Hybrid)
                return;
            if (mode == value) return;
            CancelGesture();
            mode = value;
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isActiveAndEnabled || !focused || paused || ownsPointer || playerId < 1 ||
                eventData.button != PointerEventData.InputButton.Left ||
                !TryGetOffset(eventData, out Vector2 offset, out float radius) ||
                offset.sqrMagnitude > radius * radius) return;
            ownsPointer = true;
            pointerId = eventData.pointerId;
            pressPosition = eventData.position;
            dragging = mode == BattleDirectionMode.JoystickOnly;
            if (dragging)
                ApplyJoystick(offset, radius);
            else
                SetSector(offset.magnitude >= radius * clickInnerRadius ? ClosestSector(offset) : -1);
        }

        public void OnBeginDrag(PointerEventData eventData) => OnDrag(eventData);

        public void OnDrag(PointerEventData eventData)
        {
            if (!ownsPointer || pointerId != eventData.pointerId || mode == BattleDirectionMode.ClickOnly)
                return;
            if (!dragging)
            {
                float threshold = Mathf.Max(1f, dragThresholdPixels);
                if ((eventData.position - pressPosition).sqrMagnitude <= threshold * threshold) return;
                dragging = true;
                sector = -1;
            }
            if (TryGetOffset(eventData, out Vector2 offset, out float radius)) ApplyJoystick(offset, radius);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (ownsPointer && pointerId == eventData.pointerId) CancelGesture();
        }

        public void OnEndDrag(PointerEventData eventData) => OnPointerUp(eventData);
        public void OnCancel(BaseEventData eventData) => CancelGesture();

        public void CancelGesture()
        {
            ownsPointer = false;
            dragging = false;
            SetSector(-1);
            if (knob != null) knob.anchoredPosition = knobOrigin;
        }

        private bool TryGetOffset(PointerEventData eventData, out Vector2 offset, out float radius)
        {
            offset = Vector2.zero;
            radius = directionRing != null ? Mathf.Min(directionRing.rect.width, directionRing.rect.height) * 0.5f : 0f;
            if (radius <= 0f || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    directionRing, eventData.position, eventData.pressEventCamera, out Vector2 local)) return false;
            offset = local - directionRing.rect.center;
            return true;
        }

        private void ApplyJoystick(Vector2 offset, float radius)
        {
            float knobRadius = knob != null ? Mathf.Min(knob.rect.width, knob.rect.height) * 0.5f : 0f;
            float travel = Mathf.Max(1f, radius - knobRadius);
            Vector2 displacement = Vector2.ClampMagnitude(offset, travel);
            if (knob != null)
            {
                Vector3 worldDelta = directionRing.TransformVector(displacement);
                Vector3 parentDelta = knob.parent.InverseTransformVector(worldDelta);
                knob.anchoredPosition = knobOrigin + new Vector2(parentDelta.x, parentDelta.y);
            }
            float magnitude = offset.magnitude / travel;
            float boundary = sector < 0 ? deadZone + deadZoneHysteresis : Mathf.Max(0f, deadZone - deadZoneHysteresis);
            if (magnitude <= boundary)
            {
                SetSector(-1);
                return;
            }
            float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            if (sector >= 0 && Mathf.Abs(Mathf.DeltaAngle(sector * 45f, angle)) <= 22.5f + angularHysteresis)
                return;
            SetSector(ClosestSector(offset));
        }

        private static int ClosestSector(Vector2 offset)
        {
            float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            return (Mathf.FloorToInt((angle + 22.5f) / 45f) + 8) % 8;
        }

        private void SetSector(int value)
        {
            sector = value;
            Vector2 next = Vector2.zero;
            if (value >= 0)
            {
                float angle = value * 45f * Mathf.Deg2Rad;
                next = new Vector2(Mathf.Round(Mathf.Cos(angle)), Mathf.Round(Mathf.Sin(angle)));
            }
            if (next == direction) return;
            direction = next;
            inputModule?.TrySetMoveInput(playerId, next);
        }
    }
}
