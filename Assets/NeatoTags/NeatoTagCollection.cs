using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags {
    [CreateAssetMenu( fileName = "New NeatoTag Collection", menuName = "Tools/Neato Tags/Collection", order = 0 )]
    public class NeatoTagCollection : ScriptableObject {
        [SerializeField] public List<NeatoTag> tags;

        public bool HasTag( NeatoTag tag ) {
            return tags.Contains( tag );
        }
        
        public bool AnyTagsMatch( IEnumerable<NeatoTag> tagList ) {
            return tagList.Any( HasTag );
        }
        
        public bool AllTagsMatch( IEnumerable<NeatoTag> tagList ) {
            return tagList.All( HasTag );
        }
        
        public bool NoTagsMatch( IEnumerable<NeatoTag> tagList ) {
            return !tagList.Any( HasTag );
        }
    }
}