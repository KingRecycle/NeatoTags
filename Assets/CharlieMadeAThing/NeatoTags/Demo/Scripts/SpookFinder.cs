using System;
using System.Collections.Generic;
using System.Text;
using CharlieMadeAThing.NeatoTags.Core;
using TMPro;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Demo {
    public class SpookFinder : MonoBehaviour {
        public List<NeatoTagAsset> spookerTags;

        public NeatoTagAsset humanTag;
        public NeatoTagAsset ghostTag;
        public NeatoTagAsset goblinTag;
        public NeatoTagAsset witchTag;
        [SerializeField] List<GameObject> spooksGotFromFilter = new();

        //UI
        [SerializeField] TextMeshProUGUI tmpText;

        readonly HashSet<GameObject> _spooksInRange = new();


        void Start() {
            //To filter out a list of Gameobjects, we can use the static function Tagger.StartGameObjectFilter()
            //We can pass in our own list of GameObjects to filter or leave it empty to filter all GameObjects that have a tagger in the scene.
            var allSpooks = Tagger.StartGameObjectFilter().WithTags( spookerTags ).GetMatches();
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
            if ( !potentialSpook.IsTagged() ) return;
            
            if( potentialSpook.HasTag( humanTag ) ) {
                Debug.Log( "Human spotted...maybe?" );
            }
            
            //Pass a list of tags to check against
            if ( potentialSpook.HasAnyTagsMatching( spookerTags ) ) {
                _spooksInRange.Add( potentialSpook );
            }
            //Start a filter and chain functions to it.
            //IsMatch() returns true if filter is true, otherwise false.
            //StartTagFilter().WithTag( humanTag ).WithoutTags( spookerTags ).IsMatch() is the same as checking 
            //gameObject.HasTag( humanTag ) && !gameObject.HasAnyTagsMatching( spookerTags ) but is cleaner to read.
            if ( potentialSpook.StartTagFilter().WithTag( humanTag ).WithoutTags( spookerTags ).IsMatch() ) {
                Debug.Log( "Found human, truly!" );
            }
            //If you have a small list of tags to search then instead of passing a list just add each tag to the function seperated by commas.
            var filter = potentialSpook.StartTagFilter().WithAnyTags( ghostTag, goblinTag, witchTag );
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