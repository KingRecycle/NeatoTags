using CharlieMadeAThing.NeatoTags.Core;
using NUnit.Framework;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Tests.PlayMode {
    // Regression tests for GitHub issue #11.
    // Tagger.RemoveTag(NeatoTag) had three compounding bugs:
    //   1. _tags.Remove return value ignored (no early-exit when tag wasn't present).
    //   2. taggedGameObject.Remove(gameObject) called unconditionally — could mutate the
    //      registry set for a tag this Tagger never owned.
    //   3. RegisterNonTaggedGameObject only ran inside the TryGetValue block, so a Tagger
    //      whose registry entry had been wiped never re-entered s_nonTaggedObjects when
    //      its last tag was removed.
    [TestFixture]
    public class TaggerRemoveTagTests {

        GameObject _go;
        NeatoTag _tag;

        [TearDown]
        public void TearDown() {
            //Destroy our own GameObjects/tags rather than calling ResetRegistry — clearing
            //static state in TearDown surfaces unrelated registry bugs (see issue #41 history).
            if ( _go ) Object.DestroyImmediate( _go );
            if ( _tag ) Object.DestroyImmediate( _tag );
        }

        [Test]
        public void RemoveTag_LastTagAfterRegistryEntryWiped_AddsToNonTaggedSet() {
            //Bug 3. Tagger holds _tag in its _tags list, but the registry entry for _tag
            //has been wiped (e.g., a prior UnregisterTag/ResetRegistry, or any other path).
            //Removing the Tagger's last tag must still re-register the GameObject as
            //non-tagged. Pre-fix the early-return inside the TryGetValue block skipped this.
            _go = new GameObject( "RemoveTagOrphanedHost" );
            var tagger = _go.AddComponent<Tagger>();
            _tag = ScriptableObject.CreateInstance<NeatoTag>();
            _tag.name = "OrphanableTag";
            tagger.AddTag( _tag );

            //Wipe the registry entry while the Tagger still holds _tag in _tags.
            TaggerRegistry.GetStaticTaggedObjectsDictionary().Remove( _tag );

            Assert.That( TaggerRegistry.GetStaticNonTaggedGameObjects().Contains( _go ), Is.False,
                "Pre-condition: _go was tagged so it should not be in the non-tagged set yet." );

            tagger.RemoveTag( _tag );

            Assert.That( TaggerRegistry.GetStaticNonTaggedGameObjects().Contains( _go ), Is.True,
                "After removing the last tag, _go must land in s_nonTaggedObjects even if the registry entry was already gone." );
        }

        [Test]
        public void RemoveTag_TagNotInTaggerList_DoesNotMutateRegistrySet() {
            //Bugs 1 & 2. If a Tagger calls RemoveTag for a tag it doesn't own, the call
            //must be a strict no-op for the registry. Pre-fix the indexer-style mutation
            //ran regardless of the _tags.Remove result, so the GameObject would be silently
            //pulled out of that tag's registry set.
            _go = new GameObject( "RemoveTagInconsistencyHost" );
            var tagger = _go.AddComponent<Tagger>();
            _tag = ScriptableObject.CreateInstance<NeatoTag>();
            _tag.name = "ForeignTag";

            //Place _go in _tag's registry set without adding _tag to the Tagger's _tags list.
            //This is the inconsistent state bug 2 corrupts under: a registry entry the
            //Tagger doesn't claim ownership of in its own list.
            TaggerRegistry.RegisterTag( _tag );
            TaggerRegistry.RegisterGameObjectToTag( _go, _tag );

            Assert.That( tagger.HasTag( _tag ), Is.False,
                "Pre-condition: Tagger does not list _tag." );
            Assert.That(
                TaggerRegistry.GetStaticTaggedObjectsDictionary()[_tag].Contains( _go ), Is.True,
                "Pre-condition: _go is registered to _tag's set." );

            tagger.RemoveTag( _tag );

            Assert.That(
                TaggerRegistry.GetStaticTaggedObjectsDictionary()[_tag].Contains( _go ), Is.True,
                "RemoveTag for a tag the Tagger does not own must not silently remove the GameObject from that tag's registry set." );
        }
    }
}
