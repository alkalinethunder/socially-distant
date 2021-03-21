using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace RedTeam.IO
{
    public class FileSystem
    {
        private Node _root;

        private FileSystem(Node rootfs)
        {
            _root = rootfs;
        }

        private Node Resolve(string path)
        {
            var resolvedPath = PathUtils.Resolve(path);
            var parts = PathUtils.Split(resolvedPath);

            var node = _root;

            foreach (var part in parts)
            {
                if (node.CanList)
                {
                    var child = node.Children.FirstOrDefault(x => x.Name == part);
                    node = child;
                    if (node == null)
                        break;
                }
                else
                {
                    node = null;
                    break;
                }
            }
            
            return node;
        }

        public byte[] ReadAllBytes(string path)
        {
            var node = Resolve(path);
            if (node == null)
                throw new InvalidOperationException("File not found.");
            
            if (!node.CanRead)
                throw new InvalidOperationException("Is a directory.");

            using var s = node.Open();

            var arr = new byte[s.Length];
            s.Read(arr, 0, arr.Length);

            return arr;
        }

        public IEnumerable<string> GetDirectoryTree()
        {
            foreach (var node in _root.Collapse())
            {
                var parts = new List<string>();

                var p = node;
                while (p != null)
                {
                    parts.Insert(0, p.Name);
                    p = p.Parent;
                }
                
                yield return PathUtils.Combine(parts.ToArray());
            }
        }
        
        public string ReadAllText(string path)
        {
            var arr = ReadAllBytes(path);

            return Encoding.UTF8.GetString(arr);
        }
        
        public bool FileExists(string path)
        {
            var node = Resolve(path);
            return node != null && node.CanRead;
        }

        public bool DirectoryExists(string path)
        {
            var node = Resolve(path);
            return node != null && node.CanList;
        }

        public IEnumerable<string> GetFiles(string path)
        {
            var node = Resolve(path);

            if (node.CanList)
            {
                foreach (var child in node.Children)
                {
                    if (child.CanRead)
                    {
                        yield return PathUtils.Resolve(path, child.Name);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("File exists.");
            }
        }
        
        public IEnumerable<string> GetDirectories(string path)
        {
            var node = Resolve(path);

            if (node.CanList)
            {
                var resolved = PathUtils.Resolve(path);
                yield return PathUtils.Combine(resolved, PathUtils.CurrentDirectory);
                yield return PathUtils.Combine(resolved, PathUtils.ParentDirectory);
                
                
                foreach (var child in node.Children)
                {
                    if (child.CanList)
                    {
                        yield return PathUtils.Resolve(path, child.Name);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("File exists.");
            }
        }
        
        public static FileSystem FromHostOS()
        {
            var node = null as Node;

            if (RedTeamPlatform.IsPlatform(Platform.Windows))
            {
                node = new WindowsPseudoNode();
            }
            else
            {
                throw new InvalidOperationException("Cannot access the filesystem of this platform.");
            }
            
            return new FileSystem(node);
        }
    }
}