
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags {
    [CreateAssetMenu( fileName = "New NeatoTag", menuName = "Tools/Neato Tags/New Tag", order = 0 )]
    [System.Serializable]
    public class NeatoTag : ScriptableObject {
        public Color color;
    }
}