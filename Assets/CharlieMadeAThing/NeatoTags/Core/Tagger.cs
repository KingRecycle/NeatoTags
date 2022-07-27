using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Core
{
    /// <summary>
    /// Holds the tags for a given gameobject.
    /// </summary>
    [System.Serializable]
    public class Tagger : MonoBehaviour {
        [SerializeField] List<NeatoTagAsset> tags = new();
        
        static Dictionary<GameObject, Tagger> _taggers = new();
        
        [SerializeField] List<NeatoTagAsset> _allTags = new();
        
        
        public static HashSet<NeatoTagAsset> GetAllTags() {
            var tagSet = new HashSet<NeatoTagAsset>();
            string[] guids = AssetDatabase.FindAssets( "t:NeatoTagAsset" );
            foreach ( var guid in guids ) {
                var path = AssetDatabase.GUIDToAssetPath( guid );
                var tagAsset = AssetDatabase.LoadAssetAtPath<NeatoTagAsset>( path );
                tagSet.Add( tagAsset );
            }

            return tagSet;
        }

        public List<NeatoTagAsset> GetTags => tags;


        void OnEnable() {
            _taggers.Add( gameObject, this );
        }
        
        void OnDisable() {
            _taggers.Remove( gameObject );
        }
        
        
        /// <summary>
        /// Checks if a gameobject has a Tagger component.
        /// </summary>
        /// <param name="GameObject">Gameobject to check</param>
        /// <returns>Returns true if Gameobject has a Tagger component, false if not.</returns>
        public static bool IsTagged(GameObject go) => _taggers.ContainsKey( go );
        
        /// <summary>
        /// Outs the Tagger component if it has one.
        /// </summary>
        /// <param name="go">Gameobject to check</param>
        /// <param name="tagger">Gameobject's Tagger component</param>
        /// <returns>Returns true if Gameobject has a Tagger component, otherwise false.</returns>
        public static bool TryGetTagger( GameObject go, out Tagger tagger ) {
            return _taggers.TryGetValue( go, out tagger );
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
        /// <param name="tagAsset">The tag to check for.</param>
        /// <returns>Returns true if Tagger has the tag, otherwise false.</returns>
        public bool HasTag( NeatoTagAsset tagAsset ) {
            return tags.Contains( tagAsset );
        }
        
        /// <summary>
        /// Checks if Tagger has any of the tags in the list.
        /// </summary>
        /// <param name="tagList">IEnumerable of tags.</param>
        /// <returns>Returns true if Tagger has any of the tags, otherwise false.</returns>
        public bool AnyTagsMatch( IEnumerable<NeatoTagAsset> tagList ) {
            return tagList.Any( HasTag );
        }
        
        /// <summary>
        /// Checks if all of the tags in the list are in the Tagger.
        /// </summary>
        /// <param name="tagList">IEnumerable of tags.</param>
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
        /// Starts a filter for chaining filter functions.
        /// WithTag(), WithoutTag()
        /// To get result call .IsMatch()
        /// </summary>
        /// <returns>Returns true if all filters match, otherwise false.</returns>
        public TagFilter StartFilter() {
            return new TagFilter( this );
        }


         public class TagFilter {
            readonly Tagger _target;
            bool _matchesFilter = true;

            public TagFilter( Tagger target ) {
                this._target = target;
            }
            
            public bool IsMatch() {
                return _matchesFilter;
            }
            
            public TagFilter WithTag( NeatoTagAsset tagAsset ) {
                _matchesFilter &= _target.HasTag( tagAsset );
                return this;
            }
            
            public TagFilter WithoutTag( NeatoTagAsset tagAsset ) {
                _matchesFilter &= !_target.HasTag( tagAsset );
                return this;
            }
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
    }
}
