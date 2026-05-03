using CharlieMadeAThing.NeatoTags.Core;
using NUnit.Framework;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Tests.PlayMode {
    // Regression tests for GitHub issue #19.
    // Tagger.HasTagger and Tagger.TryGetTagger passed the GameObject straight into
    // Dictionary.ContainsKey / TryGetValue, which throw ArgumentNullException on a
    // null key. The 14 query extensions in NeatoTagsExtensions (HasTag /
    // HasAnyTagsMatching / HasAllTagsMatching / HasNoTagsMatching, all overloads)
    // all routed through these two methods without their own null guard, so a null
    // GameObject reference at any of those call sites threw instead of returning
    // false (or true, for HasNoTagsMatching).
    //
    // Two tests directly exercise the upstream fix; the third confirms the cascade
    // through one of the affected extensions (HasTag is representative — the other
    // 13 extensions share its shape).
    [TestFixture]
    public class TaggerNullGameObjectQueryTests {

        [Test]
        public void HasTagger_NullGameObject_ReturnsFalse() {
            Assert.That( Tagger.HasTagger( null ), Is.False,
                "HasTagger(null) must return false instead of throwing ArgumentNullException." );
        }

        [Test]
        public void TryGetTagger_NullGameObject_ReturnsFalseAndOutsNull() {
            var result = Tagger.TryGetTagger( null, out var tagger );
            Assert.That( result, Is.False,
                "TryGetTagger(null, ...) must return false instead of throwing." );
            Assert.That( tagger, Is.Null,
                "TryGetTagger(null, ...) must out a null tagger." );
        }

        [Test]
        public void HasTag_OnNullGameObject_ReturnsFalse() {
            //Cascade verification: confirm the upstream fix propagates through one of
            //the 14 NeatoTagsExtensions query methods.
            Assert.That( ( (GameObject)null ).HasTag( "anyTagName" ), Is.False,
                "GameObject.HasTag on null receiver must return false instead of throwing." );
        }
    }
}
