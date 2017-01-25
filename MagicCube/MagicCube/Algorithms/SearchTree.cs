using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    public class SearchTree
    {
        const uint MOVE_COUNT = Faces.Count * 3;

        SearchNode _root = new SearchNode();

        class SearchNode
        {
            public SearchNode[] children = new SearchNode[MOVE_COUNT];
            public string result = null;

            public void Save(System.IO.StreamWriter file, string key)
            {
                if (result != null)
                {
                    file.WriteLine(key + ";" + result);
                }

                for (int i = 0; i < MOVE_COUNT; i++)
                {
                    if (children[i] != null)
                    {
                        children[i].Save(file, key + (char)('a' + i));
                    }
                }
            }
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

        public void Save(string fname)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fname))
            {
                _root.Save(file, string.Empty);
            }
        }

        //
        // Summary:
        //     Determines whether the SearchTree contains the specified key.
        //
        // Parameters:
        //   key:
        //     The key to locate in the SearchTree.
        //
        // Returns:
        //     true if the SearchTree contains an element with the specified key;
        //     otherwise, false.
        //
        // Exceptions:
        //     System.ArgumentNullException: key is null.
        public bool ContainsKey(string key)
        {
            string result = this[key];
            return result != null;
        }

        public string this[string key]
        {
            get
            {
                SearchNode node = _root;
                for (int i = 0; i < key.Length && node != null; i++)
                {
                    int idx = key[i] - 'a';
                    node = node.children[idx];
                }

                return node != null ? node.result : null;
            }

            set
            {
                Add(key, value);
            }
        }

        //
        // Summary:
        //     Removes all keys and values from the SearchTree.
        public void Clear()
        {
            _root = new SearchNode();
        }
    }
}
