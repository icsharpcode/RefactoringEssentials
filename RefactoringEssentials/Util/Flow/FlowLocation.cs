// (c) by Matt Warren. Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
// From https://github.com/mattwar/nullaby

using System;
using Microsoft.CodeAnalysis;

namespace Nullaby
{
	public struct FlowLocation : IEquatable<FlowLocation>, IComparable<FlowLocation>
    {
        private SyntaxNode node;
        private bool isStart;

        public FlowLocation(SyntaxNode node, bool isStart = false)
        {
            this.node = node;
            this.isStart = isStart;
        }

        private int Location
        {
            get { return isStart ? node.SpanStart : node.Span.End; }
        }

        public int CompareTo(FlowLocation other)
        {
            if (this.node == other.node
                && this.isStart == other.isStart)
            {
                return 0;
            }
            else if (this.Location < other.Location)
            {
                return -1;
            }
            else if (this.Location > other.Location)
            {
                return 1;
            }
            else if (this.node == other.node)
            {
                // the same node but different ends
                return this.isStart ? -1 : 1;
            }
            else if (this.node.Contains(other.node))
            {
                // container nodes start before and end after contained nodes
                return this.isStart ? -1 : 1;
            }
            else if (other.node.Contains(this.node))
            {
                // container nodes start before and end after contained nodes
                return other.isStart ? 1 : -1;
            }
            else
            {
                return 0;
            }
        }

        public bool Equals(FlowLocation other)
        {
            return this.node == other.node
                && this.isStart == other.isStart;
        }

        public override bool Equals(object obj)
        {
            return obj is FlowLocation && this.Equals((FlowLocation)obj);
        }

        public override int GetHashCode()
        {
            return this.node.GetHashCode();
        }

        public static bool operator ==(FlowLocation a, FlowLocation b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(FlowLocation a, FlowLocation b)
        {
            return !a.Equals(b);
        }

        public static bool operator >(FlowLocation a, FlowLocation b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator >=(FlowLocation a, FlowLocation b)
        {
            return a.CompareTo(b) >= 0;
        }

        public static bool operator <(FlowLocation a, FlowLocation b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator <=(FlowLocation a, FlowLocation b)
        {
            return a.CompareTo(b) <= 0;
        }
    }

    public static class CodeLocationExtensions
    {
        public static FlowLocation StartLocation(this SyntaxNode node)
        {
            return new FlowLocation(node, isStart: true);
        }

        public static FlowLocation EndLocation(this SyntaxNode node)
        {
            return new FlowLocation(node);
        }
    }
}
