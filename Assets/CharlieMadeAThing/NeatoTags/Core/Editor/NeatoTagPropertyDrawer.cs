using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CharlieMadeAThing.NeatoTags.Core.Editor {
    [CustomPropertyDrawer( typeof(NeatoTag) )]
    public class NeatoTagPropertyDrawer : PropertyDrawer {
        static Texture2D _buttonTexture;
        Label _label;
        PropertyField _propertyField;
        VisualElement _root;
        Button _tagButton;
        VisualElement _labelButtonContainer;
        VisualTreeAsset _tagButtonTemplate;


#if UNITY_2022_2_OR_NEWER && !ODIN_INSPECTOR

        public override VisualElement CreatePropertyGUI( SerializedProperty property ) {
            _root = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.SpaceBetween,
                },
            };

            _labelButtonContainer = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    overflow = Overflow.Hidden,
                    flexGrow = 1,
                    maxWidth = Length.Percent( 50 ),
                    justifyContent = Justify.FlexEnd,
                },
            };


            _tagButtonTemplate =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( UxmlDataLookup.ButtonTagUxml );


            _propertyField = new PropertyField( property ) {
                label = string.Empty,
                style = {
                    flexGrow = 1,
                    alignSelf = Align.FlexEnd,
                    alignContent = Align.Auto,
                    maxWidth = Length.Percent( 65 ),
                    justifyContent = Justify.FlexEnd,
                },
            };
            _propertyField.RegisterValueChangeCallback( UpdateTagDisplay );


            _label = new Label( property.displayName ) {
                style = {
                    paddingBottom = 0,
                    paddingTop = 0,
                    marginRight = 0,
                    justifyContent = Justify.FlexEnd,
                    flexGrow = 0,
                    maxWidth = Length.Percent( 50 ),
                },
            };


            _root.Add( _label );

            _labelButtonContainer.Add( _propertyField );
            _root.Add( _labelButtonContainer );
            return _root;
        }

        void UpdateTagDisplay( SerializedPropertyChangeEvent evt ) {
            var tag = evt.changedProperty.objectReferenceValue as NeatoTag;
            if ( _labelButtonContainer.Contains( _tagButton ) ) {
                _labelButtonContainer.Remove( _tagButton );
            }

            _tagButton = _tagButtonTemplate.Instantiate().Q<Button>();
            if ( tag == null ) {
                _tagButton.style.display = DisplayStyle.None;
                _label.style.display = DisplayStyle.Flex;
            }
            else {
                //_labelButtonContainer.Add( _tagButton );
                _labelButtonContainer.Insert( 0, _tagButton );
                _tagButton.tooltip = tag.Comment;
                _tagButton.text = tag.name;
                _tagButton.style.backgroundColor = tag.Color;
                _tagButton.style.marginBottom = 0;
                _tagButton.style.marginTop = 0;
                _tagButton.style.textOverflow = TextOverflow.Ellipsis;
                _tagButton.style.maxWidth = 200;
                _tagButton.style.display = DisplayStyle.Flex;
                _labelButtonContainer.style.maxWidth = 650;
                _tagButton.style.color = TaggerDrawer.GetColorLuminosity( tag.Color ) > 70 ? Color.black : Color.white;

                // if ( _label.text.Contains( "Element" ) ) {
                //     _label.style.display = DisplayStyle.None;
                // }
            }

            evt.changedProperty.serializedObject.ApplyModifiedProperties();
        }

#else
    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty( position, label, property );

            // Draw label
            position = EditorGUI.PrefixLabel( position, GUIUtility.GetControlID( FocusType.Passive ), label );

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            var buttonPlaceRect = new Rect( position.x, position.y, 150, position.height );
            var objPlaceRect = new Rect( position.x + buttonPlaceRect.width, position.y,
                position.width - buttonPlaceRect.width, position.height );
            if ( !_buttonTexture ) {
                _buttonTexture =
                    AssetDatabase.LoadAssetAtPath<Texture2D>(
                        "Assets/CharlieMadeAThing/NeatoTags/Core/Sprites/button_unitystyle.png" );
            }

            var buttonStyle = new GUIStyle( GUI.skin.button ) {
                border = new RectOffset( 4, 4, 4, 4 ),
                normal = {
                    background = _buttonTexture
                }
            };


            // Draw fields - pass GUIContent.none to each so they are drawn without labels

            //var obj = EditorGUI.ObjectField( amountRect, property.objectReferenceValue, typeof(NeatoTag), false );
            EditorGUI.PropertyField( objPlaceRect, property, GUIContent.none );
            
            var oldColor = GUI.backgroundColor;
            var p = property.objectReferenceValue as NeatoTag;
            if ( p ) {
                var lum = TaggerDrawer.GetColorLuminosity( p.Color ) > 70 ? Color.black : Color.white;
                buttonStyle.normal.textColor = lum;
                GUI.backgroundColor = p.Color;
                GUI.Button( buttonPlaceRect, p.name, buttonStyle );
                GUI.backgroundColor = oldColor;
            }


            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
#endif
    }
}