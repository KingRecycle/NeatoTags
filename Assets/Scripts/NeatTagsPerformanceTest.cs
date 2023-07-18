using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CharlieMadeAThing.NeatoTags.Core;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace CharlieMadeAThing
{
    public class NeatTagsPerformanceTest : MonoBehaviour {
        public NeatoTag nTag;
        public string unityTag;
        public GameObject target;
        
        public uint spawnCount = 1000;
        public uint iterations = 100000;
        
        List<GameObject> spawnedObjects = new();

        void Awake() {
            for ( var i = 0; i < spawnCount; i++ ) {
                var obj = new GameObject( $"TestObject {i}" ) {
                    transform = {
                        position = new Vector3( Random.Range( -100, 100 ), Random.Range( -100, 100 ), Random.Range( -100, 100 ) ),
                    },
                };
                spawnedObjects.Add( obj );
            }
        }


        void NeatoTagTest() {
            for ( var i = 0; i < iterations; i++ ) {
                target.HasTag( nTag );
            }
        }
        
        void UnityTagTest() {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            for ( var i = 0; i < iterations; i++ ) {
                target.CompareTag( unityTag );
            }
            stopwatch.Stop();
            Debug.Log( $"UnityTagTest ( {iterations} iterations ) took: {stopwatch.ElapsedMilliseconds}ms" );
        }
    }
}
