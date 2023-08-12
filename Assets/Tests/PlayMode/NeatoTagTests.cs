using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CharlieMadeAThing.NeatoTags.Core;
using NUnit.Framework;
using Unity.PerformanceTesting;
using Unity.Profiling;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


namespace CharlieMadeAThing.NeatoTags.Tests {
    public class NeatoTagTests {
        GameObject _capsule;
        GameObject _cube;
        GameObject _cylinder;
        GameObject _plane;
        GameObject _quad;
        GameObject[] _shapes;
        GameObject _sphere;


        TagRefsForTests _tagRefsForTests;
        GameObject[] _uninitializedList;

        int[] _spawnAmounts = { 1, 10, 100, 1000, 10000 };

        [SetUp]
        public void TestSetup() {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene( "TestScene" );
        }

        void OnSceneLoaded( Scene arg0, LoadSceneMode arg1 ) {
            _tagRefsForTests = GameObject.FindWithTag( "TagRef" ).GetComponent<TagRefsForTests>();
            _cube = GameObject.FindWithTag( "Cube" );
            _cylinder = GameObject.FindWithTag( "Cylinder" );
            _sphere = GameObject.FindWithTag( "Sphere" );
            _capsule = GameObject.FindWithTag( "Capsule" );
            _plane = GameObject.FindWithTag( "Plane" );
            _quad = GameObject.FindWithTag( "Quad" );
            _shapes = new[] { _cube, _cylinder, _sphere, _capsule, _plane, _quad };
        }

        #region HasTagger

        [UnityTest]
        public IEnumerator IsTagged_DoesCubeHaveTagger_ReturnTrue() {
            Assert.AreEqual( true, _cube.HasTagger() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator IsTagged_DoesPlaneHaveTagger_ReturnFalse() {
            Assert.AreEqual( false, _plane.HasTagger() );
            yield return null;
        }

        #endregion

        #region HasTag Tests

        [UnityTest]
        public IEnumerator HasTag_AddTagWithNull_CheckForNull_ReturnsFalse() {
            _cube.AddTag( null );
            Assert.AreEqual( false, _cube.HasTag( (NeatoTag) null ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTag_QuadHaveCubeTag_QuadHasTaggerButNoTags_ReturnsFalse() {
            Assert.AreEqual( false, _quad.HasTag( _tagRefsForTests.cubeTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTag_CubeHasCubeTag_ReturnsTrue() {
            Assert.AreEqual( true, _cube.HasTag( _tagRefsForTests.cubeTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTag_ByString_CubeHasCubeTag_ReturnsTrue() {
            Assert.AreEqual( true, _cube.HasTag( "Cube" ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTag_CubeHasSphereTag_ReturnsFalse() {
            Assert.AreEqual( false, _cube.HasTag( _tagRefsForTests.sphereTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTag_ByString_CubeHasSphereTag_ReturnsFalse() {
            Assert.AreEqual( false, _cube.HasTag( "Sphere" ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTag_PlaneHasTag_ReturnsFalse() {
            Assert.AreEqual( false, _plane.HasTag( _tagRefsForTests.sphereTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTag_CubeDoesNotHaveSphereTag_ReturnsTrue() {
            Assert.AreEqual( true, !_cube.HasTag( _tagRefsForTests.sphereTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTag_PlaneDoesNotHaveTag_ReturnsTrue() {
            Assert.AreEqual( true, !_plane.HasTag( _tagRefsForTests.sphereTag ) );
            yield return null;
        }

        #endregion

        #region AddRemoveTag Tests

        [UnityTest]
        public IEnumerator AddTag_PassInNull_ReturnFalse() {
            _cube.AddTag( null );
            Assert.AreEqual( false, _cube.HasTag( (NeatoTag) null ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator AddTag_AddPlaneTagToCube_CheckHasTagPlane_ReturnTrue() {
            _cube.AddTag( _tagRefsForTests.planeTag );
            Assert.AreEqual( true, _cube.HasTag( _tagRefsForTests.planeTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator RemoveTag_RemovePlaneTagToCube_CheckHasTagPlane_ReturnFalse() {
            _cube.RemoveTag( _tagRefsForTests.planeTag );
            Assert.AreEqual( false, _cube.HasTag( _tagRefsForTests.planeTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator AddTag_AddPlaneTagToPlane_PlaneHasNoTagger_ReturnFalse() {
            _plane.AddTag( _tagRefsForTests.planeTag );
            Assert.AreEqual( false, _plane.HasTag( _tagRefsForTests.planeTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator RemoveTag_RemovePlaneTagFromPlane_PlaneHasNoTagger_ReturnFalse() {
            _plane.RemoveTag( _tagRefsForTests.planeTag );
            Assert.AreEqual( false, _plane.HasTag( _tagRefsForTests.planeTag ) );
            yield return null;
        }

        #endregion

        #region HasAnyTagsMatching Tests

        [UnityTest]
        public IEnumerator HasAnyTagsMatching_AddTagAsNull_CheckForAnyNulls_ReturnsFalse() {
            _cube.AddTag( null );
            Assert.AreEqual( false,
                _cube.HasAnyTagsMatching( (NeatoTag) null, null,
                    null ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasAnyTagsMatching_ByString_AddTagAsNull_CheckForAnyNulls_ReturnsFalse() {
            _cube.AddTag( null );
            Assert.AreEqual( false,
                _cube.HasAnyTagsMatching( (string) null, null,
                    null ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasAnyTagsMatching_DoesCubeHaveAnyListedTags_CubeSpherePlane_ReturnTrue() {
            Assert.AreEqual( true,
                _cube.HasAnyTagsMatching( _tagRefsForTests.cubeTag, _tagRefsForTests.sphereTag,
                    _tagRefsForTests.planeTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasAnyTagsMatching_ByString_DoesCubeHaveAnyListedTags_CubeSpherePlane_ReturnTrue() {
            Assert.AreEqual( true,
                _cube.HasAnyTagsMatching( "Cube", "Sphere",
                    "Plane" ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasAnyTagsMatching_IEnumerable_DoesCubeHaveAnyListedTags_CubeSpherePlane_ReturnTrue() {
            Assert.AreEqual( true,
                _cube.HasAnyTagsMatching( _tagRefsForTests.testTags ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator
            HasAnyTagsMatching_ByString_IEnumerable_DoesCubeHaveAnyListedTags_CubeSpherePlane_ReturnTrue() {
            Assert.AreEqual( true,
                _cube.HasAnyTagsMatching( _tagRefsForTests.testTagsStrings ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasAnyTagsMatching_DoesCubeHaveAnyListedTags_CapsuleSpherePlane_ReturnFalse() {
            Assert.AreEqual( false,
                _cube.HasAnyTagsMatching( _tagRefsForTests.capsuleTag, _tagRefsForTests.sphereTag,
                    _tagRefsForTests.planeTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasAnyTagsMatching_ByString_DoesCubeHaveAnyListedTags_CapsuleSpherePlane_ReturnFalse() {
            Assert.AreEqual( false,
                _cube.HasAnyTagsMatching( "Capsule", "Sphere",
                    "Plane" ) );
            yield return null;
        }

        #endregion

        #region HasAllTagsMatching Tests

        [UnityTest]
        public IEnumerator HasAllTagsMatching_DoesCubeHaveAllListedTags_CubePlatonic_ReturnTrue() {
            Assert.AreEqual( true, _cube.HasAllTagsMatching( _tagRefsForTests.cubeTag, _tagRefsForTests.platonicTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasAllTagsMatching_ByString_DoesCubeHaveAllListedTags_CubePlatonic_ReturnTrue() {
            Assert.AreEqual( true, _cube.HasAllTagsMatching( "Cube", "Platonic Solid" ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasAllTagsMatching_ByString_IEnumerable_DoesCubeHaveAllListedTags_CubePlatonic_ReturnTrue() {
            Assert.AreEqual( true, _cube.HasAllTagsMatching( "Cube", "Platonic Solid" ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasAllTagsMatching_DoesCubeHaveAllListedTags_CubePlatonicSphere_ReturnFalse() {
            Assert.AreEqual( false,
                _cube.HasAllTagsMatching( _tagRefsForTests.cubeTag, _tagRefsForTests.platonicTag,
                    _tagRefsForTests.sphereTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasAllTagsMatching_ByString_DoesCubeHaveAllListedTags_CubePlatonicSphere_ReturnFalse() {
            Assert.AreEqual( false,
                _cube.HasAllTagsMatching( "Cube", "Platonic Solid",
                    "Sphere" ) );
            yield return null;
        }

        #endregion

        #region HasNoTagsMatching

        [UnityTest]
        public IEnumerator HasNoTagsMatching_DoesCubeNotHaveListedTags_CubePlatonic_ReturnFalse() {
            Assert.AreEqual( false, _cube.HasNoTagsMatching( _tagRefsForTests.cubeTag, _tagRefsForTests.platonicTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasNoTagsMatching_ByString_DoesCubeNotHaveListedTags_CubePlatonic_ReturnFalse() {
            Assert.AreEqual( false, _cube.HasNoTagsMatching( "Cube", "Platonic Solid" ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasNoTagsMatching_DoesPlaneNotHaveListedTags_CubePlatonic_ReturnTrue() {
            Assert.AreEqual( true, _plane.HasNoTagsMatching( _tagRefsForTests.cubeTag, _tagRefsForTests.platonicTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasNoTagsMatching_ByString_DoesPlaneNotHaveListedTags_CubePlatonic_ReturnTrue() {
            Assert.AreEqual( true, _plane.HasNoTagsMatching( "Cube", "Platonic Solid" ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasNoTagsMatching_ByString_IEnumerable_DoesPlaneNotHaveListedTags_CubePlatonic_ReturnTrue() {
            Assert.AreEqual( true, _plane.HasNoTagsMatching( "Cube", "Platonic Solid" ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasNoTagsMatching_DoesCubeNotHaveListedTags_SpherePlaneCapsule_ReturnTrue() {
            Assert.AreEqual( true,
                _cube.HasNoTagsMatching( _tagRefsForTests.sphereTag, _tagRefsForTests.planeTag,
                    _tagRefsForTests.capsuleTag ) );
            yield return null;
        }

        #endregion

        #region FilterTags Tests

        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithTagCube_ReturnTrue() {
            Assert.AreEqual( true, _cube.FilterTags().WithTag( _tagRefsForTests.cubeTag ).IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithTagSphere_ReturnFalse() {
            Assert.AreEqual( false, _cube.FilterTags().WithTag( _tagRefsForTests.sphereTag ).IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithTagsListed_CubePlatonic_ReturnTrue() {
            Assert.AreEqual( true,
                _cube.FilterTags().WithTags( _tagRefsForTests.cubeTag, _tagRefsForTests.platonicTag ).IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithTagsListed_CubeSphere_ReturnFalse() {
            Assert.AreEqual( false,
                _cube.FilterTags().WithTags( _tagRefsForTests.cubeTag, _tagRefsForTests.sphereTag ).IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithoutTagCube_ReturnFalse() {
            Assert.AreEqual( false, _cube.FilterTags().WithoutTag( _tagRefsForTests.cubeTag ).IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithoutTagSphere_ReturnTrue() {
            Assert.AreEqual( true, _cube.FilterTags().WithoutTag( _tagRefsForTests.sphereTag ).IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithoutTagsListed_CubePlatonic_ReturnFalse() {
            Assert.AreEqual( false,
                _cube.FilterTags().WithoutTags( _tagRefsForTests.cubeTag, _tagRefsForTests.platonicTag ).IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithoutTagsListed_SpherePlane_ReturnTrue() {
            Assert.AreEqual( true,
                _cube.FilterTags().WithoutTags( _tagRefsForTests.sphereTag, _tagRefsForTests.planeTag ).IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithAnyTagsListed_CubePlatonicPlane_ReturnTrue() {
            Assert.AreEqual( true,
                _cube.FilterTags().WithAnyTags( _tagRefsForTests.cubeTag, _tagRefsForTests.platonicTag,
                    _tagRefsForTests.planeTag ).IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_WithTagCube_WithTagPlatonic_ReturnTrue() {
            Assert.AreEqual( true,
                _cube.FilterTags().WithTag( _tagRefsForTests.cubeTag ).WithTag( _tagRefsForTests.platonicTag )
                    .IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithTagCube_WithTagSphere_ReturnFalse() {
            Assert.AreEqual( false,
                _cube.FilterTags().WithTag( _tagRefsForTests.cubeTag ).WithTag( _tagRefsForTests.sphereTag )
                    .IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_Capsule_WithTagCornerless_WithTagSphere_ReturnFalse() {
            Assert.AreEqual( false,
                _capsule.FilterTags().WithTag( _tagRefsForTests.cornerlessTag ).WithTag( _tagRefsForTests.sphereTag )
                    .IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_Sphere_WithTagCornerless_WithoutTagCapsule_ReturnFalse() {
            Assert.AreEqual( false,
                _sphere.FilterTags().WithTag( _tagRefsForTests.cornerlessTag ).WithTag( _tagRefsForTests.capsuleTag )
                    .IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithoutTagsCornerlessAndSphere_ReturnFalse() {
            Assert.AreEqual( true,
                _cube.FilterTags().WithoutTags( _tagRefsForTests.cornerlessTag, _tagRefsForTests.sphereTag )
                    .IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithoutTagsCapsuleAndSphere_ReturnTrue() {
            Assert.AreEqual( true,
                _cube.FilterTags().WithoutTags( _tagRefsForTests.capsuleTag, _tagRefsForTests.sphereTag ).IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithAnyTagsPlatonicPlaneSphere_ReturnTrue() {
            Assert.AreEqual( true,
                _cube.FilterTags().WithAnyTags( _tagRefsForTests.platonicTag, _tagRefsForTests.planeTag,
                    _tagRefsForTests.sphereTag ).IsMatch() );
            yield return null;
        }

        #endregion

        #region StartGameObjectFilter Tests

        [UnityTest, Performance]
        public IEnumerator StartGameObjectFilter_PassInUninitializedList_ReturnsNothing() {
            Measure.Method( () => {
                    var filteredShapes =
                        Tagger.FilterGameObjects( _uninitializedList ).WithTag( _tagRefsForTests.planeTag ).GetMatches();
                } )
                .WarmupCount( 1 )
                .MeasurementCount( 9 )
                .Run();
            var filteredShapes =
                Tagger.FilterGameObjects( _uninitializedList ).WithTag( _tagRefsForTests.planeTag ).GetMatches();
            Assert.AreEqual( 0, filteredShapes.Count );
            yield return null;
        }

        [UnityTest, Performance]
        public IEnumerator StartGameObjectFilter_GetMatchesNoFilter_ReturnsNoShapes() {
            Measure.Method( () => {
                    var filteredShapes = Tagger.FilterGameObjects( _shapes ).WithTag( _tagRefsForTests.planeTag ).GetMatches();
                } )
                .WarmupCount( 1 )
                .MeasurementCount( 9 )
                .Run();
            var filteredShapes =
                Tagger.FilterGameObjects( _shapes ).WithTag( _tagRefsForTests.planeTag ).GetMatches();
            Assert.AreEqual( 0, filteredShapes.Count );
            yield return null;
        }

        [UnityTest, Performance]
        public IEnumerator StartGameObjectFilter_WithTagCube_ReturnsOneCube() {
            Measure.Method( () => {
                    var filteredShapes = Tagger.FilterGameObjects( _shapes ).WithTag( _tagRefsForTests.cubeTag ).GetMatches();
                } )
                .WarmupCount( 1 )
                .MeasurementCount( 9 )
                .Run();
            var filteredShapes = Tagger.FilterGameObjects( _shapes ).WithTag( _tagRefsForTests.cubeTag ).GetMatches();
            Assert.AreEqual( 1, filteredShapes.Count );
            filteredShapes.TryGetValue( _cube, out var cubeShape );
            Assert.AreEqual( _cube, cubeShape );
            yield return null;
        }

        [UnityTest, Performance]
        public IEnumerator StartGameObjectFilter_WithTagPlane_ReturnsZeroShapes() {
            Measure.Method( () => {
                    var filteredShapes = Tagger.FilterGameObjects( _shapes ).WithTag( _tagRefsForTests.planeTag ).GetMatches();
                } )
                .WarmupCount( 1 )
                .MeasurementCount( 9 )
                .Run();
            var filteredShapes =
                Tagger.FilterGameObjects( _shapes ).WithTag( _tagRefsForTests.planeTag ).GetMatches();
            Assert.AreEqual( 0, filteredShapes.Count );
            yield return null;
        }

        [UnityTest, Performance]
        public IEnumerator StartGameObjectFilter_WithTagCornerless_ReturnsTwoShapes_SphereCapsule() {
            Measure.Method( () => {
                    var filteredShapes =
                        Tagger.FilterGameObjects( _shapes ).WithTag( _tagRefsForTests.cornerlessTag ).GetMatches();
                } )
                .WarmupCount( 1 )
                .MeasurementCount( 9 )
                .Run();
            var filteredShapes =
                Tagger.FilterGameObjects( _shapes ).WithTag( _tagRefsForTests.cornerlessTag ).GetMatches();
            Assert.AreEqual( 2, filteredShapes.Count );
            if ( filteredShapes.Contains( _plane ) || filteredShapes.Contains( _cube ) ||
                 filteredShapes.Contains( _cylinder ) ) {
                Assert.Fail( "Filtered shapes should not contain plane, cube, or cylinder" );
            } else {
                Assert.Pass();
            }

            yield return null;
        }

        [UnityTest, Performance]
        public IEnumerator StartGameObjectFilter_WithTags_CubePlatonic_ReturnsOneCube() {
            Measure.Method( () => {
                    var filteredShapes = Tagger.FilterGameObjects( _shapes )
                        .WithTags( _tagRefsForTests.cubeTag, _tagRefsForTests.platonicTag ).GetMatches();
                } )
                .WarmupCount( 1 )
                .MeasurementCount( 9 )
                .Run();
            var filteredShapes = Tagger.FilterGameObjects( _shapes )
                .WithTags( _tagRefsForTests.cubeTag, _tagRefsForTests.platonicTag ).GetMatches();
            Assert.AreEqual( 1, filteredShapes.Count );
            filteredShapes.TryGetValue( _cube, out var cubeShape );
            Assert.AreEqual( _cube, cubeShape );
            yield return null;
        }

        [UnityTest, Performance]
        public IEnumerator StartGameObjectFilter_WithTags_CubeSphere_ReturnsZeroShapes() {
            Measure.Method( () => {
                    var filteredShapes = Tagger.FilterGameObjects( _shapes )
                        .WithTags( _tagRefsForTests.cubeTag, _tagRefsForTests.sphereTag ).GetMatches();
                } )
                .WarmupCount( 1 )
                .MeasurementCount( 9 )
                .Run();
            var filteredShapes = Tagger.FilterGameObjects( _shapes )
                .WithTags( _tagRefsForTests.cubeTag, _tagRefsForTests.sphereTag ).GetMatches();
            Assert.AreEqual( 0, filteredShapes.Count );
            yield return null;
        }


        [UnityTest, Performance]
        public IEnumerator StartGameObjectFilter_WithoutTagCube_ReturnsFourShapesNoCube() {
            Measure.Method( () => {
                    var filteredShapes = Tagger.FilterGameObjects( _shapes ).WithoutTag( _tagRefsForTests.cubeTag )
                        .GetMatches();
                } )
                .WarmupCount( 1 )
                .MeasurementCount( 9 )
                .Run();
            var filteredShapes = Tagger.FilterGameObjects( _shapes ).WithoutTag( _tagRefsForTests.cubeTag )
                .GetMatches();
            Assert.AreEqual( 4, filteredShapes.Count,
                $"Cube should not be in the list of shapes. {string.Join( ", ", filteredShapes.Select( x => x.name ) )}" );
            if ( filteredShapes.Contains( _cube ) ) {
                Assert.Fail(
                    $"Cube should not be in the list of shapes. {string.Join( ", ", filteredShapes.Select( x => x.name ) )}" );
            } else {
                Assert.Pass();
            }

            yield return null;
        }

        [UnityTest, Performance]
        public IEnumerator StartGameObjectFilter_WithoutTagPlane_ReturnsFiveShapes() {
            Measure.Method( () => {
                    var filteredShapes = Tagger.FilterGameObjects( _shapes ).WithoutTag( _tagRefsForTests.planeTag )
                        .GetMatches();
                } )
                .WarmupCount( 1 )
                .MeasurementCount( 9 )
                .Run();
            var filteredShapes = Tagger.FilterGameObjects( _shapes ).WithoutTag( _tagRefsForTests.planeTag )
                .GetMatches();
            Assert.AreEqual( 5, filteredShapes.Count );
            yield return null;
        }

        [UnityTest, Performance]
        public IEnumerator StartGameObjectFilter_WithoutTags_CubeSphere_ReturnsThreeShapesNoCubeOrSphere() {
            Measure.Method( () => {
                    var filteredShapes = Tagger.FilterGameObjects( _shapes )
                        .WithoutTags( _tagRefsForTests.cubeTag, _tagRefsForTests.sphereTag ).GetMatches();
                } )
                .WarmupCount( 1 )
                .MeasurementCount( 9 )
                .Run();
            var filteredShapes = Tagger.FilterGameObjects( _shapes )
                .WithoutTags( _tagRefsForTests.cubeTag, _tagRefsForTests.sphereTag ).GetMatches();
            Assert.AreEqual( 3, filteredShapes.Count );
            if ( filteredShapes.Contains( _cube ) ) {
                Assert.Fail( "Cube should not be in the list of shapes" );
            } else if ( filteredShapes.Contains( _sphere ) ) {
                Assert.Fail( "Sphere should not be in the list of shapes" );
            } else {
                Assert.Pass();
            }

            yield return null;
        }

        [UnityTest, Performance]
        public IEnumerator
            StartGameObjectFilter_WithAnyTags_CubeSphereCapsule_ReturnsThreeShapes_ReturnOnlyCubeSphereCapsule() {
            Measure.Method( () => {
                    var filteredShapes = Tagger.FilterGameObjects( _shapes ).WithAnyTags( _tagRefsForTests.cubeTag,
                            _tagRefsForTests.sphereTag, _tagRefsForTests.capsuleTag )
                        .GetMatches();
                } )
                .WarmupCount( 1 )
                .MeasurementCount( 9 )
                .Run();
            var filteredShapes = Tagger.FilterGameObjects( _shapes ).WithAnyTags( _tagRefsForTests.cubeTag,
                    _tagRefsForTests.sphereTag, _tagRefsForTests.capsuleTag )
                .GetMatches();
            Assert.AreEqual( 3, filteredShapes.Count );
            if ( filteredShapes.Contains( _plane ) || filteredShapes.Contains( _cylinder ) ) {
                Assert.Fail( "Plane or Cylinder should not be in the list of shapes" );
            } else {
                Assert.Pass();
            }

            yield return null;
        }

        [UnityTest, Performance]
        public IEnumerator
            StartGameObjectFilter_WithAnyTags_PlatonicPlane_ReturnsOneShape_ReturnOnlyCube() {
            Measure.Method( () => {
                    var filteredShapes = Tagger.FilterGameObjects( _shapes )
                        .WithAnyTags( _tagRefsForTests.platonicTag, _tagRefsForTests.planeTag )
                        .GetMatches();
                } )
                .WarmupCount( 1 )
                .MeasurementCount( 9 )
                .Run();
            var filteredShapes = Tagger.FilterGameObjects( _shapes )
                .WithAnyTags( _tagRefsForTests.platonicTag, _tagRefsForTests.planeTag )
                .GetMatches();
            Assert.AreEqual( 1, filteredShapes.Count );
            if ( filteredShapes.Contains( _plane ) || filteredShapes.Contains( _cylinder ) ||
                 filteredShapes.Contains( _sphere ) || filteredShapes.Contains( _capsule ) ) {
                Assert.Fail( "Plane, cylinder, sphere, or capsule should not be in the list of shapes" );
            } else {
                Assert.Pass();
            }

            yield return null;
        }

        [UnityTest, Performance]
        public IEnumerator StartGameObjectFilter_WithTagCornerless_WithoutTagSphere_ReturnOneShapeCapsule() {
            Measure.Method( () => {
                    var filteredShapes =
                        Tagger.FilterGameObjects( _shapes ).WithTag( _tagRefsForTests.cornerlessTag )
                            .WithoutTag( _tagRefsForTests.sphereTag )
                            .GetMatches();
                } )
                .WarmupCount( 1 )
                .MeasurementCount( 9 )
                .Run();
            var filteredShapes =
                Tagger.FilterGameObjects( _shapes ).WithTag( _tagRefsForTests.cornerlessTag ).WithoutTag( _tagRefsForTests.sphereTag )
                    .GetMatches();
            Assert.AreEqual( 1, filteredShapes.Count );
            if ( filteredShapes.Contains( _plane ) || filteredShapes.Contains( _cube ) ||
                 filteredShapes.Contains( _cylinder ) || filteredShapes.Contains( _sphere ) ) {
                Assert.Fail( "Filtered shapes should not contain plane, cube, cylinder, or sphere" );
            } else {
                Assert.Pass();
            }

            yield return null;
        }

        [UnityTest, Performance]
        public IEnumerator StartGameObjectFilter_WithTagCube_WithoutTagPlatonic_ReturnZeroShapes() {
            Measure.Method( () => {
                    var filteredShapes =
                        Tagger.FilterGameObjects( _shapes ).WithTag( _tagRefsForTests.cubeTag )
                            .WithoutTag( _tagRefsForTests.platonicTag )
                            .GetMatches();
                } )
                .WarmupCount( 1 )
                .MeasurementCount( 9 )
                .Run();
            var filteredShapes =
                Tagger.FilterGameObjects( _shapes ).WithTag( _tagRefsForTests.cubeTag ).WithoutTag( _tagRefsForTests.platonicTag )
                    .GetMatches();
            Assert.AreEqual( 0, filteredShapes.Count );

            yield return null;
        }

        #endregion

        #region GameObjectFilter BIG Tests

        [UnityTest, Performance]
        public IEnumerator BigGameObjectFilter_GetMatchesCube_Performance_RandomGameObjectsHaveTag() {
            var newShapes = new List<GameObject>();


            foreach ( var amount in _spawnAmounts ) {
                var sg = new SampleGroup( $"Filter 1 GameObject from {amount}", SampleUnit.Millisecond );
                
                var amountToSpawn = amount;
                Measure.Method( () => { Tagger.FilterGameObjects( newShapes ).WithTag( _tagRefsForTests.testTags[0] ).GetMatches(); } )
                    .SetUp( () => {
                        for ( var j = 0; j < amountToSpawn; j++ ) {
                            var shape = GameObject.CreatePrimitive( PrimitiveType.Cube );
                            newShapes.Add( shape );

                            shape.AddComponent<Tagger>().AddTag( j == amountToSpawn - 1
                                ? _tagRefsForTests.testTags[0]
                                : _tagRefsForTests.testTags[1] );
                        }
                    })
                    .CleanUp( () => {
                        foreach ( var shapeGo in newShapes ) {
                            Object.DestroyImmediate( shapeGo );
                        }
                        newShapes.Clear();
                    })
                    .WarmupCount( 5 )
                    .MeasurementCount( 10 )
                    .SampleGroup(sg)
                    .Run();
            }
            
            Assert.Pass();
            yield return null;
        }
        #endregion
    }
}