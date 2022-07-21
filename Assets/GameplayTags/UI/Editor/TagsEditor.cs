using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CharlieMadeAThing.GameplayTags.UI.Editor {
    public class TagsEditor : EditorWindow {
        [SerializeField] VisualTreeAsset tree;
        List<string> _tagStrings = new();
        List<GameplayTag> _gameplayTags = new();
        Dictionary<string, Foldout> _foldouts = new();
        List<TreeNode<string>> _tags = new();

        [MenuItem("Tools/Gameplay Tags Window")]
        public static void ShowTagEditor() {
            var window = GetWindow<TagsEditor>("Gameplay Tags");
        }

        void CreateGUI() {
            _gameplayTags.Clear();
            _foldouts.Clear();
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

            foreach ( var gameplayTag in _gameplayTags ) {
                if ( gameplayTag.isBaseParent ) {
                    
                }
            }
            
            foreach ( var gameplayTag in _gameplayTags ) {
                if ( gameplayTag.isParent ) {
                    
                }
                var label = new Label();
                label.text = gameplayTag.value;
                Debug.Log(_foldouts);
                _foldouts[gameplayTag.parent.value].Add( label );
            }
            

            foreach ( var foldout in _foldouts ) {
                rootVisualElement.Q<GroupBox>( "LeftBox" ).Add( foldout.Value );
            }

        }

        void CreateTagTree( List<string> tagList ) {
            foreach ( var tagString in tagList ) {
                var splitted = tagString.Split( "." );
                
                for ( var i = 0; i < splitted.Length; i++ ) {
                    Debug.Log( $"Current Tag: {splitted[i]}" );
                    if ( TryGetGameplayTag( splitted[i], out var tag ) ) { //If Exist check if has parent and update parent
                        Debug.Log( $"{tag.value} -- {tag.isParent}" );
                        if ( i > 0 && TryGetGameplayTag( splitted[i - 1], out var parentTag ) ) {
                            parentTag.isParent = true;
                            parentTag.child.Add(tag);
                            tag.parent = parentTag;
                        }
                    } else { //If not exist set up and add to gameplayTags
                        var gt = new GameplayTag();
                        gt.value = splitted[i];
                        if ( i == 0 ) {
                            gt.isBaseParent = true;
                            gt.isParent = true;
                            var fo = new Foldout();
                            fo.text = gt.value;
                            gt.foldOut = fo;
                        } else if ( i > 0 && i < splitted.Length ) {
                            gt.isBaseParent = false;
                            gt.isParent = true;
                            var fo = new Foldout();
                            fo.text = gt.value;
                            gt.foldOut = fo;
                        } else {
                            gt.isBaseParent = false;
                            gt.isParent = false;
                            var lab = new Label();
                            lab.text = gt.value;
                            gt.label = lab;
                        }
                        
                        Debug.Log("ADDING: " + gt.value);
                        _gameplayTags.Add( gt );
                    }
                }
            }

            foreach ( var gameplayTag in _gameplayTags ) {
                Debug.Log($"{gameplayTag.value}"  );
            }
            
        }

        bool TryGetGameplayTag( string tagName, out GameplayTag gameplayTag ) {
            Debug.Log($"Gameplay Tags Length: {_gameplayTags.Count}"  );
            var gt = _gameplayTags.Find( tag => tag.value == tagName );
            Debug.Log($"Does {tagName} exist?: {gt != null}");
            if ( gt != null ) {
                gameplayTag = gt;
                return true;
            }

            gameplayTag = null;
            return false;
        }
    }

    class GameplayTag {
        public string value;
        public bool isParent;
        public bool isBaseParent;
        public GameplayTag parent;
        public List<GameplayTag> child = new();
        public Foldout foldOut;
        public Label label;
    }
}
