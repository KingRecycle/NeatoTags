using System.Collections;
using System.Linq;
using CharlieMadeAThing.NeatoTags;
using CharlieMadeAThing.NeatoTags.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CharlieMadeAThing {
    public class NeatoTagTests {
        public GameObject capsule;
        public GameObject cube;
        public GameObject cylinder;
        public GameObject plane; 
        public GameObject sphere;
        public GameObject quad;
        public GameObject[] shapes;
        public GameObject[] unintializedList;

        


        public TagRefsForTests tagRefsForTests;

        [SetUp]
        public void TestSetup() {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene( "TestScene" );
        }

        void OnSceneLoaded( Scene arg0, LoadSceneMode arg1 ) {
            tagRefsForTests = GameObject.FindWithTag( "TagRef" ).GetComponent<TagRefsForTests>();
            cube = GameObject.FindWithTag( "Cube" );
            cylinder = GameObject.FindWithTag( "Cylinder" );
            sphere = GameObject.FindWithTag( "Sphere" );
            capsule = GameObject.FindWithTag( "Capsule" );
            plane = GameObject.FindWithTag( "Plane" );
            quad = GameObject.FindWithTag( "Quad" );
            shapes = new[] { cube, cylinder, sphere, capsule, plane, quad };
        }

        #region IsTagged

        [UnityTest]
        public IEnumerator IsTagged_DoesCubeHaveTagger_ReturnTrue() {
            Assert.AreEqual( true, cube.IsTagged() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator IsTagged_DoesPlaneHaveTagger_ReturnFalse() {
            Assert.AreEqual( false, plane.IsTagged() );
            yield return null;
        }

        #endregion

        #region HasTag Tests

        [UnityTest]
        public IEnumerator HasTag_AddTagWithNull_CheckForNull_ReturnsFalse() {
            cube.AddTag( null );
            Assert.AreEqual( false, cube.HasTag( null ) );
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator HasTag_QuadHaveCubeTag_QuadHasTaggerButNoTags_ReturnsFalse() {
            Assert.AreEqual( false, quad.HasTag( tagRefsForTests.cubeTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTag_CubeHasCubeTag_ReturnsTrue() {
            Assert.AreEqual( true, cube.HasTag( tagRefsForTests.cubeTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTag_CubeHasSphereTag_ReturnsFalse() {
            Assert.AreEqual( false, cube.HasTag( tagRefsForTests.sphereTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTag_PlaneHasTag_ReturnsFalse() {
            Assert.AreEqual( false, plane.HasTag( tagRefsForTests.sphereTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTag_CubeDoesNotHaveSphereTag_ReturnsTrue() {
            Assert.AreEqual( true, !cube.HasTag( tagRefsForTests.sphereTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTag_PlaneDoesNotHaveTag_ReturnsTrue() {
            Assert.AreEqual( true, !plane.HasTag( tagRefsForTests.sphereTag ) );
            yield return null;
        }

        #endregion

        #region AddRemoveTag Tests

        [UnityTest]
        public IEnumerator AddTag_PassInNull_ReturnFalse() {
            cube.AddTag( null );
            Assert.AreEqual( false, cube.HasTag( null ) );
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator AddTag_AddPlaneTagToCube_CheckHasTagPlane_ReturnTrue() {
            cube.AddTag( tagRefsForTests.planeTag );
            Assert.AreEqual( true, cube.HasTag( tagRefsForTests.planeTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator RemoveTag_RemovePlaneTagToCube_CheckHasTagPlane_ReturnFalse() {
            cube.RemoveTag( tagRefsForTests.planeTag );
            Assert.AreEqual( false, cube.HasTag( tagRefsForTests.planeTag ) );
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator AddTag_AddPlaneTagToPlane_PlaneHasNoTagger_ReturnFalse() {
            plane.AddTag( tagRefsForTests.planeTag );
            Assert.AreEqual( false, plane.HasTag( tagRefsForTests.planeTag ) );
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator RemoveTag_RemovePlaneTagFromPlane_PlaneHasNoTagger_ReturnFalse() {
            plane.RemoveTag( tagRefsForTests.planeTag );
            Assert.AreEqual( false, plane.HasTag( tagRefsForTests.planeTag ) );
            yield return null;
        }

        #endregion

        #region HasAnyTagsMatching Tests

        [UnityTest]
        public IEnumerator HasAnyTagsMatching_AddTagAsNull_CheckForAnyNulls_ReturnsFalse() {
            cube.AddTag( null );
            Assert.AreEqual( false,
                cube.HasAnyTagsMatching( null, null,
                    null ) );
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator HasAnyTagsMatching_DoesCubeHaveAnyListedTags_CubeSpherePlane_ReturnTrue() {
            Assert.AreEqual( true,
                cube.HasAnyTagsMatching( tagRefsForTests.cubeTag, tagRefsForTests.sphereTag,
                    tagRefsForTests.planeTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasAnyTagsMatching_DoesCubeHaveAnyListedTags_CapsuleSpherePlane_ReturnFalse() {
            Assert.AreEqual( false,
                cube.HasAnyTagsMatching( tagRefsForTests.capsuleTag, tagRefsForTests.sphereTag,
                    tagRefsForTests.planeTag ) );
            yield return null;
        }

        #endregion

        #region HasAllTagsMatching Tests

        [UnityTest]
        public IEnumerator HasAllTagsMatching_DoesCubeHaveAllListedTags_CubePlatonic_ReturnTrue() {
            Assert.AreEqual( true, cube.HasAllTagsMatching( tagRefsForTests.cubeTag, tagRefsForTests.platonicTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasAllTagsMatching_DoesCubeHaveAllListedTags_CubePlatonicSphere_ReturnFalse() {
            Assert.AreEqual( false,
                cube.HasAllTagsMatching( tagRefsForTests.cubeTag, tagRefsForTests.platonicTag,
                    tagRefsForTests.sphereTag ) );
            yield return null;
        }
        #endregion

        #region HasNoTagsMatching
        [UnityTest]
        public IEnumerator HasNoTagsMatching_DoesCubeNotHaveListedTags_CubePlatonic_ReturnFalse() {
            Assert.AreEqual( false, cube.HasNoTagsMatching( tagRefsForTests.cubeTag, tagRefsForTests.platonicTag ) );
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator HasNoTagsMatching_DoesPlaneNotHaveListedTags_CubePlatonic_ReturnTrue() {
            Assert.AreEqual( true, plane.HasNoTagsMatching( tagRefsForTests.cubeTag, tagRefsForTests.platonicTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasNoTagsMatching_DoesCubeNotHaveListedTags_SpherePlaneCapsule_ReturnTrue() {
            Assert.AreEqual( true,
                cube.HasNoTagsMatching( tagRefsForTests.sphereTag, tagRefsForTests.planeTag,
                    tagRefsForTests.capsuleTag ) );
            yield return null;
        }
        
        
        #endregion
        
        #region StartTagFilter Tests

        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithTagCube_ReturnTrue() {
            Assert.AreEqual( true, cube.StartTagFilter().WithTag( tagRefsForTests.cubeTag ).IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithTagSphere_ReturnFalse() {
            Assert.AreEqual( false, cube.StartTagFilter().WithTag( tagRefsForTests.sphereTag ).IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithTagsListed_CubePlatonic_ReturnTrue() {
            Assert.AreEqual( true,
                cube.StartTagFilter().WithTags( tagRefsForTests.cubeTag, tagRefsForTests.platonicTag ).IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithTagsListed_CubeSphere_ReturnFalse() {
            Assert.AreEqual( false,
                cube.StartTagFilter().WithTags( tagRefsForTests.cubeTag, tagRefsForTests.sphereTag ).IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithoutTagCube_ReturnFalse() {
            Assert.AreEqual( false, cube.StartTagFilter().WithoutTag( tagRefsForTests.cubeTag ).IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithoutTagSphere_ReturnTrue() {
            Assert.AreEqual( true, cube.StartTagFilter().WithoutTag( tagRefsForTests.sphereTag ).IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithoutTagsListed_CubePlatonic_ReturnFalse() {
            Assert.AreEqual( false,
                cube.StartTagFilter().WithoutTags( tagRefsForTests.cubeTag, tagRefsForTests.platonicTag ).IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithoutTagsListed_SpherePlane_ReturnTrue() {
            Assert.AreEqual( true,
                cube.StartTagFilter().WithoutTags( tagRefsForTests.sphereTag, tagRefsForTests.planeTag ).IsMatch() );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithAnyTagsListed_CubePlatonicPlane_ReturnTrue() {
            Assert.AreEqual( true,
                cube.StartTagFilter().WithAnyTags( tagRefsForTests.cubeTag, tagRefsForTests.platonicTag,
                    tagRefsForTests.planeTag ).IsMatch() );
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator StartTagFilter_WithTagCube_WithTagPlatonic_ReturnTrue() {
            Assert.AreEqual( true,
                cube.StartTagFilter().WithTag( tagRefsForTests.cubeTag ).WithTag( tagRefsForTests.platonicTag ).IsMatch() );
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithTagCube_WithTagSphere_ReturnFalse() {
            Assert.AreEqual( false,
                cube.StartTagFilter().WithTag( tagRefsForTests.cubeTag ).WithTag( tagRefsForTests.sphereTag ).IsMatch() );
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator StartTagFilter_Capsule_WithTagCornerless_WithTagSphere_ReturnFalse() {
            Assert.AreEqual( false,
                capsule.StartTagFilter().WithTag( tagRefsForTests.cornerlessTag ).WithTag( tagRefsForTests.sphereTag ).IsMatch() );
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator StartTagFilter_Sphere_WithTagCornerless_WithoutTagCapsule_ReturnFalse() {
            Assert.AreEqual( false,
                sphere.StartTagFilter().WithTag( tagRefsForTests.cornerlessTag ).WithTag( tagRefsForTests.capsuleTag ).IsMatch() );
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithoutTagsCornerlessAndSphere_ReturnFalse() {
            Assert.AreEqual( true,
                cube.StartTagFilter().WithoutTags( tagRefsForTests.cornerlessTag, tagRefsForTests.sphereTag ).IsMatch() );
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithoutTagsCapsuleAndSphere_ReturnTrue() {
            Assert.AreEqual( true,
                cube.StartTagFilter().WithoutTags( tagRefsForTests.capsuleTag, tagRefsForTests.sphereTag ).IsMatch() );
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator StartTagFilter_Cube_WithAnyTagsPlatonicPlaneSphere_ReturnTrue() {
            Assert.AreEqual( true,
                cube.StartTagFilter().WithAnyTags( tagRefsForTests.platonicTag, tagRefsForTests.planeTag, tagRefsForTests.sphereTag ).IsMatch() );
            yield return null;
        }

        #endregion

        #region StartGameObjectFilter Tests
        
        [UnityTest]
        public IEnumerator StartGameObjectFilter_PassInUninitializedList_ReturnsNothing() {
            var filteredShapes =
                Tagger.StartGameObjectFilter( unintializedList ).WithTag( tagRefsForTests.planeTag ).GetMatches();
            Assert.AreEqual( 0, filteredShapes.Count );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartGameObjectFilter_GetMatchesNoFilter_ReturnsNoShapes() {
            var filteredShapes =
                Tagger.StartGameObjectFilter( shapes ).WithTag( tagRefsForTests.planeTag ).GetMatches();
            Assert.AreEqual( 0, filteredShapes.Count );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartGameObjectFilter_WithTagCube_ReturnsOneCube() {
            var filteredShapes = Tagger.StartGameObjectFilter( shapes ).WithTag( tagRefsForTests.cubeTag ).GetMatches();
            Assert.AreEqual( 1, filteredShapes.Count );
            filteredShapes.TryGetValue( cube, out var cubeShape );
            Assert.AreEqual( cube, cubeShape );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartGameObjectFilter_WithTagPlane_ReturnsZeroShapes() {
            var filteredShapes =
                Tagger.StartGameObjectFilter( shapes ).WithTag( tagRefsForTests.planeTag ).GetMatches();
            Assert.AreEqual( 0, filteredShapes.Count );
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator StartGameObjectFilter_WithTagCornerless_ReturnsTwoShapes_SphereCapsule() {
            var filteredShapes =
                Tagger.StartGameObjectFilter( shapes ).WithTag( tagRefsForTests.cornerlessTag ).GetMatches();
            Assert.AreEqual( 2, filteredShapes.Count );
            if( filteredShapes.Contains(plane) || filteredShapes.Contains(cube) || filteredShapes.Contains(cylinder)) {
                Assert.Fail( "Filtered shapes should not contain plane, cube, or cylinder" );
            } else {
                Assert.Pass();
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartGameObjectFilter_WithTags_CubePlatonic_ReturnsOneCube() {
            var filteredShapes = Tagger.StartGameObjectFilter( shapes )
                .WithTags( tagRefsForTests.cubeTag, tagRefsForTests.platonicTag ).GetMatches();
            Assert.AreEqual( 1, filteredShapes.Count );
            filteredShapes.TryGetValue( cube, out var cubeShape );
            Assert.AreEqual( cube, cubeShape );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartGameObjectFilter_WithTags_CubeSphere_ReturnsZeroShapes() {
            var filteredShapes = Tagger.StartGameObjectFilter( shapes )
                .WithTags( tagRefsForTests.cubeTag, tagRefsForTests.sphereTag ).GetMatches();
            Assert.AreEqual( 0, filteredShapes.Count );
            yield return null;
        }
        
        


        [UnityTest]
        public IEnumerator StartGameObjectFilter_WithoutTagCube_ReturnsFourShapesNoCube() {
            var filteredShapes = Tagger.StartGameObjectFilter( shapes ).WithoutTag( tagRefsForTests.cubeTag )
                .GetMatches();
            Assert.AreEqual( 4, filteredShapes.Count, $"Cube should not be in the list of shapes. {string.Join( ", ", filteredShapes.Select( x=> x.name ) )  }" );
            if ( filteredShapes.Contains( cube ) ) {
                Assert.Fail( $"Cube should not be in the list of shapes. {string.Join( ", ", filteredShapes.Select( x=> x.name ) )  }" );
            } else {
                Assert.Pass();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator StartGameObjectFilter_WithoutTagPlane_ReturnsFiveShapes() {
            var filteredShapes = Tagger.StartGameObjectFilter( shapes ).WithoutTag( tagRefsForTests.planeTag )
                .GetMatches();
            Assert.AreEqual( 5, filteredShapes.Count );
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartGameObjectFilter_WithoutTags_CubeSphere_ReturnsThreeShapesNoCubeOrSphere() {
            var filteredShapes = Tagger.StartGameObjectFilter( shapes )
                .WithoutTags( tagRefsForTests.cubeTag, tagRefsForTests.sphereTag ).GetMatches();
            Assert.AreEqual( 3, filteredShapes.Count );
            if ( filteredShapes.Contains( cube ) ) {
                Assert.Fail( "Cube should not be in the list of shapes" );
            } else if ( filteredShapes.Contains( sphere ) ) {
                Assert.Fail( "Sphere should not be in the list of shapes" );
            } else {
                Assert.Pass();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator
            StartGameObjectFilter_WithAnyTags_CubeSphereCapsule_ReturnsThreeShapes_ReturnOnlyCubeSphereCapsule() {
            var filteredShapes = Tagger.StartGameObjectFilter( shapes ).WithAnyTags( tagRefsForTests.cubeTag,
                    tagRefsForTests.sphereTag, tagRefsForTests.capsuleTag )
                .GetMatches();
            Assert.AreEqual( 3, filteredShapes.Count );
            if ( filteredShapes.Contains( plane ) || filteredShapes.Contains(cylinder) ) {
                Assert.Fail( "Plane or Cylinder should not be in the list of shapes" );
            } else {
                Assert.Pass();
            }

            yield return null;
        }
        
        [UnityTest]
        public IEnumerator
            StartGameObjectFilter_WithAnyTags_PlatonicPlane_ReturnsOneShape_ReturnOnlyCube() {
            var filteredShapes = Tagger.StartGameObjectFilter( shapes ).WithAnyTags( tagRefsForTests.platonicTag, tagRefsForTests.planeTag )
                .GetMatches();
            Assert.AreEqual( 1, filteredShapes.Count );
            if ( filteredShapes.Contains( plane ) || filteredShapes.Contains(cylinder) || filteredShapes.Contains(sphere) || filteredShapes.Contains(capsule) ) {
                Assert.Fail( "Plane, cylinder, sphere, or capsule should not be in the list of shapes" );
            } else {
                Assert.Pass();
            }

            yield return null;
        }
        
        [UnityTest]
        public IEnumerator StartGameObjectFilter_WithTagCornerless_WithoutTagSphere_ReturnOneShapeCapsule() {
            var filteredShapes =
                Tagger.StartGameObjectFilter( shapes ).WithTag( tagRefsForTests.cornerlessTag ).WithoutTag(tagRefsForTests.sphereTag).GetMatches();
            Assert.AreEqual( 1, filteredShapes.Count );
            if( filteredShapes.Contains(plane) || filteredShapes.Contains(cube) || filteredShapes.Contains(cylinder) || filteredShapes.Contains(sphere)) {
                Assert.Fail( "Filtered shapes should not contain plane, cube, cylinder, or sphere" );
            } else {
                Assert.Pass();
            }
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator StartGameObjectFilter_WithTagCube_WithoutTagPlatonic_ReturnZeroShapes() {
            var filteredShapes =
                Tagger.StartGameObjectFilter( shapes ).WithTag( tagRefsForTests.cubeTag ).WithoutTag(tagRefsForTests.platonicTag).GetMatches();
            Assert.AreEqual( 0, filteredShapes.Count );
            
            yield return null;
        }
        

        #endregion
    }
}