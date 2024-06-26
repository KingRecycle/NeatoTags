using System;
using System.Collections.Generic;
using System.Linq;
using CharlieMadeAThing.NeatoTags.Core.Editor;
using UnityEngine;

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
        static Dictionary<GameObject, Tagger> _taggers = new();

        // All tagged objects in the scene by tag (@runtime).
        static Dictionary<NeatoTag, HashSet<GameObject>> _taggedObjects = new();

        // All gameobjects in the scene that have a Tagger component but no tags (@runtime).
        static HashSet<GameObject> _nonTaggedObjects = new();

        //--------------------------------------------------------------------------------------------------------------

        // This taggers tags for its gameobject.
        [SerializeField] List<NeatoTag> tags = new();
        public List<NeatoTag> GetTags => tags;

        void Awake() {
            //Cleanup and setup tagger. Nulls can be left behind, so we need to remove those.
            tags.RemoveAll( nTag => nTag == null );
            _taggers.Add( gameObject, this );
            _nonTaggedObjects.Add( gameObject );
            foreach ( var neatoTagAsset in tags ) {
                _taggedObjects.TryAdd( neatoTagAsset, new HashSet<GameObject>() );
                _taggedObjects[neatoTagAsset].Add( gameObject );
                _nonTaggedObjects.Remove( gameObject );
            }
        }

        void OnDestroy() {
            //Remove this tagger from everything
            foreach ( var neatoTag in tags ) {
                _taggedObjects[neatoTag].Remove( gameObject );
            }

            _taggers.Remove( gameObject );
            _nonTaggedObjects.Remove( gameObject );

#if UNITY_EDITOR
            NeatoTagTaggerTracker.UnregisterTagger( this );
            WantRepaint = null;
#endif
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
        /// <returns>True if Gameobject has a Tagger component, false if not.</returns>
        public static bool HasTagger( GameObject gameObject ) => _taggers.ContainsKey( gameObject );

        /// <summary>
        ///     Checks if a gameobject has a Tagger component and if true will out the tagger.
        /// </summary>
        /// <param name="gameObject">Gameobject to check</param>
        /// <param name="tagger">Gameobject's Tagger component</param>
        /// <returns>True if Gameobject has a Tagger component, otherwise false.</returns>
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
        public bool HasTag( NeatoTag neatoTag ) => tags.Contains( neatoTag );

        /// <summary>
        ///     Checks if Tagger has a specific tag by tag name.
        /// </summary>
        /// <param name="neatoTag">The tag name to check for</param>
        /// <returns>True if Tagger has the tag, otherwise false.</returns>
        public bool HasTag( string neatoTag ) {
            return tags.Any( t => t.name == neatoTag );
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
        ///     Checks if all of the tags in the list are in the Tagger by name.
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
        ///     Create a new tag.
        ///     This tag will be deleted if not saved.
        ///     Name is trimmed of whitespace.
        /// </summary>
        /// <param name="tagName">Name of the new tag.</param>
        /// <returns>Returns new tag if successful otherwise returns null.</returns>
        public NeatoTag CreateTag( string tagName ) {
            var trimmedName = tagName.Trim();
            if ( trimmedName == string.Empty ) {
                Debug.LogWarning( $"A tag name can't be an empty string." );
                return null;
            }

            var neatoTag = ScriptableObject.CreateInstance<NeatoTag>();
            if ( _taggedObjects.Keys.Any( t => t.name == trimmedName ) ) {
                Debug.LogWarning( $"You are trying to create a tag with the name {trimmedName} that already exists." );
                return null;
            }

            neatoTag.name = trimmedName;
            return neatoTag;
        }

        /// <summary>
        ///     Create a new tag and add it to the tagger.
        ///     This tag will be deleted if not saved.
        ///     Name is trimmed of whitespace.
        /// </summary>
        /// <param name="tagName">Name of the new tag.</param>
        public bool TryCreateAndAddTag( string tagName ) {
            var neatoTag = CreateTag( tagName );
            if ( neatoTag == null ) {
                return false;
            }

            AddTag( neatoTag );
            return true;
        }

        /// <summary>
        ///     Add a tag to the tagger.
        /// </summary>
        /// <param name="neatoTag">Tag to add.</param>
        public void AddTag( NeatoTag neatoTag ) {
            if ( neatoTag == null || tags.Contains( neatoTag ) ) {
                Debug.LogWarning( "You are trying to add a tag that is either null or already exist on tagger." );
                return;
            }

            tags.Add( neatoTag );
            //Doesn't matter if the TryAdd was successful or not just that it was added if it didn't exist.
            _taggedObjects.TryAdd( neatoTag, new HashSet<GameObject>() );
            _taggedObjects[neatoTag].Add( gameObject );
            _nonTaggedObjects.Remove( gameObject );
        }

        /// <summary>
        ///     Remove a tag from the tagger.
        /// </summary>
        /// <param name="neatoTag">Tag to remove.</param>
        public void RemoveTag( NeatoTag neatoTag ) {
            if ( neatoTag == null ) {
                Debug.LogWarning( "You are trying to remove a tag that is null." );
                return;
            }

            tags.Remove( neatoTag );

            if ( !_taggedObjects.ContainsKey( neatoTag ) ) {
                return;
            }

            _taggedObjects[neatoTag].Remove( gameObject );
            if ( tags.Count == 0 ) {
                _nonTaggedObjects.Add( gameObject );
            }
        }

        /// <summary>
        ///     Remove a tag from the tagger by name.
        /// </summary>
        /// <param name="tagName"></param>
        public void RemoveTag( string tagName ) {
            var neatoTag = tags.FirstOrDefault( t => t.name == tagName );
            if ( neatoTag == null ) {
                Debug.LogWarning( $"You are trying to remove a tag with the name {tagName} that doesn't exist." );
                return;
            }

            RemoveTag( neatoTag );
        }

        /// <summary>
        ///     Remove ALL tags from the tagger.
        /// </summary>
        public void RemoveAllTags() {
            for ( var i = tags.Count - 1; i >= 0; i-- ) {
                RemoveTag( tags[i] );
            }
        }

        #endregion

        #region Filter Methods

        /// <summary>
        ///     Starts a filter for tags on a GameObject.
        ///     WithTag(), WithTags(), WithoutTag(), WithoutTags(), WithAnyTags()
        ///     To get result call .IsMatch() or .GetMatches()
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
        ///     Don't use directly. Use Tagger.FilterGameObjects() instead.
        /// </summary>
        public sealed class GameObjectFilter {
            readonly HashSet<GameObject> _matches;

            public GameObjectFilter( IEnumerable<GameObject> gameObjects ) {
                _matches = new HashSet<GameObject>();
                var gameObjectsToFilter = gameObjects == null ? _taggers.Keys : gameObjects.Where( x => x.HasTagger() );
                _matches.UnionWith( gameObjectsToFilter );
            }

            /// <summary>
            ///     the result of the filter.
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
            ///     Filters for GameObjects that have all of the tags.
            /// </summary>
            /// <param name="tags">IEnumerable of NeatoTag.</param>
            /// <returns></returns>
            public GameObjectFilter WithTags( IEnumerable<NeatoTag> tags ) {
                return tags.Aggregate( this, ( current, neatoTag ) => current.WithTag( neatoTag ) );
            }

            /// <summary>
            ///     Filters for GameObjects that have all of the tags.
            /// </summary>
            /// <param name="tags">IEnumerable of NeatoTag.</param>
            /// <returns></returns>
            public GameObjectFilter WithTags( params NeatoTag[] tags ) {
                return tags.Aggregate( this, ( current, neatoTag ) => current.WithTag( neatoTag ) );
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
                return tags.Aggregate( this, ( current, neatoTag ) => current.WithoutTag( neatoTag ) );
            }

            /// <summary>
            ///     FilterGameObjects for GameObjects that have any of the tags.
            /// </summary>
            /// <param name="tags">IEnumerable of NeatTagAsset</param>
            /// <returns></returns>
            public GameObjectFilter WithAnyTags( IEnumerable<NeatoTag> tags ) {
                foreach ( var taggedObject in
                         _taggedObjects.Where( taggedObject => tags.Contains( taggedObject.Key ) ) ) {
                    _matches.IntersectWith( taggedObject.Value );
                }

                return this;
            }

            /// <summary>
            ///     FilterGameObjects for GameObjects that have any of the tags.
            /// </summary>
            /// <param name="tags">IEnumerable of NeatTagAsset</param>
            /// <returns></returns>
            public GameObjectFilter WithAnyTags( params NeatoTag[] tags ) {
                var tempMatches = new HashSet<GameObject>();
                foreach ( var taggedObject in
                         _taggedObjects.Where( taggedObject => tags.Contains( taggedObject.Key ) ) ) {
                    tempMatches.UnionWith( taggedObject.Value );
                }

                _matches.IntersectWith( tempMatches );
                return this;
            }
        }

        /// <summary>
        ///     FilterTags class for chaining filter functions.
        ///     Don't use directly. Use FilterTags() instead.
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
            ///     Checks if gameobject has tag.
            /// </summary>
            /// <param name="tag">Tag to check for</param>
            /// <returns></returns>
            public TagFilter WithTag( NeatoTag tag ) {
                _matchesFilter &= _target.HasTag( tag );
                return this;
            }

            /// <summary>
            ///     Checks if gameobject has all the tags in params.
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
            ///     Checks if gameobject has all the tags in list.
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
            ///     Checks if gameobject doesn't have tag.
            /// </summary>
            /// <param name="tag">Tags to check for</param>
            /// <returns></returns>
            public TagFilter WithoutTag( NeatoTag tag ) {
                _matchesFilter &= !_target.HasTag( tag );

                return this;
            }

            /// <summary>
            ///     Checks if gameobject doesn't have tags in params.
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
            ///     Checks if gameobject doesn't have tags in list.
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
            ///     Checks if gameobject has any of the tags in list.
            /// </summary>
            /// <param name="tagList">Tags to check for</param>
            /// <returns></returns>
            public TagFilter WithAnyTags( IEnumerable<NeatoTag> tagList ) {
                var neatoTagAssets = tagList as NeatoTag[] ?? tagList.ToArray();
                _matchesFilter &= _target.AnyTagsMatch( neatoTagAssets );

                return this;
            }

            /// <summary>
            ///     Checks if gameobject has any of the tags in params.
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

        public event RepaintAction WantRepaint;
#endif


#if UNITY_EDITOR
        void RepaintInspector() {
            WantRepaint?.Invoke();
        }

        void Reset() {
            RemoveAllTags();
            RepaintInspector();
        }

        void OnValidate() {
            NeatoTagTaggerTracker.RegisterTagger( this );
            tags.RemoveAll( neatoTag => neatoTag == null );
        }
#endif
    }
}