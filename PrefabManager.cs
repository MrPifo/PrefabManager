using BattleTanks.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Sperlich.PrefabManager.AutoLoader;

namespace Sperlich.PrefabManager {
	[SingletonPrefab(false, true, true)]
	public class PrefabManager : SingleMonoPrefab<PrefabManager> {

		[SerializeField]
		private List<PrefabInfo> prefabs;
		private List<PoolData> Pools;

		public static string DefaultSceneSpawn { get; set; }

		public static void Initialize() => Initialize(AutoLoader.Prefabs.Values.ToArray(), SceneManager.GetActiveScene().name);
		public static void Initialize(string defaultScene) => Initialize(AutoLoader.Prefabs.Values.ToArray(), defaultScene);
		public static void Initialize(PrefabInfo[] prefabs, string defaultScene) {
			Instance = GetSingleInstance<PrefabManager>();
			DefaultSceneSpawn = defaultScene;
			Instance.prefabs = new List<PrefabInfo>(prefabs);
			Instance.CreatePools();

			Debug.Log("Prefabmanager intialized.");
		}

		private void CreatePools() {
			Pools = new List<PoolData>();
			foreach (PrefabInfo info in prefabs) {
				GameObject p = new GameObject(info.prefab.name);
				p.transform.SetParent(transform);
				PoolData pool = p.AddComponent<PoolData>();
				pool.Initialize(info);
				Pools.Add(pool);
			}
		}

		/// <summary>
		/// Call this to reset and delete all gameobjects that have been spawned with the manager.
		/// </summary>
		public static void ResetPrefabManager() {
			Initialize(DefaultSceneSpawn);
			/*if(Pools != null) {
				foreach(PoolData pool in Pools) {
					foreach(PoolData.PoolObject op in pool.pooledObjects) {
						if(Application.isPlaying) {
							Destroy(op.storedObject);
						} else {
							DestroyImmediate(op.storedObject);
						}
					}
				}
				for(int i = 0; i < Pools.Count; i++) {
					if(Pools[i] != null) {
						if(Application.isPlaying) {
							Destroy(Pools[i].gameObject);
						} else {
							DestroyImmediate(Pools[i].gameObject);
						}
					}
				}
			}

			Pools = new List<PoolData>();
			HasBeenInitialized = false;
			SLog.Log(Category.System, "Prefabmanager has been reset.");*/
		}

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
				GameObject o = Instantiate(GetPrefabData(type).prefab, position, rotation, parent);
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
				DestroyImmediate(instance);
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
		public static GameObject Spawn(Prefabs type, Transform parent, Vector3 position = new Vector3(), Quaternion rotation = new Quaternion()) {
			try {
				PoolData pool = GetPool(type);
				PoolData.PoolObject poolObject = pool.FetchFreePoolObject();
				if (parent != null) {
					poolObject.storedObject.transform.SetParent(parent);
				}
				poolObject.storedObject.transform.SetPositionAndRotation(position, rotation);
				poolObject.storedObject.SetActive(true);
				return poolObject.storedObject;
			} catch(Exception e) {
				Debug.LogError($"PrefabManager failed to spawn {type}. \n" + e.StackTrace);
				return null;
			}
        }
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
		/// Must be called to free a GameObject so that it can be recycled.
		/// </summary>
		/// <param name="gameobject"></param>
		/// <param name="delay"></param>
		public static void FreeGameObject(IRecycle gameobject, float delay = 0f) {
			try {
				if(gameobject.PoolObject == null) {
					Debug.LogError($"Error: Tried to recycle a GameObject that has not been spawned with the PrefabManager!" );
					return;
				}
				if (delay == 0) {
					PoolData pool = GetPool(gameobject.PoolObject.type);
					pool.FreeGameObject(gameobject.PoolObject);
				} else {
					Instance.StartCoroutine(IDelay());

					IEnumerator IDelay() {
						float time = 0;

						while(time < delay) {
							time += Time.deltaTime;
							yield return null;
						}

						PoolData pool = GetPool(gameobject.PoolObject.type);
						pool.FreeGameObject(gameobject.PoolObject);
					}
				}
			} catch(Exception e) {
				Debug.LogError($"PrefabManager failed to free the GameObject {gameobject} \n" + e.StackTrace);
			}
		}

		public static bool ContainsPrefab(string name) {
			foreach(var e in Enum.GetNames(typeof(Prefabs))) {
				if(e.ToLower() == name) {
					return true;
				}
			}
			return false;
		}

		public static PrefabInfo GetPrefabData(Enum en) => GetPrefabData(en.ToString());
		public static PrefabInfo GetPrefabData(string prefabType) {
			PrefabInfo info = Instance.prefabs.Where(p => p.prefab.name.ToLower() == prefabType.ToLower()).FirstOrDefault();
			if(info == null) {
				Debug.LogError($"No PrefabInfo found for {prefabType}.");
				//throw new NullReferenceException();
			}
			if(info.prefab == null) {
				Debug.LogError($"Prefab not set for {prefabType}.");
				//throw new NullReferenceException();
			}
			return info;
		}
		public static PoolData GetPool(PrefabInfo info) => GetPool(info.Name);
		public static PoolData GetPool(Enum en) => GetPool(en.ToString());
		public static PoolData GetPool(string prefabType) {
			return Instance.Pools.Where(p => p.prefabInfo.prefab.name.ToLower() == prefabType.ToLower()).FirstOrDefault();
			/*if(pool == null) {
				SLog.Error($"No Pool found for {prefabType}.");
			}
			return pool;*/
		}
	}
}