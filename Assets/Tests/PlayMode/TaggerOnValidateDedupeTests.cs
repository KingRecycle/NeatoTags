using CharlieMadeAThing.NeatoTags.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Tests.PlayMode {
    // Regression test for GitHub issue #17.
    // Tagger._tags is a [SerializeField] List<NeatoTag> exposed in the inspector.
    // Inspector drag-and-drop bypasses Tagger.AddTag (which guards against
    // duplicates), so the same NeatoTag asset can land in _tags twice. Once that
    // happens, _tags.Count lies, RemoveTag(tag) only removes the first occurrence,
    // and HasTag still matches the phantom entry.
    //
    // We exercise this through Unity's serialization API — SerializedProperty +
    // ApplyModifiedProperties is the same code path the inspector uses, and
    // ApplyModifiedProperties fires OnValidate where the dedupe must happen.
    [TestFixture]
    public class TaggerOnValidateDedupeTests {

        GameObject _go;
        NeatoTag _tag;

        [TearDown]
        public void TearDown() {
            if ( _go ) Object.DestroyImmediate( _go );
            if ( _tag ) Object.DestroyImmediate( _tag );
        }

        [Test]
        public void OnValidate_AfterInspectorDuplicateInjected_RemovesDuplicate() {
            _go = new GameObject( "DedupeHost" );
            var tagger = _go.AddComponent<Tagger>();
            _tag = ScriptableObject.CreateInstance<NeatoTag>();
            _tag.name = "DupeTag";

            //Simulate the inspector path that introduces a duplicate.
            var so = new SerializedObject( tagger );
            var prop = so.FindProperty( "_tags" );
            prop.arraySize = 2;
            prop.GetArrayElementAtIndex( 0 ).objectReferenceValue = _tag;
            prop.GetArrayElementAtIndex( 1 ).objectReferenceValue = _tag;
            so.ApplyModifiedProperties();   //fires OnValidate

            Assert.That( tagger.GetTags.Count, Is.EqualTo( 1 ),
                "OnValidate must dedupe _tags after an inspector edit produced a duplicate." );
            Assert.That( tagger.HasTag( _tag ), Is.True,
                "The single retained entry must still match the original tag." );
        }
    }
}
