using System.Collections.Generic;
using UnityEditor;

namespace CharlieMadeAThing.NeatoTags.Core.Editor {
    public class NeatoTagAssetModificationProcessor : AssetPostprocessor {
        static readonly List<TaggerDrawer> TAGGER_DRAWERS = new();
        static readonly List<NeatoTagDrawer> NEATO_TAG_DRAWERS = new();


        //When an asset is created, deleted, or renamed, this method is called.
        //Sadly I can't specify a type of asset to watch for, so I have to watch for all of them.
        //This shouldn't be too slow since it's only called once even if there are multiple assets being modified.
        static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths ) {
            UpdateTaggers();
        }

        //Loop through all TaggerDrawers and NeatoTagDrawers and update them.
        //Without this custom inspector/windows won't update until the editor needs to redraw them and I find it not as cool unless they update automatically.
        public static void UpdateTaggers() {
            foreach ( var taggerDrawer in TAGGER_DRAWERS ) {
                taggerDrawer.PopulateButtons();
            }

            foreach ( var neatoTagDrawer in NEATO_TAG_DRAWERS ) {
                neatoTagDrawer.UpdateTagButtonText();
            }
        }


        //Grabs the TaggerDrawer. Doesn't work if not a list for some reason...
        public static void RegisterTaggerDrawer( TaggerDrawer taggerDrawer ) {
            if ( TAGGER_DRAWERS.Contains( taggerDrawer ) ) {
                return;
            }

            TAGGER_DRAWERS.Add( taggerDrawer );
        }

        //Grabs the NeatoTagDrawer. Doesn't work if not a list for some reason...
        public static void RegisterNeatoTagDrawer( NeatoTagDrawer neatoTagDrawer ) {
            if ( NEATO_TAG_DRAWERS.Contains( neatoTagDrawer ) ) {
                return;
            }

            NEATO_TAG_DRAWERS.Add( neatoTagDrawer );
        }
    }
}