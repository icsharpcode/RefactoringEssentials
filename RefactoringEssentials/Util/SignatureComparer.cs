using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.CodeAnalysis;

namespace RefactoringEssentials
{
    [RoslynReflectionUsage(RoslynReflectionAllowedContext.CodeFixes)]
#if NR6
	public
#endif
    static class SignatureComparer
    {
        static Lazy<object> instance = new Lazy<object>(() =>
            RoslynReflection.SignatureComparer.TypeInfo.GetField("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null));
        static object Instance => instance.Value;

        public static bool HaveSameSignature(IList<IParameterSymbol> parameters1, IList<IParameterSymbol> parameters2)
        {
            try
            {
                return (bool)RoslynReflection.SignatureComparer.HaveSameSignatureMethod.Invoke(Instance, new object[] { parameters1, parameters2 });
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return false;
            }
        }

        public static bool HaveSameSignature(IPropertySymbol property1, IPropertySymbol property2, bool caseSensitive)
        {
            try
            {
                return (bool)RoslynReflection.SignatureComparer.HaveSameSignature2Method.Invoke(Instance, new object[] { property1, property2, caseSensitive });
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return false;
            }
        }

        public static bool HaveSameSignature(ISymbol symbol1, ISymbol symbol2, bool caseSensitive)
        {
            try
            {
                return (bool)RoslynReflection.SignatureComparer.HaveSameSignature3Method.Invoke(Instance, new object[] { symbol1, symbol2, caseSensitive });
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return false;
            }
        }

        public static bool HaveSameSignature(IMethodSymbol method1, IMethodSymbol method2, bool caseSensitive, bool compareParameterName = false, bool isParameterCaseSensitive = false)
        {
            try
            {
                return (bool)RoslynReflection.SignatureComparer.HaveSameSignature4Method.Invoke(Instance, new object[] { method1, method2, caseSensitive, compareParameterName, isParameterCaseSensitive });
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return false;
            }
        }

        public static bool HaveSameSignature(IList<IParameterSymbol> parameters1, IList<IParameterSymbol> parameters2, bool compareParameterName, bool isCaseSensitive)
        {
            try
            {
                return (bool)RoslynReflection.SignatureComparer.HaveSameSignature5Method.Invoke(Instance, new object[] { parameters1, parameters2, compareParameterName, isCaseSensitive });
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return false;
            }
        }

        public static bool HaveSameSignatureAndConstraintsAndReturnTypeAndAccessors(ISymbol symbol1, ISymbol symbol2, bool caseSensitive)
        {
            try
            {
                return (bool)RoslynReflection.SignatureComparer.HaveSameSignatureAndConstraintsAndReturnTypeAndAccessorsMethod.Invoke(Instance, new object[] { symbol1, symbol2, caseSensitive });
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return false;
            }
        }
    }
}