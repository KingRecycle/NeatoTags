using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags
{
    [System.Serializable]
    public class Tagger : MonoBehaviour {
        // [SerializeField] NeatoTagCollection tagCollection;
        // public NeatoTagCollection TagCollection => tagCollection;
        
        [SerializeField] List<NeatoTagAsset> tags = new();
        
        static Dictionary<GameObject, Tagger> _taggers = new();
        
        [SerializeField] List<NeatoTagAsset> _allTags = new();
        

        
        /// <summary>
        /// Updates the tagger's tag collection.
        /// This should be called whenever the tag collection is changed.
        /// Does not need to be called manually.
        /// </summary>
       public void OnValidate() {
            _allTags.Clear();
            string[] guids = AssetDatabase.FindAssets( "t:NeatoTagAsset" );
            foreach ( var guid in guids ) {
                var path = AssetDatabase.GUIDToAssetPath( guid );
                var tagAsset = AssetDatabase.LoadAssetAtPath<NeatoTagAsset>( path );
                if( _allTags.Contains( tagAsset ) ) continue;
                _allTags.Add( tagAsset );
            }
            
            var deDupe = new HashSet<NeatoTagAsset>();
            foreach ( var neatoTag in tags ) {
                deDupe.Add( neatoTag );
            }
            tags.Clear();
            deDupe.Remove( null );
            tags.AddRange( deDupe );
        }
        


        void OnEnable() {
            _taggers.Add( gameObject, this );
        }
        
        void OnDisable() {
            _taggers.Remove( gameObject );
        }
        
        
        
        public static bool IsTagged(GameObject go) => _taggers.ContainsKey( go );
        
        public static bool TryGetTagger( GameObject go, out Tagger tagger ) {
            return _taggers.TryGetValue( go, out tagger );
        }

        public static Dictionary<GameObject, Tagger> GetTagged() {
            return _taggers;
        }

        public bool HasTag( NeatoTagAsset tagAsset ) {
            return tags.Contains( tagAsset );
        }
        
        public bool AnyTagsMatch( IEnumerable<NeatoTagAsset> tagList ) {
            return tagList.Any( HasTag );
        }
        
        public bool AllTagsMatch( IEnumerable<NeatoTagAsset> tagList ) {
            return tagList.All( HasTag );
        }
        
        public bool NoTagsMatch( IEnumerable<NeatoTagAsset> tagList ) {
            return !tagList.Any( HasTag );
        }

        public TagFilter Filter() {
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
    }
}
