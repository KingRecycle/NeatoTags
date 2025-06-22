using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Core {
    /// <summary>
    ///     GameObjectFilter class for chaining filter functions.
    ///     Not intended to be used directly. Use Tagger.FilterGameObjects() instead.
    /// </summary>
    public sealed class GameObjectFilter {
        readonly HashSet<GameObject> _matches;

        internal GameObjectFilter( IEnumerable<GameObject> gameObjects ) {
            _matches = new HashSet<GameObject>();
            if ( gameObjects == null ) {
                Debug.LogWarning(
                    "You are trying to filter a null list of gameobjects. Defaulting to all tagged gameobjects." );
            }

            var gameObjectsToFilter = gameObjects == null
                ? TaggerRegistry.GetStaticTaggersDictionary().Keys
                : gameObjects.Where( x => x.HasTagger() );
            _matches.UnionWith( gameObjectsToFilter );
        }


        /// <summary>
        ///     The result of the filter.
        ///     Will not return duplicate GameObjects.
        /// </summary>
        /// <returns>HashSet of GameObjects</returns>
        public HashSet<GameObject> GetMatches() => _matches;


        /// <summary>
        ///     Filters for GameObjects that have the tag.
        /// </summary>
        /// <param name="tag">Tag to check for.</param>
        /// <returns></returns>
        public GameObjectFilter WithTag( NeatoTag tag ) {
            TaggerRegistry.GetStaticTaggedObjectsDictionary().TryGetValue( tag, out var tempMatches );
            tempMatches ??= new HashSet<GameObject>();

            _matches.IntersectWith( tempMatches );
            return this;
        }

        /// <summary>
        ///     Filters for GameObjects that have all the tags.
        /// </summary>
        /// <param name="tags">IEnumerable of NeatoTag.</param>
        /// <returns></returns>
        public GameObjectFilter WithTags( IEnumerable<NeatoTag> tags ) {
            return tags.Aggregate( this, ( current, neatoTag ) => current.WithTag( neatoTag ) );
        }

        /// <summary>
        ///     Filters for GameObjects that have all the tags.
        /// </summary>
        /// <param name="tags">IEnumerable of NeatoTag.</param>
        /// <returns></returns>
        public GameObjectFilter WithTags( params NeatoTag[] tags ) => WithTags( tags.AsEnumerable() );

        /// <summary>
        ///     FilterGameObjects for GameObjects that don't have the tag.
        /// </summary>
        /// <param name="tag">Tag to check for.</param>
        /// <returns></returns>
        public GameObjectFilter WithoutTag( NeatoTag tag ) {
            _matches.RemoveWhere( taggedObject => taggedObject.HasTag( tag ) );
            return this;
        }

        /// <summary>
        ///     Filters for GameObjects that have none of the tags.
        /// </summary>
        /// <param name="tags">IEnumerable of NeatoTag.</param>
        /// <returns></returns>
        public GameObjectFilter WithoutTags( IEnumerable<NeatoTag> tags ) {
            return tags.Aggregate( this, ( current, neatoTag ) => current.WithoutTag( neatoTag ) );
        }

        /// <summary>
        ///     Filters for GameObjects that have none of the tags.
        /// </summary>
        /// <param name="tags">IEnumerable of NeatoTag.</param>
        /// <returns></returns>
        public GameObjectFilter WithoutTags( params NeatoTag[] tags ) => WithoutTags( tags.AsEnumerable() );

        /// <summary>
        ///     FilterGameObjects for GameObjects that have any of the tags.
        /// </summary>
        /// <param name="tags">IEnumerable of NeatTagAsset</param>
        /// <returns></returns>
        public GameObjectFilter WithAnyTags( IEnumerable<NeatoTag> tags ) {
            if ( tags == null ) {
                _matches.Clear();
                return this;
            }

            var neatoTags = tags as NeatoTag[] ?? tags.ToArray();
            if ( !neatoTags.Any() ) {
                _matches.Clear();
                return this;
            }

            var tempMatches = new HashSet<GameObject>();
            var taggedObjectsMap = TaggerRegistry.GetStaticTaggedObjectsDictionary();
            foreach ( var neatoTag in neatoTags ) {
                if ( !taggedObjectsMap.TryGetValue( neatoTag, out var taggedObjects ) ) continue;
                tempMatches.UnionWith( taggedObjects );
            }

            _matches.IntersectWith( tempMatches );
            return this;
        }

        /// <summary>
        ///     FilterGameObjects for GameObjects that have any of the tags.
        /// </summary>
        /// <param name="tags">IEnumerable of NeatTagAsset</param>
        /// <returns></returns>
        public GameObjectFilter WithAnyTags( params NeatoTag[] tags ) => WithAnyTags( tags.AsEnumerable() );
    }
}