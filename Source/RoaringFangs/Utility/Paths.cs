/*
The MIT License (MIT)

Copyright (c) 2016 Roaring Fangs Entertainment

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoaringFangs.Utility
{
    public static class Paths
    {
        public static readonly char[] DirectorySeparators =
            {
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar
            };
        public static readonly string DirectorySeparatorString =
            new String(Path.AltDirectorySeparatorChar, 1);

        public static string ResolvePath(string path_from, string path_relative)
        {
            var elements_relative = path_relative.Split(DirectorySeparators);
            var elements_from = path_from.Split(DirectorySeparators);
            var elements_all = elements_from.Concat(elements_relative);
            var path_stack = new Stack<string>();
            foreach (var element in elements_all)
            {
                switch (element)
                {
                    case ".":
                        break;
                    case "..":
                        path_stack.Pop();
                        break;
                    default:
                        path_stack.Push(element);
                        break;
                }
            }
            var elements_resolved = path_stack.Reverse().ToArray();
            return String.Join(DirectorySeparatorString, elements_resolved);
        }

        public static string GetRelativePath(string path_from, string path_to)
        {
            var elements_to = path_to.Split(DirectorySeparators);
            var elements_from = path_from.Split(DirectorySeparators);
            var path_stack = new Stack<string>();
            int elements_length_min = Math.Min(elements_to.Length, elements_from.Length);
            int start_index;
            for (start_index = 0; start_index < elements_length_min; start_index++)
                if (elements_to[start_index] != elements_from[start_index])
                    break;
            for (int i = start_index; i < elements_from.Length; i++)
                path_stack.Push("..");
            for (int i = start_index; i < elements_to.Length; i++)
                path_stack.Push(elements_to[i]);
            var elements_resolved = path_stack.Reverse().ToArray();
            return String.Join(DirectorySeparatorString, elements_resolved);
        }

        public static bool IsSubPath(string base_path, string sub_path)
        {
            return !String.IsNullOrEmpty(sub_path) && sub_path.StartsWith(base_path + Paths.DirectorySeparatorString);
        }
    }
}
