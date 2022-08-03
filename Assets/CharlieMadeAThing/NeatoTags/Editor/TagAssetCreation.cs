using System.IO;
using System.Linq;
using System.Reflection;
using CharlieMadeAThing.NeatoTags.Core;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Editor {
    public class TagAssetCreation : EditorWindow {
        [MenuItem( "Tools/Neato Tags/Set Tag Folder Location" )]
        static void SetTagFolderLocation()
        {
            var path = EditorUtility.OpenFolderPanel("Tag Folder Location", "Assets", "");
            var tagPath = GetTagFolderLocation();
            //Get the path to the Assets folder
            if( string.IsNullOrEmpty( path ) ) {
                return;
            }
            var selectedFolder = "Assets\\" + Path.GetRelativePath( Application.dataPath, path );
            if ( string.IsNullOrEmpty( tagPath ) ) {
                var neatTagsDirectory = GetNeatoTagsDirectory();
                var newDataHolder = CreateInstance<EditorDataHolder>();
                newDataHolder.tagFolderLocation = selectedFolder;
                AssetDatabase.CreateAsset(newDataHolder, neatTagsDirectory + "/Editor/EditorDataContainer.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            } else {
                GetEditorDataContainer().tagFolderLocation = selectedFolder;
            }
        }

        public static void SetTagFolder() {
            var path = EditorUtility.OpenFolderPanel("Tag Folder Location", "Assets", "");
            var tagPath = GetTagFolderLocation();
            //Get the path to the Assets folder
            if( string.IsNullOrEmpty( path ) ) {
                return;
            }
            var selectedFolder = "Assets\\" + Path.GetRelativePath( Application.dataPath, path );
            Debug.Log( selectedFolder );
            if ( string.IsNullOrEmpty( tagPath ) ) {
                var neatTagsDirectory = GetNeatoTagsDirectory();
                var newDataHolder = CreateInstance<EditorDataHolder>();
                newDataHolder.tagFolderLocation = selectedFolder;
                AssetDatabase.CreateAsset(newDataHolder, neatTagsDirectory + "\\Editor\\EditorDataContainer.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            } else {
                GetEditorDataContainer().tagFolderLocation = selectedFolder;
            }
        }
        
        static string GetNeatoTagsDirectory() {
            var dirs = Directory.GetDirectories( $"{Application.dataPath}", "NeatoTags", SearchOption.AllDirectories );
            var path = "Assets\\" + Path.GetRelativePath( Application.dataPath, dirs[0] );
            if ( dirs.Length != 0 ) return path;
            Debug.LogError("[TagAssetCreation]: Could not find NeatoTags directory.");
            return "";
        }
        
        static string GetTagFolderLocation() {
            var dataHolder = GetEditorDataContainer();
            return dataHolder == null ? "" : dataHolder.tagFolderLocation;
        }

        static EditorDataHolder GetEditorDataContainer() {
            var holdersGuids = AssetDatabase.FindAssets( "EditorDataContainer" );
            
            if ( holdersGuids.Length > 1 ) {
                Debug.LogWarning($"[TagAssetCreation]: Found more than one EditorDataHolder. This is not supported. Only the first one will be used.");
                var holderPath = AssetDatabase.GUIDToAssetPath( holdersGuids[0] );
                return AssetDatabase.LoadAssetAtPath<EditorDataHolder>( holderPath );
            }

            if ( holdersGuids.Length == 1 ) {
                var holderPath = AssetDatabase.GUIDToAssetPath( holdersGuids[0] );
                return AssetDatabase.LoadAssetAtPath<EditorDataHolder>( holderPath );
            }

            return null;
        }
        
        [MenuItem( "Tools/Neato Tags/New Tag", priority = 0)]
        static void NewTag()
        {
            var dataHolder = GetEditorDataContainer();
            if( dataHolder == null || string.IsNullOrEmpty( dataHolder.tagFolderLocation ) )
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
                }
            } else {
                var newTag = CreateInstance<NeatoTagAsset>();
                Debug.Log( dataHolder.tagFolderLocation );
                ProjectWindowUtil.CreateAsset(newTag, dataHolder.tagFolderLocation + "/New Tag.asset");
                // AssetDatabase.SaveAssets();
                // AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = newTag;
            }

        }

        //Non menu version of NewTag function
        public static NeatoTagAsset CreateNewTag( string tagName, bool shouldFocusInProjectWindow = true ) {
            var allTags = Tagger.GetAllTags();
            if( allTags.Any( x => x.name == tagName ) )
            {
                tagName = tagName + " " + allTags.Count( x => x.name == tagName );
            }
            if ( tagName == string.Empty ) {
                tagName = "New Tag";
            }

            NeatoTagAsset newTag = null;
            var dataHolder = GetEditorDataContainer();
            if( dataHolder == null || string.IsNullOrEmpty( dataHolder.tagFolderLocation ) )
            {
                if ( TryGetActiveFolderPath( out var path ) ) {
                    newTag = CreateInstance<NeatoTagAsset>();
                    var newPath = AssetDatabase.GenerateUniqueAssetPath( $"{path}/{tagName}.asset" );
                    AssetDatabase.CreateAsset(newTag, newPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    if ( shouldFocusInProjectWindow ) {
                        EditorUtility.FocusProjectWindow();
                        Selection.activeObject = newTag;
                    }
                }
                else {
                    EditorUtility.DisplayDialog("Error", "Please set the tag folder location first or have a project folder selected.", "OK");
                }
            } else {
                newTag = CreateInstance<NeatoTagAsset>();
                AssetDatabase.CreateAsset(newTag, $"{dataHolder.tagFolderLocation}/{tagName}.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                if ( shouldFocusInProjectWindow ) {
                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = newTag;
                }
            }

            return newTag;
        }
        
        //Try and get the folder path that is selected.
        public static bool TryGetActiveFolderPath( out string path )
        {
            var _tryGetActiveFolderPath = typeof(ProjectWindowUtil).GetMethod( "TryGetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic );

            object[] args = new object[] { null };
            bool found = (bool)_tryGetActiveFolderPath.Invoke( null, args );
            path = (string)args[0];

            return found;
        }

        public static void DeleteTag( NeatoTagAsset selectedTag ) {
            if ( !selectedTag ) return;
            AssetDatabase.DeleteAsset( AssetDatabase.GetAssetPath( selectedTag ) );
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}