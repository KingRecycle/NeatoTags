using CharlieMadeAThing.NeatoTags.Core;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CharlieMadeAThing.NeatoTags.Tests.PlayMode {
    // Regression tests for GitHub issue #30.
    // Tagger.GetOrCreate(name) used to always call ScriptableObject.CreateInstance
    // when the tag wasn't already on THIS tagger, even when a NeatoTag with the
    // same name was registered project-wide (asset or prior runtime tag). That
    // produced a split-brain identity: HasTag(name) matched the runtime tag but
    // HasTag(assetReference) did not. The fix adds a TaggerRegistry lookup
    // between the local check and CreateInstance, so an already-registered tag
    // is reused instead of duplicated.
    [TestFixture]
    public class TaggerGetOrCreateRegistryReuseTests {
        const string TagName = "Issue30RegressionTag";
        NeatoTag _registeredTag;
        GameObject _target;

        [SetUp]
        public void SetUp() {
            _registeredTag = ScriptableObject.CreateInstance<NeatoTag>();
            _registeredTag.name = TagName;
            TaggerRegistry.RegisterTag( _registeredTag );

            _target = new GameObject( "Issue30Target" );
            _target.AddComponent<Tagger>();
        }

        [TearDown]
        public void TearDown() {
            if ( _target ) Object.Destroy( _target );
            if ( _registeredTag ) {
                TaggerRegistry.UnregisterTag( _registeredTag );
                Object.Destroy( _registeredTag );
            }
        }

        [Test]
        public void GetOrCreate_NameMatchesRegisteredTag_ReturnsRegisteredInstance() {
            var tagger = _target.GetComponent<Tagger>();

            var result = tagger.GetOrCreate( TagName );

            Assert.That( result, Is.SameAs( _registeredTag ),
                "GetOrCreate should reuse the registered NeatoTag with the same name, not allocate a new ScriptableObject." );
        }

        [Test]
        public void GetOrCreate_NameMatchesRegisteredTag_HasTagByReferenceReturnsTrue() {
            var tagger = _target.GetComponent<Tagger>();

            tagger.GetOrCreate( TagName );

            Assert.That( tagger.HasTag( _registeredTag ), Is.True,
                "After GetOrCreate, the Tagger must hold the registered tag (reference-equality check) — not a same-named runtime duplicate." );
        }

        [Test]
        public void GetOrCreate_NameMatchesRegisteredTag_TaggerHoldsExactlyOneTag() {
            var tagger = _target.GetComponent<Tagger>();

            tagger.GetOrCreate( TagName );

            Assert.That( tagger.GetTags.Count, Is.EqualTo( 1 ),
                "GetOrCreate should add the registered tag once; no phantom duplicates in _tags." );
        }
    }
}
