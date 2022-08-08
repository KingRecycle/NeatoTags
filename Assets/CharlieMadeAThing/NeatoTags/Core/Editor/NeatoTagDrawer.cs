using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CharlieMadeAThing.NeatoTags.Core.Editor {
    [CustomEditor( typeof( NeatoTag ) )]
    public class NeatoTagDrawer : UnityEditor.Editor {
        static readonly List<TaggerDrawer> TAGGER_DRAWERS = new();
        Button _button;
        ColorField _colorField;
        TextField _commentField;
        NeatoTag _neatoTag;

        //UI
        VisualElement _root;
        VisualElement _tagButtonBox;
        VisualTreeAsset _tagButtonTemplate;
        SerializedProperty PropertyColor { get; set; }
        SerializedProperty PropertyComment { get; set; }


        void OnEnable() {
            _root = new VisualElement();

            // Load in UXML template and USS styles, then apply them to the root element.
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( UxmlDataLookup.NeatoTagUxml );
            visualTree.CloneTree( _root );
            _tagButtonBox = _root.Q<VisualElement>( "tagButtonBox" );
            _tagButtonTemplate =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( UxmlDataLookup.ButtonTagUxml );
            NeatoTagAssetModificationProcessor.RegisterNeatoTagDrawer( this );
        }

        public override VisualElement CreateInspectorGUI() {
            _neatoTag = target as NeatoTag;
            FindProperties();
            _colorField = _root.Q<ColorField>( "tagColor" );
            _colorField.BindProperty( PropertyColor );
            _colorField.RegisterValueChangedCallback( UpdateTagIconVisual );
            _colorField.showAlpha = false;
            PropertyColor.colorValue = Color.gray;
            PropertyColor.colorValue = _colorField.value;

            _button = _tagButtonTemplate.Instantiate().Q<Button>();
            _button.text = target.name;
            _button.style.backgroundColor = PropertyColor.colorValue;
            _button.name = "tagIcon";
            _tagButtonBox.Add( _button );

            _commentField = _root.Q<TextField>( "commentField" );
            _commentField.BindProperty( PropertyComment );

            return _root;
        }


        void FindProperties() {
            PropertyColor = serializedObject.FindProperty( "color" );
            PropertyComment = serializedObject.FindProperty( "comment" );
        }

        public void UpdateTagButtonText() {
            if ( target != null && _button != null ) {
                _button.text = _neatoTag.name;
            }
        }


        void UpdateTagIconVisual( ChangeEvent<Color> evt ) {
            PropertyColor.colorValue = evt.newValue;
            _button.style.backgroundColor = PropertyColor.colorValue;
            _button.style.color = TaggerDrawer.GetColorLuminosity( PropertyColor.colorValue ) > 70
                ? Color.black
                : Color.white;
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