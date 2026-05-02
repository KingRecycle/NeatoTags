using System.Collections;
using CharlieMadeAThing.NeatoTags.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CharlieMadeAThing.NeatoTags.Tests.PlayMode {
    // Regression tests for GitHub issue #9.
    //   1. [DisallowMultipleComponent] on Tagger should prevent two Taggers on one GameObject.
    //   2. RegisterTagger should not throw if the same GameObject is registered twice
    //      (covers domain-reload and direct-call paths the attribute can't reach).
    [TestFixture]
    public class TaggerRegistryRegisterTaggerTests {

        [UnityTest]
        public IEnumerator AddComponent_SecondTaggerOnSameGameObject_IsRefused() {
            var go = new GameObject( "DuplicateTaggerHost" );
            var first = go.AddComponent<Tagger>();

            //AddComponent on a [DisallowMultipleComponent] type that's already present
            //prints an engine-level error message. On some Unity versions this error is
            //emitted from native code and doesn't pass through LogAssert's hook, so we
            //suppress the auto-fail-on-error behaviour entirely for this scope.
            LogAssert.ignoreFailingMessages = true;
            try {
                Tagger second = null;
                Assert.DoesNotThrow( () => second = go.AddComponent<Tagger>() );

                Assert.That( first, Is.Not.Null, "First Tagger should be added." );
                Assert.That( second, Is.Null,
                    "Second Tagger should be refused by [DisallowMultipleComponent]." );
                Assert.That( go.GetComponents<Tagger>().Length, Is.EqualTo( 1 ),
                    "Only one Tagger component should exist on the GameObject." );
            }
            finally {
                LogAssert.ignoreFailingMessages = false;
            }

            Object.Destroy( go );
            yield return null;
        }

        [UnityTest]
        public IEnumerator InitializeNewTagger_CalledTwiceForSameGameObject_DoesNotThrow() {
            //Simulates the domain-reload / re-Awake path that [DisallowMultipleComponent]
            //can't prevent: the same Tagger instance gets registered with the registry twice.
            //Pre-fix, the second RegisterTagger call throws ArgumentException because
            //Dictionary.Add rejects duplicate keys.
            var go = new GameObject( "DuplicateRegistrationHost" );
            var tagger = go.AddComponent<Tagger>();   //Awake calls InitializeNewTagger once.

            Assert.DoesNotThrow( () => TaggerRegistry.InitializeNewTagger( go, tagger ) );

            Object.Destroy( go );
            yield return null;
        }
    }
}
