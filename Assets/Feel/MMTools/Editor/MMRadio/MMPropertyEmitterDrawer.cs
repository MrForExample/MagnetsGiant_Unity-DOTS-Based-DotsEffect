using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;

namespace MoreMountains.Tools
{
	[CustomPropertyDrawer(typeof(MMPropertyEmitter), true)]
	[CanEditMultipleObjects]
	public class MMPropertyEmitterDrawer : MMPropertyPickerDrawer
	{
		protected Color _mmBlue = new Color(0.2235294f, 0.6745098f, 1f);
		protected Color _mmRed = MMColors.Orangered;

		protected override void FillAuthorizedTypes()
		{
			_authorizedTypes = new Type[]
			{
				typeof(float),
				typeof(Vector2),
				typeof(Vector3),
				typeof(Vector4),
				typeof(Quaternion),
				typeof(int),
				typeof(bool)
			};
		}

		/// <summary>
		/// Defines the height of the drawer
		/// </summary>
		/// <param name="property"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public override float AdditionalHeight()
		{
			int additionalLines = 0;
			if (_TargetObject != null)
			{
				if ((_selectedPropertyIndex != 0) && (_propertyType != null))
				{
					additionalLines = 1;
					if (_propertyType == typeof(bool))
					{
						additionalLines = 2;
					}
					if (_propertyType == typeof(float))
					{
						additionalLines = 4;
					}
					if (_propertyType == typeof(int))
					{
						additionalLines = 4;
					}
					if (_propertyType == typeof(Vector2))
					{
						additionalLines = 5;
					}
					if (_propertyType == typeof(Vector3))
					{
						additionalLines = 5;
					}
					if (_propertyType == typeof(Vector4))
					{
						additionalLines = 5;
					}
					if (_propertyType == typeof(Quaternion))
					{
						additionalLines = 5;
					}
				}

				if (Application.isPlaying)
				{
					additionalLines += 1;
				}
			}
			_numberOfLines = _numberOfLines + additionalLines;

			return _lineHeight * additionalLines + _lineMargin * additionalLines - 1;
		}

		/// <summary>
		/// Draws the inspector
		/// </summary>
		/// <param name="position"></param>
		/// <param name="property"></param>
		/// <param name="label"></param>
		protected override void DisplayAdditionalProperties(Rect position, SerializedProperty property, GUIContent label)
		{
			Rect additional1Rect = new Rect(position.x, position.y + (_lineHeight + _lineMargin) * 4, position.width, _lineHeight);
			Rect additional2Rect = new Rect(position.x, position.y + (_lineHeight + _lineMargin) * 5, position.width, _lineHeight);
			Rect additional3Rect = new Rect(position.x, position.y + (_lineHeight + _lineMargin) * 6, position.width, _lineHeight);
			Rect additional4Rect = new Rect(position.x, position.y + (_lineHeight + _lineMargin) * 7, position.width, _lineHeight);
			Rect additional5Rect = new Rect(position.x, position.y + (_lineHeight + _lineMargin) * 8, position.width, _lineHeight);
			Rect additional6Rect = new Rect(position.x, position.y + (_lineHeight + _lineMargin) * 9, position.width, _lineHeight);
			Rect additional7Rect = new Rect(position.x, position.y + (_lineHeight + _lineMargin) * 10, position.width, _lineHeight);
			Rect additional8Rect = new Rect(position.x, position.y + (_lineHeight + _lineMargin) * 11, position.width, _lineHeight);
			Rect additional9Rect = new Rect(position.x, position.y + (_lineHeight + _lineMargin) * 12, position.width, _lineHeight);
			Rect additional10Rect = new Rect(position.x, position.y + (_lineHeight + _lineMargin) * 13, position.width, _lineHeight);

			// displays the related properties
			if ((_selectedPropertyIndex != 0) && (_propertyType != null))
			{                
				if (_propertyType == typeof(bool))
				{
					EditorGUI.PropertyField(additional1Rect, property.FindPropertyRelative("BoolRemapFalse"), new GUIContent("Remap False"), true);
					EditorGUI.PropertyField(additional2Rect, property.FindPropertyRelative("BoolRemapTrue"), new GUIContent("Remap True"), true);
				}

				if (_propertyType == typeof(float))
				{
					EditorGUI.PropertyField(additional1Rect, property.FindPropertyRelative("FloatRemapMinToZero"), new GUIContent("RemapToZero"), true);
					EditorGUI.PropertyField(additional2Rect, property.FindPropertyRelative("FloatRemapMaxToOne"), new GUIContent("RemapToOne"), true);
					EditorGUI.PropertyField(additional3Rect, property.FindPropertyRelative("ClampMin"), new GUIContent("ClampMin"), true);
					EditorGUI.PropertyField(additional4Rect, property.FindPropertyRelative("ClampMax"), new GUIContent("ClampMax"), true);
				}

				if (_propertyType == typeof(Vector2))
				{
					EditorGUI.PropertyField(additional1Rect, property.FindPropertyRelative("Vector2Option"), new GUIContent("Target axis"), true);
					EditorGUI.PropertyField(additional2Rect, property.FindPropertyRelative("FloatRemapMinToZero"), new GUIContent("RemapToZero"), true);
					EditorGUI.PropertyField(additional3Rect, property.FindPropertyRelative("FloatRemapMaxToOne"), new GUIContent("RemapToOne"), true);
					EditorGUI.PropertyField(additional4Rect, property.FindPropertyRelative("ClampMin"), new GUIContent("ClampMin"), true);
					EditorGUI.PropertyField(additional5Rect, property.FindPropertyRelative("ClampMax"), new GUIContent("ClampMax"), true);
				}

				if (_propertyType == typeof(Vector3))
				{
					EditorGUI.PropertyField(additional1Rect, property.FindPropertyRelative("Vector3Option"), new GUIContent("Target axis"), true);
					EditorGUI.PropertyField(additional2Rect, property.FindPropertyRelative("FloatRemapMinToZero"), new GUIContent("RemapToZero"), true);
					EditorGUI.PropertyField(additional3Rect, property.FindPropertyRelative("FloatRemapMaxToOne"), new GUIContent("RemapToOne"), true);
					EditorGUI.PropertyField(additional4Rect, property.FindPropertyRelative("ClampMin"), new GUIContent("ClampMin"), true);
					EditorGUI.PropertyField(additional5Rect, property.FindPropertyRelative("ClampMax"), new GUIContent("ClampMax"), true);
				}

				if (_propertyType == typeof(Vector4))
				{
					EditorGUI.PropertyField(additional1Rect, property.FindPropertyRelative("Vector4Option"), new GUIContent("Target axis"), true);
					EditorGUI.PropertyField(additional2Rect, property.FindPropertyRelative("FloatRemapMinToZero"), new GUIContent("RemapToZero"), true);
					EditorGUI.PropertyField(additional3Rect, property.FindPropertyRelative("FloatRemapMaxToOne"), new GUIContent("RemapToOne"), true);
					EditorGUI.PropertyField(additional4Rect, property.FindPropertyRelative("ClampMin"), new GUIContent("ClampMin"), true);
					EditorGUI.PropertyField(additional5Rect, property.FindPropertyRelative("ClampMax"), new GUIContent("ClampMax"), true);
				}

				if (_propertyType == typeof(Quaternion))
				{
					EditorGUI.PropertyField(additional1Rect, property.FindPropertyRelative("Vector3Option"), new GUIContent("Target axis"), true);
					EditorGUI.PropertyField(additional2Rect, property.FindPropertyRelative("QuaternionRemapMinToZero"), new GUIContent("Remap Zero"), true);
					EditorGUI.PropertyField(additional3Rect, property.FindPropertyRelative("QuaternionRemapMaxToOne"), new GUIContent("Remap One"), true);
					EditorGUI.PropertyField(additional4Rect, property.FindPropertyRelative("ClampMin"), new GUIContent("ClampMin"), true);
					EditorGUI.PropertyField(additional5Rect, property.FindPropertyRelative("ClampMax"), new GUIContent("ClampMax"), true);
				}

				if (_propertyType == typeof(int))
				{
					EditorGUI.PropertyField(additional1Rect, property.FindPropertyRelative("IntRemapMinToZero"), new GUIContent("RemapToZero"), true);
					EditorGUI.PropertyField(additional2Rect, property.FindPropertyRelative("IntRemapMaxToOne"), new GUIContent("RemapToOne"), true);
					EditorGUI.PropertyField(additional3Rect, property.FindPropertyRelative("ClampMin"), new GUIContent("ClampMin"), true);
					EditorGUI.PropertyField(additional4Rect, property.FindPropertyRelative("ClampMax"), new GUIContent("ClampMax"), true);
				}
			}

			if ((_TargetObject != null) && (_selectedPropertyIndex != 0) && (_propertyType != null) && (Application.isPlaying))
			{
				// if the application is playing, we display a progress bar

				float level = property.FindPropertyRelative("Level").floatValue;
				DrawLevelProgressBar(position, level, _mmBlue, _mmRed);
			}
		}

	}
}