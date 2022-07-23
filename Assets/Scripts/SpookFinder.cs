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
        // Start is called before the first frame update
        void Start()
        {
            
            Debug.Log( target.IsTaggable()  );
            Debug.Log( target.IsTagged()  );
            Debug.Log( $"Has Any Tags matching SPOOKS List: {target.HasAnyTagsMatching( Spooks )} "  );
            Debug.Log( $"Has All Tags matching SPOOKS List: {target.HasAllTagsMatching( Spooks )} "  );
            Debug.Log( $"Has No Tags matching SPOOKS List: {target.HasNoTagsMatching( Spooks )} "  );
            Debug.Log( $"Chaining with TagFilter(With:{tag1.name}, With:{tag2.name}): {target.TagFilter().WithTag( tag1 ).WithTag(tag2).IsMatch()}");
            Debug.Log( $"Chaining with TagFilter(With:{tag1.name}, Without:{tag2.name}, With:{tag3.name}): {target2.TagFilter().WithTag( tag1 ).WithoutTag(tag2).WithTag(tag3).IsMatch()}");
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
