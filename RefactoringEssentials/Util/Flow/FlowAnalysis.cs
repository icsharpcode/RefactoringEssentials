// (c) by Matt Warren. Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
// From https://github.com/mattwar/nullaby

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Nullaby
{
	public class FlowAnalysis<TState>
        where TState : FlowState
    {
        private readonly ImmutableDictionary<FlowLocation, TState> states;
        private readonly FlowLocation[] locations;

        internal FlowAnalysis(ImmutableDictionary<FlowLocation, TState> states)
        {
            this.states = states;
            this.locations = this.states.Keys.OrderBy(k => k).ToArray();
        }

        public TState GetFlowState(SyntaxNode node, bool isStart = false)
        {
            return GetState(new FlowLocation(node, isStart: isStart));
        }

        public TState GetState(FlowLocation location)
        {
            int index = BinarySearchLowerBound(this.locations, location);
            var nearestLocation = this.locations[index];
            return this.states[nearestLocation];
        }

        private static int BinarySearchLowerBound(FlowLocation[] array, FlowLocation value)
        {
            int low = 0;
            int high = array.Length - 1;

            while (low <= high)
            {
                int middle = low + ((high - low) >> 1);
                if (array[middle].CompareTo(value) > 0)
                {
                    high = middle - 1;
                }
                else
                {
                    low = middle + 1;
                }
            }

            return high;
        }
    }
}