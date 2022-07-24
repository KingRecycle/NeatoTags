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
            GetAllTagsAndCreateButtons();
        }

        void OnValidate() {
            
        }

        public override VisualElement CreateInspectorGUI() {
            return _root;
        }

        //Loop through all tags in PropertyTagCollection and add a button for each one.
        public void GetAllTagsAndCreateButtons() {
            Debug.Log( "[TaggerDrawer]: GetAllTagsAndCreateButtons" );
            
            if ( NeatoTagAssets == null ) return;
            Debug.Log($"{NeatoTagAssets == null} : {NeatoTagAssets.arraySize}");
            RemoveDuplicateTags();
            for ( var j = 0; j < NeatoTagAssets.arraySize; j++ ) {
                var tag = NeatoTagAssets.GetArrayElementAtIndex( j ).objectReferenceValue as NeatoTagAsset;
                
                if ( tag == null ) continue;
                
                CreateTagButtonToAllViewer( tag );
                UpdateButtonTagPosition( tag );
            }
        }

        void UpdateButtonTagPosition( NeatoTagAsset tag) {
            if ( TagIsSelected( tag ) ) {
                AddTagButtonToActiveTagViewer( tagButtonsLookup[tag] );
            } else {
                AddTagButtonToAllViewer( tagButtonsLookup[tag]);
            }
        }

        void CreateTagButtonToAllViewer( NeatoTagAsset tag ) {
            Debug.Log( "[TaggerDrawer]: CreateTagButtonToAllViewer" );
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
            if( _tagViewer.Contains( button ) ) return;
            var tag = buttonTagLookup[button];
            AddTag( tag );
            button.style.backgroundColor = tag.color;
            _tagViewer.Add( button );
            Repaint();
        }

        public void UpdateAllTagViewer() {
            Debug.Log( "[TaggerDrawer]: UpdateAllTagViewer" );
            if ( NeatoTagAssets == null ) return;
            RemoveDuplicateTags();
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

        void RemoveDuplicateTags() {
            Debug.Log( "[TaggerDrawer]: RemoveDuplicateTags" );
            var deDupe = new HashSet<NeatoTagAsset>();
            for ( var i = 0; i < SelectedTags.arraySize; i++ ) {
                deDupe.Add( SelectedTags.GetArrayElementAtIndex( i ).objectReferenceValue as NeatoTagAsset );
                SelectedTags.DeleteArrayElementAtIndex( i );
            }

            foreach ( var neatoTagAsset in deDupe ) {
                SelectedTags.InsertArrayElementAtIndex( SelectedTags.arraySize );
                SelectedTags.GetArrayElementAtIndex( SelectedTags.arraySize - 1 ).objectReferenceValue = neatoTagAsset;
            }

            SelectedTags.serializedObject.ApplyModifiedProperties();
        }

        public void RemoveIfContains( VisualElement parent, VisualElement e)
        {
            if (parent.Contains(e))
                parent.Remove(e);
        }
        
        void AddTag( NeatoTagAsset tag ) {
            Debug.Log( "[TaggerDrawer]: AddTag" );
            var index = SelectedTags.arraySize;
            SelectedTags.InsertArrayElementAtIndex( index );
            SelectedTags.GetArrayElementAtIndex( index ).objectReferenceValue = tag;
            SelectedTags.serializedObject.ApplyModifiedProperties();
        }

        void RemoveTag( NeatoTagAsset tag ) {
            Debug.Log( "[TaggerDrawer]: RemoveTag" );
            for ( var i = 0; i < SelectedTags.arraySize; i++ ) {
                if ( SelectedTags.GetArrayElementAtIndex( i ).objectReferenceValue == tag ) {
                    SelectedTags.DeleteArrayElementAtIndex( i );
                    break;
                }
            }

            SelectedTags.serializedObject.ApplyModifiedProperties();
        }

        bool TagIsSelected( NeatoTagAsset tag ) {
            for ( var i = 0; i < SelectedTags.arraySize; i++ ) {
                if ( SelectedTags.GetArrayElementAtIndex( i ).objectReferenceValue == tag ) {
                    return true;
                }
            }
            return false;
        }

        void FindProperties() {
            NeatoTagAssets = serializedObject.FindProperty( "_allTags" );
            SelectedTags = serializedObject.FindProperty( "tags" );
        }
    }
}