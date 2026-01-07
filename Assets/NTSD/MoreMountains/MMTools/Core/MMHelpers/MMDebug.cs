using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Linq;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Tools
{	
    /// <summary>
    /// 调试助手类，提供各种调试功能
    /// </summary>
    public static class MMDebug 
    {
        #region Commands

        // 缓存的调试日志命令列表
        private static MethodInfo[] _commands;
        // 日志的最大长度
        private static readonly int _logHistoryMaxLength = 256;

        #if UNITY_EDITOR
        private static bool _debugDrawEnabledSet = false;
        #endif
        private static bool _debugDrawEnabled = false;
        private static bool _debugLogEnabled = false;
        private static bool _debugLogEnabledSet = false;

        /// <summary>
        /// 获取项目中所有程序集中的调试命令行列表
        /// </summary>
        public static MethodInfo[] Commands
        {
            get
            {
                if (_commands == null)
                {
                    _commands = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(
                            m => m.GetTypes().SelectMany(
                                n => n.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                                    .Where(o => o.GetCustomAttribute<MMDebugLogCommandAttribute>() != null))).ToArray();
                }

                return _commands;
            }
        }

        /// <summary>
        /// 尝试输入命令
        /// </summary>
        /// <param name="command">要执行的命令</param>
        public static void DebugLogCommand(string command)
        {
            // 如果命令为空，输出空行
            if (command == string.Empty || command == null)
            {
                LogCommand("", "#ff2a00");
                return; 
            }

            // 按空格分割命令
            string[] splitCommand = command.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (splitCommand == null || splitCommand.Length == 0)
            {
                LogCommand("Empty command", "#ff2a00");
                return;
            }
            
            // 检查第一个命令是否存在
            string commandFirst = MMString.UppercaseFirst(splitCommand[0]);
            MethodInfo[] methods = Commands.Where(m => m.Name == commandFirst).ToArray();
            if (methods.Length == 0)
            {
                LogCommand("Command " + commandFirst + " not found.", "#ff2a00");
                return;
            }

            MethodInfo commandInfo;
            object[] parameters = null;

            if (splitCommand.Length > 1)
            { 
                // 如果有参数
                commandInfo = methods.Where(m => m.GetParameters().Length > 0).FirstOrDefault();

                if (commandInfo == null)
                {
                    LogCommand("A version of command " + commandFirst + " with arguments could not be found. Maybe try without arguments.", "#ff2a00");
                    return;
                }

                MMDebugLogCommandArgumentCountAttribute argumentAttribute = commandInfo.GetCustomAttributes<MMDebugLogCommandArgumentCountAttribute>(true).FirstOrDefault();
                if (argumentAttribute != null && argumentAttribute.ArgumentCount > splitCommand.Length - 1)
                { 
                    LogCommand("A version of command " + commandFirst + " needs at least " + argumentAttribute.ArgumentCount + " arguments.", "#ff2a00");
                    return;
                }

                parameters = new object[] { splitCommand };
            }
            else
            { 
                // 如果没有参数
                commandInfo = methods.Where(m => m.GetParameters().Length == 0).FirstOrDefault();

                if (commandInfo == null)
                {
                    LogCommand("A version of command " + commandFirst + " without arguments could not be found.", "#ff2a00");
                    return;
                }
            }

            LogCommand(command, "#FFC400");
            methods[0].Invoke(null, parameters);
        }

        /// <summary>
        /// 记录命令，将其添加到日志历史记录并触发事件
        /// </summary>
        /// <param name="command">命令内容</param>
        /// <param name="color">显示颜色</param>
        private static void LogCommand(string command, string color)
        {
            DebugLogItem item = new DebugLogItem(command, color, Time.frameCount, Time.time, 3, true);
            LogHistory.Add(item);
            MMDebugLogEvent.Trigger(new DebugLogItem(null, "", Time.frameCount, Time.time, 0, false));
        }

        #endregion

        #region DebugLog

        /// <summary>
        /// 用于存储日志项的结构体
        /// </summary>
        public struct DebugLogItem
        {
            public object Message;          // 消息内容
            public string Color;            // 显示颜色
            public int Framecount;          // 帧数
            public float Time;              // 时间
            public int TimePrecision;       // 时间精度
            public bool DisplayFrameCount;  // 是否显示帧数

            public DebugLogItem(object message, string color, int framecount, float time, int timePrecision, bool displayFrameCount)
            {
                Message = message;
                Color = color;
                Framecount = framecount;
                Time = time;
                TimePrecision = timePrecision;
                DisplayFrameCount = displayFrameCount;
            }
        }

        /// <summary>
        /// 所有调试日志的列表（最多DebugLogMaxLength条记录）
        /// </summary>
        public static List<DebugLogItem> LogHistory = new List<DebugLogItem>(_logHistoryMaxLength);

        /// <summary>
        /// 返回包含所有日志历史记录的压缩字符串
        /// </summary>
        public static string LogHistoryText
        {
            get
            {
                string colorPrefix = "";
                string colorSuffix = "";

                StringBuilder log = new StringBuilder();
                for (int i = 0; i < LogHistory.Count; i++)
                {
                    // 处理颜色
                    if (!string.IsNullOrEmpty(LogHistory[i].Color))
                    {
                        colorPrefix = "<color=" + LogHistory[i].Color + ">";
                        colorSuffix = "</color>";
                    }

                    // 构建输出
                    if (LogHistory[i].DisplayFrameCount)
                    {
                        log.Append("<color=#82d3f9>[" + LogHistory[i].Framecount + "]</color> ");
                    }
                    log.Append("<color=#f9a682>[" + MMTime.FloatToTimeString(LogHistory[i].Time, false, true, true, true) + "]</color> ");
                    log.Append(colorPrefix + LogHistory[i].Message + colorSuffix);
                    log.Append(System.Environment.NewLine);
                }
                return log.ToString();
            }
        }

        /// <summary>
        /// 清除调试日志
        /// </summary>
        public static void DebugLogClear()
        {
            LogHistory.Clear();
            MMDebugLogEvent.Trigger(new DebugLogItem(null, "", Time.frameCount, Time.time, 0, false));
        }

        /// <summary>
        /// 向控制台输出信息消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="color">显示颜色</param>
        /// <param name="timePrecision">时间精度</param>
        /// <param name="displayFrameCount">是否显示帧数</param>
        public static void DebugLogInfo(object message, string color = "", int timePrecision = 3, bool displayFrameCount = true)
        {
            DebugLogTime(message, color, timePrecision, displayFrameCount);
        }

        /// <summary>
        /// 向控制台输出消息对象，带有当前时间戳前缀
        /// </summary>
        /// <param name="message">消息内容</param>
        public static void DebugLogTime(object message, string color = "", int timePrecision = 3, bool displayFrameCount = true)
        {
            if (!DebugLogsEnabled)
            {
                return;
            }

            string callerObjectName = new StackTrace().GetFrame(1).GetMethod().ReflectedType.Name;
            color = (color == "") ? "#00FFFF" : color;
            
            // 处理颜色
            string colorPrefix = "";
            string colorSuffix = "";
            if (!string.IsNullOrEmpty(color))
            {
                colorPrefix = "<color=" + color + ">";
                colorSuffix = "</color>";
            }

            // 构建输出
            string output = "";
            if (displayFrameCount)
            {
                output += "<color=#82d3f9>[f" + Time.frameCount + "]</color> ";
            }
            output += "<color=#f9a682>[" + MMTime.FloatToTimeString(Time.time, false, true, true, true) + "]</color> ";
            output += callerObjectName + " : ";
            output += colorPrefix + message + colorSuffix;

            // 输出到控制台
            Debug.Log(output);

            // 记录到MM控制台
            DebugLogItem item = LogDebugToConsole(message, color, timePrecision, displayFrameCount);
        }

        /// <summary>
        /// 将指定消息记录到控制台
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="color">显示颜色</param>
        /// <param name="timePrecision">时间精度</param>
        /// <param name="displayFrameCount">是否显示帧数</param>
        /// <returns>创建的DebugLogItem</returns>
        public static DebugLogItem LogDebugToConsole(object message, string color, int timePrecision, bool displayFrameCount)
        {
            DebugLogItem item = new DebugLogItem(message, color, Time.frameCount, Time.time, timePrecision, displayFrameCount);

            // 添加到DebugLog列表
            if (LogHistory.Count > _logHistoryMaxLength)
            {
                LogHistory.RemoveAt(0);
            }

            LogHistory.Add(item);

            // 触发事件
            MMDebugLogEvent.Trigger(item);

            return item;
        }

        /// <summary>
        /// 用于广播调试日志的事件
        /// </summary>
        public struct MMDebugLogEvent
        {
            static private event Delegate OnEvent;
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
            static public void Register(Delegate callback) { OnEvent += callback; }
            static public void Unregister(Delegate callback) { OnEvent -= callback; }

            public delegate void Delegate(DebugLogItem item);
            static public void Trigger(DebugLogItem item)
            {
                OnEvent?.Invoke(item);
            }
        }

        #endregion

        #region EnableDisableDebugs

        /// <summary>
        /// 是否启用调试日志（MMDebug.DebugLogTime, MMDebug.DebugOnScreen）
        /// </summary>
        public static bool DebugLogsEnabled
        {
            get
            {
                if (_debugLogEnabledSet)
                {
                    return _debugLogEnabled;
                }
                
                if (PlayerPrefs.HasKey(_editorPrefsDebugLogs))
                {
                    _debugLogEnabled = (PlayerPrefs.GetInt(_editorPrefsDebugLogs) == 0) ? false : true;
                }
                else
                {
                    _debugLogEnabled = true;
                }

                _debugLogEnabledSet = true;
                return _debugLogEnabled;
            }
            private set
            {
                _debugLogEnabledSet = true;
                _debugLogEnabled = value;
            }
        }

        /// <summary>
        /// 是否启用调试绘制
        /// </summary>
        public static bool DebugDrawEnabled
        {
            get
            {
                #if UNITY_EDITOR
                if (_debugDrawEnabledSet)
                {
                    return _debugDrawEnabled;
                }

                if (PlayerPrefs.HasKey(_editorPrefsDebugDraws))
                {
                    _debugDrawEnabled = (PlayerPrefs.GetInt(_editorPrefsDebugDraws) == 0) ? false : true;
                }
                else
                {
                    _debugDrawEnabled = true;
                }
                _debugDrawEnabledSet = true;
                return _debugDrawEnabled;
                #else
                    return false;
                #endif
            }
            private set { }
        }

        private const string _editorPrefsDebugLogs = "DebugLogsEnabled";
        private const string _editorPrefsDebugDraws = "DebugDrawsEnabled";

        /// <summary>
        /// 启用或禁用调试日志
        /// </summary>
        /// <param name="status">是否启用</param>
        public static void SetDebugLogsEnabled(bool status)
        {
            DebugLogsEnabled = status;
            _debugLogEnabled = status;
            #if UNITY_EDITOR
            int newStatus = status ? 1 : 0;
            PlayerPrefs.SetInt(_editorPrefsDebugLogs, newStatus);
            #endif
        }

        /// <summary>
        /// 启用或禁用调试绘制
        /// </summary>
        /// <param name="status">是否启用</param>
        public static void SetDebugDrawEnabled(bool status)
        {
            DebugDrawEnabled = status;
            _debugDrawEnabled = status;
            #if UNITY_EDITOR
            int newStatus = status ? 1 : 0;
            PlayerPrefs.SetInt(_editorPrefsDebugDraws, newStatus);
            #endif
        }

        #endregion

        #region Casts

        /// <summary>
        /// 在2D中绘制调试射线并执行实际的射线检测
        /// </summary>
        /// <param name="rayOriginPoint">射线起点</param>
        /// <param name="rayDirection">射线方向</param>
        /// <param name="rayDistance">射线距离</param>
        /// <param name="mask">层级遮罩</param>
        /// <param name="color">射线颜色</param>
        /// <param name="drawGizmo">是否绘制Gizmo</param>
        /// <returns>射线检测结果</returns>
        public static RaycastHit2D RayCast(Vector2 rayOriginPoint, Vector2 rayDirection, float rayDistance, LayerMask mask, Color color,bool drawGizmo=false)
        {	
            if (drawGizmo && DebugDrawEnabled) 
            {
                Debug.DrawRay (rayOriginPoint, rayDirection * rayDistance, color);
            }
            return Physics2D.Raycast(rayOriginPoint,rayDirection,rayDistance,mask);		
        }

        /// <summary>
        /// 执行盒子投射并绘制盒子Gizmo
        /// </summary>
        /// <param name="origin">起点</param>
        /// <param name="size">大小</param>
        /// <param name="angle">角度</param>
        /// <param name="direction">方向</param>
        /// <param name="length">长度</param>
        /// <param name="mask">层级遮罩</param>
        /// <param name="color">颜色</param>
        /// <param name="drawGizmo">是否绘制Gizmo</param>
        /// <returns>检测结果</returns>
        public static RaycastHit2D BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, float length, LayerMask mask, Color color, bool drawGizmo = false)
        {
            if (drawGizmo && DebugDrawEnabled)
            {
                Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

                Vector3[] points = new Vector3[8];

                float halfSizeX = size.x / 2f;
                float halfSizeY = size.y / 2f;

                points[0] = rotation * (origin + (Vector2.left * halfSizeX) + (Vector2.up * halfSizeY)); // 左上
                points[1] = rotation * (origin + (Vector2.right * halfSizeX) + (Vector2.up * halfSizeY)); // 右上
                points[2] = rotation * (origin + (Vector2.right * halfSizeX) - (Vector2.up * halfSizeY)); // 右下
                points[3] = rotation * (origin + (Vector2.left * halfSizeX) - (Vector2.up * halfSizeY)); // 左下
                
                points[4] = rotation * ((origin + Vector2.left * halfSizeX + Vector2.up * halfSizeY) + length * direction); // 左上
                points[5] = rotation * ((origin + Vector2.right * halfSizeX + Vector2.up * halfSizeY) + length * direction); // 右上
                points[6] = rotation * ((origin + Vector2.right * halfSizeX - Vector2.up * halfSizeY) + length * direction); // 右下
                points[7] = rotation * ((origin + Vector2.left * halfSizeX - Vector2.up * halfSizeY) + length * direction); // 左下
                                
                Debug.DrawLine(points[0], points[1], color);
                Debug.DrawLine(points[1], points[2], color);
                Debug.DrawLine(points[2], points[3], color);
                Debug.DrawLine(points[3], points[0], color);

                Debug.DrawLine(points[4], points[5], color);
                Debug.DrawLine(points[5], points[6], color);
                Debug.DrawLine(points[6], points[7], color);
                Debug.DrawLine(points[7], points[4], color);
                
                Debug.DrawLine(points[0], points[4], color);
                Debug.DrawLine(points[1], points[5], color);
                Debug.DrawLine(points[2], points[6], color);
                Debug.DrawLine(points[3], points[7], color);
            }
            return Physics2D.BoxCast(origin, size, angle, direction, length, mask);
        }

        /// <summary>
        /// 绘制调试射线而不分配内存
        /// </summary>
        /// <param name="array">结果数组</param>
        /// <param name="rayOriginPoint">射线起点</param>
        /// <param name="rayDirection">射线方向</param>
        /// <param name="rayDistance">射线距离</param>
        /// <param name="mask">层级遮罩</param>
        /// <param name="color">颜色</param>
        /// <param name="drawGizmo">是否绘制Gizmo</param>
        /// <returns>第一个碰撞结果</returns>
        public static RaycastHit2D MonoRayCastNonAlloc(RaycastHit2D[] array, Vector2 rayOriginPoint, Vector2 rayDirection, float rayDistance, LayerMask mask, Color color,bool drawGizmo=false)
        {	
            if (drawGizmo && DebugDrawEnabled) 
            {
                Debug.DrawRay (rayOriginPoint, rayDirection * rayDistance, color);
            }
            if (Physics2D.RaycastNonAlloc(rayOriginPoint, rayDirection, array, rayDistance, mask) > 0)
            {
                return array[0];
            }
            return new RaycastHit2D();        	
        }

        /// <summary>
        /// 在3D中绘制调试射线并执行实际的射线检测
        /// </summary>
        /// <param name="rayOriginPoint">射线起点</param>
        /// <param name="rayDirection">射线方向</param>
        /// <param name="rayDistance">射线距离</param>
        /// <param name="mask">层级遮罩</param>
        /// <param name="color">颜色</param>
        /// <param name="drawGizmo">是否绘制Gizmo</param>
        /// <param name="queryTriggerInteraction">触发器交互设置</param>
        /// <returns>射线检测结果</returns>
        public static RaycastHit Raycast3D(Vector3 rayOriginPoint, Vector3 rayDirection, float rayDistance, LayerMask mask, Color color,bool drawGizmo=false, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            if (drawGizmo && DebugDrawEnabled) 
            {
                Debug.DrawRay (rayOriginPoint, rayDirection * rayDistance, color);
            }
            RaycastHit hit;
            Physics.Raycast(rayOriginPoint, rayDirection, out hit, rayDistance, mask, queryTriggerInteraction);	
            return hit;
        }

        #endregion

        #region DebugOnScreen
        
        #if MM_UI
        // 屏幕调试控制台
        public static MMDebugOnScreenConsole _console;
        private const string _debugConsolePrefabPath = "MMDebugOnScreenConsole";
                
        /// <summary>
        /// 如果没有MMConsole则实例化一个，并将参数中的消息添加到其中
        /// </summary>
        /// <param name="message">要显示的消息</param>
        public static void DebugOnScreen(string message)
        {
            if (!DebugLogsEnabled)
            {
                return;
            }

            InstantiateOnScreenConsole();
            _console.AddMessage(message, "", 30);
        }

        /// <summary>
        /// 如果没有MMConsole则实例化一个，并以粗体显示标签，在其旁边显示值
        /// </summary>
        /// <param name="label">标签</param>
        /// <param name="value">值</param>
        /// <param name="fontSize">可选的字体大小</param>
        public static void DebugOnScreen(string label, object value, int fontSize=25)
        {
            if (!DebugLogsEnabled)
            {
                return;
            }

            InstantiateOnScreenConsole(fontSize);
            _console.AddMessage(label, value, fontSize);
        }

        /// <summary>
        /// 如果没有屏幕控制台则实例化一个
        /// </summary>
        /// <param name="fontSize">字体大小</param>
        public static void InstantiateOnScreenConsole(int fontSize=25)
        {
            if (!DebugLogsEnabled)
            {
                return;
            }

            if (_console == null)
            {
                // 尝试在场景中找到一个
                _console = (MMDebugOnScreenConsole) GameObject.FindObjectOfType(typeof(MMDebugOnScreenConsole));
            }

            if (_console == null)
            {	
                // 实例化控制台
                GameObject loaded = UnityEngine.Object.Instantiate(Resources.Load(_debugConsolePrefabPath) as GameObject);
                loaded.name = "MMDebugOnScreenConsole";
                _console = loaded.GetComponent<MMDebugOnScreenConsole>();                
            }
        }

        /// <summary>
        /// 使用此方法指定要使用的控制台
        /// </summary>
        /// <param name="newConsole">新的控制台</param>
        public static void SetOnScreenConsole(MMDebugOnScreenConsole newConsole)
        {
            _console = newConsole;
        }
        #endif

        #endregion

        #region DebugDraw

        /// <summary>
        /// 绘制从原点位置开始并沿Vector3方向的Gizmo箭头
        /// </summary>
        /// <param name="origin">起点</param>
        /// <param name="direction">方向</param>
        /// <param name="color">颜色</param>
        /// <param name="arrowHeadLength">箭头长度</param>
        /// <param name="arrowHeadAngle">箭头角度</param>
        public static void DrawGizmoArrow(Vector3 origin, Vector3 direction, Color color, float arrowHeadLength = 3f, float arrowHeadAngle = 25f)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Gizmos.color = color;
            Gizmos.DrawRay(origin, direction);
       
            DrawArrowEnd(true, origin, direction, color, arrowHeadLength, arrowHeadAngle);
        }

        /// <summary>
        /// 绘制从原点位置开始并沿Vector3方向的调试箭头
        /// </summary>
        /// <param name="origin">起点</param>
        /// <param name="direction">方向</param>
        /// <param name="color">颜色</param>
        /// <param name="arrowHeadLength">箭头长度</param>
        /// <param name="arrowHeadAngle">箭头角度</param>
        public static void DebugDrawArrow(Vector3 origin, Vector3 direction, Color color, float arrowHeadLength = 0.2f, float arrowHeadAngle = 35f)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Debug.DrawRay(origin, direction, color);
       
            DrawArrowEnd(false,origin,direction,color,arrowHeadLength,arrowHeadAngle);
        }

        /// <summary>
        /// 绘制从原点位置开始并沿Vector3方向的调试箭头
        /// </summary>
        /// <param name="origin">起点</param>
        /// <param name="direction">方向</param>
        /// <param name="color">颜色</param>
        /// <param name="arrowLength">箭头长度</param>
        /// <param name="arrowHeadLength">箭头头部长度</param>
        /// <param name="arrowHeadAngle">箭头头部角度</param>
        public static void DebugDrawArrow(Vector3 origin, Vector3 direction, Color color, float arrowLength, float arrowHeadLength = 0.20f, float arrowHeadAngle = 35.0f)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Debug.DrawRay(origin, direction * arrowLength, color);

            DrawArrowEnd(false,origin,direction * arrowLength,color,arrowHeadLength,arrowHeadAngle);
        }

        /// <summary>
        /// 在指定点绘制指定大小和颜色的调试十字
        /// </summary>
        /// <param name="spot">位置</param>
        /// <param name="crossSize">十字大小</param>
        /// <param name="color">颜色</param>
        public static void DebugDrawCross (Vector3 spot, float crossSize, Color color)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Vector3 tempOrigin = Vector3.zero;
            Vector3 tempDirection = Vector3.zero;

            tempOrigin.x = spot.x - crossSize / 2;
            tempOrigin.y = spot.y - crossSize / 2;
            tempOrigin.z = spot.z ;
            tempDirection.x = 1; 
            tempDirection.y = 1;
            tempDirection.z = 0;
            Debug.DrawRay (tempOrigin, tempDirection * crossSize, color);

            tempOrigin.x = spot.x - crossSize / 2;
            tempOrigin.y = spot.y + crossSize / 2;
            tempOrigin.z = spot.z ;
            tempDirection.x = 1; 
            tempDirection.y = -1;
            tempDirection.z = 0;
            Debug.DrawRay (tempOrigin, tempDirection * crossSize, color);
        }

        /// <summary>
        /// 为DebugDrawArrow绘制箭头末端
        /// </summary>
        /// <param name="drawGizmos">是否使用Gizmos绘制</param>
        /// <param name="arrowEndPosition">箭头末端位置</param>
        /// <param name="direction">方向</param>
        /// <param name="color">颜色</param>
        /// <param name="arrowHeadLength">箭头长度</param>
        /// <param name="arrowHeadAngle">箭头角度</param>
        private static void DrawArrowEnd (bool drawGizmos, Vector3 arrowEndPosition, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 40.0f)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            if (direction == Vector3.zero)
            {
                return;
            }
            Vector3 right = Quaternion.LookRotation (direction) * Quaternion.Euler (arrowHeadAngle, 0, 0) * Vector3.back;
            Vector3 left = Quaternion.LookRotation (direction) * Quaternion.Euler (-arrowHeadAngle, 0, 0) * Vector3.back;
            Vector3 up = Quaternion.LookRotation (direction) * Quaternion.Euler (0, arrowHeadAngle, 0) * Vector3.back;
            Vector3 down = Quaternion.LookRotation (direction) * Quaternion.Euler (0, -arrowHeadAngle, 0) * Vector3.back;
            if (drawGizmos) 
            {
                Gizmos.color = color;
                Gizmos.DrawRay (arrowEndPosition + direction, right * arrowHeadLength);
                Gizmos.DrawRay (arrowEndPosition + direction, left * arrowHeadLength);
                Gizmos.DrawRay (arrowEndPosition + direction, up * arrowHeadLength);
                Gizmos.DrawRay (arrowEndPosition + direction, down * arrowHeadLength);
            }
            else
            {
                Debug.DrawRay (arrowEndPosition + direction, right * arrowHeadLength, color);
                Debug.DrawRay (arrowEndPosition + direction, left * arrowHeadLength, color);
                Debug.DrawRay (arrowEndPosition + direction, up * arrowHeadLength, color);
                Debug.DrawRay (arrowEndPosition + direction, down * arrowHeadLength, color);
            }
        }

        /// <summary>
        /// 绘制手柄以在屏幕上显示对象的边界
        /// </summary>
        /// <param name="bounds">边界</param>
        /// <param name="color">颜色</param>
        public static void DrawHandlesBounds(Bounds bounds, Color color)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            #if UNITY_EDITOR
            Vector3 boundsCenter = bounds.center;
            Vector3 boundsExtents = bounds.extents;
          
            Vector3 v3FrontTopLeft     = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z - boundsExtents.z);  // 前左上角
            Vector3 v3FrontTopRight    = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z - boundsExtents.z);  // 前右上角
            Vector3 v3FrontBottomLeft  = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z - boundsExtents.z);  // 前左下角
            Vector3 v3FrontBottomRight = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z - boundsExtents.z);  // 前右下角
            Vector3 v3BackTopLeft      = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z + boundsExtents.z);  // 后左上角
            Vector3 v3BackTopRight     = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z + boundsExtents.z);  // 后右上角
            Vector3 v3BackBottomLeft   = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z + boundsExtents.z);  // 后左下角
            Vector3 v3BackBottomRight  = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z + boundsExtents.z);  // 后右下角

            Handles.color = color;

            // 绘制前面
            Handles.DrawLine (v3FrontTopLeft, v3FrontTopRight);
            Handles.DrawLine (v3FrontTopRight, v3FrontBottomRight);
            Handles.DrawLine (v3FrontBottomRight, v3FrontBottomLeft);
            Handles.DrawLine (v3FrontBottomLeft, v3FrontTopLeft);
         
            // 绘制后面
            Handles.DrawLine (v3BackTopLeft, v3BackTopRight);
            Handles.DrawLine (v3BackTopRight, v3BackBottomRight);
            Handles.DrawLine (v3BackBottomRight, v3BackBottomLeft);
            Handles.DrawLine (v3BackBottomLeft, v3BackTopLeft);
         
            // 连接前后
            Handles.DrawLine (v3FrontTopLeft, v3BackTopLeft);
            Handles.DrawLine (v3FrontTopRight, v3BackTopRight);
            Handles.DrawLine (v3FrontBottomRight, v3BackBottomRight);
            Handles.DrawLine (v3FrontBottomLeft, v3BackBottomLeft);  
            #endif
        }

        /// <summary>
        /// 在指定位置和大小绘制指定颜色的实心矩形
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="size">大小</param>
        /// <param name="borderColor">边框颜色</param>
        /// <param name="solidColor">填充颜色</param>
        public static void DrawSolidRectangle(Vector3 position, Vector3 size, Color borderColor, Color solidColor)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            #if UNITY_EDITOR

            Vector3 halfSize = size / 2f;

            Vector3[] verts = new Vector3[4];
            verts[0] = new Vector3(halfSize.x, halfSize.y, halfSize.z);
            verts[1] = new Vector3(-halfSize.x, halfSize.y, halfSize.z);
            verts[2] = new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
            verts[3] = new Vector3(halfSize.x, -halfSize.y, halfSize.z);
            Handles.DrawSolidRectangleWithOutline(verts, solidColor, borderColor);
            
            #endif
        }
        
        /// <summary>
        /// 在指定位置绘制指定大小和颜色的Gizmo球体
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="size">大小</param>
        /// <param name="color">颜色</param>
        public static void DrawGizmoPoint(Vector3 position, float size, Color color)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }
            Gizmos.color = color;
            Gizmos.DrawWireSphere(position,size);
        }

        /// <summary>
        /// 在指定位置绘制指定颜色和大小的立方体
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="color">颜色</param>
        /// <param name="size">大小</param>
        public static void DrawCube (Vector3 position, Color color, Vector3 size)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Vector3 halfSize = size / 2f; 

            Vector3[] points = new Vector3 []
            {
                position + new Vector3(halfSize.x,halfSize.y,halfSize.z),
                position + new Vector3(-halfSize.x,halfSize.y,halfSize.z),
                position + new Vector3(-halfSize.x,-halfSize.y,halfSize.z),
                position + new Vector3(halfSize.x,-halfSize.y,halfSize.z),			
                position + new Vector3(halfSize.x,halfSize.y,-halfSize.z),
                position + new Vector3(-halfSize.x,halfSize.y,-halfSize.z),
                position + new Vector3(-halfSize.x,-halfSize.y,-halfSize.z),
                position + new Vector3(halfSize.x,-halfSize.y,-halfSize.z),
            };

            Debug.DrawLine (points[0], points[1], color ); 
            Debug.DrawLine (points[1], points[2], color ); 
            Debug.DrawLine (points[2], points[3], color ); 
            Debug.DrawLine (points[3], points[0], color ); 
        }

        /// <summary>
        /// 在指定位置和偏移处绘制指定大小的立方体
        /// </summary>
        /// <param name="transform">变换组件</param>
        /// <param name="offset">偏移</param>
        /// <param name="cubeSize">立方体大小</param>
        /// <param name="wireOnly">是否只绘制线框</param>
        public static void DrawGizmoCube(Transform transform, Vector3 offset, Vector3 cubeSize, bool wireOnly)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Matrix4x4 rotationMatrix = transform.localToWorldMatrix;
            Gizmos.matrix = rotationMatrix;
            if (wireOnly)
            {
                Gizmos.DrawWireCube(offset, cubeSize);
            }
            else
            {
                Gizmos.DrawCube(offset, cubeSize);
            }
        }

        /// <summary>
        /// 绘制Gizmo矩形
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="size">大小</param>
        /// <param name="color">颜色</param>
        public static void DrawGizmoRectangle(Vector2 center, Vector2 size, Color color)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Gizmos.color = color;

            Vector3 v3TopLeft = new Vector3(center.x - size.x/2, center.y + size.y/2, 0);
            Vector3 v3TopRight = new Vector3(center.x + size.x/2, center.y + size.y/2, 0);;
            Vector3 v3BottomRight = new Vector3(center.x + size.x/2, center.y - size.y/2, 0);;
            Vector3 v3BottomLeft = new Vector3(center.x - size.x/2, center.y - size.y/2, 0);;

            Gizmos.DrawLine(v3TopLeft,v3TopRight);
            Gizmos.DrawLine(v3TopRight,v3BottomRight);
            Gizmos.DrawLine(v3BottomRight,v3BottomLeft);
            Gizmos.DrawLine(v3BottomLeft,v3TopLeft);
        }

        /// <summary>
        /// 绘制Gizmo矩形（带旋转）
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="size">大小</param>
        /// <param name="rotationMatrix">旋转矩阵</param>
        /// <param name="color">颜色</param>
        public static void DrawGizmoRectangle(Vector2 center, Vector2 size, Matrix4x4 rotationMatrix, Color color)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            GL.PushMatrix();

            Gizmos.color = color;

            Vector3 v3TopLeft = rotationMatrix * new Vector3(center.x - size.x / 2, center.y + size.y / 2, 0);
            Vector3 v3TopRight = rotationMatrix * new Vector3(center.x + size.x / 2, center.y + size.y / 2, 0); ;
            Vector3 v3BottomRight = rotationMatrix * new Vector3(center.x + size.x / 2, center.y - size.y / 2, 0); ;
            Vector3 v3BottomLeft = rotationMatrix * new Vector3(center.x - size.x / 2, center.y - size.y / 2, 0); ;

            
            Gizmos.DrawLine(v3TopLeft, v3TopRight);
            Gizmos.DrawLine(v3TopRight, v3BottomRight);
            Gizmos.DrawLine(v3BottomRight, v3BottomLeft);
            Gizmos.DrawLine(v3BottomLeft, v3TopLeft);
            GL.PopMatrix();
        }

        /// <summary>
        /// 基于Rect和颜色绘制矩形
        /// </summary>
        /// <param name="rectangle">矩形区域</param>
        /// <param name="color">颜色</param>
        public static void DrawRectangle (Rect rectangle, Color color)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Vector3 pos = new Vector3( rectangle.x + rectangle.width/2, rectangle.y + rectangle.height/2, 0.0f );
            Vector3 scale = new Vector3 (rectangle.width, rectangle.height, 0.0f );

            MMDebug.DrawRectangle (pos, color, scale); 
        }	

        /// <summary>
        /// 在指定位置绘制指定颜色和大小的矩形
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="color">颜色</param>
        /// <param name="size">大小</param>
        public static void DrawRectangle  (Vector3 position, Color color, Vector3 size)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Vector3 halfSize = size / 2f; 

            Vector3[] points = new Vector3 []
            {
                position + new Vector3(halfSize.x,halfSize.y,halfSize.z),
                position + new Vector3(-halfSize.x,halfSize.y,halfSize.z),
                position + new Vector3(-halfSize.x,-halfSize.y,halfSize.z),
                position + new Vector3(halfSize.x,-halfSize.y,halfSize.z),	
            };

            Debug.DrawLine (points[0], points[1], color ); 
            Debug.DrawLine (points[1], points[2], color ); 
            Debug.DrawLine (points[2], points[3], color ); 
            Debug.DrawLine (points[3], points[0], color ); 
        }
        
        /// <summary>
        /// 在指定位置绘制指定颜色和大小的点
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="color">颜色</param>
        /// <param name="size">大小</param>
        public static void DrawPoint (Vector3 position, Color color, float size)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Vector3[] points = new Vector3[] 
            {
                position + (Vector3.up * size), 
                position - (Vector3.up * size), 
                position + (Vector3.right * size), 
                position - (Vector3.right * size), 
                position + (Vector3.forward * size), 
                position - (Vector3.forward * size)
            }; 		

            Debug.DrawLine (points[0], points[1], color ); 
            Debug.DrawLine (points[2], points[3], color ); 
            Debug.DrawLine (points[4], points[5], color ); 
            Debug.DrawLine (points[0], points[2], color ); 
            Debug.DrawLine (points[0], points[3], color ); 
            Debug.DrawLine (points[0], points[4], color ); 
            Debug.DrawLine (points[0], points[5], color ); 
            Debug.DrawLine (points[1], points[2], color ); 
            Debug.DrawLine (points[1], points[3], color ); 
            Debug.DrawLine (points[1], points[4], color ); 
            Debug.DrawLine (points[1], points[5], color ); 
            Debug.DrawLine (points[4], points[2], color ); 
            Debug.DrawLine (points[4], points[3], color ); 
            Debug.DrawLine (points[5], points[2], color ); 
            Debug.DrawLine (points[5], points[3], color ); 
        }
        
        /// <summary>
        /// 使用Gizmos绘制指定颜色和大小的线
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="color">颜色</param>
        /// <param name="size">大小</param>
        public static void DrawGizmoPoint (Vector3 position, Color color, float size)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Vector3[] points = new Vector3[] 
            {
                position + (Vector3.up * size), 
                position - (Vector3.up * size), 
                position + (Vector3.right * size), 
                position - (Vector3.right * size), 
                position + (Vector3.forward * size), 
                position - (Vector3.forward * size)
            }; 		

            Gizmos.color = color;
            Gizmos.DrawLine (points[0], points[1]); 
            Gizmos.DrawLine (points[2], points[3]); 
            Gizmos.DrawLine (points[4], points[5]); 
            Gizmos.DrawLine (points[0], points[2]); 
            Gizmos.DrawLine (points[0], points[3]); 
            Gizmos.DrawLine (points[0], points[4]); 
            Gizmos.DrawLine (points[0], points[5]); 
            Gizmos.DrawLine (points[1], points[2]); 
            Gizmos.DrawLine (points[1], points[3]); 
            Gizmos.DrawLine (points[1], points[4]); 
            Gizmos.DrawLine (points[1], points[5]); 
            Gizmos.DrawLine (points[4], points[2]); 
            Gizmos.DrawLine (points[4], points[3]); 
            Gizmos.DrawLine (points[5], points[2]); 
            Gizmos.DrawLine (points[5], points[3]); 
        }

        #endregion

        #region Info

        /// <summary>
        /// 获取系统信息
        /// </summary>
        /// <returns>系统信息字符串</returns>
        public static string GetSystemInfo()
        {
            string result = "SYSTEM INFO";

            #if UNITY_IOS
                 result += "\n[iPhone generation]iPhone.generation.ToString()";
            #endif

            #if UNITY_ANDROID
                result += "\n[system info]" + SystemInfo.deviceModel;
            #endif

            result += "\n<color=#FFFFFF>Device Type :</color> " + SystemInfo.deviceType;
            result += "\n<color=#FFFFFF>OS Version :</color> " + SystemInfo.operatingSystem;
            result += "\n<color=#FFFFFF>System Memory Size :</color> " + SystemInfo.systemMemorySize;
            result += "\n<color=#FFFFFF>Graphic Device Name :</color> " + SystemInfo.graphicsDeviceName + " (version " + SystemInfo.graphicsDeviceVersion + ")";
            result += "\n<color=#FFFFFF>Graphic Memory Size :</color> " + SystemInfo.graphicsMemorySize;
            result += "\n<color=#FFFFFF>Graphic Max Texture Size :</color> " + SystemInfo.maxTextureSize;
            result += "\n<color=#FFFFFF>Graphic Shader Level :</color> " + SystemInfo.graphicsShaderLevel;
            result += "\n<color=#FFFFFF>Compute Shader Support :</color> " + SystemInfo.supportsComputeShaders;

            result += "\n<color=#FFFFFF>Processor Count :</color> " + SystemInfo.processorCount;
            result += "\n<color=#FFFFFF>Processor Type :</color> " + SystemInfo.processorType;
            result += "\n<color=#FFFFFF>3D Texture Support :</color> " + SystemInfo.supports3DTextures;
            result += "\n<color=#FFFFFF>Shadow Support :</color> " + SystemInfo.supportsShadows;

            result += "\n<color=#FFFFFF>Platform :</color> " + Application.platform;
            result += "\n<color=#FFFFFF>Screen Size :</color> " + Screen.width + " x " + Screen.height;
            result += "\n<color=#FFFFFF>DPI :</color> " + Screen.dpi;

            return result;
        }

        #endregion
        
        #region Console
        
        /// <summary>
        /// 清空控制台
        /// </summary>
        public static void ClearConsole()
        {
            Type logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            if (logEntries != null)
            {
                MethodInfo clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (clearMethod != null)
                {
                    clearMethod.Invoke(null, null);    
                }
            }
        }
        
        #endregion
    }
}
