using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Rendering;

namespace MoreMountains.TopDownEngine
{

	[CustomEditor (typeof(Character), true)]
	[CanEditMultipleObjects]

	/// <summary>
	/// Adds custom labels to the Character inspector
	/// </summary>

	public class CharacterInspector : Editor 
	{		
		public enum Modes { TwoD, ThreeD }


        /// <summary>
        /// 在检查角色时，向常规检查器添加一些标签，这些标签对于调试很有用。
        /// </summary>
        public override void OnInspectorGUI()
		{
			serializedObject.Update();

			Character character = (Character)target;


			// draws the default inspector if in Player mode
			if (character.CharacterType == Character.CharacterTypes.Player)
			{
				DrawDefaultInspector();
			}

			// in AI mode draws everything but the PlayerID field
			if (character.CharacterType == Character.CharacterTypes.AI)
			{
				Editor.DrawPropertiesExcluding(serializedObject, new string[] { "PlayerID" });
			}
			
			serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// Adds all the possible components to a character
		/// </summary>
		protected virtual void GenerateCharacter(Character.CharacterTypes type, Modes mode)
		{
			Character character = (Character)target;

			Debug.LogFormat(character.name + " : Character Autobuild Start");

			if (type == Character.CharacterTypes.Player)
			{
				character.CharacterType = Character.CharacterTypes.Player;
				// sets the layer
				character.gameObject.layer = LayerMask.NameToLayer("Player");
				// sets the tag
				character.gameObject.tag = "Player";
			}

			if (type == Character.CharacterTypes.AI)
			{
				character.CharacterType = Character.CharacterTypes.AI;
				// sets the layer
				character.gameObject.layer = LayerMask.NameToLayer("Enemies");
			}

			if (mode == Modes.TwoD)
			{
				// Adds the rigidbody2D
				Rigidbody2D rigidbody2D = (character.GetComponent<Rigidbody2D>() == null) ? character.gameObject.AddComponent<Rigidbody2D>() : character.GetComponent<Rigidbody2D>();
				rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
				rigidbody2D.simulated = true;
				rigidbody2D.useAutoMass = false;
				rigidbody2D.mass = 1;
				rigidbody2D.drag = 1;
				rigidbody2D.angularDrag = 0.05f;
				rigidbody2D.gravityScale = 0;
				rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
				rigidbody2D.sleepMode = RigidbodySleepMode2D.StartAwake;
				rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
				rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;

				SortingGroup sortingGroup = (character.GetComponent<SortingGroup>() == null) ? character.gameObject.AddComponent<SortingGroup>() : character.GetComponent<SortingGroup>();
				sortingGroup.sortingLayerName = "Characters";

				// Adds the boxcollider2D if needed
				BoxCollider2D boxcollider2D = (character.GetComponent<BoxCollider2D>() == null) ? character.gameObject.AddComponent<BoxCollider2D>() : character.GetComponent<BoxCollider2D>();
				boxcollider2D.isTrigger = false;

				// adds the top down controller 2D
				TopDownController2D topDownController2D = (character.GetComponent<TopDownController2D>() == null) ? character.gameObject.AddComponent<TopDownController2D>() : character.GetComponent<TopDownController2D>();
				topDownController2D.Gravity = -30;                
				topDownController2D.GroundLayerMask = LayerMask.GetMask("Ground");

				
			}

			if (mode == Modes.ThreeD)
			{
				// adds a character controller
				CharacterController characterController = (character.GetComponent<CharacterController>() == null) ? character.gameObject.AddComponent<CharacterController>() : character.GetComponent<CharacterController>();
				characterController.slopeLimit = 45f;
				characterController.stepOffset = 0.3f;
				characterController.skinWidth = 0.08f;
				characterController.minMoveDistance = 0.001f;
				characterController.radius = 0.5f;

				// adds a rigidbody
				Rigidbody rigidbody = (character.GetComponent<Rigidbody>() == null) ? character.gameObject.AddComponent<Rigidbody>() : character.GetComponent<Rigidbody>();
				rigidbody.mass = 1;
				rigidbody.drag = 0;
				rigidbody.angularDrag = 0.05f;
				rigidbody.interpolation = RigidbodyInterpolation.None;
				rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
				rigidbody.useGravity = true;
				rigidbody.isKinematic = true;

				
				
				
			}


			// adds health
			Health health = (character.GetComponent<Health>() == null) ? character.gameObject.AddComponent<Health>() : character.GetComponent<Health>();
			health.MaximumHealth = 100;
			health.CurrentHealth = 100;
            
			Debug.LogFormat(character.name + " : Character Autobuild Complete");
		}
	}
}