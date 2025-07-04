using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CharlieMadeAThing.NeatoTags.Core;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace CharlieMadeAThing.NeatoTags.Tests.PlayMode {
    [TestFixture]
    public class BasicPerformanceTests : NeatoTagTests {
        const int MeasurementCount = 10000;

        [UnityTest, Performance]
        public IEnumerator HasTag_Performance() {
            Measure.Method( () => { Cube.HasTag( "Cube" ); } )
                .WarmupCount( 10 )
                .MeasurementCount( MeasurementCount )
                .Run();

            yield return null;
        }

        [UnityTest, Performance]
        public IEnumerator FilterTags_WithMultipleConditions_Performance() {
            Measure.Method( () => {
                    Cube.FilterTags()
                        .WithTag( TagRefsForTests.cubeTag )
                        .WithTag( TagRefsForTests.platonicTag )
                        .WithoutTag( TagRefsForTests.sphereTag )
                        .IsMatch();
                } )
                .WarmupCount( 10 )
                .MeasurementCount( MeasurementCount )
                .Run();

            yield return null;
        }

        [UnityTest, Performance]
        public IEnumerator SimpleFilterGameObjects_Performance() {
            Measure.Method( () => {
                    Tagger.FilterGameObjects( ShapeTagsToFilter )
                        .WithTag( TagRefsForTests.cornerlessTag )
                        .WithoutTag( TagRefsForTests.sphereTag )
                        .GetMatches();
                } )
                .WarmupCount( 10 )
                .MeasurementCount( MeasurementCount )
                .Run();

            yield return null;
        }
    }

    [TestFixture]
    public class FilterPerformanceTests : NeatoTagTests {
        const int WarmupCount = 5;
        const int MeasurementCount = 100;
        List<GameObject> _testObjects1;
        List<GameObject> _testObjects10;
        List<GameObject> _testObjects100;
        List<GameObject> _testObjects1000;
        List<GameObject> _testObjects10000;

        [UnitySetUp]
        public new IEnumerator SetUp() {
            yield return base.SetUp();

            // Create test objects with different scales
            _testObjects1 = CreateTaggedObjects( 1 );
            _testObjects10 = CreateTaggedObjects( 10 );
            _testObjects100 = CreateTaggedObjects( 100 );
            _testObjects1000 = CreateTaggedObjects( 1000 );
            _testObjects10000 = CreateTaggedObjects( 10000 );
        }

        [UnityTearDown]
        public IEnumerator TearDown() {
            // Clean up all created objects
            DestroyTaggedObjects( _testObjects1 );
            DestroyTaggedObjects( _testObjects10 );
            DestroyTaggedObjects( _testObjects100 );
            DestroyTaggedObjects( _testObjects1000 );
            DestroyTaggedObjects( _testObjects10000 );
            yield return null;
        }

        List<GameObject> CreateTaggedObjects( int count ) {
            var result = new List<GameObject>( count );

            for ( var i = 0; i < count; i++ ) {
                var obj = new GameObject( $"TestObject_{count}_{i}" );
                var tagger = obj.AddComponent<Tagger>();

                // Distribute tags with different patterns to create realistic filtering scenarios
                if ( i % 2 == 0 ) tagger.AddTag( TagRefsForTests.cubeTag );
                if ( i % 3 == 0 ) tagger.AddTag( TagRefsForTests.sphereTag );
                if ( i % 7 == 0 ) tagger.AddTag( TagRefsForTests.capsuleTag );
                if ( i % 11 == 0 ) tagger.AddTag( TagRefsForTests.platonicTag );
                if ( i % 13 == 0 ) tagger.AddTag( TagRefsForTests.cornerlessTag );
                if ( i % 16 == 0 ) tagger.AddTag( TagRefsForTests.cubeTag );
                if ( i % 16 == 0 ) tagger.AddTag( TagRefsForTests.platonicTag );

                result.Add( obj );
            }

            return result;
        }

        void DestroyTaggedObjects( List<GameObject> objects ) {
            if ( objects == null ) return;

            foreach ( var obj in objects ) {
                if ( obj != null )
                    Object.Destroy( obj );
            }
        }

        [UnityTest, Performance]
        public IEnumerator FilterGameObjects_1_Performance() {
            Measure.Method( () => {
                    Tagger.FilterGameObjects( _testObjects1 )
                        .WithTag( TagRefsForTests.cubeTag )
                        .GetMatches();
                } )
                .WarmupCount( WarmupCount )
                .MeasurementCount( MeasurementCount )
                .Run();

            yield return null;
        }

        [UnityTest, Performance]
        public IEnumerator FilterGameObjects_10_Performance() {
            Measure.Method( () => {
                    Tagger.FilterGameObjects( _testObjects10 )
                        .WithTag( TagRefsForTests.cubeTag )
                        .GetMatches();
                } )
                .WarmupCount( WarmupCount )
                .MeasurementCount( MeasurementCount )
                .Run();

            yield return null;
        }

        [UnityTest, Performance]
        public IEnumerator FilterGameObjects_100_Performance() {
            Measure.Method( () => {
                    Tagger.FilterGameObjects( _testObjects100 )
                        .WithTag( TagRefsForTests.cubeTag )
                        .GetMatches();
                } )
                .WarmupCount( WarmupCount )
                .MeasurementCount( MeasurementCount )
                .Run();

            yield return null;
        }

        [UnityTest, Performance]
        public IEnumerator FilterGameObjects_1000_Performance() {
            Measure.Method( () => {
                    Tagger.FilterGameObjects( _testObjects1000 )
                        .WithTag( TagRefsForTests.cubeTag )
                        .GetMatches();
                } )
                .WarmupCount( WarmupCount )
                .MeasurementCount( MeasurementCount )
                .Run();

            yield return null;
        }

        [UnityTest, Performance]
        public IEnumerator FilterGameObjects_10000_Performance() {
            Measure.Method( () => {
                    Tagger.FilterGameObjects( _testObjects10000 )
                        .WithTag( TagRefsForTests.cubeTag )
                        .GetMatches();
                } )
                .WarmupCount( WarmupCount )
                .MeasurementCount( MeasurementCount )
                .Run();

            yield return null;
        }

        [UnityTest, Performance]
        public IEnumerator FilterGameObjects_Complex_1000_Performance() {
            Measure.Method( () => {
                    Tagger.FilterGameObjects( _testObjects1000 )
                        .WithTag( TagRefsForTests.cubeTag )
                        .WithoutTag( TagRefsForTests.sphereTag )
                        .WithAnyTags( TagRefsForTests.platonicTag, TagRefsForTests.cornerlessTag )
                        .GetMatches();
                } )
                .WarmupCount( WarmupCount )
                .MeasurementCount( MeasurementCount )
                .Run();

            yield return null;
        }

        [UnityTest, Performance]
        public IEnumerator FilterGameObjects_MultipleConditions_1000_Performance() {
            Measure.Method( () => {
                    Tagger.FilterGameObjects( _testObjects1000 )
                        .WithTag( TagRefsForTests.cubeTag )
                        .WithTag( TagRefsForTests.platonicTag )
                        .WithoutTag( TagRefsForTests.sphereTag )
                        .GetMatches();
                } )
                .WarmupCount( WarmupCount )
                .MeasurementCount( MeasurementCount )
                .Run();

            yield return null;
        }
    }

    [TestFixture]
    public class MemoryPerformanceTests : NeatoTagTests {
        const int WarmupCount = 5;
        const int MeasurementCount = 100;
        List<GameObject> _testObjects1000;

        [UnitySetUp]
        public new IEnumerator SetUp() {
            yield return base.SetUp();
            _testObjects1000 = CreateTaggedObjects( 1000 );
        }

        List<GameObject> CreateTaggedObjects( int amount ) {
            var result = new List<GameObject>( amount );
            for ( var i = 0; i < amount; i++ ) {
                var obj = new GameObject( $"TestObject_{amount}_{i}" );
                var tagger = obj.AddComponent<Tagger>();
                if ( i % 2 == 0 ) tagger.AddTag( TagRefsForTests.cubeTag );
                if ( i % 3 == 0 ) tagger.AddTag( TagRefsForTests.sphereTag );
                if ( i % 7 == 0 ) tagger.AddTag( TagRefsForTests.capsuleTag );
                if ( i % 11 == 0 ) tagger.AddTag( TagRefsForTests.platonicTag );
                if ( i % 13 == 0 ) tagger.AddTag( TagRefsForTests.cornerlessTag );
                if ( i % 16 == 0 ) tagger.AddTag( TagRefsForTests.cubeTag );
                if ( i % 16 == 0 ) tagger.AddTag( TagRefsForTests.platonicTag );
                if ( i % 16 == 0 ) tagger.AddTag( TagRefsForTests.capsuleTag );
                result.Add( obj );
            }

            return result;
        }

        [UnityTearDown]
        public IEnumerator TearDown() {
            DestroyTaggedObjects( _testObjects1000 );
            yield return null;
        }

        void DestroyTaggedObjects( List<GameObject> objectsToDestroy ) {
            foreach ( var obj in objectsToDestroy.Where( obj => obj ) ) {
                Object.Destroy( obj );
            }
        }

        [UnityTest, Performance]
        public IEnumerator CompareOptimizations() {
            var tags = new[] {
                TagRefsForTests.cubeTag,
                TagRefsForTests.platonicTag,
                TagRefsForTests.sphereTag,
            };

            var filter = Tagger.FilterGameObjects( _testObjects1000 );

            // Current implementation
            Measure.Method( () => { filter.WithTags( tags ).GetMatches(); } )
                .WarmupCount( WarmupCount )
                .MeasurementCount( MeasurementCount )
                .SampleGroup( "Current" )
                .Run();

            yield return null;
        }

        [UnityTest, Performance]
        public IEnumerator Compare_WithAnyTags_Implementation() {
            var tags = new[] {
                TagRefsForTests.cubeTag,
                TagRefsForTests.platonicTag,
                TagRefsForTests.sphereTag,
            };

            var filter = Tagger.FilterGameObjects( _testObjects1000 );

            // Current WithAnyTags implementation
            Measure.Method( () => { filter.WithAnyTags( tags ).GetMatches(); } )
                .WarmupCount( WarmupCount )
                .MeasurementCount( MeasurementCount )
                .SampleGroup( "Current" )
                .Run();


            yield return null;
        }
    }
}