using BattleTanks.Interfaces;
using Sperlich.PrefabManager;
using System.Collections;
using UnityEngine;

namespace Sperlich.PrefabManager {
	/// <summary>
	/// This is a generic Script you can attach to a pooled GameObject
	/// </summary>
	public class PoolGameObject : MonoBehaviour, IRecycle {
		public PoolData.PoolObject PoolObject { get; set; }

		public void Recycle() {
			// TODO:
			PrefabManager.FreeGameObject(this);
		}

		public void Recycle(float delay) {
			StartCoroutine(IDelay());

			IEnumerator IDelay() {
				float time = 0;

				while (time < delay) {
					time += Time.deltaTime;
					yield return null;
				}

				Recycle();
			}
		}
	}
}