using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sperlich.PrefabManager {
	/// <summary>
	/// Attach this Attribute to any Prefab that should be called via the PrefabManager.
	/// <para></para>
	/// Make sure this Attribute is only attached at the ROOT node ONCE on a single Prefab or it won't be found.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public abstract class PrefabManagerAttribute : Attribute {
        
    }
}