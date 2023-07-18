using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Core {
    public static class NeatoTagTaggerTracker {
        #if UNITY_EDITOR
        static readonly HashSet<Tagger> TAGGERS = new();
        
        public static void RegisterTagger( Tagger tagger ) {
            TAGGERS.Add( tagger );
        }

        public static void UnregisterTagger( Tagger tagger ) {
            TAGGERS.Remove( tagger );
        }

        public static void RegisterTaggersInScene() {
            var taggers = GetAllObjectsOnlyInScene();
            foreach ( var tagger in taggers ) {
                RegisterTagger( tagger );
            }
        }

        public static void SelectAllGameObjectsWithTaggerThatHasTag( NeatoTag tag ) {
            Selection.objects = TAGGERS
                .Where( tagger => tagger.HasTag( tag ) )
                .Select( tagger => tagger.gameObject as Object )
                .ToArray();
        }

        static List<Tagger> GetAllObjectsOnlyInScene()
        {
            var objectsInScene = new List<Tagger>();

            foreach (var tagger in (Tagger[]) Resources.FindObjectsOfTypeAll(typeof(Tagger)))
            {
                if (!EditorUtility.IsPersistent(tagger.transform.root.gameObject) && tagger.hideFlags is not (HideFlags.NotEditable or HideFlags.HideAndDontSave))
                    objectsInScene.Add(tagger);
            }

            return objectsInScene;
        }
        
        #endif
    }
}