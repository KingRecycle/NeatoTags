using System.Collections.Generic;
using System.Text;
using CharlieMadeAThing.NeatoTags.Core;
using TMPro;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Demo {
    public class SpookFinder : MonoBehaviour {
        public List<NeatoTagAsset> spookerTags;

        public NeatoTagAsset human;
        public NeatoTagAsset ghost;
        public NeatoTagAsset goblin;
        public NeatoTagAsset witch;
        [SerializeField] List<GameObject> spooksGotFromFilter = new();

        //UI
        [SerializeField] TextMeshProUGUI tmpText;

        readonly HashSet<GameObject> _spooksInRange = new();


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
            //Check if a gameobject has a Tagger component
            if ( !other.gameObject.IsTagged() ) return;
            
            //Pass a list of tags to check against
            if ( other.gameObject.HasAnyTagsMatching( spookerTags ) ) {
                _spooksInRange.Add( other.gameObject );
            }
            //Start a filter and chain functions to it.
            if ( other.gameObject.TagFilter().WithTag( human ).WithoutTags( spookerTags ).IsMatch() ) {
                Debug.Log( "Found human" );
            }
            //If you have a small list of tags to search then instead of passing a list just add each tag to the function seperated by commas.
            var filter = other.gameObject.TagFilter().WithAnyTags( ghost, goblin, witch );
            if ( filter != null && filter.IsMatch() ) {
                var spookies = filter.GetMatches(); // GetMatches returns a list of GameObjects that match the filter

                foreach ( var spooky in spookies ) {
                    if ( spooksGotFromFilter.Contains( spooky ) ) continue;
                    spooksGotFromFilter.Add( spooky );
                    Debug.Log( "Found spooky: " + spooky.name );
                }
            }
        }

        void OnTriggerExit( Collider other ) {
            if ( _spooksInRange.Contains( other.gameObject ) ) {
                _spooksInRange.Remove( other.gameObject );
            }
        }
    }
}