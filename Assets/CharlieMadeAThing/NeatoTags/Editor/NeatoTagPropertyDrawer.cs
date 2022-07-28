using CharlieMadeAThing.NeatoTags.Core;
using UnityEditor;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Editor {
    [CustomPropertyDrawer( typeof( NeatoTagAsset ) )]
    public class NeatoTagPropertyDrawer : PropertyDrawer {
        
        static Texture2D buttonTexture;
        public override void OnGUI( Rect position, SerializedProperty property,
            GUIContent label ) {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            var buttonPlaceRect = new Rect(position.x, position.y, 150, position.height);
            var objPlaceRect = new Rect( position.x + buttonPlaceRect.width, position.y, position.width - buttonPlaceRect.width, position.height );
            if ( !buttonTexture ) {
                buttonTexture = AssetDatabase.LoadAssetAtPath<Texture2D>( "Assets/CharlieMadeAThing/NeatoTags/button_unitystyle.png" );
            }

            var buttonStyle = new GUIStyle( GUI.skin.button ) {
                border = new RectOffset( 4, 4, 4, 4 ),
                normal = {
                    background = buttonTexture
                }
            };
            
            
            
            // Draw fields - pass GUIContent.none to each so they are drawn without labels

            //var obj = EditorGUI.ObjectField( amountRect, property.objectReferenceValue, typeof(NeatoTagAsset), false );
            var obj = EditorGUI.PropertyField( objPlaceRect, property, GUIContent.none );
            
                var oldColor = GUI.backgroundColor;
                var p = property.objectReferenceValue as NeatoTagAsset;
                if ( p != null ) {
                    var lum = TaggerDrawer.GetColorLuminosity( p.Color ) > 70 ? Color.black : Color.white;
                    buttonStyle.normal.textColor = lum;
                    GUI.backgroundColor = p.Color;
                    var btn = GUI.Button( buttonPlaceRect, p.name, buttonStyle);
                    GUI.backgroundColor = oldColor;
                }
                

                // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight( SerializedProperty property,
            GUIContent label ) {
            return base.GetPropertyHeight( property, label );
        }
    }
}