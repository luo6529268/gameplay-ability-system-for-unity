#if UNITY_EDITOR

using System;
using System.IO;
using UnityEngine;
using UnityEditor;
 

namespace TMPro
{
    [System.Serializable]
    public class TMP_PackageResourceImporter
    {
        bool m_EssentialResourcesImported;
        bool m_ExamplesAndExtrasResourcesImported;
        internal bool m_IsImportingExamples;

        public TMP_PackageResourceImporter() { }

        public void OnDestroy()
        {
        }

        public void OnGUI()
        {
            // Check if the resources state has changed.
            m_EssentialResourcesImported = Directory.Exists("Assets/TextMeshPro");
            // The portable distribution has no package importer or examples archive.
            m_ExamplesAndExtrasResourcesImported = true;

            GUILayout.BeginVertical();
            {
                // Display options to import Essential resources
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUILayout.Label("Portable TMP Resources", EditorStyles.boldLabel);
                    GUILayout.Label("Core resources are included in Assets/TextMeshPro. The default font is initialized automatically after import.", new GUIStyle(EditorStyles.label) { wordWrap = true } );
                    GUILayout.Space(5f);

                    GUI.enabled = !m_EssentialResourcesImported;
                    if (GUILayout.Button("Import TMP Essentials"))
                    {
                        AssetDatabase.importPackageCompleted += ImportCallback;

                        string packageFullPath = GetPackageFullPath();
                        AssetDatabase.ImportPackage(packageFullPath + "/Package Resources/TMP Essential Resources.unitypackage", false);
                    }
                    GUILayout.Space(5f);
                    GUI.enabled = true;
                }
                GUILayout.EndVertical();

                // Display options to import Examples & Extras
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUILayout.Label("Examples & Extras", EditorStyles.boldLabel);
                    GUILayout.Label("Optional examples are not included in this portable distribution.", new GUIStyle(EditorStyles.label) { wordWrap = true });
                    GUILayout.Space(5f);

                    GUI.enabled = m_EssentialResourcesImported && !m_ExamplesAndExtrasResourcesImported;
                    if (GUILayout.Button("Import TMP Examples & Extras"))
                    {
                        // Set flag to get around importing scripts as per of this package which results in an assembly reload which in turn prevents / clears any callbacks.
                        m_IsImportingExamples = true;

                        var packageFullPath = GetPackageFullPath();
                        AssetDatabase.ImportPackage(packageFullPath + "/Package Resources/TMP Examples & Extras.unitypackage", false);
                    }
                    GUILayout.Space(5f);
                    GUI.enabled = true;
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
            GUILayout.Space(5f);
        }

        internal void RegisterResourceImportCallback()
        {
            AssetDatabase.importPackageCompleted += ImportCallback;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        void ImportCallback(string packageName)
        {
            if (packageName == "TMP Essential Resources")
            {
                m_EssentialResourcesImported = true;
                TMPro_EventManager.ON_RESOURCES_LOADED();

                #if UNITY_2018_3_OR_NEWER
                SettingsService.NotifySettingsProviderChanged();
                #endif
            }
            else if (packageName == "TMP Examples & Extras")
            {
                m_ExamplesAndExtrasResourcesImported = true;
                m_IsImportingExamples = false;
            }

            Debug.Log("[" + packageName + "] have been imported.");

            AssetDatabase.importPackageCompleted -= ImportCallback;
        }

        static string GetPackageFullPath()
        {
            // Check for potential UPM package
            string packagePath = Path.GetFullPath("Packages/com.unity.textmeshpro");
            if (Directory.Exists(packagePath))
            {
                return packagePath;
            }

            packagePath = Path.GetFullPath("Assets/..");
            if (Directory.Exists(packagePath))
            {
                // Search default location for development package
                if (Directory.Exists(packagePath + "/Assets/Packages/com.unity.TextMeshPro/Editor Resources"))
                {
                    return packagePath + "/Assets/Packages/com.unity.TextMeshPro";
                }

                // Search for default location of normal TextMesh Pro AssetStore package
                if (Directory.Exists(packagePath + "/Assets/TextMeshPro/Editor Resources"))
                {
                    return packagePath + "/Assets/TextMeshPro";
                }

                // Search for potential alternative locations in the user project
                string[] matchingPaths = Directory.GetDirectories(packagePath, "TextMeshPro", SearchOption.AllDirectories);
                string path = ValidateLocation(matchingPaths, packagePath);
                if (path != null) return packagePath + path;
            }

            return null;
        }

        static string ValidateLocation(string[] paths, string projectPath)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                // Check if the Editor Resources folder exists.
                if (Directory.Exists(paths[i] + "/Editor Resources"))
                {
                    string folderPath = paths[i].Replace(projectPath, "");
                    folderPath = folderPath.TrimStart('\\', '/');
                    return folderPath;
                }
            }

            return null;
        }
    }

    public class TMP_PackageResourceImporterWindow : EditorWindow
    {
        [SerializeField]
        TMP_PackageResourceImporter m_ResourceImporter;

        public static void ShowPackageImporterWindow()
        {
            var window = GetWindow<TMP_PackageResourceImporterWindow>();
            window.titleContent = new GUIContent("TMP Importer");
            window.Focus();
        }

        void OnEnable()
        {
            // Set Editor Window Size
            SetEditorWindowSize();

            if (m_ResourceImporter == null)
                m_ResourceImporter = new TMP_PackageResourceImporter();

            if (m_ResourceImporter.m_IsImportingExamples)
                m_ResourceImporter.RegisterResourceImportCallback();
        }

        void OnDestroy()
        {
            m_ResourceImporter.OnDestroy();
        }

        void OnGUI()
        {
            m_ResourceImporter.OnGUI();
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }
        
        /// <summary>
        /// Limits the minimum size of the editor window.
        /// </summary>
        void SetEditorWindowSize()
        {
            EditorWindow editorWindow = this;

            Vector2 windowSize = new Vector2(640, 210);
            editorWindow.minSize = windowSize;
            editorWindow.maxSize = windowSize;
        }
    }

}

#endif
