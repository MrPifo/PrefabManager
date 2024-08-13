using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sperlich.PrefabManager {
    [PoolPrefab]
    public class PrefabExample : MonoBehaviour, IRecycle {

		public void Trigger() {
            Recycle();
        }

		public void Recycle() {
            this.Free(Random.Range(0.5f, 2f));
		}
	}
}