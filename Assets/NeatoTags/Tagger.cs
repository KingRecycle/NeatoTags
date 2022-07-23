using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CharlieMadeAThing.NeatoTags;
using UnityEngine;
using YamlDotNet.Core.Tokens;

namespace CharlieMadeAThing
{
    [System.Serializable]
    public class Tagger : MonoBehaviour {
        [SerializeField] NeatoTagCollection tagCollection;
        public NeatoTagCollection TagCollection => tagCollection;
        
        static Dictionary<GameObject, Tagger> _taggers = new();

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

        public bool HasTag( NeatoTagAsset tagAsset ) {
            return tagCollection.tags.Contains( tagAsset );
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
