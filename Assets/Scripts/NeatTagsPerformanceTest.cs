using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using CharlieMadeAThing.NeatoTags.Core;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace CharlieMadeAThing
{
    public class NeatTagsPerformanceTest : MonoBehaviour {
        public GameObject prefab;
        public int count = 1000;
        public NeatoTag humanTag;
        public List<NeatoTag> tags;
        Stopwatch timer = new();
        void Start() {
            for ( var i = 0; i < count; i++ ) {
                var go = Instantiate(prefab, Vector3.right + new Vector3(i, 0, 0 ),  Quaternion.identity);
                //go.AddTag( tags[Random.Range(0, tags.Count)] );
            }
        }

        void Update() {
            if ( Input.GetKeyDown( KeyCode.Space ) ) {
                timer.Reset();
                timer.Start();
                GetAllGameObjectsWithHumanTag();
                timer.Stop();
                Debug.Log( $"Elapsed: {timer.Elapsed} -- ms: {timer.ElapsedMilliseconds}" );
            }
        }

        void GetAllGameObjectsWithHumanTag() {
            var go = Tagger.StartGameObjectFilter().WithTag( humanTag ).GetMatches();
            Debug.Log( go.Count);
        }
    }
}
