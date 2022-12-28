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
            return Tagger.HasTagger( gameObject );
        }

        #region AddRemoveTags

        /// <summary>
        /// Adds a tag to this gameobject.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tag">Tag to add</param>
        public static void AddTag( this GameObject gameObject, NeatoTag tag ) {
            if ( tag == null ) return;
            if ( Tagger.TryGetTagger( gameObject, out var tagger ) ) {
                tagger.AddTag( tag );
            }
        }

        /// <summary>
        /// Removes a tag from this gameobject.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tag">Tag to remove</param>
        public static void RemoveTag( this GameObject gameObject, NeatoTag tag ) {
            if ( tag == null ) return;
            if ( Tagger.TryGetTagger( gameObject, out var tagger ) ) {
                tagger.RemoveTag( tag );
            }
        }

        #endregion

        #region HasTag

        /// <summary>
        /// Returns true if the gameobject is tagged with the given tag.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tag">Tag to check for.</param>
        /// <returns>True if has matching tag, otherwise false.</returns>
        public static bool HasTag( this GameObject gameObject, NeatoTag tag ) {
            return Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.HasTag( tag );
        }

        /// <summary>
        /// Returns true if the gameobject is tagged with the given tag by name.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagName">Tag to check for</param>
        /// <returns>True if has matching tag, otherwise false.</returns>
        public static bool HasTag( this GameObject gameObject, string tagName ) {
            return Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.HasTag( tagName );
        }

        #endregion

        #region HasAnyTagsMatch

        /// <summary>
        /// Returns true if the gameobject is tagged with any of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagParams">params array of tags</param>
        /// <returns>True if any tags match, otherwise false.</returns>
        public static bool HasAnyTagsMatching( this GameObject gameObject, params NeatoTag[] tagParams ) {
            return Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.AnyTagsMatch( tagParams );
        }

        /// <summary>
        /// Returns true if the gameobject is tagged with any of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagNameParams">params array of tag names</param>
        /// <returns>True if any tags match, otherwise false.</returns>
        public static bool HasAnyTagsMatching( this GameObject gameObject, params string[] tagNameParams ) {
            return Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.AnyTagsMatch( tagNameParams );
        }

        /// <summary>
        /// Returns true if the gameobject is tagged with any of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagList">IEnumerable of tags</param>
        /// <returns>True if any tags match, otherwise false.</returns>
        public static bool HasAnyTagsMatching( this GameObject gameObject, IEnumerable<NeatoTag> tagList ) {
            return Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.AnyTagsMatch( tagList );
        }

        /// <summary>
        /// Returns true if the gameobject is tagged with any of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagList">IEnumerable of tag names</param>
        /// <returns>True if any tags match, otherwise false.</returns>
        public static bool HasAnyTagsMatching( this GameObject gameObject, IEnumerable<string> tagList ) {
            return Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.AnyTagsMatch( tagList );
        }

        #endregion

        #region HasAllTagsMatching

        /// <summary>
        /// Returns true if the gameobject is tagged with all of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagParams">params array of tags</param>
        /// <returns>True if all tags match, otherwise false.</returns>
        public static bool HasAllTagsMatching( this GameObject gameObject, params NeatoTag[] tagParams ) {
            return Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.AllTagsMatch( tagParams );
        }

        /// <summary>
        /// Returns true if the gameobject is tagged with all of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagNameParams">params array of tag names</param>
        /// <returns>True if all tags match, otherwise false.</returns>
        public static bool HasAllTagsMatching( this GameObject gameObject, params string[] tagNameParams ) {
            return Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.AllTagsMatch( tagNameParams );
        }

        /// <summary>
        /// Returns true if the gameobject is tagged with all of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagList">IEnumerable of tags</param>
        /// <returns>True if any tags match, otherwise false.</returns>
        public static bool HasAllTagsMatching( this GameObject gameObject, IEnumerable<NeatoTag> tagList ) {
            return Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.AllTagsMatch( tagList );
        }

        /// <summary>
        /// Returns true if the gameobject is tagged with all of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagNameList">IEnumerable of tag names</param>
        /// <returns>True if any tags match, otherwise false.</returns>
        public static bool HasAllTagsMatching( this GameObject gameObject, IEnumerable<string> tagNameList ) {
            return Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.AllTagsMatch( tagNameList );
        }

        #endregion

        #region HasNoTagsMatching

        /// <summary>
        /// Return true if the gameobject is not tagged with any of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagList">params array of tags</param>
        /// <returns>True if none of the tags match, otherwise false.</returns>
        public static bool HasNoTagsMatching( this GameObject gameObject, params NeatoTag[] tagList ) {
            //If there is no tagger, then it is not tagged with any of the given tags.
            return !Tagger.TryGetTagger( gameObject, out var tagger ) || tagger.NoTagsMatch( tagList );
        }

        /// <summary>
        /// Return true if the gameobject is not tagged with any of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagNameList">params array of tag names</param>
        /// <returns>True if none of the tags match, otherwise false.</returns>
        public static bool HasNoTagsMatching( this GameObject gameObject, params string[] tagNameList ) {
            //If there is no tagger, then it is not tagged with any of the given tags.
            return !Tagger.TryGetTagger( gameObject, out var tagger ) || tagger.NoTagsMatch( tagNameList );
        }

        /// <summary>
        /// Return true if the gameobject is not tagged with any of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagList">IEnumerable array of tags</param>
        /// <returns>True if none of the tags match, otherwise false.</returns>
        public static bool HasNoTagsMatching( this GameObject gameObject, IEnumerable<NeatoTag> tagList ) {
            //If there is no tagger, then it is not tagged with any of the given tags.
            return !Tagger.TryGetTagger( gameObject, out var tagger ) || tagger.NoTagsMatch( tagList );
        }

        /// <summary>
        /// Return true if the gameobject is not tagged with any of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagNameList">IEnumerable array of tag names</param>
        /// <returns>True if none of the tags match, otherwise false.</returns>
        public static bool HasNoTagsMatching( this GameObject gameObject, IEnumerable<string> tagNameList ) {
            //If there is no tagger, then it is not tagged with any of the given tags.
            return !Tagger.TryGetTagger( gameObject, out var tagger ) || tagger.NoTagsMatch( tagNameList );
        }

        #endregion

        /// <summary>
        /// Starts a tag filter.
        /// Starts a filter for chaining filter functions.
        /// WithTag(), WithTags(), WithoutTag(), WithoutTags(), WithAnyTags()
        /// To get result call .IsMatch() on the returned filter.
        /// Can be null!
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>TagFilter or null</returns>
        public static Tagger.TagFilter StartTagFilter( this GameObject gameObject ) {
            return Tagger.TryGetTagger( gameObject, out var tagger ) ? tagger.StartFilter() : null;
        }
        
        
    }
}