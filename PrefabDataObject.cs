using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Sperlich.PrefabManager {
	[System.Serializable]
	public class PoolPrefab {

		public string name;
		public int preloadAmount;
		public GameObject prefab;
		public Prefabs type;

	}
}