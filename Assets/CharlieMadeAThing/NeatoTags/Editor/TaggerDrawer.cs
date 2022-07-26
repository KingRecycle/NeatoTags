using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CharlieMadeAThing.NeatoTags.Editor {
    [CustomEditor( typeof( Tagger ) )]
    public class TaggerDrawer : UnityEditor.Editor {
        readonly HashSet<NeatoTagAsset> _allTagsSet = new();
        GroupBox _tagViewerDeselected;
        ObjectField _objectField;

        //UI
        VisualElement _root;

        readonly HashSet<NeatoTagAsset> _selectedTagsSet = new();
        GroupBox _tagViewerSelected;
        readonly Dictionary<Button, NeatoTagAsset> buttonTagLookup = new();
        readonly Dictionary<NeatoTagAsset, Button> tagButtonsLookup = new();

        SerializedProperty NeatoTagAssets { get; set; }
        SerializedProperty SelectedTags { get; set; }

        void OnEnable() {
            _root = new VisualElement();
            // Load in UXML template and USS styles, then apply them to the root element.
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/CharlieMadeAThing/NeatoTags/Editor/Tagger.uxml" );
            visualTree.CloneTree( _root );

            _tagViewerSelected = _root.Q<GroupBox>( "tagViewer" );
            _tagViewerDeselected = _root.Q<GroupBox>( "allTagViewer" );
            FindProperties();

            NeatoTagAssetModificationProcessor.RegisterTaggerDrawer( this );
            NeatoTagDrawer.RegisterTaggerDrawer( this );
            GetAllTagsAndCreateButtons();
        }

        public override VisualElement CreateInspectorGUI() {
            return _root;
        }

        public void GetAllTagsAndCreateButtons() {
            if ( NeatoTagAssets == null ) return;
            for ( var j = 0; j < NeatoTagAssets.arraySize; j++ ) {
                var tag = NeatoTagAssets.GetArrayElementAtIndex( j ).objectReferenceValue as NeatoTagAsset;

                if ( tag == null ) continue;

                CreateTagButtonToAllViewer( tag );
                UpdateButtonTagPosition( tag );
            }
        }

        public void UpdateButtonTagPosition( NeatoTagAsset tag ) {
            if ( tag == null && !tagButtonsLookup.ContainsKey( tag ) ) return;
            if ( TagIsSelected( tag ) ) {
                AddTagButtonToAllViewer( tagButtonsLookup[tag] );
                AddTagButtonToActiveTagViewer( tagButtonsLookup[tag] );
            } else {
                AddTagButtonToAllViewer( tagButtonsLookup[tag] );
            }
        }

        Button CreateButton( NeatoTagAsset tag ) {
            var button = new Button();
            button.text = tag.name;
            button.style.backgroundColor = Color.clear;
            button.clicked += () => {
                if ( TagIsSelected( tag ) ) {
                    AddTagButtonToAllViewer( button );
                } else {
                    AddTagButtonToActiveTagViewer( button );
                }
            };

            return button;
        }

        void PopulateButtons() { 
            _tagViewerDeselected.Clear();
            _tagViewerSelected.Clear();
            var allTags = Tagger.GetAllTags();
            foreach ( var neatoTagAsset in allTags.Where( x => ((Tagger) target ).GetTags.Contains( x ) ) ) {
                _tagViewerSelected.Add( CreateButton( neatoTagAsset ));
            }
            
            foreach ( var neatoTagAsset in allTags.Where( x => !((Tagger) target ).GetTags.Contains( x ) ) ) {
                _tagViewerDeselected.Add( CreateButton( neatoTagAsset ));
            }
        }

        public void CreateTagButtonToAllViewer( NeatoTagAsset tag ) {
            if ( tag == null ) return;
            var button = new Button();
            button.text = tag.name;
            button.style.backgroundColor = Color.clear;
            button.clicked += () => {
                if ( TagIsSelected( tag ) ) {
                    AddTagButtonToAllViewer( button );
                } else {
                    AddTagButtonToActiveTagViewer( button );
                }
            };
            if ( !tagButtonsLookup.ContainsKey( tag ) ) {
                tagButtonsLookup.Add( tag, button );
                buttonTagLookup.Add( button, tag );
            }

            _tagViewerDeselected.Add( button );
            Repaint();
        }

        void AddTagButtonToAllViewer( Button button ) {
            if ( _tagViewerDeselected.Contains( button ) ) return;
            var tag = buttonTagLookup[button];
            RemoveTag( tag );
            button.style.backgroundColor = Color.clear;
            button.style.color = new StyleColor( Color.white );
            _tagViewerDeselected.Add( button );
            Repaint();
        }

        void AddTagButtonToActiveTagViewer( Button button ) {
            if ( _tagViewerSelected.Contains( button ) ) return;
            var tag = buttonTagLookup[button];
            AddTag( tag );
            button.style.backgroundColor = tag.color;
            var bgColor = button.style.backgroundColor;
            var lum = ( 0.2126 * bgColor.value.r + 0.7152 * bgColor.value.g + 0.0722 * bgColor.value.b ) * 100f;
            button.style.color = lum > 70 ? Color.black : Color.white;

            _tagViewerSelected.Add( button );
            Repaint();
        }

        void AddTag( NeatoTagAsset tag ) {
            FindProperties();
            var index = SelectedTags.arraySize;
            SelectedTags.InsertArrayElementAtIndex( index );
            SelectedTags.GetArrayElementAtIndex( index ).objectReferenceValue = tag;
            SelectedTags.serializedObject.ApplyModifiedProperties();
            _selectedTagsSet.Add( tag );
        }

        void RemoveTag( NeatoTagAsset tag ) {
            FindProperties();
            for ( var i = 0; i < SelectedTags.arraySize; i++ ) {
                if ( SelectedTags.GetArrayElementAtIndex( i ).objectReferenceValue == tag ) {
                    SelectedTags.DeleteArrayElementAtIndex( i );
                    _selectedTagsSet.Remove( tag );
                    break;
                }
            }

            SelectedTags.serializedObject.ApplyModifiedProperties();
        }

        bool TagIsSelected( NeatoTagAsset tag ) {
            if ( _selectedTagsSet.Contains( tag ) ) {
                return true;
            }

            return false;
        }

        public void KillTagButton( NeatoTagAsset tag ) {
            var button = tagButtonsLookup[tag];
            if ( _tagViewerDeselected.Children().Contains( button ) ) {
                _tagViewerDeselected.Remove( button );
            } else if ( _tagViewerSelected.Children().Contains( button ) ) {
                _tagViewerSelected.Remove( button );
            }

            RemoveFromLookups( tag );
            Repaint();
        }

        public void KillTagButtonByName( string tagName ) {
            Button buttonToKill = null;
            foreach ( var visualElement in _tagViewerDeselected.Children() ) {
                var button = (Button) visualElement;
                if ( button.text == tagName ) {
                    buttonToKill = button;
                    break;
                }
            }

            if ( buttonToKill != null ) {
                _tagViewerDeselected.Remove( buttonToKill );
                buttonTagLookup.Remove( buttonToKill );
                tagButtonsLookup.Remove( buttonTagLookup[buttonToKill] );
            } else {
                foreach ( var visualElement in _tagViewerSelected.Children() ) {
                    var button = (Button) visualElement;
                    if ( button.text == tagName ) {
                        buttonToKill = button;
                        break;
                    }
                }
            }
            
            if( buttonToKill != null ) {
                _tagViewerSelected.Remove( buttonToKill );
                buttonTagLookup.Remove( buttonToKill );
                tagButtonsLookup.Remove( buttonTagLookup[buttonToKill] );
            }
            
            Repaint();
        }


        void FindProperties() {
            NeatoTagAssets = serializedObject.FindProperty( "_allTags" );
            for ( var i = 0; i < NeatoTagAssets.arraySize; i++ ) {
                var t = NeatoTagAssets.GetArrayElementAtIndex( i ).objectReferenceValue;
                _allTagsSet.Add( t as NeatoTagAsset );
            }

            SelectedTags = serializedObject.FindProperty( "tags" );
            for ( var i = 0; i < SelectedTags.arraySize; i++ ) {
                var t = SelectedTags.GetArrayElementAtIndex( i ).objectReferenceValue;
                _selectedTagsSet.Add( t as NeatoTagAsset );
            }
        }

        void RemoveFromLookups( NeatoTagAsset tag ) {
            var button = tagButtonsLookup[tag];
            tagButtonsLookup.Remove( tag );
            buttonTagLookup.Remove( button );
        }


        public void CheckAndUpdateRemovedTags() {
            var keysToDelete = new List<NeatoTagAsset>();
            foreach ( var tbPair in tagButtonsLookup ) {
                if ( _allTagsSet.Contains( tbPair.Key ) ) continue;
                keysToDelete.Add( tbPair.Key );
            }

            foreach ( var neatoTagAsset in keysToDelete ) {
                KillTagButton( neatoTagAsset );
            }
        }
    }
}