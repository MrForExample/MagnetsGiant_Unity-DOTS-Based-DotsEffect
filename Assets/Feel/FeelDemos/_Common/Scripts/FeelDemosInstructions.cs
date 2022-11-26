using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace MoreMountains.Feel
{
	/// <summary>
	/// This class handles the instruction texts that appear in the Feel demo scenes
	/// </summary>
	public class FeelDemosInstructions : MonoBehaviour
	{
		[Header("Bindings")]
		/// a text component where we'll display instructions
		public Text TargetText;
		/// the delay, in seconds, before instructions disappear
		public float DisappearDelay = 3f;
		/// the duration, in seconds, of the instructions disappearing transition
		public float DisappearDuration = 0.3f;

		[Header("Texts")]
		/// the text to display when running the demos on desktop
		public string DesktopText = "Press space to...";
		/// the text to display when running the demos on mobile
		public string MobileText = "Tap anywhere to...";

		protected CanvasGroup _canvasGroup;
        
		/// <summary>
		/// On Awake we detect our platform and assign text accordingly
		/// </summary>
		protected virtual void Awake()
		{
			#if UNITY_ANDROID || UNITY_IPHONE
                TargetText.text = MobileText;
			#else
			TargetText.text = DesktopText;
			#endif

			_canvasGroup = this.gameObject.GetComponent<CanvasGroup>();
			StartCoroutine(DisappearCo());
		}

		/// <summary>
		/// A coroutine used to hide the instructions after a while
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator DisappearCo()
		{
			yield return MMCoroutine.WaitFor(DisappearDelay);
			StartCoroutine(MMFade.FadeCanvasGroup(_canvasGroup, DisappearDuration, 0f, true));
			yield return  MMCoroutine.WaitFor(DisappearDuration + 0.1f);
			this.gameObject.SetActive(false);
		}
	}
}