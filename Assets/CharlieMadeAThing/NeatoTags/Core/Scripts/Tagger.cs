using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CharlieMadeAThing.NeatoTags.Core.Editor;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Core {
    /// <summary>
    ///     Tagger class for managing and querying tags on GameObjects.
    ///     This class provides functionalities for adding, removing, and filtering tags.
    /// </summary>
    [Serializable]
    public class Tagger : MonoBehaviour {
        [SerializeField] List<NeatoTag> _tags = new();
        HashSet<string> _cachedTagNames = new();
        bool _isCacheDirty = true;

        public IReadOnlyList<NeatoTag> GetTags => _tags;

        // Allows any character except < and >, but not empty or whitespace-only strings
        public static readonly Regex TagNameRegex = new("^(?!\\s*$)[^<>]+$", RegexOptions.Compiled);


        void Awake() {
            //Cleanup and setup tagger. Nulls can be left behind, so we need to remove those.
            try {
                _tags.RemoveAll( nTag => !nTag );
                TaggerRegistry.InitializeNewTagger( gameObject, this );
            }
            catch ( Exception e ) {
                Debug.LogWarning( $"Failed to initialize Tagger: {e.Message}" );
            }
        }

        void OnDestroy() {
            //Remove this tagger from everything
            TaggerRegistry.RemoveTaggerFromRegistry( gameObject, this );

#if UNITY_EDITOR
            NeatoTagTaggerTracker.UnregisterTagger( this );
            OnWantRepaint = null;
#endif
        }

        #region Cache Methods

        void UpdateCache() {
            if ( !_isCacheDirty ) return;
            _cachedTagNames.Clear();
            foreach ( var nTag in _tags ) {
                if ( nTag != null ) {
                    _cachedTagNames.Add( nTag.name );
                }
            }


            _isCacheDirty = false;
        }

        #endregion

        /// <summary>
        ///     Try to get a NeatoTag by name.
        /// </summary>
        /// <remarks>
        /// The tag must have been registered at some point, aka added to a Tagger.
        /// Even if the tag was removed from all Taggers, it can still be retrieved.
        /// </remarks>
        /// <param name="tagName">NeatoTag name.</param>
        /// <param name="tag">The NeatoTag if found.</param>
        /// <returns>Whether the NeatoTag was found.</returns>
        public static bool TryGetNeatoTag( string tagName, out NeatoTag tag ) {
            tag = null;
            if ( !IsValidTagName( tagName ) ) {
                return false;
            }

            tag = TaggerRegistry.GetRegisteredTag( tagName );
            return tag;
        }

        #region Query Methods

        /// <summary>
        ///     All gameobjects in the scene with a tagger component.
        /// </summary>
        /// <returns>List of gameobjects in the scene with a tagger component.</returns>
        public static List<GameObject> GetAllGameObjectsWithTagger() {
            return TaggerRegistry.GetStaticTaggersDictionary().Keys.ToList();
        }

        /// <summary>
        ///     Checks if a gameobject has a Tagger component.
        /// </summary>
        /// <param name="gameObject">Gameobject to check</param>
        /// <returns>True if the Gameobject has a Tagger component, false if not.</returns>
        public static bool HasTagger( GameObject gameObject ) {
            return TaggerRegistry.GetStaticTaggersDictionary().ContainsKey( gameObject );
        }

        /// <summary>
        ///     Checks if a gameobject has a Tagger component and if true, will out the tagger.
        /// </summary>
        /// <param name="gameObject">Gameobject to check</param>
        /// <param name="tagger">Gameobject's Tagger component</param>
        /// <returns>True if the Gameobject has a Tagger component, otherwise false.</returns>
        public static bool TryGetTagger( GameObject gameObject, out Tagger tagger ) {
            return TaggerRegistry.GetStaticTaggersDictionary().TryGetValue( gameObject, out tagger );
        }

        /// <summary>
        ///     Gets a Dictionary of all the gameobjects that have a Tagger component.
        /// </summary>
        /// <returns>A Dictionary where the keys are Gameobjects and Values are the respective Tagger component.</returns>
        public static Dictionary<GameObject, Tagger> GetGameobjectsWithTagger() {
            return TaggerRegistry.GetStaticTaggersDictionary();
        }

        /// <summary>
        ///     Checks if Tagger has a specific tag.
        /// </summary>
        /// <param name="neatoTag">The tag to check for</param>
        /// <returns>True if Tagger has the tag, otherwise false.</returns>
        public bool HasTag( NeatoTag neatoTag ) {
            return _tags.Contains( neatoTag );
        }

        /// <summary>
        ///     Checks if Tagger has a specific tag by tag name.
        /// </summary>
        /// <param name="neatoTag">The tag name to check for</param>
        /// <returns>True if Tagger has the tag, otherwise false.</returns>
        public bool HasTag( string neatoTag ) {
            UpdateCache();
            return _cachedTagNames.Contains( neatoTag );
        }

        /// <summary>
        ///     Gets the number of tags on the Tagger.
        /// </summary>
        /// <returns>Number of tags on Tagger.</returns>
        public int GetTagCount() {
            return _tags.Count;
        }

        /// <summary>
        ///     Gets whether Tagger has any tags or not.
        /// </summary>
        /// <returns>Returns true if Tagger has more than 0 tags.</returns>
        public bool IsTagged() {
            return GetTagCount() > 0;
        }

        /// <summary>
        ///     Gets the tag on the tagger by name.
        /// </summary>
        /// <param name="tagName">NeatoTag name.</param>
        /// <param name="neatoTag">The NeatoTag if found.</param>
        /// <returns>
        ///     Returns true if the tag was found, otherwise false.
        /// </returns>
        public bool TryGetTag( string tagName, out NeatoTag neatoTag ) {
            var found = GetTags.FirstOrDefault( t => t.name == tagName );
            if ( found ) {
                neatoTag = found;
                return true;
            }

            neatoTag = null;
            return false;
        }

        /// <summary>
        ///     Checks if Tagger has any of the tags in the list.
        /// </summary>
        /// <param name="tagList">IEnumerable of tags</param>
        /// <returns>True if Tagger has any of the tags, otherwise false.</returns>
        public bool AnyTagsMatch( IEnumerable<NeatoTag> tagList ) {
            return tagList.Any( HasTag );
        }

        /// <summary>
        ///     Checks if Tagger has any of the tags in the list by name.
        /// </summary>
        /// <param name="tagList">IEnumerable of tag names</param>
        /// <returns>True if Tagger has any of the tags, otherwise false.</returns>
        public bool AnyTagsMatch( IEnumerable<string> tagList ) {
            return tagList.Any( HasTag );
        }

        /// <summary>
        ///     Checks if all the tags in the list are in the Tagger.
        /// </summary>
        /// <param name="tagList">IEnumerable of tags</param>
        /// <returns>True if Tagger has all the tags, otherwise false.</returns>
        public bool AllTagsMatch( IEnumerable<NeatoTag> tagList ) {
            return tagList.All( HasTag );
        }

        /// <summary>
        ///     Checks if all the tags in the list are in the Tagger by name.
        /// </summary>
        /// <param name="tagList">IEnumerable of tag names</param>
        /// <returns>True if Tagger has all the tags, otherwise false.</returns>
        public bool AllTagsMatch( IEnumerable<string> tagList ) {
            return tagList.All( HasTag );
        }

        /// <summary>
        ///     Checks if Tagger doesn't have any of the tags in the list.
        /// </summary>
        /// <param name="tagList">IEnumerable of tags</param>
        /// <returns>True if Tagger has none of the tags in the list, otherwise false.</returns>
        public bool NoTagsMatch( IEnumerable<NeatoTag> tagList ) {
            return !tagList.Any( HasTag );
        }

        /// <summary>
        ///     Checks if Tagger doesn't have any of the tags in the list by name.
        /// </summary>
        /// <param name="tagList">IEnumerable of tag names.</param>
        /// <returns>True if Tagger has none of the tags in the list, otherwise false.</returns>
        public bool NoTagsMatch( IEnumerable<string> tagList ) {
            return !tagList.Any( HasTag );
        }

        #endregion

        #region Add and Remove Methods

        /// <summary>
        ///     Gets an existing tag on the tagger with the given name or creates a new one if it doesn't exist.
        ///     Tags created this way are not saved to disk.
        /// </summary>
        /// <param name="tagName">Name of the tag to get or create.</param>
        /// <returns>Returns existing or new tag if successful, otherwise returns null.</returns>
        public NeatoTag GetOrCreate( string tagName ) {
            if ( !IsValidTagName( tagName ) ) {
                return null;
            }

            // Check if the tag already exists
            var exist = TryGetTag( tagName, out var nTag );
            if ( exist ) {
                return nTag;
            }

            // Create a new tag if it doesn't exist
            var neatoTag = ScriptableObject.CreateInstance<NeatoTag>();
            neatoTag.name = tagName;
            AddTag( neatoTag );
            return neatoTag;
        }

        /// <summary>
        ///     Checks if a tag name is valid.
        /// </summary>
        /// <param name="tagName">Name of the tag to check.</param>
        /// <returns>True if the tag name is valid, otherwise false.</returns>
        static bool IsValidTagName( string tagName ) {
            if ( string.IsNullOrWhiteSpace( tagName ) ) {
                Debug.LogWarning( "A tag name can't be empty or whitespace." );
                return false;
            }

            if ( !TagNameRegex.IsMatch( tagName ) ) {
                Debug.LogWarning( $"Invalid tag name: {tagName}. Tag names cannot contain < or > characters." );
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Add a tag to the tagger.
        /// </summary>
        /// <param name="neatoTag">Tag to add.</param>
        public void AddTag( NeatoTag neatoTag ) {
            if ( !neatoTag || _tags.Contains( neatoTag ) ) {
                Debug.LogWarning( "You are trying to add a tag that is either null or already exist on tagger." );
                return;
            }

            _tags.Add( neatoTag );
            TaggerRegistry.RegisterTag( neatoTag );
            TaggerRegistry.RegisterGameObjectToTag( gameObject, neatoTag );
            _isCacheDirty = true;
        }

        /// <summary>
        ///     Add a list of tags to the tagger.
        /// </summary>
        /// <param name="neatoTagsToAdd">Tags to add.</param>
        public void AddTags( IEnumerable<NeatoTag> neatoTagsToAdd ) {
            var changed = false;
            foreach ( var neatoTag in neatoTagsToAdd ) {
                if ( !neatoTag || _tags.Contains( neatoTag ) ) {
                    Debug.LogWarning(
                        "[NeatoTags]: You are trying to add a tag that is either null or already exist on tagger." );
                    continue;
                }

                _tags.Add( neatoTag );
                TaggerRegistry.RegisterTag( neatoTag );
                TaggerRegistry.RegisterGameObjectToTag( gameObject, neatoTag );
                changed = true;
            }

            if ( changed ) {
                _isCacheDirty = true;
            }
        }

        /// <summary>
        ///     Remove a tag from the tagger.
        /// </summary>
        /// <param name="neatoTag">Tag to remove.</param>
        public void RemoveTag( NeatoTag neatoTag ) {
            if ( !neatoTag ) {
                Debug.LogWarning( "You are trying to remove a tag that is null." );
                return;
            }

            _tags.Remove( neatoTag );
            _isCacheDirty = true;
            if ( !TaggerRegistry.GetStaticTaggedObjectsDictionary()
                    .TryGetValue( neatoTag, out var taggedGameObject ) ) {
                return;
            }

            taggedGameObject.Remove( gameObject );
            if ( _tags.Count == 0 ) {
                TaggerRegistry.RegisterNonTaggedGameObject( gameObject );
            }
        }

        /// <summary>
        ///     Remove a tag from the tagger by name.
        /// </summary>
        /// <param name="tagName"></param>
        public void RemoveTag( string tagName ) {
            var neatoTag = _tags.FirstOrDefault( t => t.name == tagName );
            if ( !neatoTag ) {
                Debug.LogWarning( $"You are trying to remove a tag with the name {tagName} that doesn't exist." );
                return;
            }

            RemoveTag( neatoTag! );
        }

        /// <summary>
        ///     Remove ALL tags from the tagger.
        /// </summary>
        public void RemoveAllTags() {
            for ( var i = _tags.Count - 1; i >= 0; i-- ) {
                RemoveTag( _tags[i] );
            }
        }

        #endregion

        #region Filter Methods

        /// <summary>
        ///     Starts a filter for tags on a GameObject.
        ///     WithTag(), WithTags(), WithoutTag(), WithoutTags(), WithAnyTags()
        ///     To get the result, call .IsMatch() or .GetMatches()
        /// </summary>
        /// <returns>FilterTags for chaining filter functions.</returns>
        public TagFilter FilterTags() {
            return new TagFilter( this );
        }

        /// <summary>
        ///     Starts a filter for a list of gameobjects.
        ///     Given a list of gameobjects, this will return a new list of gameobjects using the tag filter.
        ///     If nothing is passed in, it will check against ALL tagged GameObjects.
        /// </summary>
        /// <param name="gameObjects">Optional list of GameObjects</param>
        /// <returns>FilterTags for chaining filter functions</returns>
        public static GameObjectFilter FilterGameObjects( IEnumerable<GameObject> gameObjects ) {
            return new GameObjectFilter( gameObjects );
        }

        /// <summary>
        ///     Starts a filter for a list of gameobjects.
        ///     If nothing is passed in, it will check against ALL tagged GameObjects.
        /// </summary>
        /// <returns>FilterTags for chaining filter functions</returns>
        public static GameObjectFilter FilterGameObjects() {
            return new GameObjectFilter( TaggerRegistry.GetStaticTaggersDictionary().Keys );
        }

        #endregion

        //This is a bit of a hacky way to get the inspector to repaint when a tag is added or removed
        //when the edit tagger button has not been pressed.
#if UNITY_EDITOR
        public delegate void RepaintAction();

        public event RepaintAction OnWantRepaint;
#endif


#if UNITY_EDITOR
        void RepaintInspector() {
            OnWantRepaint?.Invoke();
        }

        void Reset() {
            RemoveAllTags();
            RepaintInspector();
        }

        void OnValidate() {
            NeatoTagTaggerTracker.RegisterTagger( this );
            _tags.RemoveAll( neatoTag => !neatoTag );
        }
#endif
    }
}