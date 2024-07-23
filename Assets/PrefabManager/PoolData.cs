using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BattleTanks.Interfaces;
using UnityEngine;
using static Sperlich.PrefabManager.AutoLoader;

namespace Sperlich.PrefabManager {
	public class PoolData : MonoBehaviour {

		public PrefabInfo prefabInfo;
		public List<PoolObject> pooledObjects;
		public int ObjectsInUse => pooledObjects.Where(p => p.inUsage).Count();
		public int ObjectsFree => pooledObjects.Where(p => p.inUsage == false).Count();

		public void Initialize(PrefabInfo info) {
			prefabInfo = info;
			pooledObjects = new List<PoolObject>();

			for(int i = 0; i < prefabInfo.preloadAmount; i++) {
				prefabInfo.prefab.SetActive(false);
				GameObject p = Instantiate(prefabInfo.prefab, transform);
				pooledObjects.Add(new PoolObject(p, prefabInfo));
			}
		}

		public PoolObject FetchFreePoolObject() {
			PoolObject po;
			if(pooledObjects.Any(p => p.inUsage == false)) {
				po = pooledObjects.Find(p => p.inUsage == false);
			} else {
				GameObject o = Instantiate(prefabInfo.prefab, transform);
				po = new PoolObject(o, prefabInfo);
				pooledObjects.Add(po);
			}
			if(po.storedObject.TryGetComponent(out IRecycle rec)) {
				rec.PoolObject = po;
				po.inUsage = true;
			} else {
				throw new System.NotImplementedException("Interface IRecycle not implemented!");
			}
			po.storedObject.name = "Used";

			return po;
		}

		public void FreeGameObject(PoolObject poolObject) {
			poolObject.inUsage = false;
			poolObject.storedObject.name = "Free";
			poolObject.storedObject.SetActive(false);
			poolObject.storedObject.transform.SetParent(transform);
		}

		public PoolObject FetchFirstUsedGameObject() {
			return pooledObjects.Find(p => p.inUsage);
		}

		public class PoolObject {

			public GameObject storedObject;
			public PrefabInfo type;
			public bool inUsage;

			public PoolObject(GameObject o, PrefabInfo type) {
				storedObject = o;
				this.type = type;
			}
		}
	}
}