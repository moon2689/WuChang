using UnityEngine;

namespace PaintIn3D
{
	[System.Serializable]
	public class P3dCanvas
	{
		public RenderTexture Main
		{
			get
			{
				if (main.IsCreated() == false)
				{
					if (main.Create() == true)
					{
						if (cacheSet == true)
						{
							P3dPainter.Replace.Blit(main, cache);

							cache    = P3dHelper.Destroy(cache);
							cacheSet = false;
						}
						else
						{
							UnityEngine.Debug.LogError("Failed to restore " + main, main);
						}
					}
					else
					{
						UnityEngine.Debug.LogError("Failed to create " + main, main);
					}
				}
				else if (cacheSet == true)
				{
					UnityEngine.Debug.LogWarning("Cache exists for no reason? " + main, main);

					cache    = P3dHelper.Destroy(cache);
					cacheSet = false;
				}

				return main;
			}
		}

		[SerializeField]
		private RenderTexture main;

		[SerializeField]
		private Texture2D cache;

		[SerializeField]
		private bool cacheSet;

		public void Store()
		{
			if (cache != null)
			{
				UnityEngine.Debug.LogWarning("Trying to create cache that already exists " + main, main);

				cache = P3dHelper.Destroy(cache);
			}

			cache = P3dHelper.Destroy(cache);
		}

		public void ReleaseAndDestroy()
		{
			if (main != null)
			{
				main.Release();

				main = P3dHelper.Destroy(main);
			}

			cache = P3dHelper.Destroy(cache);
		}
	}
}