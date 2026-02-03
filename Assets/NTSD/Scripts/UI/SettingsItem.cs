using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

namespace NTSD.UI
{
    public sealed class SettingsItem : MonoBehaviour
    {
        [Serializable]
        private sealed class Entry
        {
            public Button button;
            public TextMeshProUGUI label;
            public string actionName;
            public string bindingName;
            public string bindingPath;

            [HideInInspector] public Color defaultColor;
            [HideInInspector] public string defaultLabel;
            [HideInInspector] public string pendingLabel;
        }

        [Header("Bindings")]
        [SerializeField] private List<Entry> entries = new List<Entry>();
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private int playerIndex = 1;
        [SerializeField] private bool rebuildEntriesOnEnable = true;

        [Header("Behavior")]
        [SerializeField] private Color selectedColor = new Color(1f, 0.92f, 0.016f, 1f);
        [SerializeField] private bool includeInactive = true;
        [SerializeField] private bool allowMouseButtons = false;
        [SerializeField] private bool allowModifierKeys = false;

        private Entry currentEntry;
        private bool waitingForKey;

        private void Reset()
        {
            AutoCollectEntries();
        }

        private void OnEnable()
        {
            if (rebuildEntriesOnEnable)
            {
                AutoCollectEntries();
            }
            EnsureEntriesInitialized();
            InitializeEntriesFromInputActions();
            RegisterCallbacks();
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        private void Update()
        {
            if (!waitingForKey)
            {
                return;
            }

            var label = GetPressedInputLabel();
            if (string.IsNullOrEmpty(label))
            {
                return;
            }

            ApplyLabelToCurrentEntry(label);
        }

        private void RegisterCallbacks()
        {
            foreach (var entry in entries)
            {
                if (entry?.button == null || entry.label == null)
                {
                    continue;
                }

                var capturedEntry = entry;
                entry.button.onClick.AddListener(() => OnEntryClicked(capturedEntry));
            }
        }

        private void UnregisterCallbacks()
        {
            foreach (var entry in entries)
            {
                if (entry?.button == null)
                {
                    continue;
                }

                entry.button.onClick.RemoveAllListeners();
            }
        }

        private void OnEntryClicked(Entry entry)
        {
            if (entry == null || entry.label == null)
            {
                return;
            }

            SetCurrentEntry(entry);
            waitingForKey = true;
        }

        private void SetCurrentEntry(Entry entry)
        {
            if (currentEntry != null && currentEntry.label != null)
            {
                currentEntry.label.color = currentEntry.defaultColor;
            }

            currentEntry = entry;
            currentEntry.label.color = selectedColor;
        }

        private void ApplyLabelToCurrentEntry(string label)
        {
            if (currentEntry == null || currentEntry.label == null)
            {
                waitingForKey = false;
                return;
            }

            currentEntry.label.text = label;
            currentEntry.label.color = currentEntry.defaultColor;
            currentEntry.pendingLabel = label;
            waitingForKey = false;
        }

        private void EnsureEntriesInitialized()
        {
            if (entries == null || entries.Count == 0)
            {
                AutoCollectEntries();
            }

            foreach (var entry in entries)
            {
                if (entry?.label == null)
                {
                    continue;
                }

                entry.defaultColor = entry.label.color;
                if (string.IsNullOrWhiteSpace(entry.defaultLabel))
                {
                    entry.defaultLabel = entry.label.text;
                }
            }
        }

        private void InitializeEntriesFromInputActions()
        {
            if (inputActions == null)
            {
                return;
            }

            var mapName = $"Player_{playerIndex}";
            var map = inputActions.FindActionMap(mapName, throwIfNotFound: false);
            if (map == null)
            {
                return;
            }

            foreach (var entry in entries)
            {
                if (entry == null || entry.label == null || string.IsNullOrWhiteSpace(entry.actionName))
                {
                    continue;
                }

                var action = map.FindAction(entry.actionName, throwIfNotFound: false);
                if (action == null)
                {
                    continue;
                }

                var bindingIndex = ResolveBindingIndex(action, entry);
                if (bindingIndex < 0 || bindingIndex >= action.bindings.Count)
                {
                    continue;
                }

                var binding = action.bindings[bindingIndex];
                var label = FormatBindingLabel(binding);
                if (!string.IsNullOrEmpty(label))
                {
                    entry.label.text = label;
                    entry.defaultLabel = label;
                }
            }
        }

        private void AutoCollectEntries()
        {
            entries = new List<Entry>();
            var buttons = GetComponentsInChildren<Button>(includeInactive);
            foreach (var button in buttons)
            {
                if (button == null)
                {
                    continue;
                }

                var mapping = ResolveMapping(button.gameObject.name);
                if (!mapping.hasMapping)
                {
                    continue;
                }

                var label = button.GetComponentInChildren<TextMeshProUGUI>(includeInactive);
                if (label == null)
                {
                    continue;
                }

                entries.Add(new Entry
                {
                    button = button,
                    label = label,
                    actionName = mapping.actionName,
                    bindingName = mapping.bindingName,
                    bindingPath = mapping.bindingPath,
                    defaultColor = label.color
                });
            }
        }

        public void ApplyChanges()
        {
            if (inputActions == null)
            {
                return;
            }

            foreach (var entry in entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.pendingLabel))
                {
                    continue;
                }

                ApplyBindingOverride(entry, entry.pendingLabel);
                entry.defaultLabel = entry.pendingLabel;
                entry.pendingLabel = null;
            }
        }

        public void CancelChanges()
        {
            foreach (var entry in entries)
            {
                if (entry == null || entry.label == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(entry.defaultLabel))
                {
                    entry.label.text = entry.defaultLabel;
                }

                entry.label.color = entry.defaultColor;
                entry.pendingLabel = null;
            }

            waitingForKey = false;
        }

        private string GetPressedInputLabel()
        {
            if (allowMouseButtons)
            {
                var mouseLabel = GetMouseButtonLabel();
                if (!string.IsNullOrEmpty(mouseLabel))
                {
                    return mouseLabel;
                }
            }

            if (Keyboard.current == null || !Keyboard.current.anyKey.wasPressedThisFrame)
            {
                return null;
            }

            foreach (var keyControl in Keyboard.current.allKeys)
            {
                if (keyControl == null || !keyControl.wasPressedThisFrame)
                {
                    continue;
                }

                var key = keyControl.keyCode;
                if (key == Key.None)
                {
                    continue;
                }

                if (!allowModifierKeys && IsModifierKey(key))
                {
                    continue;
                }

                return FormatKeyText(key);
            }

            return null;
        }

        private static string GetMouseButtonLabel()
        {
            if (Mouse.current == null)
            {
                return null;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame) return "MOUSE0";
            if (Mouse.current.rightButton.wasPressedThisFrame) return "MOUSE1";
            if (Mouse.current.middleButton.wasPressedThisFrame) return "MOUSE2";
            if (Mouse.current.backButton.wasPressedThisFrame) return "MOUSE3";
            if (Mouse.current.forwardButton.wasPressedThisFrame) return "MOUSE4";

            return null;
        }

        private static bool IsModifierKey(Key key)
        {
            return key == Key.LeftShift || key == Key.RightShift ||
                   key == Key.LeftCtrl || key == Key.RightCtrl ||
                   key == Key.LeftAlt || key == Key.RightAlt ||
                   key == Key.LeftMeta || key == Key.RightMeta;
        }

        private static string FormatKeyText(Key key)
        {
            return key.ToString().ToUpperInvariant();
        }

        private static string FormatBindingLabel(InputBinding binding)
        {
            var path = binding.effectivePath;
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var readable = InputControlPath.ToHumanReadableString(
                path,
                InputControlPath.HumanReadableStringOptions.OmitDevice);

            if (string.IsNullOrWhiteSpace(readable))
            {
                return null;
            }

            return readable.ToUpperInvariant();
        }

        private void ApplyBindingOverride(Entry entry, string label)
        {
            if (inputActions == null || entry == null || string.IsNullOrWhiteSpace(entry.actionName))
            {
                return;
            }

            var mapName = $"Player_{playerIndex}";
            var map = inputActions.FindActionMap(mapName, throwIfNotFound: false);
            if (map == null)
            {
                return;
            }

            var action = map.FindAction(entry.actionName, throwIfNotFound: false);
            if (action == null)
            {
                return;
            }

            var overridePath = BuildOverridePath(label);
            if (string.IsNullOrEmpty(overridePath))
            {
                return;
            }

            var bindingIndex = ResolveBindingIndex(action, entry);
            if (bindingIndex < 0)
            {
                return;
            }

            action.ApplyBindingOverride(bindingIndex, new InputBinding { overridePath = overridePath });
        }

        private int ResolveBindingIndex(InputAction action, Entry entry)
        {
            if (!string.IsNullOrWhiteSpace(entry.bindingName))
            {
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    var binding = action.bindings[i];
                    if (binding.name == entry.bindingName)
                    {
                        return i;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(entry.bindingPath))
            {
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    var binding = action.bindings[i];
                    if (binding.path == entry.bindingPath)
                    {
                        return i;
                    }
                }
            }

            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                if (!binding.isComposite && !binding.isPartOfComposite)
                {
                    return i;
                }
            }

            return -1;
        }

        private static string BuildOverridePath(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                return null;
            }

            var lower = label.Trim().ToLowerInvariant();
            switch (lower)
            {
                case "mouse0":
                    return "<Mouse>/leftButton";
                case "mouse1":
                    return "<Mouse>/rightButton";
                case "mouse2":
                    return "<Mouse>/middleButton";
                case "mouse3":
                    return "<Mouse>/backButton";
                case "mouse4":
                    return "<Mouse>/forwardButton";
            }

            return $"<Keyboard>/{lower}";
        }

        private (bool hasMapping, string actionName, string bindingName, string bindingPath) ResolveMapping(string gameObjectName)
        {
            if (string.IsNullOrWhiteSpace(gameObjectName))
            {
                return (false, null, null, null);
            }

            if (gameObjectName == "PlayerNameTxt")
            {
                return (false, null, null, null);
            }

            switch (gameObjectName)
            {
                case "UpSetting":
                    return (true, "Move", "Up", null);
                case "DownSetting":
                    return (true, "Move", "Down", null);
                case "LeftSetting":
                    return (true, "Move", "Left", null);
                case "RightSetting":
                    return (true, "Move", "Right", null);
                case "AttSetting":
                    return (true, "Attack", null, "<Keyboard>/j");
                case "JumpSetting":
                    return (true, "Jump", null, "<Keyboard>/k");
                case "DefSetting":
                    return (true, "Defend", null, "<Keyboard>/l");
                default:
                    return (false, null, null, null);
            }
        }
    }
}
