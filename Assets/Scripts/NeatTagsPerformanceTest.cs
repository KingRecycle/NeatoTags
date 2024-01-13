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
        public NeatoTag[] tags;
        
        public GameObject[] targets;
        
        int[] _spawnAmounts = { 10, 100, 1000, 5000 };
        
        List<GameObject> _spawnedObjects = new();

        void Start() {
            foreach ( var spawnAmount in _spawnAmounts ) {
                SpawnObjects( spawnAmount );
                var withTagTime = RunAction( WithTagTest, 10 );
                Debug.Log($"With {spawnAmount} objects, WithTag took {withTagTime}ms");
                
                //Destroy all spawned objects
                foreach ( var spawnedObject in _spawnedObjects ) {
                    Destroy( spawnedObject );
                }
            }
        }

        void SpawnObjects( int amount ) {
            var index = 0;
            for ( var i = 0; i < amount; i++ ) {
                if ( index >= targets.Length ) index = 0;
                var go = Instantiate( targets[index], transform );
                _spawnedObjects.Add( go );
                index++;
            }
        }
        
        //Run action a given amount of times and return the average time it took to run
        double RunAction( Action action, int amount ) {
            var sw = new Stopwatch();
            sw.Start();
            for ( var i = 0; i < amount; i++ ) {
                action();
            }
            sw.Stop();
            return sw.Elapsed.TotalMilliseconds / amount;
        }

        void WithTagTest() {
            Tagger.FilterGameObjects().WithTags( tags[0] );
        }

    }
}
