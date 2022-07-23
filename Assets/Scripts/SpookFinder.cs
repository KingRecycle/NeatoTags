using System.Collections;
using System.Collections.Generic;
using CharlieMadeAThing.NeatoTags;
using UnityEngine;

namespace CharlieMadeAThing
{
    public class SpookFinder : MonoBehaviour
    {
        public List<NeatoTag> Spooks = new List<NeatoTag>();

        public GameObject target;
        // Start is called before the first frame update
        void Start()
        {
            Debug.Log( target.IsTaggable()  );
            Debug.Log( target.IsTagged()  );
            Debug.Log( $"Has Any Tags matching SPOOKS List: {target.HasAnyTagsMatching( Spooks )} "  );
            Debug.Log( $"Has All Tags matching SPOOKS List: {target.HasAllTagsMatching( Spooks )} "  );
            Debug.Log( $"Has No Tags matching SPOOKS List: {target.HasNoTagsMatching( Spooks )} "  );
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
