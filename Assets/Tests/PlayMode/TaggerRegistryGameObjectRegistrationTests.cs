using CharlieMadeAThing.NeatoTags.Core;
using NUnit.Framework;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Tests.PlayMode {
    // Regression tests for GitHub issues #10 and #41.
    // Both bugs are the same indexer-without-TryGetValue pattern in adjacent
    // TaggerRegistry helpers:
    //   #10  RegisterGameObjectToTag       (TaggerRegistry.cs:194)
    //   #41  UnregisterGameObjectFromTag   (TaggerRegistry.cs:209)
    [TestFixture]
    public class TaggerRegistryGameObjectRegistrationTests {

        GameObject _go;
        NeatoTag _tag;

        [TearDown]
        public void TearDown() {
            if ( _go ) Object.DestroyImmediate( _go );
            if ( _tag ) Object.DestroyImmediate( _tag );
        }

        [Test]
        public void RegisterGameObjectToTag_TagNotInRegistry_DoesNotThrow() {
            //Issue #10. RegisterGameObjectToTag is public, but its docstring claims you don't
            //need to call RegisterTag first. Pre-fix, the indexer s_taggedObjects[tag] throws
            //KeyNotFoundException whenever an external caller skips that prerequisite.
            _go = new GameObject( "RegisterTestHost" );
            _tag = ScriptableObject.CreateInstance<NeatoTag>();
            _tag.name = "UnregisteredTag";

            Assert.DoesNotThrow( () =>
                TaggerRegistry.RegisterGameObjectToTag( _go, _tag ) );

            //Verify the registration actually took effect.
            var registry = TaggerRegistry.GetStaticTaggedObjectsDictionary();
            Assert.That(
                registry.TryGetValue( _tag, out var set ) && set.Contains( _go ),
                Is.True,
                "The GameObject should now be registered to the tag's set." );
        }

        [Test]
        public void OnDestroy_AfterResetRegistry_DoesNotThrow() {
            //Issue #41. Tagger.OnDestroy → RemoveTaggerFromRegistry → UnregisterGameObjectFromTag.
            //If something cleared s_taggedObjects (ResetRegistry, UnregisterTag) while a Tagger
            //still holds the tag in its _tags list, the indexer s_taggedObjects[tag] throws
            //KeyNotFoundException as the GameObject is being torn down.
            _go = new GameObject( "UnregisterTestHost" );
            var tagger = _go.AddComponent<Tagger>();
            _tag = ScriptableObject.CreateInstance<NeatoTag>();
            _tag.name = "TagToBeOrphaned";
            tagger.AddTag( _tag );

            //Wipe the registry while the Tagger still holds _tag in its _tags list.
            TaggerRegistry.ResetRegistry();

            Assert.DoesNotThrow( () => Object.DestroyImmediate( _go ) );
            _go = null;   //Already destroyed; prevent double-destroy in TearDown.
        }
    }
}
