using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Core.Editor {
    public class TagAssetCreation : EditorWindow {
        public static void SetTagFolder() {
            var path = EditorUtility.OpenFolderPanel( "Tag Folder Location", "Assets", "" );
            var tagPath = GetTagFolderLocation();

            if ( string.IsNullOrEmpty( path ) ) {
                return;
            }

            var selectedFolder = Path.Join( "Assets", Path.GetRelativePath( Application.dataPath, path ) );
            if ( string.IsNullOrEmpty( tagPath ) ) {
                var newDataHolder = CreateInstance<EditorDataHolder>();
                newDataHolder.tagFolderLocation = selectedFolder;
                var newPath = Path.Join( GetNeatoTagsEditorDirectory(), "EditorDataContainer.asset" );
                AssetDatabase.CreateAsset( newDataHolder, newPath );
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            } else {
                GetEditorDataContainer().tagFolderLocation = selectedFolder;
            }
        }


        static string GetNeatoTagsDirectory() {
            var dirs = Directory.GetDirectories( $"{Application.dataPath}", "NeatoTags", SearchOption.AllDirectories );
            var path = Path.Join( "Assets", Path.GetRelativePath( Application.dataPath, dirs[0] ) );
            if ( dirs.Length != 0 ) return path;
            Debug.LogError( "[TagAssetCreation]: Could not find NeatoTags directory." );
            return "";
        }

        static string GetNeatoTagsEditorDirectory() {
            var neatTagsDirectory = GetNeatoTagsDirectory();
            var dirs = Directory.GetDirectories( $"{neatTagsDirectory}", "Editor",
                SearchOption.AllDirectories );
            var path = Path.Join( "Assets", Path.GetRelativePath( Application.dataPath, dirs[0] ) );
            if ( dirs.Length != 0 ) return path;
            Debug.LogError( "[TagAssetCreation]: Could not find NeatoTags Editor directory." );
            return "";
        }

        public static string GetUxmlDirectory() {
            var neatTagsDirectory = GetNeatoTagsDirectory();
            var dirs = Directory.GetDirectories( $"{neatTagsDirectory}", "UXML",
                SearchOption.AllDirectories );
            var path = Path.Join( "Assets", Path.GetRelativePath( Application.dataPath, dirs[0] ) );
            if ( dirs.Length != 0 ) return path;
            Debug.LogError( "[TagAssetCreation]: Could not find NeatoTags UXML directory." );
            return "";
        }

        public static string GetTagFolderLocation() {
            var dataHolder = GetEditorDataContainer();
            return dataHolder == null ? "" : dataHolder.tagFolderLocation;
        }

        static EditorDataHolder GetEditorDataContainer() {
            var holdersGuids = AssetDatabase.FindAssets( "EditorDataContainer" );

            switch ( holdersGuids.Length ) {
                case > 1: {
                    Debug.LogWarning(
                        "[TagAssetCreation]: Found more than one EditorDataHolder. This is not supported. Only the first one will be used." );
                    var holderPath = AssetDatabase.GUIDToAssetPath( holdersGuids[0] );
                    return AssetDatabase.LoadAssetAtPath<EditorDataHolder>( holderPath );
                }
                case 1: {
                    var holderPath = AssetDatabase.GUIDToAssetPath( holdersGuids[0] );
                    return AssetDatabase.LoadAssetAtPath<EditorDataHolder>( holderPath );
                }
                default:
                    return null;
            }
        }

        //Non menu version of NewTag function
        public static NeatoTag CreateNewTag( string tagName, bool shouldFocusInProjectWindow = true ) {
            var allTags = GetAllTags();
            if ( allTags.Any( x => x.name == tagName ) ) {
                tagName = tagName + " " + allTags.Count( x => x.name == tagName );
            }

            if ( tagName == string.Empty ) {
                tagName = "New Tag";
            }

            NeatoTag newTag = null;
            var dataHolder = GetEditorDataContainer();
            if ( dataHolder == null || string.IsNullOrEmpty( dataHolder.tagFolderLocation ) ) {
                if ( TryGetActiveFolderPath( out var path ) ) {
                    newTag = CreateInstance<NeatoTag>();
                    var newPath = AssetDatabase.GenerateUniqueAssetPath( $"{path}/{tagName}.asset" );
                    AssetDatabase.CreateAsset( newTag, newPath );
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    if ( shouldFocusInProjectWindow ) {
                        EditorUtility.FocusProjectWindow();
                        Selection.activeObject = newTag;
                    }
                } else {
                    EditorUtility.DisplayDialog( "Error",
                        "Please set the tag folder location first or have a project folder selected.", "OK" );
                }
            } else {
                newTag = CreateInstance<NeatoTag>();
                AssetDatabase.CreateAsset( newTag, $"{dataHolder.tagFolderLocation}/{tagName}.asset" );
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
        static bool TryGetActiveFolderPath( out string path ) {
            var tryGetActiveFolderPath = typeof( ProjectWindowUtil ).GetMethod( "TryGetActiveFolderPath",
                BindingFlags.Static | BindingFlags.NonPublic );

            object[] args = { null };
            var found = tryGetActiveFolderPath != null && (bool) tryGetActiveFolderPath.Invoke( null, args );
            path = (string) args[0];

            return found;
        }

        public static void DeleteTag( NeatoTag selectedTag ) {
            if ( !selectedTag ) return;
            AssetDatabase.DeleteAsset( AssetDatabase.GetAssetPath( selectedTag ) );
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Gives back a Hashset of all tags in the project.
        /// </summary>
        /// <returns>Hashset of all tags in the project.</returns>
        public static HashSet<NeatoTag> GetAllTags() {
            var tagSet = new HashSet<NeatoTag>();
            var guids = AssetDatabase.FindAssets( "t:NeatoTag" );
            foreach ( var guid in guids ) {
                var path = AssetDatabase.GUIDToAssetPath( guid );
                var tagAsset = AssetDatabase.LoadAssetAtPath<NeatoTag>( path );
                tagSet.Add( tagAsset );
            }

            return tagSet;
        }
    }
}