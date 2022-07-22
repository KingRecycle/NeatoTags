/**
 * TreeNode.cs
 * Author: Luke Holland (http://lukeholland.me/)
 */

using System.Collections.Generic;

namespace CharlieMadeAThing.GameplayTags {
    public class TreeNode<T> {
        public delegate bool TraversalDataDelegate( T data );

        public delegate bool TraversalNodeDelegate( TreeNode<T> node );

        readonly List<TreeNode<T>> _children;

        public TreeNode( T data ) {
            Data = data;
            _children = new List<TreeNode<T>>();
            Level = 0;
        }

        public TreeNode( T data, TreeNode<T> parent ) : this( data ) {
            Parent = parent;
            Level = Parent != null ? Parent.Level + 1 : 0;
        }

        public int Level { get; }

        public int Count => _children.Count;
        public bool IsRoot => Parent == null;
        public bool IsParent => !IsLeaf;
        public bool IsLeaf => _children.Count == 0;
        public T Data { get; }

        public TreeNode<T> Parent { get; }

        public string Comment { get; set; }

        public TreeNode<T> this[ int key ] => _children[key];

        public void Clear() {
            _children.Clear();
        }

        public TreeNode<T> AddChild( T value ) {
            var node = new TreeNode<T>( value, this );
            _children.Add( node );

            return node;
        }

        public bool HasChild( T data ) {
            return FindInChildren( data ) != null;
        }

        public TreeNode<T> FindInChildren( T data ) {
            int i = 0, l = Count;
            for ( ; i < l; ++i ) {
                var child = _children[i];
                if ( child.Data.Equals( data ) ) return child;
            }

            return null;
        }

        public bool RemoveChild( TreeNode<T> node ) {
            return _children.Remove( node );
        }

        public void Traverse( TraversalDataDelegate handler ) {
            if ( handler( Data ) ) {
                int i = 0, l = Count;
                for ( ; i < l; ++i ) _children[i].Traverse( handler );
            }
        }

        public void Traverse( TraversalNodeDelegate handler ) {
            if ( handler( this ) ) {
                int i = 0, l = Count;
                for ( ; i < l; ++i ) _children[i].Traverse( handler );
            }
        }
    }
}