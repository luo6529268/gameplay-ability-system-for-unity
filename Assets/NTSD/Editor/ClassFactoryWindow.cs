using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

namespace ClassFactoryTool
{
    /// <summary>
    /// 类工厂工具 - 用于动态生成继承指定基类的子类
    /// </summary>
    public static class ClassFactory
    {
        // 应跳过重写的方法名集合
        private static readonly HashSet<string> ExcludedMethods = new HashSet<string>
        {
            "Finalize", // 析构函数
            "MemberwiseClone", // 浅拷贝函数
            "Equals", // 可能会影响比较逻辑
            "GetHashCode", // 可能会影响哈希计算
            "ToString", // 字符串显示
            "GetType" // 类型信息
        };

        /// <summary>
        /// 创建子类文件
        /// </summary>
        /// <param name="baseType">基类类型</param>
        /// <param name="subclassName">子类名称</param>
        /// <param name="namespaceName">命名空间</param>
        /// <param name="outputPath">输出路径</param>
        /// <returns>是否创建成功</returns>
        public static bool CreateSubclass(Type baseType, string subclassName, string namespaceName, string outputPath)
        {
            if (baseType == null)
            {
                Debug.LogError("基类类型不能为null");
                return false;
            }

            if (string.IsNullOrEmpty(subclassName))
            {
                Debug.LogError("子类名称不能为空");
                return false;
            }

            try
            {
                string classContent = GenerateClassContent(baseType, subclassName, namespaceName);
                string fullPath = Path.Combine(outputPath, $"{subclassName}.cs");

                // 确保目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                File.WriteAllText(fullPath, classContent, Encoding.UTF8);

#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif

                Debug.Log($"成功创建子类: {subclassName} 路径: {fullPath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"创建子类时出错: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 生成类内容
        /// </summary>
        private static string GenerateClassContent(Type baseType, string subclassName, string namespaceName)
        {
            StringBuilder sb = new StringBuilder();

            // 添加using指令
            AddUsings(sb, baseType);

            // 添加命名空间
            if (!string.IsNullOrEmpty(namespaceName))
            {
                sb.AppendLine($"namespace {namespaceName}");
                sb.AppendLine("{");
            }

            // 类定义（接口和类的继承语法相同）
            sb.AppendLine($"    public class {subclassName} : {baseType.Name}");
            sb.AppendLine("    {");

            // 生成虚方法重写
            GenerateVirtualMethods(sb, baseType);

            // 生成虚属性重写
            GenerateVirtualProperties(sb, baseType);

            sb.AppendLine("    }");

            if (!string.IsNullOrEmpty(namespaceName))
            {
                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 添加using指令
        /// </summary>
        private static void AddUsings(StringBuilder sb, Type baseType)
        {
            HashSet<string> usings = new HashSet<string>
            {
                "using System.Collections;",
                "using System.Collections.Generic;",
                "using UnityEngine;"
            };

            // 添加基类所在的命名空间
            if (!string.IsNullOrEmpty(baseType.Namespace))
            {
                usings.Add($"using {baseType.Namespace};");
            }

            foreach (string usingLine in usings)
            {
                sb.AppendLine(usingLine);
            }
            sb.AppendLine();
        }

        /// <summary>
        /// 生成虚方法重写
        /// </summary>
        private static void GenerateVirtualMethods(StringBuilder sb, Type baseType)
        {
            // 获取所有方法，包括继承的虚方法
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
            MethodInfo[] methods = baseType.GetMethods(flags);

            bool isInterface = baseType.IsInterface;

            foreach (MethodInfo method in methods)
            {
                // 跳过排除列表中的方法
                if (ExcludedMethods.Contains(method.Name))
                    continue;

                // 排除属性的方法（如get_/set_方法）
                if (method.IsSpecialName)
                    continue;

                bool shouldGenerate = false;

                if (isInterface)
                {
                    // 接口：生成所有公开方法
                    shouldGenerate = method.DeclaringType == baseType;
                }
                else
                {
                    // 类：只处理虚方法且不是最终实现的方法
                    if (method.IsVirtual && !method.IsFinal && !method.IsPrivate)
                    {
                        shouldGenerate = method.DeclaringType == baseType ||
                            baseType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                .Any(m => m.Name == method.Name && m.GetParameters().SequenceEqual(method.GetParameters()));
                    }
                }

                if (shouldGenerate)
                {
                    string accessModifier = GetAccessModifier(method);
                    string returnType = GetTypeName(method.ReturnType);
                    string parameters = GetParametersString(method.GetParameters());
                    string overrideKeyword = isInterface ? "" : "override ";

                    sb.AppendLine();
                    sb.AppendLine($"        {accessModifier} {overrideKeyword}{returnType} {method.Name}({parameters})");
                    sb.AppendLine("        {");

                    if (isInterface)
                    {
                        // 接口实现：抛出未实现异常或默认返回
                        if (method.ReturnType == typeof(void))
                        {
                            sb.AppendLine("            throw new System.NotImplementedException();");
                        }
                        else
                        {
                            sb.AppendLine($"            throw new System.NotImplementedException();");
                        }
                    }
                    else
                    {
                        // 类重写：调用base
                        if (method.ReturnType == typeof(void))
                        {
                            sb.AppendLine($"            base.{method.Name}({GetParameterNames(method.GetParameters())});");
                        }
                        else
                        {
                            sb.AppendLine($"            return base.{method.Name}({GetParameterNames(method.GetParameters())});");
                        }
                    }

                    sb.AppendLine("        }");
                }
            }
        }

        /// <summary>
        /// 生成虚属性重写
        /// </summary>
        private static void GenerateVirtualProperties(StringBuilder sb, Type baseType)
        {
            PropertyInfo[] properties = baseType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            bool isInterface = baseType.IsInterface;

            foreach (PropertyInfo property in properties)
            {
                // 跳过排除列表中的属性
                if (ExcludedMethods.Contains(property.Name))
                    continue;

                MethodInfo getter = property.GetGetMethod(true);
                MethodInfo setter = property.GetSetMethod(true);

                bool shouldGenerate = false;

                if (isInterface)
                {
                    // 接口：生成所有属性
                    shouldGenerate = property.DeclaringType == baseType;
                }
                else
                {
                    // 类：只处理虚属性
                    bool isVirtualGetter = getter != null && getter.IsVirtual && !getter.IsFinal;
                    bool isVirtualSetter = setter != null && setter.IsVirtual && !setter.IsFinal;
                    shouldGenerate = (isVirtualGetter || isVirtualSetter) && property.DeclaringType == baseType;
                }

                if (shouldGenerate)
                {
                    string accessModifier = GetAccessModifier(property);
                    string propertyType = GetTypeName(property.PropertyType);
                    string overrideKeyword = isInterface ? "" : "override ";

                    sb.AppendLine();
                    sb.AppendLine($"        {accessModifier} {overrideKeyword}{propertyType} {property.Name}");
                    sb.AppendLine("        {");

                    if (getter != null)
                    {
                        if (isInterface)
                        {
                            sb.AppendLine("            get { throw new System.NotImplementedException(); }");
                        }
                        else
                        {
                            sb.AppendLine("            get { return base." + property.Name + "; }");
                        }
                    }

                    if (setter != null)
                    {
                        if (isInterface)
                        {
                            sb.AppendLine("            set { throw new System.NotImplementedException(); }");
                        }
                        else
                        {
                            sb.AppendLine("            set { base." + property.Name + " = value; }");
                        }
                    }

                    sb.AppendLine("        }");
                }
            }
        }

        /// <summary>
        /// 获取访问修饰符
        /// </summary>
        private static string GetAccessModifier(MethodInfo method)
        {
            if (method.IsPublic) return "public";
            if (method.IsFamily) return "protected";
            if (method.IsAssembly) return "internal";
            return "protected internal";
        }

        /// <summary>
        /// 获取访问修饰符
        /// </summary>
        private static string GetAccessModifier(PropertyInfo property)
        {
            MethodInfo accessor = property.GetGetMethod(true) ?? property.GetSetMethod(true);
            return accessor != null ? GetAccessModifier(accessor) : "public";
        }

        /// <summary>
        /// 获取类型名称
        /// </summary>
        private static string GetTypeName(Type type)
        {
            // 特殊处理void类型
            if (type == typeof(void))
                return "void";

            if (type.IsGenericType)
            {
                string name = type.Name.Split('`')[0];
                Type[] genericArgs = type.GetGenericArguments();
                string args = string.Join(", ", Array.ConvertAll(genericArgs, GetTypeName));
                return $"{name}<{args}>";
            }

            // 处理一些常见类型的别名
            if (type == typeof(int)) return "int";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(string)) return "string";
            if (type == typeof(object)) return "object";

            return type.Name;
        }

        /// <summary>
        /// 获取参数字符串
        /// </summary>
        private static string GetParametersString(ParameterInfo[] parameters)
        {
            List<string> paramStrings = new List<string>();
            foreach (ParameterInfo param in parameters)
            {
                paramStrings.Add($"{GetTypeName(param.ParameterType)} {param.Name}");
            }
            return string.Join(", ", paramStrings);
        }

        /// <summary>
        /// 获取参数名称字符串
        /// </summary>
        private static string GetParameterNames(ParameterInfo[] parameters)
        {
            List<string> paramNames = new List<string>();
            foreach (ParameterInfo param in parameters)
            {
                paramNames.Add(param.Name);
            }
            return string.Join(", ", paramNames);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 类工厂编辑器窗口 - UE风格版本
    /// </summary>
    public class ClassFactoryWindow : EditorWindow
    {
        // 搜索相关
        private string searchText = "";
        private string lastSearchText = "";
        private List<TypeSearchResult> searchResults = new List<TypeSearchResult>();
        private Vector2 scrollPosition;
        private bool showSearchResults = false;
        private Type selectedBaseType = null;

        // 创建相关
        private string subclassName = "NewClass";
        private string namespaceName = "";
        private string outputPath = "Assets/Scripts/";

        // UI相关
        private const float SEARCH_RESULT_HEIGHT = 60f;
        private const int MAX_SEARCH_RESULTS = 20;
        private GUIStyle searchResultStyle;
        private GUIStyle selectedResultStyle;
        private GUIStyle textStyle;
        private GUIStyle namespaceStyle;
        private GUIStyle assemblyStyle;
        private int selectedResultIndex = -1;

        // 性能优化 - 类型缓存
        private static List<Type> cachedTypes = null;
        private static bool isCacheInitialized = false;
        private double lastSearchTime = 0;
        private const double SEARCH_DELAY = 0.3; // 300ms延迟

        [MenuItem("Tools/Class Factory")]
        public static void ShowWindow()
        {
            ClassFactoryWindow window = GetWindow<ClassFactoryWindow>("Create C# Subclass");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        private void OnEnable()
        {
            // 初始化输出路径为当前选中的文件夹
            string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!string.IsNullOrEmpty(selectedPath) && AssetDatabase.IsValidFolder(selectedPath))
            {
                outputPath = selectedPath;
            }

            // 异步初始化类型缓存
            if (!isCacheInitialized)
            {
                InitializeTypeCache();
            }
        }

        /// <summary>
        /// 初始化类型缓存
        /// </summary>
        private void InitializeTypeCache()
        {
            if (cachedTypes != null) return;

            cachedTypes = new List<Type>();
            int totalTypes = 0;
            int publicTypes = 0;
            int filteredTypes = 0;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type[] types = assembly.GetTypes();
                    totalTypes += types.Length;

                    foreach (Type type in types)
                    {
                        // 缓存所有可用的类型：类、接口、抽象类、枚举、结构体等
                        // 只要求是public或nested public
                        if (type.IsPublic || type.IsNestedPublic)
                        {
                            publicTypes++;

                            // 排除编译器生成的类型（如<>c__DisplayClass等）
                            // 但保留泛型类型（如List<T>）
                            if (type.Name.Contains("<>") || type.Name.Contains("DisplayClass"))
                            {
                                filteredTypes++;
                                continue;
                            }

                            cachedTypes.Add(type);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 记录无法访问的程序集
                    Debug.LogWarning($"[Class Factory] Cannot access assembly {assembly.GetName().Name}: {ex.Message}");
                }
            }

            isCacheInitialized = true;
            Debug.Log($"[Class Factory] Type Cache Summary:");
            Debug.Log($"  - Total types: {totalTypes}");
            Debug.Log($"  - Public types: {publicTypes}");
            Debug.Log($"  - Filtered types: {filteredTypes}");
            Debug.Log($"  - Cached types: {cachedTypes.Count}");
        }

        private void InitStyles()
        {
            if (searchResultStyle == null)
            {
                searchResultStyle = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.UpperLeft,
                    padding = new RectOffset(10, 10, 8, 8),
                    margin = new RectOffset(0, 0, 2, 2)
                };
            }

            if (selectedResultStyle == null)
            {
                selectedResultStyle = new GUIStyle(searchResultStyle);
                selectedResultStyle.normal.background = MakeTexture(2, 2, new Color(0.3f, 0.5f, 0.8f, 0.5f));
            }

            if (textStyle == null)
            {
                textStyle = new GUIStyle(EditorStyles.boldLabel);
                textStyle.fontSize = 13;
                textStyle.padding = new RectOffset(0, 0, 0, 0);
                textStyle.wordWrap = false;
                textStyle.clipping = TextClipping.Clip;
            }

            if (namespaceStyle == null)
            {
                namespaceStyle = new GUIStyle(EditorStyles.label);
                namespaceStyle.fontSize = 11;
                namespaceStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
                namespaceStyle.padding = new RectOffset(0, 0, 0, 0);
                namespaceStyle.wordWrap = false;
                namespaceStyle.clipping = TextClipping.Clip;
            }

            if (assemblyStyle == null)
            {
                assemblyStyle = new GUIStyle(EditorStyles.miniLabel);
                assemblyStyle.fontSize = 10;
                assemblyStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                assemblyStyle.padding = new RectOffset(0, 0, 0, 0);
                assemblyStyle.wordWrap = false;
                assemblyStyle.clipping = TextClipping.Clip;
            }
        }

        private void OnGUI()
        {
            InitStyles();

            EditorGUILayout.Space(10);
            GUILayout.Label("Create C# Subclass", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            DrawSearchArea();
            EditorGUILayout.Space(10);
            DrawClassInfoArea();
            EditorGUILayout.Space(10);
            DrawCreateButton();

            // 如果正在等待搜索，持续重绘以触发延迟搜索
            if (!string.IsNullOrEmpty(searchText) &&
                searchText != lastSearchText &&
                EditorApplication.timeSinceStartup - lastSearchTime < SEARCH_DELAY)
            {
                Repaint();
            }
        }

        /// <summary>
        /// 绘制搜索区域
        /// </summary>
        private void DrawSearchArea()
        {
            EditorGUILayout.LabelField("Select Parent Class", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName("SearchField");

            // 搜索框
            string newSearchText = EditorGUILayout.TextField(searchText, GUI.skin.FindStyle("ToolbarSeachTextField") ?? GUI.skin.textField);

            // 清除按钮
            if (GUILayout.Button("×", GUILayout.Width(20)))
            {
                newSearchText = "";
                lastSearchText = "";
                selectedBaseType = null;
                searchResults.Clear();
                showSearchResults = false;
                GUI.FocusControl(null);
            }

            EditorGUILayout.EndHorizontal();

            // 当搜索文本改变时，使用延迟搜索
            if (newSearchText != searchText)
            {
                searchText = newSearchText;
                lastSearchTime = EditorApplication.timeSinceStartup;

                if (string.IsNullOrEmpty(searchText))
                {
                    showSearchResults = false;
                    searchResults.Clear();
                }
                else
                {
                    // 延迟搜索，避免每次输入都搜索
                    Repaint();
                }
            }

            // 延迟执行搜索
            if (!string.IsNullOrEmpty(searchText) &&
                EditorApplication.timeSinceStartup - lastSearchTime >= SEARCH_DELAY &&
                searchText != lastSearchText)
            {
                lastSearchText = searchText;
                PerformSearch(searchText);
                showSearchResults = true;
                Repaint();
            }

            // 显示搜索状态
            bool isSearching = !string.IsNullOrEmpty(searchText) &&
                               searchText != lastSearchText &&
                               EditorApplication.timeSinceStartup - lastSearchTime < SEARCH_DELAY;

            if (isSearching)
            {
                EditorGUILayout.HelpBox("Searching...", MessageType.None);
            }

            // 显示当前选中的基类
            if (selectedBaseType != null)
            {
                string typeLabel = GetTypeLabel(selectedBaseType);
                EditorGUILayout.HelpBox($"Selected: {selectedBaseType.FullName}\nType: {typeLabel}\nNamespace: {selectedBaseType.Namespace ?? "None"}", MessageType.Info);
            }

            // 绘制搜索结果
            if (showSearchResults && searchResults.Count > 0)
            {
                DrawSearchResults();
            }
            else if (showSearchResults && searchResults.Count == 0 && !string.IsNullOrEmpty(searchText) && !isSearching)
            {
                EditorGUILayout.HelpBox("No matching classes found.", MessageType.Warning);
            }
        }

        /// <summary>
        /// 绘制搜索结果列表
        /// </summary>
        private void DrawSearchResults()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"Search Results ({searchResults.Count})", EditorStyles.boldLabel);

            float maxHeight = Mathf.Min(searchResults.Count * SEARCH_RESULT_HEIGHT, 300f);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(maxHeight));

            for (int i = 0; i < searchResults.Count; i++)
            {
                TypeSearchResult result = searchResults[i];
                DrawSearchResultItem(result, i);
            }

            EditorGUILayout.EndScrollView();

            // 键盘导航
            HandleKeyboardNavigation();
        }

        /// <summary>
        /// 绘制单个搜索结果项
        /// </summary>
        private void DrawSearchResultItem(TypeSearchResult result, int index)
        {
            bool isSelected = (selectedResultIndex == index);

            // 使用EditorGUILayout来创建一个可点击的区域
            EditorGUILayout.BeginVertical(isSelected ? selectedResultStyle : searchResultStyle);

            GUILayout.Space(5);

            // 第一行：类名 + 类型标签
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(result.Type.Name, EditorStyles.boldLabel, GUILayout.ExpandWidth(true));

            // 类型标签
            string typeLabel = GetTypeLabel(result.Type);
            Color typeColor = GetTypeLabelColor(result.Type);
            GUIStyle typeLabelStyle = new GUIStyle(EditorStyles.miniLabel);
            typeLabelStyle.normal.textColor = typeColor;
            typeLabelStyle.fontStyle = FontStyle.Bold;
            typeLabelStyle.alignment = TextAnchor.MiddleRight;
            GUILayout.Label(typeLabel, typeLabelStyle, GUILayout.Width(80));

            EditorGUILayout.EndHorizontal();

            // 命名空间
            GUIStyle grayStyle = new GUIStyle(EditorStyles.label);
            grayStyle.normal.textColor = Color.gray;
            EditorGUILayout.LabelField(result.Type.Namespace ?? "Global Namespace", grayStyle);

            // 程序集
            GUIStyle miniStyle = new GUIStyle(EditorStyles.miniLabel);
            miniStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
            EditorGUILayout.LabelField(result.Type.Assembly.GetName().Name, miniStyle);

            GUILayout.Space(5);

            EditorGUILayout.EndVertical();

            // 获取刚才绘制的区域
            Rect lastRect = GUILayoutUtility.GetLastRect();

            // 检测点击
            if (Event.current.type == EventType.MouseDown && lastRect.Contains(Event.current.mousePosition))
            {
                SelectSearchResult(result);
                Event.current.Use();
            }

            // 检测悬停
            if (lastRect.Contains(Event.current.mousePosition) && selectedResultIndex != index)
            {
                selectedResultIndex = index;
                Repaint();
            }
        }

        /// <summary>
        /// 获取类型标签文本
        /// </summary>
        private string GetTypeLabel(Type type)
        {
            if (type.IsInterface) return "Interface";
            if (type.IsEnum) return "Enum";
            if (type.IsValueType) return "Struct";
            if (type.IsAbstract && type.IsSealed) return "Static Class";
            if (type.IsAbstract) return "Abstract Class";
            return "Class";
        }

        /// <summary>
        /// 获取类型标签颜色
        /// </summary>
        private Color GetTypeLabelColor(Type type)
        {
            if (type.IsInterface) return new Color(0.4f, 0.8f, 1.0f); // 浅蓝色
            if (type.IsEnum) return new Color(0.8f, 0.6f, 1.0f); // 紫色
            if (type.IsValueType) return new Color(0.4f, 1.0f, 0.6f); // 绿色
            if (type.IsAbstract) return new Color(1.0f, 0.8f, 0.4f); // 橙色
            return new Color(0.7f, 0.7f, 0.7f); // 灰色
        }

        /// <summary>
        /// 处理键盘导航
        /// </summary>
        private void HandleKeyboardNavigation()
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.DownArrow)
                {
                    selectedResultIndex = Mathf.Min(selectedResultIndex + 1, searchResults.Count - 1);
                    e.Use();
                    Repaint();
                }
                else if (e.keyCode == KeyCode.UpArrow)
                {
                    selectedResultIndex = Mathf.Max(selectedResultIndex - 1, 0);
                    e.Use();
                    Repaint();
                }
                else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                {
                    if (selectedResultIndex >= 0 && selectedResultIndex < searchResults.Count)
                    {
                        SelectSearchResult(searchResults[selectedResultIndex]);
                        e.Use();
                    }
                }
                else if (e.keyCode == KeyCode.Escape)
                {
                    showSearchResults = false;
                    e.Use();
                    Repaint();
                }
            }
        }

        /// <summary>
        /// 选择搜索结果
        /// </summary>
        private void SelectSearchResult(TypeSearchResult result)
        {
            selectedBaseType = result.Type;
            searchText = result.Type.Name;
            lastSearchText = searchText; // 防止重复搜索
            showSearchResults = false;
            selectedResultIndex = -1;

            // 自动填充命名空间
            if (!string.IsNullOrEmpty(result.Type.Namespace))
            {
                namespaceName = result.Type.Namespace;
            }

            GUI.FocusControl(null);
            Repaint();
        }

        /// <summary>
        /// 绘制类信息区域
        /// </summary>
        private void DrawClassInfoArea()
        {
            EditorGUILayout.LabelField("New Class Information", EditorStyles.boldLabel);

            subclassName = EditorGUILayout.TextField("Class Name", subclassName);
            namespaceName = EditorGUILayout.TextField("Namespace", namespaceName);

            EditorGUILayout.BeginHorizontal();
            outputPath = EditorGUILayout.TextField("Output Path", outputPath);
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Output Folder", outputPath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // 转换为相对于项目的路径
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        outputPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        outputPath = selectedPath;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制创建按钮
        /// </summary>
        private void DrawCreateButton()
        {
            EditorGUILayout.Space(10);

            GUI.enabled = selectedBaseType != null && !string.IsNullOrEmpty(subclassName);

            if (GUILayout.Button("Create Class", GUILayout.Height(35)))
            {
                CreateSubclass();
            }

            GUI.enabled = true;

            EditorGUILayout.Space(5);

            // 刷新缓存和调试按钮
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("This tool will automatically generate a subclass with overrides for all virtual methods and properties.", MessageType.Info);

            EditorGUILayout.BeginVertical(GUILayout.Width(120));
            if (GUILayout.Button("Refresh Cache", GUILayout.Height(20)))
            {
                RefreshTypeCache();
            }
            if (GUILayout.Button("Debug Type Search", GUILayout.Height(20)))
            {
                ShowDebugTypeWindow();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 显示类型搜索调试窗口
        /// </summary>
        private void ShowDebugTypeWindow()
        {
            // 使用简单的输入提示
            EditorApplication.delayCall += () =>
            {
                string message = "Enter the exact type name (interface/class name, not filename):\n\nExample: IAbilitySystemComponent";

                // 创建一个临时字符串来存储输入
                // 由于Unity没有内置的输入对话框，我们使用搜索框来输入
                if (!string.IsNullOrEmpty(searchText))
                {
                    DebugTypeSearch(searchText);
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "Debug Type Search",
                        "Please enter a type name in the search box above, then click this button again.\n\n" +
                        "Note: Search by TYPE NAME (e.g., IMyInterface), not by FILE NAME.",
                        "OK"
                    );
                }
            };
        }

        /// <summary>
        /// 调试类型搜索
        /// </summary>
        private void DebugTypeSearch(string typeName)
        {
            Debug.Log($"[Class Factory Debug] Searching for type: '{typeName}'");
            Debug.Log("==========================================");

            // 确保缓存已初始化
            if (cachedTypes == null || !isCacheInitialized)
            {
                InitializeTypeCache();
            }

            bool foundInCache = false;
            bool foundInAssembly = false;
            List<string> debugInfo = new List<string>();

            // 1. 检查缓存中是否存在
            foreach (Type type in cachedTypes)
            {
                if (type.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase) ||
                    type.FullName.Equals(typeName, StringComparison.OrdinalIgnoreCase))
                {
                    foundInCache = true;
                    debugInfo.Add($"✓ FOUND IN CACHE:");
                    debugInfo.Add($"  Name: {type.Name}");
                    debugInfo.Add($"  FullName: {type.FullName}");
                    debugInfo.Add($"  Type: {GetTypeLabel(type)}");
                    debugInfo.Add($"  IsPublic: {type.IsPublic}");
                    debugInfo.Add($"  IsNestedPublic: {type.IsNestedPublic}");
                    debugInfo.Add($"  Namespace: {type.Namespace ?? "None"}");
                    debugInfo.Add($"  Assembly: {type.Assembly.GetName().Name}");
                    break;
                }
            }

            // 2. 检查所有程序集
            if (!foundInCache)
            {
                debugInfo.Add("✗ NOT FOUND IN CACHE - Checking all assemblies...");

                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        Type[] types = assembly.GetTypes();
                        foreach (Type type in types)
                        {
                            if (type.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase) ||
                                type.FullName.Equals(typeName, StringComparison.OrdinalIgnoreCase))
                            {
                                foundInAssembly = true;
                                debugInfo.Add($"✓ FOUND IN ASSEMBLY (but not cached):");
                                debugInfo.Add($"  Name: {type.Name}");
                                debugInfo.Add($"  FullName: {type.FullName}");
                                debugInfo.Add($"  Type: {GetTypeLabel(type)}");
                                debugInfo.Add($"  IsPublic: {type.IsPublic}");
                                debugInfo.Add($"  IsNestedPublic: {type.IsNestedPublic}");
                                debugInfo.Add($"  IsVisible: {type.IsVisible}");
                                debugInfo.Add($"  Namespace: {type.Namespace ?? "None"}");
                                debugInfo.Add($"  Assembly: {type.Assembly.GetName().Name}");

                                // 分析为什么没被缓存
                                debugInfo.Add("  WHY NOT CACHED:");
                                if (!type.IsPublic && !type.IsNestedPublic)
                                {
                                    debugInfo.Add("    ✗ Not public or nested public (internal/private/protected)");
                                }
                                if (type.Name.Contains("<>"))
                                {
                                    debugInfo.Add("    ✗ Contains '<>' (compiler generated)");
                                }
                                if (type.Name.Contains("DisplayClass"))
                                {
                                    debugInfo.Add("    ✗ Contains 'DisplayClass' (compiler generated)");
                                }
                                break;
                            }
                        }
                        if (foundInAssembly) break;
                    }
                    catch (Exception ex)
                    {
                        // Ignore assembly load errors
                    }
                }
            }

            if (!foundInAssembly && !foundInCache)
            {
                debugInfo.Add("✗ NOT FOUND ANYWHERE");
                debugInfo.Add("  Possible reasons:");
                debugInfo.Add("  - Type name is incorrect");
                debugInfo.Add("  - Type is in a non-loaded assembly");
                debugInfo.Add("  - Script compilation error");
                debugInfo.Add("  - Type is internal/private");
            }

            // 输出所有调试信息
            foreach (string info in debugInfo)
            {
                Debug.Log($"[Class Factory Debug] {info}");
            }

            Debug.Log("==========================================");

            // 显示对话框
            string message = string.Join("\n", debugInfo);
            EditorUtility.DisplayDialog("Type Search Debug Result",
                $"Type: {typeName}\n\n{message}",
                "OK");
        }

        /// <summary>
        /// 刷新类型缓存
        /// </summary>
        private void RefreshTypeCache()
        {
            cachedTypes = null;
            isCacheInitialized = false;
            InitializeTypeCache();
            searchResults.Clear();
            showSearchResults = false;
            Debug.Log("[Class Factory] Type cache refreshed!");
        }

        /// <summary>
        /// 执行搜索
        /// </summary>
        private void PerformSearch(string query)
        {
            searchResults.Clear();
            selectedResultIndex = -1;

            if (string.IsNullOrEmpty(query)) return;

            // 确保缓存已初始化
            if (cachedTypes == null || !isCacheInitialized)
            {
                InitializeTypeCache();
            }

            query = query.ToLower();
            List<TypeSearchResult> allResults = new List<TypeSearchResult>();

            // 使用缓存的类型列表进行搜索
            foreach (Type type in cachedTypes)
            {
                // 计算匹配分数
                int score = CalculateMatchScore(type, query);
                if (score > 0)
                {
                    allResults.Add(new TypeSearchResult { Type = type, Score = score });
                }
            }

            // 按分数排序并取前N个结果
            searchResults = allResults
                .OrderByDescending(r => r.Score)
                .ThenBy(r => r.Type.Name)
                .Take(MAX_SEARCH_RESULTS)
                .ToList();

            Debug.Log($"[Class Factory] Search '{query}' found {searchResults.Count} results");
            if (searchResults.Count > 0)
            {
                Debug.Log($"[Class Factory] First result: {searchResults[0].Type.Name} ({searchResults[0].Type.Namespace})");
            }
        }

        /// <summary>
        /// 计算类型与查询的匹配分数
        /// </summary>
        private int CalculateMatchScore(Type type, string query)
        {
            string typeName = type.Name.ToLower();
            string fullName = type.FullName?.ToLower() ?? typeName;

            // 完全匹配 - 最高分
            if (typeName == query) return 1000;

            // 开头匹配 - 高分
            if (typeName.StartsWith(query)) return 500;

            // 包含匹配 - 中等分
            if (typeName.Contains(query)) return 200;

            // 驼峰匹配 (例如: "abc" 匹配 "AbilitySystemComponent")
            if (IsCamelCaseMatch(typeName, query)) return 300;

            // 全名匹配 - 低分
            if (fullName.Contains(query)) return 100;

            // 模糊匹配 - 最低分
            if (IsFuzzyMatch(typeName, query)) return 50;

            return 0;
        }

        /// <summary>
        /// 驼峰命名匹配
        /// </summary>
        private bool IsCamelCaseMatch(string text, string query)
        {
            int queryIndex = 0;
            for (int i = 0; i < text.Length && queryIndex < query.Length; i++)
            {
                if (char.IsUpper(text[i]) || i == 0)
                {
                    if (char.ToLower(text[i]) == query[queryIndex])
                    {
                        queryIndex++;
                    }
                }
            }
            return queryIndex == query.Length;
        }

        /// <summary>
        /// 模糊匹配
        /// </summary>
        private bool IsFuzzyMatch(string text, string query)
        {
            int queryIndex = 0;
            for (int i = 0; i < text.Length && queryIndex < query.Length; i++)
            {
                if (text[i] == query[queryIndex])
                {
                    queryIndex++;
                }
            }
            return queryIndex == query.Length;
        }

        /// <summary>
        /// 创建子类
        /// </summary>
        private void CreateSubclass()
        {
            if (selectedBaseType == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a parent class first.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(subclassName))
            {
                EditorUtility.DisplayDialog("Error", "Please enter a class name.", "OK");
                return;
            }

            bool success = ClassFactory.CreateSubclass(selectedBaseType, subclassName, namespaceName, outputPath);
            if (success)
            {
                EditorUtility.DisplayDialog("Success", $"Class '{subclassName}' created successfully!\n\nPath: {outputPath}/{subclassName}.cs", "OK");

                // 清空输入，准备创建下一个
                subclassName = "NewClass";
                selectedBaseType = null;
                searchText = "";
                lastSearchText = "";
                searchResults.Clear();
                showSearchResults = false;
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Failed to create class. Check the console for details.", "OK");
            }
        }

        /// <summary>
        /// 创建纯色纹理
        /// </summary>
        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// 搜索结果类
        /// </summary>
        private class TypeSearchResult
        {
            public Type Type;
            public int Score;
        }
    }
#endif
}
