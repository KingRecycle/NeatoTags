using System.Collections;
using CharlieMadeAThing.NeatoTags.Core;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace CharlieMadeAThing.NeatoTags.Tests.PlayMode {
    // Regression tests for GitHub issue #8.
    // UnregisterTag iterates s_taggedObjects[tag] while RemoveTag mutates that same HashSet,
    // raising InvalidOperationException whenever the tag is currently applied to any GameObject.
    [TestFixture]
    public class TaggerRegistryUnregisterTagTests : NeatoTagTests {

        //No explicit registry reset here. The next [UnitySetUp] reloads TestScene, which destroys
        //the old Taggers (their OnDestroy cleans up registry entries) and re-registers fresh ones
        //via Tagger.Awake. Calling TaggerRegistry.ResetRegistry() here would wipe s_taggedObjects
        //before the destroy phase, which surfaces a separate bug in UnregisterGameObjectFromTag
        //(TaggerRegistry.cs:215 — indexer used without TryGetValue, mirror of issue #3).

        [UnityTest]
        public IEnumerator UnregisterTag_WhenTagIsAppliedToGameObjects_DoesNotThrow() {
            //Precondition: Cube is tagged with cubeTag (proven by CheckIfTestSceneStateIsValid in the base class).
            Assert.DoesNotThrow( () => TaggerRegistry.UnregisterTag( TagRefsForTests.cubeTag ) );
            yield return null;
        }

        [UnityTest]
        public IEnumerator UnregisterTag_WhenTagIsAppliedToGameObjects_RemovesTagFromAllTaggers() {
            TaggerRegistry.UnregisterTag( TagRefsForTests.cubeTag );

            Assert.That( Cube.GetComponent<Tagger>().HasTag( TagRefsForTests.cubeTag ), Is.False,
                "Cube should no longer report having cubeTag after UnregisterTag." );

            Assert.That( TaggerRegistry.GetStaticTaggedObjectsDictionary()
                    .ContainsKey( TagRefsForTests.cubeTag ), Is.False,
                "Registry should no longer contain an entry for cubeTag after UnregisterTag." );

            yield return null;
        }
    }
}
