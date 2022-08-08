using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Core {
    /// <summary>
    /// Holds the tags for a given gameobject.
    /// </summary>
    [Serializable]
    public class Tagger : MonoBehaviour {
        static Dictionary<GameObject, Tagger> _taggers = new();
        static Dictionary<NeatoTag, HashSet<GameObject>> _taggedObjects = new();
        static HashSet<GameObject> _nonTaggedObjects = new();
        
        //This Tagger's tags.
        [SerializeField] List<NeatoTag> tags = new();
        public List<NeatoTag> GetTags => tags;

        void Awake() {
            _taggers.Add( gameObject, this );
            _nonTaggedObjects.Add( gameObject );
            foreach ( var neatoTagAsset in tags ) {
                _taggedObjects.TryAdd( neatoTagAsset, new HashSet<GameObject>() );
                _taggedObjects[neatoTagAsset].Add( gameObject );
                _nonTaggedObjects.Remove( gameObject );
            }
            
        }
        

        void OnDestroy() {
            foreach ( var neatoTag in tags ) {
                _taggedObjects[neatoTag].Remove( gameObject );
            }
            _taggers.Remove( gameObject );
            _nonTaggedObjects.Remove( gameObject );
        }

        /// <summary>
        /// Gives back a Hashset of all tags in the project.
        /// </summary>
        /// <returns>Hashset of all tags in the project.</returns>
        public static HashSet<NeatoTag> GetAllTags() {
            var tagSet = new HashSet<NeatoTag>();
            var guids = AssetDatabase.FindAssets( "t:NeatoTag" );
            foreach ( var guid in guids ) {
                var path = AssetDatabase.GUIDToAssetPath( guid );
                var tagAsset = AssetDatabase.LoadAssetAtPath<NeatoTag>( path );
                tagSet.Add( tagAsset );
            }

            return tagSet;
        }

        /// <summary>
        /// Checks if a gameobject has a Tagger component.
        /// </summary>
        /// <param name="gameObject">Gameobject to check</param>
        /// <returns>Returns true if Gameobject has a Tagger component, false if not.</returns>
        public static bool IsTagged( GameObject gameObject ) {
            return _taggers.ContainsKey( gameObject );
        }

        /// <summary>
        /// Outs the Tagger component if it has one.
        /// </summary>
        /// <param name="gameObject">Gameobject to check</param>
        /// <param name="tagger">Gameobject's Tagger component</param>
        /// <returns>Returns true if Gameobject has a Tagger component, otherwise false.</returns>
        public static bool TryGetTagger( GameObject gameObject, out Tagger tagger ) {
            return _taggers.TryGetValue( gameObject, out tagger );
        }

        /// <summary>
        /// A Dictionary of all the gameobjects that have a Tagger component.
        /// </summary>
        /// <returns>Returns a Dictionary where the keys are Gameobjects and Values are the respective Tagger component.</returns>
        public static Dictionary<GameObject, Tagger> GetTagged() {
            return _taggers;
        }

        /// <summary>
        /// Checks if Tagger has a specific tag.
        /// </summary>
        /// <param name="tag">The tag to check for</param>
        /// <returns>Returns true if Tagger has the tag, otherwise false.</returns>
        public bool HasTag( NeatoTag tag ) {
            return tags.Contains( tag );
        }

        /// <summary>
        /// Checks if Tagger has any of the tags in the list.
        /// </summary>
        /// <param name="tagList">IEnumerable of tags</param>
        /// <returns>Returns true if Tagger has any of the tags, otherwise false.</returns>
        public bool AnyTagsMatch( IEnumerable<NeatoTag> tagList ) {
            return tagList.Any( HasTag );
        }

        /// <summary>
        /// Checks if all of the tags in the list are in the Tagger.
        /// </summary>
        /// <param name="tagList">IEnumerable of tags</param>
        /// <returns>Returns true if Tagger has all of the tags, otherwise false.</returns>
        public bool AllTagsMatch( IEnumerable<NeatoTag> tagList ) {
            return tagList.All( HasTag );
        }

        /// <summary>
        /// Checks if Tagger doesn't have any of the tags in the list.
        /// </summary>
        /// <param name="tagList">IEnumerable of tags</param>
        /// <returns>Returns true if Tagger has none of the tags in the list, otherwise false.</returns>
        public bool NoTagsMatch( IEnumerable<NeatoTag> tagList ) {
            return !tagList.Any( HasTag );
        }
        
        
        /// <summary>
        /// Add a tag to the tagger.
        /// </summary>
        /// <param name="neatoTag">Tag to add.</param>
        public void AddTag( NeatoTag neatoTag ) {
            tags.Add( neatoTag );
            tags = tags.ToHashSet().ToList();
            _taggedObjects.TryAdd( neatoTag, new HashSet<GameObject>() );
            _taggedObjects[neatoTag].Add( gameObject );
            _nonTaggedObjects.Remove( gameObject );
        }

        /// <summary>
        /// Remove a tag from the tagger.
        /// </summary>
        /// <param name="neatoTag">Tag to remove.</param>
        public void RemoveTag( NeatoTag neatoTag ) {
            tags.Remove( neatoTag );
            _taggedObjects[neatoTag].Remove( gameObject );
            if( tags.Count == 0 ) {
                _nonTaggedObjects.Add( gameObject );
            }
        }

        /// <summary>
        /// Starts a filter for tags on a GameObject.
        /// WithTag(), WithTags(), WithoutTag(), WithoutTags(), WithAnyTags()
        /// To get result call .IsMatch() or .GetMatches()
        /// </summary>
        /// <returns>Returns TagFilter for chaining filter functions.</returns>
        public TagFilter StartFilter() {
            return new TagFilter( this );
        }
        
        /// <summary>
        /// Starts a filter for tagged GameObjects.
        /// If nothing is passed in, it will check against ALL tagged GameObjects.
        /// </summary>
        /// <param name="gameObjectsToCheckAgainst">Optional list of GameObjects</param>
        /// <returns>Returns TagFilter for chaining filter functions</returns>
        public static GameObjectFilter StartGameObjectFilter( IEnumerable<GameObject> gameObjectsToCheckAgainst = null ) {
            var gameObjects = gameObjectsToCheckAgainst ?? _taggers.Keys;
            return new GameObjectFilter( gameObjectsToCheckAgainst );
        }


        /// <summary>
        /// GameObjectFilter class for chaining filter functions.
        /// Don't use directly. Use StartGameObjectFilter() instead.
        /// </summary>
        public class GameObjectFilter {
            readonly IEnumerable<GameObject> _gameObjects;
            readonly HashSet<GameObject> _matches = new();
            
            public GameObjectFilter( IEnumerable<GameObject> gameObjects ) {
                if ( gameObjects == null ) {
                    _gameObjects = _taggers.Keys;
                } else {
                    this._gameObjects = gameObjects.Where( x => x.IsTagged() );
                }
                _matches.UnionWith( _gameObjects );
            }
            
            /// <summary>
            /// Returns the result of the filter.
            /// </summary>
            /// <returns>HashSet of GameObjects</returns>
            public HashSet<GameObject> GetMatches() {
                return _matches;
            }
            
            /// <summary>
            /// Filters for GameObjects that have the tag.
            /// </summary>
            /// <param name="tag">Tag to check for.</param>
            /// <returns></returns>
            public GameObjectFilter WithTag( NeatoTag tag ) {
                _taggedObjects.TryGetValue( tag, out var tempMatches );
                tempMatches ??= new HashSet<GameObject>();
                
                _matches.IntersectWith(tempMatches);
                return this;
            }
            
            /// <summary>
            /// Filters for GameObjects that have all of the tags.
            /// </summary>
            /// <param name="tags">IEnumerable of NeatoTag.</param>
            /// <returns></returns>
            public GameObjectFilter WithTags( IEnumerable<NeatoTag> tags ) {
                return tags.Aggregate( this, ( current, neatoTag ) => current.WithTag( neatoTag ) );
            }
            
            /// <summary>
            /// Filters for GameObjects that have all of the tags.
            /// </summary>
            /// <param name="tags">IEnumerable of NeatoTag.</param>
            /// <returns></returns>
            public GameObjectFilter WithTags( params NeatoTag[] tags ) {
                return tags.Aggregate( this, ( current, neatoTag ) => current.WithTag( neatoTag ) );
            }
            
            /// <summary>
            /// Filter for GameObjects that don't have the tag.
            /// </summary>
            /// <param name="tag">Tag to check for.</param>
            /// <returns></returns>
            public GameObjectFilter WithoutTag( NeatoTag tag ) {
                _matches.RemoveWhere( taggedObject => taggedObject.HasTag( tag ) );
                return this;
            }
            
            /// <summary>
            /// Filters for GameObjects that have none of the tags.
            /// </summary>
            /// <param name="tags">IEnumerable of NeatoTag.</param>
            /// <returns></returns>
            public GameObjectFilter WithoutTags( IEnumerable<NeatoTag> tags ) {
                return tags.Aggregate( this, ( current, neatoTag ) => current.WithoutTag( neatoTag ) );
            }
            
            /// <summary>
            /// Filters for GameObjects that have none of the tags.
            /// </summary>
            /// <param name="tags">IEnumerable of NeatoTag.</param>
            /// <returns></returns>
            public GameObjectFilter WithoutTags( params NeatoTag[] tags ) {
                return tags.Aggregate( this, ( current, neatoTag ) => current.WithoutTag( neatoTag ) );
            }
            
            /// <summary>
            /// Filter for GameObjects that have any of the tags.
            /// </summary>
            /// <param name="tags">IEnumerable of NeatTagAsset</param>
            /// <returns></returns>
            public GameObjectFilter WithAnyTags( IEnumerable<NeatoTag> tags ) {
                foreach ( var taggedObject in _taggedObjects.Where( taggedObject => tags.Contains( taggedObject.Key ) ) ) {
                    _matches.IntersectWith(taggedObject.Value);
                }
                return this;
            }
            
            /// <summary>
            /// Filter for GameObjects that have any of the tags.
            /// </summary>
            /// <param name="tags">IEnumerable of NeatTagAsset</param>
            /// <returns></returns>
            public GameObjectFilter WithAnyTags( params NeatoTag[] tags ) {
                var tempMatches = new HashSet<GameObject>();
                foreach ( var taggedObject in _taggedObjects.Where( taggedObject => tags.Contains( taggedObject.Key ) ) ) {
                    tempMatches.UnionWith( taggedObject.Value );
                }
                _matches.IntersectWith(tempMatches);
                return this;
            }
        }

        /// <summary>
        /// TagFilter class for chaining filter functions.
        /// Don't use directly. Use StartFilter() instead.
        /// </summary>
        public class TagFilter {
            readonly Tagger _target;
            bool _matchesFilter = true;

            public TagFilter( Tagger target ) {
                _target = target;
            }

            /// <summary>
            /// Checks if the filter matches.
            /// </summary>
            /// <returns>Returns true if filter matches, otherwise false.</returns>
            public bool IsMatch() {
                return _matchesFilter;
            }
            

            /// <summary>
            /// Checks if gameobject has tag.
            /// </summary>
            /// <param name="tag">Tag to check for</param>
            /// <returns></returns>
            public TagFilter WithTag( NeatoTag tag ) {
                _matchesFilter &= _target.HasTag( tag );
                return this;
            }

            /// <summary>
            /// Checks if gameobject has all the tags in params.
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
            /// Checks if gameobject has all the tags in list.
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
            /// Checks if gameobject doesn't have tag.
            /// </summary>
            /// <param name="tag">Tags to check for</param>
            /// <returns></returns>
            public TagFilter WithoutTag( NeatoTag tag ) {
                _matchesFilter &= !_target.HasTag( tag );

                return this;
            }

            /// <summary>
            /// Checks if gameobject doesn't have tags in params.
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
            /// Checks if gameobject doesn't have tags in list.
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
            /// Checks if gameobject has any of the tags in list.
            /// </summary>
            /// <param name="tagList">Tags to check for</param>
            /// <returns></returns>
            public TagFilter WithAnyTags( IEnumerable<NeatoTag> tagList ) {
                var neatoTagAssets = tagList as NeatoTag[] ?? tagList.ToArray();
                _matchesFilter &= _target.AnyTagsMatch( neatoTagAssets );

                return this;
            }

            /// <summary>
            /// Checks if gameobject has any of the tags in params.
            /// </summary>
            /// <param name="tags">Tags to check for</param>
            /// <returns></returns>
            public TagFilter WithAnyTags( params NeatoTag[] tags ) {
                _matchesFilter &= _target.AnyTagsMatch( tags );

                return this;
            }
        }
    }
}