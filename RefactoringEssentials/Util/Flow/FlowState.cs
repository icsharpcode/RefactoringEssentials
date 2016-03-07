// (c) by Matt Warren. Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
// From https://github.com/mattwar/nullaby

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Nullaby
{
    public abstract class FlowState : IEquatable<FlowState>
    {
        /// <summary>
        /// Returns true if the two states are the same.
        /// </summary>
        public abstract bool Equals(FlowState state);

        /// <summary>
        /// Joins two states together.
        /// </summary>
        public abstract FlowState Join(FlowState state);

        /// <summary>
        /// Returns the state after the node has executed.
        /// </summary>
        public abstract FlowState After(SyntaxNode node);

        /// <summary>
        /// Returns true if the node contributes to branching states.
        /// </summary>
        public abstract bool IsConditional(SyntaxNode node);

        /// <summary>
        /// Return the states after the conditional node has executed.
        /// </summary>
        public abstract void AfterConditional(SyntaxNode node, out FlowState trueState, out FlowState falseState);
    }
}