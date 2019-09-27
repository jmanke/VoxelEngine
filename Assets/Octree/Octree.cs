using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Toast
{
    public enum NodeIndex
    {
        /// 3 bits
        X = 1,
        Y = 2,
        Z = 4
    }

    public enum Direction
    {
        Right,
        Left,
        Top,
        Bottom,
        Front,
        Back
    }

    public class Octree : MonoBehaviour
    {
        private class Node
        {
            public int depth;
            public int x;
            public int y;
            public int z;
            public Node[] children = new Node[8];
            public int size;

            public Node(int x, int y, int z, int depth, int size)
            {
                //chunk = new Chunk(size);
                this.depth = depth;
                this.x = x;
                this.y = y;
                this.z = z;
                this.size = size;
            }

            public Node Search(Vector3 position)
            {
                float halfLength = AxisLength / 2f;
                byte index = 0;

                if (position.x >= x * size + halfLength) index |= 1;
                if (position.y >= y * size + halfLength) index |= 2;
                if (position.z >= z * size + halfLength) index |= 4;

                return children[index];
            }

            public Vector3 Centre
            {
                get
                {
                    float halfLength = AxisLength / 2f;
                    return new Vector3(x * size + halfLength, y * size + halfLength, z * size + halfLength);
                }
            }

            public float AxisLength
            {
                get
                {
                    return Mathf.Pow(2, depth) * size;
                }
            }
        }

        public int depth = 4;
        public int size = 16;
        public int showDepth = 0;
        public float renderAngle = 10f;

        public Transform target;
        public Color[] colors;

        private Node root;

        private void BuildTreeRecursive(Node node)
        {
            if (node.depth < 1)
                return;

            int halfSize = (int)Mathf.Pow(2, node.depth) / 2;
            //Debug.Log("Halfsize = " + halfSize);

            node.children[0] = new Node(node.x, node.y, node.z, node.depth - 1, size);
            node.children[1] = new Node(node.x + halfSize, node.y, node.z, node.depth - 1, size);
            node.children[4] = new Node(node.x, node.y, node.z + halfSize, node.depth - 1, size);
            node.children[5] = new Node(node.x + halfSize, node.y, node.z + halfSize, node.depth - 1, size);

            node.children[2] = new Node(node.x, node.y + halfSize, node.z, node.depth - 1, size);
            node.children[3] = new Node(node.x + halfSize, node.y + halfSize, node.z, node.depth - 1, size);
            node.children[6] = new Node(node.x, node.y + halfSize, node.z + halfSize, node.depth - 1, size);
            node.children[7] = new Node(node.x + halfSize, node.y + halfSize, node.z + halfSize, node.depth - 1, size);

            foreach (var child in node.children)
            {
                BuildTreeRecursive(child);
            }
        }

        public void BuildTree()
        {
            root = new Node(0, 0, 0, depth, size);

            BuildTreeRecursive(root);
        }

        private void DrawNode(Node node, Color color)
        {
            var origin = new Vector3(node.x, node.y, node.z) * size;
            float currWidth = node.AxisLength;

            var x1 = origin;
            var x2 = origin + new Vector3(currWidth, 0f, 0f);
            var x3 = origin + new Vector3(0f, currWidth, 0f);
            var x4 = origin + new Vector3(currWidth, currWidth, 0f);

            var x5 = origin + new Vector3(0f, 0f, currWidth);
            var x6 = origin + new Vector3(currWidth, 0f, currWidth);
            var x7 = origin + new Vector3(0f, currWidth, currWidth);
            var x8 = origin + new Vector3(currWidth, currWidth, currWidth);

            Handles.color = color;

            Handles.DrawLine(x1, x2);
            Handles.DrawLine(x1, x3);
            Handles.DrawLine(x2, x4);
            Handles.DrawLine(x3, x4);

            Handles.DrawLine(x5, x6);
            Handles.DrawLine(x5, x7);
            Handles.DrawLine(x6, x8);
            Handles.DrawLine(x7, x8);

            Handles.DrawLine(x1, x5);
            Handles.DrawLine(x2, x6);
            Handles.DrawLine(x3, x7);
            Handles.DrawLine(x4, x8);
        }

        private void DrawLODS(Node node)
        {
            if (node.depth == 0)
            {
                DrawNode(node, Color.white);
                return;
            }

            float o = node.AxisLength / 2f;
            float a = Vector3.Distance(target.position, node.Centre);
            float angle = Mathf.Rad2Deg * Mathf.Atan(o / a);

            if (angle < renderAngle)
                DrawNode(node, Color.white);
            else
            {
                foreach (var child in node.children)
                {
                    DrawLODS(child);
                }
            }
        }

        private void DrawTree(Node node, int depth)
        {
            if (node == null)
            {
                return;
            }

            if (node.depth == depth)
            {
                DrawNode(node, Color.red);
            }
            else
            {
                foreach (var child in node.children)
                {
                    DrawTree(child, depth);
                }
            }
        }

        private Node Search(Node node, Vector3 target, int depth)
        {
            if (node.depth == depth)
            {
                return node;
            }

            return Search(node.Search(target), target, depth);
        }

        private Node Search(Vector3 target, int depth)
        {
            return Search(root, target, depth);
        }

        // Testing only
        public void OnDrawGizmos()
        {
            if (root == null)
                BuildTree();
            //TestDraw(Vector3.zero, depth);
            //DrawTree(root);
            DrawLODS(root);
        }
    }
}

