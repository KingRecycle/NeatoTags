using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CharlieMadeAThing.NeatoTags.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CharlieMadeAThing.NeatoTags.Editor {
    public class NeatoTagManager : EditorWindow {
        static VisualTreeAsset _tagButtonTemplate;

        readonly Dictionary<Button, Action> _buttonActionMap = new();
        //Top Half
        ToolbarButton _addTagButton;
        GroupBox _allTagsBox;
        ToolbarSearchField _tagSearchField;
        
        //Bottom Half
        Button _renameButton;
        Button _renameButtonDisplay;
        TextField _renameField;
        Toolbar _renameToolbar;
        GroupBox _tagInfoBox;
        Button _deleteTagButton;
        
        //Selected Data
        NeatoTagAsset _selectedTag;
        TextField _selectedTagTextField;
        ColorField _selectedTagColorField;
        VisualElement _selectedTagInfoElement;
        
        [MenuItem( "Tools/Neato Tags/Neato Tag Manager" )]
        public static void ShowWindow() {
            var wnd = GetWindow<NeatoTagManager>();
            wnd.titleContent = new GUIContent( "Neato Tag Manager" );
        }
        

        public void CreateGUI() {
            // Each editor window contains a root VisualElement object
            var root = rootVisualElement;

            // Import UXML
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/CharlieMadeAThing/NeatoTags/Editor/NeatoTagManager 1.uxml" );
            visualTree.CloneTree( root );

            

            _tagButtonTemplate =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/CharlieMadeAThing/NeatoTags/Editor/buttonTag.uxml" );

            _allTagsBox = root.Q<GroupBox>( "allTagsBox" );
            _tagInfoBox = root.Q<GroupBox>( "tagInfoBox" );
            _renameToolbar = root.Q<Toolbar>( "renameToolbar" );


            _renameField = root.Q<TextField>( "renameField" );
            _renameField.style.visibility = Visibility.Hidden;

            _renameButton = root.Q<Button>( "renameButton" );
            _renameButton.style.visibility = Visibility.Hidden;

            _renameButtonDisplay = _tagButtonTemplate.Instantiate().Q<Button>();
            _renameButtonDisplay.style.visibility = Visibility.Hidden;

            _addTagButton = root.Q<ToolbarButton>( "addTagButton" );
            _addTagButton.clicked += () => {
                var newTag = TagAssetCreation.CreateNewTag( "New Tag" );
                PopulateAllTagsBox();
                if ( newTag ) {
                    _buttonActionMap.First( x => x.Key.text == newTag.name ).Value.Invoke();
                }
            };
            
            
            
            _deleteTagButton = root.Q<Button>( "deleteTagButton" );
            _deleteTagButton.clicked += DeleteSelectedTag;
            
            _tagSearchField = root.Q<ToolbarSearchField>( "tagSearchField" );
            _tagSearchField.RegisterValueChangedCallback( evt => {
                PopulateButtonsWithSearch( evt.newValue );
            } );

            PopulateAllTagsBox();
            
        }

        void PopulateButtonsWithSearch( string evtNewValue ) {
            if( evtNewValue == string.Empty ) {
                PopulateAllTagsBox();
                return;
            }
            _allTagsBox.Clear();
            var allTags = Tagger.GetAllTags();
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

        
        void PopulateAllTagsBox() {
            _allTagsBox.Clear();
            _buttonActionMap.Clear();

            var allTags = Tagger.GetAllTags();

            foreach ( var neatoTagAsset in allTags ) {
                _allTagsBox.Add( CreateTagButton( neatoTagAsset ) );
            }
        }


        Button CreateTagButton( NeatoTagAsset tag ) {
            var button = _tagButtonTemplate.Instantiate().Q<Button>();
            if ( tag.Comment != string.Empty ) {
                button.tooltip = tag.Comment;
            }

            button.text = tag.name;
            Color.RGBToHSV( tag.Color, out var h, out var s, out var v );
            button.style.unityBackgroundImageTintColor = tag.Color;
            button.style.color = TaggerDrawer.GetColorLuminosity( tag.Color ) > 70 ? Color.black : Color.white;

            _buttonActionMap.Add( button, () => {
                _selectedTag = tag;
                DisplayTag();
            } );

            button.clicked += () => _buttonActionMap[button].Invoke();
            return button;
        }

        bool CanRenameTag( NeatoTagAsset tag ) {
            if ( _renameField.value != string.Empty ) {
                foreach ( var element in _allTagsBox.Children() ) {
                    if ( element is Button tagButton && tagButton.text == _renameField.value ) {
                        return false;
                    }
                }
            }

            return true;
        }
        
        void DoRename( NeatoTagAsset tag ) {
            if ( !CanRenameTag( tag ) ) return;
            var color = tag.Color;
            var comment = tag.Comment;
            var tagPath = AssetDatabase.GetAssetPath( tag );
            AssetDatabase.RenameAsset( tagPath, _renameField.value );
            tag.Color = color;
            tag.Comment = comment;
            PopulateAllTagsBox();
            _buttonActionMap.First( x => x.Key.text == tag.name ).Value.Invoke();
        }

        void UnDisplayTag() {
            _tagInfoBox.Remove( _selectedTagInfoElement);
            _renameField.style.visibility = Visibility.Hidden;
            _renameButton.style.visibility = Visibility.Hidden;
            _renameButtonDisplay.style.visibility = Visibility.Hidden;
        }

        void DisplayTag() {
                var visualTree = AssetDatabase
                    .LoadAssetAtPath<VisualTreeAsset>( "Assets/CharlieMadeAThing/NeatoTags/Editor/NeatoTag.uxml" )
                    .Instantiate();
                var tagVisualElement = visualTree.Q<VisualElement>( "topVisualElement" );

                _selectedTagInfoElement = tagVisualElement;
                
                var oldVisualElement = _tagInfoBox.Q<VisualElement>( "topVisualElement" );
                if ( oldVisualElement != null ) {
                    _tagInfoBox.Remove( oldVisualElement );
                }

                _tagInfoBox.Add( tagVisualElement );

                _tagSearchField.value = string.Empty;
                
                _selectedTagColorField = rootVisualElement.Q<ColorField>( "tagColor" );
                _selectedTagTextField = rootVisualElement.Q<TextField>( "commentField" );

                //Unregister callbacks
                _selectedTagColorField?.UnregisterValueChangedCallback( UpdateTagColor );
                _selectedTagTextField?.UnregisterValueChangedCallback( UpdateTagComment );

                _renameField.RegisterCallback<KeyDownEvent>( evt => {
                    if ( evt.keyCode == KeyCode.Return ) {
                        DoRename( _selectedTag );
                        _renameField.value = string.Empty;
                    }
                } );


                _renameButton.style.visibility = Visibility.Visible;

                _renameButton.clicked += () => {
                    DoRename( _selectedTag );
                    _renameField.value = string.Empty;
                };
                
                _selectedTagColorField.value = _selectedTag.Color;
                _selectedTagColorField.RegisterValueChangedCallback( UpdateTagColor );
                
                _selectedTagTextField.value = _selectedTag.Comment;
                _selectedTagTextField.RegisterValueChangedCallback( UpdateTagComment );

                var tagIcon = rootVisualElement.Q<Button>( "tagIcon" );
                tagIcon.parent.Remove( tagIcon );

                _renameToolbar.hierarchy.Insert( 2, _renameButtonDisplay );

                _renameField.style.visibility = Visibility.Visible;
                _renameButtonDisplay.style.visibility = Visibility.Visible;

                _renameButtonDisplay.style.unityBackgroundImageTintColor = _selectedTag.Color;
                _renameButtonDisplay.style.color =
                    TaggerDrawer.GetColorLuminosity( _selectedTag.Color ) > 70 ? Color.black : Color.white;
                _renameButtonDisplay.text = _selectedTag.name;
                _renameButtonDisplay.style.color =
                    TaggerDrawer.GetColorLuminosity( _selectedTag.Color ) > 70 ? Color.black : Color.white;
                _renameButtonDisplay.clicked += () => {
                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = _selectedTag;
                };
                
        }

        void UpdateTagColor( ChangeEvent<Color> evt ) {
            _selectedTag.Color = evt.newValue;
            _renameButtonDisplay.style.unityBackgroundImageTintColor = _selectedTag.Color;
            _renameButtonDisplay.style.color =
                TaggerDrawer.GetColorLuminosity( _selectedTag.Color ) > 70 ? Color.black : Color.white;
            
        }

        void UpdateTagComment( ChangeEvent<string> evt ) {
            _selectedTag.Comment = evt.newValue;
            PopulateAllTagsBox();
        }
    }
}