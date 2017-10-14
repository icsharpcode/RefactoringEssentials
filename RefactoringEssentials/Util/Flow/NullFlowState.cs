// (c) by Matt Warren. Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
// From https://github.com/mattwar/nullaby
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactoringEssentials;

namespace Nullaby
{
	internal enum NullState
    {
        Unknown,
        Null,
        NotNull,
        CouldBeNull,
        ShouldNotBeNull
    }

    internal class NullFlowState : FlowState
    {
        private readonly SemanticModel model;
        private readonly ImmutableDictionary<object, NullState> variableStates;

        public NullFlowState(SemanticModel model)
            : this(model, ImmutableDictionary.Create<object, NullState>(new VariableComparer(model)))
        {
        }

        private NullFlowState(SemanticModel model, ImmutableDictionary<object, NullState> variableStates)
        {
            this.model = model;
            this.variableStates = variableStates;
        }

        private NullFlowState With(ImmutableDictionary<object, NullState> newVariableStates)
        {
            if (this.variableStates != newVariableStates)
            {
                return new NullFlowState(this.model, newVariableStates);
            }
            else
            {
                return this;
            }
        }

        public override bool Equals(FlowState state)
        {
            var nfs = state as NullFlowState;
            return nfs != null && nfs.variableStates == this.variableStates;
        }

        public override FlowState Join(FlowState state)
        {
            var nfs = (NullFlowState)state;
            var joinedVariableStates = this.variableStates;

            Join(this.variableStates, nfs.variableStates, ref joinedVariableStates);
            Join(nfs.variableStates, this.variableStates, ref joinedVariableStates);

            return this.With(joinedVariableStates);
        }

        private void Join(
            ImmutableDictionary<object, NullState> branchA,
            ImmutableDictionary<object, NullState> branchB,
            ref ImmutableDictionary<object, NullState> joined)
        {
            // for all items in a
            foreach (var kvp in branchA)
            {
                NullState bs;
                if (!branchB.TryGetValue(kvp.Key, out bs))
                {
                    bs = GetDeclaredState(kvp.Key);
                }

                var w = Join(kvp.Value, bs);

                joined = joined.SetItem(kvp.Key, w);
            }
        }

        private NullState Join(NullState a, NullState b)
        {
            switch (a)
            {
                case NullState.Unknown:
                    switch (b)
                    {
                        case NullState.Unknown:
                        case NullState.NotNull:
                        case NullState.ShouldNotBeNull:
                        return NullState.Unknown;
                        case NullState.Null:
                        case NullState.CouldBeNull:
                        return NullState.CouldBeNull;
                    }
                break;

                case NullState.CouldBeNull:
                return NullState.CouldBeNull;

                case NullState.ShouldNotBeNull:
                    switch (b)
                    {
                        case NullState.ShouldNotBeNull:
                        case NullState.NotNull:
                        return NullState.ShouldNotBeNull;

                        case NullState.Unknown:
                        return NullState.Unknown;

                        case NullState.CouldBeNull:
                        case NullState.Null:
                        return NullState.CouldBeNull;
                    }
                break;

                case NullState.Null:
                    switch (b)
                    {
                        case NullState.Unknown:
                        case NullState.CouldBeNull:
                        case NullState.ShouldNotBeNull:
                        case NullState.NotNull:
                        return NullState.CouldBeNull;

                        case NullState.Null:
                        return NullState.Null;
                    }
                break;

                case NullState.NotNull:
                    switch (b)
                    {
                        case NullState.Unknown:
                        return NullState.Unknown;
                        case NullState.ShouldNotBeNull:
                        return NullState.ShouldNotBeNull;
                        case NullState.NotNull:
                        return NullState.NotNull;
                        case NullState.CouldBeNull:
                        case NullState.Null:
                        return NullState.CouldBeNull;
                    }
                break;
            }

            return NullState.Unknown;
        }

        public override FlowState After(SyntaxNode node)
        {
            // variables can change state after assignment
            var assign = node as AssignmentExpressionSyntax;
            if (assign != null)
            {
                return this.WithReferenceState(assign.Left, this.GetReferenceState(assign.Right));
            }

            // variables acquire initial state from initializer
            var declarator = node as VariableDeclaratorSyntax;
            if (declarator != null)
            {
                if (declarator.Initializer != null)
                {
                    var symbol = this.model.GetDeclaredSymbol(node);
                    if (symbol != null)
                    {
                        return this.WithReferenceState(symbol, GetReferenceState(declarator.Initializer.Value));
                    }
                }
            }

            if (this.IsConditional(node))
            {
                FlowState trueState;
                FlowState falseState;
                this.AfterConditional(node, out trueState, out falseState);
                return trueState.Join(falseState);
            }

            return this;
        }

        public override bool IsConditional(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.EqualsExpression:
                case SyntaxKind.NotEqualsExpression:
                return true;
                default:
                return false;
            }
        }

        public override void AfterConditional(SyntaxNode node, out FlowState trueState, out FlowState falseState)
        {
            trueState = this;
            falseState = this;

            var kind = node.Kind();
            if (kind == SyntaxKind.EqualsExpression || kind == SyntaxKind.NotEqualsExpression)
            {
                var binop = (BinaryExpressionSyntax)node;

                ExpressionSyntax influencedExpr = null;
                if (binop.Right.IsKind(SyntaxKind.NullLiteralExpression))
                {
                    influencedExpr = this.GetVariableExpression(binop.Left);
                }
                else if (binop.Left.IsKind(SyntaxKind.NullLiteralExpression))
                {
                    influencedExpr = this.GetVariableExpression(binop.Right);
                }

                if (influencedExpr != null)
                { 
                    if (kind == SyntaxKind.EqualsExpression)
                    {
                        trueState = this.WithReferenceState(influencedExpr, NullState.Null);
                        falseState = this.WithReferenceState(influencedExpr, NullState.NotNull);
                    }
                    else
                    {
                        trueState = this.WithReferenceState(influencedExpr, NullState.NotNull);
                        falseState = this.WithReferenceState(influencedExpr, NullState.Null);
                    }
                }
            }
        }

        public NullFlowState WithReferenceState(ISymbol symbol, NullState state)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Local:
                case SymbolKind.Parameter:
                case SymbolKind.RangeVariable:
                return this.With(this.variableStates.SetItem(symbol, state));
                default:
                return this;
            }
        }

        public NullFlowState WithReferenceState(ExpressionSyntax expr, NullState state)
        {
            var variable = GetVariableExpression(expr);
            if (variable != null)
            {
                return this.With(this.variableStates.SetItem(variable, state));
            }

            return this;
        }

        /// <summary>
        /// Returns the portion of the expression that represents the variable
        /// that can be tracked, or null if the expression is not trackable.
        /// </summary>
        private ExpressionSyntax GetVariableExpression(ExpressionSyntax expr)
        {
            expr = WithoutParens(expr);

            switch (expr.Kind())
            {
                // assignment expressions yield their LHS variable for tracking
                // this comes into play during null checks: (x = y) != null
                // in this case x can be assigned tested-not-null state.. (what about y?)
                case SyntaxKind.SimpleAssignmentExpression:
                return GetVariableExpression(((BinaryExpressionSyntax)expr).Left);

                    // all dotted names are trackable.
                case SyntaxKind.SimpleMemberAccessExpression:
                case SyntaxKind.PointerMemberAccessExpression:
                case SyntaxKind.QualifiedName:
                case SyntaxKind.IdentifierName:
                case SyntaxKind.AliasQualifiedName:
                return expr;

                default:
                return null;
            }
        }

        private ExpressionSyntax WithoutParens(ExpressionSyntax expr)
        {
            while (expr.IsKind(SyntaxKind.ParenthesizedExpression))
            {
                expr = ((ParenthesizedExpressionSyntax)expr).Expression;
            }

            return expr;
        }

        public NullState GetAssignmentState(ExpressionSyntax variable, bool isInvocationParameter = false)
        {
            var symbol = this.model.GetSymbolInfo(variable).Symbol;
            if (symbol != null)
            {
                return GetAssignmentState(symbol, isInvocationParameter);
            }
            else
            {
                return NullState.Unknown;
            }
        }

        public NullState GetAssignmentState(ISymbol symbol, bool isInvocationParameter = false)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Local:
                return NullState.Unknown;
                case SymbolKind.Parameter:
                    if (!isInvocationParameter)
                    {
                        // method body parameters get their state assigned just like locals
                        return NullState.Unknown;
                    }
                    else
                    {
                        goto default;
                    }
                default:
                return GetDeclaredState(symbol);
            }
        }

        public NullState GetReferenceState(ExpressionSyntax expression)
        {
            if (expression != null)
            {
                expression = WithoutParens(expression);

                NullState state;
                if (this.variableStates.TryGetValue(expression, out state))
                {
                    return state;
                }

                switch (expression.Kind())
                {
                    case SyntaxKind.NullLiteralExpression:
                    return NullState.Null;

                    case SyntaxKind.StringLiteralExpression:
                    case SyntaxKind.ArrayCreationExpression:
                        return NullState.NotNull;

                    case SyntaxKind.ObjectCreationExpression:
                        var creationExprType = model.GetTypeInfo(expression).Type;
                        if ((creationExprType != null) && creationExprType.IsNullable())
                            return NullState.CouldBeNull;
                        return NullState.NotNull;

                    case SyntaxKind.ConditionalAccessExpression:
                        var ca = (ConditionalAccessExpressionSyntax)expression;
                        var exprState = GetReferenceState(ca.Expression);
                        switch (GetReferenceState(ca.Expression))
                        {
                            case NullState.Null:
                            return NullState.Null;
                            case NullState.CouldBeNull:
                            case NullState.Unknown:
                            return NullState.CouldBeNull;
                            default:
                            return GetDeclaredState(ca.WhenNotNull);
                        }

                    case SyntaxKind.CoalesceExpression:
                        var co = (BinaryExpressionSyntax)expression;
                    return GetReferenceState(co.Right);
                }

                var symbol = this.model.GetSymbolInfo(expression).Symbol;
                if (symbol != null)
                {
                    return GetReferenceState(symbol);
                }
            }

            return NullState.Unknown;
        }

        public NullState GetReferenceState(ISymbol symbol)
        {
            NullState state;
            if (this.variableStates.TryGetValue(symbol, out state))
            {
                return state;
            }

            return GetDeclaredState(symbol);
        }

        public NullState GetDeclaredState(object symbolOrSyntax)
        {
            var syntax = symbolOrSyntax as ExpressionSyntax;
            if (syntax != null)
            {
                return GetDeclaredState(syntax);
            }

            var symbol = symbolOrSyntax as ISymbol;
            if (symbol != null)
            {
                return GetDeclaredState(symbol);
            }

            return NullState.Unknown;
        }

        public NullState GetDeclaredState(ExpressionSyntax syntax)
        {
            var symbol = this.model.GetSymbolInfo(syntax).Symbol;
            if (symbol != null)
            {
                return GetDeclaredState(symbol);
            }
            else
            {
                return NullState.Unknown;
            }
        }

        public static NullState GetDeclaredState(ISymbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Local:
                return NullState.Unknown;

                default:
                return GetSymbolInfo(symbol).NullState;
            }
        }

        private static bool TryGetAttributedState(ImmutableArray<AttributeData> attrs, out NullState state)
        {
            foreach (var a in attrs)
            {
                if (a.AttributeClass.Name == "ShouldNotBeNullAttribute")
                {
                    state = NullState.ShouldNotBeNull;
                    return true;
                }
                else if (a.AttributeClass.Name == "CouldBeNullAttribute")
                {
                    state = NullState.CouldBeNull;
                    return true;
                }
            }

            state = NullState.Unknown;
            return false;

        }

        private class SymbolInfo
        {
            public readonly NullState NullState;

            public SymbolInfo(NullState defaultState)
            {
                this.NullState = defaultState;
            }
        }

        private static ConditionalWeakTable<ISymbol, SymbolInfo> symbolInfos
        = new ConditionalWeakTable<ISymbol, SymbolInfo>();

        private static SymbolInfo GetSymbolInfo(ISymbol symbol)
        {
            SymbolInfo info;
            if (!symbolInfos.TryGetValue(symbol, out info))
            {
                info = CreateSymbolInfo(symbol);
                info = symbolInfos.GetValue(symbol, _ => info);
            }

            return info;
        }

        private static SymbolInfo CreateSymbolInfo(ISymbol symbol)
        {
            // check if it can possibly be null
            var type = GetVariableType(symbol);
            if (type != null)
            {
                var possibleToBeNull = type.IsReferenceType
                                           || type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
                                           || (symbol as IMethodSymbol)?.MethodKind == MethodKind.BuiltinOperator;
                if (!possibleToBeNull)
                {
                    return new SymbolInfo(NullState.NotNull);
                }
            }

            // check any explicit attributes
            ImmutableArray<AttributeData> attrs;
            switch (symbol.Kind)
            {
                case SymbolKind.Method:
                    attrs = ((IMethodSymbol)symbol).GetReturnTypeAttributes();
                break;

                default:
                    attrs = symbol.GetAttributes();
                break;
            }

            NullState state;
            if (!TryGetAttributedState(attrs, out state))
            {
                // if defaulted to null, then obviously it could be null!
                if (symbol.Kind == SymbolKind.Parameter)
                {
                    var ps = (IParameterSymbol)symbol;
                    if (ps.HasExplicitDefaultValue && ps.ExplicitDefaultValue == null)
                    {
                        return new SymbolInfo(NullState.CouldBeNull);
                    }
                }

                // otherwise try to pickup default from assembly level attribute
                if (symbol.Kind != SymbolKind.Assembly)
                {
                    state = GetSymbolInfo(symbol.ContainingAssembly).NullState;
                }
            }

            return new SymbolInfo(state);
        }

        private static ITypeSymbol GetVariableType(ISymbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Parameter:
                return ((IParameterSymbol)symbol).Type;

                case SymbolKind.Local:
                return ((ILocalSymbol)symbol).Type;

                case SymbolKind.Field:
                return ((IFieldSymbol)symbol).Type;

                case SymbolKind.Property:
                return ((IPropertySymbol)symbol).Type;

                case SymbolKind.Method:
                return ((IMethodSymbol)symbol).ReturnType;

                default:
                return null;
            }
        }

        internal class VariableComparer : IEqualityComparer<object>
        {
            private readonly SemanticModel model;

            public VariableComparer(SemanticModel model)
            {
                this.model = model;
            }

            public new bool Equals(object x, object y)
            {
                if (x == y)
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                var xs = x as ISymbol;
                var ys = y as ISymbol;

                var xn = x as ExpressionSyntax;
                var yn = y as ExpressionSyntax;

                if (xs == null && xn != null)
                {
                    xs = this.model.GetSymbolInfo(xn).Symbol;
                }

                if (ys == null && yn != null)
                {
                    ys = this.model.GetSymbolInfo(yn).Symbol;
                }

                if (xs.Equals(ys))
                {
                    // don't need to compare syntax to match these (or static symbols)
                    if (xs.Kind == SymbolKind.Local || xs.Kind == SymbolKind.Parameter || xs.Kind == SymbolKind.RangeVariable || xs.IsStatic)
                    {
                        return true;
                    }

                    // syntax must be similar to be confident this reaches the same instance
                    return xn != null && yn != null && SyntaxFactory.AreEquivalent(xn, yn, topLevel: false);
                }

                return false;
            }

            public int GetHashCode(object obj)
            {
                // hash code is based on symbol's hash code
                var sym = obj as ISymbol;
                var exp = obj as ExpressionSyntax;

                if (sym == null && exp != null)
                {
                    sym = this.model.GetSymbolInfo(exp).Symbol;
                }

                if (sym != null)
                {
                    return sym.GetHashCode();
                }

                return obj.GetHashCode();
            }
        }
    }
}