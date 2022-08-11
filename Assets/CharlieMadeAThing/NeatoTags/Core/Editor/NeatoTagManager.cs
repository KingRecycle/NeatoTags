using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CharlieMadeAThing.NeatoTags.Core.Editor {
    public class NeatoTagManager : EditorWindow {
        static VisualTreeAsset _tagButtonTemplate;
        static VisualElement _root;

        static readonly Dictionary<Button, Action> BUTTON_ACTION_MAP = new();
        static GroupBox _allTagsBox;
        static ToolbarSearchField _tagSearchField;

        //Bottom Half
        static Button _renameButton;
        static Button _renameButtonDisplay;
        static TextField _renameField;
        static Toolbar _renameToolbar;
        static GroupBox _tagInfoBox;
        static Button _deleteTagButton;
        static Button _setTagFolderButton;
        static Label _tagDirectoryLabel;

        //Selected Data
        static NeatoTag _selectedTag;
        static TextField _selectedTagTextField;
        static ColorField _selectedTagColorField;

        static VisualElement _selectedTagInfoElement;

        //Top Half
        ToolbarButton _addTagButton;


        public void CreateGUI() {
            // Each editor window contains a root VisualElement object
            _root = rootVisualElement;

            // Import UXML
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( UxmlDataLookup.NeatTagManagerUxml );
            visualTree.CloneTree( _root );


            _tagButtonTemplate =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( UxmlDataLookup.ButtonTagUxml );

            _allTagsBox = _root.Q<GroupBox>( "allTagsBox" );
            _tagInfoBox = _root.Q<GroupBox>( "tagInfoBox" );
            _renameToolbar = _root.Q<Toolbar>( "renameToolbar" );


            _renameField = _root.Q<TextField>( "renameField" );
            _renameField.style.visibility = Visibility.Hidden;

            _renameButton = _root.Q<Button>( "renameButton" );
            _renameButton.style.visibility = Visibility.Hidden;

            _renameButtonDisplay = _tagButtonTemplate.Instantiate().Q<Button>();
            _renameButtonDisplay.style.visibility = Visibility.Hidden;

            _addTagButton = _root.Q<ToolbarButton>( "addTagButton" );
            _addTagButton.clicked += () => {
                var newTag = TagAssetCreation.CreateNewTag( "New Tag" );
                PopulateAllTagsBox();
                if ( newTag ) {
                    BUTTON_ACTION_MAP.First( x => x.Key.text == newTag.name ).Value.Invoke();
                }
            };

            _setTagFolderButton = _root.Q<Button>( "setTagFolderButton" );
            _setTagFolderButton.clicked += () => {
                TagAssetCreation.SetTagFolder();
                UpdatePathLabel();
            };

            _tagDirectoryLabel = _root.Q<Label>( "tagDirectoryLabel" );
            UpdatePathLabel();


            _deleteTagButton = _root.Q<Button>( "deleteTagButton" );
            _deleteTagButton.clicked += DeleteSelectedTag;
            _deleteTagButton.style.visibility = Visibility.Hidden;

            _tagSearchField = _root.Q<ToolbarSearchField>( "tagSearchField" );
            _tagSearchField.RegisterValueChangedCallback( evt => { PopulateButtonsWithSearch( evt.newValue ); } );

            PopulateAllTagsBox();
        }

        [MenuItem( "Window/Neato Tag Manager" )]
        public static void ShowWindow() {
            var wnd = GetWindow<NeatoTagManager>();
            wnd.titleContent = new GUIContent( "Neato Tag Manager" );
        }

        public static void ShowWindow( NeatoTag tag ) {
            var wnd = GetWindow<NeatoTagManager>();
            wnd.titleContent = new GUIContent( "Neato Tag Manager" );
            _selectedTag = tag;
            DisplayTag();
        }

        void UpdatePathLabel() {
            var tagFolderLocation = TagAssetCreation.GetTagFolderLocation();
            var tagPath = tagFolderLocation == string.Empty ? "Not Assigned" : tagFolderLocation;
            _tagDirectoryLabel.text = $"Default Tag Folder Location: {tagPath}";
        }
        
        void PopulateButtonsWithSearch( string evtNewValue ) {
            if ( evtNewValue == string.Empty ) {
                PopulateAllTagsBox();
                return;
            }

            _allTagsBox.Clear();
            var allTags = TagAssetCreation.GetAllTags();
            foreach ( var neatoTagAsset in allTags ) {
                if ( Regex.IsMatch( neatoTagAsset.name, evtNewValue, RegexOptions.IgnoreCase ) ) {
                    _allTagsBox.Add( CreateTagButton( neatoTagAsset ) );
                }
            }
        }

        void DeleteSelectedTag() {
            if ( !_selectedTag ) return;
            TagAssetCreation.DeleteTag( _selectedTag );
            _selectedTag = null;
            PopulateAllTagsBox();
            UnDisplayTag();
        }


        static void PopulateAllTagsBox() {
            _allTagsBox.Clear();
            BUTTON_ACTION_MAP.Clear();

            var allTags = TagAssetCreation.GetAllTags();

            foreach ( var neatoTagAsset in allTags ) {
                _allTagsBox.Add( CreateTagButton( neatoTagAsset ) );
            }
        }


        static Button CreateTagButton( NeatoTag tag ) {
            var button = _tagButtonTemplate.Instantiate().Q<Button>();
            if ( tag.Comment != string.Empty ) {
                button.tooltip = tag.Comment;
            }

            button.text = tag.name;
            button.style.backgroundColor = tag.Color;
            button.style.color = TaggerDrawer.GetColorLuminosity( tag.Color ) > 70 ? Color.black : Color.white;

            BUTTON_ACTION_MAP.Add( button, () => {
                _selectedTag = tag;
                DisplayTag();
            } );

            button.clicked += () => BUTTON_ACTION_MAP[button].Invoke();
            return button;
        }

        static bool CanRenameTag() {
            if ( _renameField.value != string.Empty ) {
                foreach ( var element in _allTagsBox.Children() ) {
                    if ( element is Button tagButton && tagButton.text == _renameField.value ) {
                        return false;
                    }
                }
            }

            return true;
        }

        static void DoRename() {
            if ( !CanRenameTag() ) return;
            var color = _selectedTag.Color;
            var comment = _selectedTag.Comment;
            var tagPath = AssetDatabase.GetAssetPath( _selectedTag );
            AssetDatabase.RenameAsset( tagPath, _renameField.value );
            _selectedTag.Color = color;
            _selectedTag.Comment = comment;
            PopulateAllTagsBox();
            BUTTON_ACTION_MAP.First( x => x.Key.text == _selectedTag.name ).Value.Invoke();
            NeatoTagAssetModificationProcessor.UpdateTaggers();
        }

        void UnDisplayTag() {
            _tagInfoBox.Remove( _selectedTagInfoElement );
            _renameField.style.visibility = Visibility.Hidden;
            _renameButton.style.visibility = Visibility.Hidden;
            _renameButtonDisplay.style.visibility = Visibility.Hidden;
            _deleteTagButton.style.visibility = Visibility.Hidden;
        }

        static void DisplayTag() {
            var visualTree = AssetDatabase
                .LoadAssetAtPath<VisualTreeAsset>( UxmlDataLookup.NeatoTagUxml )
                .Instantiate();
            var tagVisualElement = visualTree.Q<VisualElement>( "topVisualElement" );

            _selectedTagInfoElement = tagVisualElement;

            var oldVisualElement = _tagInfoBox.Q<VisualElement>( "topVisualElement" );
            if ( oldVisualElement != null ) {
                _tagInfoBox.Remove( oldVisualElement );
            }

            _tagInfoBox.Add( tagVisualElement );

            _tagSearchField.value = string.Empty;

            _selectedTagColorField = _root.Q<ColorField>( "tagColor" );
            _selectedTagTextField = _root.Q<TextField>( "commentField" );

            //Unregister callbacks
            _selectedTagColorField?.UnregisterValueChangedCallback( UpdateTagColor );
            _selectedTagTextField?.UnregisterValueChangedCallback( UpdateTagComment );

            _renameField.RegisterCallback<KeyDownEvent>( evt => {
                if ( evt.keyCode == KeyCode.Return ) {
                    DoRename();
                    _renameField.value = string.Empty;
                }
            } );


            _renameButton.style.visibility = Visibility.Visible;

            _renameButton.clicked += () => {
                DoRename();
                _renameField.value = string.Empty;
            };

            if ( _selectedTagColorField != null ) {
                _selectedTagColorField.value = _selectedTag.Color;
                _selectedTagColorField.RegisterValueChangedCallback( UpdateTagColor );
            }

            if ( _selectedTagTextField != null ) {
                _selectedTagTextField.value = _selectedTag.Comment;
                _selectedTagTextField.RegisterValueChangedCallback( UpdateTagComment );
            }


            _renameToolbar.hierarchy.Insert( 2, _renameButtonDisplay );

            _renameField.style.visibility = Visibility.Visible;
            _renameButtonDisplay.style.visibility = Visibility.Visible;
            _renameButtonDisplay.tooltip = $"Click to show {_selectedTag.name} in the project view.";
            _renameButtonDisplay.style.backgroundColor = _selectedTag.Color;
            _renameButtonDisplay.style.color =
                TaggerDrawer.GetColorLuminosity( _selectedTag.Color ) > 70 ? Color.black : Color.white;
            _renameButtonDisplay.text = _selectedTag.name;
            _renameButtonDisplay.style.color =
                TaggerDrawer.GetColorLuminosity( _selectedTag.Color ) > 70 ? Color.black : Color.white;
            _renameButtonDisplay.clicked += () => {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = _selectedTag;
            };

            _deleteTagButton.style.visibility = Visibility.Visible;
        }

        static void UpdateTagColor( ChangeEvent<Color> evt ) {
            _selectedTag.Color = evt.newValue;
            _renameButtonDisplay.style.backgroundColor = _selectedTag.Color;
            _renameButtonDisplay.style.color =
                TaggerDrawer.GetColorLuminosity( _selectedTag.Color ) > 70 ? Color.black : Color.white;
            foreach ( var element in _allTagsBox.Children() ) {
                if ( element is Button tagButton && tagButton.text == _selectedTag.name ) {
                    tagButton.style.backgroundColor = _selectedTag.Color;
                    tagButton.style.color =
                        TaggerDrawer.GetColorLuminosity( _selectedTag.Color ) > 70 ? Color.black : Color.white;
                }
            }

            NeatoTagAssetModificationProcessor.UpdateTaggers();
        }

        static void UpdateTagComment( ChangeEvent<string> evt ) {
            _selectedTag.Comment = evt.newValue;
            PopulateAllTagsBox();
            NeatoTagAssetModificationProcessor.UpdateTaggers();
        }
    }
}