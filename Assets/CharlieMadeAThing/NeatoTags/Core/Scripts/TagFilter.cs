using System.Collections.Generic;
using System.Linq;

namespace CharlieMadeAThing.NeatoTags.Core {
    /// <summary>
    ///     FilterTags class for chaining filter functions.
    ///     Not meant to be used directly. Use Tagger.FilterTags() instead.
    /// </summary>
    public sealed class TagFilter {
        readonly Tagger _target;
        bool _matchesFilter = true;

        internal TagFilter( Tagger target ) => _target = target;

        /// <summary>
        ///     Checks if the filter matches.
        /// </summary>
        /// <returns>true if filter matches, otherwise false.</returns>
        public bool IsMatch() => _matchesFilter;


        /// <summary>
        ///     Checks if the gameobject has the tag.
        /// </summary>
        /// <param name="tag">Tag to check for</param>
        /// <returns></returns>
        public TagFilter WithTag( NeatoTag tag ) {
            _matchesFilter &= _target.HasTag( tag );
            return this;
        }

        /// <summary>
        ///     Checks if the gameobject has all the tags in params.
        /// </summary>
        /// <param name="tags">Tags to check for</param>
        /// <returns></returns>
        public TagFilter WithTags( params NeatoTag[] tags ) {
            foreach ( var tagAsset in tags ) {
                _matchesFilter &= _target.HasTag( tagAsset );
            }

            return this;
        }

        /// <summary>
        ///     Checks if the gameobject has all the tags in a list.
        /// </summary>
        /// <param name="tagList">Tags to check for</param>
        /// <returns></returns>
        public TagFilter WithTags( IEnumerable<NeatoTag> tagList ) {
            foreach ( var tagAsset in tagList ) {
                _matchesFilter &= _target.HasTag( tagAsset );
            }

            return this;
        }

        /// <summary>
        ///     Checks if the gameobject doesn't have the tag.
        /// </summary>
        /// <param name="tag">Tags to check for</param>
        /// <returns></returns>
        public TagFilter WithoutTag( NeatoTag tag ) {
            _matchesFilter &= !_target.HasTag( tag );

            return this;
        }

        /// <summary>
        ///     Checks if the gameobject doesn't have tags in params.
        /// </summary>
        /// <param name="tags">Tags to check for</param>
        /// <returns></returns>
        public TagFilter WithoutTags( params NeatoTag[] tags ) {
            foreach ( var tagAsset in tags ) {
                _matchesFilter &= !_target.HasTag( tagAsset );
            }

            return this;
        }

        /// <summary>
        ///     Checks if the gameobject doesn't have tags in a list.
        /// </summary>
        /// <param name="tagList">Tags to check for</param>
        /// <returns></returns>
        public TagFilter WithoutTags( IEnumerable<NeatoTag> tagList ) {
            foreach ( var tagAsset in tagList ) {
                _matchesFilter &= !_target.HasTag( tagAsset );
            }

            return this;
        }

        /// <summary>
        ///     Checks if the gameobject has any of the tags in a list.
        /// </summary>
        /// <param name="tagList">Tags to check for</param>
        /// <returns></returns>
        public TagFilter WithAnyTags( IEnumerable<NeatoTag> tagList ) {
            var neatoTagAssets = tagList as NeatoTag[] ?? tagList.ToArray();
            _matchesFilter &= _target.AnyTagsMatch( neatoTagAssets );

            return this;
        }

        /// <summary>
        ///     Checks if the gameobject has any of the tags in params.
        /// </summary>
        /// <param name="tags">Tags to check for</param>
        /// <returns></returns>
        public TagFilter WithAnyTags( params NeatoTag[] tags ) {
            _matchesFilter &= _target.AnyTagsMatch( tags );

            return this;
        }
    }
}