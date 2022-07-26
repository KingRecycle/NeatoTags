using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CharlieMadeAThing.NeatoTags.Editor {
    [CustomEditor( typeof( Tagger ) )]
    public class TaggerDrawer : UnityEditor.Editor {
        Dictionary<Button, NeatoTagAsset> buttonTagLookup = new();
        Dictionary<NeatoTagAsset, Button> tagButtonsLookup = new();
        ObjectField _objectField;

        //UI
        VisualElement _root;
        GroupBox _tagViewer;
        GroupBox _allTagViewer;

        SerializedProperty NeatoTagAssets { get; set; }
        SerializedProperty SelectedTags { get; set; }

        HashSet<NeatoTagAsset> _selectedTagsSet = new();
        HashSet<NeatoTagAsset> _allTagsSet = new();

        void OnEnable() {
            Debug.Log( "[TaggerDrawer]: OnEnable" );

            _root = new VisualElement();
            // Load in UXML template and USS styles, then apply them to the root element.
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/CharlieMadeAThing/NeatoTags/Editor/Tagger.uxml" );
            visualTree.CloneTree( _root );
            
            _tagViewer = _root.Q<GroupBox>( "tagViewer" );
            _allTagViewer = _root.Q<GroupBox>( "allTagViewer" );
            FindProperties();
            
            NeatoTagAssetModificationProcessor.RegisterTaggerDrawer( this );
            NeatoTagDrawer.RegisterTaggerDrawer( this );
            GetAllTagsAndCreateButtons();
        }

        public override VisualElement CreateInspectorGUI() {
            return _root;
        }

        //Loop through all tags in PropertyTagCollection and add a button for each one.
        public void GetAllTagsAndCreateButtons() {
            Debug.Log( "[TaggerDrawer]: GetAllTagsAndCreateButtons" );
            
            if ( NeatoTagAssets == null ) return;
            Debug.Log($"{NeatoTagAssets == null} : {NeatoTagAssets.arraySize}");
            for ( var j = 0; j < NeatoTagAssets.arraySize; j++ ) {
                var tag = NeatoTagAssets.GetArrayElementAtIndex( j ).objectReferenceValue as NeatoTagAsset;
                
                if ( tag == null ) continue;
                
                CreateTagButtonToAllViewer( tag );
                UpdateButtonTagPosition( tag );
            }
        }

        public void UpdateButtonTagPosition( NeatoTagAsset tag ) {
            if( tag == null && !tagButtonsLookup.ContainsKey( tag ) ) return;
            if ( TagIsSelected( tag ) ) {
                AddTagButtonToAllViewer( tagButtonsLookup[tag]);
                AddTagButtonToActiveTagViewer( tagButtonsLookup[tag] );
            } else {
                AddTagButtonToAllViewer( tagButtonsLookup[tag]);
            }
        }

        public void RefreshAllTagButtons() {
            var tagViewerButtons = _tagViewer.Children();
            foreach ( var btn in tagViewerButtons ) {
                AddTagButtonToAllViewer( (Button) btn );
            }

            foreach ( var neatoTagAsset in _allTagsSet ) {
                UpdateButtonTagPosition( neatoTagAsset );
            }
            
        }

        public void CreateTagButtonToAllViewer( NeatoTagAsset tag ) {
            Debug.Log( $"[TaggerDrawer]: CreateTagButtonToAllViewer {tag}" );
            if ( tag == null ) return;
            var button = new Button();
            button.text = tag.name;
            button.style.backgroundColor = Color.clear;
            button.clicked += () => {
                Debug.Log( "Button clicked: " + tag.name );
                if ( TagIsSelected( tag ) ) {
                    AddTagButtonToAllViewer( button );
                } else {
                    AddTagButtonToActiveTagViewer( button );
                }
            };
            if( !tagButtonsLookup.ContainsKey( tag )) {
                Debug.Log( "[TaggerDrawer]: CreateTagButtonToAllViewer: Adding tag button to lookup" );
                tagButtonsLookup.Add( tag, button );
                buttonTagLookup.Add( button, tag );
            }
            _allTagViewer.Add( button );
            Repaint();
        }

        void AddTagButtonToAllViewer( Button button ) {
            Debug.Log( "[TaggerDrawer]: AddTagButtonToAllViewer" );
            if( _allTagViewer.Contains( button ) ) return;
            var tag = buttonTagLookup[button];
            RemoveTag( tag );
            button.style.backgroundColor = Color.clear;
            _allTagViewer.Add( button );
            Repaint();
        }

        void AddTagButtonToActiveTagViewer( Button button ) {
            Debug.Log( "[TaggerDrawer]: AddTagButtonToActiveTagViewer" );
            if ( _tagViewer.Contains( button ) ) return;
            var tag = buttonTagLookup[button];
            AddTag( tag );
            button.style.backgroundColor = tag.color;
            var bgColor = button.style.backgroundColor;
            var L = (0.2126 * bgColor.value.r + 0.7152 * bgColor.value.g + 0.0722 * bgColor.value.b) * 100f;
            button.style.color = L > 70 ? Color.black : Color.white;
            
            _tagViewer.Add( button );
            Repaint();
        }

        public void UpdateAllTagViewer() {
            Debug.Log( "[TaggerDrawer]: UpdateAllTagViewer" );
            if ( NeatoTagAssets == null && NeatoTagAssets.arraySize == null ) return;
            Debug.Log( NeatoTagAssets.arraySize);
            for ( var j = 0; j < NeatoTagAssets.arraySize; j++ ) {
                var tag = NeatoTagAssets.GetArrayElementAtIndex( j ).objectReferenceValue as NeatoTagAsset;
                if( tag == null ) continue;
                if ( !TagIsSelected( tag ) ) {
                    var button = tagButtonsLookup[tag];
                    AddTagButtonToAllViewer( button );
                }
            }
            Repaint();
            
        }

        public void UpdateTagViewer() {
            foreach ( var neatoTagAsset in _allTagsSet ) {
                
            }
        }
        
        
        void AddTag( NeatoTagAsset tag ) {
            Debug.Log( "[TaggerDrawer]: AddTag" );
            var index = SelectedTags.arraySize;
            SelectedTags.InsertArrayElementAtIndex( index );
            SelectedTags.GetArrayElementAtIndex( index ).objectReferenceValue = tag;
            SelectedTags.serializedObject.ApplyModifiedProperties();
            _selectedTagsSet.Add( tag );
        }

        void RemoveTag( NeatoTagAsset tag ) {
            Debug.Log( "[TaggerDrawer]: RemoveTag" );
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
            Debug.Log( $"[TaggerDrawer]: KillTagButton {tag}. Does viewer contain button?: {_allTagViewer.Children().Contains( button )}" );
            if ( _allTagViewer.Children().Contains( button ) ) {
                Debug.Log( $"KILLING BUTTON VIA TAG {tag}" );
                _allTagViewer.Remove( button );
                buttonTagLookup.Remove( button );
                tagButtonsLookup.Remove( tag );
            } else if ( _tagViewer.Children().Contains( button ) ) {
                _tagViewer.Remove( button );
                buttonTagLookup.Remove( button );
                tagButtonsLookup.Remove( tag );
            }
            Repaint();
        }
        
        public void KillTagButton( Button button ) {
            
            Debug.Log( $"[TaggerDrawer]: KillTagButton {button}. Does viewer contain button?: {_allTagViewer.Children().Contains( button )}" );
            if ( _allTagViewer.Children().Contains( button ) ) {
                Debug.Log( $"KILLING BUTTON {button}" );
                _allTagViewer.Remove( button );
                buttonTagLookup.Remove( button );
                tagButtonsLookup.Remove( buttonTagLookup[button] );
            } else if ( _tagViewer.Children().Contains( button ) ) {
                _tagViewer.Remove( button );
                buttonTagLookup.Remove( button );
                tagButtonsLookup.Remove( buttonTagLookup[button] );
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

        void UpdateSets() {
            _allTagsSet.Clear();
            _selectedTagsSet.Clear();
            for ( var i = 0; i < NeatoTagAssets.arraySize; i++ ) {
                var t = NeatoTagAssets.GetArrayElementAtIndex( i ).objectReferenceValue;
                _allTagsSet.Add( t as NeatoTagAsset );
            }
            for ( var i = 0; i < SelectedTags.arraySize; i++ ) {
                var t = SelectedTags.GetArrayElementAtIndex( i ).objectReferenceValue;
                _selectedTagsSet.Add( t as NeatoTagAsset );
            }
        }

        public void CheckAndUpdateRemovedTags() {
            Debug.Log( "[TaggerDrawer]: CheckAndUpdateRemovedTags" );
            UpdateSets();
            var keysToDelete = new List<NeatoTagAsset>();
            foreach ( var tbPair in tagButtonsLookup ) {
                if( _allTagsSet.Contains( tbPair.Key ) ) continue;
                keysToDelete.Add( tbPair.Key );
            }

            foreach ( var neatoTagAsset in keysToDelete ) {
                Debug.Log($"KILLING BUTTON {neatoTagAsset}");
                KillTagButton( neatoTagAsset);
            }
            
            // var buttonsToDelete = new List<Button>();
            // foreach ( var neatoTagAsset in _allTagsSet ) {
            //     foreach ( var btPair in buttonTagLookup ) {
            //         if( btPair.Value.name == neatoTagAsset.name ) continue;
            //         buttonsToDelete.Add( btPair.Key );
            //     }
            // }
            //
            // foreach ( var button in buttonsToDelete ) {
            //     Debug.Log($"KILLING BUTTON {button.text}");
            //     KillTagButton( button );
            // }

        }
    }
}