using CharlieMadeAThing.NeatoTags.Core;
using UnityEditor;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Editor {
    [CustomPropertyDrawer( typeof( NeatoTagAsset ) )]
    public class NeatoTagPropertyDrawer : PropertyDrawer {
        
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
            var amountRect = new Rect(position.x, position.y, 150, position.height);
            var buttonPlace = new Rect( position.x + amountRect.width, position.y, position.width - amountRect.width, position.height );

            // Draw fields - pass GUIContent.none to each so they are drawn without labels

            //var obj = EditorGUI.ObjectField( amountRect, property.objectReferenceValue, typeof(NeatoTagAsset), false );
            var obj = EditorGUI.PropertyField( amountRect, property, GUIContent.none );
            
                var oldColor = GUI.backgroundColor;
                var p = property.objectReferenceValue as NeatoTagAsset;
                if ( p != null ) {
                    GUI.backgroundColor = p.Color;
                    var btn = GUI.Button( buttonPlace, p.name );
                    
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