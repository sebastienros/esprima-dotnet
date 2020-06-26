#region Copyright (c) .NET Foundation and Contributors. All rights reserved.
//
// The MIT License (MIT)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
#endregion

using System;

namespace Esprima.Utils // System.Runtime.Serialization;
{
    // https://github.com/dotnet/corefx/blob/795957f4e18238ae7688ccab729f1ef8e825ab3a/src/System.Runtime.Serialization.Primitives/src/System/Runtime/Serialization/EnumMemberAttribute.cs

    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class EnumMemberAttribute : Attribute
    {
        private string _value = "";

        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                IsValueSetExplicitly = true;
            }
        }

        public bool IsValueSetExplicitly { get; set; }
    }
}
