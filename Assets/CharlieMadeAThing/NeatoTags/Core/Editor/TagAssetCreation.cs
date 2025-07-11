﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Core.Editor {
    public class TagAssetCreation : EditorWindow {
        static HashSet<NeatoTag> s_cachedTags;
        static Dictionary<string, NeatoTag> s_tagNameLookup;
        static bool s_tagCacheDirty = true;
        const int MaxNameCounter = 1000; //Max attempts to add a number to the end of the tag name. Arbitrary amount.
        const int MaxCachedTags = 1000; //Max number of tags to cache. Don't tell me you need more than 1000!?

        /// <summary>
        ///     Opens a folder selection dialog, allowing the user to choose a folder within the Unity project.
        ///     Validates that the chosen folder resides within the project structure. Updates or creates the
        ///     folder location in the associated editor data container. Displays error dialogs for invalid selections.
        /// </summary>
        public static void OpenFolderDialogAndSelectFolder() {
            var path = EditorUtility.OpenFolderPanel( "Tag Folder Location", "Assets", "" );
            var tagPath = GetTagFolderLocation();

            if ( string.IsNullOrEmpty( path ) ) return;

            // Validate the path is within the project
            var projectPath = Path.GetFullPath( Application.dataPath );
            var selectedPath = Path.GetFullPath( path );

            if ( !selectedPath.StartsWith( projectPath, StringComparison.OrdinalIgnoreCase ) ) {
                EditorUtility.DisplayDialog( "Error",
                    "Selected folder must be within the Unity project.", "OK" );
                return;
            }

            var selectedFolder = FileUtil.GetProjectRelativePath( path );
            if ( selectedFolder.Contains( ".." ) ) {
                EditorUtility.DisplayDialog( "Error",
                    "Invalid folder selection.", "OK" );
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
                var dirs = Directory.GetDirectories( $"{Application.dataPath}", "NeatoTags",
                    SearchOption.AllDirectories );
                if ( dirs.Length == 0 ) {
                    Debug.LogError( "[TagAssetCreation]: Could not find NeatoTags directory." );
                    return "";
                }

                var selectedDir = dirs[0];
                // Verify the directory still exists before using it
                if ( !Directory.Exists( selectedDir ) ) {
                    Debug.LogWarning( "[TagAssetCreation]: NeatoTags directory was removed during operation." );
                    return "";
                }

                return Path.Join( "Assets", Path.GetRelativePath( Application.dataPath, selectedDir ) );
            }
            catch ( DirectoryNotFoundException ex ) {
                Debug.LogError( $"[TagAssetCreation]: Directory access failed: {ex.Message}" );
                return "";
            }
            catch ( UnauthorizedAccessException ex ) {
                Debug.LogError( $"[TagAssetCreation]: Access denied: {ex.Message}" );
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

                return Path.Join( "Assets", Path.GetRelativePath( Application.dataPath, selectedDir ) );
            }
            catch ( DirectoryNotFoundException ex ) {
                Debug.LogError( $"[TagAssetCreation]: Directory access failed: {ex.Message}" );
                return "";
            }
            catch ( UnauthorizedAccessException ex ) {
                Debug.LogError( $"[TagAssetCreation]: Access denied: {ex.Message}" );
                return "";
            }
        }


        /// <summary>
        ///     Retrieves the directory path of the UXML folder within the NeatoTags directory.
        ///     Searches for a directory named "UXML" within all subdirectories of the NeatoTags directory.
        ///     Logs an error if the directory is not found and returns an empty string in such cases.
        /// </summary>
        /// <returns>
        ///     Returns the relative path to the UXML folder as a string, or an empty string if the directory cannot be
        ///     located.
        /// </returns>
        public static string GetUxmlDirectory() {
            var neatTagsDirectory = GetNeatoTagsDirectory();
            var dirs = Directory.GetDirectories( $"{neatTagsDirectory}", "UXML",
                SearchOption.AllDirectories );
            var path = Path.Join( "Assets", Path.GetRelativePath( Application.dataPath, dirs[0] ) );
            if ( dirs.Length != 0 ) return path;
            Debug.LogError( "[TagAssetCreation]: Could not find NeatoTags UXML directory." );
            return "";
        }

        /// <summary>
        ///     Retrieves the folder location specified for storing tags within the project.
        ///     If no location has been set, returns an empty string.
        /// </summary>
        /// <returns>Returns the tag folder location as a string, or an empty string if no folder location exists.</returns>
        public static string GetTagFolderLocation() {
            var dataHolder = GetEditorDataContainer();
            return dataHolder == null ? "" : dataHolder.tagFolderLocation;
        }

        /// <summary>
        ///     Retrieves the existing EditorDataHolder asset used to store editor preferences and configurations for NeatoTags.
        ///     If multiple EditorDataHolder assets are found, logs a warning and returns the first one located.
        /// </summary>
        /// <returns>Returns an <see cref="EditorDataHolder" /> instance if one exists, or null if no asset is found.</returns>
        public static EditorDataHolder GetEditorDataContainer() {
            var holdersGuids = AssetDatabase.FindAssets( "EditorDataContainer" );

            if ( holdersGuids.Length == 0 ) return null;

            if ( holdersGuids.Length > 1 ) {
                Debug.LogWarning( "[TagAssetCreation]: Found multiple EditorDataHolders. Using the first one." );
            }

            var holderPath = AssetDatabase.GUIDToAssetPath( holdersGuids[0] );
            return AssetDatabase.LoadAssetAtPath<EditorDataHolder>( holderPath );
        }

        /// <summary>
        ///     Creates a new tag with the specified name, assigns it a unique identifier if necessary,
        ///     adds it to the asset database, updates the tag cache, and optionally focuses on the created tag in the Project
        ///     window.
        /// </summary>
        /// <param name="tagName">The name of the tag to be created. If empty or null, a default name "New Tag" will be used.</param>
        /// <param name="shouldFocusInProjectWindow">
        ///     A flag indicating whether the created tag should be focused in the Project
        ///     window. Default is true.
        /// </param>
        /// <returns>Returns the newly created <see cref="NeatoTag" /> instance, or null if creation fails.</returns>
        public static NeatoTag CreateNewTag( string tagName, bool shouldFocusInProjectWindow = true ) {
            if ( string.IsNullOrEmpty( tagName ) ) {
                tagName = "New Tag";
            }

            uint counter = 0;
            var uniqueName = tagName;
            while ( s_tagNameLookup != null && s_tagNameLookup.ContainsKey( uniqueName ) ) {
                counter++;
                uniqueName = $"{tagName} {counter}";
                if ( counter <= MaxNameCounter ) continue;
                Debug.LogError( "[TagAssetCreation]: Could not create tag. Hard limit reached. Try a different name." );
                return null;
            }

            var newTag = CreateInstance<NeatoTag>();
            var assetPath = GetTagAssetPath( uniqueName );
            if ( string.IsNullOrEmpty( assetPath ) ) {
                Debug.LogError( "[TagAssetCreation]: Could not create tag. Invalid path." );
                return null;
            }

            AssetDatabase.CreateAsset( newTag, assetPath );
            AssetDatabase.SaveAssets();

            if ( s_cachedTags != null && s_tagNameLookup != null ) {
                s_cachedTags.Add( newTag );
                s_tagNameLookup[uniqueName] = newTag;
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
            return "";
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

        /// <summary>
        ///     Deletes the specified tag from the project, removes it from the cache,
        ///     and refreshes the asset database.
        /// </summary>
        /// <param name="selectedTag">The tag to be deleted.</param>
        public static void DeleteTag( NeatoTag selectedTag ) {
            if ( !selectedTag ) return;
            if ( s_cachedTags != null && s_tagNameLookup != null ) {
                s_cachedTags.Remove( selectedTag );
                s_tagNameLookup.Remove( selectedTag.name );
                AssetDatabase.DeleteAsset( AssetDatabase.GetAssetPath( selectedTag ) );
            }
            else {
                InvalidateTagCache();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        ///     Flags the tag cache as dirty, indicating that the cached tag data needs to be refreshed.
        /// </summary>
        public static void InvalidateTagCache() {
            s_tagCacheDirty = true;
        }

        static void RefreshTagCache() {
            s_cachedTags = new HashSet<NeatoTag>();
            s_tagNameLookup = new Dictionary<string, NeatoTag>();

            var guids = AssetDatabase.FindAssets( "t:NeatoTag" );
            if ( guids.Length > MaxCachedTags ) {
                Debug.LogWarning(
                    $"[TagAssetCreation]: Found more than {MaxCachedTags} tags. Only the first {MaxCachedTags} will be used." );
                Array.Resize( ref guids, MaxCachedTags );
            }

            foreach ( var guid in guids ) {
                try {
                    var path = AssetDatabase.GUIDToAssetPath( guid );
                    var tagAsset = AssetDatabase.LoadAssetAtPath<NeatoTag>( path );

                    if ( !tagAsset ) continue;
                    s_cachedTags.Add( tagAsset );
                    s_tagNameLookup[tagAsset.name] = tagAsset;
                }
                catch ( Exception ex ) {
                    Debug.LogWarning( $"[TagAssetCreation]: Failed to load tag {guid}: {ex.Message}" );
                }
            }

            s_tagCacheDirty = false;
        }

        /// <summary>
        ///     Gives back a Hashset of all tags in the project.
        /// </summary>
        /// <returns>Hashset of all tags in the project.</returns>
        public static HashSet<NeatoTag> GetAllTags() {
            if ( s_tagCacheDirty || s_cachedTags == null ) {
                RefreshTagCache();
            }

            return s_cachedTags;
        }
    }
}