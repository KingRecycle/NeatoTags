using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CharlieMadeAThing.NeatoTags.Editor {
    [CustomEditor( typeof( Tagger ) )]
    public class TaggerDrawer : UnityEditor.Editor {
        SerializedProperty PropertyTagCollection { get; set; }
        NeatoTagCollection NeatoTagCollection { get; set; }
        
        //UI
        VisualElement _root;
        GroupBox _tagViewer;
        ObjectField _objectField;
        List<Button> _buttons = new();

        void OnEnable() {
            _root = new VisualElement();
            // Load in UXML template and USS styles, then apply them to the root element.
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/NeatoTags/Editor/Tagger.uxml");
            visualTree.CloneTree(_root);
            FindProperties();

            _tagViewer = _root.Q<GroupBox>("tagViewer");
            _objectField = _root.Q<ObjectField>("collectionField");
            _objectField.objectType = typeof(NeatoTagCollection);
            _objectField.value = PropertyTagCollection.objectReferenceValue;
            _objectField.RegisterValueChangedCallback( OnCollectionAdded );
            GetAllTagsAndCreateButtons();
        }

        public override VisualElement CreateInspectorGUI() {
            
            
            return _root;
        }

        public override void OnInspectorGUI() {
            _objectField.value = PropertyTagCollection.objectReferenceValue;
        }

        void OnCollectionAdded( ChangeEvent<Object> evt ) {
            NeatoTagCollection = evt.newValue as NeatoTagCollection;
            PropertyTagCollection.objectReferenceValue = NeatoTagCollection;
            PropertyTagCollection.serializedObject.ApplyModifiedProperties();
            GetAllTagsAndCreateButtons();
        }


        //Loop through all tags in PropertyTagCollection and add a button for each one.
        void GetAllTagsAndCreateButtons() {
            foreach ( var button in _buttons ) {
                _tagViewer.Remove( button );
            }
            _buttons.Clear();
            var collectionList = NeatoTagCollection.tags;
            
            for ( var i = 0; i < collectionList.Count; i++ ) {
                var btn = new Button();
                btn.text = collectionList[i].name;
                btn.style.backgroundColor = collectionList[i].color;
                _tagViewer.Add( btn );
                _buttons.Add( btn );
                
            }

        }

        void FindProperties() {
            PropertyTagCollection = serializedObject.FindProperty( "tagCollection" );
            NeatoTagCollection = PropertyTagCollection.objectReferenceValue as NeatoTagCollection;
            
        }
    }
}