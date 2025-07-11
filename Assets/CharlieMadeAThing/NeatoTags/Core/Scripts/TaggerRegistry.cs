using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Core {
    /// <summary>
    ///     Provides a centralized registry for managing taggers and their associated tags
    ///     and objects within the context of a Unity project.
    /// </summary>
    /// <remarks>
    ///     This static class is responsible for tracking the relationships between
    ///     Taggers, GameObjects, and NeatoTags, enabling efficient lookup and
    ///     management of tagged entities. It maintains separate collections for:
    ///     - Registered taggers associated with specific GameObjects.
    ///     - GameObjects registered under specific tags.
    ///     - GameObjects that are not associated with any tags.
    /// </remarks>
    public static class TaggerRegistry {
        // Static collections for tracking tagged objects and taggers in the scene.

        // All taggers in the scene (@runtime).
        static readonly Dictionary<GameObject, Tagger> s_taggers = new();

        // All tagged objects in the scene by tag (@runtime).
        //NOTE: This is NOT cleared when the scene changes, but the gameobjects in it are.
        static readonly Dictionary<NeatoTag, HashSet<GameObject>> s_taggedObjects = new();

        // All gameobjects in the scene that have a Tagger component but no tags (@runtime).
        static readonly HashSet<GameObject> s_nonTaggedObjects = new();

        public static Dictionary<GameObject, Tagger> GetStaticTaggersDictionary() {
            return s_taggers;
        }

        public static Dictionary<NeatoTag, HashSet<GameObject>> GetStaticTaggedObjectsDictionary() {
            return s_taggedObjects;
        }

        public static HashSet<GameObject> GetStaticNonTaggedGameObjects() {
            return s_nonTaggedObjects;
        }

        /// <summary>
        ///     Initializes a new <see cref="Tagger" /> instance by registering it with the tag registry
        ///     and associating its tags with the relevant <see cref="GameObject" />.
        /// </summary>
        /// <remarks>
        ///     This is used by the <see cref="Tagger" /> component to register itself.
        ///     You do not need to call this method manually.
        /// </remarks>
        /// <param name="gameObject">The <see cref="GameObject" /> to associate with the provided <see cref="Tagger" />.</param>
        /// <param name="tagger">The <see cref="Tagger" /> instance to be initialized and registered.</param>
        public static void InitializeNewTagger( GameObject gameObject, Tagger tagger ) {
            if ( !gameObject || !tagger ) {
                Debug.LogWarning( "[TaggerRegistry]: Attempting to initialize a null tagger or gameobject!" );
                return;
            }

            RegisterTagger( gameObject, tagger );

            if ( !tagger.IsTagged() ) {
                RegisterNonTaggedGameObject( gameObject );
            }

            //Loop through this gameobject's tags and register the gameobject to its other tags.
            foreach ( var neatoTag in tagger.GetTags ) {
                RegisterTag( neatoTag ); //Just in case the tag hasn't been registered yet.
                RegisterGameObjectToTag( gameObject, neatoTag );
            }
        }

        /// <summary>
        ///     Resets the internal registries of the <see cref="TaggerRegistry" /> by clearing
        ///     all taggers, tagged objects, and non-tagged objects stored in the registry.
        /// </summary>
        /// <remarks>
        ///     This method will remove all data from the registry and log a message indicating
        ///     that the registry has been reset. This can be useful for scenarios where a clean
        ///     slate for the registry is required. When is that? I have no clue, so use it at your own risk.
        /// </remarks>
        public static void ResetRegistry() {
            s_taggers.Clear();
            s_taggedObjects.Clear();
            s_nonTaggedObjects.Clear();
            Debug.Log( "[TaggerRegistry]: Registry has been reset." );
        }


        /// <summary>
        ///     Removes a <see cref="Tagger" /> and its associated tags from
        ///     the specified <see cref="GameObject" /> in the tag registry.
        ///     Effectively removing it from the system.
        /// </summary>
        /// <remarks>
        ///     This is used by the <see cref="Tagger" /> component to unregister itself.
        ///     You do not need to call this method manually.
        /// </remarks>
        /// <param name="gameObject">
        ///     The <see cref="GameObject" /> from which the <see cref="Tagger" /> and its tags are to be
        ///     removed.
        /// </param>
        /// <param name="tagger">
        ///     The <see cref="Tagger" /> instance associated with the <see cref="GameObject" /> to be removed
        ///     from the registry.
        /// </param>
        public static void RemoveTaggerFromRegistry( GameObject gameObject, Tagger tagger ) {
            foreach ( var neatoTag in tagger.GetTags ) {
                UnregisterGameObjectFromTag( gameObject, neatoTag );
            }

            UnregisterTagger( gameObject );
            UnregisterNonTaggedGameObject( gameObject );
        }

        /// <summary>
        ///     Registers a <see cref="Tagger" /> instance and associates it with the provided <see cref="GameObject" />.
        /// </summary>
        /// <param name="gameObject">The <see cref="GameObject" /> to associate with the provided <see cref="Tagger" />.</param>
        /// <param name="tagger">The <see cref="Tagger" /> instance to be registered.</param>
        static void RegisterTagger( GameObject gameObject, Tagger tagger ) {
            if ( !gameObject || !tagger ) {
                Debug.LogWarning( "[TaggerRegistry]: Attempting to register a null tagger or gameobject!" );
                return;
            }

            s_taggers.Add( gameObject, tagger );
        }

        /// <summary>
        ///     Unregisters a <see cref="Tagger" /> instance associated with the specified <see cref="GameObject" />
        ///     from the tag registry, effectively removing it from the system.
        /// </summary>
        /// <param name="gameObject">The <see cref="GameObject" /> whose associated <see cref="Tagger" /> is to be unregistered.</param>
        static void UnregisterTagger( GameObject gameObject ) {
            if ( !gameObject ) {
                Debug.LogWarning( "[TaggerRegistry]: Attempting to unregister a null tagger or gameobject!" );
                return;
            }

            s_taggers.Remove( gameObject );
        }

        /// <summary>
        ///     Registers a new tag in the tag registry. This ensures the tag is tracked
        ///     and ready to be associated with GameObjects at runtime.
        /// </summary>
        /// <remarks>
        ///     This is used by the <see cref="Tagger" /> component to register its tags.
        ///     You do not need to call this method manually.
        ///     Registered tags do not unregister themselves on scene change by default.
        /// </remarks>
        /// <param name="tagToRegister">The <see cref="NeatoTag" /> to register in the system.</param>
        public static void RegisterTag( NeatoTag tagToRegister ) {
            if ( !tagToRegister ) {
                Debug.LogWarning( "[TaggerRegistry]: Attempting to register a null tag!" );
                return;
            }

            s_taggedObjects.TryAdd( tagToRegister, new HashSet<GameObject>() );
        }

        /// <summary>
        ///     Unregisters a specified <see cref="NeatoTag" /> from the tag registry, removing all associations
        ///     it has with any <see cref="GameObject" />.
        /// </summary>
        /// <remarks>
        ///     This is used by the <see cref="Tagger" /> component to unregister its tags.
        ///     You do not need to call this method manually unless you want to remove a tag from the registry.
        ///     By default, this is not called automatically.
        ///     Doing so will remove the tag and the tag will not be considered to have ever existed on any gameobject.
        /// </remarks>
        /// <param name="tagToUnregister">The <see cref="NeatoTag" /> to be unregistered from the registry.</param>
        public static void UnregisterTag( NeatoTag tagToUnregister ) {
            if ( !tagToUnregister ) {
                Debug.LogWarning( "[TaggerRegistry]: Attempting to unregister a null tag!" );
                return;
            }

            foreach ( var gameObject in s_taggedObjects[tagToUnregister] ) {
                gameObject.RemoveTag( tagToUnregister );
            }
            s_taggedObjects.Remove( tagToUnregister );
        }

        /// <summary>
        ///     Registers the specified <see cref="GameObject" /> to the specified <see cref="NeatoTag" /> in the tag registry.
        /// </summary>
        /// <remarks>
        ///     This is used by the <see cref="Tagger" /> component to register its tags.
        ///     You do not need to call this method manually.
        /// </remarks>
        /// <param name="gameObject">The <see cref="GameObject" /> to register to the given <see cref="NeatoTag" />.</param>
        /// <param name="tag">The <see cref="NeatoTag" /> to associate with the given <see cref="GameObject" />.</param>
        public static void RegisterGameObjectToTag( GameObject gameObject, NeatoTag tag ) {
            if ( !gameObject || !tag ) {
                Debug.LogWarning( "[TaggerRegistry]: Attempting to register a null gameobject or tag!" );
                return;
            }

            s_taggedObjects[tag].Add( gameObject );
            s_nonTaggedObjects.Remove( gameObject );
        }

        /// <summary>
        ///     Unregisters a <see cref="GameObject" /> from a specified <see cref="NeatoTag" /> in the tag registry.
        /// </summary>
        /// <param name="gameObject">The <see cref="GameObject" /> to remove from the specified tag.</param>
        /// <param name="tag">The <see cref="NeatoTag" /> from which the <see cref="GameObject" /> will be removed.</param>
        static void UnregisterGameObjectFromTag( GameObject gameObject, NeatoTag tag ) {
            if ( !gameObject || !tag ) {
                Debug.LogWarning( "[TaggerRegistry]: Attempting to unregister a null gameobject or tag!" );
                return;
            }

            s_taggedObjects[tag].Remove( gameObject );
            if ( !gameObject.IsTagged() ) {
                s_nonTaggedObjects.Add( gameObject );
            }
        }

        /// <summary>
        ///     Registers a <see cref="GameObject" /> that lacks any associated tags to the non-tagged objects registry.
        /// </summary>
        /// <remarks>
        ///     This is used by the <see cref="Tagger" /> component to register itself when it has no tags.
        ///     You do not need to call this method manually.
        /// </remarks>
        /// <param name="gameObject">The <see cref="GameObject" /> to register as non-tagged. Must not be null.</param>
        public static void RegisterNonTaggedGameObject( GameObject gameObject ) {
            if ( !gameObject ) {
                Debug.LogWarning( "[TaggerRegistry]: Attempting to register a null gameobject!" );
                return;
            }

            s_nonTaggedObjects.Add( gameObject );
        }

        /// <summary>
        ///     Unregisters a <see cref="GameObject" /> from the list of non-tagged objects.
        /// </summary>
        /// <param name="gameObject">The <see cref="GameObject" /> to be removed from the non-tagged objects list.</param>
        static void UnregisterNonTaggedGameObject( GameObject gameObject ) {
            if ( !gameObject ) {
                Debug.LogWarning( "[TaggerRegistry]: Attempting to unregister a null gameobject!" );
                return;
            }

            s_nonTaggedObjects.Remove( gameObject );
        }

        /// <summary>
        ///     Retrieves a registered <see cref="NeatoTag" /> instance associated with the specified tag name.
        /// </summary>
        /// <param name="tagName">The name of the tag to retrieve from the registry.</param>
        /// <returns>The <see cref="NeatoTag" /> instance if found; otherwise, null.</returns>
        public static NeatoTag GetRegisteredTag( string tagName ) {
            return s_taggedObjects.Keys.FirstOrDefault( t => t.name == tagName );
        }
    }
}