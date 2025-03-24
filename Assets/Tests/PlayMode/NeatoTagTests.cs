using System.Collections;
using CharlieMadeAThing.NeatoTags.Core;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CharlieMadeAThing.NeatoTags.Tests.PlayMode 
{
    public abstract class NeatoTagTestBase 
    {
        protected GameObject Cube;
        protected GameObject Cylinder;
        protected GameObject Sphere;
        protected GameObject Capsule;
        protected GameObject Plane;
        protected GameObject[] Shapes;
        protected TagRefsForTests TagRefsForTests;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SceneManager.LoadScene("TestScene");
            yield return null; // Wait for scene to load

            TagRefsForTests = GameObject.Find("TagRefs").GetComponent<TagRefsForTests>();
            Cube = GameObject.Find("Cube");
            Cylinder = GameObject.Find("Cylinder");
            Sphere = GameObject.Find("Sphere");
            Capsule = GameObject.Find("Capsule");
            Plane = GameObject.Find("Plane");
            Shapes = new[] { Cube, Cylinder, Sphere, Capsule, Plane };
        }
    }

    [TestFixture]
    public class TaggerBasicTests : NeatoTagTestBase
    {
        [UnityTest]
        public IEnumerator HasTagger_ObjectWithTagger_ReturnsTrue()
        {
            Assert.IsTrue(Cube.HasTagger(), "Cube should have a Tagger component");
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTagger_ObjectWithoutTagger_ReturnsFalse()
        {
            Assert.IsFalse(Plane.HasTagger(), "Plane should not have a Tagger component");
            yield return null;
        }
    }

    [TestFixture]
    public class TagExistenceTests : NeatoTagTestBase
    {
        [UnityTest]
        public IEnumerator HasTag_ObjectHasTag_ReturnsTrue()
        {
            Assert.IsTrue(Cube.HasTag(TagRefsForTests.cubeTag), "Cube should have the 'Cube' tag");
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTag_ObjectDoesNotHaveTag_ReturnsFalse()
        {
            Assert.IsFalse(Cube.HasTag(TagRefsForTests.sphereTag), "Cube should not have the 'Sphere' tag");
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTag_ByString_ObjectHasTag_ReturnsTrue()
        {
            Assert.IsTrue(Cube.HasTag("Cube"), "Cube should have the 'Cube' tag by string reference");
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTag_NullTag_ReturnsFalse()
        {
            Cube.AddTag(null);
            Assert.IsFalse(Cube.HasTag((NeatoTag)null), "Null tag should not be found");
            yield return null;
        }
    }

    [TestFixture]
    public class TagModificationTests : NeatoTagTestBase
    {
        [UnityTest]
        public IEnumerator AddTag_NewTag_TagIsAdded()
        {
            Cube.AddTag(TagRefsForTests.planeTag);
            Assert.IsTrue(Cube.HasTag(TagRefsForTests.planeTag), "Plane tag should be added to cube");
            yield return null;
        }

        [UnityTest]
        public IEnumerator AddTags_MultipleTags_AllTagsAreAdded()
        {
            Cube.AddTags(TagRefsForTests.planeTag, TagRefsForTests.sphereTag);
            Assert.IsTrue(Cube.HasTag(TagRefsForTests.planeTag) && Cube.HasTag(TagRefsForTests.sphereTag), 
                "Both tags should be added to cube");
            yield return null;
        }

        [UnityTest]
        public IEnumerator RemoveTag_ExistingTag_TagIsRemoved()
        {
            Cube.AddTag(TagRefsForTests.planeTag);
            Cube.RemoveTag(TagRefsForTests.planeTag);
            Assert.IsFalse(Cube.HasTag(TagRefsForTests.planeTag), "Plane tag should be removed from cube");
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator RemoveAllTags_AllTagsRemoved()
        {
            Cube.AddTags(TagRefsForTests.planeTag, TagRefsForTests.sphereTag);
            Cube.RemoveAllTags();
            Assert.IsFalse(Cube.HasTag(TagRefsForTests.planeTag) && Cube.HasTag(TagRefsForTests.sphereTag), 
                "All tags should be removed from cube");
            Assert.AreEqual(0, Cube.GetComponent<Tagger>().GetTags.Count, 
                "Tagger should have 0 tags after removing all tags");
            yield return null;
        }

        [UnityTest]
        public IEnumerator AddTag_DuplicateTag_OnlyOneInstanceExists()
        {
            Cylinder.AddTag(TagRefsForTests.cylinderTag);
            Cylinder.AddTag(TagRefsForTests.cylinderTag);
            Assert.AreEqual(1, Cylinder.GetComponent<Tagger>().GetTags.Count, 
                "Tagger should not add duplicate tags");
            yield return null;
        }
    }

    [TestFixture]
    public class RuntimeTagCreationTests : NeatoTagTestBase
    {
        [UnityTest]
        public IEnumerator GetOrCreateTag_NewTag_TagCreatedAndAdded()
        {
            Cube.GetOrCreateTag("Very Sharp");
            Assert.IsTrue(Cube.HasTag("Very Sharp"), "New runtime tag should be created and added");
            yield return null;
        }

        [UnityTest]
        public IEnumerator GetOrCreateTag_EmptyName_TagNotCreated()
        {
            Cube.GetOrCreateTag(" ");
            Assert.IsFalse(Cube.HasTag(" "), "Empty tag should not be created");
            Assert.IsFalse(Cube.HasTag(""), "Empty tag should not be created");
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator GetOrCreateTag_GetTag_TagIsReturned()
        {
            var tag = Cube.GetOrCreateTag("Cube");
            Assert.AreEqual(TagRefsForTests.cubeTag, tag, "Should return existing tag");
            yield return null;
        }
    }

    [TestFixture]
    public class TagGroupMatchingTests : NeatoTagTestBase
    {
        [UnityTest]
        public IEnumerator HasAnyTagsMatching_ObjectHasOneTag_ReturnsTrue()
        {
            Assert.IsTrue(Cube.HasAnyTagsMatching(
                TagRefsForTests.cubeTag, TagRefsForTests.sphereTag, TagRefsForTests.planeTag),
                "Cube should match at least one of the tags");
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasAnyTagsMatching_ObjectHasNoMatchingTags_ReturnsFalse()
        {
            Assert.IsFalse(Cube.HasAnyTagsMatching(
                TagRefsForTests.capsuleTag, TagRefsForTests.sphereTag, TagRefsForTests.planeTag),
                "Cube should not match any of these tags");
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasAllTagsMatching_ObjectHasAllTags_ReturnsTrue()
        {
            Assert.IsTrue(Cube.HasAllTagsMatching(
                TagRefsForTests.cubeTag, TagRefsForTests.platonicTag),
                "Cube should have both Cube and Platonic tags");
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasAllTagsMatching_ObjectMissingSomeTag_ReturnsFalse()
        {
            Assert.IsFalse(Cube.HasAllTagsMatching(
                TagRefsForTests.cubeTag, TagRefsForTests.platonicTag, TagRefsForTests.sphereTag),
                "Cube should not have all these tags");
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasNoTagsMatching_ObjectHasNoneOfTheTags_ReturnsTrue()
        {
            Assert.IsTrue(Cube.HasNoTagsMatching(
                TagRefsForTests.sphereTag, TagRefsForTests.planeTag, TagRefsForTests.capsuleTag),
                "Cube should have none of these tags");
            yield return null;
        }
    }

    [TestFixture]
    public class TagFilterTests : NeatoTagTestBase
    {
        [UnityTest]
        public IEnumerator FilterTags_WithMatchingTag_ReturnsTrue()
        {
            Assert.IsTrue(Cube.FilterTags().WithTag(TagRefsForTests.cubeTag).IsMatch(),
                "Cube should match its own tag");
            yield return null;
        }

        [UnityTest]
        public IEnumerator FilterTags_ChainedMatchingConditions_ReturnsTrue()
        {
            Assert.IsTrue(Cube.FilterTags()
                .WithTag(TagRefsForTests.cubeTag)
                .WithTag(TagRefsForTests.platonicTag)
                .IsMatch(),
                "Cube should match chained filter conditions");
            yield return null;
        }

        [UnityTest]
        public IEnumerator FilterTags_ChainedNonMatchingConditions_ReturnsFalse()
        {
            Assert.IsFalse(Cube.FilterTags()
                .WithTag(TagRefsForTests.cubeTag)
                .WithTag(TagRefsForTests.sphereTag)
                .IsMatch(),
                "Cube should not match chained filter with sphere tag");
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

            Assert.AreEqual( 1, filteredShapes.Count, "Should match exactly one object" );
            Assert.IsTrue( filteredShapes.Contains( Cube ), "Matched object should be the cube" );

            yield return null;
        }

        [UnityTest]
        public IEnumerator FilterGameObjects_WithMultipleTagRequirements_ReturnsCorrectObjects() {
            var filteredShapes = Tagger.FilterGameObjects( Shapes )
                .WithTag( TagRefsForTests.cornerlessTag )
                .WithoutTag( TagRefsForTests.sphereTag )
                .GetMatches();

            Assert.AreEqual( 1, filteredShapes.Count, "Should match only capsule" );
            Assert.IsTrue( filteredShapes.Contains( Capsule ), "Should match capsule" );
            Assert.IsFalse( filteredShapes.Contains( Sphere ), "Should not match sphere" );

            yield return null;
        }

        [UnityTest]
        public IEnumerator FilterGameObjects_WithAnyTags_ReturnsMatchingObjects() {
            var filteredShapes = Tagger.FilterGameObjects( Shapes )
                .WithAnyTags( TagRefsForTests.cubeTag, TagRefsForTests.sphereTag, TagRefsForTests.capsuleTag )
                .GetMatches();

            Assert.AreEqual( 3, filteredShapes.Count, "Should match cube, sphere, and capsule" );
            Assert.IsTrue( filteredShapes.Contains( Cube ) &&
                           filteredShapes.Contains( Sphere ) &&
                           filteredShapes.Contains( Capsule ),
                "Should match cube, sphere, and capsule" );

            yield return null;
        }
    }
}