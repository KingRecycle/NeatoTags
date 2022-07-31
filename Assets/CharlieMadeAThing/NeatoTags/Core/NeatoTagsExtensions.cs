using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Core {
    public static class NeatoTagsExtensions {
        /// <summary>
        /// Returns true if the gameobject has a Tagger component.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>bool</returns>
        public static bool IsTagged( this GameObject gameObject ) {
            return Tagger.IsTagged( gameObject );
        }
        
        /// <summary>
        /// Adds a tag to this gameobject.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tag">Tag to add</param>
        public static void AddTag( this GameObject gameObject, NeatoTagAsset tag ) {
            if( Tagger.TryGetTagger( gameObject, out var tagger ) ) {
                tagger.AddTag( tag );
            }
        }
        
        /// <summary>
        /// Removes a tag from this gameobject.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tag">Tag to remove</param>
        public static void RemoveTag( this GameObject gameObject, NeatoTagAsset tag ) {
            if( Tagger.TryGetTagger( gameObject, out var tagger ) ) {
                tagger.RemoveTag( tag );
            }
        }

        /// <summary>
        /// Returns true if the gameobject is tagged with the given tag.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagAsset">params array of tags</param>
        /// <returns>True if has matching tag, otherwise false.</returns>
        public static bool HasTag( this GameObject gameObject, NeatoTagAsset tagAsset ) {
            return Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.HasTag( tagAsset );
        }


        /// <summary>
        /// Returns true if the gameobject is tagged with any of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagParams">params array of tags</param>
        /// <returns>True if any tags match, otherwise false.</returns>
        public static bool HasAnyTagsMatching( this GameObject gameObject, params NeatoTagAsset[] tagParams ) {
            return Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.AnyTagsMatch( tagParams );
        }
        
        /// <summary>
        /// Returns true if the gameobject is tagged with any of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagList">IEnumerable of tags</param>
        /// <returns>True if any tags match, otherwise false.</returns>
        public static bool HasAnyTagsMatching( this GameObject gameObject, IEnumerable<NeatoTagAsset> tagList ) {
            return Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.AnyTagsMatch( tagList );
        }

        /// <summary>
        /// Returns true if the gameobject is tagged with all of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagParams">params array of tags</param>
        /// <returns>True if all tags match, otherwise false.</returns>
        public static bool HasAllTagsMatching( this GameObject gameObject, params NeatoTagAsset[] tagParams ) {
            return Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.AllTagsMatch( tagParams );
        }
        
        /// <summary>
        /// Returns true if the gameobject is tagged with all of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagList">IEnumerable of tags</param>
        /// <returns>True if any tags match, otherwise false.</returns>
        public static bool HasAllTagsMatching( this GameObject gameObject, IEnumerable<NeatoTagAsset> tagList ) {
            return Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.AllTagsMatch( tagList );
        }

        /// <summary>
        /// Return true if the gameobject is not tagged with any of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagList">params array of tags</param>
        /// <returns>bool</returns>
        public static bool HasNoTagsMatching( this GameObject gameObject, params NeatoTagAsset[] tagList ) {
            return Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.NoTagsMatch( tagList );
        }
        
        /// <summary>
        /// Return true if the gameobject is not tagged with any of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagList">IEnumerable array of tags</param>
        /// <returns>bool</returns>
        public static bool HasNoTagsMatching( this GameObject gameObject, IEnumerable<NeatoTagAsset> tagList ) {
            return Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.NoTagsMatch( tagList );
        }

        /// <summary>
        /// Starts a tag filter.
        /// Starts a filter for chaining filter functions.
        /// WithTag(), WithTags(), WithoutTag(), WithoutTags(), WithAnyTags()
        /// To get result call .IsMatch() or GetMatches()
        /// Can be null!
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>TagFilter or null</returns>
        public static Tagger.TagFilter TagFilter( this GameObject gameObject ) {
            
            return Tagger.TryGetTagger( gameObject, out var tagger ) ? tagger.StartFilter() : null;
        }
    }
}