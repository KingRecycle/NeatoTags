using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Plastic.Antlr3.Runtime.Debug;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace CharlieMadeAThing.NeatoTags.Core.Editor {
    /// <summary>
    /// The NeatoTags Manager window.
    /// </summary>
    public class NeatoTagManager : EditorWindow {
        static VisualTreeAsset _tagButtonTemplate;
        static VisualElement _root;

        static readonly Dictionary<Button, Action> BUTTON_ACTION_MAP = new();
        static GroupBox _allTagsBox;
        static ToolbarSearchField _tagSearchField;

        //Bottom Half
        static Button _renameButton;
        static Button _selectedTagButton;
        static TextField _renameField;
        static Toolbar _renameToolbar;
        static GroupBox _tagInfoBox;
        static Button _deleteTagButton;
        static Button _setTagFolderButton;
        static Label _tagDirectoryLabel;
        static Button _selectAllButton;

        //Selected Data
        static SerializedObject _selectedTag;
        static TextField _selectedTagTextField;
        static ColorField _selectedTagColorField;

        static VisualElement _selectedTagInfoElement;

        //Top Half
        ToolbarButton _addTagButton;
        
        
        public void CreateGUI() {
            _root = rootVisualElement;
            
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
            
            _selectAllButton = _root.Q<Button>( "selectAllButton" );
            _selectAllButton.style.visibility = Visibility.Hidden;
            

            _selectedTagButton = _tagButtonTemplate.Instantiate().Q<Button>();
            _selectedTagButton.style.visibility = Visibility.Hidden;

            _addTagButton = _root.Q<ToolbarButton>( "addTagButton" );
            _addTagButton.clicked -= AddNewTag;
            _addTagButton.clicked += AddNewTag;

            _setTagFolderButton = _root.Q<Button>( "setTagFolderButton" );
            _setTagFolderButton.clicked -= SetTagFolder;
            _setTagFolderButton.clicked += SetTagFolder;

            _tagDirectoryLabel = _root.Q<Label>( "tagDirectoryLabel" );
            UpdatePathLabel();


            _deleteTagButton = _root.Q<Button>( "deleteTagButton" );
            _deleteTagButton.clicked -= DeleteSelectedTag;
            _deleteTagButton.clicked += DeleteSelectedTag;
            _deleteTagButton.style.visibility = Visibility.Hidden;

            _tagSearchField = _root.Q<ToolbarSearchField>( "tagSearchField" );
            _tagSearchField.tooltip = $"Search for tags by name. Use ^ at the beginning of your search to search for exact matches.";
            _tagSearchField.UnregisterValueChangedCallback( evt => { PopulateButtonsWithSearch( evt.newValue ); } );
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
            _selectedTag = new SerializedObject( tag );
            DisplayTag();
        }

        static bool _isDirty = true;
        void OnEnable() {
            //Keeping track of Tagger components in the scene so got to do an initial grab of all the components on editor scene load.
            //Afterwards, they are registered as they are created and unregistered as the component is removed from gameobjects.
            if ( _isDirty ) {
                EditorSceneManager.sceneOpened -= OnSceneOpened;
                EditorSceneManager.sceneOpened += OnSceneOpened;
                NeatoTagTaggerTracker.RegisterTaggersInScene();
                _isDirty = false;
            }
        }

        static void OnSceneOpened( Scene scene, OpenSceneMode mode ) {
            _isDirty = true;
        }

        static void UpdatePathLabel() {
            var tagFolderLocation = TagAssetCreation.GetTagFolderLocation();
            var tagPath = tagFolderLocation == string.Empty ? "Not Assigned" : tagFolderLocation;
            _tagDirectoryLabel.text = $"Default Tag Folder Location: {tagPath}";
        }

        static void PopulateButtonsWithSearch( string evtNewValue ) {
            if ( evtNewValue == string.Empty ) {
                PopulateAllTagsBox();
                return;
            }

            _allTagsBox.Clear();
            var allTags = TagAssetCreation.GetAllTags().ToList().OrderBy( tag => tag.name );
            foreach ( var neatoTagAsset in allTags ) {
                if ( Regex.IsMatch( neatoTagAsset.name, $"{evtNewValue}", RegexOptions.IgnoreCase ) ) {
                    _allTagsBox.Add( CreateTagButton( neatoTagAsset ) );
                }
            }
        }

        static void DeleteSelectedTag() {
            if ( !_selectedTag.targetObject ) return;
            TagAssetCreation.DeleteTag( _selectedTag.targetObject as NeatoTag );
            _selectedTag = null;
            PopulateAllTagsBox();
            UnDisplayTag();
        }


        static void PopulateAllTagsBox() {
            _allTagsBox.Clear();
            BUTTON_ACTION_MAP.Clear();

            var allTags = TagAssetCreation.GetAllTags().ToList().OrderBy( tag => tag.name );

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
            button.style.color = TaggerDrawer.GetTextColorBasedOnBackground( button.style.backgroundColor.value );

            BUTTON_ACTION_MAP.Add( button, () => {
                _selectedTag = new SerializedObject( tag );
                DisplayTag();
            } );

            button.clicked += () => BUTTON_ACTION_MAP[button].Invoke();
            return button;
        }
        
        //Checks if the tag name is valid and if the name doesn't already exist.
        static bool CanRenameTag( string newName ) {
            //Trim the name to remove any leading or trailing spaces.
            if ( newName == string.Empty ) {
                Debug.LogWarning( $"[Neato Tag Manager]: Tried to rename tag {_selectedTag.targetObject.name} but no name was entered." );
            }

            foreach ( var element in _allTagsBox.Children() ) {
                var button = element.Q<Button>();
                if ( button.text.Equals( newName ) ) {
                    Debug.LogWarning($"Tried to rename tag {_selectedTag.targetObject.name} to {newName} but a tag with that name already exists.");
                }
            }

            return true;
        }

        static void DoRename() {
            var newName = _renameField.text.Trim();
            if ( !CanRenameTag( newName ) ) return;
            var tagPath = AssetDatabase.GetAssetPath( _selectedTag.targetObject );
            AssetDatabase.RenameAsset( tagPath, newName );
            PopulateAllTagsBox();
            BUTTON_ACTION_MAP.First( x => x.Key.text == _selectedTag.targetObject.name ).Value.Invoke();
            NeatoTagAssetModificationProcessor.UpdateTaggers();
            _renameField.value = string.Empty;
            _selectedTag.ApplyModifiedProperties();
        }

        static void UnDisplayTag() {
            _tagInfoBox.Remove( _selectedTagInfoElement );
            _renameField.style.visibility = Visibility.Hidden;
            _renameButton.style.visibility = Visibility.Hidden;
            _selectedTagButton.style.visibility = Visibility.Hidden;
            _deleteTagButton.style.visibility = Visibility.Hidden;
            _selectAllButton.style.visibility = Visibility.Hidden;
        }
        
        //NOTE: Don't forget to unsubscribe from events before subscribing to them again.
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

            
            //Event when Enter/Return key is pressed when rename field is focused.
            _renameField.UnregisterCallback<KeyDownEvent>( evt => {
                if ( evt.keyCode != KeyCode.Return ) return;
                DoRename();
            } );
            _renameField.RegisterCallback<KeyDownEvent>( evt => {
                if ( evt.keyCode != KeyCode.Return ) return;
                DoRename();
            } );

            //Event when Rename button is clicked
            _renameButton.clicked -= DoRename;
            _renameButton.clicked += DoRename;

            _renameButton.style.visibility = Visibility.Visible;
            _renameButton.tooltip = $"Rename {_selectedTag.targetObject.name} tag.";
            
            _selectAllButton.style.visibility = Visibility.Visible;
            _selectAllButton.tooltip = $"Select all gameobjects in scene with the {_selectedTag.targetObject.name} tag.";
            
            _selectAllButton.clicked -= SelectAllWithTag;
            _selectAllButton.clicked += SelectAllWithTag;

            if ( _selectedTagColorField != null ) {
                _selectedTagColorField.value = _selectedTag.FindProperty("color").colorValue;
                _selectedTagColorField.RegisterValueChangedCallback( UpdateTagColor );
            }

            if ( _selectedTagTextField != null ) {
                _selectedTagTextField.value = _selectedTag.FindProperty( "comment" ).stringValue;
                _selectedTagTextField.RegisterValueChangedCallback( UpdateTagComment );
            }


            _renameToolbar.hierarchy.Insert( 2, _selectedTagButton );

            _renameField.style.visibility = Visibility.Visible;
            _selectedTagButton.style.visibility = Visibility.Visible;
            _selectedTagButton.tooltip = $"Click to show {_selectedTag.targetObject.name} in the project view.";
            _selectedTagButton.style.backgroundColor = _selectedTag.FindProperty("color").colorValue;
            _selectedTagButton.text = _selectedTag.targetObject.name;
            _selectedTagButton.style.color =
                TaggerDrawer.GetTextColorBasedOnBackground( _selectedTag.FindProperty("color").colorValue );

            _selectedTagButton.clicked -= SelectedTagAssetInProjectView;
            _selectedTagButton.clicked += SelectedTagAssetInProjectView;

            _deleteTagButton.style.visibility = Visibility.Visible;
        }

        //Called the SelectedAllGameObjectsWithTaggerThatHasTag() function
        //Wrapper function to play nice with events/callbacks since they don't like functions with parameters.
        static void SelectAllWithTag() {
            NeatoTagTaggerTracker.SelectAllGameObjectsWithTaggerThatHasTag( _selectedTag.targetObject as NeatoTag );
        }
        
        //Selects and puts into focus the selected tag in the project view.
        static void SelectedTagAssetInProjectView() {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = _selectedTag.targetObject;
        }

        static void AddNewTag() {
            var newTag = TagAssetCreation.CreateNewTag( "New Tag" );
            PopulateAllTagsBox();
            if ( newTag ) {
                BUTTON_ACTION_MAP.First( x => x.Key.text == newTag.name ).Value.Invoke();
            }
        }

        static void SetTagFolder() {
            TagAssetCreation.SetTagFolder();
            UpdatePathLabel();
        }

        static void UpdateTagColor( ChangeEvent<Color> evt ) {
            _selectedTag.FindProperty("color").colorValue = evt.newValue;
            _selectedTagButton.style.backgroundColor = _selectedTag.FindProperty("color").colorValue;
            _selectedTagButton.style.color =
                TaggerDrawer.GetTextColorBasedOnBackground( _selectedTagButton.style.backgroundColor.value );
            foreach ( var element in _allTagsBox.Children() ) {
                if ( element is not Button tagButton || tagButton.text != _selectedTag.targetObject.name ) continue;
                tagButton.style.backgroundColor = _selectedTag.FindProperty("color").colorValue;
                tagButton.style.color =
                    TaggerDrawer.GetTextColorBasedOnBackground( tagButton.style.backgroundColor.value );
            }
            _selectedTag.ApplyModifiedProperties();
            NeatoTagAssetModificationProcessor.UpdateTaggers();
        }

        static void UpdateTagComment( ChangeEvent<string> evt ) {
            _selectedTag.FindProperty("comment").stringValue = evt.newValue;
            PopulateAllTagsBox();
            _selectedTag.ApplyModifiedProperties();
            NeatoTagAssetModificationProcessor.UpdateTaggers();
        }
        
    }
}