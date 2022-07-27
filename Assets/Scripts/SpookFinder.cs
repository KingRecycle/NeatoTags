using CharlieMadeAThing.NeatoTags.Core;
using UnityEngine;

namespace CharlieMadeAThing {
    public class SpookFinder : MonoBehaviour {
        public Sprite spookSprite;
        public NeatoTagAsset tag1;
        public NeatoTagAsset tag2;
        public NeatoTagAsset tag3;

        public GameObject target;
        public GameObject target2;

        void Start() {
            Debug.Log( target.IsTagged() );
            Debug.Log( $"HasTag({tag1})[{target2.name}]: {target2.HasTag( tag1 )}" );
            Debug.Log( $"Has Any Tags matching List[{target.name}]: {target.HasAnyTagsMatching( tag1, tag2, tag3 )} " );
            Debug.Log( $"Has All Tags matching List[{target.name}]: {target.HasAllTagsMatching( tag2, tag3 )} " );
            Debug.Log( $"Has No Tags matching List[{target.name}]: {target.HasNoTagsMatching( tag1 )} " );
            Debug.Log(
                $"Chaining with TagFilter(With:{tag1.name}, With:{tag2.name}): {target.TagFilter().WithTag( tag1 ).WithTag( tag2 ).IsMatch()}" );
            Debug.Log(
                $"Chaining with TagFilter[{target2}]( With:{tag1.name}, Without:{tag2.name}, With:{tag3.name} ): {target2.TagFilter().WithTag( tag1 ).WithoutTag( tag2 ).WithTag( tag3 ).IsMatch()}" );
        }
    }
}