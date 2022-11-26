using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feel
{
	/// <summary>
	/// A class used in Feel's Bounce demo scene to push a bunch of tiny cubes in the air
	/// </summary>
	public class BounceRocks : MonoBehaviour
	{
		public List<Rigidbody> Rocks;
		public Vector3 MinForce;
		public Vector3 MaxForce;
		public Vector3 MinTorque;
		public Vector3 MaxTorque;

		protected Vector3 _force;
		protected Vector3 _torque;

		public virtual void Bounce()
		{
			foreach(Rigidbody rock in Rocks)
			{
				_force = MMMaths.RandomVector3(MinForce, MaxForce);
				_torque = MMMaths.RandomVector3(MinTorque, MaxTorque);
				rock.AddForce(_force, ForceMode.Impulse);
				rock.AddTorque(_torque, ForceMode.Impulse);
			}
		}
	}    
}