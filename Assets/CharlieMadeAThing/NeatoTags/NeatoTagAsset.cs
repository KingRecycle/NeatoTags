using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CharlieMadeAThing.NeatoTags {
    [CreateAssetMenu( fileName = "New NeatoTag", menuName = "Tools/Neato Tags/New Tag", order = 0 )]
    [System.Serializable]
    public class NeatoTagAsset : ScriptableObject {
        public Color color;
        
    }
}