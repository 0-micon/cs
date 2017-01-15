using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    class SearchTree
    {
        const uint MOVE_COUNT = Cube.FACE_NUM * 3;

        SearchNode _root = new SearchNode();

        class SearchNode
        {
            public SearchNode[] children = new SearchNode[MOVE_COUNT];
            public string result = null;
        }

        public string GetReplacement(string key, ref int start)
        {
            SearchNode node = _root;
            for (; start < key.Length; start++)
            {
                int idx = key[start] - 'a';
                node = node.children[idx];
                if(node == null)
                {
                    return null;
                }
                if(node.result != null)
                {
                    return node.result;
                }
            }
            return null;
        }

        public void Add(string key, string val)
        {
            SearchNode node = _root;
            for(int i = 0; i < key.Length; i++)
            {
                int idx = key[i] - 'a';
                if (node.children[idx] == null)
                {
                    node.children[idx] = new SearchNode();
                }

                node = node.children[idx];
            }

            Debug.Assert(node.result == null);
            node.result = val;
        }

        public void Load(string fname)
        {
            using (System.IO.StreamReader file = new System.IO.StreamReader(fname))
            {
                for (string buf; (buf = file.ReadLine()) != null;)
                {
                    string[] arr = buf.Split(';');
                    if (arr.Length > 1)
                    {
                        Add(arr[0], arr[1]);
                    }
                    else if(arr.Length > 0)
                    {
                        Add(arr[0], string.Empty);
                    }
                }
            }
        }
    }
}
