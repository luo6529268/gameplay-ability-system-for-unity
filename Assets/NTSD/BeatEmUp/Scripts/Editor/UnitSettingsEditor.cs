using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

namespace BeatEmUpTemplate2D
{

    /// <summary>
    /// UnitSettingsEditor类是为UnitSettings组件创建的自定义编辑器
    /// 继承自Editor类，用于在Unity编辑器中自定义UnitSettings组件的显示和编辑方式
    /// </summary>
    [CanEditMultipleObjects]  // 允许同时编辑多个选中的UnitSettings组件
    [CustomEditor(typeof(UnitSettings))]  // 指定这个编辑器是用于UnitSettings类型的组件
    class UnitSettingsEditor : Editor
    {

        /// <summary>
        /// 静态字典，用于存储各个折叠面板的展开状态
        /// 使用静态是为了防止直接从Project文件夹编辑设置时出现奇怪的行为
        /// （这种情况下编辑器更新会重置这些布尔值）
        /// </summary>
        public static Dictionary<string, bool> foldOutList = new Dictionary<string, bool> {
        { "linkedComponentsFoldout", false },  // 关联组件折叠面板状态
        { "movementFoldout", false },          // 移动设置折叠面板状态
        { "jumpFoldout", false },              // 跳跃设置折叠面板状态
        { "attackDataFoldout", false },        // 攻击数据折叠面板状态
        { "comboDataFoldout", false },         // 连击数据折叠面板状态
        { "knockDownFoldout", false },         // 击倒设置折叠面板状态
        { "throwFoldout", false },             // 投掷设置折叠面板状态
        { "defenceFoldout", false },           // 防御设置折叠面板状态
        { "grabFoldout", false },              // 抓取设置折叠面板状态
        { "weaponFoldout", false },            // 武器设置折叠面板状态
        { "unitNameFoldout", false },          // 单位名称折叠面板状态
        { "fovFoldout", false },               // 视野设置折叠面板状态
    };


        //缓存序列化的属性字段（用于支持多编辑）
        private SerializedProperty[] properties;
        private HashSet<string> linkedComponentFields = new HashSet<string> { "shadowPrefab", "weaponBone", "hitEffect", "hitBox", "spriteRenderer" };
        private HashSet<string> movementFields = new HashSet<string> { "startDirection", "moveSpeed", "moveSpeedAir", "useAcceleration" };
        private HashSet<string> accelerationFields = new HashSet<string> { "moveAcceleration", "moveDeceleration" };
        private HashSet<string> jumpFields = new HashSet<string> { "jumpHeight", "jumpSpeed", "jumpGravity", "verticalMoveSpeed", "minGroundHeight", "maxGroundHeight" };
        private HashSet<string> attackDataFields = new HashSet<string> { "jumpPunch", "jumpKick", "grabPunch", "grabKick", "grabThrow", "groundPunch", "groundKick" };
        private HashSet<string> comboDataFields = new HashSet<string> { "comboResetTime", "continueComboOnHit" };
        private HashSet<string> knockdownFields = new HashSet<string> { "knockDownHeight", "knockDownDistance", "knockDownSpeed", "knockDownFloorTime", "hitOtherEnemiesDuringFall", "hitOtherEnemiesWhenFalling" };
        private HashSet<string> throwFields = new HashSet<string> { "throwHeight", "throwDistance", "hitOtherEnemiesWhenThrown" };
        private HashSet<string> defenceFieldsPlayer = new HashSet<string> { "canChangeDirWhileDefending", "rearDefenseEnabled" };
        private HashSet<string> defenceFieldsEnemy = new HashSet<string> { "defendChance", "defendDuration", "rearDefenseEnabled" };
        private HashSet<string> grabFields = new HashSet<string> { "grabAnimation", "grabPosition", "grabDuration" };
        private HashSet<string> weaponFields = new HashSet<string> { "loseWeaponWhenHit", "loseWeaponWhenKnockedDown" };
        private HashSet<string> unitNameFieldsPlayer = new HashSet<string> { "unitName", "unitPortrait", "showNameInAllCaps" };
        private HashSet<string> unitNameFieldsEnemy = new HashSet<string> { "unitName", "showNameInAllCaps", "unitPortrait", "loadRandomNameFromList" };
        private HashSet<string> fovFields = new HashSet<string> { "enableFOV", "viewDistance", "viewAngle", "viewPosOffset", "showFOVCone", "targetInSight" };

        //icons
        private Texture2D iconArrowClose;
        private Texture2D iconArrowOpen;
        private Texture2D iconInfo;

        //other
        private DIRECTION prevDirection = DIRECTION.LEFT; //用于跟踪编辑器中的方向变化

        private string newline = "\n\n";
        private string space = "  ";

        /**
         * OnEnable方法
         * 当脚本对象被启用时调用，用于初始化和加载资源
         */
        void OnEnable()
        {
            //加载图标资源
            //从Resources文件夹中加载关闭状态的箭头图标
            iconArrowClose = Resources.Load<Texture2D>("IconArrowClose");
            //从Resources文件夹中加载打开状态的箭头图标
            iconArrowOpen = Resources.Load<Texture2D>("IconArrowOpen");
            //从Resources文件夹中加载信息图标
            iconInfo = Resources.Load<Texture2D>("IconInfo");

            //获取所有序列化属性并进行缓存
            //调用方法缓存所有需要序列化的属性，以提高性能
            CacheSerializedProperties();
        }

        /**
         * OnInspectorGUI方法
         * 重写Editor类的OnInspectorGUI方法，用于绘制自定义检视面板界面
         */
        public override void OnInspectorGUI()
        {
            //获取当前选中的目标对象
            var settings = (UnitSettings)target;
            //如果目标对象为空，直接返回
            if (settings == null) return;

            //启用撤销功能
            //记录对象的当前状态，以便可以撤销后续的修改
            Undo.RecordObject(settings, "Undo change settings");

            //开始检查属性变更
            //更新序列化对象的状态
            serializedObject.Update();
            //开始检查GUI控件的变更
            EditorGUI.BeginChangeCheck();

            //绘制主要内容
            //调用方法绘制检视面板的主要内容
            MainContent(settings);

            //保存变更
            //如果检测到有变更发生
            if (EditorGUI.EndChangeCheck())
            {
                //应用修改后的属性
                serializedObject.ApplyModifiedProperties();
                //标记对象为已修改，确保更改被保存
                EditorUtility.SetDirty(settings);
            }
        }


        /**
         * 主内容绘制函数
         * 用于绘制单位设置界面的主要内容，包括各种属性和设置项
         * @param settings 单位设置对象，包含所有需要配置的单位属性
         */
        void MainContent(UnitSettings settings)
        {

            // 绘制单位类型属性
            DrawPropertyField("unitType");

            


            // 绘制链接组件部分
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(space + "Linked Components - 链接组件", GetArrow(foldOutList["linkedComponentsFoldout"])), FoldOutStyle(), GUILayout.ExpandWidth(true), GUILayout.Height(30))) foldOutList["linkedComponentsFoldout"] = !foldOutList["linkedComponentsFoldout"];
            if (GUILayout.Button(new GUIContent("", GetInfoIcon()), FoldOutStyle(), GUILayout.Width(50), GUILayout.Height(30))) showInfo(0, settings.unitType);
            EditorGUILayout.EndHorizontal();

            // 如果链接组件折叠面板展开，则绘制相关属性
            if (foldOutList["linkedComponentsFoldout"])
            {
                EditorGUI.indentLevel++;
                DrawPropertyFields(linkedComponentFields);
                EditorGUI.indentLevel--;
            }

            // 绘制移动设置部分
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(space + "Movement Settings - 移动设置", GetArrow(foldOutList["movementFoldout"])), FoldOutStyle(), GUILayout.ExpandWidth(true), GUILayout.Height(30))) foldOutList["movementFoldout"] = !foldOutList["movementFoldout"];
            if (GUILayout.Button(new GUIContent("", GetInfoIcon()), FoldOutStyle(), GUILayout.Width(50), GUILayout.Height(30))) showInfo(1, settings.unitType);
            EditorGUILayout.EndHorizontal();

            // 如果移动设置折叠面板展开，则绘制相关属性
            if (foldOutList["movementFoldout"])
            {
                EditorGUI.indentLevel++;
                DrawPropertyFields(movementFields);

                // 处理单位在关卡中的旋转
                if (prevDirection != settings.startDirection)
                {
                    settings.transform.localRotation = (settings.startDirection == DIRECTION.LEFT) ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
                    prevDirection = settings.startDirection;
                }

                // 如果使用加速度，则绘制加速度相关属性
                if (settings.useAcceleration)
                {
                    EditorGUI.indentLevel++;
                    DrawPropertyFields(accelerationFields);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }

            // 绘制跳跃设置部分
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(space + "Jump Settings - 跳跃设置", GetArrow(foldOutList["jumpFoldout"])), FoldOutStyle(), GUILayout.ExpandWidth(true), GUILayout.Height(30))) foldOutList["jumpFoldout"] = !foldOutList["jumpFoldout"];
            if (GUILayout.Button(new GUIContent("", GetInfoIcon()), FoldOutStyle(), GUILayout.Width(50), GUILayout.Height(30))) showInfo(2, settings.unitType);
            EditorGUILayout.EndHorizontal();

            // 如果跳跃设置折叠面板展开，则绘制相关属性
            if (foldOutList["jumpFoldout"])
            {
                EditorGUI.indentLevel++;
                DrawPropertyFields(jumpFields);
                EditorGUI.indentLevel--;
            }

            // 如果是玩家单位，绘制攻击数据部分
            if (settings.unitType == UNITTYPE.PLAYER)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent(space + "Attack Data - 攻击数据", GetArrow(foldOutList["attackDataFoldout"])), FoldOutStyle(), GUILayout.ExpandWidth(true), GUILayout.Height(30))) foldOutList["attackDataFoldout"] = !foldOutList["attackDataFoldout"];
                if (GUILayout.Button(new GUIContent("", GetInfoIcon()), FoldOutStyle(), GUILayout.Width(50), GUILayout.Height(30))) showInfo(3, settings.unitType);
                EditorGUILayout.EndHorizontal();

                // 显示攻击数据
                if (foldOutList["attackDataFoldout"])
                {
                    EditorGUI.indentLevel++;
                    foreach (string attack in attackDataFields) ShowAttackData(GetPropertyByName(attack), false);
                    EditorGUI.indentLevel--;
                }

                // 绘制连击数据部分
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent(space + "Combo Data - 连击数据", GetArrow(foldOutList["comboDataFoldout"])), FoldOutStyle(), GUILayout.ExpandWidth(true), GUILayout.Height(30))) foldOutList["comboDataFoldout"] = !foldOutList["comboDataFoldout"];
                if (GUILayout.Button(new GUIContent("", GetInfoIcon()), FoldOutStyle(), GUILayout.Width(50), GUILayout.Height(30))) showInfo(4, settings.unitType);
                EditorGUILayout.EndHorizontal();

                // 如果连击数据折叠面板展开，则绘制相关属性
                if (foldOutList["comboDataFoldout"])
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.Space(5);
                    ShowHeader("Combo Settings");
                    DrawPropertyFields(comboDataFields);
                    EditorGUILayout.Space(5);
                    ShowHeader("Combo List");
                    ShowComboData(settings.comboData);
                    EditorGUI.indentLevel--;
                }
            }

            // 如果是敌人单位，绘制攻击数据部分
            if (settings.unitType == UNITTYPE.ENEMY)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent(space + "Attack Data - 攻击数据", GetArrow(foldOutList["attackDataFoldout"])), FoldOutStyle(), GUILayout.ExpandWidth(true), GUILayout.Height(30))) foldOutList["attackDataFoldout"] = !foldOutList["attackDataFoldout"];
                if (GUILayout.Button(new GUIContent("", GetInfoIcon()), FoldOutStyle(), GUILayout.Width(50), GUILayout.Height(30))) showInfo(3, settings.unitType);
                EditorGUILayout.EndHorizontal();

                // 如果攻击数据折叠面板展开，则绘制相关属性
                if (foldOutList["attackDataFoldout"])
                {
                    EditorGUI.indentLevel++;
                    if (settings.unitType == UNITTYPE.ENEMY) DrawPropertyField("enemyPauseBeforeAttack");
                    EditorGUILayout.Space(5);
                    ShowHeader("Enemy Attack List");
                    ShowEnemyAttackData();
                    EditorGUI.indentLevel--;
                }
            }



            // 绘制击倒设置部分
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(space + "KnockDown Settings - 击倒设置", GetArrow(foldOutList["knockDownFoldout"])), FoldOutStyle(), GUILayout.ExpandWidth(true), GUILayout.Height(30))) foldOutList["knockDownFoldout"] = !foldOutList["knockDownFoldout"];
            if (GUILayout.Button(new GUIContent("", GetInfoIcon()), FoldOutStyle(), GUILayout.Width(50), GUILayout.Height(30))) showInfo(5, settings.unitType);
            EditorGUILayout.EndHorizontal();

            // 如果击倒设置折叠面板展开，则绘制相关属性
            if (foldOutList["knockDownFoldout"])
            {
                EditorGUI.indentLevel++;
                DrawPropertyField("canBeKnockedDown");
                if (settings.canBeKnockedDown) DrawPropertyFields(knockdownFields);
                EditorGUI.indentLevel--;
            }

            // 绘制投掷设置部分
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(space + "Throw Settings - 投掷设置", GetArrow(foldOutList["throwFoldout"])), FoldOutStyle(), GUILayout.ExpandWidth(true), GUILayout.Height(30))) foldOutList["throwFoldout"] = !foldOutList["throwFoldout"];
            if (GUILayout.Button(new GUIContent("", GetInfoIcon()), FoldOutStyle(), GUILayout.Width(50), GUILayout.Height(30))) showInfo(6, settings.unitType);
            EditorGUILayout.EndHorizontal();

            // 如果投掷设置折叠面板展开，则绘制相关属性
            if (foldOutList["throwFoldout"])
            {
                EditorGUI.indentLevel++;
                DrawPropertyFields(throwFields);
                EditorGUI.indentLevel--;
            }

            // 绘制防御设置部分
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(space + "Defence Settings - 防御设置", GetArrow(foldOutList["defenceFoldout"])), FoldOutStyle(), GUILayout.ExpandWidth(true), GUILayout.Height(30))) foldOutList["defenceFoldout"] = !foldOutList["defenceFoldout"];
            if (GUILayout.Button(new GUIContent("", GetInfoIcon()), FoldOutStyle(), GUILayout.Width(50), GUILayout.Height(30))) showInfo(7, settings.unitType);
            EditorGUILayout.EndHorizontal();

            // 如果防御设置折叠面板展开，则根据单位类型绘制相关属性
            if (foldOutList["defenceFoldout"])
            {
                EditorGUI.indentLevel++;
                if (settings.unitType == UNITTYPE.ENEMY) DrawPropertyFields(defenceFieldsEnemy);
                else if (settings.unitType == UNITTYPE.PLAYER) DrawPropertyFields(defenceFieldsPlayer);
                EditorGUI.indentLevel--;
            }

            // 绘制抓取设置部分
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(space + "Grab Settings - 抓取设置", GetArrow(foldOutList["grabFoldout"])), FoldOutStyle(), GUILayout.ExpandWidth(true), GUILayout.Height(30))) foldOutList["grabFoldout"] = !foldOutList["grabFoldout"];
            if (GUILayout.Button(new GUIContent("", GetInfoIcon()), FoldOutStyle(), GUILayout.Width(50), GUILayout.Height(30))) showInfo(8, settings.unitType);
            EditorGUILayout.EndHorizontal();

            // 如果抓取设置折叠面板展开，则根据单位类型绘制相关属性
            if (foldOutList["grabFoldout"])
            {
                EditorGUI.indentLevel++;
                if (settings.unitType == UNITTYPE.PLAYER) DrawPropertyFields(grabFields);
                if (settings.unitType == UNITTYPE.ENEMY) DrawPropertyField("canBeGrabbed");
                EditorGUI.indentLevel--;
            }

            // 如果是玩家单位，绘制武器设置部分
            if (settings.unitType == UNITTYPE.PLAYER)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent(space + "Weapon Settings - 武器设置", GetArrow(foldOutList["weaponFoldout"])), FoldOutStyle(), GUILayout.ExpandWidth(true), GUILayout.Height(30))) foldOutList["weaponFoldout"] = !foldOutList["weaponFoldout"];
                if (GUILayout.Button(new GUIContent("", GetInfoIcon()), FoldOutStyle(), GUILayout.Width(50), GUILayout.Height(30))) showInfo(9, settings.unitType);
                EditorGUILayout.EndHorizontal();

                // 如果武器设置折叠面板展开，则绘制相关属性
                if (foldOutList["weaponFoldout"])
                {
                    EditorGUI.indentLevel++;
                    DrawPropertyFields(weaponFields);
                    EditorGUI.indentLevel--;
                }
            }

            // 绘制单位名称和头像设置部分
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(space + "Unit Name & Portrait - 单位名称", GetArrow(foldOutList["unitNameFoldout"])), FoldOutStyle(), GUILayout.ExpandWidth(true), GUILayout.Height(30))) foldOutList["unitNameFoldout"] = !foldOutList["unitNameFoldout"];
            if (GUILayout.Button(new GUIContent("", GetInfoIcon()), FoldOutStyle(), GUILayout.Width(50), GUILayout.Height(30))) showInfo(10, settings.unitType);
            EditorGUILayout.EndHorizontal();

            // 如果单位名称折叠面板展开，则根据单位类型绘制相关属性
            if (foldOutList["unitNameFoldout"])
            {
                EditorGUI.indentLevel++;
                if (settings.unitType == UNITTYPE.PLAYER) DrawPropertyFields(unitNameFieldsPlayer);
                if (settings.unitType == UNITTYPE.ENEMY)
                {
                    DrawPropertyFields(unitNameFieldsEnemy);
                    if (settings.loadRandomNameFromList) DrawPropertyField("unitNamesList");
                }
                EditorGUI.indentLevel--;
            }

            // 如果是敌人单位，绘制视野设置部分
            if (settings.unitType == UNITTYPE.ENEMY)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent(space + "Field Of View Settings - 视野设置", GetArrow(foldOutList["fovFoldout"])), FoldOutStyle(), GUILayout.ExpandWidth(true), GUILayout.Height(30))) foldOutList["fovFoldout"] = !foldOutList["fovFoldout"];
                if (GUILayout.Button(new GUIContent("", GetInfoIcon()), FoldOutStyle(), GUILayout.Width(50), GUILayout.Height(30))) showInfo(11, settings.unitType);
                EditorGUILayout.EndHorizontal();

                // 如果视野设置折叠面板展开，则绘制相关属性
                if (foldOutList["fovFoldout"])
                {
                    EditorGUI.indentLevel++;
                    DrawPropertyFields(fovFields);
                    EditorGUI.indentLevel--;
                }
            }
        }


        /*********************************************************************************
                 * 方法名：ShowEnemyAttackData
                 * 功能：可视化显示敌人的攻击数据列表
                 * 描述：在Unity编辑器中显示敌人攻击数据的列表，并提供添加和删除功能
                 *********************************************************************************/
        void ShowEnemyAttackData()
        {

            // 获取enemyAttackList序列化属性
            SerializedProperty enemyAttackListProperty = serializedObject.FindProperty("enemyAttackList");

            // 检查列表是否存在且是数组类型
            if (enemyAttackListProperty != null && enemyAttackListProperty.isArray)
            {

                // 当列表为空时显示提示信息
                if (enemyAttackListProperty.arraySize == 0) EditorGUILayout.LabelField("No Enemy Attack Data Available");

                // 遍历并显示每个攻击数据
                for (int i = 0; i < enemyAttackListProperty.arraySize; i++)
                {
                    SerializedProperty attackDataProperty = enemyAttackListProperty.GetArrayElementAtIndex(i);
                    ShowAttackData(attackDataProperty, true);
                }

                // 开始创建底部按钮区域
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(" ", GUILayout.Width(17));

                // 删除按钮（-）
                // 仅当列表中至少有一个元素时才显示删除按钮
                if (enemyAttackListProperty.arraySize > 0)
                {
                    // 点击删除按钮时移除最后一个元素
                    if (GUILayout.Button("-", smallButtonStyle())) enemyAttackListProperty.DeleteArrayElementAtIndex(enemyAttackListProperty.arraySize - 1);
                }

                // 添加按钮（+）
                // 点击添加按钮时在列表末尾插入新元素
                if (GUILayout.Button("+", smallButtonStyle(), GUILayout.Width(25))) enemyAttackListProperty.InsertArrayElementAtIndex(enemyAttackListProperty.arraySize);

                // 结束按钮区域布局
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(10);
            }
        }


        /**
         * 显示连招数据的可视化界面
         * @param comboList 连招列表，包含所有需要显示的连招数据
         */
        void ShowComboData(List<Combo> comboList)
        {
            // 检查是否有连招数据，如果没有则显示提示信息
            if (comboList.Count == 0) EditorGUILayout.LabelField("No Combo Data Available");

            // 遍历所有连招数据并创建可视化界面
            foreach (Combo combo in comboList)
            {
                // 创建可折叠的连招标题栏
                combo.foldout = EditorGUILayout.Foldout(combo.foldout, combo.comboName, true);
                if (combo.foldout)
                {
                    // 显示连招名称输入框
                    combo.comboName = EditorGUILayout.TextField("Combo Name:", combo.comboName);

                    // 显示攻击序列标题
                    ShowHeader("Attack Sequence");

                    // 显示攻击序列列表
                    if (combo.attackSequence.Count == 0) EditorGUILayout.LabelField("This combo does not have any attacks listed");
                    // 遍历并显示每个攻击数据
                    foreach (AttackData data in combo.attackSequence)
                    {
                        // 增加缩进级别
                        EditorGUI.indentLevel++;
                        // 创建可折叠的攻击数据标题栏
                        data.foldout = EditorGUILayout.Foldout(data.foldout, data.name, true);
                        if (data.foldout)
                        {
                            // 显示攻击数据的各个属性
                            data.name = EditorGUILayout.TextField("Attack Name:", data.name);
                            data.damage = EditorGUILayout.IntField("Damage", data.damage);
                            data.sfx = EditorGUILayout.TextField("Sfx (on hit)", data.sfx);
                            data.animationState = EditorGUILayout.TextField("Animation StateNode", data.animationState);
                            data.attackType = (ATTACKTYPE)EditorGUILayout.EnumPopup("Attack Type", data.attackType);
                            data.knockdown = EditorGUILayout.Toggle("Knockdown", data.knockdown);
                            GUILayout.Space(10);
                        }
                        // 恢复缩进级别
                        EditorGUI.indentLevel--;
                    }

                    // 显示底部按钮区域
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(" ", GUILayout.Width(17));

                    // 删除最后一个攻击的按钮
                    if (comboList.Count > 0) if (GUILayout.Button("-", smallButtonStyle())) combo.attackSequence.RemoveAt(combo.attackSequence.Count - 1);

                    // 添加新攻击的按钮
                    if (GUILayout.Button("+", smallButtonStyle(), GUILayout.Width(25))) combo.attackSequence.Add(new AttackData("[New Attack]", 0, null, ATTACKTYPE.NONE, false));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(10);
                }
            }

            // 显示连招列表的底部按钮
            EditorGUILayout.Space(15);
            EditorGUILayout.BeginHorizontal();
            // 添加新连招的按钮
            if (GUILayout.Button("Add Combo", GUILayout.Width(200), GUILayout.Height(25))) comboList.Add(new Combo());
            // 删除最后一个连招的按钮
            if (comboList.Count > 0) if (GUILayout.Button("Remove Combo", GUILayout.Width(200), GUILayout.Height(25))) comboList.RemoveAt(comboList.Count - 1);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
        }

        /**
         * 显示攻击数据的可视化界面
         * @param property 攻击数据的序列化属性
         * @param showName 是否显示攻击名称
         */
        void ShowAttackData(SerializedProperty property, bool showName)
        {
            // 增加缩进级别
            EditorGUI.indentLevel++;

            // 获取各个属性的序列化引用
            SerializedProperty foldout = property.FindPropertyRelative("foldout");
            SerializedProperty nameProp = property.FindPropertyRelative("name");

            // 设置折叠标题的名称
            if (showName)
            {
                // 如果显示名称，使用攻击名称作为标题
                string foldoutLabel = nameProp != null ? nameProp.stringValue : ObjectNames.NicifyVariableName(property.name);
                foldout.boolValue = EditorGUILayout.Foldout(foldout.boolValue, new GUIContent(foldoutLabel), true);
            }
            else
            {
                // 如果不显示名称，使用属性名称作为标题
                foldout.boolValue = EditorGUILayout.Foldout(foldout.boolValue, property.name, true);
            }

            // 如果展开，显示所有属性字段
            if (foldout.boolValue)
            {
                // 获取各个属性的序列化引用
                SerializedProperty damageProp = property.FindPropertyRelative("damage");
                SerializedProperty animationStateProp = property.FindPropertyRelative("animationState");
                SerializedProperty sfxProp = property.FindPropertyRelative("sfx");
                SerializedProperty attackTypeProp = property.FindPropertyRelative("attackType");
                SerializedProperty knockdownProp = property.FindPropertyRelative("knockdown");

                // 显示各个属性字段
                if (showName) EditorGUILayout.PropertyField(nameProp, new GUIContent("Attack Name:"));
                EditorGUILayout.PropertyField(damageProp, new GUIContent("Damage"));
                EditorGUILayout.PropertyField(animationStateProp, new GUIContent("Animation StateNode"));
                EditorGUILayout.PropertyField(sfxProp, new GUIContent("sfx"));
                EditorGUILayout.PropertyField(attackTypeProp, new GUIContent("Attack Type"));
                EditorGUILayout.PropertyField(knockdownProp, new GUIContent("Knockdown"));
                GUILayout.Space(10);
            }
            // 恢复缩进级别
            EditorGUI.indentLevel--;
        }


        /**
                 * 缓存所有序列化属性（用于多编辑支持）
                 * 通过反射获取UnitSettings类型的所有字段（包括公共和非公共实例字段）
                 * 并将这些字段转换为SerializedProperty数组进行缓存
                 */
        private void CacheSerializedProperties()
        {
            var targetType = typeof(UnitSettings);
            var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            properties = new SerializedProperty[fields.Length];
            for (int i = 0; i < fields.Length; i++) properties[i] = serializedObject.FindProperty(fields[i].Name);
        }

        /**
         * 绘制属性字段列表
         * @param propertyHash 包含需要绘制的属性名称的哈希集合
         * 遍历所有缓存的属性，如果属性存在于哈希集合中则进行绘制
         * 对于Sprite类型的属性，使用带缩略图的特殊绘制方式
         * 其他类型使用默认的属性字段绘制方式
         */
        public void DrawPropertyFields(HashSet<string> propertyHash)
        {
            foreach (var property in properties)
            {
                if (property != null && propertyHash.Contains(property.name))
                {

                    // 检查属性是否为Sprite类型
                    if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue is Sprite)
                    {

                        // 使用带缩略图的方式绘制Sprite字段
                        property.objectReferenceValue = (Sprite)EditorGUILayout.ObjectField(
                            new GUIContent(ObjectNames.NicifyVariableName(property.name)),
                            property.objectReferenceValue,
                            typeof(Sprite),
                            allowSceneObjects: false);

                    }
                    else
                    {

                        // 对其他类型使用默认方式绘制字段
                        EditorGUILayout.PropertyField(property, new GUIContent(ObjectNames.NicifyVariableName(property.name)));
                    }
                }
            }
        }

        /**
         * 根据属性名称获取缓存的属性
         * @param propertyName 要查找的属性名称
         * @return 找到的SerializedProperty对象，如果未找到则返回null
         */
        public SerializedProperty GetPropertyByName(string propertyName)
        {
            foreach (var property in properties) if (property != null && property.name == propertyName) return property;
            return null;
        }

        /**
         * 绘制单个属性字段
         * @param propertyName 要绘制的属性名称
         * 通过创建只包含单个属性名称的HashSet来调用DrawPropertyFields方法
         */
        public void DrawPropertyField(string propertyName)
        {
            DrawPropertyFields(new HashSet<string> { propertyName });
        }

        /**
         * 显示标题
         * @param label 要显示的标题文本
         * 创建并应用自定义的GUIStyle来显示格式化的标题文本
         */
        void ShowHeader(string label)
        {
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.wordWrap = true;
            style.richText = true;
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = 13;
            style.richText = true;
            style.padding = new RectOffset(16, 0, 4, 0);
            GUILayout.Label(label, style);
        }

        /**
         * 创建折叠按钮的GUIStyle
         * @return 配置好的GUIStyle对象
         * 根据编辑器当前是深色还是浅色模式来调整按钮样式
         */
        GUIStyle FoldOutStyle()
        {
            bool isDarkMode = EditorGUIUtility.isProSkin; // 检查Unity编辑器是深色还是浅色模式
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.alignment = TextAnchor.MiddleLeft; // 左对齐
            style.fixedHeight = 32;
            style.stretchWidth = true;
            style.padding = new RectOffset(12, 10, 0, 0);
            style.margin = new RectOffset(0, 0, 5, 5);
            style.normal.background = MakeTex(1, 1, new Color(1f, 1f, 1f, isDarkMode ? 0.1f : 0.2f));  // 设置按钮背景颜色
            style.normal.textColor = isDarkMode ? Color.white : Color.black;
            return style;
        }

        /**
         * 创建小型加减按钮的GUIStyle
         * @return 配置好的GUIStyle对象
         * 根据编辑器当前是深色还是浅色模式来调整按钮样式
         */
        GUIStyle smallButtonStyle()
        {
            bool isDarkMode = EditorGUIUtility.isProSkin; // 检查Unity编辑器是深色还是浅色模式
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.fixedHeight = 22;
            style.fixedWidth = 22;
            style.fontSize = 18;
            style.padding = new RectOffset(2, 2, 2, 2);
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.background = MakeTex(1, 1, new Color(1f, 1f, 1f, isDarkMode ? 0.1f : 0.2f));  // 设置按钮背景颜色
            style.normal.textColor = isDarkMode ? Color.white : Color.black;
            return style;
        }

        /**
         * 为折叠按钮创建背景纹理
         * @param width 纹理宽度
         * @param height 纹理高度
         * @param color 纹理颜色
         * @return 创建的Texture2D对象
         * 创建指定大小和颜色的纯色纹理，用于GUI元素的背景
         */
        private Texture2D MakeTex(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }


        /// <summary>
        /// 获取适当的箭头图标
        /// </summary>
        /// <param name="isFoldedOut">布尔值，表示当前是否展开状态</param>
        /// <returns>返回展开或关闭状态的箭头图标，如果图标为空则返回null</returns>
        Texture2D GetArrow(bool isFoldedOut)
        {
            if (iconArrowClose == null || iconArrowOpen == null) return null;
            else return isFoldedOut ? iconArrowOpen : iconArrowClose;
        }

        /// <summary>
        /// 获取信息图标
        /// </summary>
        /// <returns>返回信息图标，如果图标为空则返回null</returns>
        Texture2D GetInfoIcon()
        {
            if (iconInfo == null) return null;
            else return iconInfo;
        }

        /// <summary>
        /// 高亮显示文本项的快捷方法
        /// </summary>
        /// <param name="label">需要高亮显示的文本</param>
        /// <param name="size">文本大小，默认值为13</param>
        /// <returns>返回带有高亮效果的格式化文本</returns>
        string highlightItem(string label, int size = 13)
        {
            return "<b><size=" + size + "><color=#FFFFFF>" + label + "</color></size></b>";
        }


        //当用户按下?图标时显示文档
        public void showInfo(int id, UNITTYPE unitType)
        {
            string title = "";
            string content = "";

            /*
             * 这是一个根据不同的ID值来设置标题和内容的switch语句
             * 主要用于显示游戏单位(unit)的各种设置选项的说明文档
             * 每个case对应一个设置类别，包含标题和详细内容说明
             */
            switch (id)
            {
                case 0:
                    /* 链接组件设置 */
                    // 设置标题为"Linked Components"
                    title = "Linked Components";

                    // 设置内容说明，介绍本单元使用的组件和外部引用链接
                    content = "本部分包含本单元使用的多个组件和外部引用的链接。以下是每个项目的描述：" + newline;

                    // 添加阴影预制件的说明
                    content += highlightItem("Shadow Prefab: ") + "一个位于单位底部的阴影精灵。（可选）" + newline;

                    // 添加武器骨骼的说明
                    content += highlightItem("Weapon Bone: ") + "A transform that represents the position of the unit's hand. When a weapon is picked up, it will be parented to the weapon bone." + newline;

                    // 添加受击效果的说明
                    content += highlightItem("Hit Effect: ") + "An effect displayed when the unit is hit. (Optional)" + newline;

                    // 添加碰撞框的说明
                    content += highlightItem("Hitbox: ") + "A link to a sprite (red box) representing the hit area during an attack animation." + newline;

                    // 添加精灵渲染器的说明
                    content += highlightItem("Sprite Renderer: ") + "A Link to the sprite of this unit." + newline;

                    break;
                // 单位属性设置说明的switch语句，根据不同的case显示不同的设置说明
                case 1:
                    /* 移动设置 */
                    // 设置标题为"Movement Settings"
                    title = "Movement Settings";
                    // 设置内容说明，解释各个移动参数的含义
                    content = "These values define how fast a unit moves around a level. Here's what each term means:" + newline;
                    // 起始方向说明
                    content += highlightItem("Start Direction: ") + "The direction the unit faces at the beginning of a level." + newline;
                    // 移动速度说明
                    content += highlightItem("Move Speed: ") + "The unit's running speed when moving across the level." + newline;
                    // 空中移动速度说明
                    content += highlightItem("Move Speed Air: ") + "The unit's speed while in the air during a jump." + newline;
                    // 是否使用加速度说明
                    content += highlightItem("Use Acceleration: ") + "Option to enable or disable gradual speed changes." + newline;
                    // 移动加速度说明
                    content += highlightItem("Move Acceleration: ") + "The rate at which the unit's speed increases when accelerating." + newline;
                    // 移动减速度说明
                    content += highlightItem("Move Deceleration: ") + "The rate at which the unit's speed decreases when slowing down." + newline;
                    break;

                case 2:
                    /* 跳跃设置 */
                    // 设置标题为"Jump Settings"
                    title = "Jump Settings";
                    // 设置内容说明，解释跳跃相关参数
                    content = "These values define the jump behaviour of a unit." + newline;
                    // 跳跃高度说明
                    content += highlightItem("Jump Height: ") + "The height of a jump" + newline;
                    // 跳跃速度说明
                    content += highlightItem("Jump Speed: ") + "The speed of the jump simulation" + newline;
                    // 重力参数说明
                    content += highlightItem("Gravity: ") + "The strength of gravitational force applied to the character during a jump." + newline;
                    break;

                case 3:
                    /* 攻击数据设置 */
                    // 设置标题为"Attack Data"
                    title = "Attack Data";
                    // 设置内容说明，解释攻击相关参数
                    content = "This section provides a list of attack details, where you can modify data such as damage, animation, attack type, and other data for each attack." + newline;
                    // 伤害值说明
                    content += highlightItem("Damage: ") + "The amount of Health Points subtracted from the enemy's health bar." + newline;
                    // 动画状态说明
                    content += highlightItem("Animation StateNode: ") + "The animation state that needs to be played on this unit's Animator component." + newline;
                    // 攻击类型说明
                    content += highlightItem("Attack Type: ") + "The attack type of this attack." + newline;
                    // 是否击倒说明
                    content += highlightItem("Knockdown: ") + "Indicates if a successful hit causes the enemy to be knocked down." + newline;
                    break;

                case 4:
                    /* 连击设置 */
                    // 设置标题为"Combo Settings"
                    title = "Combo Settings";
                    // 设置内容说明，解释连击系统相关参数
                    content = "The combo section allows you to configure and manage sequential attacks. Here, you can set up a series of moves that will be executed in a specific order, creating a combo." + newline;
                    // 连击重置时间说明
                    content += highlightItem("Combo Reset Time: ") + "If the player presses a button within this time window, it will count as part of the combo sequence." + newline;
                    // 命中后继续连击说明
                    content += highlightItem("Continue Combo On Hit: ") + "Option to only proceed with the combo if the attack connects; otherwise, restart the combo sequence." + newline;
                    // 攻击序列标题
                    content += highlightItem("Attack Sequence") + "\n";

                    // 攻击序列参数说明
                    content += "For each combo attack you can modify data such as damage, animation, attack type:" + newline;
                    // 攻击名称说明
                    content += highlightItem("Attack Name: ") + "The name of this attack." + newline;
                    // 伤害值说明
                    content += highlightItem("Damage: ") + "The amount of Health Points subtracted from the enemy's health bar." + newline;
                    // 动画状态说明
                    content += highlightItem("Animation StateNode: ") + "The animation state that needs to be played on this unit's Animator component." + newline;
                    // 攻击类型说明
                    content += highlightItem("Attack Type: ") + "The attack type of this attack." + newline;
                    // 是否击倒说明
                    content += highlightItem("Knockdown: ") + "Indicates if a successful hit causes the enemy to be knocked down." + newline;
                    break;

                case 5:
                    /* 击倒设置 */
                    // 设置标题为"Knockdown Settings"
                    title = "Knockdown Settings";
                    // 设置内容说明，解释击倒相关参数
                    content = "These values determine how a unit behaves when knocked down:" + newline;
                    // 是否可被击倒说明
                    content += highlightItem("Can Be Knocked Down: ") + "Whether or not this unit can be knocked down." + newline;
                    // 击倒高度说明
                    content += highlightItem("Knockdown Height: ") + "The height to which a unit is propelled upward when knocked down." + newline;
                    // 击倒距离说明
                    content += highlightItem("Knockdown Distance: ") + "The distance a unit is pushed backward during a knockdown." + newline;
                    // 击倒速度说明
                    content += highlightItem("Knockdown Speed: ") + "The speed of the Knockdown simulation." + newline;
                    // 倒地时间说明
                    content += highlightItem("Knockdown Floor Time: ") + "The duration a unit remains on the ground before getting back up." + newline;
                    break;

                case 6:
                    /* 投掷设置 */
                    // 设置标题为"Throw Settings"
                    title = "Throw Settings";
                    // 设置内容说明，解释投掷相关参数
                    content = "These values determine how a unit behaves when being thrown:" + newline;
                    // 投掷高度说明
                    content += highlightItem("Throw Height: ") + "The height to which a unit is propelled upward when being thrown." + newline;
                    // 投掷距离说明
                    content += highlightItem("Throw Distance: ") + "The distance a unit travels while in the air after being thrown." + newline;
                    break;

                case 7:
                    /* 防御设置 */
                    // 设置标题为"Defence Settings"
                    title = "Defence Settings";
                    // 设置内容说明，解释防御相关参数
                    content = "These values determine how a unit behaves while defending:" + newline;
                    // 玩家防御时是否可以改变方向
                    if (unitType == UNITTYPE.PLAYER) content += highlightItem("Can Change Dir While Defending: ") + "Enable or disable the ability for the player to change direction while holding the defence button." + newline;
                    // 敌人防御概率
                    if (unitType == UNITTYPE.ENEMY) content += highlightItem("Defend Chance: ") + "The probability (0 - 100) that the enemy will successfully defend against an incoming attack." + newline;
                    // 敌人防御持续时间
                    if (unitType == UNITTYPE.ENEMY) content += highlightItem("Defend Duration: ") + "The amount of time an enemy remains in the defence state after initiating defense." + newline;
                    // 是否可以防御来自背后的攻击
                    content += highlightItem("Rear Defense Enabled: ") + "Determines whether this unit can defend against attacks coming from behind." + newline;
                    break;

                case 8:
                    /* 抓取设置 */
                    // 设置标题为"Grab Settings"
                    title = "Grab Settings";
                    // 设置内容说明，解释抓取相关参数
                    content = "These values determine how a player behaves when grabbing and holding an enemy:" + newline;
                    // 抓取动画说明
                    content += highlightItem("Grab Animation: ") + "The name of the animation state that contains the Grab Animation in this unit's Animator component" + newline;
                    // 抓取位置说明
                    content += highlightItem("Grab Position: ") + "The position this unit moves to while grabbing, relative to the enemy it is holding." + newline;
                    // 抓取持续时间说明
                    content += highlightItem("Grab Duration: ") + "The duration of the grab, before this unit and it's target return back to normal." + newline;
                    break;

                case 9:
                    /* 武器设置 */
                    // 设置标题为"Weapon Settings"
                    title = "Weapon Settings";
                    // 设置内容说明，解释武器相关参数
                    content = "\n";
                    // 被击中时是否掉落武器
                    content += highlightItem("Lose Weapon When Hit: ") + "Specifies whether the unit should drop the currently equipped weapon when hit." + newline;
                    // 被击倒时是否掉落武器
                    content += highlightItem("Lose Weapon When Knocked Down: ") + "Determines whether the unit retains or drops the currently equipped weapon when knocked down." + newline;
                    break;

                case 10:
                    /* 单位名称和肖像设置 */
                    // 设置标题为"Unit Name & Portrait"
                    title = "Unit Name & Portrait";
                    // 设置内容说明，解释名称和肖像相关参数
                    content = "\n";
                    // 敌人是否从列表中随机加载名称
                    if (unitType == UNITTYPE.ENEMY) content += highlightItem("Load Random Name From List: ") + "Option to load a random enemy name from a txt file." + newline;
                    // 玩家单位名称显示位置
                    if (unitType == UNITTYPE.PLAYER) content += highlightItem("Unit Name: ") + "The unit's name as shown in the top left corner near the health bar." + newline;
                    // 敌人单位名称显示位置
                    if (unitType == UNITTYPE.ENEMY) content += highlightItem("Unit Name: ") + "The unit's name as shown in the top near the enemy health bar." + newline;
                    // 名称是否全大写显示
                    content += highlightItem("Show name in all caps: ") + "Determines whether the name should be displayed in capital letters." + newline;
                    // 单位肖像说明
                    content += highlightItem("Unit Portrait: ") + "The unit portrait sprite is a small icon displayed at the top, near the health bar." + newline;
                    break;

                case 11:
                    /* 视野设置 */
                    // 设置标题为"Field Of View Settings"
                    title = "Field Of View Settings";
                    // 设置内容说明，解释视野相关参数
                    content = "These values determine whether an enemy detects the player when they enter its Field Of View (FOV)." + newline;
                    // 是否启用视野系统
                    content += highlightItem("Enable FOV: ") + "Enable or disable the Field Of View. When disabled a unit always spots the player by default." + newline;
                    // 视野距离说明
                    content += highlightItem("View Distance: ") + "How far this unit can see." + newline;
                    // 视野角度说明
                    content += highlightItem("View Angle: ") + "How wide this unit can see." + newline;
                    // 视野位置偏移说明
                    content += highlightItem("View position Offset: ") + "The starting position (eye level) of the view cone, " + newline;
                    // 编辑器中是否显示视野锥体
                    content += highlightItem("Show FOV Cone in Editor: ") + "Useful for debugging, this option displays the field of view cone in the Unity Editor." + newline;
                    // 目标是否在视野中（调试用）
                    content += highlightItem("Target in Sight: ") + "A read-only value for debugging that indicates whether the target has been spotted." + newline;
                    break;

            }

            // 显示自定义窗口，展示标题和内容
            CustomWindow.ShowWindow(title, content, new Vector2(600, 500));

        }
    }
}