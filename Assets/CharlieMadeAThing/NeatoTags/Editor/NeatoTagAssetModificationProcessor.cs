using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CharlieMadeAThing.NeatoTags.Editor {
    public class NeatoTagAssetModificationProcessor : AssetModificationProcessor {
        static List<TaggerDrawer> _taggerDrawers = new();

        static void OnWillCreateAsset(string assetName)
        {
            foreach ( var tagger in Object.FindObjectsOfType<Tagger>() ) {
                tagger.OnValidate();
                //Update TaggerDrawer
            }

            foreach ( var taggerDrawer in _taggerDrawers ) {
                taggerDrawer.UpdateAllTagViewer();
            }
        }

        static string[] OnWillSaveAssets( string[] paths ) {
            Debug.Log("OnWillSaveAssets is being called with the following asset: " + paths + ".");
            foreach ( var tagger in Object.FindObjectsOfType<Tagger>() ) {
                tagger.OnValidate();
                //Update TaggerDrawer
            }
            foreach ( var taggerDrawer in _taggerDrawers ) {
                taggerDrawer.UpdateAllTagViewer();
            }
            return paths;
        }

        public static void RegisterTaggerDrawer( TaggerDrawer taggerDrawer ) {
            _taggerDrawers.Add(taggerDrawer);
        }
    }
}