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

namespace RoaringFangs.Attributes
{
    /// <summary>
    /// Attribute for marking backing fields of properties so that their
    /// get/set accessors are invoked during in-editor serialization by calling
    /// <see cref="Editor.EditorUtilities.OnBeforeSerializeAutoProperties{TSelf}(TSelf)"/>
    /// </summary>
    /// <remarks>
    /// Only handles fields...for now...
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public class AutoPropertyAttribute : Attribute
    {
        public readonly string CorrespondingMemberName;
        public int _MemberValueHash;

        public AutoPropertyAttribute() :
            base()
        {
        }

        public AutoPropertyAttribute(string corresponding_member_name) :
            this()
        {
            CorrespondingMemberName = corresponding_member_name;
        }
    }
}