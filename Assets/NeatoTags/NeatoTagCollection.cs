using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags {
    [CreateAssetMenu( fileName = "New NeatoTag Collection", menuName = "Tools/Neato Tags/Collection", order = 0 )]
    public class NeatoTagCollection : ScriptableObject {
        [SerializeField] public List<NeatoTagAsset> tags;

    }

    
}