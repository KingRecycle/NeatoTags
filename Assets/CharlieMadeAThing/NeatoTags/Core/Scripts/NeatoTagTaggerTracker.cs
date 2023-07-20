using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CharlieMadeAThing.NeatoTags.Core.Editor {
    /// <summary>
    ///     Tracks all <see cref="Tagger" />s in the scene.
    ///     Allows for selecting all <see cref="Tagger" />s that have a specific <see cref="NeatoTag" />.
    /// </summary>
    public static class NeatoTagTaggerTracker {
#if UNITY_EDITOR
        static readonly HashSet<Tagger> Taggers = new();

        public static void RegisterTagger( Tagger tagger ) {
            CleanUpNulls();
            Taggers.Add( tagger );
        }

        public static void UnregisterTagger( Tagger tagger ) {
            CleanUpNulls();
            Taggers.Remove( tagger );
        }

        public static void RegisterTaggersInScene() {
            Taggers.Clear();
            var taggers = GetAllObjectsOnlyInScene();
            foreach ( var tagger in taggers ) {
                RegisterTagger( tagger );
            }
        }

        public static void SelectAllGameObjectsWithTaggerThatHasTag( NeatoTag tag ) {
            Selection.objects = Taggers
                .Where( tagger => tagger.HasTag( tag ) )
                .Select( tagger => tagger.gameObject as Object )
                .ToArray();
        }

        static void CleanUpNulls() {
            Taggers.RemoveWhere( tagger => tagger == null );
        }

        static IEnumerable<Tagger> GetAllObjectsOnlyInScene() {
            var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

            return rootObjects.SelectMany( rootObject => rootObject.GetComponentsInChildren<Tagger>() );
        }

#endif
    }
}