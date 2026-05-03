using CharlieMadeAThing.NeatoTags.Core;
using NUnit.Framework;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Tests.PlayMode {
    // Regression tests for GitHub issue #12.
    // NeatoTagsExtensions.AddTag(GameObject, NeatoTag, bool) interpolated tag.name in
    // the "gameObject is null" warning before the tag null-check, throwing
    // NullReferenceException whenever both arguments were null. Both public entry
    // points (AddTag and AddTagWithForce) flow through the same private overload.
    [TestFixture]
    public class NeatoTagsExtensionsAddTagNullTests {

        [Test]
        public void AddTag_NullGameObjectAndNullTag_DoesNotThrow() {
            Assert.DoesNotThrow( () => ( (GameObject)null ).AddTag( null ) );
        }

        [Test]
        public void AddTagWithForce_NullGameObjectAndNullTag_DoesNotThrow() {
            Assert.DoesNotThrow( () => ( (GameObject)null ).AddTagWithForce( null ) );
        }
    }
}
