#region MoreLINQ - Extensions to LINQ to Objects

//
// (C) 2008 Jonathan Skeet. Portions
// Portions (C) 2009 Atif Aziz, Chris Ammerman, Konrad Rudolph.
// Portions (C) 2010 Johannes Rudolph, Leopold Bushkin.
// Portions (C) 2015 Felipe Sateler, "sholland".
// Portions (C) 2016 Andreas Gullberg Larsen, Leandro F. Vieira (leandromoh).
// Portions (C) 2017 Jonas Nyrup (jnyrup).
// Portions (C) Microsoft. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Esprima.Tests
{
    internal class BreakingSequence<T> : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator()
        {
            throw new InvalidOperationException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal class BreakingCollection<T> : BreakingSequence<T>, ICollection<T>
    {
        protected readonly IList<T> List;

        public BreakingCollection(params T[] values) : this((IList<T>) values) { }

        public BreakingCollection(IList<T> list)
        {
            List = list;
        }

        public BreakingCollection(int count) :
            this(Enumerable.Repeat(default(T)!, count).ToList())
        {
        }

        public int Count => List.Count;

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            return List.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            List.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly => true;
    }

    internal class BreakingReadOnlyCollection<T> : BreakingSequence<T>, IReadOnlyCollection<T>
    {
        private readonly IReadOnlyCollection<T> _collection;

        public BreakingReadOnlyCollection(params T[] values) : this((IReadOnlyCollection<T>) values) { }

        public BreakingReadOnlyCollection(IReadOnlyCollection<T> collection)
        {
            _collection = collection;
        }

        public int Count => _collection.Count;
    }

    internal sealed class BreakingReadOnlyList<T> : BreakingReadOnlyCollection<T>, IReadOnlyList<T>
    {
        private readonly IReadOnlyList<T> _list;

        public BreakingReadOnlyList(params T[] values) : this((IReadOnlyList<T>) values) { }

        public BreakingReadOnlyList(IReadOnlyList<T> list) : base(list)
        {
            _list = list;
        }

        public T this[int index] => _list[index];
    }
}
