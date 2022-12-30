using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CharlieMadeAThing.NeatoTags.Core;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace CharlieMadeAThing
{
    public class NeatTagsPerformanceTest : MonoBehaviour {
        public GameObject prefab;
        public int goSpawnCount = 100000;
        public NeatoTag humanTag;
        public NeatoTag ghostTag;
        public NeatoTag wizardTag;
        public List<NeatoTag> tags;
        
        public uint iterations = 1000;
        public bool isRunning;

        readonly Stopwatch timer = new();
        TimeSpan _leastTime = TimeSpan.MaxValue;
        TimeSpan _mostTime = TimeSpan.MinValue;
        List<TimeSpan> _times = new();

        HashSet<GameObject> _results = new HashSet<GameObject>(100000);
        readonly List<GameObject> _gameObjects = new List<GameObject>(100000);
        void Start() {
            for ( var i = 0; i < goSpawnCount; i++ ) {
                var go = Instantiate(prefab, Vector3.right + new Vector3(i, 0, 0 ),  Quaternion.identity);
                go.AddTag( tags[Random.Range(0, tags.Count)] );
                _gameObjects.Add(go);
            }
        }
        
        void ResetTimer() {
            _leastTime = TimeSpan.MaxValue;
            _mostTime = TimeSpan.MinValue;
            _times.Clear();
        }

        void Update() {
            if ( Input.GetKeyDown( KeyCode.Alpha1 ) )  isRunning = true;

            if ( isRunning ) {
                CheckAllTaggerGameObjectsForTag();
                CheckAllTaggerGameObjectsForTagOldWay();
            }

        }

        void CheckForTagPerformance() {
            ResetTimer();
            for ( var i = 0; i < iterations; i++ ) {
                timer.Reset();
                timer.Start();
                _gameObjects[0].HasTag( humanTag );
                timer.Stop();
                if ( timer.Elapsed < _leastTime ) _leastTime = timer.Elapsed;
                if ( timer.Elapsed > _mostTime ) _mostTime = timer.Elapsed;
                _times.Add(timer.Elapsed);
            }
            isRunning = false;
            var average = _times.Average( t => t.Milliseconds );
            Debug.Log( $"[Performance]: Check for Tag on a GameObject x {iterations}. Min Elapsed: {_leastTime.Milliseconds} | Max Elapsed: {_mostTime.Milliseconds} | Average: {average}" );
        }
        
        void CheckAllTaggerGameObjectsForTag() {
            var allTagers = Tagger.GetAllGameObjectsWithTagger();
            ResetTimer();
            for ( var i = 0; i < iterations; i++ ) {
                timer.Reset();
                timer.Start();
                var justHumans = Tagger.FilterGameObjects().WithTag( humanTag );

                //var justHumans = Tagger.IncludesAllTags( Tagger.GetAllGameObjectsWithTagger(), humanTag );
                timer.Stop();
                if ( timer.Elapsed < _leastTime ) _leastTime = timer.Elapsed;
                if ( timer.Elapsed > _mostTime ) _mostTime = timer.Elapsed;
                _times.Add(timer.Elapsed);
            }
            isRunning = false;
            var average = _times.Average( t => t.Milliseconds );
            Debug.Log( $"[Performance]: Check for Tag on {goSpawnCount} gameObjects x {iterations}. Min Elapsed: {_leastTime}ms | Max Elapsed: {_mostTime}ms | Average: {average}ms" );
        }
        
        void CheckAllTaggerGameObjectsForTagOldWay() {
            ResetTimer();
            for ( var i = 0; i < iterations; i++ ) {
                timer.Reset();
                timer.Start();
                var justHumans = Tagger.FilterGameObjects().WithTags( humanTag ).GetMatches();
                timer.Stop();
                if ( timer.Elapsed < _leastTime ) _leastTime = timer.Elapsed;
                if ( timer.Elapsed > _mostTime ) _mostTime = timer.Elapsed;
                _times.Add(timer.Elapsed);
            }
            isRunning = false;
            var average = _times.Average( t => t.Milliseconds );
            Debug.Log( $"[Performance]: (OLD WAY) Check for Tag on {goSpawnCount} gameObjects x {iterations}. Min Elapsed: {_leastTime}ms | Max Elapsed: {_mostTime}ms | Average: {average}ms" );
        }

    }
}
