using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CharlieMadeAThing.NeatoTags.Core.Editor;
using UnityEngine;
#nullable enable
namespace CharlieMadeAThing.NeatoTags.Core {
    /// <summary>
    ///     Stores and tracks attached gameobject and its tags.
    ///     Also tracks all tagged gameobjects in the scene using static collections.
    ///     Provides methods for querying the gameobject tags.
    /// </summary>
    [Serializable]
    public class Tagger : MonoBehaviour {
        // Static collections for tracking tagged objects and taggers in the scene.

        // All taggers in the scene (@runtime).
        static readonly Dictionary<GameObject, Tagger> _taggers = new();

        // All tagged objects in the scene by tag (@runtime).
        static readonly Dictionary<NeatoTag, HashSet<GameObject>> _taggedObjects = new();

        // All gameobjects in the scene that have a Tagger component but no tags (@runtime).
        static readonly HashSet<GameObject> _nonTaggedObjects = new();
        
        //--------------------------------------------------------------------------------------------------------------
        
        
        // This tagger's tags for its gameobject.
        [SerializeField] List<NeatoTag> _tags = new();
		HashSet<NeatoTag> _tagsSet = new();
        HashSet<string> _cachedTagNames = new();
        bool _isCacheDirty = true;
        public IReadOnlyList<NeatoTag> GetTags => _tags;
        static readonly Regex s_tagNameRegex = new("^[a-zA-Z0-9]+([ '-][a-zA-Z0-9]+)*$", RegexOptions.Compiled);


        void Awake() {
            //Cleanup and setup tagger. Nulls can be left behind, so we need to remove those.
            try {
                _tags.RemoveAll( nTag => !nTag );
                _tagsSet.RemoveWhere( nTag => !nTag );
                _taggers.Add( gameObject, this );
                _nonTaggedObjects.Add( gameObject );
                foreach ( var neatoTagAsset in _tags ) {
                    _taggedObjects.TryAdd( neatoTagAsset, new HashSet<GameObject>() );
                    _taggedObjects[neatoTagAsset].Add( gameObject );
                    _nonTaggedObjects.Remove( gameObject );
                    _tagsSet.Add( neatoTagAsset );
                }
            }
            catch ( Exception e ) {
                Debug.LogWarning($"Failed to initialize Tagger: {e.Message}");
            }
        }

        void OnDestroy() {
            //Remove this tagger from everything
            foreach ( var neatoTag in _tags ) {
                _taggedObjects[neatoTag].Remove( gameObject );
            }

            _taggers.Remove( gameObject );
            _nonTaggedObjects.Remove( gameObject );

#if UNITY_EDITOR
            NeatoTagTaggerTracker.UnregisterTagger( this );
            OnWantRepaint = null;
#endif
        }

        #region Cache Methods

        void UpdateCache() {
            if (!_isCacheDirty) return;
            _cachedTagNames.Clear();
            foreach ( var nTag in _tags.Where( nTag => nTag ) ) {
                _cachedTagNames.Add(nTag.name);
            }
            _isCacheDirty = false;
        }


        #endregion

        /// <summary>
        /// Try to get a NeatoTag by name.
        /// NeatoTag must exist in the scene.
        /// </summary>
        /// <param name="tagName">NeatoTag name.</param>
        /// <param name="tag">The NeatoTag if found.</param>
        /// <returns>Whether the NeatoTag was found.</returns>
        public static bool TryGetNeatoTag( string tagName, out NeatoTag? tag ) {
            tag = null;
            if ( !IsValidTagName( tagName ) ) {
                return false;
            }
            tag = _taggedObjects.Keys.FirstOrDefault( t => t.name == tagName );
            return tag;
        }

        #region Query Methods

        /// <summary>
        ///     All gameobjects in the scene with a tagger component.
        /// </summary>
        /// <returns>List of gameobjects in the scene with a tagger component.</returns>
        public static List<GameObject> GetAllGameObjectsWithTagger() => _taggers.Keys.ToList();


        /// <summary>
        ///     Checks if a gameobject has a Tagger component.
        /// </summary>
        /// <param name="gameObject">Gameobject to check</param>
        /// <returns>True if the Gameobject has a Tagger component, false if not.</returns>
        public static bool HasTagger( GameObject gameObject ) => _taggers.ContainsKey( gameObject );

        /// <summary>
        ///     Checks if a gameobject has a Tagger component and if true, will out the tagger.
        /// </summary>
        /// <param name="gameObject">Gameobject to check</param>
        /// <param name="tagger">Gameobject's Tagger component</param>
        /// <returns>True if the Gameobject has a Tagger component, otherwise false.</returns>
        public static bool TryGetTagger( GameObject gameObject, out Tagger tagger ) =>
            _taggers.TryGetValue( gameObject, out tagger );

        /// <summary>
        ///     Gets a Dictionary of all the gameobjects that have a Tagger component.
        /// </summary>
        /// <returns>A Dictionary where the keys are Gameobjects and Values are the respective Tagger component.</returns>
        public static Dictionary<GameObject, Tagger> GetGameobjectsWithTagger() => _taggers;

        /// <summary>
        ///     Checks if Tagger has a specific tag.
        /// </summary>
        /// <param name="neatoTag">The tag to check for</param>
        /// <returns>True if Tagger has the tag, otherwise false.</returns>
        public bool HasTag( NeatoTag neatoTag ) => _tagsSet.Contains( neatoTag );

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
        /// Gets the tag on the tagger by name.
        /// </summary>
        /// <param name="tagName">NeatoTag name.</param>
        /// <param name="neatoTag">The NeatoTag if found.</param>
        /// <returns>
        /// Returns true if the tag was found, otherwise false.
        /// </returns>
        public bool TryGetTag( string tagName, out NeatoTag? neatoTag ) {
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
        public bool AnyTagsMatch( IEnumerable<NeatoTag> tagList ) => tagList.Any( HasTag );

        /// <summary>
        ///     Checks if Tagger has any of the tags in the list by name.
        /// </summary>
        /// <param name="tagList">IEnumerable of tag names</param>
        /// <returns>True if Tagger has any of the tags, otherwise false.</returns>
        public bool AnyTagsMatch( IEnumerable<string> tagList ) => tagList.Any( HasTag );

        /// <summary>
        ///     Checks if all the tags in the list are in the Tagger.
        /// </summary>
        /// <param name="tagList">IEnumerable of tags</param>
        /// <returns>True if Tagger has all the tags, otherwise false.</returns>
        public bool AllTagsMatch( IEnumerable<NeatoTag> tagList ) => tagList.All( HasTag );

        /// <summary>
        ///     Checks if all the tags in the list are in the Tagger by name.
        /// </summary>
        /// <param name="tagList">IEnumerable of tag names</param>
        /// <returns>True if Tagger has all the tags, otherwise false.</returns>
        public bool AllTagsMatch( IEnumerable<string> tagList ) => tagList.All( HasTag );

        /// <summary>
        ///     Checks if Tagger doesn't have any of the tags in the list.
        /// </summary>
        /// <param name="tagList">IEnumerable of tags</param>
        /// <returns>True if Tagger has none of the tags in the list, otherwise false.</returns>
        public bool NoTagsMatch( IEnumerable<NeatoTag> tagList ) => !tagList.Any( HasTag );

        /// <summary>
        ///     Checks if Tagger doesn't have any of the tags in the list by name.
        /// </summary>
        /// <param name="tagList">IEnumerable of tag names.</param>
        /// <returns>True if Tagger has none of the tags in the list, otherwise false.</returns>
        public bool NoTagsMatch( IEnumerable<string> tagList ) => !tagList.Any( HasTag );

        #endregion

        #region Add and Remove Methods

        /// <summary>
        ///     Gets an existing tag on the tagger with the given name or creates a new one if it doesn't exist.
        ///     Tags created this way are not saved to disk.
        /// </summary>
        /// <param name="tagName">Name of the tag to get or create.</param>
        /// <returns>Returns existing or new tag if successful, otherwise returns null.</returns>
        public NeatoTag? GetOrCreate( string tagName ) {
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

            if ( !s_tagNameRegex.IsMatch( tagName ) ) {
                Debug.LogWarning(
                    $"Invalid tag name: {tagName}." );
                return false;
            }
            return true;
        }

        /// <summary>
        ///     Add a tag to the tagger.
        /// </summary>
        /// <param name="neatoTag">Tag to add.</param>
        public void AddTag( NeatoTag neatoTag ) {
            if ( !neatoTag || _tagsSet.Contains( neatoTag ) ) {
                Debug.LogWarning( "You are trying to add a tag that is either null or already exist on tagger." );
                return;
            }

            _tags.Add( neatoTag );
            _tagsSet.Add( neatoTag );
            //Doesn't matter if the TryAdd was successful or not just that it was added if it didn't exist.
            _taggedObjects.TryAdd( neatoTag, new HashSet<GameObject>() );
            _taggedObjects[neatoTag].Add( gameObject );
            _nonTaggedObjects.Remove( gameObject );
            _isCacheDirty = true;
        }

        /// <summary>
        ///     Add a list of tags to the tagger.
        /// </summary>
        /// <param name="neatoTagsToAdd">Tags to add.</param>
        public void AddTags( IEnumerable<NeatoTag> neatoTagsToAdd ) {
            var changed = false;
            foreach ( var neatoTag in neatoTagsToAdd ) {
                if ( !neatoTag || _tagsSet.Contains( neatoTag ) ) {
                    Debug.LogWarning(
                        "[NeatoTags]: You are trying to add a tag that is either null or already exist on tagger." );
                    continue;
                }

                _tags.Add( neatoTag );
                _tagsSet.Add( neatoTag );
                _taggedObjects.TryAdd( neatoTag, new HashSet<GameObject>() );
                _taggedObjects[neatoTag].Add( gameObject );
                _nonTaggedObjects.Remove( gameObject );
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
            _tagsSet.Remove( neatoTag );
            _isCacheDirty = true;
            if ( !_taggedObjects.TryGetValue( neatoTag, out var taggedGameObject ) ) {
                return;
            }

            taggedGameObject.Remove( gameObject );
            if ( _tags.Count == 0 ) {
                _nonTaggedObjects.Add( gameObject );
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
        public TagFilter FilterTags() => new( this );

        /// <summary>
        ///     Starts a filter for a list of gameobjects.
        ///     Given a list of gameobjects, this will return a new list of gameobjects using the tag filter.
        ///     If nothing is passed in, it will check against ALL tagged GameObjects.
        /// </summary>
        /// <param name="gameObjects">Optional list of GameObjects</param>
        /// <returns>FilterTags for chaining filter functions</returns>
        public static GameObjectFilter FilterGameObjects( IEnumerable<GameObject> gameObjects ) => new( gameObjects );

        /// <summary>
        ///     Starts a filter for a list of gameobjects.
        ///     If nothing is passed in, it will check against ALL tagged GameObjects.
        /// </summary>
        /// <returns>FilterTags for chaining filter functions</returns>
        public static GameObjectFilter FilterGameObjects() => new( _taggers.Keys );


        /// <summary>
        ///     GameObjectFilter class for chaining filter functions.
        ///     Don't use it directly. Use Tagger.FilterGameObjects() instead.
        /// </summary>
        public sealed class GameObjectFilter {
            readonly HashSet<GameObject> _matches;

            public GameObjectFilter( IEnumerable<GameObject> gameObjects ) {
                _matches = new HashSet<GameObject>();
                var gameObjectsToFilter = gameObjects == null ? _taggers.Keys : gameObjects.Where( x => x.HasTagger() );
                _matches.UnionWith( gameObjectsToFilter );
            }

            /// <summary>
            ///     The result of the filter.
            ///     Will not return duplicate GameObjects.
            /// </summary>
            /// <returns>HashSet of GameObjects</returns>
            public HashSet<GameObject> GetMatches() => _matches;


            /// <summary>
            ///     Filters for GameObjects that have the tag.
            /// </summary>
            /// <param name="tag">Tag to check for.</param>
            /// <returns></returns>
            public GameObjectFilter WithTag( NeatoTag tag ) {
                _taggedObjects.TryGetValue( tag, out var tempMatches );
                tempMatches ??= new HashSet<GameObject>();

                _matches.IntersectWith( tempMatches );
                return this;
            }

            /// <summary>
            ///     Filters for GameObjects that have all the tags.
            /// </summary>
            /// <param name="tags">IEnumerable of NeatoTag.</param>
            /// <returns></returns>
            public GameObjectFilter WithTags( IEnumerable<NeatoTag> tags ) {
                return tags.Aggregate( this, ( current, neatoTag ) => current.WithTag( neatoTag ) );
            }

            /// <summary>
            ///     Filters for GameObjects that have all the tags.
            /// </summary>
            /// <param name="tags">IEnumerable of NeatoTag.</param>
            /// <returns></returns>
            public GameObjectFilter WithTags( params NeatoTag[] tags ) {
                return WithTags( tags.AsEnumerable() );
            }
            
            /// <summary>
            ///     FilterGameObjects for GameObjects that don't have the tag.
            /// </summary>
            /// <param name="tag">Tag to check for.</param>
            /// <returns></returns>
            public GameObjectFilter WithoutTag( NeatoTag tag ) {
                _matches.RemoveWhere( taggedObject => taggedObject.HasTag( tag ) );
                return this;
            }

            /// <summary>
            ///     Filters for GameObjects that have none of the tags.
            /// </summary>
            /// <param name="tags">IEnumerable of NeatoTag.</param>
            /// <returns></returns>
            public GameObjectFilter WithoutTags( IEnumerable<NeatoTag> tags ) {
                return tags.Aggregate( this, ( current, neatoTag ) => current.WithoutTag( neatoTag ) );
            }

            /// <summary>
            ///     Filters for GameObjects that have none of the tags.
            /// </summary>
            /// <param name="tags">IEnumerable of NeatoTag.</param>
            /// <returns></returns>
            public GameObjectFilter WithoutTags( params NeatoTag[] tags ) {
                return WithoutTags( tags.AsEnumerable() );
            }

            /// <summary>
            ///     FilterGameObjects for GameObjects that have any of the tags.
            /// </summary>
            /// <param name="tags">IEnumerable of NeatTagAsset</param>
            /// <returns></returns>
            public GameObjectFilter WithAnyTags( IEnumerable<NeatoTag> tags ) {
                if ( tags == null ) {
                    _matches.Clear();
                    return this;
                }
                var neatoTags = tags as NeatoTag[] ?? tags.ToArray();
                if ( !neatoTags.Any() ) {
                    _matches.Clear();
                    return this;
                }
                var tempMatches = new HashSet<GameObject>();
                
                foreach ( var neatoTag in neatoTags ) {
                    if ( !_taggedObjects.TryGetValue( neatoTag, out var taggedObjects ) ) continue;
                    tempMatches.UnionWith( taggedObjects );
                }
                _matches.IntersectWith( tempMatches );
                return this;
            }

            /// <summary>
            ///     FilterGameObjects for GameObjects that have any of the tags.
            /// </summary>
            /// <param name="tags">IEnumerable of NeatTagAsset</param>
            /// <returns></returns>
            public GameObjectFilter WithAnyTags( params NeatoTag[] tags ) {
                return WithAnyTags( tags.AsEnumerable() );
            }
        }

        /// <summary>
        ///     FilterTags class for chaining filter functions.
        ///     Don't use it directly. Use FilterTags() instead.
        /// </summary>
        public class TagFilter {
            readonly Tagger _target;
            bool _matchesFilter = true;

            public TagFilter( Tagger target ) => _target = target;

            /// <summary>
            ///     Checks if the filter matches.
            /// </summary>
            /// <returns>true if filter matches, otherwise false.</returns>
            public bool IsMatch() => _matchesFilter;


            /// <summary>
            ///     Checks if the gameobject has the tag.
            /// </summary>
            /// <param name="tag">Tag to check for</param>
            /// <returns></returns>
            public TagFilter WithTag( NeatoTag tag ) {
                _matchesFilter &= _target.HasTag( tag );
                return this;
            }

            /// <summary>
            ///     Checks if the gameobject has all the tags in params.
            /// </summary>
            /// <param name="tags">Tags to check for</param>
            /// <returns></returns>
            public TagFilter WithTags( params NeatoTag[] tags ) {
                foreach ( var tagAsset in tags ) {
                    _matchesFilter &= _target.HasTag( tagAsset );
                }

                return this;
            }

            /// <summary>
            ///     Checks if the gameobject has all the tags in a list.
            /// </summary>
            /// <param name="tagList">Tags to check for</param>
            /// <returns></returns>
            public TagFilter WithTags( IEnumerable<NeatoTag> tagList ) {
                foreach ( var tagAsset in tagList ) {
                    _matchesFilter &= _target.HasTag( tagAsset );
                }

                return this;
            }

            /// <summary>
            ///     Checks if the gameobject doesn't have the tag.
            /// </summary>
            /// <param name="tag">Tags to check for</param>
            /// <returns></returns>
            public TagFilter WithoutTag( NeatoTag tag ) {
                _matchesFilter &= !_target.HasTag( tag );

                return this;
            }

            /// <summary>
            ///     Checks if the gameobject doesn't have tags in params.
            /// </summary>
            /// <param name="tags">Tags to check for</param>
            /// <returns></returns>
            public TagFilter WithoutTags( params NeatoTag[] tags ) {
                foreach ( var tagAsset in tags ) {
                    _matchesFilter &= !_target.HasTag( tagAsset );
                }

                return this;
            }

            /// <summary>
            ///     Checks if the gameobject doesn't have tags in a list.
            /// </summary>
            /// <param name="tagList">Tags to check for</param>
            /// <returns></returns>
            public TagFilter WithoutTags( IEnumerable<NeatoTag> tagList ) {
                foreach ( var tagAsset in tagList ) {
                    _matchesFilter &= !_target.HasTag( tagAsset );
                }

                return this;
            }

            /// <summary>
            ///     Checks if the gameobject has any of the tags in a list.
            /// </summary>
            /// <param name="tagList">Tags to check for</param>
            /// <returns></returns>
            public TagFilter WithAnyTags( IEnumerable<NeatoTag> tagList ) {
                var neatoTagAssets = tagList as NeatoTag[] ?? tagList.ToArray();
                _matchesFilter &= _target.AnyTagsMatch( neatoTagAssets );

                return this;
            }

            /// <summary>
            ///     Checks if the gameobject has any of the tags in params.
            /// </summary>
            /// <param name="tags">Tags to check for</param>
            /// <returns></returns>
            public TagFilter WithAnyTags( params NeatoTag[] tags ) {
                _matchesFilter &= _target.AnyTagsMatch( tags );

                return this;
            }
        }

        #endregion

        //This is a bit of a hacky way to get the inspector to repaint when a tag is added or removed
        //when the edit tagger button has not been pressed.
#if UNITY_EDITOR
        public delegate void RepaintAction();

        public event RepaintAction? OnWantRepaint;
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
            _tagsSet.RemoveWhere( neatoTag => !neatoTag );
        }
#endif
    }
}