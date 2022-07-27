using System.Collections.Generic;
using System.Linq;
using CharlieMadeAThing.NeatoTags.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CharlieMadeAThing.NeatoTags.Editor {
    [CustomEditor( typeof( Tagger ) )]
    public class TaggerDrawer : UnityEditor.Editor {
        GroupBox _tagViewerDeselected;

        //UI
        VisualElement _root;
        GroupBox _tagViewerSelected;

        void OnEnable() {
            _root = new VisualElement();
            // Load in UXML template and USS styles, then apply them to the root element.
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/CharlieMadeAThing/NeatoTags/Editor/Tagger.uxml" );
            visualTree.CloneTree( _root );

            _tagViewerSelected = _root.Q<GroupBox>( "tagViewer" );
            _tagViewerDeselected = _root.Q<GroupBox>( "allTagViewer" );

            NeatoTagAssetModificationProcessor.RegisterTaggerDrawer( this );
            NeatoTagDrawer.RegisterTaggerDrawer( this );
            PopulateButtons();
        }

        public override VisualElement CreateInspectorGUI() {
            return _root;
        }
        

        Button CreateDeselectedButton( NeatoTagAsset tag ) {
            var button = new Button();
            button.text = tag.name;
            button.style.backgroundColor = Color.clear;
            Color.RGBToHSV( tag.Color, out var h, out var s, out var v );
            button.style.backgroundColor = Color.HSVToRGB( h, s * 0.40f, v * 0.40f );
            button.clicked += () => {
                Undo.RecordObject( target as Tagger, $"Added Tag: {tag.name}" );
                ((Tagger) target).AddTag( tag );
                PopulateButtons();
            };

            return button;
        }

        Button CreateSelectedButton( NeatoTagAsset tag ) {
            var button = new Button();
            button.text = tag.name;
            button.style.backgroundColor = tag.Color;
            var bgColor = button.style.backgroundColor.value;
            
            button.style.color = GetColorLuminosity( bgColor ) > 70 ? Color.black : Color.white;
            button.clicked += () => {
                Undo.RecordObject( target as Tagger, $"Removed Tag: {tag.name}" );
                ((Tagger) target).RemoveTag( tag );
                PopulateButtons();
            };
            return button;
        }

        public void PopulateButtons() { 
            _tagViewerDeselected.Clear();
            _tagViewerSelected.Clear();
            var allTags = Tagger.GetAllTags();
            foreach ( var neatoTagAsset in allTags.Where( x => ((Tagger) target ).GetTags.Contains( x ) ) ) {
                _tagViewerSelected.Add( CreateSelectedButton( neatoTagAsset ));
            }
            
            foreach ( var neatoTagAsset in allTags.Where( x => !((Tagger) target ).GetTags.Contains( x ) ) ) {
                _tagViewerDeselected.Add( CreateDeselectedButton( neatoTagAsset ));
            }
        }
        
        public static float GetColorLuminosity( Color color ) {
            return (0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b) * 100f;
        }
    }
}