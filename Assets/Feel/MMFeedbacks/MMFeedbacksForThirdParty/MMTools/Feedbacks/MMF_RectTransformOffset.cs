﻿using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control the offset of the lower left corner of the rectangle relative to the lower left anchor, and the offset of the upper right corner of the rectangle relative to the upper right anchor.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback lets you control the offset of the lower left corner of the rectangle relative to the lower left anchor, and the offset of the upper right corner of the rectangle relative to the upper right anchor.")]
	[FeedbackPath("UI/RectTransform Offset")]
	public class MMF_RectTransformOffset : MMF_FeedbackBase
	{
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetRectTransform == null); }
		public override string RequiredTargetText { get { return TargetRectTransform != null ? TargetRectTransform.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetRectTransform be set to be able to work properly. You can set one below."; } }
		#endif

		[MMFInspectorGroup("Target RectTransform", true, 37, true)]
		/// The RectTransform we want to modify
		public RectTransform TargetRectTransform;

		[MMFInspectorGroup("Offset Min", true, 40)] 
		/// whether we should modify the offset min or not
		[Tooltip("whether we should modify the offset min or not")]
		public bool ModifyOffsetMin = true;
		/// the curve to animate the min offset on
		[Tooltip("the curve to animate the min offset on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public MMTweenType OffsetMinCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// the value to remap the min curve's 0 on
		[Tooltip("the value to remap the min curve's 0 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public Vector2 OffsetMinRemapZero = Vector2.zero;
		/// the value to remap the min curve's 1 on
		[Tooltip("the value to remap the min curve's 1 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)MMFeedbackBase.Modes.Instant)]
		public Vector2 OffsetMinRemapOne = Vector2.one;
        
		[MMFInspectorGroup("Offset Max", true, 41)] 
		/// whether we should modify the offset max or not
		[Tooltip("whether we should modify the offset max or not")]
		public bool ModifyOffsetMax = true;
		/// the curve to animate the max offset on
		[Tooltip("the curve to animate the max offset on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public MMTweenType OffsetMaxCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// the value to remap the max curve's 0 on
		[Tooltip("the value to remap the max curve's 0 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public Vector2 OffsetMaxRemapZero = Vector2.zero;
		/// the value to remap the max curve's 1 on
		[Tooltip("the value to remap the max curve's 1 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)MMFeedbackBase.Modes.Instant)]
		public Vector2 OffsetMaxRemapOne = Vector2.one;
        
		protected override void FillTargets()
		{
			if (TargetRectTransform == null)
			{
				return;
			}
            
			MMF_FeedbackBaseTarget targetMin = new MMF_FeedbackBaseTarget();
			MMPropertyReceiver receiverMin = new MMPropertyReceiver();
			receiverMin.TargetObject = TargetRectTransform.gameObject;
			receiverMin.TargetComponent = TargetRectTransform;
			receiverMin.TargetPropertyName = "offsetMin";
			receiverMin.RelativeValue = RelativeValues;
			receiverMin.Vector2RemapZero = OffsetMinRemapZero;
			receiverMin.Vector2RemapOne = OffsetMinRemapOne;
			receiverMin.ShouldModifyValue = ModifyOffsetMin;
			targetMin.Target = receiverMin;
			targetMin.LevelCurve = OffsetMinCurve;
			targetMin.RemapLevelZero = 0f;
			targetMin.RemapLevelOne = 1f;
			targetMin.InstantLevel = 1f;

			_targets.Add(targetMin);
            
			MMF_FeedbackBaseTarget targetMax = new MMF_FeedbackBaseTarget();
			MMPropertyReceiver receiverMax = new MMPropertyReceiver();
			receiverMax.TargetObject = TargetRectTransform.gameObject;
			receiverMax.TargetComponent = TargetRectTransform;
			receiverMax.TargetPropertyName = "offsetMax";
			receiverMax.RelativeValue = RelativeValues;
			receiverMax.Vector2RemapZero = OffsetMaxRemapZero;
			receiverMax.Vector2RemapOne = OffsetMaxRemapOne;
			receiverMax.ShouldModifyValue = ModifyOffsetMax;
			targetMax.Target = receiverMax;
			targetMax.LevelCurve = OffsetMaxCurve;
			targetMax.RemapLevelZero = 0f;
			targetMax.RemapLevelOne = 1f;
			targetMax.InstantLevel = 1f;

			_targets.Add(targetMax);
		}

	}
}