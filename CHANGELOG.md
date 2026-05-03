# Changelog

All notable changes to NeatoTags are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Fixed

- `NeatoTagAssetModificationProcessor.OnPostprocessAllAssets` now early-returns when no `NeatoTag` asset is present in the imported, moved, or deleted batches. Previously every asset import (textures, scripts, materials, anything) triggered a full `UpdateTaggers()` + `InvalidateTagCache()` pass, which caused inspector lag during normal editor work and slowed bulk reimports. ([#20](https://github.com/KingRecycle/NeatoTags/issues/20))
- `Tagger.HasTagger` and `Tagger.TryGetTagger` now short-circuit on a null `GameObject` instead of letting it reach the underlying `Dictionary` lookup (which throws `ArgumentNullException` on null keys). All 14 query extensions in `NeatoTagsExtensions` (`HasTag`, `HasAnyTagsMatching`, `HasAllTagsMatching`, `HasNoTagsMatching` overloads) inherit the safety, returning `false`/`true`-by-vacuity instead of throwing when called on a null receiver. ([#19](https://github.com/KingRecycle/NeatoTags/issues/19))
- `Tagger.RemoveTag(string)` no longer throws `NullReferenceException` when `_tags` contains a true C# null entry. The lookup predicate now skips null entries before reading `.name`. Affects setup paths where `_tags` is mutated post-`Awake` (test fixtures, post-`Awake` serialization writes) and would otherwise leave a real null in the list. ([#18](https://github.com/KingRecycle/NeatoTags/issues/18))
- `Tagger.OnValidate` now dedupes `_tags` after the existing null-removal pass, so an inspector edit that drops the same `NeatoTag` into the list twice (which bypasses the runtime `AddTag` guard) no longer leaves a phantom entry. `_tags.Count` matches the logical tag count, and `RemoveTag` removes the only entry instead of leaving a duplicate behind. ([#17](https://github.com/KingRecycle/NeatoTags/issues/17))
- `Tagger.HasTag(string)` no longer returns stale results after a `NeatoTag` rename. The per-`Tagger` `HashSet<string>` cache was only invalidated on add/remove, so renaming a tag silently produced false positives for the old name and false negatives for the new one. Cache removed; `HasTag(string)` now iterates `_tags` directly. ([#16](https://github.com/KingRecycle/NeatoTags/issues/16))
- `TaggerDrawer.OnDisable` now null-checks `Target` before unsubscribing from `OnWantRepaint`, preventing `NullReferenceException` in narrow editor teardown paths where the underlying target reference has been cleared. ([#15](https://github.com/KingRecycle/NeatoTags/issues/15))
- `NeatoTagManager.CreateGUI` now null-checks the loaded `VisualTreeAsset` before calling `CloneTree`. A missing or renamed UXML asset now logs a clear error instead of throwing `NullReferenceException`. ([#14](https://github.com/KingRecycle/NeatoTags/issues/14))
- `TagAssetCreation.GetUxmlDirectory` now checks `dirs.Length` before indexing `dirs[0]`. A missing UXML directory now produces the intended `LogError` instead of an `IndexOutOfRangeException`. ([#13](https://github.com/KingRecycle/NeatoTags/issues/13))
- `NeatoTagsExtensions.AddTag` and `AddTagWithForce` no longer throw `NullReferenceException` when called with both a null `GameObject` and a null `NeatoTag`. The "GameObject is null" warning no longer interpolates `tag.name` before checking whether `tag` itself is null. ([#12](https://github.com/KingRecycle/NeatoTags/issues/12))
- `Tagger.RemoveTag(NeatoTag)` is now a strict no-op when called for a tag the `Tagger` doesn't own. It also correctly re-registers the `GameObject` as non-tagged after removing its last tag, even when the registry entry for that tag has already been wiped. ([#11](https://github.com/KingRecycle/NeatoTags/issues/11))
- `TaggerRegistry.RegisterGameObjectToTag` and `UnregisterGameObjectFromTag` now guard their tag-set lookups with `TryGetValue` instead of indexing the dictionary directly. Calling these helpers without a prior `RegisterTag`, or after `UnregisterTag` / `ResetRegistry`, no longer throws `KeyNotFoundException`. ([#10](https://github.com/KingRecycle/NeatoTags/issues/10), [#41](https://github.com/KingRecycle/NeatoTags/issues/41))
- `TaggerRegistry.RegisterTagger` now uses `Dictionary.TryAdd` instead of `Add`, and `Tagger` carries a `[DisallowMultipleComponent]` attribute. Duplicate `Tagger` registration no longer throws `ArgumentException`, and Unity refuses to add a second `Tagger` to the same `GameObject`. ([#9](https://github.com/KingRecycle/NeatoTags/issues/9))
- `TaggerRegistry.UnregisterTag` now snapshots the tag's `GameObject` set with `ToList()` before iterating, preventing the `InvalidOperationException` that fired whenever the tag was applied to any `GameObject` at the time of unregistration. ([#8](https://github.com/KingRecycle/NeatoTags/issues/8))

[Unreleased]: https://github.com/KingRecycle/NeatoTags/compare/master...HEAD
