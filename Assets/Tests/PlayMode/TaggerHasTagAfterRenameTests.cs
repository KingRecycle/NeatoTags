using CharlieMadeAThing.NeatoTags.Core;
using NUnit.Framework;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Tests.PlayMode {
    // Regression test for GitHub issue #16.
    // Tagger.HasTag(string) consulted a per-instance HashSet<string> cache that was
    // only rebuilt when _isCacheDirty flipped, and _isCacheDirty was only flipped on
    // membership changes (Add/Remove). Renaming the underlying NeatoTag never touched
    // any Tagger's dirty flag, so HasTag(newName) returned false and HasTag(oldName)
    // returned true after a rename — silent false negatives and false positives.
    //
    // We exercise the cache directly by mutating NeatoTag.name on an in-memory
    // ScriptableObject. The asset-rename path (AssetDatabase.RenameAsset, used by
    // NeatoTagManager.DoRename) ends up at the same field, so this test covers the
    // production failure mode without needing AssetDatabase mutation.
    [TestFixture]
    public class TaggerHasTagAfterRenameTests {

        GameObject _go;
        NeatoTag _tag;

        [TearDown]
        public void TearDown() {
            if ( _go ) Object.DestroyImmediate( _go );
            if ( _tag ) Object.DestroyImmediate( _tag );
        }

        [Test]
        public void HasTag_ByString_AfterTagRenamed_ReflectsNewName() {
            _go = new GameObject( "RenameCacheHost" );
            var tagger = _go.AddComponent<Tagger>();
            _tag = ScriptableObject.CreateInstance<NeatoTag>();
            _tag.name = "Enemy";
            tagger.AddTag( _tag );

            //Prime the cache pre-fix so the bug surfaces. (Post-fix: harmless sanity check.)
            Assert.That( tagger.HasTag( "Enemy" ), Is.True,
                "Pre-condition: HasTag should find the tag by its original name." );

            _tag.name = "Hostile";

            Assert.That( tagger.HasTag( "Hostile" ), Is.True,
                "After rename, HasTag(newName) must return true." );
            Assert.That( tagger.HasTag( "Enemy" ), Is.False,
                "After rename, HasTag(oldName) must return false." );
        }
    }
}
