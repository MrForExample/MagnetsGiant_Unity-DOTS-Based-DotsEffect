﻿using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Add this class to an object that you expect to pool from an objectPooler. 
	/// Note that these objects can't be destroyed by calling Destroy(), they'll just be set inactive (that's the whole point).
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/Object Pool/MMPoolableObject")]
	public class MMPoolableObject : MMObjectBounds
	{
		[Header("Events")]
		public UnityEvent ExecuteOnEnable;
		public UnityEvent ExecuteOnDisable;
		
		public delegate void Events();
		public event Events OnSpawnComplete;

		[Header("Poolable Object")]
		/// The life time, in seconds, of the object. If set to 0 it'll live forever, if set to any positive value it'll be set inactive after that time.
		public float LifeTime = 0f;

		/// <summary>
		/// Turns the instance inactive, in order to eventually reuse it.
		/// </summary>
		public virtual void Destroy()
		{
			gameObject.SetActive(false);
		}

		/// <summary>
		/// Called every frame
		/// </summary>
		protected virtual void Update()
		{

		}

		/// <summary>
		/// When the objects get enabled (usually after having been pooled from an ObjectPooler, we initiate its death countdown.
		/// </summary>
		protected virtual void OnEnable()
		{
			Size = GetBounds().extents * 2;
			if (LifeTime > 0f)
			{
				Invoke("Destroy", LifeTime);	
			}
			ExecuteOnEnable?.Invoke();
		}

		/// <summary>
		/// When the object gets disabled (maybe it got out of bounds), we cancel its programmed death
		/// </summary>
		protected virtual void OnDisable()
		{
			ExecuteOnDisable?.Invoke();
			CancelInvoke();
		}

		/// <summary>
		/// Triggers the on spawn complete event
		/// </summary>
		public virtual void TriggerOnSpawnComplete()
		{
			OnSpawnComplete?.Invoke();
		}
	}
}