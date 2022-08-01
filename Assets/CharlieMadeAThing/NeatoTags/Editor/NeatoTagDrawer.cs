using System;
using System.Collections.Generic;
using CharlieMadeAThing.NeatoTags.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CharlieMadeAThing.NeatoTags.Editor {
    [CustomEditor( typeof( NeatoTagAsset ) )]
    public class NeatoTagDrawer : UnityEditor.Editor {
        static readonly List<TaggerDrawer> TAGGER_DRAWERS = new();
        Button _button;
        ColorField _colorField;
        TextField _commentField;

        //UI
        VisualElement _root;
        SerializedProperty PropertyColor { get; set; }
        SerializedProperty PropertyComment { get; set; }
        

        void OnEnable() {
            _root = new VisualElement();

            // Load in UXML template and USS styles, then apply them to the root element.
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/CharlieMadeAThing/NeatoTags/Editor/NeatoTag.uxml" );
            visualTree.CloneTree( _root );
            NeatoTagAssetModificationProcessor.RegisterNeatoTagDrawer( this );
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
            PropertyComment = serializedObject.FindProperty( "comment" );
        }

        public override void OnInspectorGUI() {
            if ( target != null ) {
                _button.text = target.name;
            }
        }


        void UpdateTagIconVisual( ChangeEvent<Color> evt ) {
            PropertyColor.colorValue = evt.newValue;
            _button.style.backgroundColor = PropertyColor.colorValue;
            _button.style.color = TaggerDrawer.GetColorLuminosity( PropertyColor.colorValue ) > 70 ? Color.black : Color.white;
            foreach ( var taggerDrawer in TAGGER_DRAWERS ) {
                taggerDrawer.PopulateButtons();
            }
        }

        public static void RegisterTaggerDrawer( TaggerDrawer taggerDrawer ) {
            if ( TAGGER_DRAWERS.Contains( taggerDrawer ) ) {
                return;
            }

            TAGGER_DRAWERS.Add( taggerDrawer );
        }
    }
}