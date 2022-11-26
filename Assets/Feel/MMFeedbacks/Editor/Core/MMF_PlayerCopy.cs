﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using Object = UnityEngine.Object;


namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A helper class to copy and paste feedback properties
	/// </summary>
	static class MMF_PlayerCopy
	{
		// Single Copy --------------------------------------------------------------------

		static public System.Type Type { get; private set; }
		static List<SerializedProperty> Properties = new List<SerializedProperty>();
        
		public static readonly List<MMF_Feedback> CopiedFeedbacks = new List<MMF_Feedback>();

		public static List<MMF_Player> ShouldKeepChanges = new List<MMF_Player>();

		static string[] IgnoreList = new string[]
		{
			"m_ObjectHideFlags",
			"m_CorrespondingSourceObject",
			"m_PrefabInstance",
			"m_PrefabAsset",
			"m_GameObject",
			"m_Enabled",
			"m_EditorHideFlags",
			"m_Script",
			"m_Name",
			"m_EditorClassIdentifier"
		};

		static public bool HasCopy()
		{
			return CopiedFeedbacks != null && CopiedFeedbacks.Count == 1;
		}

		static public bool HasMultipleCopies()
		{
			return CopiedFeedbacks != null && CopiedFeedbacks.Count > 1;
		}

		static public void Copy(MMF_Feedback feedback)
		{
			Type feedbackType = feedback.GetType();
			MMF_Feedback newFeedback = (MMF_Feedback)Activator.CreateInstance(feedbackType);
			EditorUtility.CopySerializedManagedFieldsOnly(feedback, newFeedback);
			CopiedFeedbacks.Clear();
			CopiedFeedbacks.Add(newFeedback);
		}
        
		static public void CopyAll(MMF_Player sourceFeedbacks)
		{
			CopiedFeedbacks.Clear();
			foreach (MMF_Feedback feedback in sourceFeedbacks.FeedbacksList)
			{
				Type feedbackType = feedback.GetType();
				MMF_Feedback newFeedback = (MMF_Feedback)Activator.CreateInstance(feedbackType);
				EditorUtility.CopySerializedManagedFieldsOnly(feedback, newFeedback);
				CopiedFeedbacks.Add(newFeedback);    
			}
		}

		// Multiple Copy ----------------------------------------------------------


		static public void PasteAll(MMF_PlayerEditor targetEditor)
		{
			foreach (MMF_Feedback feedback in MMF_PlayerCopy.CopiedFeedbacks)
			{
				targetEditor.TargetMmfPlayer.AddFeedback(feedback);
			}
			CopiedFeedbacks.Clear();
		}
	}
}