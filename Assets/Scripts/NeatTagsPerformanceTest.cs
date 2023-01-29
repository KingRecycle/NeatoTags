using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CharlieMadeAThing.NeatoTags.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace CharlieMadeAThing
{
    public class NeatTagsPerformanceTest : MonoBehaviour {
        public NeatoTag humanTag;
        public string unityTag;
        public GameObject target;

        public uint iterations = 100000;


        [Button("Neato Tag Test")]
        void NeatoTagTest() {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            for ( var i = 0; i < iterations; i++ ) {
                target.HasTag( humanTag );
            }
            stopwatch.Stop();
            Debug.Log( $"NeatoTagTest ( {iterations} iterations ) took: {stopwatch.ElapsedMilliseconds}ms" );
        }

        [Button("Unity Tag Test")]
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
