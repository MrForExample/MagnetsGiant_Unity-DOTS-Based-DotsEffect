using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace MoreMountains.Feel
{    
	/// <summary>
	/// A class used in Feel's Brass demo to generate a ground that reacts to the music made of many cubes
	/// </summary>
	public class FeelBrassGroundGenerator : MonoBehaviour
	{
		[Header("Dimensions")]
		/// the amount of rows of cubes we want to draw 
		public int NumberOfRows = 10;
		/// the amount of columns of cubes we want to draw
		public int NumberOfColumns = 10;
		/// the offset to apply to all cubes
		public Vector3 Offset;
		/// the offset to apply to the dancer's position
		public Vector3 DancerOffset;
		/// the curve on which to remap cube's amplitude
		public AnimationCurve Amplitude;
		/// the width of a cube
		public float Width = 0.5f;
		/// the depth of a cube 
		public float Depth = 0.5f;
		/// the minimum amount by which to multiply the amplitude level
		public float MinRandom = 1f;
		/// the maximum amount by which to multiply the amplitude level
		public float MaxRandom = 2f;
		/// the fixed multiplier to apply to the amplitude level
		public float AmplitudeMultiplier = 2f;
		/// the amount of floating cubes we want
		public int FloatingCubesAmount = 20;

		[Header("Air Cubes")] 
		/// the chance (in percent) of a floating block spawning for every grid cell
		public int FloatingBlockChance = 3;
		/// the minimum height at which floating cubes can be found
		public float MinHeight = 1f;
		/// the maximum height at which floating cubes can be found
		public float MaxHeight = 5f;
		/// the radius around the dancer within which no floating cube should be spawned
		public float MinDistanceToDancer = 2f;
		/// the minimum scale for floating cubes
		public float MinScale = 0.5f;
		/// the maximum scale for floating cubes
		public float MaxScale = 2f;
        
		[Header("Materials")]
		/// the main ground material
		public Material GroundMaterial;
		/// an alt material for the ground, used only for some cubes
		public Material GroundMaterialAlt1;
		/// another alt material for the ground, used only for some cubes
		public Material GroundMaterialAlt2;
        
		[Header("Bindings")]
		/// the prefab to use for the ground cubes
		public MMRadioReceiver GroundPrefabToInstantiate;
		/// the node under which to nest all cubes
		public Transform ParentContainer;
		/// the dancer in the scene
		public Transform Dancer;

		[Header("Behaviour")] 
		/// whether or not to generate the ground on Awake
		public bool GenerateOnAwake = false;
        
		[Header("Debug")] 
		/// a test button to generate the ground
		[MMInspectorButton("GenerateGround")]
		public bool GenerateGroundBtn;

		protected MMRadioReceiver _receiver;
		protected Vector3 _wipPosition;
		protected string _wipName;
		protected int _counter;
        
		/// <summary>
		/// On Awake we generate our ground if needed
		/// </summary>
		protected virtual void Awake()
		{
			if (GenerateOnAwake)
			{
				GenerateGround();    
			}
		}

		/// <summary>
		/// Instantiates cubes to form a ground and randomizes their settings
		/// </summary>
		protected virtual void GenerateGround()
		{
			int counter = 0;
			List<GameObject> list = new List<GameObject>();
			for (int i = 0; i< ParentContainer.transform.childCount; i++)
			{
				list.Add(ParentContainer.transform.GetChild(i).gameObject);
			}
			foreach(GameObject child in list)
			{
				counter++;
				if (Application.isPlaying)
				{
					Destroy(child.gameObject);    
				}
				else
				{
					DestroyImmediate(child.gameObject);
				}
			}

			_counter = 0;
			// we instantiate our ground grid
			for (int i = 0; i < NumberOfRows; i++)
			{
				for (int j = 0; j < NumberOfColumns; j++)
				{
					_wipPosition.x = i * Width;
					_wipPosition.y = 0;
					_wipPosition.z = j * Depth;
					_wipPosition += Offset;
					_wipName = "GroundBlock_" + _counter;
					InstantiateBlock(_wipPosition, _wipName);
					_counter++;
				}
			}

			// we generate some floating cubes too
			for (int i = 0; i < NumberOfRows; i++)
			{
				for (int j = 0; j < NumberOfColumns; j++)
				{
					_wipPosition.x = i * Width;
					_wipPosition.y = Random.Range(MinHeight,MaxHeight);
					_wipPosition.z = j * Depth;
					_wipPosition += Offset;
                    
					if ((MMMaths.Chance(FloatingBlockChance)) && (Vector3.Distance(_wipPosition, Dancer.transform.position) > MinDistanceToDancer))
					{
						_wipName = "AirBlock_" + _counter;
						_receiver = InstantiateBlock(_wipPosition, _wipName);

						_receiver.transform.localScale = _receiver.transform.localScale * Random.Range(MinScale, MaxScale);
						_receiver.MinRandomLevelMultiplier *= 3f;
						_receiver.MaxRandomLevelMultiplier *= 3f;

						MMAutoRotate autoRotate = _receiver.gameObject.AddComponent<MMAutoRotate>();
						autoRotate.RotationSpeed = new Vector3(0f, 100f, 0f);
                        
						_counter++;    
					}
				}
			}
		}

		protected virtual MMRadioReceiver InstantiateBlock(Vector3 newPosition, string newName)
		{
			// instantiating the block and setting its name
			_receiver = Instantiate(GroundPrefabToInstantiate, newPosition, Quaternion.identity,  ParentContainer);
			if (ParentContainer == null)
			{
				SceneManager.MoveGameObjectToScene(_receiver.gameObject, this.gameObject.scene);    
			}
			_receiver.name = newName;

			// setting its receiver settings
			float distanceToDancer = Vector3.Distance(Dancer.transform.position + DancerOffset, newPosition);
			float maxDistance = Mathf.Max(NumberOfColumns * Depth, NumberOfRows * Width) / 2f;
			float remappedDistance = MMMaths.Remap(distanceToDancer, 0f, maxDistance, 0f, 1f);
			float newAmplitude = Amplitude.Evaluate(remappedDistance);
			float random = Random.Range(MinRandom, MaxRandom);
            
			newAmplitude *= random;
			newAmplitude *= AmplitudeMultiplier;

			int channel = Random.Range(0, 2);
            
			_receiver.MinRandomLevelMultiplier = newAmplitude;
			_receiver.MaxRandomLevelMultiplier = newAmplitude;
			_receiver.GenerateRandomLevelMultiplier();
			_receiver.Channel = channel;

			// setting its material
			float randomMaterial = Random.Range(0f, 100f);
			if (randomMaterial < 80f)
			{
				_receiver.GetComponent<MeshRenderer>().material = GroundMaterial;
			}
			else
			{
				if (randomMaterial < 90f)
				{
					_receiver.GetComponent<MeshRenderer>().material = GroundMaterialAlt1;
				}
				else
				{
					_receiver.GetComponent<MeshRenderer>().material = GroundMaterialAlt2;
				}
			}
            
			// setting its position
			_receiver.transform.position = newPosition;

			return _receiver;
		}
	}
}