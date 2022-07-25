using System;
using System.Collections.Generic;
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
        static List<TaggerDrawer> _taggerDrawers = new();
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
            _colorField.showAlpha = true;
            PropertyColor.colorValue = Color.black;
            PropertyColor.colorValue = _colorField.value;
            
            _button = _root.Q<Button>( "tagIcon" );
            _button.text = target.name;
            _button.style.backgroundColor = PropertyColor.colorValue;
            
            
            return _root;
        }
        
        void FindProperties() {
            PropertyColor = serializedObject.FindProperty( "color" );
        }

        public override void OnInspectorGUI() {
            _button.text = target.name;
        }


        void UpdateTagIconVisual( ChangeEvent<Color> evt ) {
            PropertyColor.colorValue = evt.newValue;
            _button.style.backgroundColor = PropertyColor.colorValue;
            foreach ( var taggerDrawer in _taggerDrawers ) {
                taggerDrawer.UpdateButtonTagPosition( target as NeatoTagAsset );
            }
        }
        
        public static void RegisterTaggerDrawer( TaggerDrawer taggerDrawer ) {
            if( _taggerDrawers.Contains( taggerDrawer ) ) {
                return;
            }
            _taggerDrawers.Add(taggerDrawer);
        }
    }
    
}