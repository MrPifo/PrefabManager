using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sperlich.PrefabManager {
	/// <summary>
	/// Attach this Attribute to any Prefab class that should be called via the Prefabmanager.
	/// <para>If no custom name is specified, the name of the class will be taken to search for the Prefab, otherwise a custom name must be specified.</para>
	/// <para>PrefabNames = Specify all Prefab-Variant names here. Every single Prefab that inherits from this class must be listed here (Prefab Filenames).</para>
	/// </summary>
	public class PoolPrefabAttribute : PrefabAttribute {

		public int preloadAmount;

		public PoolPrefabAttribute() { }
		public PoolPrefabAttribute(int preloadAmount) {
			this.preloadAmount = preloadAmount;
		}
	}
}