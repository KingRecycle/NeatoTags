using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Core.Editor {
    /// <summary>
    ///     Keeps track of editor scripts (drawer scripts) which allows updating Tagger inspectors when tags are added/removed
    ///     from the project.
    /// </summary>
    public class NeatoTagAssetModificationProcessor : AssetPostprocessor {
        static readonly List<TaggerDrawer> s_taggerDrawers = new();
        static readonly List<NeatoTagDrawer> s_neatoTagDrawers = new();


        //When an asset is created, deleted, or renamed, this method is called.
        //Sadly, I can't specify a type of asset to watch for, so I have to watch for all of them.
        //This shouldn't be too slow since it's only called once even if there are multiple assets being modified.
        static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths ) {
            UpdateTaggers();
            TagAssetCreation.InvalidateTagCache();
        }

        //Loop through all TaggerDrawers and NeatoTagDrawers and update them.
        //Without this custom inspector/windows won't update until the editor needs to redraw them,
        //and I find it not as cool unless they update automatically.
        public static void UpdateTaggers() {
            foreach ( var taggerDrawer in s_taggerDrawers ) {
                if ( !taggerDrawer ) continue;
                taggerDrawer.PopulateButtons();
            }

            foreach ( var neatoTagDrawer in s_neatoTagDrawers ) {
                if ( !neatoTagDrawer ) continue;
                neatoTagDrawer.UpdateTagButtonText();
            }
        }


        //Grabs the TaggerDrawers that are currently being displayed in the Editor.
        //This is in most cases just 1 because most people just have 1 inspector window open,
        //but it's possible to have multiple inspectors open at once so this can be more than 1.
        public static void RegisterTaggerDrawer( TaggerDrawer taggerDrawer ) {
            //static lists can hold null values, so let's just remove them if any while we are here.
            s_taggerDrawers.RemoveAll( drawer => !drawer );
            if ( s_taggerDrawers.Contains( taggerDrawer ) ) {
                return;
            }

            s_taggerDrawers.Add( taggerDrawer );
        }

        public static void UnregisterTaggerDrawer( TaggerDrawer taggerDrawer ) {
            //static lists can hold null values, so let's just remove them if any while we are here.
            s_taggerDrawers.RemoveAll( drawer => !drawer );
            s_taggerDrawers.Remove( taggerDrawer );
        }

        //Grabs the NeatoTagDrawer that are currently being displayed in the Editor.
        //This is in most cases just 1 because most people just have 1 inspector window open,
        //but it's possible to have multiple inspectors open at once so this can be more than 1.
        public static void RegisterNeatoTagDrawer( NeatoTagDrawer neatoTagDrawer ) {
            //static lists can hold null values, so let's just remove them if any while we are here.
            s_neatoTagDrawers.RemoveAll( drawer => !drawer );
            if ( s_neatoTagDrawers.Contains( neatoTagDrawer ) ) {
                return;
            }

            s_neatoTagDrawers.Add( neatoTagDrawer );
        }

        public static void UnregisterNeatoTagDrawer( NeatoTagDrawer neatoTagDrawer ) {
            //static lists can hold null values, so let's just remove them if any while we are here.
            s_neatoTagDrawers.RemoveAll( drawer => !drawer );
            s_neatoTagDrawers.Remove( neatoTagDrawer );
        }
    }
}