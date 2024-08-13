using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sperlich.PrefabManager {
    public static class PrefabManagerExt {

		/// <summary>
		/// <para>Trigger this function to free this Pool-Object and return it back to the pool.</para>
		/// <para>This is the same as calling PrefabManager.Free(this)</para>
		/// </summary>
		/// <param name="delay">Delay's the freeing process.</param>
		public static void Free(this IRecycle rec, float delay = 0) {
            PrefabManager.Free(rec, delay);
        }

    }
}