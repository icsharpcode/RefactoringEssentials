using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace RefactoringEssentials
{
    // struct with two values
#if NR6
    public
#endif
    struct ValueTuple<T1, T2> : IEquatable<ValueTuple<T1, T2>>
    {
        private static readonly EqualityComparer<T1> s_comparer1 = EqualityComparer<T1>.Default;
        private static readonly EqualityComparer<T2> s_comparer2 = EqualityComparer<T2>.Default;

        public readonly T1 Item1;
        public readonly T2 Item2;

        public ValueTuple(T1 item1, T2 item2)
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }

        public bool Equals(ValueTuple<T1, T2> other)
        {
            return s_comparer1.Equals(this.Item1, other.Item1)
                && s_comparer2.Equals(this.Item2, other.Item2);
        }

        public override bool Equals(object obj)
        {
            if (obj is ValueTuple<T1, T2>)
            {
                var other = (ValueTuple<T1, T2>)obj;
                return this.Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Hash.Combine(s_comparer1.GetHashCode(Item1), s_comparer2.GetHashCode(Item2));
        }

        public static bool operator ==(ValueTuple<T1, T2> left, ValueTuple<T1, T2> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ValueTuple<T1, T2> left, ValueTuple<T1, T2> right)
        {
            return !left.Equals(right);
        }
    }
    static class Hash
    {
        /// <summary>
        /// This is how VB Anonymous Types combine hash values for fields.
        /// </summary>
        internal static int Combine(int newKey, int currentKey)
        {
            return unchecked((currentKey * (int)0xA5555529) + newKey);
        }

        internal static int Combine(bool newKeyPart, int currentKey)
        {
            return Combine(currentKey, newKeyPart ? 1 : 0);
        }

        /// <summary>
        /// This is how VB Anonymous Types combine hash values for fields.
        /// PERF: Do not use with enum types because that involves multiple
        /// unnecessary boxing operations.  Unfortunately, we can't constrain
        /// T to "non-enum", so we'll use a more restrictive constraint.
        /// </summary>
        internal static int Combine<T>(T newKeyPart, int currentKey) where T : class
        {
            int hash = unchecked(currentKey * (int)0xA5555529);

            if (newKeyPart != null)
            {
                return unchecked(hash + newKeyPart.GetHashCode());
            }

            return hash;
        }

        internal static int CombineValues<T>(IEnumerable<T> values, int maxItemsToHash = int.MaxValue)
        {
            if (values == null)
            {
                return 0;
            }

            var hashCode = 0;
            var count = 0;
            foreach (var value in values)
            {
                if (count++ >= maxItemsToHash)
                {
                    break;
                }

                // Should end up with a constrained virtual call to object.GetHashCode (i.e. avoid boxing where possible).
                if (value != null)
                {
                    hashCode = Hash.Combine(value.GetHashCode(), hashCode);
                }
            }

            return hashCode;
        }

        internal static int CombineValues<T>(T[] values, int maxItemsToHash = int.MaxValue)
        {
            if (values == null)
            {
                return 0;
            }

            var maxSize = Math.Min(maxItemsToHash, values.Length);
            var hashCode = 0;

            for (int i = 0; i < maxSize; i++)
            {
                T value = values[i];

                // Should end up with a constrained virtual call to object.GetHashCode (i.e. avoid boxing where possible).
                if (value != null)
                {
                    hashCode = Hash.Combine(value.GetHashCode(), hashCode);
                }
            }

            return hashCode;
        }

        internal static int CombineValues<T>(ImmutableArray<T> values, int maxItemsToHash = int.MaxValue)
        {
            if (values.IsDefaultOrEmpty)
            {
                return 0;
            }

            var hashCode = 0;
            var count = 0;
            foreach (var value in values)
            {
                if (count++ >= maxItemsToHash)
                {
                    break;
                }

                // Should end up with a constrained virtual call to object.GetHashCode (i.e. avoid boxing where possible).
                if (value != null)
                {
                    hashCode = Hash.Combine(value.GetHashCode(), hashCode);
                }
            }

            return hashCode;
        }

        internal static int CombineValues(IEnumerable<string> values, StringComparer stringComparer, int maxItemsToHash = int.MaxValue)
        {
            if (values == null)
            {
                return 0;
            }

            var hashCode = 0;
            var count = 0;
            foreach (var value in values)
            {
                if (count++ >= maxItemsToHash)
                {
                    break;
                }

                if (value != null)
                {
                    hashCode = Hash.Combine(stringComparer.GetHashCode(value), hashCode);
                }
            }

            return hashCode;
        }

        /// <summary>
        /// The generative factor used in the FNV-1a algorithm
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// </summary>
        internal const int FnvPrime = 16777619;

        /// <summary>
        /// Combine a char with an existing FNV-1a hash code
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// </summary>
        /// <param name="hashCode">The accumulated hash code</param>
        /// <param name="ch">The new character to combine</param>
        /// <returns>The result of combining <paramref name="hashCode"/> with <paramref name="ch"/> using the FNV-1a algorithm</returns>
        internal static int CombineFNVHash(int hashCode, char ch)
        {
            return unchecked((hashCode ^ ch) * Hash.FnvPrime);
        }
    }
}
