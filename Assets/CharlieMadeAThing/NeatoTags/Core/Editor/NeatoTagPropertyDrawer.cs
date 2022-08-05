using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CharlieMadeAThing.NeatoTags.Core.Editor {
    [CustomPropertyDrawer( typeof( NeatoTagAsset ) )]
    public class NeatoTagPropertyDrawer : PropertyDrawer {
        static Texture2D _buttonTexture;
        Label _label;
        PropertyField _propertyField;
        VisualElement _root;
        Button _tagButton;
        VisualElement _labelButtonContainer;
        VisualTreeAsset _tagButtonTemplate;

        
#if  UNITY_2022_2_OR_NEWER
        
        public override VisualElement CreatePropertyGUI( SerializedProperty property ) {
            _root = new VisualElement {
                style = {
                    flexDirection = FlexDirection.RowReverse,
                    justifyContent = Justify.SpaceBetween
                }
            };

            _labelButtonContainer = new VisualElement();
            _labelButtonContainer.style.flexDirection = FlexDirection.Row;
            _labelButtonContainer.style.overflow = Overflow.Hidden;
            

            _tagButtonTemplate =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( UxmlDataLookup.ButtonTagUxml );


            _propertyField = new PropertyField( property ) {
                label = string.Empty,
                style = {
                    flexGrow = 1,
                    alignSelf = Align.FlexEnd
                }
            };
            _propertyField.RegisterValueChangeCallback( UpdateTagDisplay );


            _label = new Label( property.displayName ) {
                style = {
                    paddingBottom = 0,
                    paddingTop = 0,
                    justifyContent = Justify.FlexEnd
                }
            };

            
            _root.Add( _propertyField );
            
            _labelButtonContainer.Add( _label );
            _root.Add(_labelButtonContainer);
            return _root;
        }

        void UpdateTagDisplay( SerializedPropertyChangeEvent evt ) {
            var tag = evt.changedProperty.objectReferenceValue as NeatoTagAsset;
            _tagButton ??= _tagButtonTemplate.Instantiate().Q<Button>();
            if ( tag == null ) {
                _tagButton.style.display = DisplayStyle.None;
                _label.style.display = DisplayStyle.Flex;
            } else {
                _tagButton.tooltip = tag.Comment;
                _tagButton.text = tag.name;
                _tagButton.style.backgroundColor = tag.Color;
                _tagButton.style.marginBottom = 0;
                _tagButton.style.marginTop = 0;
                _tagButton.style.alignSelf = Align.FlexStart;
                _labelButtonContainer.Add( _tagButton );
                //_root.hierarchy.Insert( 1, _tagButton );
                _tagButton.style.display = DisplayStyle.Flex;
                _propertyField.style.maxWidth = 200;
                

                if ( _label.text.Contains( "Element" ) ) {
                    _label.style.display = DisplayStyle.None;
                }
            }

            evt.changedProperty.serializedObject.ApplyModifiedProperties();
        }
        
#else
    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
            EditorGUILayout.HelpBox( "Neato Tags requires Unity 2022.2+ to show NeatTag properties correctly.", MessageType.Warning );
            base.OnGUI( position, property, label );
        }
#endif
    }
}