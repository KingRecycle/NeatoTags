using System.Collections.Generic;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Core {
    public static class NeatoTagsExtensions {
        /// <summary>
        ///     Returns true if the gameobject has a Tagger component.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>bool</returns>
        public static bool HasTagger( this GameObject gameObject ) => Tagger.HasTagger( gameObject );

        /// <summary>
        ///     Starts a tag filter.
        ///     Starts a filter for chaining filter functions.
        ///     WithTag(), WithTags(), WithoutTag(), WithoutTags(), WithAnyTags()
        ///     To get result call .IsMatch() on the returned filter.
        ///     Can be null!
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>FilterTags or null</returns>
        public static Tagger.TagFilter FilterTags( this GameObject gameObject ) =>
            Tagger.TryGetTagger( gameObject, out var tagger ) ? tagger.FilterTags() : null;

        /// <summary>
        ///     Gets or creates a tag and adds it to the gameobject.
        ///     Tags created this way will not be saved to the project/game.
        ///     You must implement your own way of saving tags to the project/game.
        ///     Name is trimmed of whitespace.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="newTagName">Name of the tag to get or create.</param>
        /// <returns>Returns tag if successful, otherwise returns new tag with given name.</returns>
        public static NeatoTag GetOrCreateTag( this GameObject gameObject, string newTagName ) {
            if ( Tagger.TryGetTagger( gameObject, out var tagger ) ) {
                return tagger.GetOrCreate( newTagName );
            }
            return null;
        }

        #region AddRemoveTags

        /// <summary>
        ///     Adds a tag to this gameobject.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tag">Tag to add</param>
        public static void AddTag( this GameObject gameObject, NeatoTag tag ) {
            if ( tag == null ) {
                Debug.LogWarning($"Attempting to add tag to {gameObject} but tag argument is null!");
                return;
            }

            if ( Tagger.TryGetTagger( gameObject, out var tagger ) ) {
                tagger.AddTag( tag );
            }
        }
        
        /// <summary>
        ///     Adds all tags from a list to this gameobject's Tagger component.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tags">List of tags to add.</param>
        public static void AddTags( this GameObject gameObject, params NeatoTag[] tags ) {
            if ( tags == null ) {
                Debug.LogWarning($"Attempting to add tags to {gameObject} but tags argument is null!");
                return;
            }

            if ( Tagger.TryGetTagger( gameObject, out var tagger ) ) {
                tagger.AddTags( tags );
            }
        }

        /// <summary>
        ///     Removes a tag from this gameobject.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tag">Tag to remove</param>
        public static void RemoveTag( this GameObject gameObject, NeatoTag tag ) {
            if ( Tagger.TryGetTagger( gameObject, out var tagger ) ) {
                tagger.RemoveTag( tag );
            }
        }
        
        /// <summary>
        ///     Remove a tag by name from this gameobject.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagName"></param>
        public static void RemoveTag( this GameObject gameObject, string tagName ) {
            if ( Tagger.TryGetTagger( gameObject, out var tagger ) ) {
                tagger.RemoveTag( tagName );
            }
        }
        
        /// <summary>
        /// Remove all tags from this gameobject.
        /// </summary>
        /// <param name="gameObject"></param>
        public static void RemoveAllTags( this GameObject gameObject ) {
            if ( Tagger.TryGetTagger( gameObject, out var tagger ) ) {
                tagger.RemoveAllTags();
            }
        }

        #endregion

        #region HasTag

        /// <summary>
        ///     Returns true if the gameobject is tagged with the given tag.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tag">Tag to check for.</param>
        /// <returns>True if it has matching tag, otherwise false.</returns>
        public static bool HasTag( this GameObject gameObject, NeatoTag tag ) =>
            Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.HasTag( tag );

        /// <summary>
        ///     Returns true if the gameobject is tagged with the given tag by name.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagName">Tag to check for</param>
        /// <returns>True if it has matching tag, otherwise false.</returns>
        public static bool HasTag( this GameObject gameObject, string tagName ) =>
            Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.HasTag( tagName );

        #endregion

        #region HasAnyTagsMatch

        /// <summary>
        ///     Returns true if the gameobject is tagged with any of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagParams">params array of tags</param>
        /// <returns>True if any tags match, otherwise false.</returns>
        public static bool HasAnyTagsMatching( this GameObject gameObject, params NeatoTag[] tagParams ) =>
            Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.AnyTagsMatch( tagParams );

        /// <summary>
        ///     Returns true if the gameobject is tagged with any of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagNameParams">params array of tag names</param>
        /// <returns>True if any tags match, otherwise false.</returns>
        public static bool HasAnyTagsMatching( this GameObject gameObject, params string[] tagNameParams ) =>
            Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.AnyTagsMatch( tagNameParams );

        /// <summary>
        ///     Returns true if the gameobject is tagged with any of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagList">IEnumerable of tags</param>
        /// <returns>True if any tags match, otherwise false.</returns>
        public static bool HasAnyTagsMatching( this GameObject gameObject, IEnumerable<NeatoTag> tagList ) =>
            Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.AnyTagsMatch( tagList );

        /// <summary>
        ///     Returns true if the gameobject is tagged with any of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagList">IEnumerable of tag names</param>
        /// <returns>True if any tags match, otherwise false.</returns>
        public static bool HasAnyTagsMatching( this GameObject gameObject, IEnumerable<string> tagList ) =>
            Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.AnyTagsMatch( tagList );

        #endregion

        #region HasAllTagsMatching

        /// <summary>
        ///     Returns true if the gameobject is tagged with all the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagParams">params array of tags</param>
        /// <returns>True if all tags match, otherwise false.</returns>
        public static bool HasAllTagsMatching( this GameObject gameObject, params NeatoTag[] tagParams ) =>
            Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.AllTagsMatch( tagParams );

        /// <summary>
        ///     Returns true if the gameobject is tagged with all the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagNameParams">params array of tag names</param>
        /// <returns>True if all tags match, otherwise false.</returns>
        public static bool HasAllTagsMatching( this GameObject gameObject, params string[] tagNameParams ) =>
            Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.AllTagsMatch( tagNameParams );

        /// <summary>
        ///     Returns true if the gameobject is tagged with all the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagList">IEnumerable of tags</param>
        /// <returns>True if any tags match, otherwise false.</returns>
        public static bool HasAllTagsMatching( this GameObject gameObject, IEnumerable<NeatoTag> tagList ) =>
            Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.AllTagsMatch( tagList );

        /// <summary>
        ///     Returns true if the gameobject is tagged with all the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagNameList">IEnumerable of tag names</param>
        /// <returns>True if any tags match, otherwise false.</returns>
        public static bool HasAllTagsMatching( this GameObject gameObject, IEnumerable<string> tagNameList ) =>
            Tagger.TryGetTagger( gameObject, out var tagger ) && tagger.AllTagsMatch( tagNameList );

        #endregion

        #region HasNoTagsMatching

        /// <summary>
        ///     Return true if the gameobject is not tagged with any of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagList">params array of tags</param>
        /// <returns>True if none of the tags match, otherwise false.</returns>
        public static bool HasNoTagsMatching( this GameObject gameObject, params NeatoTag[] tagList ) =>
            //If there is no tagger, then it is not tagged with any of the given tags.
            !Tagger.TryGetTagger( gameObject, out var tagger ) || tagger.NoTagsMatch( tagList );

        /// <summary>
        ///     Return true if the gameobject is not tagged with any of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagNameList">params array of tag names</param>
        /// <returns>True if none of the tags match, otherwise false.</returns>
        public static bool HasNoTagsMatching( this GameObject gameObject, params string[] tagNameList ) =>
            //If there is no tagger, then it is not tagged with any of the given tags.
            !Tagger.TryGetTagger( gameObject, out var tagger ) || tagger.NoTagsMatch( tagNameList );

        /// <summary>
        ///     Return true if the gameobject is not tagged with any of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagList">IEnumerable array of tags</param>
        /// <returns>True if none of the tags match, otherwise false.</returns>
        public static bool HasNoTagsMatching( this GameObject gameObject, IEnumerable<NeatoTag> tagList ) =>
            //If there is no tagger, then it is not tagged with any of the given tags.
            !Tagger.TryGetTagger( gameObject, out var tagger ) || tagger.NoTagsMatch( tagList );

        /// <summary>
        ///     Return true if the gameobject is not tagged with any of the given tags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagNameList">IEnumerable array of tag names</param>
        /// <returns>True if none of the tags match, otherwise false.</returns>
        public static bool HasNoTagsMatching( this GameObject gameObject, IEnumerable<string> tagNameList ) =>
            //If there is no tagger, then it is not tagged with any of the given tags.
            !Tagger.TryGetTagger( gameObject, out var tagger ) || tagger.NoTagsMatch( tagNameList );

        #endregion
    }
}