using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using File = System.IO.File;

namespace CharlieMadeAThing.GameplayTags.UI.Editor {
    public class TagsEditor : EditorWindow {
        [SerializeField] VisualTreeAsset tree;
        List<string> _tagStrings = new();
        HashSet<TreeNode<string>> _tags = new();

        [MenuItem("Tools/Gameplay Tags Window")]
        public static void ShowTagEditor() {
            var window = GetWindow<TagsEditor>("Gameplay Tags");
        }

        void CreateGUI() {
            
            _tagStrings.Add( "Vehicle" );
            _tagStrings.Add("Vehicle.Car"  );
            _tagStrings.Add("Vehicle.Helicopter"  );
            _tagStrings.Add("Vehicle.Helicopter.Flying"  );
            _tagStrings.Add( "Vehicle.Car.Drive" );
            _tagStrings.Add( "Magic.Arcane.Child2.Child3.Child4" );
            _tagStrings.Add( "Magic.Arcane.Neat" );
            _tagStrings.Add( "Magic.Fire" );
            CreateTagTree( _tagStrings );
            
            
            
            
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( "Assets/GameplayTags/UI/Editor/tageditor.uxml" );
            visualTree.CloneTree( rootVisualElement );

            foreach ( var treeNode in _tags ) {
                TraverseAndAddToElement( treeNode, rootVisualElement.Q<GroupBox>("treebox" ) );
            }
            
            GetNodesFromYAML();

        }

        static void TraverseAndAddToElement( TreeNode<string> node, VisualElement element ) {
            
            if ( node.IsParent || node.IsRoot ) {
                var foldout = new Foldout {
                    text = node.Data
                };
                
                if ( node.IsRoot ) {
                    foldout.style.borderBottomWidth = new StyleFloat( 2 );
                    foldout.style.borderBottomColor = new StyleColor( Color.gray );
                }

                if ( !node.IsRoot && node.Parent.IsRoot ) {
                    foldout.style.paddingLeft = new StyleLength( 10 );
                }

                element.Add( foldout );

                for ( var i = 0; i < node.Count; i++ ) {
                    var currentChild = node[i];
                    TraverseAndAddToElement( currentChild, foldout );
                }
            } else {
                var label = new Label();
                label.text = node.Data;
                
                element.Add( label );
            }

            

        }

        void CreateTagTree( List<string> tagList ) {
            foreach ( var tagString in tagList ) {
                var splitted = tagString.Split( "." );
                var rootWord = splitted[0];
                if ( !TryGetRootNodeByData( rootWord, out var rootNode ) ) {
                    rootNode = CreateRootTreeNodeTag( rootWord );
                }

                var currentNode = rootNode;
                for ( var i = 1; i < splitted.Length; i++ ) {
                    Debug.Log( currentNode );
                    if ( currentNode.HasChild( splitted[i] ) ) {
                        currentNode = currentNode.FindInChildren( splitted[i] );
                    } else {
                        currentNode = currentNode.AddChild( splitted[i] );
                    }
                }
                
            }
        }

        TreeNode<string> CreateRootTreeNodeTag( string rootWord ) {
            var rootNode = new TreeNode<string>( rootWord );
            _tags.Add( rootNode );
            return rootNode;
        }

        bool TryGetRootNodeByData( string value, out TreeNode<string> treeNode ) {
            foreach ( var node in _tags.Where( node => node.Data == value ) ) {
                treeNode = node;
                return true;
            }

            treeNode = null;
            return false;
        }

        void GetNodesFromYAML() {
            var serializer = new SerializerBuilder()
                .WithNamingConvention( CamelCaseNamingConvention.Instance )
                .Build();
            
            
            var t = AssetDatabase.LoadAssetAtPath<TextAsset>( "Assets/GameplayTags/TagData/TagsList.txt" );
            var yaml = serializer.Serialize( _tags );
            File.WriteAllText( "Assets/GameplayTags/TagData/TagsList.txt", yaml );
        }
        
    }

    class TagData {
        public string Tag;
        public string Comment;
    }
}
