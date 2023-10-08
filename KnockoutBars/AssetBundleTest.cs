using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace KnockoutBars
{
    public class AssetBundleTest
    {
        public AssetBundle CachedAB { get; private set; }

        public string AssetBundlePath { get; private set; }
        public string AssetBundleIdentifier { get; private set; }

        public List<GameObject> GameObjects { get; private set; }

        public AssetBundleTest(string assetPath, string identifier, bool isEmbeddedPath = false) {
            AssetBundlePath = assetPath;

            CachedAB = isEmbeddedPath ? LoadEmbeddedAssetBundle() : LoadAssetBundle();
            AssetBundleIdentifier = identifier ?? CachedAB.name;
            if (CachedAB is not null) GameObjects = ReturnLoadedObjects(CachedAB);
        }

        public void Destroy(){
            CachedAB.Unload(true);
        }

        private AssetBundle LoadAssetBundle()
        {
            if (AssetBundlePath is null) return null;
            AssetBundle assetBundle = AssetBundle.LoadFromFile(AssetBundlePath);
            return assetBundle;
        }

        private AssetBundle LoadEmbeddedAssetBundle()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream resourceStream = assembly.GetManifestResourceStream("KnockoutBars.AssetBundles." + AssetBundlePath))
            {
                using (MemoryStream memoryStream = new())
                {
                    resourceStream.CopyTo(memoryStream);
                    AssetBundle assetBundle = AssetBundle.LoadFromMemory(memoryStream.ToArray());
                    return assetBundle;
                }
            }
        }

        private List<GameObject> ReturnLoadedObjects(AssetBundle assetBundle){
            List<GameObject> objects = new();

            foreach (Object asset in assetBundle.LoadAllAssets()){
                asset.hideFlags = HideFlags.DontUnloadUnusedAsset;
                if (asset is GameObject) {
                    objects.Add(asset as GameObject);
                }
            };

            return objects;
        }
    }
}
