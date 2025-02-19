using System.Collections.Generic;
using System.Text;
using CharlieMadeAThing.NeatoTags.Core;
using TMPro;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Demo {
    public class SpookFinder : MonoBehaviour {
        public List<NeatoTag> spookerTags;

        public NeatoTag humanTag;
        public NeatoTag ghostTag;
        public NeatoTag goblinTag;
        public NeatoTag witchTag;
        [SerializeField] List<GameObject> spookyGameObjects;

        //UI
        [SerializeField] TextMeshProUGUI tmpText;

        readonly HashSet<GameObject> _spooksInRange = new();


        void Start() {
            //To filter out a list of Gameobjects, we can use the static function Tagger.StartGameObjectFilter()
            //We can pass in our own list of GameObjects to filter or leave it empty to filter all GameObjects that have a tagger in the scene.
            //Any GameObjects that do not have a Tagger Component will be ignored.
            var allSpooks = Tagger.FilterGameObjects().WithTags( spookerTags ).GetMatches();
            var humans = Tagger.FilterGameObjects().WithTag( humanTag ).WithoutTags( witchTag, goblinTag, ghostTag )
                .GetMatches();
            var ghosts = Tagger.FilterGameObjects( spookyGameObjects ).WithTag( ghostTag ).GetMatches();
            
            //Use .FilterGameObjects() for cleaner looking function 
            var ghostAndHumans = Tagger.FilterGameObjects( spookyGameObjects ).WithAnyTags( ghostTag, humanTag ).GetMatches();

            //Use direct reference to tag (recommended) or use the tag name (not recommended)
            Debug.Log( "Reference by name: " + spookyGameObjects[0].HasTag( "Ghost" ) );
            Debug.Log( "Reference by tag: " + spookyGameObjects[0].HasTag( ghostTag ) );
            
            
            
            
            //We can also use the static function Tagger.GetAllGameObjectsWithTagger() to get all GameObjects with a Tagger Component
            //This is useful if you want to filter all GameObjects in the scene
            var allGameObjects = Tagger.GetAllGameObjectsWithTagger();
        }

        void Update() {
            if ( Input.GetKey( KeyCode.W ) ) {
                transform.Translate( Vector3.forward * ( 2f * Time.deltaTime ) );
            }

            if ( Input.GetKey( KeyCode.S ) ) {
                transform.Translate( Vector3.back * ( 2f * Time.deltaTime ) );
            }

            if ( Input.GetKey( KeyCode.A ) ) {
                transform.Translate( Vector3.left * ( 2f * Time.deltaTime ) );
            }

            if ( Input.GetKey( KeyCode.D ) ) {
                transform.Translate( Vector3.right * ( 2f * Time.deltaTime ) );
            }

            var sb = new StringBuilder();
            foreach ( var spook in _spooksInRange ) {
                sb.Append( spook.name + " " );
            }

            tmpText.text = sb.ToString();
        }

        void OnTriggerEnter( Collider other ) {
            var potentialSpook = other.gameObject;
            //Check if a gameobject has a Tagger component
            //GameObjects without a Tagger component are considered to not have any tags (HasTags() returns false) and (!HasTag() returns true)
            //and won't show up in the list of tagged objects.
            //So if you want to be sure you are only checking tagged objects, you can use HasTagger()
            if ( !potentialSpook.HasTagger() ) return;

            //HasTag only cares about the specified tag.
            //In this example the Witch gameobject which has the Witch and Human tag will also return true.
            if ( potentialSpook.HasTag( humanTag ) ) {
                Debug.Log( "Human spotted...maybe?" );
            }

            //Pass a list of tags to check against
            //When checking for any tags it does not have to be all tags but any GameObject with one of the tags will be returned as true.
            //use HasAllTagsMatching() to check for if ALL the tags are present.
            if ( potentialSpook.HasAnyTagsMatching( spookerTags ) ) {
                _spooksInRange.Add( potentialSpook );
            }

            //Start a filter and chain functions to it.
            //IsMatch() returns true if filter is true, otherwise false.
            //FilterTags().WithTag( humanTag ).WithoutTags( spookerTags ).IsMatch() is the same as checking 
            //gameObject.HasTag( humanTag ) && !gameObject.HasAnyTagsMatching( spookerTags ) but is cleaner to read ( maybe ;P )
            if ( potentialSpook.FilterTags().WithTag( humanTag ).WithoutTags( spookerTags ).IsMatch() ) {
                Debug.Log( "Found human, truly!" );
            }

            //If you have a small list of tags to search then instead of passing a list just add each tag to the function seperated by commas.
            var filter = potentialSpook.FilterTags().WithAnyTags( ghostTag, goblinTag, witchTag );
            if ( filter.IsMatch() ) {
                Debug.Log( "Found a Spook!" );
            }
        }

        void OnTriggerExit( Collider other ) {
            if ( _spooksInRange.Contains( other.gameObject ) ) {
                _spooksInRange.Remove( other.gameObject );
            }
        }
    }
}