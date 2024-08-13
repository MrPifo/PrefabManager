using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sperlich.PrefabManager {
	public abstract class SingleMonoPrefab<T> : MonoBehaviour where T : SingleMonoPrefab<T> {

		protected static T _instance;
		public static T Instance {
			get {
				if(_instance == null) {
					_instance = FindObjectOfType<T>();
					if (_instance == null) {
						_instance = AutoLoader.GetSingleInstance<T>();
					}
				}
				return _instance;
			}
			protected set {
				// Destroy existing Instance
				T[] objs = FindObjectsOfType<T>();
				if(objs.Length > 1) {
					foreach(T t in objs) {
						if(t != value) {
							Destroy(t.gameObject);
						}
					}
				}
				_instance = value;
			}
		}

		protected virtual void Awake() {
			Instance = this as T;
		}
		protected virtual void OnApplicationQuit() {
			_instance = null;
		}
	}
}