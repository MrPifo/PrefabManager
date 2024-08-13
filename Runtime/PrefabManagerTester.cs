using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sperlich.PrefabManager {
	public class PrefabManagerTester : MonoBehaviour {

		public void Awake() {
			StartCoroutine(IRepeat());

			IEnumerator IRepeat() {
				while (true) {
					var obj = PrefabManager.Spawn<PrefabExample>(Prefabs.PrefabExample);
					obj.Trigger();
					
					yield return new WaitForSeconds(Random.Range(0.01f, 0.75f));
				}
			}
		}
	}
}