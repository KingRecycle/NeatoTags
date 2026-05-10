using CharlieMadeAThing.NeatoTags.Core;
using NUnit.Framework;

namespace CharlieMadeAThing.NeatoTags.Tests.PlayMode {
    // Regression tests for GitHub issue #46.
    // Tagger.TagNameRegex previously only blocked '<', '>', and whitespace-only
    // strings. NeatoTag instances are ScriptableObject assets on disk and
    // NeatoTag.name is the asset's filename basename, so any name reaching
    // AssetDatabase.RenameAsset must be a valid filename across platforms.
    // The fix tightens the regex to also reject path separators (/ \), Windows-
    // reserved characters (: " | ? *), and ASCII control characters (\x00-\x1F).
    [TestFixture]
    public class TagNameRegexValidationTests {
        [TestCase( "Enemy" )]
        [TestCase( "Boss_v2" )]
        [TestCase( "Item.Weapon.Sword" )]
        [TestCase( "name with spaces" )]
        [TestCase( "Name-With-Dashes" )]
        [TestCase( "123Numbers" )]
        [TestCase( "UTF8 éñ漢字" )]
        [TestCase( "specialOK!@#$%^&()=+~" )]
        public void IsMatch_ValidName_ReturnsTrue( string name ) {
            Assert.That( Tagger.TagNameRegex.IsMatch( name ), Is.True,
                $"Expected '{name}' to be a valid tag name." );
        }

        [TestCase( "", TestName = "Empty string" )]
        [TestCase( " ", TestName = "Single space (whitespace-only)" )]
        [TestCase( "   ", TestName = "Multiple spaces (whitespace-only)" )]
        [TestCase( "<Bracketed>", TestName = "Contains < and >" )]
        [TestCase( "Has<bracket", TestName = "Contains <" )]
        [TestCase( "Has>bracket", TestName = "Contains >" )]
        public void IsMatch_PreviouslyRejected_StillReturnsFalse( string name ) {
            Assert.That( Tagger.TagNameRegex.IsMatch( name ), Is.False,
                $"Expected '{name}' to remain rejected (regression check for pre-#46 behavior)." );
        }

        // Names that matched the old regex but would corrupt asset paths or
        // fail AssetDatabase.RenameAsset cross-platform. These are the actual
        // #46 regression coverage — each must now be rejected.
        [TestCase( "Enemies/Boss", TestName = "Forward slash (path separator on every modern FS)" )]
        [TestCase( "Enemies\\Boss", TestName = "Backslash (Windows path separator)" )]
        [TestCase( "Tag:Group", TestName = "Colon (Windows reserved)" )]
        [TestCase( "Quoted\"Name", TestName = "Double quote (Windows reserved)" )]
        [TestCase( "Pipe|Sep", TestName = "Pipe (Windows reserved)" )]
        [TestCase( "Wild?card", TestName = "Question mark (Windows reserved)" )]
        [TestCase( "Star*", TestName = "Asterisk (Windows reserved)" )]
        [TestCase( "Null\0Char", TestName = "Null byte (control 0x00)" )]
        [TestCase( "Bell\x07Char", TestName = "Bell (control 0x07)" )]
        [TestCase( "Tab\tChar", TestName = "Embedded tab (control 0x09)" )]
        [TestCase( "LF\nChar", TestName = "Line feed (control 0x0A)" )]
        [TestCase( "CR\rChar", TestName = "Carriage return (control 0x0D)" )]
        public void IsMatch_FilesystemHostile_ReturnsFalse( string name ) {
            Assert.That( Tagger.TagNameRegex.IsMatch( name ), Is.False,
                $"Expected '{name}' to be rejected as filesystem-hostile (issue #46)." );
        }
    }
}
