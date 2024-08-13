using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Sperlich.PrefabManager.AutoLoader;

namespace Sperlich.PrefabManager {
	public static class PrefabManager {

		internal static Transform container;
		internal static PoolGameObject helper;
		internal static List<PoolPrefab> prefabs = new();
		private static List<Pool> pools = new();

		private static Scene _mainScene = default;
		public static bool EnableAutoInit { get; set; } = true;
		public static bool MovePoolObjectToMainScene { get; set; } = false;
		public static string DefaultSceneSpawn { get; set; }
		public static Scene MainScene {
			get {
				if (_mainScene == default) {
					for (int i = 0; i < SceneManager.sceneCount; i++) {
						Scene s = SceneManager.GetSceneAt(i);
						
						if (s.name == DefaultSceneSpawn) {
							_mainScene = s;
							break;
						}
					}
				}

				return _mainScene;
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void AutoInit() {
			if(EnableAutoInit) {
				AutoLoader.Initialize();
				Initialize();
			}
		}
		public static void Initialize() => Initialize(AutoLoader.Prefabs.Values.ToArray(), null);
		public static void Initialize(string defaultScene) => Initialize(AutoLoader.Prefabs.Values.ToArray(), defaultScene);
		public static void Initialize(PrefabInfo[] prefabs, string defaultScene) {
			ResetPrefabManager();
			container = new GameObject("PrefabManager").transform;
			helper = new GameObject("Prefabmanager Helper").AddComponent<PoolGameObject>();
			UnityEngine.Object.DontDestroyOnLoad(container.gameObject);
			UnityEngine.Object.DontDestroyOnLoad(helper.gameObject);
			helper.gameObject.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.HideAndDontSave;
			DefaultSceneSpawn = defaultScene == string.Empty ? string.Empty : defaultScene;
			PrefabManager.prefabs = new();

			foreach(PrefabInfo p in prefabs) {
				if(p.attribute is PoolPrefabAttribute poolPrefabAttr) {
					PoolPrefab info = new PoolPrefab() {
						name = ToCleanString(p.Name),
						prefab = p.prefab,
						type = GetPrefabTypeFromName(p.Name),
						preloadAmount = poolPrefabAttr.preloadAmount
					};

					PrefabManager.prefabs.Add(info);
				}
			}
			CreatePools();
			
			Debug.Log("Prefabmanager intialized.");
		}

		static void CreatePools() {
			pools = new List<Pool>();

			foreach (var info in prefabs) {
				var pool = new Pool(info);
				pools.Add(pool);
			}
		}

		#region API
		/// <summary>
		/// Must be called to free a GameObject so that it can be recycled.
		/// </summary>
		/// <param name="gameobject"></param>
		/// <param name="delay"></param>
		public static void Free(IRecycle rec, float delay = 0f) {
			try {
				Pool pool = GetPool(rec);
				Pool.PoolObject poolObject = pool.pooledObjects[rec];

				if (poolObject.isBeingFreed) {
					return;
				}
				if (delay == 0) {
					poolObject.isBeingFreed = false;
					pool.FreeGameObject(rec);
				} else {
					poolObject.isBeingFreed = true;
					helper.StartCoroutine(IDelay());

					IEnumerator IDelay() {
						yield return new WaitForSeconds(delay);
						pool.FreeGameObject(rec);
						poolObject.isBeingFreed = false;
					}
				}
			} catch (Exception e) {
				Debug.LogError($"PrefabManager failed to free the GameObject {rec} \n" + e.StackTrace);
			}
		}
		/// <summary>
		/// Call this to reset and delete all gameobjects that have been spawned with the manager.
		/// </summary>
		public static void ResetPrefabManager() {
			if(pools != null) {
				foreach(Pool pool in pools) {
					foreach(var val in pool.pooledObjects) {
						val.Key.Recycle();
					}
				}
			}

			pools.Clear();

			if (container != null) {
				UnityEngine.Object.Destroy(container.gameObject);
			}
			if(helper != null) {
				UnityEngine.Object.Destroy(helper.gameObject);
			}
		}
		#endregion

		#region Instance-API
		/// <summary>
		/// Instantiates and returns the GameObject.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="parent"></param>
		/// <param name="position"></param>
		/// <param name="rotation"></param>
		/// <returns></returns>
		public static GameObject Instantiate(Prefabs type, Transform parent, Vector3 position = new Vector3(), Quaternion rotation = new Quaternion()) {
			try {
				GameObject o = UnityEngine.Object.Instantiate(GetPrefabData(type).prefab, position, rotation, parent);
				o.SetActive(true);
				return o;
			} catch (Exception e) {
				Debug.LogError($"PrefabManager failed to instantiate [{type}]. \n" + e.StackTrace);
				return null;
            }
		}
		/// <summary>
		/// Instantiates and returns the GameObject.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="parent"></param>
		/// <param name="position"></param>
		/// <param name="rotation"></param>
		/// <returns></returns>
		public static GameObject Instantiate(Prefabs type) => Instantiate(type, null);
		/// <summary>
		/// Instantiates and returns the given Component.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="parent"></param>
		/// <param name="position"></param>
		/// <param name="rotation"></param>
		/// <returns></returns>
		public static T Instantiate<T>(Prefabs type) => Instantiate<T>(type, null);
		/// <summary>
		/// Instantiates and returns the given Component.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="parent"></param>
		/// <param name="position"></param>
		/// <param name="rotation"></param>
		/// <returns></returns>
		public static T Instantiate<T>(Prefabs type, Transform parent, Vector3 position = new Vector3(), Quaternion rotation = new Quaternion()) {
			GameObject instance = Instantiate(type, parent, position, rotation);
			if(instance == null) {
				return default;
			}
			if(instance.scene.name != DefaultSceneSpawn && SceneManager.GetSceneByName(DefaultSceneSpawn).IsValid()) {
				SceneManager.MoveGameObjectToScene(instance, SceneManager.GetSceneByName(DefaultSceneSpawn));
			}
			return instance.GetComponent<T>();
		}
		public static T Instantiate<T>(GameObject @object, Transform parent = null, Vector3 position = new Vector3(), Quaternion rotation = new Quaternion()) {
			GameObject instance = UnityEngine.Object.Instantiate(@object, position, rotation, parent);
			if (instance.TryGetComponent(out T component) == false) {
				UnityEngine.Object.DestroyImmediate(instance);
				UnityEngine.Debug.LogError($"Failed to instantiate GameObject. Component {typeof(T).Name} could not be found.");
			}
			if (instance.scene.name != DefaultSceneSpawn && SceneManager.GetSceneByName(DefaultSceneSpawn).IsValid()) {
				SceneManager.MoveGameObjectToScene(instance, SceneManager.GetSceneByName(DefaultSceneSpawn));
			}
			return component;
		}

		public static GameObject Spawn(Prefabs type, Vector3 position = new Vector3(), Quaternion rotation = new Quaternion()) => Spawn(type, null, position, rotation);
		/// <summary>
		/// Returns the required GameObject without creating a new one. If no stored GameObject is available though, a new one will be generated and stored for reuse.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="parent"></param>
		/// <param name="position"></param>
		/// <param name="rotation"></param>
		/// <returns></returns>
		public static GameObject Spawn(Prefabs type) => Spawn(type, null);
		/// <summary>
		/// Returns the GameObject with the required Component without creating a new one. If no stored GameObject is available though, a new one will be generated and stored for reuse.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="parent"></param>
		/// <param name="position"></param>
		/// <param name="rotation"></param>
		/// <returns></returns>
		public static T Spawn<T>(Prefabs type) => Spawn<T>(type, null);
		public static T Spawn<T>(Prefabs type, Vector3 position = new Vector3(), Quaternion rotation = new Quaternion()) => Spawn<T>(type, null, position, rotation);
		/// <summary>
		/// Returns the GameObject with the required Component without creating a new one. If no stored GameObject is available though, a new one will be generated and stored for reuse.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="parent"></param>
		/// <param name="position"></param>
		/// <param name="rotation"></param>
		/// <returns></returns>
		public static T Spawn<T>(Prefabs type, Transform parent, Vector3 position = new Vector3(), Quaternion rotation = new Quaternion()) {
			return Spawn(type, parent, position, rotation).GetComponent<T>();
		}
		/// <summary>
		/// Returns the required GameObject without creating a new one. If no stored GameObject is available though, a new one will be generated and stored for reuse.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="parent"></param>
		/// <param name="position"></param>
		/// <param name="rotation"></param>
		/// <returns></returns>
		public static GameObject Spawn(Prefabs type, Transform parent, Vector3 position = new Vector3(), Quaternion rotation = new Quaternion()) {
			try {
				Pool pool = GetPool(type);
				Pool.PoolObject poolObject = pool.FetchFreePoolObject();
				GameObject obj = poolObject.storedObject;

				if (parent != null) {
					obj.transform.SetParent(parent);
				} else if(MovePoolObjectToMainScene && MainScene != default) {
					obj.transform.SetParent(null);
					SceneManager.MoveGameObjectToScene(obj, MainScene);
				} else if(MovePoolObjectToMainScene) {
					obj.transform.SetParent(null);
				}

				obj.transform.SetPositionAndRotation(position, rotation);
				obj.SetActive(true);

				return poolObject.storedObject;
			} catch (Exception e) {
				Debug.LogError($"PrefabManager failed to spawn {type}. \n" + e.Message);
				return null;
			}
		}
		#endregion

		internal static Prefabs GetPrefabTypeFromName(string name) {
			name = ToCleanString(name);

			foreach(var e in Enum.GetValues(typeof(Prefabs))) {
				Prefabs prefab = (Prefabs)e;
				string compareName = ToCleanString(prefab.ToString());
				
				if (name == compareName) {
					return prefab;
				}
			}
			
			throw new KeyNotFoundException($"Failed to find the corresponding PrefabType for {name}");
		}
		internal static bool ContainsPrefab(string name) {
			name = ToCleanString(name);

			foreach(var e in Enum.GetNames(typeof(Prefabs))) {
				if(ToCleanString(e) == name) {
					return true;
				}
			}
			return false;
		}
		internal static PoolPrefab GetPrefabData(Enum e) {
			return prefabs.Where(p => p.type.ToString() == e.ToString()).First();
		}
		internal static PoolPrefab GetPrefabData(string prefabType) {
			PoolPrefab info = prefabs.Where(p => ToCleanString(p.prefab.name) == ToCleanString(prefabType)).FirstOrDefault();
			if (info == null) {
				Debug.LogError($"No PrefabInfo found for {prefabType}.");
				//throw new NullReferenceException();
			}
			if (info.prefab == null) {
				Debug.LogError($"Prefab not set for {prefabType}.");
				//throw new NullReferenceException();
			}
			return info;
		}
		internal static Pool GetPool(IRecycle rec) {
			foreach(var pool in pools) {
				if (pool.pooledObjects.Keys.Contains(rec)) {
					return pool;
				}
			}

			throw new NullReferenceException("Failed to retrieve the pool. Perhaps you tried to free a non-pool gameobject?");
		}
		internal static Pool GetPool(PoolPrefab info) => GetPool(info.name);
		internal static Pool GetPool(Enum en) => GetPool(en.ToString());
		internal static Pool GetPool(string prefabType) {
			return pools.Where(p => ToCleanString(p.prefabInfo.prefab.name) == ToCleanString(prefabType)).First();
		}
		internal static string ToCleanString(string input) {
			// Remove all non-alphanumeric characters
			input = Regex.Replace(input, @"[^a-zA-Z0-9]", "");

			// Remove leading digits
			input = Regex.Replace(input, @"^\d+", "");

			// If the string is empty after cleaning, return "enumvalue" (or another default value)
			if (string.IsNullOrEmpty(input)) {
				return "none";
			}

			// Convert to lowercase
			input = input.ToLower();

			return input;
		}
	}
}