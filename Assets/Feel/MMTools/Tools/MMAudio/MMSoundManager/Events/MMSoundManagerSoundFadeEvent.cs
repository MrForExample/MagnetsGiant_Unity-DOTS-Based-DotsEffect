using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// This event will let you pause 
	///
	/// Example : MMSoundManagerSoundFadeEvent.Trigger(33, 2f, 0.3f, new MMTweenType(MMTween.MMTweenCurve.EaseInElastic));
	/// will fade the sound with an ID of 33 towards a volume of 0.3, over 2 seconds, on an elastic curve
	/// </summary>
	public struct MMSoundManagerSoundFadeEvent
	{
		/// the ID of the sound to fade
		public int SoundID;
		/// the duration of the fade (in seconds)
		public float FadeDuration;
		/// the volume towards which to fade this sound
		public float FinalVolume;
		/// the tween over which to fade this sound
		public MMTweenType FadeTween;
        
		public MMSoundManagerSoundFadeEvent(int soundID, float fadeDuration, float finalVolume, MMTweenType fadeTween)
		{
			SoundID = soundID;
			FadeDuration = fadeDuration;
			FinalVolume = finalVolume;
			FadeTween = fadeTween;
		}

		static MMSoundManagerSoundFadeEvent e;
		public static void Trigger(int soundID, float fadeDuration, float finalVolume, MMTweenType fadeTween)
		{
			e.SoundID = soundID;
			e.FadeDuration = fadeDuration;
			e.FinalVolume = finalVolume;
			e.FadeTween = fadeTween;
			MMEventManager.TriggerEvent(e);
		}
	}
}