using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CharlieMadeAThing.NeatoTags.Editor {
    [CustomEditor( typeof( Tagger ) )]
    public class TaggerDrawer : UnityEditor.Editor {
        readonly List<Button> _activeButtons = new();
        readonly List<Button> _unactiveButtons = new();
        ObjectField _objectField;

        //UI
        VisualElement _root;
        GroupBox _tagViewer;
        GroupBox _allTagViewer;

        SerializedProperty NeatoTagAssets { get; set; }
        SerializedProperty SelectedTags { get; set; }

        void OnEnable() {
            _root = new VisualElement();
            // Load in UXML template and USS styles, then apply them to the root element.
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/CharlieMadeAThing/NeatoTags/Editor/Tagger.uxml" );
            visualTree.CloneTree( _root );
            FindProperties();

            _tagViewer = _root.Q<GroupBox>( "tagViewer" );
            _allTagViewer = _root.Q<GroupBox>( "allTagViewer" );
            
            GetAllTagsAndCreateButtons();
        }

        void OnValidate() {
            GetAllTagsAndCreateButtons();
        }

        public override VisualElement CreateInspectorGUI() {
            return _root;
        }


        //Loop through all tags in PropertyTagCollection and add a button for each one.
        public void GetAllTagsAndCreateButtons() {
            
            if ( NeatoTagAssets == null ) return;
            for ( var j = 0; j < NeatoTagAssets.arraySize; j++ ) {
                var tag = NeatoTagAssets.GetArrayElementAtIndex( j ).objectReferenceValue as NeatoTagAsset;
                if ( tag == null ) continue;
                var button = new Button();
                button.text = tag.name;
                button.style.backgroundColor = Color.clear;
                button.clicked += () => {
                    Debug.Log( "Button clicked: " + tag.name );
                    if ( TagIsSelected( tag ) ) {
                        RemoveTag( tag );
                        button.style.backgroundColor = Color.clear;
                        //_tagViewer.Remove( button );
                        _activeButtons.Remove( button );
                        
                        _allTagViewer.Add( button );
                        _unactiveButtons.Add( button );
                    } else {
                        AddTag( tag );
                        button.style.backgroundColor = tag.color;
                        _tagViewer.Add( button );
                        _activeButtons.Add( button );
                        //_allTagViewer.Remove( button );
                        _unactiveButtons.Remove( button );
                    }
                };
                
                if ( !TagIsSelected( tag ) ) {
                    RemoveTag( tag );
                    button.style.backgroundColor = Color.clear;
                    RemoveIfContains( _tagViewer, button );
                    _activeButtons.Remove( button );
                    _allTagViewer.Add( button );
                    _unactiveButtons.Add( button );
                } else {
                    AddTag( tag );
                    button.style.backgroundColor = tag.color;
                    _tagViewer.Add( button );
                    _activeButtons.Add( button );
                    RemoveIfContains( _allTagViewer, button );
                    _unactiveButtons.Remove( button );
                }
                RemoveDuplicateTags();
                Repaint();
            }
        }

        void RemoveDuplicateTags() {
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
            var index = SelectedTags.arraySize;
            SelectedTags.InsertArrayElementAtIndex( index );
            SelectedTags.GetArrayElementAtIndex( index ).objectReferenceValue = tag;
            SelectedTags.serializedObject.ApplyModifiedProperties();
        }

        void RemoveTag( NeatoTagAsset tag ) {
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