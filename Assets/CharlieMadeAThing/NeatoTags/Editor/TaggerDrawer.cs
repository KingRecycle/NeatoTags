using System;
using System.Collections;
using System.Collections.Generic;
using CharlieMadeAThing.NeatoTags;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CharlieMadeAThing.NeatoTags.Editor {
    [CustomEditor( typeof( Tagger ) )]
    public class TaggerDrawer : UnityEditor.Editor {
        
        SerializedProperty NeatoTagAssets { get; set; }
        SerializedProperty SelectedTags { get; set; }
        
        //UI
        VisualElement _root;
        GroupBox _tagViewer;
        ObjectField _objectField;
        List<Button> _buttons = new();

        void OnEnable() {
            _root = new VisualElement();
            // Load in UXML template and USS styles, then apply them to the root element.
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/CharlieMadeAThing/NeatoTags/Editor/Tagger.uxml");
            visualTree.CloneTree(_root);
            FindProperties();

            _tagViewer = _root.Q<GroupBox>("tagViewer");

            GetAllTagsAndCreateButtons();
            
        }
        

        public override VisualElement CreateInspectorGUI() {
            GetAllTagsAndCreateButtons();
            return _root;
        }

        void OnValidate() {
            GetAllTagsAndCreateButtons();
        }


        //Loop through all tags in PropertyTagCollection and add a button for each one.
        public void GetAllTagsAndCreateButtons() {

            foreach ( var button in _buttons ) {
                _tagViewer.Remove( button );
            }
            _buttons.Clear();

            if ( NeatoTagAssets == null ) return;
            for ( var j = 0; j < NeatoTagAssets.arraySize; j++ ) {
                    var tag = NeatoTagAssets.GetArrayElementAtIndex( j ).objectReferenceValue as NeatoTagAsset;
                    if( tag == null ) continue;
                    var button = new Button();
                    button.text = tag.name;
                    button.style.backgroundColor = Color.clear;
                    button.clicked += () => {
                        Debug.Log( "Button clicked: " + tag.name );
                        if( TagIsSelected ( tag ) ) {
                            RemoveTag( tag );
                            button.style.backgroundColor = Color.clear;
                        } else {
                            AddTag( tag );
                            button.style.backgroundColor = tag.color;
                        }
                    };
                    
                    if( TagIsSelected( tag ) ) {
                        button.style.backgroundColor = tag.color;
                    }
                    
                    _tagViewer.Add( button );
                    _buttons.Add( button );
                    Repaint();
            }
        }

        void AddTag( NeatoTagAsset tag ) {
            var index = SelectedTags.arraySize;
            SelectedTags.InsertArrayElementAtIndex( index );
            SelectedTags.GetArrayElementAtIndex( index ).objectReferenceValue = tag;
            SelectedTags.serializedObject.ApplyModifiedProperties();
            FindProperties();
        }

        void RemoveTag( NeatoTagAsset tag ) {
            for ( var i = 0; i < SelectedTags.arraySize; i++ ) {
                if ( SelectedTags.GetArrayElementAtIndex( i ).objectReferenceValue == tag ) {
                    SelectedTags.DeleteArrayElementAtIndex( i );
                    break;
                }
            }
            SelectedTags.serializedObject.ApplyModifiedProperties();
            FindProperties();
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