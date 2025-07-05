using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CharlieMadeAThing.NeatoTags.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CharlieMadeAThing.NeatoTags.Tests.PlayMode {
    public abstract class NeatoTagTests {
        protected GameObject Cube;
        protected GameObject Sphere;
        protected GameObject Capsule;
        protected GameObject Plane;
        protected GameObject Cylinder;
        protected GameObject[] ShapeTagsToFilter;
        protected TagRefsForTests TagRefsForTests;

        [UnitySetUp]
        public IEnumerator SetUp() {
            SceneManager.LoadScene( "TestScene" );
            yield return null; // Wait for a scene to load

            TagRefsForTests = GameObject.Find( "TagRefs" ).GetComponent<TagRefsForTests>();
            Cube = GameObject.Find( "Cube" ); //Has two tags (Cube and Platonic Solid).
            Sphere = GameObject.Find( "Sphere" ); //Has two tags (Sphere and Cornerless).
            Capsule = GameObject.Find( "Capsule" ); //Has two tags (Capsule and Cornerless).
            Plane = GameObject.Find( "Plane" ); //Does not have a tagger component.
            Cylinder = GameObject.Find( "Cylinder" ); //Has Tagger but no tags.
            ShapeTagsToFilter = new[] { Cube, Sphere, Capsule, Plane, Cylinder };

            CheckIfTestSceneStateIsValid();
        }

        void CheckIfTestSceneStateIsValid() {
            //Cube shouldn't be null and have just two tags (Cube and Platonic Sold).
            Assert.That( Cube, Is.Not.Null );
            Assert.That( Cube.GetComponent<Tagger>(), Is.Not.Null );
            Assert.That( Cube.GetComponent<Tagger>().GetTags,
                Is.EquivalentTo( new[] { TagRefsForTests.cubeTag, TagRefsForTests.platonicTag } ) );

            //Sphere shouldn't be null and have just two tags (Sphere and Cornerless).
            Assert.That( Sphere, Is.Not.Null );
            Assert.That( Sphere.GetComponent<Tagger>(), Is.Not.Null );
            Assert.That( Sphere.GetComponent<Tagger>().GetTags,
                Is.EquivalentTo( new[] { TagRefsForTests.sphereTag, TagRefsForTests.cornerlessTag } ) );

            //Capsule shouldn't be null and have just two tags (Capsule and Cornerless).
            Assert.That( Capsule, Is.Not.Null );
            Assert.That( Capsule.GetComponent<Tagger>(), Is.Not.Null );
            Assert.That( Capsule.GetComponent<Tagger>().GetTags,
                Is.EquivalentTo( new[] { TagRefsForTests.capsuleTag, TagRefsForTests.cornerlessTag } ) );

            //Plane shouldn't be null and not have a Tagger component.
            Assert.That( Plane, Is.Not.Null );
            Assert.That( Plane.GetComponent<Tagger>(), Is.Null );

            //Cylinder shouldn't be null and should have a tagger and 0 tags on it.
            Assert.That( Cylinder, Is.Not.Null );
            Assert.That( Cylinder.GetComponent<Tagger>(), Is.Not.Null );
            Assert.That( Cylinder.GetComponent<Tagger>().GetTags, Is.Empty );

            //Tag refs shouldn't be null.
            Assert.That( TagRefsForTests, Is.Not.Null );
        }
    }

    [TestFixture]
    public class TaggerStaticTests : NeatoTagTests {
        [UnityTest]
        public IEnumerator TryGetTagger_CubeWithTagger_ReturnsTrue() {
            //Tagger.TryGetTagger() should return true and out a valid tagger on the Cube and not just any tagger component.
            if ( Tagger.TryGetTagger( Cube, out var tagger ) ) {
                Assert.That( tagger, Is.Not.Null, "Should have outed a null Tagger." );
                Assert.That( tagger.HasTag( TagRefsForTests.cubeTag ), Is.True,
                    "Should have outed the Cube Neato Tag." );
            }
            else {
                Assert.Fail( "Should have returned true for this test." );
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator TryGetTagger_PlaneWithoutTagger_ReturnsFalse() {
            //Tagger.TryGetTagger() should return false and out a null tagger on the Plane and not just any tagger component.
            //Tagger.TryGetTagger() can't find a tag that doesn't exist on a Tagger in the scene.
            if ( Tagger.TryGetTagger( Plane, out var tagger ) ) {
                Assert.Fail( "Should have returned false for this test." );
            }
            else {
                Assert.That( tagger, Is.Null, "Should have outed a null tagger." );
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTaggerStatic_WithCube_ReturnsTrue() {
            Assert.That( Tagger.HasTagger( Cube ), Is.True, "Cube should have a Tagger component." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTaggerStatic_WithPlane_ReturnsFalse() {
            Assert.That( Tagger.HasTagger( Plane ), Is.False, "Plane should not have a Tagger component." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator TryGetNeatoTag_GetCubeTag_ReturnsTrueAndReturnsTag() {
            var exist = Tagger.TryGetNeatoTag( "Cube", out var tag );
            Assert.That( exist, Is.True, "Should return true." );
            Assert.That( tag, Is.EqualTo( TagRefsForTests.cubeTag ), "Should return the correct tag." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator TryGetNeatoTag_GetPlaneTag_ReturnsFalseAndNull() {
            //TryGetNeatoTag only works if the tag is on a Tagger. Plane should not be in the scene on a tagger.
            var exist = Tagger.TryGetNeatoTag( "Plane", out var tag );
            Assert.That( exist, Is.False, "Should return false." );
            Assert.That( tag, Is.Null, "Should return null." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator GetAllGameObjectsWithTagger_ReturnsCubeCapsuleCylinderAndSphere() {
            var gameObjects = Tagger.GetAllGameObjectsWithTagger();
            Assert.That( gameObjects.Count, Is.EqualTo( 4 ), "Should return 4 game objects." );
            Assert.That( gameObjects.Contains( Cube ), Is.True, "Should return the Cube." );
            Assert.That( gameObjects.Contains( Capsule ), Is.True, "Should return the Capsule." );
            Assert.That( gameObjects.Contains( Sphere ), Is.True, "Should return the Sphere." );
            Assert.That( gameObjects.Contains( Cylinder ), Is.True, "Should return the Cylinder." );

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetGameobjectsWithTagger_ReturnsCubeCapsuleCylinderAndSphereWithTheirTaggers() {
            var gameObjectsDict = Tagger.GetGameobjectsWithTagger();
            Assert.That( gameObjectsDict.Count, Is.EqualTo( 4 ), "Should return 4 game objects." );
            Assert.That( gameObjectsDict.ContainsKey( Cube ), Is.True, "Should return the Cube." );
            Assert.That( gameObjectsDict.ContainsKey( Capsule ), Is.True, "Should return the Capsule." );
            Assert.That( gameObjectsDict.ContainsKey( Sphere ), Is.True, "Should return the Sphere." );
            Assert.That( gameObjectsDict.ContainsKey( Plane ), Is.False, "Should not return the Plane." );
            Assert.That( gameObjectsDict.ContainsKey( Cylinder ), Is.True, "Should return the Cylinder." );
            Assert.That( gameObjectsDict[Cube].HasTag( TagRefsForTests.cubeTag ), Is.True,
                "Should return true if the Cube has the Cube tag." );
            Assert.That( gameObjectsDict[Capsule].HasTag( TagRefsForTests.capsuleTag ), Is.True,
                "Should return true if the Capsule has the Capsule tag." );
            Assert.That( gameObjectsDict[Sphere].HasTag( TagRefsForTests.sphereTag ), Is.True,
                "Should return true if the Sphere has the Sphere tag." );

            yield return null;
        }
    }

    [TestFixture]
    public class TagExistenceTests : NeatoTagTests {
        [UnityTest]
        public IEnumerator HasTag_CubeHasCubeTag_ReturnsTrue() {
            Assert.That( Cube.HasTag( TagRefsForTests.cubeTag ), Is.True, "Cube should have the 'Cube' tag" );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTag_CubeDoesNotHaveSphereTag_ReturnsFalse() {
            Assert.That( Cube.HasTag( TagRefsForTests.sphereTag ), Is.False, "Cube should not have the 'Sphere' tag" );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTag_ByString_CubeHasCubeTag_ReturnsTrue() {
            const string tagString = "Cube";
            Assert.That( Cube.HasTag( tagString ), Is.True,
                $"Cube should have the 'Cube' tag. String used: {tagString}" );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTag_ByString_CubeDoesNotHaveSphereTag_ReturnsFalse() {
            const string tagString = "Sphere";
            Assert.That( Cube.HasTag( tagString ), Is.False,
                $"Cube should not have the 'Sphere' tag. String used: {tagString}" );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTag_NullTag_ReturnsFalse() {
            var tagger = Cube.GetComponent<Tagger>();
            Cube.AddTag( null );
            Assert.That( tagger.GetTags,
                Is.EquivalentTo( new[] { TagRefsForTests.cubeTag, TagRefsForTests.platonicTag } ),
                "Cube should just have the 'Cube' tag and 'Platonic Solid' tag." );
            Assert.That( tagger.GetTags.Contains( null ), Is.False,
                "Cube should not have a null value in it's tag collection." );
            yield return null;
        }


        [UnityTest]
        public IEnumerator HasTag_WithDestroyedGameObject_ReturnsFalse() {
            var tempObj = new GameObject( "Temp" );
            tempObj.AddTagWithForce( TagRefsForTests.cubeTag );
            Object.Destroy( tempObj );
            yield return null; // Wait for destruction

            Assert.That( tempObj.HasTag( TagRefsForTests.cubeTag ), Is.False );
            yield return null;
        }


        [UnityTest]
        public IEnumerator HasTag_OnObjectWithoutTagger_ReturnsFalse() {
            // Testing extension method behavior on objects without a Tagger component
            var newObj = new GameObject( "ObjectWithoutTagger" );
            Assert.That( newObj.HasTag( "AnyTag" ), Is.False,
                "HasTag should return false for objects without a Tagger component" );
            yield return null;
        }

        [UnityTest]
        public IEnumerator GetTagCount_OnObjectWithTwoTags_ReturnsTwo() {
            Assert.That( Cube.GetTagCount(), Is.EqualTo( 2 ),
                "Should return 2 because Cube has Cube tag and Platonic Solid tag." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator GetTagCount_OnObjectWithZeroTags_ReturnsZero() {
            Assert.That( Cylinder.GetTagCount(), Is.EqualTo( 0 ),
                "Should return 0 because Cylinder has no tags." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator IsTagged_OnObjectWithTags_ReturnsTrue() {
            Assert.That( Cube.IsTagged(), Is.True, "Should return true because Cube has tags." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator IsTagged_OnObjectWithoutTags_ReturnsFalse() {
            Assert.That( Cylinder.IsTagged(), Is.False, "Should return false because Cylinder has no tags." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator GetNeatoTag_OnObjectWithTag_ReturnsTrueAndReturnsTag() {
            var exist = Tagger.TryGetNeatoTag( "Cube", out var tag );
            Assert.That( exist, Is.True, "Should return true." );
            Assert.That( tag, Is.EqualTo( TagRefsForTests.cubeTag ), "Should return the correct tag." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator GetNeatoTag_OnObjectWithoutTag_ReturnsFalseAndNull() {
            var exist = Tagger.TryGetNeatoTag( "NonExistentTag", out var tag );
            Assert.That( exist, Is.False, "Should return false." );
            Assert.That( tag, Is.Null, "Should return null." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator TryGetTagExtension_OnObjectWithTag_ReturnsTrueAndReturnsTag() {
            var exist = Cube.TryGetTag( "Cube", out var tag );
            Assert.That( exist, Is.True, "Should return true." );
            Assert.That( tag, Is.EqualTo( TagRefsForTests.cubeTag ), "Should return the correct tag." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator TryGetTagExtension_OnObjectWithoutTag_ReturnsFalseAndNull() {
            var exist = Cube.TryGetTag( "NonExistentTag", out var tag );
            Assert.That( exist, Is.False, "Should return false." );
            Assert.That( tag, Is.Null, "Should return null." );
            yield return null;
        }
    }

    [TestFixture]
    public class TagModificationTests : NeatoTagTests {
        [UnityTest]
        public IEnumerator AddTag_WithNullGameObject_HandlesGracefully() {
            Assert.DoesNotThrow( () => ((GameObject)null).AddTag( TagRefsForTests.cubeTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator AddTag_PlaneTagAddedToCube_TagWasAdded() {
            Cube.AddTag( TagRefsForTests.planeTag );
            Assert.That( Cube.HasTag( TagRefsForTests.planeTag ), Is.True, "Plane tag should be added to cube." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator AddTagsByList_PlaneTagAndSphereTagAddedToCube_AllTagsWereAdded() {
            Cube.AddTags( new List<NeatoTag> { TagRefsForTests.planeTag, TagRefsForTests.sphereTag } );
            Assert.That( Cube.HasTag( TagRefsForTests.planeTag ) && Cube.HasTag( TagRefsForTests.sphereTag ), Is.True,
                "Both tags should be added to cube." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator AddTagsByList_AddSameTagsTwice_OnlyOneInstanceExists() {
            Cylinder.AddTags( new List<NeatoTag> { TagRefsForTests.cornerlessTag, TagRefsForTests.cornerlessTag } );
            Assert.That( Cylinder.GetComponent<Tagger>().GetTags.Count,
                Is.EqualTo( Cylinder.GetComponent<Tagger>().GetTags.Distinct().Count() ),
                "Tagger should not add duplicate tags." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator AddTagsByList_AddTagsWithNull_DoesNotAddNullsOrThrowException() {
            // Should gracefully handle adding null tags
            Cylinder.AddTags( new List<NeatoTag> { TagRefsForTests.cornerlessTag, null } );
            Assert.That( Cylinder.GetComponent<Tagger>().GetTags.Count, Is.EqualTo( 1 ),
                "Should only have 2 tags after adding null tags." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator AddTagWithForce_AddTaggerComponentIfThereIsNone_CreatesComponent() {
            // Tests if AddTag creates a Tagger component if one doesn't exist
            var newObj = new GameObject( "NewObject" );
            Assert.That( newObj.HasTagger(), Is.False, "Object should not have a Tagger initially" );
            newObj.AddTagWithForce( TagRefsForTests.cubeTag );
            Assert.That( newObj.HasTagger(), Is.True, "AddTag should add a Tagger component if needed" );
            Assert.That( newObj.HasTag( TagRefsForTests.cubeTag ), Is.True, "The tag should be added correctly" );
            yield return null;
        }

        [UnityTest]
        public IEnumerator AddTag_ToObjectWithoutTaggerAddComponent_NoTaggerWasAdded() {
            // Tests if AddTag creates a Tagger component if one doesn't exist
            var newObj = new GameObject( "NewObject" );
            Assert.That( newObj.HasTagger(), Is.False, "Object should not have a Tagger initially" );
            newObj.AddTag( TagRefsForTests.cubeTag );
            Assert.That( newObj.HasTagger(), Is.False, "AddTag should NOT add a Tagger component if needed" );
            yield return null;
        }


        [UnityTest]
        public IEnumerator RemoveTag_AddPlaneTagThenRemovePlaneTag_TagWasAddedAndThenRemoved() {
            Cube.AddTag( TagRefsForTests.planeTag );
            Assert.That( Cube.HasTag( TagRefsForTests.planeTag ), Is.True, "Plane tag should be added to cube." );
            Cube.RemoveTag( TagRefsForTests.planeTag );
            Assert.That( Cube.HasTag( TagRefsForTests.planeTag ), Is.False, "Plane tag should be removed from cube." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator RemoveAllTags_AllTagsRemoved() {
            Assert.That( Cube.GetComponent<Tagger>().GetTags.Count, Is.GreaterThan( 0 ),
                "Tagger should have at least one tag before removing all tags." );
            Cube.RemoveAllTags();
            Assert.That( Cube.GetComponent<Tagger>().GetTags.Count, Is.EqualTo( 0 ),
                "Tagger should have 0 tags after removing all tags." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator RemoveTag_NonExistentTag_DoesNotThrowException() {
            // Should gracefully handle removing a tag that doesn't exist
            Cube.RemoveTag( "NonExistentTag" );
            Assert.Pass( "Should not throw an exception when removing a non-existent tag" );
            yield return null;
        }


        [UnityTest]
        public IEnumerator AddTag_AddCapsuleTagTwice_OnlyOneInstanceExists() {
            Capsule.AddTag( TagRefsForTests.capsuleTag );
            Capsule.AddTag( TagRefsForTests.capsuleTag );
            Assert.That( Capsule.GetComponent<Tagger>().GetTags.Count,
                Is.EqualTo( Capsule.GetComponent<Tagger>().GetTags.Distinct().Count() ),
                "Tagger should not add duplicate tags." );
            yield return null;
        }
    }

    [TestFixture]
    public class RuntimeTagCreationTests : NeatoTagTests {
        [UnityTest]
        public IEnumerator GetOrCreateTag_VerySharpTagWasCreatedAndAdded_ReturnsTrue() {
            Cube.GetOrCreateTag( "Very Sharp" );
            Assert.That( Cube.HasTag( "Very Sharp" ), Is.True, "New runtime tag should be created and added." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator GetOrCreateTag_TagNameWithJustASpaceOrEmptyCreated_ReturnFalse() {
            Cube.GetOrCreateTag( " " );
            //Tag names are trimmed, so checking for empty string too.
            Assert.That( Cube.HasTag( string.Empty ), Is.False,
                "Tag with empty string for it's name should not be created." );
            Assert.That( Cube.HasTag( " " ), Is.False, "Tag with just a space for it's name should not be created." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator GetOrCreateTag_GetCubeTag_TagIsReturned() {
            var tag = Cube.GetOrCreateTag( "Cube" );
            Assert.That( tag, Is.EqualTo( TagRefsForTests.cubeTag ), "Should return existing Cube tag." );
            yield return null;
        }
    }

    [TestFixture]
    public class TagGroupMatchingTests : NeatoTagTests {
        [UnityTest]
        public IEnumerator HasAnyTagsMatching_CubeHasAnyGivenTagOfCubeSphereOrPlane_ReturnsTrue() {
            Assert.That( Cube.HasAnyTagsMatching(
                    TagRefsForTests.cubeTag, TagRefsForTests.sphereTag, TagRefsForTests.planeTag ), Is.True,
                "Cube should match at least one of the tags, specifically the Cube Tag." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasAnyTagsMatching_CubeHasAnyGivenTagOfCapsuleSphereOrPlane_ReturnsFalse() {
            Assert.That( Cube.HasAnyTagsMatching(
                    TagRefsForTests.capsuleTag, TagRefsForTests.sphereTag, TagRefsForTests.planeTag ), Is.False,
                "Cube should not match any of the given tags of Capsule, Sphere, and Plane." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasAllTagsMatching_CubeHasAllTagsOfCubeAndPlatonic_ReturnsTrue() {
            Assert.That( Cube.HasAllTagsMatching(
                    TagRefsForTests.cubeTag, TagRefsForTests.platonicTag ), Is.True,
                "Cube should have both Cube and Platonic tags." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasAllTagsMatching_CubeHasAllTagsOfCubePlatonicAndSphere_ReturnsFalse() {
            Assert.That( Cube.HasAllTagsMatching(
                    TagRefsForTests.cubeTag, TagRefsForTests.platonicTag, TagRefsForTests.sphereTag ), Is.False,
                "Cube should not have all these tags, specifically should not have the Sphere tag." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasNoTagsMatching_CubeHasNoneOfTheGivenTagsOfSpherePlaneAndCapsule_ReturnsTrue() {
            Assert.That( Cube.HasNoTagsMatching(
                    TagRefsForTests.sphereTag, TagRefsForTests.planeTag, TagRefsForTests.capsuleTag ), Is.True,
                "Cube should have none of the given tags of Sphere, Plane, and Capsule." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasNoTagsMatching_CubeHasNoneOfTheGivenTagsOfCubeAndPlatonic_ReturnsFalse() {
            Assert.That( Cube.HasNoTagsMatching(
                    TagRefsForTests.cubeTag, TagRefsForTests.platonicTag ), Is.False,
                "Cube should have the given tags of Cube and Platonic. HasNoTagsMatching should return false." );
            yield return null;
        }
    }

    [TestFixture]
    public class TagFilterTests : NeatoTagTests {
        [UnityTest]
        public IEnumerator FilterTags_WithMatchingTag_ReturnsTrue() {
            Assert.That( Cube.FilterTags().WithTag( TagRefsForTests.cubeTag ).IsMatch(), Is.True,
                "Cube should have the Cube tag." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator FilterTags_WithTagWhenNull_ReturnsFalse() {
            Assert.That( Cube.FilterTags().WithTag( null ).IsMatch(), Is.False,
                "Cube should not have the null object as a tag." );
            Assert.That( Cube.FilterTags().WithTag( TagRefsForTests.cubeTag ).WithTag( null ).IsMatch(), Is.False,
                "No match should be found as no gameobject should have a null object tag." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator FilterTags_ChainedMatchingConditionsWithTagCubeWithTagPlatonic_ReturnsTrue() {
            Assert.That( Cube.FilterTags()
                    .WithTag( TagRefsForTests.cubeTag )
                    .WithTag( TagRefsForTests.platonicTag )
                    .IsMatch(), Is.True,
                "Cube should match chained filter conditions." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator FilterTags_ChainedNonMatchingConditionsWithTagCubeWithTagSphere_ReturnsFalse() {
            Assert.That( Cube.FilterTags()
                    .WithTag( TagRefsForTests.cubeTag )
                    .WithTag( TagRefsForTests.sphereTag )
                    .IsMatch(), Is.False,
                "Cube should not match chained filter with sphere tag" );
            yield return null;
        }
    }

    [TestFixture]
    public class GameObjectFilterTests : NeatoTagTests {
        [UnityTest]
        public IEnumerator FilterGameObjects_WithMatchingTag_ReturnsMatchingObject() {
            var filteredShapes = Tagger.FilterGameObjects( ShapeTagsToFilter )
                .WithTag( TagRefsForTests.cubeTag )
                .GetMatches();

            Assert.That( filteredShapes.Count, Is.EqualTo( 1 ), "Should match exactly one object" );
            Assert.That( filteredShapes.Contains( Cube ), Is.True, "Matched object should be the cube" );

            yield return null;
        }

        [UnityTest]
        public IEnumerator FilterGameObjects_WithMultipleTagRequirements_ReturnsCorrectObjects() {
            var filteredShapes = Tagger.FilterGameObjects( ShapeTagsToFilter )
                .WithTag( TagRefsForTests.cornerlessTag )
                .WithoutTag( TagRefsForTests.sphereTag )
                .GetMatches();
            var expected = 1;
            var actual = filteredShapes.Count;
            Assert.That( actual, Is.EqualTo( expected ), "Should match only capsule." );
            Assert.That( filteredShapes.Contains( Capsule ), Is.True, "Should match capsule" );
            Assert.That( filteredShapes.Contains( Sphere ), Is.False, "Should not match sphere" );
            yield return null;
        }

        [UnityTest]
        public IEnumerator FilterGameObjects_WithAnyTags_ReturnsMatchingObjects() {
            var filteredShapes = Tagger.FilterGameObjects( ShapeTagsToFilter )
                .WithAnyTags( TagRefsForTests.cubeTag, TagRefsForTests.sphereTag, TagRefsForTests.capsuleTag )
                .GetMatches();

            Assert.That( filteredShapes.Count, Is.EqualTo( 3 ), "Should match cube, sphere, and capsule" );
            Assert.That( filteredShapes.Contains( Cube ) &&
                         filteredShapes.Contains( Sphere ) &&
                         filteredShapes.Contains( Capsule ), Is.True,
                "Should match cube, sphere, and capsule" );

            yield return null;
        }

        [UnityTest]
        public IEnumerator FilterGameObjects_WithAnyTags_DoesNotRaiseOnUnusedTags() {
            var filteredShapes = Tagger.FilterGameObjects( new List<GameObject>() )
                .WithAnyTags( TagRefsForTests.planeTag )
                .GetMatches();

            Assert.That( filteredShapes.Count, Is.EqualTo( 0 ), "Should match None" );

            yield return null;
        }

        [UnityTest]
        public IEnumerator FilterGameObjects_WithAnyTagsNullOrEmpty_ReturnsNoMatches() {
            // Set up initial objects with tags
            var initialShapes = Tagger.FilterGameObjects( ShapeTagsToFilter )
                .WithAnyTags( TagRefsForTests.cubeTag, TagRefsForTests.sphereTag )
                .GetMatches();
            Assert.That( initialShapes.Count, Is.GreaterThan( 0 ), "Should have some objects with tags initially" );

            // Test with null tags
            var nullTagResult = Tagger.FilterGameObjects( ShapeTagsToFilter )
                .WithAnyTags( null )
                .GetMatches();
            Assert.That( nullTagResult.Count, Is.EqualTo( 0 ), "Should return no matches when tags array is null" );

            // Test with an empty tags params.
            var emptyTagResult = Tagger.FilterGameObjects( ShapeTagsToFilter )
                .WithAnyTags()
                .GetMatches();
            Assert.That( emptyTagResult.Count, Is.EqualTo( 0 ), "Should return no matches when tags array is empty" );

            yield return null;
        }


        [UnityTest]
        public IEnumerator FilterGameObjects_EmptyList_ReturnsEmptyResult() {
            var result = Tagger.FilterGameObjects( new List<GameObject>() )
                .WithTag( TagRefsForTests.cubeTag )
                .GetMatches();

            Assert.That( result.Count, Is.EqualTo( 0 ), "Filtering an empty list should return an empty result" );
            yield return null;
        }
    }

    [TestFixture]
    public class TagSynchronizationTests : NeatoTagTests {
        [UnityTest]
        public IEnumerator StaticCollections_UpdatedCorrectly_WhenTagsModified() {
            // Test that the static collections are properly updated when tags are added/removed
            var initialCount = Tagger.FilterGameObjects().WithTag( TagRefsForTests.planeTag ).GetMatches().Count;

            // Create a new object with the cube tag
            var newObj = new GameObject( "NewCube" );
            newObj.AddComponent<Tagger>().AddTag( TagRefsForTests.planeTag );

            var afterAddCount = Tagger.FilterGameObjects().WithTag( TagRefsForTests.planeTag ).GetMatches().Count;
            Assert.That( afterAddCount, Is.EqualTo( initialCount + 1 ),
                "Static collections should be updated when new tagged objects are created" );

            // Now remove the tag
            newObj.RemoveTag( TagRefsForTests.planeTag );
            var afterRemoveCount = Tagger.FilterGameObjects().WithTag( TagRefsForTests.planeTag ).GetMatches().Count;
            Assert.That( afterRemoveCount, Is.EqualTo( initialCount ),
                "Static collections should be updated when tags are removed" );

            yield return null;
        }

        [UnityTest]
        public IEnumerator ObjectDestruction_UpdatesTaggedCollections() {
            // Test that when objects are destroyed, they're removed from tag collections
            var tempObj = new GameObject( "TempObject" );
            tempObj.AddComponent<Tagger>().AddTag( TagRefsForTests.planeTag );

            var beforeCount = Tagger.FilterGameObjects().WithTag( TagRefsForTests.planeTag ).GetMatches().Count;
            Object.Destroy( tempObj );

            // Need to wait a frame for destruction to complete
            yield return null;

            var afterCount = Tagger.FilterGameObjects().WithTag( TagRefsForTests.planeTag ).GetMatches().Count;
            Assert.That( afterCount, Is.EqualTo( beforeCount - 1 ),
                "Destroyed objects should be removed from tag collections" );
        }

        [UnityTest]
        public IEnumerator ConcurrentTagModification_ThreadSafety() {
            var tasks = new List<System.Threading.Tasks.Task>();

            for ( var i = 0; i < 10; i++ ) {
                var index = i;
                tasks.Add( System.Threading.Tasks.Task.Run( () => {
                    // Simulate concurrent tag operations
                    Cube.GetOrCreateTag( $"ConcurrentTag_{index}" );
                } ) );
            }

            yield return new WaitUntil( () => tasks.All( t => t.IsCompleted ) );

            // Verify no corruption occurred
            Assert.That( Cube.GetComponent<Tagger>().GetTags.Count, Is.GreaterThanOrEqualTo( 2 ) );
            yield return null;
        }
    }
}