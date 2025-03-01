using System.Collections.Generic;
using UnityEditor;

namespace CharlieMadeAThing.NeatoTags.Core.Editor {
    /// <summary>
    ///     Keeps track of editor scripts (drawer scripts) which allows updating Tagger inspectors when tags are added/removed
    ///     from the project.
    /// </summary>
    public class NeatoTagAssetModificationProcessor : AssetPostprocessor {
        static readonly List<TaggerDrawer> TaggerDrawers = new();
        static readonly List<NeatoTagDrawer> NeatoTagDrawers = new();


        //When an asset is created, deleted, or renamed, this method is called.
        //Sadly I can't specify a type of asset to watch for, so I have to watch for all of them.
        //This shouldn't be too slow since it's only called once even if there are multiple assets being modified.
        static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths ) {
            UpdateTaggers();
            TagAssetCreation.InvalidateTagCache();
        }

        //Loop through all TaggerDrawers and NeatoTagDrawers and update them.
        //Without this custom inspector/windows won't update until the editor needs to redraw them and I find it not as cool unless they update automatically.
        public static void UpdateTaggers() {
            foreach ( var taggerDrawer in TaggerDrawers ) {
                taggerDrawer.PopulateButtons();
            }

            foreach ( var neatoTagDrawer in NeatoTagDrawers ) {
                neatoTagDrawer.UpdateTagButtonText();
            }
        }


        //Grabs the TaggerDrawer. Doesn't work if not a list for some reason...
        public static void RegisterTaggerDrawer( TaggerDrawer taggerDrawer ) {
            if ( TaggerDrawers.Contains( taggerDrawer ) ) {
                return;
            }

            TaggerDrawers.Add( taggerDrawer );
        }

        public static void UnregisterTaggerDrawer( TaggerDrawer taggerDrawer ) {
            TaggerDrawers.Remove( taggerDrawer );
        }

        //Grabs the NeatoTagDrawer. Doesn't work if not a list for some reason...
        public static void RegisterNeatoTagDrawer( NeatoTagDrawer neatoTagDrawer ) {
            if ( NeatoTagDrawers.Contains( neatoTagDrawer ) ) {
                return;
            }

            NeatoTagDrawers.Add( neatoTagDrawer );
        }

        public static void UnregisterNeatoTagDrawer( NeatoTagDrawer neatoTagDrawer ) {
            NeatoTagDrawers.Remove( neatoTagDrawer );
        }
    }
}