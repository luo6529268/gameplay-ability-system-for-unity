using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

namespace MoreMountains.TopDownEngine
{	
    /// <summary>
    /// 关卡管理器类，负责玩家生成、复活、检查点管理和关卡边界控制
    /// </summary>
    [AddComponentMenu("TopDown Engine/Managers/Level Manager")]
    public class LevelManager : MMSingleton<LevelManager>, MMEventListener<TopDownEngineEvent>
    {	
        // 角色实例化相关设置
        [Header("Instantiate Characters")]
        [MMInformation("LevelManager负责处理生成/复活、检查点管理和关卡边界。在这里，你可以为关卡定义一个或多个可玩角色。", MMInformationAttribute.InformationType.Info,false)]
        
        /// <summary>
        /// 玩家预制体数组，用于在关卡开始时实例化
        /// </summary>
        [Tooltip("The list of player prefabs this level manager will instantiate on Start")]
        public Character[] PlayerPrefabs;

        // 场景中已存在的角色设置
        [Header("Characters already in the scene")]
        [MMInformation("建议让 LevelManager 实例化您的角色，但如果您希望它们已经出现在场景中，只需在下面的列表中绑定它们即可。", MMInformationAttribute.InformationType.Info, false)]
      
        // 检查点相关设置
        [Header("Checkpoints")]
        
        /// <summary>
        /// 初始生成点，如果没有指定入口点则使用此检查点
        /// </summary>
        [Tooltip("如果未指定入口点，则用作初始生成点的检查点")]
        public CheckPoint InitialSpawnPoint;
        
        /// <summary>
        /// 当前活动检查点，即玩家最后通过的检查点
        /// </summary>
        [Tooltip("当前活动的 checkpoint （玩家通过的最后一个 checkpoint）")]
        public CheckPoint CurrentCheckpoint;

        // 入口点相关设置
        [Header("Points of Entry")]
        
        /// <summary>
        /// 关卡入口点数组，可被其他关卡用作初始目标
        /// </summary>
        [Tooltip("此关卡的入口点列表，可从其他关卡用作初始目标")]
        public Transform[] PointsOfEntry;
        				
        // 过渡效果持续时间设置
        [Space(10)]
        [Header("Intro and Outro durations")]
        [MMInformation("在这里，你可以指定关卡开始和结束时的淡入和淡出的长度。您还可以确定重生前的延迟。", MMInformationAttribute.InformationType.Info,false)]
        
        /// <summary>
        /// 初始淡入效果的持续时间（秒）
        /// </summary>
        [Tooltip("初始淡入的持续时间（以秒为单位）")]
        public float IntroFadeDuration = 1f;

        /// <summary>
        /// 生成延迟时间
        /// </summary>
        public float SpawnDelay = 0f;
        
        /// <summary>
        /// 关卡结束时淡入黑场的持续时间（秒）
        /// </summary>
        [Tooltip("关卡结束时淡入黑场的持续时间（以秒为单位）")]
        public float OutroFadeDuration = 1f;
        
        /// <summary>
        /// 淡入淡出效果的ID，需要与要使用的淡入淡出组件的ID匹配
        /// </summary>
        [Tooltip("触发事件时使用的 ID（应与要使用的推子上的 ID 匹配）")]
        public int FaderID = 0;
        
        /// <summary>
        /// 淡入淡出效果的动画曲线
        /// </summary>
        [Tooltip("the curve to use for in and out fades")]
        public MMTweenType FadeCurve = new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic);
        
        /// <summary>
        /// 主角死亡到重生之间的延迟时间
        /// </summary>
        [Tooltip("主角死亡与其重生之间的持续时间")]
        public float RespawnDelay = 2f;

        // 复活循环相关设置
        [Header("Respawn Loop")]
        
        /// <summary>
        /// 玩家死亡后显示死亡画面的延迟时间（秒）
        /// </summary>
        [Tooltip("玩家死亡后显示死亡屏幕之前的延迟（以秒为单位）")]
        public float DelayBeforeDeathScreen = 1f;
        
        // 场景加载相关设置
        [Header("Scene Loading")]
        
        /// <summary>
        /// 场景加载模式
        /// </summary>
        [Tooltip("用于加载目标层级的方法")]
        public MMLoadScene.LoadingSceneModes LoadingSceneMode = MMLoadScene.LoadingSceneModes.MMSceneLoadingManager;
        
        /// <summary>
        /// MMSceneLoadingManager场景的名称
        /// </summary>
        [Tooltip("要使用的 MMSceneLoadingManager 场景的名称")]
        [MMEnumCondition("LoadingSceneMode", (int) MMLoadScene.LoadingSceneModes.MMSceneLoadingManager)]
        public string LoadingSceneName = "LoadingScreen";
        
        /// <summary>
        /// 叠加模式下加载场景的设置
        /// </summary>
        [Tooltip("在叠加模式下加载场景时使用的设置")]
        [MMEnumCondition("LoadingSceneMode", (int)MMLoadScene.LoadingSceneModes.MMAdditiveSceneLoadingManager)]
        public MMAdditiveSceneLoadingManagerSettings AdditiveLoadingSettings; 
        
        // 关卡信息
        [Header("关卡")]
        [MMReadOnly]
        public int CurrentLevel;

        // 运行时间属性
        /// <summary>
        /// 关卡开始后的运行时间
        /// </summary>
        public virtual TimeSpan RunningTime { get { return DateTime.UtcNow - _started ;}}
        
        // 私有变量
        public virtual List<CheckPoint> Checkpoints { get; protected set; }
        public virtual List<Character> Players { get; protected set; }

        protected DateTime _started;
        protected Vector3 _initialSpawnPointPosition;
        protected bool _levelStarted = false;

        // 静态初始化，支持进入播放模式
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        protected static void InitializeStatics()
        {
            _instance = null;
        }
        
        // 初始化方法
        protected override void Awake()
        {
            base.Awake();
        }

        protected virtual void Start()
        {
        }

        public virtual void StartLevel()
        {
            if (_levelStarted) return;
            _levelStarted = true;
            
            InitializationCoroutine().Forget();
        }

        // 初始化协程
        protected virtual async UniTask InitializationCoroutine()
        {
            await UniTask.WaitForSeconds(SpawnDelay);
            
            // 实例化可玩角色
            InstantiatePlayableCharacters();

            // 如果没有玩家，直接返回
            if (Players == null || Players.Count == 0) { return; }

            // 基础初始化
            Initialization();

            // 触发生成开始事件
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.SpawnCharacterStarts, null);

            // 处理角色生成
            if (Players.Count == 1)
            {
                SpawnSingleCharacter();
            }
            else
            {
                SpawnMultipleCharacters();
            }

            // 触发淡入效果
            MMFadeOutEvent.Trigger(IntroFadeDuration, FadeCurve, FaderID);

            // 触发关卡开始事件
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LevelStart, null);
            MMGameEvent.Trigger("Load");

            // 设置相机目标和开始跟随
            MMCameraEvent.Trigger(MMCameraEventTypes.SetTargetCharacter, Players[0]);
            MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);
            MMGameEvent.Trigger("CameraBound");
        }

        // 生成多个角色（需要在子类中重写）
        protected virtual void SpawnMultipleCharacters()
        {

        }

        // 实例化可玩角色
        protected virtual void InstantiatePlayableCharacters()
        {
            // 获取初始生成点位置
            _initialSpawnPointPosition = (InitialSpawnPoint == null) ? Vector3.zero : InitialSpawnPoint.transform.position;
            
            Players = new List<Character> ();


            // 如果没有玩家预制体，直接返回
            if (PlayerPrefabs == null) { return; }

            // 实例化玩家预制体
            if (PlayerPrefabs.Length != 0)
            { 
                foreach (Character playerPrefab in PlayerPrefabs)
                {
                    Character newPlayer = Instantiate (playerPrefab, _initialSpawnPointPosition, Quaternion.identity);
                    newPlayer.name = playerPrefab.name;
                    Players.Add(newPlayer);
                }
            }
        }

       
        // 初始化关卡
        protected virtual void Initialization()
        {
            // 获取所有检查点并排序
            Checkpoints = FindObjectsOfType<CheckPoint>().OrderBy(o => o.CheckPointOrder).ToList();
            // 记录开始时间
            _started = DateTime.UtcNow;
        }

        // 生成单个角色
        protected virtual void SpawnSingleCharacter()
        {
            // 获取入口点
            PointsOfEntryStorage point = GameManager.Instance.GetPointsOfEntry(SceneManager.GetActiveScene().name);
            if ((point != null) && (PointsOfEntry.Length >= (point.PointOfEntryIndex + 1)))
            {
                TopDownEngineEvent.Trigger(TopDownEngineEventTypes.SpawnComplete, Players[0]);
                return;
            }

            // 使用初始生成点
            if (InitialSpawnPoint != null)
            {
                InitialSpawnPoint.SpawnPlayer(Players[0]);
                TopDownEngineEvent.Trigger(TopDownEngineEventTypes.SpawnComplete, Players[0]);
                return;
            }
        }

        // 跳转到指定关卡
        public virtual void GotoLevel(string levelName)
        {
            TriggerEndLevelEvents();
            StartCoroutine(GotoLevelCo(levelName));
        }

        // 触发关卡结束事件
        public virtual void TriggerEndLevelEvents()
        {
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LevelEnd, null);
            MMGameEvent.Trigger("Save");
        }

        // 跳转关卡协程
        protected virtual IEnumerator GotoLevelCo(string levelName)
        {
            // 禁用所有玩家
            if (Players != null && Players.Count > 0)
            { 
                foreach (Character player in Players)
                {
                    player.Disable();	
                }	    		
            }

            // 触发淡入效果
            MMFadeInEvent.Trigger(OutroFadeDuration, FadeCurve, FaderID);
            
            // 等待淡入完成
            if (Time.timeScale > 0.0f)
            { 
                yield return new WaitForSeconds(OutroFadeDuration);
            }

            // 触发取消暂停和加载下一场景事件
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.UnPause, null);
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LoadNextScene, null);

            // 确定目标场景
            string destinationScene = (string.IsNullOrEmpty(levelName)) ? "StartScreen" : levelName;

            // 根据加载模式加载场景
            switch (LoadingSceneMode)
            {
                case MMLoadScene.LoadingSceneModes.UnityNative:
                    SceneManager.LoadScene(destinationScene);			        
                    break;
                case MMLoadScene.LoadingSceneModes.MMSceneLoadingManager:
                    MMSceneLoadingManager.LoadScene(destinationScene, LoadingSceneName);
                    break;
                case MMLoadScene.LoadingSceneModes.MMAdditiveSceneLoadingManager:
                    MMAdditiveSceneLoadingManager.LoadScene(levelName, AdditiveLoadingSettings);
                    break;
            }
        }


        // 开始复活
        protected virtual void Respawn()
        {
            if (Players.Count < 2)
            {
                StartCoroutine(SoloModeRestart());
            }
        }

        // 单人模式重启协程
        protected virtual IEnumerator SoloModeRestart()
        {
            // 如果没有玩家预制体和场景角色，直接返回
            if ((PlayerPrefabs.Length <= 0))
            {
                yield break;
            }

            // 处理生命值系统
            if (GameManager.Instance.MaximumLives > 0)
            {
                GameManager.Instance.LoseLife();
                if (GameManager.Instance.CurrentLives <= 0)
                {
                    TopDownEngineEvent.Trigger(TopDownEngineEventTypes.GameOver, null);
                    if ((GameManager.Instance.GameOverScene != null) && (GameManager.Instance.GameOverScene != ""))
                    {
                        MMSceneLoadingManager.LoadScene(GameManager.Instance.GameOverScene);
                    }
                }
            }

            // 停止相机跟随
            MMCameraEvent.Trigger(MMCameraEventTypes.StopFollowing);

            // 触发淡入效果
            MMFadeInEvent.Trigger(OutroFadeDuration, FadeCurve, FaderID, true, Players[0].transform.position);
            yield return new WaitForSeconds(OutroFadeDuration);

            // 等待复活延迟
            yield return new WaitForSeconds(RespawnDelay);
            
            // 触发淡出效果
            MMFadeOutEvent.Trigger(OutroFadeDuration, FadeCurve, FaderID, true, Players[0].transform.position);

            // 确保当前检查点存在
            if (CurrentCheckpoint == null)
            {
                CurrentCheckpoint = InitialSpawnPoint;
            }

            // 如果玩家不存在，重新实例化
            if (Players[0] == null)
            {
                InstantiatePlayableCharacters();
            }

            // 在检查点生成玩家
            if (CurrentCheckpoint != null)
            {
                CurrentCheckpoint.SpawnPlayer(Players[0]);
            }
            else
            {
                Debug.LogWarning("LevelManager : no checkpoint or initial spawn point has been defined, can't respawn the Player.");
            }

            // 重置开始时间
            _started = DateTime.UtcNow;
            
            // 恢复相机跟随
            MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);

            // 重置分数并触发复活完成事件
            TopDownEnginePointEvent.Trigger(PointsMethods.Set, 0);
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.RespawnComplete, Players[0]);
            yield break;
        }

        // 设置当前检查点
        public virtual void SetCurrentCheckpoint(CheckPoint newCheckPoint)
        {
            // 如果检查点强制分配，直接设置
            if (newCheckPoint.ForceAssignation)
            {
                CurrentCheckpoint = newCheckPoint;
                return;
            }

            // 如果没有当前检查点，直接设置
            if (CurrentCheckpoint == null)
            {
                CurrentCheckpoint = newCheckPoint;
                return;
            }

            // 只有当新检查点的顺序大于当前检查点时才更新
            if (newCheckPoint.CheckPointOrder >= CurrentCheckpoint.CheckPointOrder)
            {
                CurrentCheckpoint = newCheckPoint;
            }
        }

        // 处理引擎事件
        public virtual void OnMMEvent(TopDownEngineEvent engineEvent)
        {
            switch (engineEvent.EventType)
            {
                case TopDownEngineEventTypes.RespawnStarted:
                    Respawn();
                    break;
            }
        }

        // 启用事件监听
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<TopDownEngineEvent>();
        }

        // 禁用事件监听
        protected virtual void OnDisable()
        {
            this.MMEventStopListening<TopDownEngineEvent>();
        }
    }
}
