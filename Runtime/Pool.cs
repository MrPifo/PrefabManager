using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sperlich.PrefabManager {
	public class Pool {

		internal readonly Transform transform;
		internal readonly PoolPrefab prefabInfo;
		internal readonly Dictionary<IRecycle, PoolObject> pooledObjects;
		private readonly HashSet<IRecycle> usedObjects;
		private readonly HashSet<IRecycle> freeObjects;
		public int ObjectsInUse => usedObjects.Count;
		public int ObjectsFree => freeObjects.Count;

		public Pool(PoolPrefab info) {
			transform = new GameObject(info.name).transform;
			transform.SetParent(PrefabManager.container);
			prefabInfo = info;
			pooledObjects = new();
			usedObjects = new();
			freeObjects = new();

			for(int i = 0; i < prefabInfo.preloadAmount; i++) {
				var instance = GetNewInstance();
				freeObjects.Add(instance.interfaceRef);
			}
		}

		internal PoolObject FetchFreePoolObject() {
			PoolObject po;

			if(freeObjects.Count > 0) {
				po = pooledObjects[freeObjects.First()];
				freeObjects.Remove(po.interfaceRef);
				
			} else {
				po = GetNewInstance();
				pooledObjects.Add(po.interfaceRef, po);
			}

			usedObjects.Add(po.interfaceRef);
			po.storedObject.name = "Used";
			po.storedObject.SetActive(true);

			return po;
		}
		PoolObject GetNewInstance() {
			GameObject obj = Object.Instantiate(prefabInfo.prefab, transform);
			var instance = new PoolObject(obj);
			obj.SetActive(false);

			return instance;
		}

		public void FreeGameObject(IRecycle recycleInterface) {
			PoolObject poolObject = pooledObjects[recycleInterface];
			GameObject obj = poolObject.storedObject;
			usedObjects.Remove(recycleInterface);

			if (obj != null) {
				freeObjects.Add(recycleInterface);
				obj.name = "Free";
				obj.SetActive(false);
				if (obj.transform.parent != transform) {
					obj.transform.SetParent(transform);
				}
			}
			// Case if the object got accidentally destroyed
			else {
				pooledObjects.Remove(recycleInterface);
				Debug.LogWarning("Tried to free a pooled GameObject that got previously destroyed. This should be avoided.");
			}
		}

		internal class PoolObject {

			internal bool isBeingFreed;
			internal IRecycle interfaceRef;
			internal GameObject storedObject;

			internal PoolObject(GameObject o) {
				if (o.TryGetComponent(out interfaceRef) == false) {
					throw new System.NotImplementedException("Interface IRecycle not implemented!");
				}

				this.storedObject = o;
			}
		}
	}
}