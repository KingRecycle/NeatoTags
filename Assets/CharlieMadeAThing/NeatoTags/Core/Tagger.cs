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
        [SerializeField] List<NeatoTagAsset> tags = new();

        public List<NeatoTagAsset> GetTags => tags;


        void OnEnable() {
            _taggers.Add( gameObject, this );
        }

        void OnDisable() {
            _taggers.Remove( gameObject );
        }


        public static HashSet<NeatoTagAsset> GetAllTags() {
            var tagSet = new HashSet<NeatoTagAsset>();
            var guids = AssetDatabase.FindAssets( "t:NeatoTagAsset" );
            foreach ( var guid in guids ) {
                var path = AssetDatabase.GUIDToAssetPath( guid );
                var tagAsset = AssetDatabase.LoadAssetAtPath<NeatoTagAsset>( path );
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
        /// <returns></returns>
        public static Dictionary<GameObject, Tagger> GetTagged() {
            return _taggers;
        }

        /// <summary>
        /// Checks if Tagger has a specific tag.
        /// </summary>
        /// <param name="tagAsset">The tag to check for</param>
        /// <returns>Returns true if Tagger has the tag, otherwise false.</returns>
        public bool HasTag( NeatoTagAsset tagAsset ) {
            return tags.Contains( tagAsset );
        }

        /// <summary>
        /// Checks if Tagger has any of the tags in the list.
        /// </summary>
        /// <param name="tagList">IEnumerable of tags</param>
        /// <returns>Returns true if Tagger has any of the tags, otherwise false.</returns>
        public bool AnyTagsMatch( IEnumerable<NeatoTagAsset> tagList ) {
            return tagList.Any( HasTag );
        }

        /// <summary>
        /// Checks if all of the tags in the list are in the Tagger.
        /// </summary>
        /// <param name="tagList">IEnumerable of tags</param>
        /// <returns>Returns true if Tagger has all of the tags, otherwise false.</returns>
        public bool AllTagsMatch( IEnumerable<NeatoTagAsset> tagList ) {
            return tagList.All( HasTag );
        }

        /// <summary>
        /// Checks if Tagger doesn't have any of the tags in the list.
        /// </summary>
        /// <param name="tagList">IEnumerable of tags</param>
        /// <returns>Returns true if Tagger has none of the tags in the list, otherwise false.</returns>
        public bool NoTagsMatch( IEnumerable<NeatoTagAsset> tagList ) {
            return !tagList.Any( HasTag );
        }
        
        /// <summary>
        /// Returns a Hashset of GameObjects that have the tag.
        /// </summary>
        /// <param name="tagAsset">Tag to check for</param>
        /// <returns>Hashset of Gameobjects</returns>
        public HashSet<GameObject> GetTaggedGameObjectsWithTag( NeatoTagAsset tagAsset ) {
            var taggedGameObjects = new HashSet<GameObject>();
            foreach ( var tagger in _taggers.Values.Where( tagger => tagger.HasTag( tagAsset ) ) ) {
                taggedGameObjects.Add( tagger.gameObject );
            }

            return taggedGameObjects;
        }

        /// <summary>
        /// Returns a Hashset of GameObjects that don't have the tag.
        /// </summary>
        /// <param name="tagAsset">Tag to check for</param>
        /// <returns>Hashset of Gameobjects</returns>
        public HashSet<GameObject> GetTaggedGameObjectsWithoutTag( NeatoTagAsset tagAsset ) {
            var taggedGameObjects = new HashSet<GameObject>();
            foreach ( var tagger in _taggers.Values.Where( tagger => !tagger.HasTag( tagAsset ) ) ) {
                taggedGameObjects.Add( tagger.gameObject );
            }

            return taggedGameObjects;
        }

        /// <summary>
        /// Starts a filter for chaining filter functions.
        /// WithTag(), WithTags(), WithoutTag(), WithoutTags(), WithAnyTags()
        /// To get result call .IsMatch() or .GetMatches()
        /// </summary>
        /// <returns></returns>
        public TagFilter StartFilter() {
            return new TagFilter( this );
        }

        /// <summary>
        /// Add a tag to the tagger.
        /// </summary>
        /// <param name="neatoTagAsset">Tag to add.</param>
        public void AddTag( NeatoTagAsset neatoTagAsset ) {
            tags.Add( neatoTagAsset );
            tags = tags.ToHashSet().ToList();
        }

        /// <summary>
        /// Remove a tag from the tagger.
        /// </summary>
        /// <param name="neatoTagAsset">Tag to remove.</param>
        public void RemoveTag( NeatoTagAsset neatoTagAsset ) {
            tags.Remove( neatoTagAsset );
        }

        /// <summary>
        /// TagFilter class for chaining filter functions.
        /// Don't use directly. Use StartFilter() instead.
        /// </summary>
        public class TagFilter {
            readonly Tagger _target;
            bool _matchesFilter = true;
            readonly HashSet<GameObject> _taggedGameObjects;

            public TagFilter( Tagger target ) {
                _target = target;
                _taggedGameObjects = new HashSet<GameObject>();
            }

            /// <summary>
            /// Checks if the filter matches.
            /// </summary>
            /// <returns>Returns true if filter matches, otherwise false.</returns>
            public bool IsMatch() {
                return _matchesFilter;
            }

            /// <summary>
            /// Returns a Hashset of GameObjects that have matched the filter.
            /// </summary>
            /// <returns>Hashset of GameObjects</returns>
            public HashSet<GameObject> GetMatches() {
                return _taggedGameObjects;
            }

            /// <summary>
            /// Checks if gameobject has tag.
            /// </summary>
            /// <param name="tagAsset">Tag to check for</param>
            /// <returns></returns>
            public TagFilter WithTag( NeatoTagAsset tagAsset ) {
                _matchesFilter &= _target.HasTag( tagAsset );
                if ( _target.HasTag( tagAsset ) ) {
                    _taggedGameObjects.Add( _target.gameObject );
                } else {
                    _taggedGameObjects.Remove( _target.gameObject );
                }
                return this;
            }

            /// <summary>
            /// Checks if gameobject has all the tags in params.
            /// </summary>
            /// <param name="tags">Tags to check for</param>
            /// <returns></returns>
            public TagFilter WithTags( params NeatoTagAsset[] tags ) {
                foreach ( var tagAsset in tags ) {
                    _matchesFilter &= _target.HasTag( tagAsset );
                    if ( _target.HasTag( tagAsset ) ) {
                        _taggedGameObjects.Add( _target.gameObject );
                    } else {
                        _taggedGameObjects.Remove( _target.gameObject );
                    }
                }

                return this;
            }
            
            /// <summary>
            /// Checks if gameobject has all the tags in list.
            /// </summary>
            /// <param name="tagList">Tags to check for</param>
            /// <returns></returns>
            public TagFilter WithTags( IEnumerable<NeatoTagAsset> tagList ) {
                foreach ( var tagAsset in tagList ) {
                    _matchesFilter &= _target.HasTag( tagAsset );
                    if ( _target.HasTag( tagAsset ) ) {
                        _taggedGameObjects.Add( _target.gameObject );
                    } else {
                        _taggedGameObjects.Remove( _target.gameObject );
                    }
                }

                return this;
            }

            /// <summary>
            /// Checks if gameobject doesn't have tag.
            /// </summary>
            /// <param name="tagAsset">Tags to check for</param>
            /// <returns></returns>
            public TagFilter WithoutTag( NeatoTagAsset tagAsset ) {
                _matchesFilter &= !_target.HasTag( tagAsset );
                if ( !_target.HasTag( tagAsset ) ) {
                    _taggedGameObjects.Add( _target.gameObject );
                } else {
                    _taggedGameObjects.Remove( _target.gameObject );
                }
                return this;
            }
            
            /// <summary>
            /// Checks if gameobject doesn't have tags in params.
            /// </summary>
            /// <param name="tags">Tags to check for</param>
            /// <returns></returns>
            public TagFilter WithoutTags( params NeatoTagAsset[] tags ) {
                foreach ( var tagAsset in tags ) {
                    _matchesFilter &= !_target.HasTag( tagAsset );
                    if ( !_target.HasTag( tagAsset ) ) {
                        _taggedGameObjects.Add( _target.gameObject );
                    } else {
                        _taggedGameObjects.Remove( _target.gameObject );
                    }
                }

                return this;
            }
            
            /// <summary>
            /// Checks if gameobject doesn't have tags in list.
            /// </summary>
            /// <param name="tagList">Tags to check for</param>
            /// <returns></returns>
            public TagFilter WithoutTags( IEnumerable<NeatoTagAsset> tagList ) {
                foreach ( var tagAsset in tagList ) {
                    _matchesFilter &= !_target.HasTag( tagAsset );
                    if ( !_target.HasTag( tagAsset ) ) {
                        _taggedGameObjects.Add( _target.gameObject );
                    } else {
                        _taggedGameObjects.Remove( _target.gameObject );
                    }
                }

                return this;
            }
            
            /// <summary>
            /// Checks if gameobject has any of the tags in list.
            /// </summary>
            /// <param name="tagList">Tags to check for</param>
            /// <returns></returns>
            public TagFilter WithAnyTags( IEnumerable<NeatoTagAsset> tagList ) {
                var neatoTagAssets = tagList as NeatoTagAsset[] ?? tagList.ToArray();
                _matchesFilter &= _target.AnyTagsMatch( neatoTagAssets );
                if ( _target.AnyTagsMatch( neatoTagAssets ) ) {
                    _taggedGameObjects.Add( _target.gameObject );
                } else {
                    _taggedGameObjects.Remove( _target.gameObject );
                }
                return this;
            }
            
            /// <summary>
            /// Checks if gameobject has any of the tags in params.
            /// </summary>
            /// <param name="tags">Tags to check for</param>
            /// <returns></returns>
            public TagFilter WithAnyTags( params NeatoTagAsset[] tags ) {
                _matchesFilter &= _target.AnyTagsMatch( tags );
                if ( _target.AnyTagsMatch( tags ) ) {
                    _taggedGameObjects.Add( _target.gameObject );
                }
                return this;
            }
            
        }
    }
}