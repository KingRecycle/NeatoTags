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
        }

        public override VisualElement CreateInspectorGUI() {
            FindProperties();

            _tagViewer = _root.Q<GroupBox>("tagViewer");
            _objectField = _root.Q<ObjectField>("collectionField");
            _objectField.objectType = typeof(NeatoTagCollection);
            _objectField.value = PropertyTagCollection.objectReferenceValue;
            _objectField.RegisterValueChangedCallback( OnCollectionAdded );
            
            GetAllTagsAndCreateButtons();
            
            return _root;
        }

        void OnCollectionAdded( ChangeEvent<Object> evt ) {
            var collection = evt.newValue as NeatoTagCollection;
            PropertyTagCollection.objectReferenceValue = evt.newValue;
            Debug.Log("Collection added: " + collection.name);
        }


        //Loop through all tags in PropertyTagCollection and add a button for each one.
        void GetAllTagsAndCreateButtons() {
            var collectionList = NeatoTagCollection.tags;
            for ( var i = 0; i < collectionList.Count; i++ ) {
                var btn = new Button();
                btn.text = collectionList[i].name;
                btn.style.backgroundColor = collectionList[i].color;
                _tagViewer.Add( btn );
            }

        }

        void FindProperties() {
            PropertyTagCollection = serializedObject.FindProperty( "tagCollection" );
            NeatoTagCollection = PropertyTagCollection.objectReferenceValue as NeatoTagCollection;
            
        }
    }
}