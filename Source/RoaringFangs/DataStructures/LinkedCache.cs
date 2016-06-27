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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoaringFangs.DataStructures
{
    public class LinkedCache<K, V>
    {
        private class LinkedElement
        {
            public K Self;
            public LinkedElement Next;
            public LinkedElement Previous;
        }
        private struct LinkMappedEntry
        {
            public V Data;
            public LinkedElement Link;
        }
        private int _CacheSizeLimit;
        private LinkedElement Head, Tail;
        private Dictionary<K, LinkMappedEntry> _KeyLinkMap;
        public LinkedCache(int cache_size_limit = 256)
        {
            _CacheSizeLimit = cache_size_limit;
            _KeyLinkMap = new Dictionary<K, LinkMappedEntry>();
        }

        public V Get(K key)
        {
            var entry = _KeyLinkMap[key];
            var link = entry.Link;
            // Extract
            if (link.Next != null)
                link.Next.Previous = link.Previous;
            if (link.Previous != null)
                link.Previous.Next = link.Next;
            // Put at head
            link.Next = Head;
            Head.Previous = link;
            Head = link;
            return entry.Data;
        }
        public void Set(K key, V value)
        {
            if (_KeyLinkMap.Count == _CacheSizeLimit)
            {
                _KeyLinkMap.Remove(Tail.Self);
                Tail = Tail.Previous;
                Tail.Next = null;
            }
            var new_head = new LinkedElement()
            {
                Self = key,
                Next = Head,
            };
            _KeyLinkMap[key] = new LinkMappedEntry()
            {
                Data = value,
                Link = new_head,
            };
            if(Head != null)
                Head.Previous = new_head;
            Head = new_head;
        }
        public bool Contains(K key)
        {
            return _KeyLinkMap.ContainsKey(key);
        }
    }
}
