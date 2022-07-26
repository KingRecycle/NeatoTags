using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CharlieMadeAThing.NeatoTags.Editor {
    public class NeatoTagAssetModificationProcessor : AssetPostprocessor {
        static List<TaggerDrawer> _taggerDrawers = new();

        static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths ) {
            var tagsToAdd = new List<NeatoTagAsset>();
            
            foreach (string str in importedAssets) {
                var tag = AssetDatabase.LoadAssetAtPath<NeatoTagAsset>( str );
                if ( tag != null ) {
                    tagsToAdd.Add( tag );
                }
            }
            
            //Deleted
            // if( deletedAssets.Length > 0 ) {
            //     foreach ( var taggerDrawer in _taggerDrawers ) {
            //         taggerDrawer.CheckAndUpdateRemovedTags();
            //         taggerDrawer.RefreshAllTagButtons();
            //     }
            //     
            // }
            //
            //
            // foreach ( var taggerDrawer in _taggerDrawers ) {
            //     if( tagsToAdd.Count > 0 ) {
            //         foreach ( var neatoTagAsset in tagsToAdd ) {
            //             taggerDrawer.CheckAndUpdateRemovedTags();
            //             taggerDrawer.CreateTagButtonToAllViewer( neatoTagAsset );
            //         }
            //     }
            //     taggerDrawer.RefreshAllTagButtons();
            // }
        }


        public static void RegisterTaggerDrawer( TaggerDrawer taggerDrawer ) {
            if( _taggerDrawers.Contains( taggerDrawer ) ) {
                return;
            }
            _taggerDrawers.Add(taggerDrawer);
        }
    }
}