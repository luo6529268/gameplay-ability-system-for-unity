#if UNITY_EDITOR
using Kako.Common;
using UnityEditor;
using UnityEngine;

namespace Kako.CameraFit
{
    [CustomEditor(typeof(CameraFitter))]
    public class CameraFitterEditor : KakoEditor<CameraFitter>
    {
        private const string ScriptPropertyName = "Script";
        
        // LABELS
        private const string MainLabel = "CAMERA FITTER";
        private const string SettingsLabel = "Settings";
        private const string RectFitLabel = "Rect Fit";
        private const string FocusLabel = "Focus";
        private const string DebugLabel = "Debug";
        private const string FitPointsLabel = "Fit Points";
        private const string FitButtonLabel = "Fit";
        
        // MESSAGES
        private const string CameraErrorMessage = "You need to assign a camera!";
        private const string FitAreaErrorMessage = "You need to assign a RectTransform for fit area!";
        private const string FitAreaCanvasErrorMessage = "You need to assign a canvas that contains the fit area!";
        private const string FocusObjectErrorMessage = "You need to assign a focus object!";
        private const string FitMeshInfoMessage = "Meshes with a large number of vertices may cause playtime performance problems!";

        // COLORS
        private static readonly Color LayoutBoxColor = Color.black;
        private static readonly Color SeparatorColor = Color.grey;

        // PROPERTIES
        private SerializedProperty _cameraProperty, _paddingProperty, _orientationProperty;
        private SerializedProperty _hasFocusProperty, _focusTypeProperty, _focusPositionProperty, _focusTransformProperty;
        private SerializedProperty _isRectFitProperty, _fitAreaCanvasProperty, _fitAreaProperty;
        private SerializedProperty _fitPositionsProperty, _fitPivotsProperty, _fitMeshesProperty;
        private SerializedProperty _debugEnabledProperty, _debugFitFrustumProperty, _debugCenterLineProperty, _debugClipPlaneProperty, _debugClipPlanePercentProperty;
        private SerializedProperty _debugFitFrustumColorProperty, _debugCenterLineColorProperty, _debugClipPlaneColorProperty;

        private void OnEnable()
        {
            _cameraProperty = GetProperty("fitCamera");
            _paddingProperty = GetProperty("padding");
            _orientationProperty = GetProperty("orientation");
            _hasFocusProperty = GetProperty("enableFocus");
            _focusTypeProperty = GetProperty("focusType");
            _focusPositionProperty = GetProperty("focusPosition");
            _focusTransformProperty = GetProperty("focusObject");
            _isRectFitProperty = GetProperty("enableRectFit");
            _fitAreaCanvasProperty = GetProperty("fitAreaCanvas");
            _fitAreaProperty = GetProperty("fitArea");
            _fitPositionsProperty = GetProperty("fitPositions");
            _fitPivotsProperty = GetProperty("fitPivots");
            _fitMeshesProperty = GetProperty("fitMeshes");
            _debugEnabledProperty = GetProperty("enableDebug");
            _debugFitFrustumProperty = GetProperty("drawFitFrustum");
            _debugCenterLineProperty = GetProperty("drawFitFrustumCenterLine");
            _debugClipPlaneProperty = GetProperty("drawClipPlane");
            _debugClipPlanePercentProperty = GetProperty("clipPlanePercent");
            _debugFitFrustumColorProperty = GetProperty("fitFrustumColor");
            _debugCenterLineColorProperty = GetProperty("centerLineColor");
            _debugClipPlaneColorProperty = GetProperty("clipPlaneColor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(ScriptPropertyName, MonoScript.FromMonoBehaviour((CameraFitter)target), typeof(CameraFitter), false);
            EditorGUI.EndDisabledGroup();
            
            DrawLabel(MainLabel, TextAnchor.MiddleCenter, Color.yellow, 30f);

            // SETTINGS
            BeginVerticalLayoutBox(LayoutBoxColor);
            if (DrawFoldout(SettingsLabel))
            {
                DrawSeparatorLine(1f, SeparatorColor);
                Separator();
                if (Target.Camera == null) Error(CameraErrorMessage, false);
                DrawProperty(_cameraProperty);
                DrawProperty(_paddingProperty);
                DrawProperty(_orientationProperty);
            }
            EndVerticalLayoutBox();

            Separator();

            // FIT POINTS
            BeginVerticalLayoutBox(LayoutBoxColor);
            if (DrawFoldout(FitPointsLabel))
            {
                DrawSeparatorLine(1f, SeparatorColor);
                Separator();
                BeginIndent();
                BeginIndent();
                DrawProperty(_fitPositionsProperty);
                DrawProperty(_fitPivotsProperty);
                DrawProperty(_fitMeshesProperty);
                EndIndent();
                Info(FitMeshInfoMessage, true);
                EndIndent();
            }
            EndVerticalLayoutBox();

            Separator();

            // RECT FIT
            BeginVerticalLayoutBox(LayoutBoxColor);
            if (DrawToggleFoldout(RectFitLabel, _isRectFitProperty))
            {
                DrawSeparatorLine(1f, SeparatorColor);
                Separator();
                if (!Target.IsRectFit)
                    BeginDisabled();

                if (Target.IsRectFit && Target.FitArea == null) Error(FitAreaErrorMessage, false);
                DrawProperty(_fitAreaProperty);
                if (Target.IsRectFit && Target.FitAreaCanvas == null) Error(FitAreaCanvasErrorMessage, false);
                DrawProperty(_fitAreaCanvasProperty);

                if (!Target.IsRectFit)
                    EndDisabled();
            }
            EndVerticalLayoutBox();

            Separator();

            // FOCUS
            BeginVerticalLayoutBox(LayoutBoxColor);
            if (DrawToggleFoldout(FocusLabel, _hasFocusProperty))
            {
                DrawSeparatorLine(1f, SeparatorColor);
                Separator();
                if (!Target.HasFocus)
                    BeginDisabled();

                DrawProperty(_focusTypeProperty);
                switch (Target.FocusType)
                {
                    case FocusType.Position:
                        DrawProperty(_focusPositionProperty);
                        break;
                    case FocusType.ObjectPivot:
                        if (Target.HasFocus && Target.FocusTransform == null) Error(FocusObjectErrorMessage, false);
                        DrawProperty(_focusTransformProperty);
                        break;
                }

                if (!Target.HasFocus)
                    EndDisabled();
            }
            EndVerticalLayoutBox();

            Separator();

            // FIT BUTTON
            if (DrawButton(FitButtonLabel, 40f))
            {
                Target.Fit();
            }

            Separator();

            // DEBUG
            BeginVerticalLayoutBox(LayoutBoxColor);
            if (DrawToggleFoldout(DebugLabel, _debugEnabledProperty))
            {
                DrawSeparatorLine(1f, SeparatorColor);
                Separator();

                if (!Target.IsDebugEnabled)
                    BeginDisabled();

                DrawProperty(_debugFitFrustumProperty);
                if (Target.DebugDrawFitFrustum)
                {
                    BeginIndent();
                    DrawProperty(_debugFitFrustumColorProperty);
                    EndIndent();
                }
                
                Separator();

                DrawProperty(_debugCenterLineProperty);
                if (Target.DebugFitFrustumCenterLine)
                {
                    BeginIndent();
                    DrawProperty(_debugCenterLineColorProperty);
                    EndIndent();
                }
                
                Separator();

                DrawProperty(_debugClipPlaneProperty);
                if (Target.DebugDrawClipPlane)
                {
                    BeginIndent();
                    DrawProperty(_debugClipPlaneColorProperty);   
                    DrawProperty(_debugClipPlanePercentProperty);   
                    EndIndent();
                }

                if (!Target.IsDebugEnabled)
                    EndDisabled();
            }
            EndVerticalLayoutBox();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
