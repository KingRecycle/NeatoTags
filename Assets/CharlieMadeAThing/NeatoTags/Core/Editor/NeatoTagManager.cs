using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace CharlieMadeAThing.NeatoTags.Core.Editor {
    /// <summary>
    ///     The NeatoTags Manager window.
    /// </summary>
    public class NeatoTagManager : EditorWindow {
        VisualTreeAsset _tagButtonTemplate;
        VisualElement _root;

        GroupBox _allTagsBox;
        ToolbarSearchField _tagSearchField;

        //Bottom Half
        Button _renameButton;
        Button _selectedTagButton;
        TextField _renameField;
        Toolbar _renameToolbar;
        GroupBox _tagInfoBox;
        Button _deleteTagButton;
        Button _setTagFolderButton;
        Label _tagDirectoryLabel;
        Button _selectAllButton;

        //Selected Data
        SerializedObject _selectedTag;
        TextField _selectedTagTextField;
        ColorField _selectedTagColorField;

        VisualElement _selectedTagInfoElement;

        //Top Half
        ToolbarButton _addTagButton;

        //Fields for Async Population
        List<NeatoTag> _tagsToProcess;
        int _currentTagIndex;
        bool _isPopulating;
        readonly int _batchSize = 6;
        double _lastSearchTime;
        string _pendingSearchText;
        bool _searchPending;
        readonly float _searchWaitTime = 0.2f;
        bool _isBackspaceHeld;


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

            _setTagFolderButton = _root.Q<Button>( "setTagFolderButton" );

            _tagDirectoryLabel = _root.Q<Label>( "tagDirectoryLabel" );
            UpdatePathLabel();


            _deleteTagButton = _root.Q<Button>( "deleteTagButton" );
            _deleteTagButton.style.visibility = Visibility.Hidden;

            _tagSearchField = _root.Q<ToolbarSearchField>( "tagSearchField" );
            _tagSearchField.tooltip =
                "Search for tags by name. Use ^ at the beginning of your search to search for exact matches.";
            UnregisterCallbacks();
            RegisterCallbacks();
            SetupSearchField();
            PopulateAllTagsBoxAsync();
        }

        [MenuItem( "Window/Neato Tag Manager" )]
        public static void ShowWindow() {
            var wnd = GetWindow<NeatoTagManager>();
            wnd.titleContent = new GUIContent( "Neato Tag Manager" );
        }

        public void ShowWindow( NeatoTag tag ) {
            var wnd = GetWindow<NeatoTagManager>();
            wnd.titleContent = new GUIContent( "Neato Tag Manager" );
            _selectedTag = new SerializedObject( tag );
            DisplayTag();
        }

        bool _isDirty = true;

        void OnEnable() {
            //Keeping track of Tagger components in the scene so got to do an initial grab of all the components on editor scene load.
            //Afterward, they are registered as they are created and unregistered as the component is removed from gameobjects.
            if ( _isDirty ) {
                EditorSceneManager.sceneOpened -= OnSceneOpened;
                EditorSceneManager.sceneOpened += OnSceneOpened;
                NeatoTagTaggerTracker.RegisterTaggersInScene();
                _isDirty = false;
            }
        }

        void OnDisable() {
            UnregisterCallbacks();
        }

        void OnDestroy() {
            UnregisterCallbacks();
        }

        //Register callbacks that can be registered immediately. This is called when the window is opened.
        //EditorApplication.update should only be registered when needed and not when the window is opened.
        void RegisterCallbacks() {
            _addTagButton.clicked += AddNewTag;
            _setTagFolderButton.clicked += SetTagFolder;
            _deleteTagButton.clicked += DeleteSelectedTag;
            _tagSearchField.RegisterValueChangedCallback( OnValueChanged );
            _tagSearchField.RegisterCallback<KeyDownEvent>( OnKeyDown );
            _tagSearchField.RegisterCallback<KeyUpEvent>( OnKeyUp );
        }

        //Unregister all callbacks. This is called when the window is closed.
        void UnregisterCallbacks() {
            _addTagButton.clicked -= AddNewTag;
            _setTagFolderButton.clicked -= SetTagFolder;
            _deleteTagButton.clicked -= DeleteSelectedTag;
            _tagSearchField.UnregisterValueChangedCallback( OnValueChanged );
            _tagSearchField.UnregisterCallback<KeyDownEvent>( OnKeyDown );
            _tagSearchField.UnregisterCallback<KeyUpEvent>( OnKeyUp );
            EditorApplication.update -= ProcessTagBatch;
            EditorApplication.update -= ProcessSearchDebounce;
            UnregisterDisplayCallbacks();
        }
        
        //Register tag-specific callbacks when tag is selected and displayed.
        void RegisterDisplayCallbacks() {
            _renameField.RegisterCallback<KeyDownEvent>(RenameFieldOnKeyDown, TrickleDown.TrickleDown);
            _renameButton.clicked += DoRename;
            _selectAllButton.clicked += SelectAllWithTag;
            _selectedTagColorField?.RegisterValueChangedCallback(UpdateTagColor);
            _selectedTagTextField?.RegisterValueChangedCallback(UpdateTagComment);
            _selectedTagButton.clicked += SelectedTagAssetInProjectView;
        }

        //Unregister tag-specific callbacks when tag is unselected and hidden.
        void UnregisterDisplayCallbacks() {
            _renameField.UnregisterCallback<KeyDownEvent>(RenameFieldOnKeyDown);
            _renameButton.clicked -= DoRename;
            _selectAllButton.clicked -= SelectAllWithTag;
            _selectedTagColorField?.UnregisterValueChangedCallback(UpdateTagColor);
            _selectedTagTextField?.UnregisterValueChangedCallback(UpdateTagComment);
            _selectedTagButton.clicked -= SelectedTagAssetInProjectView;
        }

        void OnSceneOpened( Scene scene, OpenSceneMode mode ) {
            _isDirty = true;
        }

        void UpdatePathLabel() {
            var tagFolderLocation = TagAssetCreation.GetTagFolderLocation();
            var tagPath = tagFolderLocation == string.Empty ? "Not Assigned" : tagFolderLocation;
            _tagDirectoryLabel.text = $"Default Tag Folder Location: {tagPath}";
        }

        void PopulateButtonsWithSearchAsync( string evtNewValue ) {
            if ( string.IsNullOrEmpty( evtNewValue ) ) {
                PopulateAllTagsBoxAsync();
                return;
            }

            if ( _isPopulating ) {
                EditorApplication.update -= ProcessTagBatch;
                _isPopulating = false;
            }

            _allTagsBox.Clear();
            var allTags = TagAssetCreation.GetAllTags().ToList()
                .Where( t => Regex.IsMatch( t.name, $"{evtNewValue}", RegexOptions.IgnoreCase ) )
                .OrderBy( tag => tag.name )
                .ToList();

            _tagsToProcess = allTags;
            _currentTagIndex = 0;
            _isPopulating = true;

            EditorApplication.update += ProcessTagBatch;
        }

        void SetupSearchField() {
            // Clear any existing callbacks to avoid duplicates
            _tagSearchField.UnregisterValueChangedCallback( evt => {
                PopulateButtonsWithSearchAsync( evt.newValue );
            } );

            _tagSearchField.UnregisterCallback<KeyDownEvent>( OnKeyDown );
            _tagSearchField.UnregisterCallback<KeyUpEvent>( OnKeyUp );

            // Register new callbacks
            _tagSearchField.RegisterCallback<KeyDownEvent>( OnKeyDown, TrickleDown.TrickleDown );
            _tagSearchField.RegisterCallback<KeyUpEvent>( OnKeyUp, TrickleDown.TrickleDown );

            _tagSearchField.RegisterValueChangedCallback( OnValueChanged );
            _tagSearchField.Focus();
        }

        void OnKeyDown( KeyDownEvent keydownEvent ) {
            if ( keydownEvent.keyCode == KeyCode.Backspace ) {
                _isBackspaceHeld = true;

                // Cancel any pending search
                if ( _searchPending ) {
                    _searchPending = false;
                    EditorApplication.update -= ProcessSearchDebounce;
                }
            }
        }

        void OnKeyUp( KeyUpEvent keydownEvent ) {
            if ( keydownEvent.keyCode == KeyCode.Backspace ) {
                _isBackspaceHeld = false;
                _lastSearchTime = EditorApplication.timeSinceStartup;

                // Schedule a search after a delay
                if ( !_searchPending ) {
                    _searchPending = true;
                    EditorApplication.update += ProcessSearchDebounce;
                }
            }
        }

        void OnValueChanged( ChangeEvent<string> evt ) {
            _pendingSearchText = evt.newValue;
            _lastSearchTime = EditorApplication.timeSinceStartup;

            // Don't trigger immediate search for backspace
            if ( _isBackspaceHeld ) {
                // Stop any current population
                if ( _isPopulating ) {
                    EditorApplication.update -= ProcessTagBatch;
                    _isPopulating = false;
                }

                return;
            }

            // For non-backspace changes, schedule search with debounce
            if ( !_searchPending ) {
                _searchPending = true;
                EditorApplication.update += ProcessSearchDebounce;
            }
        }

        //Prevents the search field from updating too frequently and causing performance issues.
        void ProcessSearchDebounce() {
            // If we're still holding backspace, don't process yet
            if ( _isBackspaceHeld ) {
                return;
            }

            var debounceTime = string.IsNullOrEmpty( _pendingSearchText ) ? 0.1f : _searchWaitTime;

            // Wait for the debounce time to elapse
            if ( EditorApplication.timeSinceStartup - _lastSearchTime < debounceTime ) {
                return;
            }

            _searchPending = false;
            EditorApplication.update -= ProcessSearchDebounce;

            // Only populate if we're not still holding backspace
            if ( !_isBackspaceHeld && !string.IsNullOrEmpty( _pendingSearchText ) ) {
                PopulateButtonsWithSearchAsync( _pendingSearchText );
            }
            else {
                PopulateAllTagsBoxAsync();
            }
        }


        void DeleteSelectedTag() {
            if ( !_selectedTag.targetObject ) {
                return;
            }

            TagAssetCreation.DeleteTag( _selectedTag.targetObject as NeatoTag );
            //Remove tag button from tag manager window.
            foreach ( var ele in _allTagsBox.Children() ) {
                if ( ele is not Button { userData: NeatoTag tag } || tag != _selectedTag.targetObject ) continue;
                _allTagsBox.Remove( ele );
                break;
            }

            _selectedTag = null;
            UnDisplayTag();
        }

        void PopulateAllTagsBoxAsync() {
            if ( _isPopulating ) {
                EditorApplication.update -= ProcessTagBatch;
                return;
            }

            _allTagsBox.Clear();
            var allTags = TagAssetCreation.GetAllTags().ToList().OrderBy( tag => tag.name );
            _tagsToProcess = allTags.ToList();
            _currentTagIndex = 0;
            _isPopulating = true;
            EditorApplication.update += ProcessTagBatch;
        }

        void ProcessTagBatch() {
            if ( !_isPopulating || _tagsToProcess == null ) {
                EditorApplication.update -= ProcessTagBatch;
                return;
            }

            // Process a batch of tags
            var endIndex = Mathf.Min( _currentTagIndex + _batchSize, _tagsToProcess.Count );
            for ( var i = _currentTagIndex; i < endIndex; i++ ) {
                _allTagsBox.Add( CreateTagButton( _tagsToProcess[i] ) );
            }

            _currentTagIndex = endIndex;

            // Check if we're done
            if ( _currentTagIndex >= _tagsToProcess.Count ) {
                _isPopulating = false;
                _tagsToProcess = null;
                EditorApplication.update -= ProcessTagBatch;
                EditorUtility.ClearProgressBar();
            }
        }

        Button CreateTagButton( NeatoTag tag ) {
            var button = _tagButtonTemplate.Instantiate().Q<Button>();
            if ( tag.Comment != string.Empty ) {
                button.tooltip = tag.Comment;
            }

            button.text = tag.name;
            button.style.backgroundColor = tag.Color;
            button.style.color = TaggerDrawer.GetTextColorBasedOnBackground( button.style.backgroundColor.value );
            button.userData = tag;

            button.clicked += () => {
                var associatedTag = button.userData as NeatoTag;
                if ( associatedTag == null ) return;
                _selectedTag = new SerializedObject( associatedTag );
                DisplayTag();
            };

            return button;
        }

        //Checks if the tag name is valid and if the name doesn't already exist.
        bool CanRenameTag( string newName ) {
            //Trim the name to remove any leading or trailing spaces.
            if ( newName == string.Empty ) {
                Debug.LogWarning("Tried to rename tag to an empty string.");
                return false;
            }

            foreach ( var element in _allTagsBox.Children() ) {
                var button = element.Q<Button>();
                if ( button.text.Equals( newName ) ) {
                    Debug.LogWarning(
                        $"Tried to rename tag {_selectedTag.targetObject.name} to {newName} but a tag with that name already exists." );
                    return false;
                }
            }

            return true;
        }

        void DoRename() {
            var newName = _renameField.text.Trim();
            if ( !CanRenameTag( newName ) ) {
                return;
            }

            var tagPath = AssetDatabase.GetAssetPath( _selectedTag.targetObject );
            AssetDatabase.RenameAsset( tagPath, newName );
            _selectedTag.targetObject.name = newName;
            EditorUtility.SetDirty( _selectedTag.targetObject );

            AssetDatabase.SaveAssets();
            _selectedTag.Update();
            _selectedTag.ApplyModifiedProperties();

            TagAssetCreation.InvalidateTagCache();
            PopulateAllTagsBoxAsync();
            NeatoTagAssetModificationProcessor.UpdateTaggers();
            _renameField.value = string.Empty;

            DisplayTag();
        }

        void RenameFieldOnKeyDown( KeyDownEvent evt ) {
            if ( evt.keyCode == KeyCode.Return ) {
                DoRename();
            }
        }

        void UnDisplayTag() {
            UnregisterDisplayCallbacks();
            _tagInfoBox.Remove( _selectedTagInfoElement );
            _renameField.style.visibility = Visibility.Hidden;
            _renameButton.style.visibility = Visibility.Hidden;
            _selectedTagButton.style.visibility = Visibility.Hidden;
            _deleteTagButton.style.visibility = Visibility.Hidden;
            _selectAllButton.style.visibility = Visibility.Hidden;
        }

        //NOTE: Don't forget to unsubscribe from events before subscribing to them again.
        void DisplayTag() {
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

            _renameButton.style.visibility = Visibility.Visible;
            _renameButton.tooltip = $"Rename {_selectedTag.targetObject.name} tag.";

            _selectAllButton.style.visibility = Visibility.Visible;
            _selectAllButton.tooltip =
                $"Select all gameobjects in scene with the {_selectedTag.targetObject.name} tag.";
            
            RegisterDisplayCallbacks();

            if ( _selectedTagColorField != null ) {
                _selectedTagColorField.value = _selectedTag.FindProperty( "color" ).colorValue;
            }

            if ( _selectedTagTextField != null ) {
                _selectedTagTextField.value = _selectedTag.FindProperty( "comment" ).stringValue;
            }


            _renameToolbar.hierarchy.Insert( 2, _selectedTagButton );

            _renameField.style.visibility = Visibility.Visible;
            _selectedTagButton.style.visibility = Visibility.Visible;
            _selectedTagButton.tooltip = $"Click to show {_selectedTag.targetObject.name} in the project view.";
            _selectedTagButton.style.backgroundColor = _selectedTag.FindProperty( "color" ).colorValue;
            _selectedTagButton.text = _selectedTag.targetObject.name;
            _selectedTagButton.style.color =
                TaggerDrawer.GetTextColorBasedOnBackground( _selectedTag.FindProperty( "color" ).colorValue );

            _deleteTagButton.style.visibility = Visibility.Visible;
        }

        //Called the SelectedAllGameObjectsWithTaggerThatHasTag() function
        //Wrapper function to play nice with events/callbacks since they don't like functions with parameters.
        void SelectAllWithTag() {
            NeatoTagTaggerTracker.SelectAllGameObjectsWithTaggerThatHasTag( _selectedTag.targetObject as NeatoTag );
        }

        //Selects and puts into focus the selected tag in the project view.
        void SelectedTagAssetInProjectView() {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = _selectedTag.targetObject;
        }

        void AddNewTag() {
            var newTag = TagAssetCreation.CreateNewTag( "New Tag" );
            var newTagButton = CreateTagButton( newTag );
            _allTagsBox.Add( newTagButton );

            if ( !newTag ) return;
            foreach ( var element in _allTagsBox.Children() ) {
                if ( element is not Button { userData: NeatoTag tag } || tag != newTag ) continue;
                _selectedTag = new SerializedObject( newTag );
                DisplayTag();
                break;
            }
        }

        void SetTagFolder() {
            TagAssetCreation.SetTagFolder();
            UpdatePathLabel();
        }

        void UpdateTagColor( ChangeEvent<Color> evt ) {
            _selectedTag.FindProperty( "color" ).colorValue = evt.newValue;
            _selectedTagButton.style.backgroundColor = _selectedTag.FindProperty( "color" ).colorValue;
            _selectedTagButton.style.color =
                TaggerDrawer.GetTextColorBasedOnBackground( _selectedTagButton.style.backgroundColor.value );
            foreach ( var element in _allTagsBox.Children() ) {
                if ( element is not Button tagButton || tagButton.text != _selectedTag.targetObject.name ) {
                    continue;
                }

                tagButton.style.backgroundColor = _selectedTag.FindProperty( "color" ).colorValue;
                tagButton.style.color =
                    TaggerDrawer.GetTextColorBasedOnBackground( tagButton.style.backgroundColor.value );
            }

            _selectedTag.ApplyModifiedProperties();
            NeatoTagAssetModificationProcessor.UpdateTaggers();
        }

        void UpdateTagComment( ChangeEvent<string> evt ) {
            _selectedTag.FindProperty( "comment" ).stringValue = evt.newValue;
            _selectedTag.ApplyModifiedProperties();
            NeatoTagAssetModificationProcessor.UpdateTaggers();
        }
    }
}