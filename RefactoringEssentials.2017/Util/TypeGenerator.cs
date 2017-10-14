using System;
using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace RefactoringEssentials
{
    [RoslynReflectionUsage(RoslynReflectionAllowedContext.CodeFixes)]
#if NR6
    public
#endif
    class TypeGenerator
    {
        object instance;

        internal object Instance
        {
            get
            {
                return instance;
            }
        }

        public TypeGenerator()
        {
            instance = Activator.CreateInstance(RoslynReflection.TypeGenerator.TypeInfo);
        }

        public ITypeSymbol CreateArrayTypeSymbol(ITypeSymbol elementType, int rank)
        {
            try
            {
                return (ITypeSymbol)RoslynReflection.TypeGenerator.CreateArrayTypeSymbolMethod.Invoke(instance, new object[] { elementType, rank });
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return null;
            }
        }

        public ITypeSymbol CreatePointerTypeSymbol(ITypeSymbol pointedAtType)
        {
            try
            {
                return (ITypeSymbol)RoslynReflection.TypeGenerator.CreatePointerTypeSymbolMethod.Invoke(instance, new object[] { pointedAtType });
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return null;
            }
        }

        public ITypeSymbol Construct(INamedTypeSymbol namedType, ITypeSymbol[] typeArguments)
        {
            try
            {
                return (ITypeSymbol)RoslynReflection.TypeGenerator.ConstructMethod.Invoke(instance, new object[] { namedType, typeArguments });
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return null;
            }
        }
    }

}
