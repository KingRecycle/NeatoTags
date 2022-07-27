using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CharlieMadeAThing.NeatoTags.Core {
    [CreateAssetMenu( fileName = "New NeatoTag", menuName = "Neato Tags/New Tag", order = 0 )]
    [System.Serializable]
    public class NeatoTagAsset : ScriptableObject {
        [SerializeField] Color color = Color.black;
        [SerializeField] string comment = string.Empty;
        
         public Color Color => color;
         public string Comment => comment;
    }
}