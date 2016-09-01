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

namespace RoaringFangs.Utility
{
    [Serializable]
    public class Checklist<T>
    {
        public class CompleteEventArgs : EventArgs
        {
            public readonly IEnumerable<T> Items;

            public CompleteEventArgs(IEnumerable<T> items)
            {
                Items = items;
            }
        }

        public event EventHandler<CompleteEventArgs> Complete;

        private List<T> _IncompletedItems;
        private List<T> _CompletedItems;

        public IEnumerable<T> IncompletedItems
        {
            get { return _IncompletedItems; }
        }

        public IEnumerable<T> CompletedItems
        {
            get { return _CompletedItems; }
        }

        public Checklist(params T[] items)
        {
            _IncompletedItems = new List<T>(items);
            _CompletedItems = new List<T>();
        }

        public void CheckItem(T item)
        {
            _IncompletedItems.Remove(item);
            _CompletedItems.Add(item);
            if (_IncompletedItems.Count == 0)
            {
                if (Complete != null)
                {
                    var args = new CompleteEventArgs(CompletedItems);
                    Complete(this, args);
                }
            }
        }
    }
}