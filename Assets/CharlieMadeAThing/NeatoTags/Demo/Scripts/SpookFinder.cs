using System;
using System.Collections.Generic;
using System.Linq;
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
        
        HashSet<GameObject> _spooksInRange = new HashSet<GameObject>();
        [SerializeField]List<GameObject> spooksGotFromFilter = new List<GameObject>();
        
        //UI
        [SerializeField] TextMeshProUGUI tmpText;
        

        void Update() {
            if( Input.GetKey( KeyCode.W ) ) {
                transform.Translate( Vector3.forward * (2f * Time.deltaTime) );
            }
            if( Input.GetKey( KeyCode.S ) ) {
                transform.Translate( Vector3.back * (2f * Time.deltaTime) );
            }
            if( Input.GetKey( KeyCode.A ) ) {
                transform.Translate( Vector3.left * (2f * Time.deltaTime) );
            }
            if( Input.GetKey( KeyCode.D ) ) {
                transform.Translate( Vector3.right * (2f * Time.deltaTime) );
            }

            var sb = new StringBuilder();
            foreach ( var spook in _spooksInRange ) {
                sb.Append( spook.name + " " );
            }

            tmpText.text = sb.ToString();
        }

        void OnTriggerEnter( Collider other ) {
            if ( !other.gameObject.IsTagged() ) return;
            
            if ( other.gameObject.HasAnyTagsMatching( spookerTags ) ) {
                _spooksInRange.Add( other.gameObject );
            }

            if( other.gameObject.TagFilter().WithTag( human ).WithoutTags( spookerTags ).IsMatch() ) {
                Debug.Log( "Found human" );
            }

            var filter = other.gameObject.TagFilter().WithAnyTags( ghost, goblin, witch );
            if ( filter != null && filter.IsMatch() ) {
                var spookies = filter.GetMatches();
                
                foreach ( var spooky in spookies ) {
                    if( spooksGotFromFilter.Contains( spooky ) ) continue;
                    spooksGotFromFilter.Add( spooky );
                    Debug.Log( "Found spooky: " + spooky.name );
                }
            }


        }

        void OnTriggerExit( Collider other ) {
            if( _spooksInRange.Contains( other.gameObject ) ) {
                _spooksInRange.Remove( other.gameObject );
            }
        }
    }
}