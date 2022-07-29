using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Editor {
    public class NeatoTagAssetModificationProcessor : AssetPostprocessor {
        static readonly List<TaggerDrawer> TAGGER_DRAWERS = new();
        static readonly  List<NeatoTagDrawer> NEATO_TAG_DRAWERS = new();

        static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths ) {
            //Updates Tagger inspectors when tag assets are added or deleted.
            foreach ( var taggerDrawer in TAGGER_DRAWERS ) {
                taggerDrawer.PopulateButtons();
            }

            
        }

        void OnPreprocessAsset() {
            foreach ( var neatoTagDrawer in NEATO_TAG_DRAWERS ) {
                neatoTagDrawer.OnInspectorGUI();
            }
        }

        public static void RegisterTaggerDrawer( TaggerDrawer taggerDrawer ) {
            if ( TAGGER_DRAWERS.Contains( taggerDrawer ) ) {
                return;
            }

            TAGGER_DRAWERS.Add( taggerDrawer );
        }
        
        public static void RegisterNeatoTagDrawer( NeatoTagDrawer neatoTagDrawer ) {
            if ( NEATO_TAG_DRAWERS.Contains( neatoTagDrawer ) ) {
                return;
            }

            NEATO_TAG_DRAWERS.Add( neatoTagDrawer );
        }
    }
}