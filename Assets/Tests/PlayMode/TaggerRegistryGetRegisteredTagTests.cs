using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CharlieMadeAThing.NeatoTags.Core;
using NUnit.Framework;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Tests.PlayMode {
    // Regression test for GitHub issue #47.
    // TaggerRegistry.GetRegisteredTag iterates s_taggedObjects.Keys via
    // FirstOrDefault(t => t.name == tagName). When any key is a Unity-fake-null
    // (the underlying NeatoTag ScriptableObject was destroyed without unregistering),
    // accessing .name throws MissingReferenceException — same bug class as #18 in a
    // sibling helper.
    //
    // We engineer the dead-key state with public API (Register + DestroyImmediate),
    // then verify the lookup returns null instead of throwing. SetUp/TearDown sweep
    // dead keys via reflection so this test is isolated from earlier-test pollution
    // and doesn't pollute later tests.
    [TestFixture]
    public class TaggerRegistryGetRegisteredTagTests {

        [SetUp]
        public void SetUp() {
            SweepDestroyedTagKeys();
        }

        [TearDown]
        public void TearDown() {
            SweepDestroyedTagKeys();
        }

        [Test]
        public void GetRegisteredTag_WithDestroyedTagKeyInRegistry_ReturnsNullWithoutThrowing() {
            //Engineer the bug: register a tag, then destroy it without unregistering.
            //s_taggedObjects.Keys now contains a fake-null entry.
            var tag = ScriptableObject.CreateInstance<NeatoTag>();
            tag.name = "DestroyableTag";
            TaggerRegistry.RegisterTag( tag );
            Object.DestroyImmediate( tag );

            NeatoTag result = null;
            Assert.DoesNotThrow( () => {
                    result = TaggerRegistry.GetRegisteredTag( "DestroyableTag" );
                },
                "GetRegisteredTag must not throw when iterating past a destroyed NeatoTag key." );
            Assert.That( result, Is.Null,
                "Lookup for a destroyed tag's old name should return null." );
        }

        [Test]
        public void GetRegisteredTag_WithUnrelatedNameAndDestroyedKeyPresent_ReturnsNullWithoutThrowing() {
            //Same setup, different lookup string. Confirms the fix protects every
            //predicate evaluation, not just the specific name we registered.
            var tag = ScriptableObject.CreateInstance<NeatoTag>();
            tag.name = "DestroyableTag2";
            TaggerRegistry.RegisterTag( tag );
            Object.DestroyImmediate( tag );

            NeatoTag result = null;
            Assert.DoesNotThrow( () => {
                    result = TaggerRegistry.GetRegisteredTag( "NeverRegisteredName" );
                },
                "Lookup for an unrelated name with a destroyed key in the registry must not throw." );
            Assert.That( result, Is.Null );
        }

        static void SweepDestroyedTagKeys() {
            //Reach into s_taggedObjects directly — it's a private static field. The
            //!key check uses Unity's overloaded operator bool, which returns false for
            //both real C# null and Unity-fake-null (destroyed) references.
            var field = typeof(TaggerRegistry).GetField( "s_taggedObjects",
                BindingFlags.NonPublic | BindingFlags.Static );
            var dict = (Dictionary<NeatoTag, HashSet<GameObject>>)field!.GetValue( null );
            var deadKeys = dict.Keys.Where( k => !k ).ToList();
            foreach ( var key in deadKeys ) dict.Remove( key );
        }
    }
}
