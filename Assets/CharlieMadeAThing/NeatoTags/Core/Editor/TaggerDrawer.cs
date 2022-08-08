using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CharlieMadeAThing.NeatoTags.Core.Editor {
    [CustomEditor( typeof( Tagger ) )]
    public class TaggerDrawer : UnityEditor.Editor {
        static VisualTreeAsset _tagButtonTemplate;
        static VisualTreeAsset _tagButtonWithXTemplate;
        Button _addTagButton;
        TextField _addTagTextField;
        GroupBox _allTagsBox;
        ToolbarButton _editTaggerButton;

        bool _isEditTaggerMode;

        //UI
        VisualElement _root;
        ToolbarSearchField _searchField;
        Label _searchLabel;
        ToolbarSearchField _taggerSearchAvailable;
        GroupBox _tagViewerDeselected;
        GroupBox _tagViewerSelected;

        void OnEnable() {
            TagAssetCreation.GetUxmlDirectory();
            _root = new VisualElement();

            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( UxmlDataLookup.TaggerUxml );
            visualTree.CloneTree( _root );

            _tagButtonTemplate =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( UxmlDataLookup.ButtonTagUxml );

            _tagButtonWithXTemplate =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( UxmlDataLookup.ButtonTagWithXUxml );


            _tagViewerSelected = _root.Q<GroupBox>( "tagViewer" );
            _tagViewerDeselected = _root.Q<GroupBox>( "allTagViewer" );

            _searchField = _root.Q<ToolbarSearchField>( "taggerSearch" );
            _searchField.RegisterValueChangedCallback( _ => { PopulateButtonsWithSearch(); } );
            _searchField.tooltip = $"Search through tags currently tagged to {target.name}";
            _searchLabel = _root.Q<Label>( "searchLabel" );

            _taggerSearchAvailable = _root.Q<ToolbarSearchField>( "taggerSearchAvailable" );
            _taggerSearchAvailable.RegisterValueChangedCallback( _ => { PopulateButtonsWithSearch(); } );
            _taggerSearchAvailable.tooltip = $"Search through tags currently available to add to {target.name}";

            _addTagButton = _root.Q<Button>( "addTagButton" );
            _addTagButton.tooltip = $"Create a new tag and add it to {target.name}";
            _addTagButton.clicked += CreateNewTag;
            _addTagTextField = _root.Q<TextField>( "addTagTextField" );
            _addTagTextField.RegisterCallback<KeyDownEvent>( evt => {
                if ( evt.keyCode == KeyCode.Return )
                    CreateNewTag();
            } );

            _allTagsBox = _root.Q<GroupBox>( "allTagsBox" );

            _editTaggerButton = _root.Q<ToolbarButton>( "editTaggerButton" );
            _editTaggerButton.tooltip = $"Edit Tags for {target.name}";

            _searchField.style.display = DisplayStyle.None;
            _addTagButton.style.display = DisplayStyle.None;
            _addTagTextField.style.display = DisplayStyle.None;
            _searchLabel.style.display = DisplayStyle.None;
            _allTagsBox.style.display = DisplayStyle.None;
            _editTaggerButton.clicked += () => {
                _isEditTaggerMode = !_isEditTaggerMode;
                if ( _isEditTaggerMode ) {
                    _searchField.style.display = DisplayStyle.Flex;
                    _addTagButton.style.display = DisplayStyle.Flex;
                    _addTagTextField.style.display = DisplayStyle.Flex;
                    _searchLabel.style.display = DisplayStyle.Flex;
                    _allTagsBox.style.display = DisplayStyle.Flex;
                    PopulateButtons();
                } else {
                    _searchField.style.display = DisplayStyle.None;
                    _addTagButton.style.display = DisplayStyle.None;
                    _addTagTextField.style.display = DisplayStyle.None;
                    _searchLabel.style.display = DisplayStyle.None;
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


        Button CreateDeselectedButton( NeatoTag tag ) {
            var button = _tagButtonTemplate.Instantiate().Q<Button>();
            if ( tag.Comment != string.Empty ) {
                button.tooltip = tag.Comment;
            }

            StyleButton( button, tag );
            button.clicked += () => {
                Undo.RecordObject( target as Tagger, $"Added Tag: {tag.name}" );
                ( (Tagger) target ).AddTag( tag );
                if ( _taggerSearchAvailable.value != string.Empty ) {
                    PopulateButtonsWithSearch();
                } else {
                    PopulateButtons();
                }
            };

            return button;
        }

        VisualElement CreateSelectedButton( NeatoTag tag ) {
            var tagButton = _isEditTaggerMode
                ? _tagButtonWithXTemplate.Instantiate().Q<VisualElement>()
                : _tagButtonTemplate.Instantiate().Q<Button>();

            if ( tag.Comment != string.Empty ) {
                tagButton.tooltip = tag.Comment;
            }

            if ( !_isEditTaggerMode ) {
                if ( tagButton is Button button ) {
                    StyleButton( button, tag );
                    button.clicked += () => { NeatoTagManager.ShowWindow( tag ); };
                }
            } else {
                var button = tagButton.Q<Button>( "tagButton" );
                var removeButton = tagButton.Q<Button>( "removeTagButton" );
                removeButton.tooltip = $"Remove {tag.name} tag from {target.name}";
                StyleButton( button, tag );

                removeButton.style.color = GetColorLuminosity( tag.Color ) > 70 ? Color.black : Color.white;
                removeButton.clicked += () => {
                    if ( !_isEditTaggerMode ) return;
                    Undo.RecordObject( target as Tagger, $"Removed Tag: {tag.name}" );
                    ( (Tagger) target ).RemoveTag( tag );
                    if ( _searchField.value != string.Empty ) {
                        PopulateButtonsWithSearch();
                    } else {
                        PopulateButtons();
                    }
                };
            }

            return tagButton;
        }

        static void StyleButton( Button button, NeatoTag tag ) {
            button.text = tag.name;
            button.style.backgroundColor = tag.Color;
            button.style.color = GetColorLuminosity( tag.Color ) > 70 ? Color.black : Color.white;
        }

        public void PopulateButtons() {
            _tagViewerDeselected.Clear();
            _tagViewerSelected.Clear();
            var allTags = Tagger.GetAllTags();

            if ( ( (Tagger) target ).GetTags != null ) {
                foreach ( var neatoTagAsset in allTags.Where( x => ( (Tagger) target ).GetTags.Contains( x ) ) ) {
                    _tagViewerSelected.Add( CreateSelectedButton( neatoTagAsset ) );
                }

                foreach ( var neatoTagAsset in allTags.Where( x => !( (Tagger) target ).GetTags.Contains( x ) ) ) {
                    _tagViewerDeselected.Add( CreateDeselectedButton( neatoTagAsset ) );
                }
            } else {
                foreach ( var neatoTagAsset in allTags ) {
                    _tagViewerDeselected.Add( CreateDeselectedButton( neatoTagAsset ) );
                }
            }
        }


        void PopulateButtonsWithSearch() {
            _tagViewerDeselected.Clear();
            _tagViewerSelected.Clear();

            var allTags = Tagger.GetAllTags();
            //Selected Tags
            foreach ( var neatoTagAsset in allTags.Where( x => ( (Tagger) target ).GetTags.Contains( x ) ) ) {
                if ( !string.IsNullOrEmpty( _searchField.value ) && !string.IsNullOrWhiteSpace( _searchField.value ) ) {
                    if ( Regex.IsMatch( neatoTagAsset.name, _searchField.value, RegexOptions.IgnoreCase ) ) {
                        _tagViewerSelected.Add( CreateSelectedButton( neatoTagAsset ) );
                    }
                } else {
                    _tagViewerSelected.Add( CreateSelectedButton( neatoTagAsset ) );
                }
            }

            //Available Tags
            foreach ( var neatoTagAsset in allTags.Where( x => !( (Tagger) target ).GetTags.Contains( x ) ) ) {
                if ( !string.IsNullOrEmpty( _taggerSearchAvailable.value ) &&
                     !string.IsNullOrWhiteSpace( _taggerSearchAvailable.value ) ) {
                    if ( Regex.IsMatch( neatoTagAsset.name, _taggerSearchAvailable.value, RegexOptions.IgnoreCase ) ) {
                        _tagViewerDeselected.Add( CreateDeselectedButton( neatoTagAsset ) );
                    }
                } else {
                    _tagViewerDeselected.Add( CreateDeselectedButton( neatoTagAsset ) );
                }
            }
        }

        public static float GetColorLuminosity( Color color ) {
            return ( 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b ) * 100f;
        }
    }
}