using System;
using System.Collections.Generic;
using CharlieMadeAThing.NeatoTags.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CharlieMadeAThing.NeatoTags.Editor {
    [CustomEditor(typeof(NeatoTagAsset))]
    public class NeatoTagDrawer : UnityEditor.Editor {
        SerializedProperty PropertyColor { get; set; }
        SerializedProperty PropertyComment { get; set; }
        
        //UI
        VisualElement _root;
        ColorField _colorField;
        TextField _commentField;
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
            
            _commentField = _root.Q<TextField>( "commentField" );
            _commentField.BindProperty( PropertyComment );
            
            
            return _root;
        }
        
        void FindProperties() {
            PropertyColor = serializedObject.FindProperty( "color" );
            PropertyComment = serializedObject.FindProperty("comment");
        }

        public override void OnInspectorGUI() {
            _button.text = target.name;
        }


        void UpdateTagIconVisual( ChangeEvent<Color> evt ) {
            PropertyColor.colorValue = evt.newValue;
            _button.style.backgroundColor = PropertyColor.colorValue;
            
            var L = (0.2126 * PropertyColor.colorValue.r + 0.7152 * PropertyColor.colorValue.g + 0.0722 * PropertyColor.colorValue.b) * 100f;
            _button.style.color = L > 70 ? Color.black : Color.white;
            foreach ( var taggerDrawer in _taggerDrawers ) {
                taggerDrawer.PopulateButtons();
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