using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CharlieMadeAThing.NeatoTags.Core.Editor {
    [CustomEditor( typeof(Tagger) ), CanEditMultipleObjects]
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

        //Tagger (not the drawer)
        SerializedObject _tagger;
        ToolbarSearchField _taggerSearchAvailable;
        GroupBox _tagViewerDeselected;
        GroupBox _tagViewerSelected;

        Tagger Target => target as Tagger;

        void OnEnable() {
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
            _searchField.tooltip =
                $"Search through tags currently tagged to {target.name}. Use ^ at the beginning to search for exact matches.";
            _searchLabel = _root.Q<Label>( "searchLabel" );

            _taggerSearchAvailable = _root.Q<ToolbarSearchField>( "taggerSearchAvailable" );
            _taggerSearchAvailable.RegisterValueChangedCallback( _ => { PopulateButtonsWithSearch(); } );
            _taggerSearchAvailable.tooltip =
                $"Search through tags currently available to add to {target.name}. Use ^ at the beginning to search for exact matches.";

            _addTagButton = _root.Q<Button>( "addTagButton" );
            _addTagButton.tooltip = $"Create a new tag and add it to {target.name}";
            _addTagButton.clicked -= CreateNewTag;
            _addTagButton.clicked += CreateNewTag;
            _addTagTextField = _root.Q<TextField>( "addTagTextField" );
            _addTagTextField.RegisterCallback<KeyDownEvent>( evt => {
                if ( evt.keyCode == KeyCode.Return ) {
                    CreateNewTag();
                }
            } );

            _allTagsBox = _root.Q<GroupBox>( "allTagsBox" );

            _editTaggerButton = _root.Q<ToolbarButton>( "editTaggerButton" );
            _editTaggerButton.tooltip = $"Edit Tags for {target.name}";


            _searchField.style.display = DisplayStyle.None;
            _addTagButton.style.display = DisplayStyle.None;
            _addTagTextField.style.display = DisplayStyle.None;
            _searchLabel.style.display = DisplayStyle.None;
            _allTagsBox.style.display = DisplayStyle.None;
            _editTaggerButton.clicked -= ShowEditTagger;
            _editTaggerButton.clicked += ShowEditTagger;


            NeatoTagAssetModificationProcessor.RegisterTaggerDrawer( this );
            NeatoTagDrawer.RegisterTaggerDrawer( this );
            PopulateButtons();
            Target.WantRepaint += DoRepaint;
            _tagger = new SerializedObject( Target );
        }

        void OnDisable() {
            Target.WantRepaint -= DoRepaint;
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
                ( (Tagger)target ).AddTag( tag );
            }

            UpdatePrefab();
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

            StyleButtonAvailable( button, tag );
            button.clicked += () => {
                Undo.RecordObject( target as Tagger, $"Added Tag: {tag.name}" );
                if ( targets.Length > 1 ) {
                    foreach ( var obj in targets ) {
                        ( (Tagger)obj ).AddTag( tag );
                    }
                }
                else {
                    ( (Tagger)target ).AddTag( tag );
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
                ? _tagButtonWithXTemplate.Instantiate().Q<VisualElement>()
                : _tagButtonTemplate.Instantiate().Q<Button>();

            if ( tag.Comment != string.Empty ) {
                tagButton.tooltip = tag.Comment;
            }

            if ( !_isEditTaggerMode ) {
                if ( tagButton is Button button ) {
                    StyleButton( button, tag );
                    button.clicked += () => {
                        var window = EditorWindow.GetWindow<NeatoTagManager>();
                        window.ShowWindow(tag); 
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
                            ( (Tagger)obj ).RemoveTag( tag );
                        }
                    }
                    else {
                        ( (Tagger)target ).RemoveTag( tag );
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

        public void PopulateButtons() {
            _tagViewerDeselected.Clear();
            _tagViewerSelected.Clear();
            var allTags = TagAssetCreation.GetAllTags().ToList().OrderBy( tag => tag.name );

            var taggerTarget = (Tagger)target;
            foreach ( var neatoTagAsset in allTags ) {
                if ( taggerTarget.GetTags != null && taggerTarget.GetTags.Contains( neatoTagAsset ) ) {
                    _tagViewerSelected.Add( CreateSelectedButton( neatoTagAsset ) );
                }
                else {
                    _tagViewerDeselected.Add( CreateDeselectedButton( neatoTagAsset ) );
                }
            }
        }


        void PopulateButtonsWithSearch() {
            _tagViewerDeselected.Clear();
            _tagViewerSelected.Clear();

            var allTags = TagAssetCreation.GetAllTags().ToList().OrderBy( tag => tag.name );
            var taggerTarget = (Tagger)target;

            foreach ( var neatoTagAsset in allTags ) {
                //Search for already selected tags
                if ( taggerTarget.GetTags != null && taggerTarget.GetTags.Contains( neatoTagAsset ) ) {
                    if ( !string.IsNullOrWhiteSpace( _searchField.value ) ) {
                        if ( Regex.IsMatch( neatoTagAsset.name, $"{_searchField.value}", RegexOptions.IgnoreCase ) ) {
                            _tagViewerSelected.Add( CreateSelectedButton( neatoTagAsset ) );
                        }
                    }
                    else {
                        _tagViewerSelected.Add( CreateSelectedButton( neatoTagAsset ) );
                    }
                }
                else {
                    //Search for available tags
                    if ( !string.IsNullOrWhiteSpace( _taggerSearchAvailable.value ) ) {
                        if ( Regex.IsMatch( neatoTagAsset.name, $"{_taggerSearchAvailable.value}",
                                RegexOptions.IgnoreCase ) ) {
                            _tagViewerDeselected.Add( CreateDeselectedButton( neatoTagAsset ) );
                        }
                    }
                    else {
                        _tagViewerDeselected.Add( CreateDeselectedButton( neatoTagAsset ) );
                    }
                }
            }
        }

        void UpdatePrefab() {
            EditorUtility.SetDirty( target );
            PrefabUtility.RecordPrefabInstancePropertyModifications( target );
        }

        public static float GetColorLuminosity( Color color ) {
            return ( 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b ) * 100f;
        }

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