using System;
using System.Collections.Generic;
using System.Linq;
using CharlieMadeAThing.NeatoTags.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CharlieMadeAThing.NeatoTags.Editor {
    public class NeatoTagManager : EditorWindow {
        static VisualTreeAsset _tagButtonTemplate;

        readonly Dictionary<Button, Action> _buttonActionMap = new();
        ToolbarButton _addTagButton;
        GroupBox _allTagsBox;
        Button _renameButton;
        Button _renameButtonDisplay;
        TextField _renameField;
        Toolbar _renameToolbar;
        GroupBox _tagInfoBox;
        NeatoTagAsset _selectedTag;

        public void CreateGUI() {
            // Each editor window contains a root VisualElement object
            var root = rootVisualElement;

            // Import UXML
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/CharlieMadeAThing/NeatoTags/Editor/NeatoTagManager.uxml" );
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


            PopulateAllTagsBox();
        }

        [MenuItem( "Tools/Neato Tags/Neato Tag Manager" )]
        public static void ShowWindow() {
            var wnd = GetWindow<NeatoTagManager>();
            wnd.titleContent = new GUIContent( "Neato Tag Manager" );
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
            if ( CanRenameTag( tag ) ) {
                var tagPath = AssetDatabase.GetAssetPath( tag );
                AssetDatabase.RenameAsset( tagPath, _renameField.value );
                PopulateAllTagsBox();
                _buttonActionMap.First( x => x.Key.text == tag.name ).Value.Invoke();
            }
        }

        void DisplayTag( ) {
                var visualTree = AssetDatabase
                    .LoadAssetAtPath<VisualTreeAsset>( "Assets/CharlieMadeAThing/NeatoTags/Editor/NeatoTag.uxml" )
                    .Instantiate();
                var tagVisualElement = visualTree.Q<VisualElement>( "topVisualElement" );
                
                var oldVisualElement = _tagInfoBox.Q<VisualElement>( "topVisualElement" );
                if ( oldVisualElement != null ) {
                    _tagInfoBox.Remove( oldVisualElement );
                }

                _tagInfoBox.Add( tagVisualElement );
                

                //Unregister callback from _renameField

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

                var colorField = rootVisualElement.Q<ColorField>( "tagColor" );
                colorField.value = _selectedTag.Color;
                colorField.RegisterValueChangedCallback( evt => {
                    _selectedTag.Color = evt.newValue;
                    PopulateAllTagsBox();
                } );

                var commentField = rootVisualElement.Q<TextField>( "commentField" );
                commentField.value = _selectedTag.Comment;
                commentField.RegisterValueChangedCallback( evt => {
                    _selectedTag.Comment = evt.newValue;
                    PopulateAllTagsBox();
                } );

                var tagIcon = rootVisualElement.Q<Button>( "tagIcon" );
                tagIcon.parent.Remove( tagIcon );

                _renameToolbar.hierarchy.Insert( 0, _renameButtonDisplay );

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
        }
    }