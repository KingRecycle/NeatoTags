using System.Collections.Generic;
using System.Reflection;
using CharlieMadeAThing.NeatoTags.Core;
using NUnit.Framework;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Tests.PlayMode {
    // Regression test for GitHub issue #18.
    // Tagger.RemoveTag(string) used `_tags.FirstOrDefault(t => t.name == tagName)` —
    // accessing .name on a true C# null entry threw NullReferenceException. Awake
    // and OnValidate normally sweep nulls out of _tags, but a path where _tags is
    // mutated post-Awake (test fixtures, post-Awake serialization writes, reflection)
    // can leave a real C# null in the list before RemoveTag(string) runs.
    //
    // We reproduce that state by reflection on the private _tags field — there's no
    // public API that lets a true C# null land in _tags after Awake, which is exactly
    // why the bug is silent in normal flows but bites in setup/test paths.
    [TestFixture]
    public class TaggerRemoveTagByNameNullEntryTests {

        GameObject _go;

        [TearDown]
        public void TearDown() {
            if ( _go ) Object.DestroyImmediate( _go );
        }

        [Test]
        public void RemoveTagByName_WithCSharpNullEntryInTags_DoesNotThrow() {
            _go = new GameObject( "RemoveTagNullEntryHost" );
            var tagger = _go.AddComponent<Tagger>();   //Awake just cleaned _tags, so it's empty.

            //Inject a true C# null directly into _tags, post-Awake, post-OnValidate.
            var field = typeof(Tagger).GetField( "_tags",
                BindingFlags.NonPublic | BindingFlags.Instance );
            var list = (List<NeatoTag>)field.GetValue( tagger );
            list.Add( null );

            Assert.DoesNotThrow( () => tagger.RemoveTag( "anyName" ),
                "RemoveTag(string) must not throw when _tags contains a C# null entry." );
        }
    }
}
