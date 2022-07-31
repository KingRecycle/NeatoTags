using System.IO;
using System.Linq;
using System.Reflection;
using CharlieMadeAThing.NeatoTags.Core;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Editor {
    public class TagAssetCreation : EditorWindow {
        public static string TagFolderPath = string.Empty;
        
        [MenuItem( "Neato Tags/Set Tag Folder Location" )]
        static void Apply()
        {
            var path = EditorUtility.OpenFolderPanel("Tag Folder Location", "", "");
            TagFolderPath = path;
            var holdersGuids = AssetDatabase.FindAssets( "t:EditorDataHolder" );
            if ( holdersGuids.Length > 1 ) {
                Debug.LogWarning($"[TagAssetCreation]: Found more than one EditorDataHolder. This is not supported. Only the first one will be used.");
            } else if ( holdersGuids.Length == 1 ) {
                var holderPath = AssetDatabase.GUIDToAssetPath( holdersGuids[0] );
                var dataHolder = AssetDatabase.LoadAssetAtPath<EditorDataHolder>( holderPath );
                dataHolder.tagFolderLocation = path;
            } else {
                // var newDataHolder = CreateInstance<EditorDataHolder>();
                // AssetDatabase.CreateAsset(newDataHolder, path + "/EditorDataHolder.asset");
                // AssetDatabase.SaveAssets();
                // AssetDatabase.Refresh();
                GetCharlieMadeAThingDirectory();
            }
        }
        
        static void GetCharlieMadeAThingDirectory()
        {
            
        }
        
        [MenuItem( "Neato Tags/New Tag" )]
        static void NewTag()
        {
            if( TagFolderPath == string.Empty )
            {
                if ( TryGetActiveFolderPath( out var path ) ) {
                    var newTag = CreateInstance<NeatoTagAsset>();
                    ProjectWindowUtil.CreateAsset(newTag, path + "/New Tag.asset");
                    // AssetDatabase.SaveAssets();
                    // AssetDatabase.Refresh();
                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = newTag;
                }
                else {
                    EditorUtility.DisplayDialog("Error", "Please set the tag folder location first or have a project folder selected.", "OK");
                    return;
                }
            } else {
                var newTag = CreateInstance<NeatoTagAsset>();
                ProjectWindowUtil.CreateAsset(newTag, TagFolderPath + "/New Tag.asset");
                // AssetDatabase.SaveAssets();
                // AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = newTag;
            }

        }

        public static void CreateNewTag( string tagName ) {
            var allTags = Tagger.GetAllTags();
            if( allTags.Any( x => x.name == tagName ) )
            {
                tagName = tagName + " " + allTags.Count( x => x.name == tagName );
            }
            if ( tagName == string.Empty ) {
                tagName = "New Tag";
            }
            if( TagFolderPath == string.Empty )
            {
                if ( TryGetActiveFolderPath( out var path ) ) {
                    var newTag = CreateInstance<NeatoTagAsset>();
                    AssetDatabase.CreateAsset(newTag, $"{path}/{tagName}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = newTag;
                }
                else {
                    EditorUtility.DisplayDialog("Error", "Please set the tag folder location first or have a project folder selected.", "OK");
                    return;
                }
            } else {
                var newTag = CreateInstance<NeatoTagAsset>();
                AssetDatabase.CreateAsset(newTag, $"{TagFolderPath}/{tagName}.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = newTag;
            }
        }
        
        public static bool TryGetActiveFolderPath( out string path )
        {
            var _tryGetActiveFolderPath = typeof(ProjectWindowUtil).GetMethod( "TryGetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic );

            object[] args = new object[] { null };
            bool found = (bool)_tryGetActiveFolderPath.Invoke( null, args );
            path = (string)args[0];

            return found;
        }
    }
}