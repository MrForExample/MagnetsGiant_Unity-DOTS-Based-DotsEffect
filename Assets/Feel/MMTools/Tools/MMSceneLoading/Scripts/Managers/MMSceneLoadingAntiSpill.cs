using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{
	/// <summary>
	/// This helper class, meant to be used by the MMAdditiveSceneLoadingManager, creates a temporary scene to store objects that might get instantiated, and empties it in the destination scene once loading is complete
	/// </summary>
	public class MMSceneLoadingAntiSpill
	{
		protected Scene _antiSpillScene;
		protected Scene _destinationScene;
		protected UnityAction<Scene, Scene> _onActiveSceneChangedCallback;
		protected string _sceneToLoadName;
		protected List<GameObject> _spillSceneRoots = new List<GameObject>(50);
		
		/// <summary>
		/// Creates the temporary scene
		/// </summary>
		/// <param name="sceneToLoadName"></param>
		public virtual void PrepareAntiFill(string sceneToLoadName)
		{
			_antiSpillScene = SceneManager.CreateScene($"AntiSpill_{sceneToLoadName}");
			_destinationScene = default; 
			_sceneToLoadName = sceneToLoadName;

			if (_onActiveSceneChangedCallback != null) { SceneManager.activeSceneChanged -= _onActiveSceneChangedCallback; }
			_onActiveSceneChangedCallback = OnActiveSceneChanged;
			SceneManager.activeSceneChanged += _onActiveSceneChangedCallback;
			SceneManager.SetActiveScene(_antiSpillScene);
		}
		
		/// <summary>
		/// Once the destination scene has been loaded, we catch that event and prepare to empty
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		protected virtual void OnActiveSceneChanged(Scene from, Scene to)
		{
			if (from == _antiSpillScene)
			{
				SceneManager.activeSceneChanged -= _onActiveSceneChangedCallback;
				_onActiveSceneChangedCallback = null;
				
				EmptyAntiSpillScene();
			}
		}

		/// <summary>
		/// Empties the contents of the anti spill scene into the destination scene
		/// </summary>
		protected virtual void EmptyAntiSpillScene()
		{
			if (_antiSpillScene.IsValid() && _antiSpillScene.isLoaded)
			{
				_spillSceneRoots.Clear();
				_antiSpillScene.GetRootGameObjects(_spillSceneRoots);

				_destinationScene = SceneManager.GetSceneByName(_sceneToLoadName);
				
				if (_spillSceneRoots.Count > 0)
				{
					if (_destinationScene.IsValid() && _destinationScene.isLoaded)
					{
						foreach (var root in _spillSceneRoots)
						{
							SceneManager.MoveGameObjectToScene(root, _destinationScene);
						}
					}
				}

				SceneManager.UnloadSceneAsync(_antiSpillScene);
			}
		}
	}
}