using System.Linq;
using System.Text.RegularExpressions;
using CharlieMadeAThing.NeatoTags.Core;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CharlieMadeAThing.NeatoTags.Editor {
    [CustomEditor( typeof( Tagger ) )]
    public class TaggerDrawer : UnityEditor.Editor {
        //UI
        VisualElement _root;
        GroupBox _tagViewerDeselected;
        GroupBox _tagViewerSelected;
        Foldout _foldout;
        ToolbarSearchField _searchField;
        Button _addTagButton;
        TextField _addTagTextField;
        static bool _isFoldoutOpen = true;
        static VisualTreeAsset _tagButtonTemplate;

        void OnEnable() {
            _root = new VisualElement();
            // Load in UXML template and USS styles, then apply them to the root element.
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/CharlieMadeAThing/NeatoTags/Editor/Tagger.uxml" );
            visualTree.CloneTree( _root );

            _tagViewerSelected = _root.Q<GroupBox>( "tagViewer" );
            _tagViewerDeselected = _root.Q<GroupBox>( "allTagViewer" );
            _foldout = _root.Q<Foldout>( "foldout" );
            _foldout.value = _isFoldoutOpen;
            _foldout.RegisterValueChangedCallback( evt => {
                _isFoldoutOpen = evt.newValue;
            } );
            _searchField = _root.Q<ToolbarSearchField>( "taggerSearch" );
            _searchField.RegisterValueChangedCallback( evt => {
                PopulateButtonsWithSearch( evt.newValue );
            } );
            _addTagButton = _root.Q<Button>( "addTagButton" );
            _addTagButton.clicked += CreateNewTag;
            _addTagTextField = _root.Q<TextField>( "addTagTextField" );
            _addTagTextField.RegisterCallback<KeyDownEvent>( evt => {
                CreateNewTag();
            });
            _tagButtonTemplate =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( "Assets/CharlieMadeAThing/NeatoTags/Editor/buttonTag.uxml" );
            NeatoTagAssetModificationProcessor.RegisterTaggerDrawer( this );
            NeatoTagDrawer.RegisterTaggerDrawer( this );
            PopulateButtons();
        }

        void CreateNewTag() {
            TagAssetCreation.CreateNewTag( _addTagTextField.value);
        }


        public override VisualElement CreateInspectorGUI() {
            return _root;   
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
        }


        Button CreateDeselectedButton( NeatoTagAsset tag ) {
            var button = _tagButtonTemplate.Instantiate().Q<Button>();
            if( tag.Comment != string.Empty ) {
                button.tooltip = tag.Comment;
            }
            button.text = tag.name;
            Color.RGBToHSV( tag.Color, out var h, out var s, out var v );
            button.style.unityBackgroundImageTintColor = Color.HSVToRGB( h, s * 0.40f, v * 0.40f );
            button.clicked += () => {
                Undo.RecordObject( target as Tagger, $"Added Tag: {tag.name}" );
                ( (Tagger) target ).AddTag( tag );
                PopulateButtons();
            };

            return button;
        }

        Button CreateSelectedButton( NeatoTagAsset tag ) {
            var button = _tagButtonTemplate.Instantiate().Q<Button>();
            if( tag.Comment != string.Empty ) {
                button.tooltip = tag.Comment;
            }
            button.text = tag.name;
            Color.RGBToHSV( tag.Color, out var h, out var s, out var v );
            button.style.unityBackgroundImageTintColor = tag.Color;
            button.style.color = GetColorLuminosity( tag.Color ) > 70 ? Color.black : Color.white;
            button.clicked += () => {
                Undo.RecordObject( target as Tagger, $"Removed Tag: {tag.name}" );
                ( (Tagger) target ).RemoveTag( tag );
                PopulateButtons();
            };

            return button;
        }

        public void PopulateButtons() {
            _tagViewerDeselected.Clear();
            _tagViewerSelected.Clear();
            var allTags = Tagger.GetAllTags();
            foreach ( var neatoTagAsset in allTags.Where( x => ( (Tagger) target ).GetTags.Contains( x ) ) ) {
                _tagViewerSelected.Add( CreateSelectedButton( neatoTagAsset ) );
            }

            foreach ( var neatoTagAsset in allTags.Where( x => !( (Tagger) target ).GetTags.Contains( x ) ) ) {
                _tagViewerDeselected.Add( CreateDeselectedButton( neatoTagAsset ) );
            }
            _searchField.value = string.Empty;
        }
        
        void PopulateButtonsWithSearch( string evtNewValue ) {
            if( evtNewValue == string.Empty ) {
                PopulateButtons();
                return;
            }
            _tagViewerDeselected.Clear();
            _tagViewerSelected.Clear();
            var allTags = Tagger.GetAllTags();
            foreach ( var neatoTagAsset in allTags.Where( x => ( (Tagger) target ).GetTags.Contains( x ) ) ) {
                _tagViewerSelected.Add( CreateSelectedButton( neatoTagAsset ) );
            }

            foreach ( var neatoTagAsset in allTags.Where( x => !( (Tagger) target ).GetTags.Contains( x ) ) ) {
                if ( Regex.IsMatch( neatoTagAsset.name, evtNewValue, RegexOptions.IgnoreCase ) ) {
                    _tagViewerDeselected.Add( CreateDeselectedButton( neatoTagAsset ) );
                }
            }
        }

        public static float GetColorLuminosity( Color color ) {
            return ( 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b ) * 100f;
        }
    }
}