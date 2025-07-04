using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CharlieMadeAThing.NeatoTags.Core.Editor {
    /// <summary>
    ///     Tracks all <see cref="Tagger" />s in the scene @ editor time.
    ///     Allows for selecting all <see cref="Tagger" />s that have a specific <see cref="NeatoTag" />.
    ///     This needs to be part of Core asmdef, but it's just necessary in the editor.
    ///     Is there a better way? Let me know.
    /// </summary>
    public static class NeatoTagTaggerTracker {
#if UNITY_EDITOR
        static readonly HashSet<Tagger> s_taggers = new();

        public static void RegisterTagger( Tagger tagger ) {
            CleanUpNulls();
            if ( tagger == null ) {
                Debug.LogWarning( "[NeatoTagTaggerTracker]: Attempting to register a null tagger!" );
                return;
            }

            s_taggers.Add( tagger );
        }

        public static void UnregisterTagger( Tagger tagger ) {
            CleanUpNulls();
            if ( tagger == null ) {
                Debug.LogWarning( "[NeatoTagTaggerTracker]: Attempting to unregister a null tagger!" );
                return;
            }

            s_taggers.Remove( tagger );
        }

        public static void RegisterTaggersInScene() {
            s_taggers.Clear();
            var taggers = GetAllObjectsOnlyInScene();
            foreach ( var tagger in taggers ) {
                RegisterTagger( tagger );
            }
        }

        public static void SelectAllGameObjectsWithTaggerThatHasTag( NeatoTag tag ) {
            if ( tag == null ) {
                Debug.LogWarning( "[NeatoTagTaggerTracker]: Attempting to select all gameobjects with a null tag!" );
                return;
            }

            Selection.objects = s_taggers
                .Where( tagger => tagger.HasTag( tag ) )
                .Select( tagger => tagger.gameObject as Object )
                .ToArray();
        }

        static void CleanUpNulls() {
            s_taggers.RemoveWhere( tagger => tagger == null );
        }

        static IEnumerable<Tagger> GetAllObjectsOnlyInScene() {
            var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

            return rootObjects.SelectMany( rootObject => rootObject.GetComponentsInChildren<Tagger>() );
        }

#endif
    }
}