using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CharlieMadeAThing.NeatoTags.Core.Editor {
    [CustomEditor( typeof(Tagger) ), CanEditMultipleObjects]
    public class TaggerDrawer : UnityEditor.Editor {
        static VisualTreeAsset s_tagButtonTemplate;
        static VisualTreeAsset s_tagButtonWithXTemplate;
        Button _addTagButton;
        TextField _addTagTextField;
        GroupBox _allTagsBox;
        ToolbarButton _editTaggerButton;

        bool _isEditTaggerMode;

        //UI
        VisualElement _root;
        ToolbarSearchField _searchField;
        Label _searchLabel;

        //Tagger (not the drawer)
        SerializedObject _tagger;
        ToolbarSearchField _taggerSearchAvailable;
        GroupBox _tagViewerDeselected;
        GroupBox _tagViewerSelected;

        Tagger Target => target as Tagger;

        void OnEnable() {
            InitializeUI();
            NeatoTagAssetModificationProcessor.RegisterTaggerDrawer( this );
            NeatoTagDrawer.RegisterTaggerDrawer( this );
            PopulateButtons();
            Target.OnWantRepaint += DoRepaint;
            _tagger = new SerializedObject( Target );
        }

        void InitializeUI() {
            _root = new VisualElement();

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( UxmlDataLookup.TaggerUxml );
            if ( !visualTree ) {
                Debug.LogError( "Failed to load tagger UXML template." );
                return;
            }

            visualTree.CloneTree( _root );

            s_tagButtonTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( UxmlDataLookup.ButtonTagUxml );
            if ( !s_tagButtonTemplate ) {
                Debug.LogError( "Failed to load tag UXML template." );
                return;
            }

            s_tagButtonWithXTemplate =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( UxmlDataLookup.ButtonTagWithXUxml );
            if ( !s_tagButtonWithXTemplate ) {
                Debug.LogError( "Failed to load tag UXML template." );
                return;
            }

            FindUIElements();
            ConfigureUIElements();
        }

        void FindUIElements() {
            _tagViewerSelected = _root.Q<GroupBox>( "tagViewer" );
            _tagViewerDeselected = _root.Q<GroupBox>( "allTagViewer" );
            _searchField = _root.Q<ToolbarSearchField>( "taggerSearch" );
            _searchLabel = _root.Q<Label>( "searchLabel" );
            _taggerSearchAvailable = _root.Q<ToolbarSearchField>( "taggerSearchAvailable" );
            _addTagButton = _root.Q<Button>( "addTagButton" );
            _addTagTextField = _root.Q<TextField>( "addTagTextField" );
            _allTagsBox = _root.Q<GroupBox>( "allTagsBox" );
            _editTaggerButton = _root.Q<ToolbarButton>( "editTaggerButton" );
        }

        void ConfigureUIElements() {
            _searchField.RegisterValueChangedCallback( _ => { PopulateButtonsWithSearch(); } );
            _searchField.tooltip =
                $"Search through tags currently tagged to {target.name}. Use ^ at the beginning to search for exact matches.";

            _taggerSearchAvailable.RegisterValueChangedCallback( _ => { PopulateButtonsWithSearch(); } );
            _taggerSearchAvailable.tooltip =
                $"Search through tags currently available to add to {target.name}. Use ^ at the beginning to search for exact matches.";

            _addTagButton.tooltip = $"Create a new tag and add it to {target.name}";
            _addTagButton.clicked -= CreateNewTag;
            _addTagButton.clicked += CreateNewTag;

            _addTagTextField.RegisterCallback<KeyDownEvent>( OnAddTagKeyDown );

            _editTaggerButton.tooltip = $"Edit Tags for {target.name}";

            _searchField.style.display = DisplayStyle.None;
            _addTagButton.style.display = DisplayStyle.None;
            _addTagTextField.style.display = DisplayStyle.None;
            _searchLabel.style.display = DisplayStyle.None;
            _allTagsBox.style.display = DisplayStyle.None;
            _editTaggerButton.clicked -= ShowEditTagger;
            _editTaggerButton.clicked += ShowEditTagger;
        }

        void OnAddTagKeyDown( KeyDownEvent evt ) {
            if ( evt.keyCode == KeyCode.Return ) {
                CreateNewTag();
            }
        }

        void OnDisable() {
            Target.OnWantRepaint -= DoRepaint;
            if ( Target == null ) {
                NeatoTagTaggerTracker.UnregisterTagger( _tagger.targetObject as Tagger );
            }

            NeatoTagAssetModificationProcessor.UnregisterTaggerDrawer( this );
        }

        void DoRepaint() {
            PopulateButtons();
            Repaint();
        }

        void ShowEditTagger() {
            _isEditTaggerMode = !_isEditTaggerMode;
            if ( _isEditTaggerMode ) {
                _searchField.style.display = DisplayStyle.Flex;
                _addTagButton.style.display = DisplayStyle.Flex;
                _addTagTextField.style.display = DisplayStyle.Flex;
                _searchLabel.style.display = DisplayStyle.Flex;
                _allTagsBox.style.display = DisplayStyle.Flex;
                PopulateButtons();
            }
            else {
                _searchField.style.display = DisplayStyle.None;
                _addTagButton.style.display = DisplayStyle.None;
                _addTagTextField.style.display = DisplayStyle.None;
                _searchLabel.style.display = DisplayStyle.None;
                _allTagsBox.style.display = DisplayStyle.None;
                PopulateButtons();
            }
        }

        void CreateNewTag() {
            Undo.RecordObject( serializedObject.targetObject, "Create New Tag" );
            var tag = TagAssetCreation.CreateNewTag( _addTagTextField.value, false );
            if ( targets.Length > 1 ) {
                foreach ( var targetObject in targets ) {
                    var tagger = targetObject as Tagger;
                    if ( tagger != null ) {
                        tagger.AddTag( tag );
                    }
                }
            }
            else {
                ((Tagger)target).AddTag( tag );
            }

            UpdatePrefab();
            PopulateButtons();
        }


        public override VisualElement CreateInspectorGUI() => _root;


        Button CreateDeselectedButton( NeatoTag tag ) {
            var button = s_tagButtonTemplate.Instantiate().Q<Button>();
            if ( tag.Comment != string.Empty ) {
                button.tooltip = tag.Comment;
            }

            StyleButtonAvailable( button, tag );
            button.clicked += () => {
                Undo.RecordObject( target as Tagger, $"Added Tag: {tag.name}" );
                if ( targets.Length > 1 ) {
                    foreach ( var obj in targets ) {
                        ((Tagger)obj).AddTag( tag );
                    }
                }
                else {
                    ((Tagger)target).AddTag( tag );
                }

                if ( _taggerSearchAvailable.value != string.Empty ) {
                    PopulateButtonsWithSearch();
                }
                else {
                    PopulateButtons();
                }

                UpdatePrefab();
            };

            return button;
        }

        VisualElement CreateSelectedButton( NeatoTag tag ) {
            var tagButton = _isEditTaggerMode
                ? s_tagButtonWithXTemplate.Instantiate().Q<VisualElement>()
                : s_tagButtonTemplate.Instantiate().Q<Button>();

            if ( tag.Comment != string.Empty ) {
                tagButton.tooltip = tag.Comment;
            }

            if ( !_isEditTaggerMode ) {
                if ( tagButton is Button button ) {
                    StyleButton( button, tag );
                    button.clicked += () => {
                        var window = EditorWindow.GetWindow<NeatoTagManager>();
                        window.ShowWindow( tag );
                    };
                }
            }
            else {
                var button = tagButton.Q<Button>( "tagButton" );
                var removeButton = tagButton.Q<Button>( "removeTagButton" );
                removeButton.tooltip = $"Remove {tag.name} tag from {target.name}";
                if ( targets.Length > 1 ) {
                    tagButton.tooltip = $"Click to add {tag.name} tag to all selected gameobjects.";
                }
                else {
                    if ( tag.Comment != string.Empty ) {
                        tagButton.tooltip = tag.Comment;
                    }
                }

                StyleButton( button, tag );
                button.clicked += () => {
                    if ( targets.Length > 1 ) {
                        GiveAllSelectedTag( tag );
                        if ( _searchField.value != string.Empty ) {
                            PopulateButtonsWithSearch();
                        }
                        else {
                            PopulateButtons();
                        }
                    }

                    UpdatePrefab();
                };

                removeButton.clicked += () => {
                    if ( !_isEditTaggerMode ) {
                        return;
                    }

                    Undo.RecordObject( target as Tagger, $"Removed Tag: {tag.name}" );

                    if ( targets.Length > 1 ) {
                        foreach ( var obj in targets ) {
                            ((Tagger)obj).RemoveTag( tag );
                        }
                    }
                    else {
                        ((Tagger)target).RemoveTag( tag );
                    }


                    if ( _searchField.value != string.Empty ) {
                        PopulateButtonsWithSearch();
                    }
                    else {
                        PopulateButtons();
                    }

                    UpdatePrefab();
                };
            }

            return tagButton;
        }

        void GiveAllSelectedTag( NeatoTag tag ) {
            foreach ( var obj in targets ) {
                var tagger = (Tagger)obj;
                tagger.AddTag( tag );
            }

            Repaint();
        }

        /// <summary>
        ///     Applies styling to a button element based on the properties of a provided tag.
        /// </summary>
        /// <param name="button">The button element to be styled.</param>
        /// <param name="tag">The tag providing information such as name and color for styling the button.</param>
        void StyleButton( Button button, NeatoTag tag ) {
            if ( targets.Length > 1 ) {
                var occurrences = 0;
                foreach ( var tagger in targets ) {
                    var otherTagger = (Tagger)tagger;
                    if ( otherTagger.HasTag( tag ) ) {
                        occurrences++;
                    }
                }

                if ( occurrences == targets.Length ) {
                    button.text = tag.name;
                    button.style.backgroundColor = tag.Color;
                    button.style.color = GetColorLuminosity( tag.Color ) > 70 ? Color.black : Color.white;
                }
                else {
                    button.style.backgroundColor = tag.Color;
                    button.style.color = GetColorLuminosity( tag.Color ) > 70 ? Color.black : Color.white;
                    button.style.unityFontStyleAndWeight = FontStyle.Italic;
                    button.text = tag.name + $"({occurrences})";
                }
            }
            else {
                button.text = tag.name;
                button.style.backgroundColor = tag.Color;
                button.style.color = GetColorLuminosity( tag.Color ) > 70 ? Color.black : Color.white;
            }
        }

        /// <summary>
        ///     Applies styling to a button element based on the properties of a provided tag.
        ///     This is for styling-available tags.
        /// </summary>
        /// <param name="button">The button element to be styled.</param>
        /// <param name="tag">The tag associated with the button, containing properties such as name and color.</param>
        void StyleButtonAvailable( Button button, NeatoTag tag ) {
            if ( targets.Length > 1 ) {
                var occurrences = 0;
                foreach ( var tagger in targets ) {
                    var otherTagger = (Tagger)tagger;
                    if ( otherTagger.HasTag( tag ) ) {
                        occurrences++;
                    }
                }

                if ( occurrences > 0 && occurrences < targets.Length ) {
                    button.style.unityFontStyleAndWeight = FontStyle.Italic;
                    button.style.backgroundColor = tag.Color;
                    button.style.color = GetColorLuminosity( tag.Color ) > 70 ? Color.black : Color.white;
                    button.text = tag.name + $"({occurrences})";
                }
                else {
                    button.text = tag.name;
                    button.style.backgroundColor = tag.Color;
                    button.style.color = GetColorLuminosity( tag.Color ) > 70 ? Color.black : Color.white;
                }
            }
            else {
                button.text = tag.name;
                button.style.backgroundColor = tag.Color;
                button.style.color = GetColorLuminosity( tag.Color ) > 70 ? Color.black : Color.white;
            }
        }

        /// <summary>
        ///     Populates the UI with buttons representing tags, categorizing them into selected or deselected groups.
        ///     This method clears existing button groups, retrieves a list of all tags, and dynamically creates
        ///     buttons for tags based on their selection state in the associated Tagger object.
        /// </summary>
        public void PopulateButtons() {
            _tagViewerDeselected.Clear();
            _tagViewerSelected.Clear();
            var allTags = TagSearchService.GetOrderedTags();
            var taggerTarget = (Tagger)target;
            var (selectedTags, availableTags) = TagSearchService.FilterTagsByTaggerState(
                allTags,
                taggerTarget
            );
            foreach ( var tag in selectedTags ) {
                _tagViewerSelected.Add( CreateSelectedButton( tag ) );
            }

            foreach ( var tag in availableTags ) {
                _tagViewerDeselected.Add( CreateDeselectedButton( tag ) );
            }
        }


        void PopulateButtonsWithSearch() {
            _tagViewerDeselected.Clear();
            _tagViewerSelected.Clear();

            var allTags = TagSearchService.GetOrderedTags();
            var taggerTarget = (Tagger)target;

            var (selectedTags, availableTags) = TagSearchService.FilterTagsByTaggerState(
                allTags,
                taggerTarget,
                _searchField.value,
                _taggerSearchAvailable.value
            );

            foreach ( var tag in selectedTags ) {
                _tagViewerSelected.Add( CreateSelectedButton( tag ) );
            }

            foreach ( var tag in availableTags ) {
                _tagViewerDeselected.Add( CreateDeselectedButton( tag ) );
            }
        }

        void UpdatePrefab() {
            EditorUtility.SetDirty( target );
            PrefabUtility.RecordPrefabInstancePropertyModifications( target );
        }

        /// <summary>
        ///     Calculates the luminosity of a given color based on its RGB values.
        /// </summary>
        /// <param name="color">The color whose luminosity is to be calculated.</param>
        /// <returns>A float value representing the perceived brightness (luminosity) of the color, ranging from 0 to 100.</returns>
        public static float GetColorLuminosity( Color color ) =>
            (0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b) * 100f;

        public static Color GetTextColorBasedOnBackground( Color backgroundColor ) {
            var luminosity = GetColorLuminosity( backgroundColor );
            var dataContainer = TagAssetCreation.GetEditorDataContainer();
            var threshold = 70;
            if ( dataContainer ) {
                threshold = dataContainer.LuminosityThreshold;
            }

            return luminosity > threshold ? Color.black : Color.white;
        }
    }
}