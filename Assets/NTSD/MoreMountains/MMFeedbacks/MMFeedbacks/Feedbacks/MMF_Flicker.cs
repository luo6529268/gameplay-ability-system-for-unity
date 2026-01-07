using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// This feedback will make the bound renderer flicker for the set duration when played (and restore its initial color when stopped)
    /// 此反馈在播放时会使得绑定的渲染器闪烁设定的持续时间（并在停止时恢复其初始颜色）
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("此反馈允许你闪烁指定渲染器（精灵、网格等）的颜色，持续一定时间，在指定的八度音阶，并使用指定的颜色。例如，当角色受到伤害时非常有用（但远不止如此！）.")]
    [MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Renderer/Flicker")]
	public class MMF_Flicker : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		public override bool EvaluateRequiresSetup() => (BoundRenderer == null);
		public override string RequiredTargetText => BoundRenderer != null ? BoundRenderer.name : "";
		public override string RequiresSetupText => "此反馈需要设置一个绑定的渲染器才能正常工作。你可以在下方设置。";
		#endif
        public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundRenderer = FindAutomatedTarget<Renderer>();

        /// <summary>
        /// 可能的模式
        /// Color：将控制 material.color
        /// PropertyName：将根据名称定位特定的着色器属性
        /// </summary>
        public enum Modes { Color, PropertyName }


        [MMFInspectorGroup("Flicker", true, 61, true)]
        /// <summary>
        /// 播放时闪烁的渲染器
        /// </summary>
        [Tooltip("播放时闪烁的渲染器")]
        public Renderer BoundRenderer;
        /// <summary>
        /// 播放时闪烁的更多渲染器
        /// </summary>
        [Tooltip("播放时闪烁的更多渲染器")]
        public List<Renderer> ExtraBoundRenderers;
        /// <summary>
        /// 闪烁渲染器的选定模式
        /// </summary>
        [Tooltip("闪烁渲染器的选定模式")]
        public Modes Mode = Modes.Color;
        /// <summary>
        /// 要定位的属性名称
        /// </summary>
        [MMFEnumCondition("Mode", (int)Modes.PropertyName)]
        [Tooltip("要定位的属性名称")]
        public string PropertyName = "_Tint";
        /// <summary>
        /// 受到攻击时闪烁的持续时间
        /// </summary>
        [Tooltip("受到攻击时闪烁的持续时间")]
        public float FlickerDuration = 0.2f;
        /// <summary>
        /// 闪烁周期的持续时间
        /// </summary>
        [Tooltip("闪烁周期的持续时间")]
        [FormerlySerializedAs("FlickerOctave")]
        public float FlickerPeriod = 0.04f;
        /// <summary>
        /// 我们应该将精灵闪烁到的颜色
        /// </summary>
        [Tooltip("我们应该将精灵闪烁到的颜色")]
        [ColorUsage(true, true)]
        public Color FlickerColor = new Color32(255, 20, 20, 255);
        /// <summary>
        /// 我们想要在目标渲染器上闪烁的材料索引列表。如果留空，将只针对索引0的材料
        /// </summary>
        [Tooltip("我们想要在目标渲染器上闪烁的材料索引列表。如果留空，将只针对索引0的材料")]
        public int[] MaterialIndexes;
        /// <summary>
        /// 如果为真，则此组件将使用材质属性块而不是在材质实例上工作
        /// </summary>
        [Tooltip("如果为真，则此组件将使用材质属性块而不是在材质实例上工作。")]
        public bool UseMaterialPropertyBlocks = false;
        /// <summary>
        /// 如果在精灵渲染器上使用材质属性块，你将需要确保在更新它时将精灵纹理传递给块。为此，你需要指定你的精灵材质的着色器的纹理属性名称。如果你不使用精灵渲染器，你可以安全地忽略这一点
        /// </summary>
        [Tooltip("如果在精灵渲染器上使用材质属性块，你将需要确保在更新它时将精灵纹理传递给块。为此，你需要指定你的精灵材质的着色器的纹理属性名称。如果你不使用精灵渲染器，你可以安全地忽略这一点。")]
        [MMCondition("UseMaterialPropertyBlocks", true)]
        public string SpriteRendererTextureProperty = "_MainTex";

        /// 此反馈的持续时间是闪烁的持续时间
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(FlickerDuration); } set { FlickerDuration = value; } }

		protected const string _colorPropertyName = "_Color";
        
		protected int[] _propertyIDs;
		protected bool[] _propertiesFound;
		protected bool _spriteRendererIsNull;
		
		protected Coroutine[] _coroutines;
		protected List<Coroutine[]> _extraCoroutines;
		
		protected Color[] _initialFlickerColors;
		protected List<Color[]> _extraInitialFlickerColors;
		
		protected MaterialPropertyBlock _propertyBlock;
		protected List<MaterialPropertyBlock> _extraPropertyBlocks;
		
		protected SpriteRenderer _spriteRenderer;
		protected List<SpriteRenderer> _spriteRenderers;
		
		protected Texture2D _spriteRendererTexture;
		protected List<Texture2D> _spriteRendererTextures;

        /// <summary>
        /// On init we grab our initial color and components
        /// 在初始化时，我们获取初始颜色和组件
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			// init material indexes
			if (MaterialIndexes.Length == 0)
			{
				MaterialIndexes = new int[1];
				MaterialIndexes[0] = 0;
			}

			_coroutines = new Coroutine[MaterialIndexes.Length];
			_initialFlickerColors = new Color[MaterialIndexes.Length];
			
			_extraCoroutines = new List<Coroutine[]>();
			_extraInitialFlickerColors = new List<Color[]>();
			foreach (Renderer renderer in ExtraBoundRenderers)
			{
				_extraCoroutines.Add(new Coroutine[MaterialIndexes.Length]);
				_extraInitialFlickerColors.Add(new Color[MaterialIndexes.Length]);
			}
			
			_propertyIDs = new int[MaterialIndexes.Length];
			_propertiesFound = new bool[MaterialIndexes.Length];
			_propertyBlock = new MaterialPropertyBlock();

			AcquireRenderers(owner);
			StoreSpriteRendererTexture();

			for (int i = 0; i < MaterialIndexes.Length; i++)
			{
				_propertiesFound[i] = false;
				int index = MaterialIndexes[i];

				if (Active && (BoundRenderer != null))
				{
					if (Mode == Modes.Color)
					{
						_propertiesFound[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[index].HasProperty(_colorPropertyName) : BoundRenderer.materials[index].HasProperty(_colorPropertyName);
						if (_propertiesFound[i])
						{
							_initialFlickerColors[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[index].color : BoundRenderer.materials[index].color;
							foreach (Renderer renderer in ExtraBoundRenderers)
							{
								_extraInitialFlickerColors[ExtraBoundRenderers.IndexOf(renderer)][i] = UseMaterialPropertyBlocks ? renderer.sharedMaterials[index].color : renderer.materials[index].color;
							}
						}
					}
					else
					{
						_propertiesFound[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[index].HasProperty(PropertyName) : BoundRenderer.materials[index].HasProperty(PropertyName); 
						if (_propertiesFound[i])
						{
							_propertyIDs[i] = Shader.PropertyToID(PropertyName);
							_initialFlickerColors[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[index].GetColor(_propertyIDs[i]) : BoundRenderer.materials[index].GetColor(_propertyIDs[i]);
							foreach (Renderer renderer in ExtraBoundRenderers)
							{
								_extraInitialFlickerColors[ExtraBoundRenderers.IndexOf(renderer)][i] = UseMaterialPropertyBlocks ? renderer.sharedMaterials[index].GetColor(_propertyIDs[i]) : renderer.materials[index].GetColor(_propertyIDs[i]);
							}
						}
					}
				}
			}
		}

		protected virtual void AcquireRenderers(MMF_Player owner)
		{
			if (Active && (BoundRenderer == null) && (owner != null))
			{
				if (Owner.gameObject.MMFGetComponentNoAlloc<Renderer>() != null)
				{
					BoundRenderer = owner.GetComponent<Renderer>();
				}
				if (BoundRenderer == null)
				{
					BoundRenderer = owner.GetComponentInChildren<Renderer>();
				}
			}
			if (BoundRenderer == null)
			{
				Debug.LogWarning("[MMFeedbackFlicker] The flicker feedback on "+Owner.name+" doesn't have a bound renderer, it won't work. You need to specify a renderer to flicker in its inspector.");    
			}
			
			_spriteRenderer = BoundRenderer.GetComponent<SpriteRenderer>();
			_spriteRenderers = new List<SpriteRenderer>();
			foreach (Renderer renderer in ExtraBoundRenderers)
			{
				if (renderer.GetComponent<SpriteRenderer>() != null)
				{
					_spriteRenderers.Add(renderer.GetComponent<SpriteRenderer>());
				}
			}
			_spriteRendererIsNull = _spriteRenderer == null;
		}

		protected virtual void StoreSpriteRendererTexture()
		{
			if (_spriteRendererIsNull)
			{
				return;
			}
			_spriteRendererTexture = _spriteRenderer.sprite.texture;
			_spriteRendererTextures = new List<Texture2D>();
			for (var index = 0; index < ExtraBoundRenderers.Count; index++)
			{
				_spriteRendererTextures.Add(_spriteRenderers[index].sprite.texture);
			}
		}

		/// <summary>
		/// On play we make our renderer flicker
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (BoundRenderer == null))
			{
				return;
			}
			for (int i = 0; i < MaterialIndexes.Length; i++)
			{
				if (_coroutines[i] != null) { Owner.StopCoroutine(_coroutines[i]); }
				_coroutines[i] = Owner.StartCoroutine(Flicker(BoundRenderer, i, _initialFlickerColors[i], FlickerColor, FlickerPeriod, FeedbackDuration));
				for (var index = 0; index < ExtraBoundRenderers.Count; index++)
				{
					_extraCoroutines[index][i] = Owner.StartCoroutine(Flicker(ExtraBoundRenderers[index], i, _extraInitialFlickerColors[index][i], FlickerColor, FlickerPeriod, FeedbackDuration));
				}
			}
		}

		/// <summary>
		/// On reset we make our renderer stop flickering
		/// </summary>
		protected override void CustomReset()
		{
			base.CustomReset();

			if (InCooldown)
			{
				return;
			}

			if (Active && FeedbackTypeAuthorized && (BoundRenderer != null))
			{
				for (int i = 0; i < MaterialIndexes.Length; i++)
				{
					SetColor(BoundRenderer, i, _initialFlickerColors[i]);
				}
			}
			
			foreach (Renderer renderer in ExtraBoundRenderers)
			{
				for (int i = 0; i < MaterialIndexes.Length; i++)
				{
					SetColor(renderer, i, _extraInitialFlickerColors[ExtraBoundRenderers.IndexOf(renderer)][i]);
				}
			}
		}
		
		protected virtual void SetStoredSpriteRendererTexture(Renderer renderer, MaterialPropertyBlock block)
		{
			if (_spriteRendererIsNull)
			{
				return;
			}

			if (renderer == BoundRenderer)
			{
				block.SetTexture(SpriteRendererTextureProperty, _spriteRendererTexture);	
			}
			else
			{
				block.SetTexture(SpriteRendererTextureProperty, _spriteRendererTextures[ExtraBoundRenderers.IndexOf(renderer)]);
			}
		}

		public virtual IEnumerator Flicker(Renderer renderer, int materialIndex, Color initialColor, Color flickerColor, float flickerSpeed, float flickerDuration)
		{
			if (renderer == null)
			{
				yield break;
			}

			if (!_propertiesFound[materialIndex])
			{
				yield break;
			}

			if (initialColor == flickerColor)
			{
				yield break;
			}

			float flickerStop = FeedbackTime + flickerDuration;
			IsPlaying = true;
			
			StoreSpriteRendererTexture();
            
			while (FeedbackTime < flickerStop)
			{
				SetColor(renderer, materialIndex, flickerColor);
				yield return WaitFor(flickerSpeed);
				SetColor(renderer, materialIndex, initialColor);
				yield return WaitFor(flickerSpeed);
			}

			SetColor(renderer, materialIndex, initialColor);
			IsPlaying = false;
		}


		protected virtual void SetColor(Renderer renderer, int materialIndex, Color color)
		{
			if (!_propertiesFound[materialIndex])
			{
				return;
			}
            
			if (Mode == Modes.Color)
			{
				if (UseMaterialPropertyBlocks)
				{
					renderer.GetPropertyBlock(_propertyBlock, MaterialIndexes[materialIndex]);
					_propertyBlock.SetColor(_colorPropertyName, color);
					SetStoredSpriteRendererTexture(renderer, _propertyBlock);
					renderer.SetPropertyBlock(_propertyBlock, MaterialIndexes[materialIndex]);
				}
				else
				{
					renderer.materials[MaterialIndexes[materialIndex]].color = color;
				}
			}
			else
			{
				if (UseMaterialPropertyBlocks)
				{
					renderer.GetPropertyBlock(_propertyBlock, MaterialIndexes[materialIndex]);
					_propertyBlock.SetColor(_propertyIDs[materialIndex], color);
					SetStoredSpriteRendererTexture(renderer, _propertyBlock);
					renderer.SetPropertyBlock(_propertyBlock, MaterialIndexes[materialIndex]);
				}
				else
				{
					renderer.materials[MaterialIndexes[materialIndex]].SetColor(_propertyIDs[materialIndex], color);
				}
			}            
		}
        
		/// <summary>
		/// Stops this feedback
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
            
			IsPlaying = false;
			for (int i = 0; i < _coroutines.Length; i++)
			{
				if (_coroutines[i] != null)
				{
					Owner.StopCoroutine(_coroutines[i]);    
				}
				_coroutines[i] = null;  
			}
			foreach (Renderer renderer in ExtraBoundRenderers)
			{
				for (int i = 0; i < MaterialIndexes.Length; i++)
				{
					if (_extraCoroutines[ExtraBoundRenderers.IndexOf(renderer)][i] != null)
					{
						Owner.StopCoroutine(_extraCoroutines[ExtraBoundRenderers.IndexOf(renderer)][i]);
					}
					_extraCoroutines[ExtraBoundRenderers.IndexOf(renderer)][i] = null;
				}
			}
		}
		
		/// <summary>
		/// On restore, we put our object back at its initial position
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			CustomReset();
		}
	}
}