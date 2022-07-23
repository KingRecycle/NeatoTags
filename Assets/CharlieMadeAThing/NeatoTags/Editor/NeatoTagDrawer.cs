using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CharlieMadeAThing.NeatoTags.Editor {
    [CustomEditor(typeof(NeatoTagAsset))]
    public class NeatoTagDrawer : UnityEditor.Editor {
        SerializedProperty PropertyColor { get; set; }
        
        //UI
        VisualElement _root;
        ColorField _colorField;
        Button _button;
        void OnEnable() {
            _root = new VisualElement();
            
            // Load in UXML template and USS styles, then apply them to the root element.
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/CharlieMadeAThing/NeatoTags/Editor/NeatoTag.uxml");
            visualTree.CloneTree(_root);
        }

        public override VisualElement CreateInspectorGUI() {
            FindProperties();
            _colorField = _root.Q<ColorField>( "tagColor" );
            _colorField.BindProperty( PropertyColor );
            _colorField.RegisterValueChangedCallback( UpdateTagIconVisual );
            
            _button = _root.Q<Button>( "tagIcon" );
            _button.text = target.name;
            _button.style.backgroundColor = _colorField.value;
            
            return _root;
        }
        
        void FindProperties() {
            PropertyColor = serializedObject.FindProperty( "color" );
        }
        
        

        void UpdateTagIconVisual( ChangeEvent<Color> evt ) {
            _button.style.backgroundColor = evt.newValue;
            PropertyColor.colorValue = evt.newValue;
        }
    }
}