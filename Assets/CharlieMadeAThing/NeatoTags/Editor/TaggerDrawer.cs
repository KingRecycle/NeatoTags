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
        ToolbarSearchField _searchField;
        ToolbarSearchField _taggerSearchAvailable;
        Label _searchLabel;
        Button _addTagButton;
        TextField _addTagTextField;
        GroupBox _allTagsBox;
        ToolbarButton _editTaggerButton;
        bool _isEditTaggerMode = false;
        static VisualTreeAsset _tagButtonTemplate;
        static VisualTreeAsset _tagButtonWithXTemplate;

        void OnEnable() {
            _root = new VisualElement();
            // Load in UXML template and USS styles, then apply them to the root element.
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/CharlieMadeAThing/NeatoTags/Editor/Tagger.uxml" );
            visualTree.CloneTree( _root );
            
            _tagButtonTemplate =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( "Assets/CharlieMadeAThing/NeatoTags/Editor/buttonTag.uxml" );
            
            _tagButtonWithXTemplate =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( "Assets/CharlieMadeAThing/NeatoTags/Editor/buttonTagWithX.uxml" );
            

            _tagViewerSelected = _root.Q<GroupBox>( "tagViewer" );
            _tagViewerDeselected = _root.Q<GroupBox>( "allTagViewer" );

            _searchField = _root.Q<ToolbarSearchField>( "taggerSearch" );
            _searchField.RegisterValueChangedCallback( evt => {
                PopulateButtonsAvailableWithSearch( evt.newValue );
                
            } );
            _searchLabel = _root.Q<Label>( "searchLabel" );
            
            _taggerSearchAvailable = _root.Q<ToolbarSearchField>( "taggerSearchAvailable" );
            _taggerSearchAvailable.RegisterValueChangedCallback( evt => {
                PopulateButtonsWithSearch( evt.newValue );
            } );
            
            _addTagButton = _root.Q<Button>( "addTagButton" );
            _addTagButton.clicked += CreateNewTag;
            _addTagTextField = _root.Q<TextField>( "addTagTextField" );
            _addTagTextField.RegisterCallback<KeyDownEvent>( evt => {
                if( evt.keyCode == KeyCode.Return )
                    CreateNewTag();
            });
            
            _allTagsBox = _root.Q<GroupBox>( "allTagsBox" );

            _editTaggerButton = _root.Q<ToolbarButton>( "editTaggerButton" );
            
            _searchField.style.visibility = Visibility.Hidden;
            _addTagButton.style.visibility = Visibility.Hidden;
            _addTagTextField.style.visibility = Visibility.Hidden;
            _searchLabel.style.visibility = Visibility.Hidden;
            _allTagsBox.style.display = DisplayStyle.None;
            _editTaggerButton.clicked += () => {
                _isEditTaggerMode = !_isEditTaggerMode;
                if( _isEditTaggerMode ) {
                    _searchField.style.visibility = Visibility.Visible;
                    _addTagButton.style.visibility = Visibility.Visible;
                    _addTagTextField.style.visibility = Visibility.Visible;
                    _searchLabel.style.visibility = Visibility.Visible;
                    _allTagsBox.style.display = DisplayStyle.Flex;
                    PopulateButtons();
                } else {
                    _searchField.style.visibility = Visibility.Hidden;
                    _addTagButton.style.visibility = Visibility.Hidden;
                    _addTagTextField.style.visibility = Visibility.Hidden;
                    _searchLabel.style.visibility = Visibility.Hidden;
                    _allTagsBox.style.display = DisplayStyle.None;
                    PopulateButtons();
                }
            };
            
            
            
            NeatoTagAssetModificationProcessor.RegisterTaggerDrawer( this );
            NeatoTagDrawer.RegisterTaggerDrawer( this );
            PopulateButtons();
        }

        void CreateNewTag() {
            var tag = TagAssetCreation.CreateNewTag( _addTagTextField.value, false );
            ( (Tagger) target ).AddTag( tag );
            PopulateButtons();
        }


        public override VisualElement CreateInspectorGUI() {
            return _root;   
        }



        Button CreateDeselectedButton( NeatoTagAsset tag ) {
            var button = _tagButtonTemplate.Instantiate().Q<Button>();
            if( tag.Comment != string.Empty ) {
                button.tooltip = tag.Comment;
            }
            button.text = tag.name;
            Color.RGBToHSV( tag.Color, out var h, out var s, out var v );
            button.style.backgroundColor = tag.Color;
                //Color.HSVToRGB( h, s * 0.40f, v * 0.40f );
            button.clicked += () => {
                Undo.RecordObject( target as Tagger, $"Added Tag: {tag.name}" );
                ( (Tagger) target ).AddTag( tag );
                PopulateButtons();
            };

            return button;
        }

        VisualElement CreateSelectedButton( NeatoTagAsset tag ) {
            VisualElement tagButton;
            if ( _isEditTaggerMode ) {
                tagButton = _tagButtonWithXTemplate.Instantiate().Q<VisualElement>();
            } else {
                tagButton = _tagButtonTemplate.Instantiate().Q<Button>();
            }

            if( tag.Comment != string.Empty ) {
                tagButton.tooltip = tag.Comment;
            }

            if ( !_isEditTaggerMode ) {
                if ( tagButton is Button button ) {
                    button.text = tag.name;
                    Color.RGBToHSV( tag.Color, out var h, out var s, out var v );
                    button.style.backgroundColor = tag.Color;
                    StyleButton( button, tag );
                    button.style.backgroundColor = tag.Color;
                    button.style.color = GetColorLuminosity( tag.Color ) > 70 ? Color.black : Color.white;
                    button.clicked += () => {
                        NeatoTagManager.ShowWindow( tag );
                    };
                }
            } else {
                var button = tagButton.Q<Button>( "tagButton" );
                var removeButton = tagButton.Q<Button>( "removeTagButton" );
                button.text = tag.name;
                Color.RGBToHSV( tag.Color, out var h, out var s, out var v );
                button.style.backgroundColor = tag.Color;
                button.style.color = GetColorLuminosity( tag.Color ) > 70 ? Color.black : Color.white;
                
                //removeButton.style.backgroundColor = tag.Color;
                removeButton.style.color = GetColorLuminosity( tag.Color ) > 70 ? Color.black : Color.white;
                removeButton.clicked += () => {
                    if ( _isEditTaggerMode ) {
                        Undo.RecordObject( target as Tagger, $"Removed Tag: {tag.name}" );
                        ( (Tagger) target ).RemoveTag( tag );
                        PopulateButtons();
                    }
                };
            }

            return tagButton;
        }

        void StyleButton( Button button, NeatoTagAsset tag ) {
            Color.RGBToHSV( tag.Color, out var h, out var s, out var v );
            button.style.backgroundColor = tag.Color;
                    
            button.style.unityBackgroundImageTintColor = tag.Color;
            button.style.color = GetColorLuminosity( tag.Color ) > 70 ? Color.black : Color.white;
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

        void PopulateButtonsAvailableWithSearch( string evtNewValue ) {
            if( evtNewValue == string.Empty ) {
                PopulateButtons();
                return;
            }
            _tagViewerDeselected.Clear();
            _tagViewerSelected.Clear();
            var allTags = Tagger.GetAllTags();
            foreach ( var neatoTagAsset in allTags.Where( x => !( (Tagger) target ).GetTags.Contains( x ) ) ) {
                _tagViewerDeselected.Add( CreateDeselectedButton( neatoTagAsset ) );
            }

            foreach ( var neatoTagAsset in allTags.Where( x => ( (Tagger) target ).GetTags.Contains( x ) ) ) {
                if ( Regex.IsMatch( neatoTagAsset.name, evtNewValue, RegexOptions.IgnoreCase ) ) {
                    _tagViewerSelected.Add( CreateSelectedButton( neatoTagAsset ) );
                }
            }
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