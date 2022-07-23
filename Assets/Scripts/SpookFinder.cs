using System;
using System.Collections;
using System.Collections.Generic;
using CharlieMadeAThing.NeatoTags;
using UnityEngine;

namespace CharlieMadeAThing
{
    public class SpookFinder : MonoBehaviour
    {
        public List<NeatoTagAsset> Spooks = new List<NeatoTagAsset>();

        public NeatoTagAsset tag1;
        public NeatoTagAsset tag2;
        public NeatoTagAsset tag3;

        public GameObject target;
        public GameObject target2;

        void Start()
        {
            
            Debug.Log( target.IsTagged()  );
            Debug.Log($"HasTag({tag1}): {target2.HasTag(tag1)}"  );
            Debug.Log( $"Has Any Tags matching List: {target.HasAnyTagsMatching( tag1, tag2, tag3 )} "  );
            Debug.Log( $"Has All Tags matching List: {target.HasAllTagsMatching( tag2, tag3 )} "  );
            Debug.Log( $"Has No Tags matching List: {target.HasNoTagsMatching( tag1 )} "  );
            Debug.Log( $"Chaining with TagFilter(With:{tag1.name}, With:{tag2.name}): {target.TagFilter().WithTag( tag1 ).WithTag(tag2).IsMatch()}");
            Debug.Log( $"Chaining with TagFilter[{target2}]( With:{tag1.name}, Without:{tag2.name}, With:{tag3.name} ): {target2.TagFilter().WithTag( tag1 ).WithoutTag(tag2).WithTag(tag3).IsMatch()}");
        }
        
    }
}
