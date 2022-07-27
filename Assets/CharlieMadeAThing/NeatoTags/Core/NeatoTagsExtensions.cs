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
        /// <param name="tagList">params array of tags</param>
        /// <returns>True if any tags match, otherwise false.</returns>
        public static bool HasAnyTagsMatching( this GameObject gameObject, params NeatoTagAsset[] tagList ) {
            return Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.AnyTagsMatch( tagList );
        }

        /// <summary>
        /// Returns true if the gameobject is tagged with all of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagList">params array of tags</param>
        /// <returns>True if all tags match, otherwise false.</returns>
        public static bool HasAllTagsMatching( this GameObject gameObject, params NeatoTagAsset[] tagList ) {
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
        /// Starts a tag filter.
        /// Starts a filter for chaining filter functions.
        /// WithTag(), WithTags(), WithoutTag(), WithoutTags(), WithAnyTags()
        /// To get result call .IsMatch()
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>Use .IsMatch() to get result</returns>
        public static Tagger.TagFilter TagFilter( this GameObject gameObject ) {
            return Tagger.TryGetTagger( gameObject, out var tagger ) ? tagger.StartFilter() : null;
        }
    }
}