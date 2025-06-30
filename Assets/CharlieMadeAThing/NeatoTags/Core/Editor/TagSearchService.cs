using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CharlieMadeAThing.NeatoTags.Core.Editor {
    /// <summary>
    ///     Service class responsible for searching and filtering tags based on various criteria.
    ///     Just used for searching boxes in the editor.
    /// </summary>
    public static class TagSearchService {
        const int PriorityBase = 10;
        const int FuzzyMatchPriorityBase = 100;

        /// <summary>
        ///     Represents a search result with relevance scoring
        /// </summary>
        class SearchResult {
            public NeatoTag Tag { get; set; }
            public int RelevanceScore { get; set; }
        }

        /// <summary>
        ///     Calculates relevance score for a tag name against a search term.
        ///     Lower scores indicate better matches.
        /// </summary>
        /// <param name="tagName">The tag name to score</param>
        /// <param name="searchTerm">The search term</param>
        /// <returns>Relevance score (lower is better)</returns>
        static int CalculateRelevanceScore( string tagName, string searchTerm ) {
            if ( string.IsNullOrWhiteSpace( searchTerm ) ) {
                return int.MaxValue; // No search term means the lowest priority
            }

            var tagNameLower = tagName.ToLower();
            var searchTermLower = searchTerm.ToLower();

            // Exact match gets the highest priority (score 0)
            if ( tagNameLower == searchTermLower ) {
                return 0;
            }

            // Starts with the search term gets the second-highest priority
            if ( tagNameLower.StartsWith( searchTermLower ) ) {
                return 1;
            }

            // Find the first occurrence of the search term
            var firstIndex = tagNameLower.IndexOf( searchTermLower, StringComparison.InvariantCultureIgnoreCase );
            if ( firstIndex >= 0 ) {
                // If the search term is close to the beginning of the tag name then it gets a lower score.

                return PriorityBase + firstIndex;
            }

            // Check for character-by-character matching (fuzzy matching)
            var fuzzyScore = CalculateFuzzyMatchScore( tagNameLower, searchTermLower );
            if ( fuzzyScore >= 0 ) {
                return FuzzyMatchPriorityBase + fuzzyScore;
            }

            return int.MaxValue;
        }

        /// <summary>
        ///     Calculates a fuzzy match score for partial character matching.
        /// </summary>
        /// <param name="tagName">The tag name</param>
        /// <param name="searchTerm">The search term</param>
        /// <returns>Fuzzy match score, or -1 if no match</returns>
        static int CalculateFuzzyMatchScore( string tagName, string searchTerm ) {
            if ( string.IsNullOrEmpty( tagName ) || string.IsNullOrEmpty( searchTerm ) ) {
                return -1; // No match possible
            }

            var tagIndex = 0;
            var searchIndex = 0;
            var score = 0;

            while ( tagIndex < tagName.Length && searchIndex < searchTerm.Length ) {
                if ( char.ToLower( tagName[tagIndex] ) == char.ToLower( searchTerm[searchIndex] ) ) {
                    // Match found: Add index to score (earlier matches have lower scores)
                    score += tagIndex;
                    searchIndex++;
                }

                tagIndex++;
            }

            // If all searchTerm characters were matched, return the score; otherwise, no fuzzy match
            return searchIndex == searchTerm.Length ? score : -1;
        }

        /// <summary>
        ///     Searches for tags based on a search term using relevance scoring.
        /// </summary>
        /// <param name="tags">The collection of tags to search through</param>
        /// <param name="searchTerm">The search term to match against tag names</param>
        /// <param name="useExactMatch">Whether to use exact matching (starting with ^)</param>
        /// <returns>Filtered and sorted collection of tags matching the search criteria</returns>
        static IEnumerable<NeatoTag> SearchTags( IEnumerable<NeatoTag> tags, string searchTerm,
            bool useExactMatch = false ) {
            if ( string.IsNullOrWhiteSpace( searchTerm ) ) {
                return tags;
            }

            if ( tags == null ) {
                Debug.LogError( "[TagSearchService]: Null tags passed to SearchTags." );
                return Enumerable.Empty<NeatoTag>();
            }

            // Handle exact match mode (when search starts with ^)
            if ( useExactMatch || searchTerm.StartsWith( "^" ) ) {
                var exactSearchTerm = searchTerm.StartsWith( "^" ) ? searchTerm[1..] : searchTerm;
                return tags.Where( tag =>
                    tag.name.StartsWith( exactSearchTerm ) );
            }

            // Use relevance-based searching
            var searchResults = new List<SearchResult>();

            foreach ( var tag in tags ) {
                if ( tag == null ) {
                    Debug.LogWarning(
                        "[TagSearchService]: Null tag found in search results. Null was skipped but this should not happen." );
                    continue;
                }

                var score = CalculateRelevanceScore( tag.name, searchTerm );
                if ( score < int.MaxValue ) {
                    // Only include tags that have some match
                    searchResults.Add( new SearchResult { Tag = tag, RelevanceScore = score } );
                }
            }


            // Sort by relevance score (lower is better), then by name as a secondary sort
            return searchResults
                .OrderBy( r => r.RelevanceScore )
                .ThenBy( r => r.Tag.name )
                .Select( r => r.Tag );
        }

        /// <summary>
        ///     Filters tags into selected and available categories based on tagger state with relevance sorting.
        /// </summary>
        /// <param name="allTags">All available tags</param>
        /// <param name="tagger">The tagger to check against</param>
        /// <param name="selectedSearchTerm">Search term for selected tags</param>
        /// <param name="availableSearchTerm">Search term for available tags</param>
        /// <returns>A tuple containing selected and available tags sorted by relevance</returns>
        public static (IEnumerable<NeatoTag> selectedTags, IEnumerable<NeatoTag> availableTags) FilterTagsByTaggerState(
            IEnumerable<NeatoTag> allTags, Tagger tagger, string selectedSearchTerm = null,
            string availableSearchTerm = null ) {
            //If only 1 or the other is null then log error but continue since you can still display the other result.
            if ( !tagger ) {
                Debug.LogError( "[TagSearchService]: Null tagger passed to FilterTagsByTaggerState." );
            }

            if ( allTags == null ) {
                Debug.LogError( "[TagSearchService]: Null tags passed to FilterTagsByTaggerState." );
            }

            if ( !tagger && allTags == null ) {
                return (Enumerable.Empty<NeatoTag>(), Enumerable.Empty<NeatoTag>());
            }

            var neatoTags = allTags == null ? Array.Empty<NeatoTag>() : allTags.ToArray();
            var tagsOnTagger = tagger.GetTags;
            var selectedTags = Enumerable.Empty<NeatoTag>();
            var availableTags = Enumerable.Empty<NeatoTag>();

            if ( tagsOnTagger != null ) {
                selectedTags = FilterAndSortTags(
                    neatoTags,
                    tag => tagger.GetTags != null && tagger.GetTags.Contains( tag ),
                    selectedSearchTerm
                );
            }

            if ( neatoTags.Length > 0 ) {
                availableTags = FilterAndSortTags(
                    neatoTags,
                    tag => tagger.GetTags == null || !tagger.GetTags.Contains( tag ),
                    availableSearchTerm
                );
            }


            return (selectedTags, availableTags);
        }

        /// <summary>
        ///     Filters and sorts tags based on whether they're selected or not, applying relevance scoring.
        /// </summary>
        /// <param name="tags">All available tags</param>
        /// <param name="isSelected">Predicate to determine if a tag is selected</param>
        /// <param name="searchTerm">Search term to filter tags</param>
        /// <returns>A sorted list of tags based on relevance and name</returns>
        static IEnumerable<NeatoTag> FilterAndSortTags(
            IEnumerable<NeatoTag> tags,
            Func<NeatoTag, bool> isSelected,
            string searchTerm ) {
            var results = new List<SearchResult>();

            foreach ( var tag in tags ) {
                if ( tag == null ) {
                    Debug.LogWarning(
                        "[TagSearchService]: Null tag found in filter results. Null was skipped but this should not happen." );
                    continue;
                }

                // Check if the tag matches the selected/available condition
                if ( !isSelected( tag ) ) continue;
                var score = CalculateRelevanceScore( tag.name, searchTerm ?? "" );
                if ( score < int.MaxValue || string.IsNullOrWhiteSpace( searchTerm ) ) {
                    results.Add( new SearchResult { Tag = tag, RelevanceScore = score } );
                }
            }

            return results
                .OrderBy( r => r.RelevanceScore )
                .ThenBy( r => r.Tag.name )
                .Select( r => r.Tag );
        }


        /// <summary>
        ///     Gets all tags ordered by name and optionally filtered by search term with relevance sorting.
        /// </summary>
        /// <param name="searchTerm">Optional search term to filter tags</param>
        /// <returns>Ordered the collection of tags sorted by relevance if a search term is provided</returns>
        public static IEnumerable<NeatoTag> GetOrderedTags( string searchTerm = null ) {
            var allTags = TagAssetCreation.GetAllTags();

            return string.IsNullOrWhiteSpace( searchTerm )
                ? allTags.OrderBy( tag => tag.name )
                : SearchTags( allTags, searchTerm );
        }
    }
}