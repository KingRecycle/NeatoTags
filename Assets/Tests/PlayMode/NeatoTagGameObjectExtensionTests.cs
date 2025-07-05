using System.Collections;
using CharlieMadeAThing.NeatoTags.Core;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace CharlieMadeAThing.NeatoTags.Tests.PlayMode {
    [TestFixture]
    public class NeatoTagsExtensionsBasicTests : NeatoTagTests {
        [UnityTest]
        public IEnumerator HasTagger_CubeWithTagger_ReturnsTrue() {
            Assert.That( Cube.HasTagger(), Is.True,
                "HasTagger() should return true if a Tagger component is on the GameObject." );
            yield return null;
        }

        [UnityTest]
        public IEnumerator HasTagger_PlaneWithoutTagger_ReturnsFalse() {
            Assert.That( Plane.HasTagger(), Is.False,
                "HasTagger() should return false if a Tagger component is not on the GameObject." );
            yield return null;
        }
    }
}