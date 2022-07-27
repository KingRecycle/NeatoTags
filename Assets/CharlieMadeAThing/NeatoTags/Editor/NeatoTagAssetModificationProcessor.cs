using System.Collections.Generic;
using UnityEditor;

namespace CharlieMadeAThing.NeatoTags.Editor {
    public class NeatoTagAssetModificationProcessor : AssetPostprocessor {
        static readonly List<TaggerDrawer> TAGGER_DRAWERS = new();

        static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths ) {
            //Updates Tagger inspectors when tag assets are added or deleted.
            foreach ( var taggerDrawer in TAGGER_DRAWERS ) {
                taggerDrawer.PopulateButtons();
            }
        }


        public static void RegisterTaggerDrawer( TaggerDrawer taggerDrawer ) {
            if ( TAGGER_DRAWERS.Contains( taggerDrawer ) ) {
                return;
            }

            TAGGER_DRAWERS.Add( taggerDrawer );
        }
    }
}