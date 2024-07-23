using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Sperlich.PrefabManager {
	public static class AutoLoader {

		public static Dictionary<string, PrefabInfo> Prefabs { get; private set; }
		public static Dictionary<Type, PrefabInfo> PrefabsType { get; private set; }
		public static List<MonoBehaviour> Singletons { get; private set; }
		private static bool initialized;
		private static MD5 MD5 = new MD5CryptoServiceProvider();

		public static void Initialize() {
			if (initialized == false) {
				UnityEngine.Debug.Log("<color=red> Initializing AutoLoader </color>");
				initialized = true;
				Prefabs = new();
				PrefabsType = new();
				Singletons = new();

				// Resources.LoadAll<Mono> returns only one MonoBehaviour from a GameObject
				// Others are ignored and wont be in the list.
				foreach (MonoBehaviour main in Resources.LoadAll("", typeof(MonoBehaviour))) {
					PrefabManagerAttribute attr = null;
					MonoBehaviour mono = null;
					// Therefore the other attached Scripts also need to be searched
					// But only search the most upper Parent for Scripts
					// Search is stopped as soon an Attribute has been found.
					foreach (MonoBehaviour search in main.transform.root.GetComponents<MonoBehaviour>()) {
						if (search != null) {
							attr = search.GetType().GetCustomAttribute(typeof(PrefabManagerAttribute), true) as PrefabManagerAttribute;
							if (attr != null) {
								mono = search;
								break;
							}
						}
					}
					
					if (mono != null) {
						if (attr is PrefabAttribute) {
							var info = new PrefabInfo(mono.gameObject, attr as PrefabAttribute);

							Prefabs.Add(GetDefinitionName(mono.name), info);

							if(PrefabsType.ContainsKey(mono.GetType()) == false) {
								PrefabsType[mono.GetType()] = info;
							} else {
								PrefabsType[mono.GetType()] = null;
							}

							//UnityEngine.Debug.Log($"Loaded: {mono.name}");
						} else if (attr is SingletonPrefabAttribute) {
							Singletons.Add(mono);
							SingletonPrefabAttribute sAttr = attr as SingletonPrefabAttribute;
							//UnityEngine.Debug.Log($"Loaded SINGLETON: {mono.name}");
							if (sAttr.autoInstance) {
								MonoBehaviour existingInstance = UnityEngine.Object.FindObjectOfType(mono.GetType()) as MonoBehaviour;
								if (existingInstance != null) {
									UnityEngine.Object.Destroy(existingInstance.gameObject);
								}

								GameObject instance = UnityEngine.Object.Instantiate(mono.gameObject, null);
								if (sAttr.dontDestroy) {
									UnityEngine.Object.DontDestroyOnLoad(instance);
								}
							}
						}
					} else {
						//UnityEngine.Debug.Log($"-- {main.name} has no attribute");
					}
				}

				UnityEngine.Debug.Log($"-- Prefabs loaded: {Prefabs.Count}");
				UnityEngine.Debug.Log($"-- Singletons loaded: {Singletons.Count}");
			}
		}
		public static T GetInstance<T>() where T : MonoBehaviour {
			Initialize();
			GameObject prefab = PrefabsType[typeof(T)]?.prefab;

			if(prefab == null) {
				throw new NullReferenceException($"Prefab of Type [{typeof(T).Name}] not found.");
			} else {
				T comp = UnityEngine.Object.Instantiate(prefab, null).GetComponent<T>();
				return comp;
			}
		}
		public static T GetInstance<T>(string gameObjectName) where T : MonoBehaviour {
			Initialize();
			GameObject prefab = Prefabs[GetDefinitionName(gameObjectName)].prefab;

			if (prefab == null) {
				throw new NullReferenceException($"Prefab [{GetDefinitionName(gameObjectName)}] not found.");
			} else {
				T comp = UnityEngine.Object.Instantiate(prefab, null).GetComponent<T>();
				return comp;
			}
		}
		public static T GetSingleInstance<T>() where T : MonoBehaviour {
			T existingInstance = UnityEngine.Object.FindObjectOfType<T>();
			if (existingInstance != null) {
				UnityEngine.Object.Destroy(existingInstance.gameObject);
			}
			// Check if there is a Singleton-Prefab for this Type
			MonoBehaviour prefab = null;
			foreach (MonoBehaviour p in Singletons) {
				if(p is T) {
					prefab = p;
					break;
				}
			}

			// If there is no Prefab found for this Singleton, instantiate a new GameObject with the attached Script
			if(prefab == null) {
				Type type = typeof(T);
				SingletonPrefabAttribute attr = type.GetCustomAttribute<SingletonPrefabAttribute>(true);
				if (attr.emptyInstanceIfMissing) {
					MonoBehaviour instance = new GameObject(typeof(T).Name).AddComponent<T>();
					instance.name = type.Name;
					if (type.GetCustomAttribute<SingletonPrefabAttribute>(true).dontDestroy) {
						UnityEngine.Object.DontDestroyOnLoad(instance.gameObject);
					}
					return (T)instance;
				}
				return null;
			} else {
				GameObject instance = UnityEngine.Object.Instantiate(prefab.gameObject, null);
				Type type = typeof(T);
				SingletonPrefabAttribute attr = type.GetCustomAttribute<SingletonPrefabAttribute>(true);
				instance.name = type.Name;

				if(attr.dontDestroy) {
					UnityEngine.Object.DontDestroyOnLoad(instance);
				}
				return instance.GetComponent<T>();
			}
		}
		public static string GetMD5Hash(string text) {
			if (string.IsNullOrEmpty(text)) {
				return string.Empty;
			}
			
			byte[] result = MD5.ComputeHash(Encoding.Default.GetBytes(text));
			return System.BitConverter.ToString(result);
		}
		public static string GetDefinitionName<T>() {
			string name = typeof(T).Name;
			return name.ToLower().Trim();
		}
		public static string GetDefinitionName(string name) {
			return name.ToLower().Trim();
		}

		[System.Serializable]
		public class PrefabInfo {
			public GameObject prefab;
			public PrefabAttribute attribute;
			public int preloadAmount;

			public string Name { get; private set; }
			public string HashCode { get; private set; }

			public PrefabInfo(GameObject @object, PrefabAttribute attribute) {
				prefab = @object;
				this.attribute = attribute;
				this.Name = @object.name.ToLower();
				HashCode = GetMD5Hash(Name);
			}

			public override bool Equals(object obj) {
				return obj is PrefabInfo other && HashCode == other.HashCode;
			}
			public override int GetHashCode() => HashCode.GetHashCode();
			public static bool operator ==(PrefabInfo left, PrefabInfo right) {
				return Equals(left, right);
			}
			public static bool operator !=(PrefabInfo left, PrefabInfo right) {
				return !Equals(left, right);
			}
		}
	}
}