using System;
using System.Collections.Generic;
using CharlieMadeAThing.GameplayTags;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class TagManagerEditor : EditorWindow
{
    [MenuItem("Window/UI Toolkit/TagManagerEditor")]
    public static void ShowExample()
    {
        TagManagerEditor wnd = GetWindow<TagManagerEditor>();
        wnd.titleContent = new GUIContent("TagManagerEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        
        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/GameplayTags/UI/Editor/TagManagerEditor.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);
        
        const int itemCount = 1000;
        var items = new List<TreeNode<string>>(itemCount);
        for (int i = 1; i <= itemCount; i++)
            items.Add(new TreeNode<string>( "Tag"+ i ));

        var listView = root.Q<ListView>( "rootTagListView" );
        // The "makeItem" function is called when the
        // ListView needs more items to render.
        Func<VisualElement> makeItem = () => {
            var label = new Label();
            label.AddToClassList( "rootTag-Label" );
            return label;
        };

        // As the user scrolls through the list, the ListView object
        // recycles elements created by the "makeItem" function,
        // and invoke the "bindItem" callback to associate
        // the element with the matching data item (specified as an index in the list).
        Action<VisualElement, int> bindItem = (e, i) => ((Label) e).text = items[i].Data;

        // Provide the list view with an explict height for every row
        // so it can calculate how many items to actually display
        const int itemHeight = 32;

        listView.makeItem = makeItem;
        listView.bindItem = bindItem;
        listView.itemsSource = items;
        listView.fixedItemHeight = itemHeight;

        listView.selectionType = SelectionType.Multiple;

        listView.onItemsChosen += objects => Debug.Log(objects);
        listView.onSelectionChange += objects => Debug.Log(objects);
        
    }
    
}