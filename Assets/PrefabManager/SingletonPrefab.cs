using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sperlich.PrefabManager {
	/// <summary>
	/// Attach this Attribute to any Prefab that should be loaded as a Singleton.
	/// <para>AutoInstance = Automaticially instances this Prefab upon Game-Start</para>
	/// <para>DontDestroy = After auto instancing this Singleton is moved to the DontDestroyOnLoad Scene</para>
	/// </summary>
	public class SingletonPrefabAttribute : PrefabManagerAttribute {

		public bool autoInstance = false;
		public bool dontDestroy = false;
		public bool emptyInstanceIfMissing = false;

		public SingletonPrefabAttribute() {

		}
		public SingletonPrefabAttribute(bool autoInstance) {
			this.autoInstance = autoInstance;
		}
		public SingletonPrefabAttribute(bool autoInstance, bool dontDestroy) {
			this.autoInstance = autoInstance;
			this.dontDestroy = dontDestroy;
			this.emptyInstanceIfMissing = false;
		}
		public SingletonPrefabAttribute(bool autoInstance, bool dontDestroy, bool emptyInstanceIfMissing) {
			this.autoInstance = autoInstance;
			this.dontDestroy = dontDestroy;
			this.emptyInstanceIfMissing = emptyInstanceIfMissing;
		}
	}
}