using System;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Core {
    [CreateAssetMenu( fileName = "New NeatoTag", menuName = "Neato Tags/New Tag", order = 0 )]
    [Serializable]
    public class NeatoTag : ScriptableObject {
        [SerializeField] Color color = Color.gray;
        [SerializeField] string comment = string.Empty;
        

        public Color Color {
            get => color;
            set => color = value;
        }

        public string Comment {
            get => comment;
            set => comment = value;
        }
        
        
    }
}