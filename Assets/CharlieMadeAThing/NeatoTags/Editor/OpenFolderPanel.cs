using System.IO;
using System.Reflection;
using CharlieMadeAThing.NeatoTags.Core;
using UnityEditor;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Editor {
    public class OpenFolderPanel : EditorWindow {
        public static string TAG_FOLDER_PATH = string.Empty;
        
        [MenuItem( "Neato Tags/Set Tag Folder Location" )]
        static void Apply()
        {
            string path = EditorUtility.OpenFolderPanel("Tag Folder Location", "", "");
            TAG_FOLDER_PATH = path;
            Debug.Log(path);
            
        }
        
        [MenuItem( "Neato Tags/New Tag" )]
        static void NewTag()
        {
            if( TAG_FOLDER_PATH == string.Empty )
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
                ProjectWindowUtil.CreateAsset(newTag, TAG_FOLDER_PATH + "/New Tag.asset");
                // AssetDatabase.SaveAssets();
                // AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = newTag;
            }

        }
        
        static bool TryGetActiveFolderPath( out string path )
        {
            var _tryGetActiveFolderPath = typeof(ProjectWindowUtil).GetMethod( "TryGetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic );

            object[] args = new object[] { null };
            bool found = (bool)_tryGetActiveFolderPath.Invoke( null, args );
            path = (string)args[0];

            return found;
        }
    }
}