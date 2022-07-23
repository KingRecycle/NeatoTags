using System.Collections.Generic;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags
{
    public static class NeatoTagsExtensions 
    {
        /// <summary>
        /// Checks if the gameobject is able to be tagged.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>bool</returns>
        public static bool IsTaggable(this GameObject gameObject) {
            return gameObject.GetComponent<Tagger>() != null;
        }
        
        /// <summary>
        /// Returns true if the gameobject is tagged
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>bool</returns>
        public static bool IsTagged(this GameObject gameObject) {
            var tagger = gameObject.GetComponent<Tagger>();
            if( tagger == null || tagger.TagCollection == null ) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns true if the gameobject is tagged with the given tag.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tag"></param>
        /// <returns>bool</returns>
        public static bool HasTag( this GameObject gameObject, NeatoTag tag ) {
            var tagger = gameObject.GetComponent<Tagger>();
            if( tagger == null || tagger.TagCollection == null ) {
                return false;
            }
            return tagger.TagCollection.HasTag( tag );
        }
        
        /// <summary>
        /// Returns true if the gameobject is tagged with any of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagList"></param>
        /// <returns>bool</returns>
        public static bool HasAnyTagsMatching( this GameObject gameObject, IEnumerable<NeatoTag> tagList ) {
            var tagger = gameObject.GetComponent<Tagger>();
            if( tagger == null || tagger.TagCollection == null ) {
                return false;
            }
            return tagger.TagCollection.AnyTagsMatch( tagList );
        }
        
        /// <summary>
        /// Returns true if the gameobject is tagged with all of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagList"></param>
        /// <returns>bool</returns>
        public static bool HasAllTagsMatching( this GameObject gameObject, IEnumerable<NeatoTag> tagList ) {
            var tagger = gameObject.GetComponent<Tagger>();
            if( tagger == null || tagger.TagCollection == null ) {
                return false;
            }
            return tagger.TagCollection.AllTagsMatch( tagList );
        }
        
        /// <summary>
        /// Return true if the gameobject is not tagged with any of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagList"></param>
        /// <returns>bool</returns>
        public static bool HasNoTagsMatching( this GameObject gameObject, IEnumerable<NeatoTag> tagList ) {
            var tagger = gameObject.GetComponent<Tagger>();
            if( tagger == null || tagger.TagCollection == null ) {
                return false;
            }
            return tagger.TagCollection.NoTagsMatch( tagList );
        }
    }
}
