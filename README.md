# NeatoTags
A GameObject Tagging System for Unity.


A GameObject Tagging System using ScriptableObjects. Multiple tags per GameObject and various functions for querying GameObjects with tags. 
No need for a singleton Tag Manager or the use of GetComponent! Just need the GameObject you want to query and a reference to the tag(s) you are interested in. 
Neato Tags have a color and comment property.

I don't know about you but I'd say that's pretty neat!

<img src="https://i.imgur.com/isAJ9CK.png" alt="Neato Tags AddRemove" width="400" height="400"/><img src="https://i.imgur.com/v0xmSNJ.png" alt="Neato Tags AddRemove" width="400" height="400"/>



## Features
- Multiple Tags per GameObject
- Tags have a color and comment property to differentiate
- Tag Manager Window for updating and editing tags
- Extends gameObject with query functions
- No Manager class to keep track of, just call query functions on a gameobject with a tagger component
- Multi-Object Editing
- Query by tag string name (If you change a tag's name you must update your strings!)

## Getting Started
This is currently where I keep my whole project for Neato Tags but I assume you are here for just Neato Tags itself then:
- Just grab the CharlieMadeAThing folder `Assets/CharlieMadeAThing` -> https://github.com/KingRecycle/NeatoTags/tree/master/Assets/CharlieMadeAThing
- Grab Neato Tags from the Asset Store -> https://assetstore.unity.com/packages/tools/utilities/neato-tags-229829

A "Quick Start" guide is included.

If you want to watch a boring video instead of reading:
https://www.youtube.com/watch?v=MIrbBueYae0

Once added to your project there are a few ways to create tags, one way is to use the Neato Tag Manager window.

In the Neato Tag Manager window I recommend setting a default folder location for tags but it isn't required.

In the same window you can press the "+" button to create a new tag and the "-" button to delete the selected tag.

Attach a Tagger component to a gameobject you want to be tagged and click "Edit Tagger" to enter the edit mode, in this mode you can add and remove tags from a gameobject.

Neato Tags has extension functions for gameobjects so you can do something like 

```if ( myGameObject.HasTag( Wizard ) ) { ... }```

There are other ways to query for tags you can checkout in the API Docs.

## Performance Tips
Neato Tags is performant and uses dictionaries and hashsets as much as possible to reduce lookup times.

The only real "slow" part is when filtering for gameobjects aka the ```Tagger.FilterGameObjects()``` function, and you won't really notice a slow-down from this unless you are checking thousands of gameobjects.

```Tagger.FilterGameObjects()``` from my tests takes about ~1-2ms for 10,000 gameobjects.

I don't see any reason why someone would try to filter 10,000 gameobjects in 99% of cases but I just wanted to be upfront about it.

## Demos
Remove the demo you don't need.

DemoBuiltIn -> Non-Scriptable Render Pipeline

DemoSRP -> URP/HDRP

NeatoTags works in either but the demos have materials for the specific version.


[!["Buy Me A Coffee"](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/KingRecycle)

