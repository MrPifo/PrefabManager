using Sperlich.PrefabManager;

namespace Sperlich.PrefabManager {
	public interface IRecycle {

		/// <summary>
		/// Must be called if the GameObject is free to use
		/// </summary>
		public void Recycle();

	}
}
