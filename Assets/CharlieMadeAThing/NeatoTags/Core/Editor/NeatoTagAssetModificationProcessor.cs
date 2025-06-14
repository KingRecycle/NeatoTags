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
        //Sadly, I can't specify a type of asset to watch for, so I have to watch for all of them.
        //This shouldn't be too slow since it's only called once even if there are multiple assets being modified.
        static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths ) {
            UpdateTaggers();
            TagAssetCreation.InvalidateTagCache();
        }

        //Loop through all TaggerDrawers and NeatoTagDrawers and update them.
        //Without this custom inspector/windows won't update until the editor needs to redraw them, and I find it not as cool unless they update automatically.
        public static void UpdateTaggers() {
            foreach ( var taggerDrawer in TaggerDrawers ) {
                if ( !taggerDrawer ) continue;
                taggerDrawer.PopulateButtons();
            }

            foreach ( var neatoTagDrawer in NeatoTagDrawers ) {
                if ( !neatoTagDrawer ) continue;
                neatoTagDrawer.UpdateTagButtonText();
            }
        }


        //Grabs the TaggerDrawer. Doesn't work if not a list for some reason...
        public static void RegisterTaggerDrawer( TaggerDrawer taggerDrawer ) {
            //static lists can hold null values, so let's just remove them if any while we are here.
            TaggerDrawers.RemoveAll( drawer => !drawer );
            if ( TaggerDrawers.Contains( taggerDrawer ) ) {
                return;
            }

            TaggerDrawers.Add( taggerDrawer );
        }

        public static void UnregisterTaggerDrawer( TaggerDrawer taggerDrawer ) {
            //static lists can hold null values, so let's just remove them if any while we are here.
            TaggerDrawers.RemoveAll( drawer => !drawer );
            TaggerDrawers.Remove( taggerDrawer );
        }

        //Grabs the NeatoTagDrawer. Doesn't work if not a list for some reason...
        public static void RegisterNeatoTagDrawer( NeatoTagDrawer neatoTagDrawer ) {
            //static lists can hold null values, so let's just remove them if any while we are here.
            NeatoTagDrawers.RemoveAll( drawer => !drawer );
            if ( NeatoTagDrawers.Contains( neatoTagDrawer ) ) {
                return;
            }

            NeatoTagDrawers.Add( neatoTagDrawer );
        }

        public static void UnregisterNeatoTagDrawer( NeatoTagDrawer neatoTagDrawer ) {
            //static lists can hold null values, so let's just remove them if any while we are here.
            NeatoTagDrawers.RemoveAll( drawer => !drawer );
            NeatoTagDrawers.Remove( neatoTagDrawer );
        }
    }
}