using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.EventSystems;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// Handles all GUI effects and changes
	/// </summary>
	[AddComponentMenu("TopDown Engine/Managers/GUI Manager")]
	public class GUIManager : MMSingleton<GUIManager> 
	{
        /// 主画布
        [Tooltip("主画布")]
        public Canvas MainCanvas;
        /// 包含抬头显示（头像、健康、分数...）的游戏对象
        [Tooltip("包含抬头显示（头像、健康、分数...）的游戏对象")]
        public GameObject HUD;
        /// 需要更新的健康条
        [Tooltip("需要更新的健康条")]
        public MMProgressBar[] HealthBars;
        /// 需要更新的冲刺条
        [Tooltip("需要更新的冲刺条")]
        public MMProgressBar[] DashBars;
        /// 用于显示当前武器弹药的面板和条
        [Tooltip("用于显示当前武器弹药的面板和条")]
        public AmmoDisplay[] AmmoDisplays;
        /// 暂停屏幕游戏对象
        [Tooltip("暂停屏幕游戏对象")]
        public GameObject PauseScreen;
        /// 死亡屏幕
        [Tooltip("死亡屏幕")]
        public GameObject DeathScreen;
        /// 移动按钮
        [Tooltip("移动按钮")]
        public CanvasGroup Buttons;
        /// 移动箭头
        [Tooltip("移动箭头")]
        public CanvasGroup Arrows;
        /// 移动摇杆
        [Tooltip("移动摇杆")]
        public CanvasGroup Joystick;
        /// 积分计数器
        [Tooltip("积分计数器")]
        public Text PointsText;
        /// 应用于格式化显示积分的模式
        [Tooltip("应用于格式化显示积分的模式")]
        public string PointsTextPattern = "000000";



        protected float _initialJoystickAlpha;
		protected float _initialButtonsAlpha;
		protected bool _initialized = false;
		
		/// <summary>
		/// Statics initialization to support enter play modes
		/// </summary>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		protected static void InitializeStatics()
		{
			_instance = null;
		}

		/// <summary>
		/// Initialization
		/// </summary>
		protected override void Awake()
		{
			base.Awake();

			Initialization();
		}

		protected virtual void Initialization()
		{
			if (_initialized)
			{
				return;
			}

			if (Joystick != null)
			{
				_initialJoystickAlpha = Joystick.alpha;
			}
			if (Buttons != null)
			{
				_initialButtonsAlpha = Buttons.alpha;
			}

			_initialized = true;
		}

		/// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Start()
		{
			RefreshPoints();
			SetPauseScreen(false);
			SetDeathScreen(false);
		}

		/// <summary>
		/// Sets the HUD active or inactive
		/// </summary>
		/// <param name="state">If set to <c>true</c> turns the HUD active, turns it off otherwise.</param>
		public virtual void SetHUDActive(bool state)
		{
			if (HUD!= null)
			{ 
				HUD.SetActive(state);
			}
			if (PointsText!= null)
			{ 
				PointsText.enabled = state;
			}
		}

		/// <summary>
		/// Sets the avatar active or inactive
		/// </summary>
		/// <param name="state">If set to <c>true</c> turns the HUD active, turns it off otherwise.</param>
		public virtual void SetAvatarActive(bool state)
		{
			if (HUD != null)
			{
				HUD.SetActive(state);
			}
		}

		/// <summary>
		/// Sets the pause screen on or off.
		/// </summary>
		/// <param name="state">If set to <c>true</c>, sets the pause.</param>
		public virtual void SetPauseScreen(bool state)
		{
			if (PauseScreen != null)
			{
				PauseScreen.SetActive(state);
				EventSystem.current.sendNavigationEvents = state;
			}
		}

		/// <summary>
		/// Sets the death screen on or off.
		/// </summary>
		/// <param name="state">If set to <c>true</c>, sets the pause.</param>
		public virtual void SetDeathScreen(bool state)
		{
			if (DeathScreen != null)
			{
				DeathScreen.SetActive(state);
				EventSystem.current.sendNavigationEvents = state;
			}
		}

		/// <summary>
		/// Sets the jetpackbar active or not.
		/// </summary>
		/// <param name="state">If set to <c>true</c>, sets the pause.</param>
		public virtual void SetDashBar(bool state, string playerID)
		{
			if (DashBars == null)
			{
				return;
			}

			foreach (MMProgressBar jetpackBar in DashBars)
			{
				if (jetpackBar != null)
				{ 
					if (jetpackBar.PlayerID == playerID)
					{
						jetpackBar.gameObject.SetActive(state);
					}					
				}
			}	        
		}

		/// <summary>
		/// Sets the ammo displays active or not
		/// </summary>
		/// <param name="state">If set to <c>true</c> state.</param>
		/// <param name="playerID">Player I.</param>
		public virtual void SetAmmoDisplays(bool state, string playerID, int ammoDisplayID)
		{
			if (AmmoDisplays == null)
			{
				return;
			}

			foreach (AmmoDisplay ammoDisplay in AmmoDisplays)
			{
				if (ammoDisplay != null)
				{ 
					if ((ammoDisplay.PlayerID == playerID) && (ammoDisplayID == ammoDisplay.AmmoDisplayID))
					{
						ammoDisplay.gameObject.SetActive(state);
					}					
				}
			}
		}
        		
		/// <summary>
		/// Sets the text to the game manager's points.
		/// </summary>
		public virtual void RefreshPoints()
		{
			if (PointsText!= null)
			{ 
				PointsText.text = GameManager.Instance.Points.ToString(PointsTextPattern);
			}
		}

		/// <summary>
		/// Updates the health bar.
		/// </summary>
		/// <param name="currentHealth">Current health.</param>
		/// <param name="minHealth">Minimum health.</param>
		/// <param name="maxHealth">Max health.</param>
		/// <param name="playerID">Player I.</param>
		public virtual void UpdateHealthBar(float currentHealth,float minHealth,float maxHealth,string playerID)
		{
			if (HealthBars == null) { return; }
			if (HealthBars.Length <= 0)	{ return; }

			foreach (MMProgressBar healthBar in HealthBars)
			{
				if (healthBar == null) { continue; }
				if (healthBar.PlayerID == playerID)
				{
					healthBar.UpdateBar(currentHealth,minHealth,maxHealth);
				}
			}

		}

		/// <summary>
		/// Updates the dash bars.
		/// </summary>
		/// <param name="currentFuel">Current fuel.</param>
		/// <param name="minFuel">Minimum fuel.</param>
		/// <param name="maxFuel">Max fuel.</param>
		/// <param name="playerID">Player I.</param>
		public virtual void UpdateDashBars(float currentFuel, float minFuel, float maxFuel,string playerID)
		{
			if (DashBars == null)
			{
				return;
			}

			foreach (MMProgressBar dashbar in DashBars)
			{
				if (dashbar == null) { return; }
				if (dashbar.PlayerID == playerID)
				{
					dashbar.UpdateBar(currentFuel,minFuel,maxFuel);	
				}    
			}
		}

        /// <summary>
        /// Updates the (optional) ammo displays.
        /// 更新（可选）弹药显示。
        /// </summary>
        /// <param name="magazineBased">If set to <c>true</c> magazine based.</param>
        /// <param name="totalAmmo">Total ammo.</param>
        /// <param name="maxAmmo">Max ammo.</param>
        /// <param name="ammoInMagazine">Ammo in magazine.</param>
        /// <param name="magazineSize">Magazine size.</param>
        /// <param name="playerID">Player I.</param>
        /// <param name="displayTotal">If set to <c>true</c> display total.</param>
        public virtual void UpdateAmmoDisplays(bool magazineBased, int totalAmmo, int maxAmmo, int ammoInMagazine, int magazineSize, string playerID, int ammoDisplayID, bool displayTotal)
		{
			if (AmmoDisplays == null)
			{
				return;
			}

			foreach (AmmoDisplay ammoDisplay in AmmoDisplays)
			{
				if (ammoDisplay == null) { return; }
				if ((ammoDisplay.PlayerID == playerID) && (ammoDisplayID == ammoDisplay.AmmoDisplayID))
				{
					ammoDisplay.UpdateAmmoDisplays (magazineBased, totalAmmo, maxAmmo, ammoInMagazine, magazineSize, displayTotal);
				}    
			}
		}
	}
}