using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Core.Editor {
    public class TagAssetCreation : EditorWindow {
        static HashSet<NeatoTag> _cachedTags;
        static Dictionary<string, NeatoTag> _tagNameLookup;
        static bool _tagCacheDirty = true;
        const int MaxNameCounter = 1000; //Max attempts to add a number to the end of the tag name. Arbitrary amount.
        const int MaxCachedTags = 1000; //Max number of tags to cache. Arbitrary amount. Don't tell me you need more than 1000!?

        public static void OpenFolderDialogAndSelectFolder() {
            var path = EditorUtility.OpenFolderPanel( "Tag Folder Location", "Assets", "" );
            var tagPath = GetTagFolderLocation();

            if ( string.IsNullOrEmpty( path ) ) return;
            
            // Validate the path is within the project
            var projectPath = Path.GetFullPath(Application.dataPath);
            var selectedPath = Path.GetFullPath(path);
    
            if (!selectedPath.StartsWith(projectPath, StringComparison.OrdinalIgnoreCase)) {
                EditorUtility.DisplayDialog("Error", 
                    "Selected folder must be within the Unity project.", "OK");
                return;
            }
            
            var selectedFolder = Path.Join( "Assets", Path.GetRelativePath( Application.dataPath, path ) );
            if (selectedFolder.Contains("..")) {
                EditorUtility.DisplayDialog("Error", 
                    "Invalid folder selection.", "OK");
                return;
            }

            if ( string.IsNullOrEmpty( tagPath ) ) {
                var newDataHolder = CreateEditorDataHolder();
                newDataHolder.tagFolderLocation = selectedFolder;
            }
            else {
                GetEditorDataContainer().tagFolderLocation = selectedFolder;
            }
        }

        static EditorDataHolder CreateEditorDataHolder() {
            var newDataHolder = CreateInstance<EditorDataHolder>();
            var newPath = Path.Join( GetNeatoTagsEditorDirectory(), "EditorDataContainer.asset" );
            AssetDatabase.CreateAsset( newDataHolder, newPath );
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return newDataHolder;
        }


        static string GetNeatoTagsDirectory() {
            try {
                var dirs = Directory.GetDirectories($"{Application.dataPath}", "NeatoTags", SearchOption.AllDirectories);
                if (dirs.Length == 0) {
                    Debug.LogError("[TagAssetCreation]: Could not find NeatoTags directory.");
                    return "";
                }
        
                var selectedDir = dirs[0];
                // Verify the directory still exists before using it
                if (!Directory.Exists(selectedDir)) {
                    Debug.LogWarning("[TagAssetCreation]: NeatoTags directory was removed during operation.");
                    return "";
                }
        
                return Path.Join("Assets", Path.GetRelativePath(Application.dataPath, selectedDir));
            }
            catch (DirectoryNotFoundException ex) {
                Debug.LogError($"[TagAssetCreation]: Directory access failed: {ex.Message}");
                return "";
            }
            catch (UnauthorizedAccessException ex) {
                Debug.LogError($"[TagAssetCreation]: Access denied: {ex.Message}");
                return "";
            }

        }

        static string GetNeatoTagsEditorDirectory() {
            try {
                var neatTagsDirectory = GetNeatoTagsDirectory();
                var dirs = Directory.GetDirectories( $"{neatTagsDirectory}", "Editor",
                    SearchOption.AllDirectories );
                if ( dirs.Length == 0 ) {
                    Debug.LogError( "[TagAssetCreation]: Could not find NeatoTags Editor directory." );
                    return "";
                }
            
                var selectedDir = dirs[0];
                if ( !Directory.Exists( selectedDir ) ) {
                    Debug.LogWarning( "[TagAssetCreation]: NeatoTags Editor directory was removed during operation." );
                    return "";
                }
            
                return Path.Join( "Assets", Path.GetRelativePath( Application.dataPath, dirs[0] ) );
            }
            catch ( DirectoryNotFoundException ex ) {
                Debug.LogError($"[TagAssetCreation]: Directory access failed: {ex.Message}");
                return "";
            }
            catch (UnauthorizedAccessException ex) {
                Debug.LogError($"[TagAssetCreation]: Access denied: {ex.Message}");
                return "";
            }
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

        public static EditorDataHolder GetEditorDataContainer() {
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
            if ( string.IsNullOrEmpty( tagName ) ) {
                tagName = "New Tag";
            }

            uint counter = 0;
            var uniqueName = tagName;
            while ( _tagNameLookup != null && _tagNameLookup.ContainsKey( uniqueName ) ) {
                counter++;
                uniqueName = $"{tagName} {counter}";
                if ( counter <= MaxNameCounter ) continue;
                Debug.LogError( "[TagAssetCreation]: Could not create tag. Hard limit reached. Try a different name." );
                return null;
            }

            var newTag = CreateInstance<NeatoTag>();
            var assetPath = GetTagAssetPath( uniqueName );
            AssetDatabase.CreateAsset( newTag, assetPath );
            AssetDatabase.SaveAssets();

            if ( _cachedTags != null && _tagNameLookup != null ) {
                _cachedTags.Add( newTag );
                _tagNameLookup[uniqueName] = newTag;
            }
            else {
                InvalidateTagCache();
            }

            if ( shouldFocusInProjectWindow ) {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = newTag;
            }

            return newTag;
        }

        static string GetTagAssetPath( string tagName ) {
            var dataHolder = GetEditorDataContainer();
            if ( dataHolder != null && !string.IsNullOrEmpty( dataHolder.tagFolderLocation ) ) {
                return AssetDatabase.GenerateUniqueAssetPath( $"{dataHolder.tagFolderLocation}/{tagName}.asset" );
            }

            if ( TryGetActiveFolderPath( out var path ) ) {
                return AssetDatabase.GenerateUniqueAssetPath( $"{path}/{tagName}.asset" );
            }

            EditorUtility.DisplayDialog( "Error",
                "Please set the tag folder location first or have a project folder selected.", "OK" );
            return null;
        }

        //Try and get the folder path that is selected.
        static bool TryGetActiveFolderPath( out string path ) {
            var tryGetActiveFolderPath = typeof(ProjectWindowUtil).GetMethod( "TryGetActiveFolderPath",
                BindingFlags.Static | BindingFlags.NonPublic );

            object[] args = { null };
            var found = tryGetActiveFolderPath != null && (bool)tryGetActiveFolderPath.Invoke( null, args );
            path = (string)args[0];

            return found;
        }

        public static void DeleteTag( NeatoTag selectedTag ) {
            if ( !selectedTag ) return;
            if ( _cachedTags != null && _tagNameLookup != null ) {
                _cachedTags.Remove( selectedTag );
                _tagNameLookup.Remove( selectedTag.name );
                AssetDatabase.DeleteAsset( AssetDatabase.GetAssetPath( selectedTag ) );
            }
            else {
                InvalidateTagCache();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void InvalidateTagCache() {
            _tagCacheDirty = true;
        }

        static void RefreshTagCache() {
            _cachedTags = new HashSet<NeatoTag>();
            _tagNameLookup = new Dictionary<string, NeatoTag>();

            var guids = AssetDatabase.FindAssets( "t:NeatoTag" );
            if ( guids.Length > MaxCachedTags ) {
                Debug.LogWarning(
                    $"[TagAssetCreation]: Found more than {MaxCachedTags} tags. Only the first {MaxCachedTags} will be used." );
                Array.Resize(ref guids, MaxCachedTags);

            }
            foreach ( var guid in guids ) {
                try {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var tagAsset = AssetDatabase.LoadAssetAtPath<NeatoTag>(path);

                    if ( !tagAsset ) continue;
                    _cachedTags.Add(tagAsset);
                    _tagNameLookup[tagAsset.name] = tagAsset;
                }
                catch (Exception ex) {
                    Debug.LogWarning($"[TagAssetCreation]: Failed to load tag {guid}: {ex.Message}");
                }

            }

            _tagCacheDirty = false;
        }

        /// <summary>
        ///     Gives back a Hashset of all tags in the project.
        /// </summary>
        /// <returns>Hashset of all tags in the project.</returns>
        public static HashSet<NeatoTag> GetAllTags() {
            if ( _tagCacheDirty || _cachedTags == null ) {
                RefreshTagCache();
            }

            return _cachedTags;
        }
    }
}