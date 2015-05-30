using System;
using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace RefactoringEssentials
{
#if NR6
    public
#endif
    class TypeGenerator
    {
        readonly static Type typeInfo;

        object instance;

        readonly static MethodInfo createArrayTypeSymbolMethod;
        readonly static MethodInfo createPointerTypeSymbolMethod;
        readonly static MethodInfo constructMethod;

        internal object Instance
        {
            get
            {
                return instance;
            }
        }

        static TypeGenerator()
        {
            typeInfo = Type.GetType("Microsoft.CodeAnalysis.CodeGeneration.TypeGenerator" + ReflectionNamespaces.WorkspacesAsmName, true);

            createArrayTypeSymbolMethod = typeInfo.GetMethod("CreateArrayTypeSymbol");
            createPointerTypeSymbolMethod = typeInfo.GetMethod("CreatePointerTypeSymbol");
            constructMethod = typeInfo.GetMethod("Construct");

        }

        public TypeGenerator()
        {
            instance = Activator.CreateInstance(typeInfo);
        }

        public ITypeSymbol CreateArrayTypeSymbol(ITypeSymbol elementType, int rank)
        {
            try
            {
                return (ITypeSymbol)createArrayTypeSymbolMethod.Invoke(instance, new object[] { elementType, rank });
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
                return (ITypeSymbol)createPointerTypeSymbolMethod.Invoke(instance, new object[] { pointedAtType });
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
                return (ITypeSymbol)constructMethod.Invoke(instance, new object[] { namedType, typeArguments });
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return null;
            }
        }
    }

}
