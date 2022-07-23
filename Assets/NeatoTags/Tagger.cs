using System.Collections;
using System.Collections.Generic;
using CharlieMadeAThing.NeatoTags;
using UnityEngine;

namespace CharlieMadeAThing
{
    public class Tagger : MonoBehaviour {
        [SerializeField] NeatoTagCollection tagCollection;
        public NeatoTagCollection TagCollection => tagCollection;
    }
}
