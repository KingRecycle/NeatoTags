using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

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
            
            _tagStrings.Add( "Tag1" );
            _tagStrings.Add("Tag2.ChildTag1"  );
            _tagStrings.Add("Tag2.ChildTag2"  );
            _tagStrings.Add("Tag2.ChildTag2.ChildTag3"  );
            _tagStrings.Add( "Tag3" );
            _tagStrings.Add( "Tag3.Child1.Child2.Child3.Child4" );
            _tagStrings.Add( "Tag4" );
            CreateTagTree( _tagStrings );
            
            
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( "Assets/GameplayTags/UI/tageditor.uxml" );
            visualTree.CloneTree( rootVisualElement );

            foreach ( var treeNode in _tags ) {
                TraverseAndAddToElement( treeNode, rootVisualElement.Q<GroupBox>("treebox" ) );
            }
            
            // foreach ( var foldout in _foldouts ) {
            //     rootVisualElement.Q<GroupBox>( "LeftBox" ).Add( foldout.Value );
            // }

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
                Debug.Log($"{element} : {foldout}"  );
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
            
            //rootVisualElement.Q<GroupBox>( "LeftBox" ).Add( fo );

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
        
        bool DoesRootNodeExist( string value ) {
            return _tags.Any( node => node.Data == value );
        }
        
    }

    class VisualNode {
        Foldout Foldout;
        Label Label;
        bool isFoldout;
    }
}
