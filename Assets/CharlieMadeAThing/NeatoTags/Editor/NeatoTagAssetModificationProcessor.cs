using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CharlieMadeAThing.NeatoTags.Editor {
    public class NeatoTagAssetModificationProcessor : AssetModificationProcessor {
        
        static void OnWillCreateAsset(string assetName)
        {
            foreach ( var tagger in Object.FindObjectsOfType<Tagger>() ) {
                tagger.OnValidate();
            }
        }

        static string[] OnWillSaveAssets( string[] paths ) {
            Debug.Log("OnWillSaveAssets is being called with the following asset: " + paths + ".");
            return paths;
        }
    }
}