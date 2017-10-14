using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace RefactoringEssentials
{
	[RoslynReflectionUsage(RoslynReflectionAllowedContext.CodeFixes)]
    class CaseCorrector
    {
        public static SyntaxAnnotation Annotation => (SyntaxAnnotation)RoslynReflection.CaseCorrector.AnnotationField.GetValue(null);

        public static Task<Document> CaseCorrectAsync(Document document, SyntaxAnnotation annotation, CancellationToken cancellationToken)
        {
            try
            {
                return (Task<Document>)RoslynReflection.CaseCorrector.CaseCorrectAsyncMethod.Invoke(null, new object[] { document, annotation, cancellationToken });
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return null;
            }
        }
    }
}