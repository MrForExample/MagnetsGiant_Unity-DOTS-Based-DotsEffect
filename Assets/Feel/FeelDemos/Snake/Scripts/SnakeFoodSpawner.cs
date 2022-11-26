using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.Feel
{
	/// <summary>
	/// A simple class used to spawn snake food in Feel's Snake demo scene
	/// </summary>
	public class SnakeFoodSpawner : MonoBehaviour
	{
		/// the food prefab to spawn
		public SnakeFood SnakeFoodPrefab;
		/// the maximum amount of food in the scene
		public int AmountOfFood = 3;
		/// the minimum coordinates to spawn at (in viewport units)
		public Vector2 MinRandom = new Vector2(0.1f, 0.1f);
		/// the maximum coordinates to spawn at (in viewport units)
		public Vector2 MaxRandom = new Vector2(0.9f, 0.9f);
        
		protected List<SnakeFood> Foods;
		protected Camera _mainCamera;

		/// <summary>
		/// On start, instantiates food
		/// </summary>
		protected virtual void Start()
		{
			_mainCamera = Camera.main;
			Foods = new List<SnakeFood>();
			for (int i = 0; i < AmountOfFood; i++)
			{
				SnakeFood food = Instantiate(SnakeFoodPrefab);
				SceneManager.MoveGameObjectToScene(food.gameObject, this.gameObject.scene);
				food.transform.position = DetermineSpawnPosition();
				food.Spawner = this;
				Foods.Add(food);
			}
		}

		/// <summary>
		/// Determines a valid position at which to spawn food
		/// </summary>
		/// <returns></returns>
		public virtual Vector3 DetermineSpawnPosition()
		{
			Vector3 newPosition = MMMaths.RandomVector2(MinRandom, MaxRandom);
			newPosition.z = 10f;
			newPosition = _mainCamera.ViewportToWorldPoint(newPosition);
			return newPosition;
		}
	}
}