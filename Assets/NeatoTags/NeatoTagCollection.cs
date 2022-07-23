using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags {
    [CreateAssetMenu( fileName = "New NeatoTag Collection", menuName = "Tools/Neato Tags/Collection", order = 0 )]
    public class NeatoTagCollection : ScriptableObject {
        [SerializeField] public List<NeatoTagAsset> tags;

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
            NeatoTagCollection target;
            bool _matchesFilter;
            public TagFilter( NeatoTagCollection target ) {
                this.target = target;
            }
            
            public bool IsMatch() {
                return _matchesFilter;
            }
            
            public TagFilter WithTag( NeatoTagAsset tagAsset ) {
                _matchesFilter = target.HasTag( tagAsset );
                return this;
            }
            
            public TagFilter WithoutTag( NeatoTagAsset tagAsset ) {
                _matchesFilter = !target.HasTag( tagAsset );
                return this;
            }
        }
        
    }

    
}