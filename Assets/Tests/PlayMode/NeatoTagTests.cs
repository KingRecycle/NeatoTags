using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CharlieMadeAThing.NeatoTags.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CharlieMadeAThing.NeatoTags.Tests.PlayMode {
    public abstract class NeatoTagTestBase {
        protected GameObject Cube;
        protected GameObject Cylinder;
        protected GameObject Sphere;
        protected GameObject Capsule;
        protected GameObject Plane;
        protected GameObject[] Shapes;
        protected TagRefsForTests TagRefsForTests;

        [UnitySetUp]
        public IEnumerator SetUp() {
            SceneManager.LoadScene( "TestScene" );
            yield return null; // Wait for a scene to load

            TagRefsForTests = GameObject.Find( "TagRefs" ).GetComponent<TagRefsForTests>();
            Cube = GameObject.Find( "Cube" ); //Has two tags (Cube and Platonic Solid)
            Cylinder = GameObject.Find( "Cylinder" ); //Has two tags (Cylinder and Cornerless)
            Sphere = GameObject.Find( "Sphere" ); //Has two tags (Sphere and Cornerless)
            Capsule = GameObject.Find( "Capsule" ); //Has two tags (Capsule and Cornerless)
            Plane = GameObject.Find( "Plane" ); //Does not have a tagger
            Shapes = new[] { Cube, Cylinder, Sphere, Capsule, Plane };
            
        }
    }

    [TestFixture]
    public class TaggerBasicTests : NeatoTagTestBase {
        [UnityTest]
        public IEnumerator HasTagger_CubeWithTagger_ReturnsTrue() {
            Assert.That( Cube.HasTagger(), Is.True, "HasTagger() should return true if a Tagger component is on the GameObject." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTagger_PlaneWithoutTagger_ReturnsFalse() {
            Assert.That( Plane.HasTagger(), Is.False, "HasTagger() should return false if a Tagger component is not on the GameObject." );
            yield return null;
        }
    }

    [TestFixture]
    public class TagExistenceTests : NeatoTagTestBase {
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
            Assert.That( Cube.HasTag( tagString ), Is.True, $"Cube should have the 'Cube' tag. String used: {tagString}" );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTag_ByString_CubeDoesNotHaveSphereTag_ReturnsFalse() {
            const string tagString = "Sphere";
            Assert.That( Cube.HasTag( tagString ), Is.False, $"Cube should not have the 'Sphere' tag. String used: {tagString}" );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTag_NullTag_ReturnsFalse() {
            var tagger = Cube.GetComponent<Tagger>();
            Cube.AddTag( null );
            Assert.That(  tagger.GetTags, Is.EquivalentTo( new[] {TagRefsForTests.cubeTag, TagRefsForTests.platonicTag} ), "Cube should have the 'Cube' tag and 'Platonic Solid' tag." );
            Assert.That( tagger.GetTags.Contains( null ), Is.False, "Cube should not have a null value in it's tag collection." );
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator HasTag_OnObjectWithoutTagger_ReturnsFalse() {
            // Testing extension method behavior on objects without a Tagger component
            GameObject newObj = new GameObject("ObjectWithoutTagger");
            Assert.That(newObj.HasTag("AnyTag"), Is.False, "HasTag should return false for objects without a Tagger component");
            yield return null;
        }

    }

    [TestFixture]
    public class TagModificationTests : NeatoTagTestBase {
        [UnityTest]
        public IEnumerator AddTag_PlaneTagAddedToCube_TagWasAdded() {
            Cube.AddTag( TagRefsForTests.planeTag );
            Assert.That( Cube.HasTag( TagRefsForTests.planeTag ), Is.True, "Plane tag should be added to cube." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator AddTags_PlaneTagAndSphereTagAddedToCube_AllTagsWereAdded() {
            Cube.AddTags( TagRefsForTests.planeTag, TagRefsForTests.sphereTag );
            Assert.That( Cube.HasTag( TagRefsForTests.planeTag ) && Cube.HasTag( TagRefsForTests.sphereTag ), Is.True,
                "Both tags should be added to cube." );
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator AddTag_ToObjectWithoutTaggerAddComponentFlagSetToTrue_CreatesComponent() {
            // Tests if AddTag creates a Tagger component if one doesn't exist
            GameObject newObj = new GameObject("NewObject");
            Assert.That(newObj.HasTagger(), Is.False, "Object should not have a Tagger initially");
            newObj.AddTag(TagRefsForTests.cubeTag, true);
            Assert.That(newObj.HasTagger(), Is.True, "AddTag should add a Tagger component if needed");
            Assert.That(newObj.HasTag(TagRefsForTests.cubeTag), Is.True, "The tag should be added correctly");
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator AddTag_ToObjectWithoutTaggerAddComponentFlagSetToFalse_NoTaggerWasAdded() {
            // Tests if AddTag creates a Tagger component if one doesn't exist
            GameObject newObj = new GameObject("NewObject");
            Assert.That(newObj.HasTagger(), Is.False, "Object should not have a Tagger initially");
            newObj.AddTag(TagRefsForTests.cubeTag);
            Assert.That(newObj.HasTagger(), Is.False, "AddTag should NOT add a Tagger component if needed");
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
            Assert.That ( Cube.GetComponent<Tagger>().GetTags.Count, Is.GreaterThan( 0 ),
                "Tagger should have at least one tag before removing all tags." );
            Cube.RemoveAllTags();
            Assert.That( Cube.GetComponent<Tagger>().GetTags.Count, Is.EqualTo( 0 ),
                "Tagger should have 0 tags after removing all tags." );
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator RemoveTag_NonExistentTag_DoesNotThrowException() {
            // Should gracefully handle removing a tag that doesn't exist
            Cube.RemoveTag("NonExistentTag");
            Assert.Pass("Should not throw an exception when removing a non-existent tag");
            yield return null;
        }


        [UnityTest]
        public IEnumerator AddTag_AddCylinderTagTwice_OnlyOneInstanceExists() {
            Cylinder.AddTag( TagRefsForTests.cylinderTag );
            Cylinder.AddTag( TagRefsForTests.cylinderTag );
            Assert.That( Cylinder.GetComponent<Tagger>().GetTags.Count, Is.EqualTo( Cylinder.GetComponent<Tagger>().GetTags.Distinct().Count() ),
                "Tagger should not add duplicate tags." );
            yield return null;
        }
    }

    [TestFixture]
    public class RuntimeTagCreationTests : NeatoTagTestBase {
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
            Assert.That( Cube.HasTag( string.Empty ), Is.False, "Tag with empty string for it's name should not be created." );
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
    public class TagGroupMatchingTests : NeatoTagTestBase {
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
    public class TagFilterTests : NeatoTagTestBase {
        [UnityTest]
        public IEnumerator FilterTags_WithMatchingTag_ReturnsTrue() {
            Assert.That( Cube.FilterTags().WithTag( TagRefsForTests.cubeTag ).IsMatch(), Is.True,
                "Cube should have the Cube tag." );
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
    public class GameObjectFilterTests : NeatoTagTestBase {
        const int MeasurementCount = 10000;

        [UnityTest]
        public IEnumerator FilterGameObjects_WithMatchingTag_ReturnsMatchingObject() {
            var filteredShapes = Tagger.FilterGameObjects( Shapes )
                .WithTag( TagRefsForTests.cubeTag )
                .GetMatches();

            Assert.That( filteredShapes.Count, Is.EqualTo( 1 ), "Should match exactly one object" );
            Assert.That( filteredShapes.Contains( Cube ), Is.True, "Matched object should be the cube" );

            yield return null;
        }

        [UnityTest]
        public IEnumerator FilterGameObjects_WithMultipleTagRequirements_ReturnsCorrectObjects() {
            var filteredShapes = Tagger.FilterGameObjects( Shapes )
                .WithTag( TagRefsForTests.cornerlessTag )
                .WithoutTag( TagRefsForTests.sphereTag )
                .GetMatches();
            int expected = 2;
            int actual = filteredShapes.Count;
            Assert.That( actual, Is.EqualTo( expected ), "Should match only capsule and Cylinder" );
            Assert.That( filteredShapes.Contains( Capsule ), Is.True, "Should match capsule" );
            Assert.That( filteredShapes.Contains( Cylinder ), Is.True, "Should match Cylinder" );
            Assert.That( filteredShapes.Contains( Sphere ), Is.False, "Should not match sphere" );
            yield return null;
        }

        [UnityTest]
        public IEnumerator FilterGameObjects_WithAnyTags_ReturnsMatchingObjects() {
            var filteredShapes = Tagger.FilterGameObjects( Shapes )
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
        public IEnumerator FilterGameObjects_EmptyList_ReturnsEmptyResult() {
            var result = Tagger.FilterGameObjects(new List<GameObject>())
                .WithTag(TagRefsForTests.cubeTag)
                .GetMatches();
        
            Assert.That(result.Count, Is.EqualTo(0), "Filtering an empty list should return an empty result");
            yield return null;
        }

    }
    
    [TestFixture]
    public class TagSynchronizationTests : NeatoTagTestBase {
        [UnityTest]
        public IEnumerator StaticCollections_UpdatedCorrectly_WhenTagsModified() {
            // Test that the static collections are properly updated when tags are added/removed
            int initialCount = Tagger.FilterGameObjects().WithTag(TagRefsForTests.planeTag).GetMatches().Count;
        
            // Create a new object with the cube tag
            GameObject newObj = new GameObject("NewCube");
            newObj.AddComponent<Tagger>().AddTag(TagRefsForTests.planeTag);
        
            int afterAddCount = Tagger.FilterGameObjects().WithTag(TagRefsForTests.planeTag).GetMatches().Count;
            Assert.That(afterAddCount, Is.EqualTo(initialCount + 1), "Static collections should be updated when new tagged objects are created");
        
            // Now remove the tag
            newObj.RemoveTag(TagRefsForTests.planeTag);
            int afterRemoveCount = Tagger.FilterGameObjects().WithTag(TagRefsForTests.planeTag).GetMatches().Count;
            Assert.That(afterRemoveCount, Is.EqualTo(initialCount), "Static collections should be updated when tags are removed");
        
            yield return null;
        }
    
        [UnityTest]
        public IEnumerator ObjectDestruction_UpdatesTaggedCollections() {
            // Test that when objects are destroyed, they're removed from tag collections
            GameObject tempObj = new GameObject("TempObject");
            tempObj.AddComponent<Tagger>().AddTag(TagRefsForTests.planeTag);
        
            int beforeCount = Tagger.FilterGameObjects().WithTag(TagRefsForTests.planeTag).GetMatches().Count;
            Object.Destroy(tempObj);
        
            // Need to wait a frame for destruction to complete
            yield return null;
        
            int afterCount = Tagger.FilterGameObjects().WithTag(TagRefsForTests.planeTag).GetMatches().Count;
            Assert.That(afterCount, Is.EqualTo(beforeCount - 1), "Destroyed objects should be removed from tag collections");
        }
    }
}