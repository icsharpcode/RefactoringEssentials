// Copyright (c) 2009-2013 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RefactoringEssentials.Xml
{
	/// <summary>
	/// Represents an element in the XML documentation.
	/// Any occurrences of "&lt;inheritdoc/>" are replaced with the inherited documentation.
	/// </summary>
	public class XmlDocumentationElement
	{
		/// <summary>
		/// Gets the XML documentation element for the specified entity.
		/// Returns null if no documentation is found.
		/// </summary>
        public static XmlDocumentationElement Get(ISymbol entity, bool inheritDocIfMissing = true)
		{
			if (entity == null)
				return null;
            var documentationComment = entity.GetDocumentationCommentXml();
			if (documentationComment != null) {
				return Create(documentationComment, entity);
			}
			
            var member = entity as ISymbol;
			if (inheritDocIfMissing && member != null) {
                var method = member as IMethodSymbol;
                if (method != null && method.MethodKind == MethodKind.Constructor) {
					// For constructors, the documentation of the base class ctor
					// isn't really suitable as constructors are not inherited.
					// We'll use the type's documentation instead:
                    return Get(entity.ContainingType, inheritDocIfMissing);
				}
                // TODO
//            foreach (var baseMember in InheritanceHelper.GetBaseMembers(member, includeImplementedInterfaces: true)) {
//	documentationComment = baseMember.Documentation;
//	if (documentationComment != null)
//		return Create(documentationComment, baseMember);
//}
			}
			return null;
		}
		
        static XmlDocumentationElement Create(string documentationComment, ISymbol declaringEntity)
		{
            var doc = new AXmlParser().Parse(SourceText.From(documentationComment));
            return new XmlDocumentationElement(doc, declaringEntity);
		}
		
		readonly AXmlObject xmlObject;
		readonly AXmlElement element;
        readonly ISymbol declaringEntity;
		volatile string textContent;
		
		/// <summary>
		/// Inheritance level; used to prevent cyclic doc inheritance.
		/// </summary>
		int nestingLevel;
		
		/// <summary>
		/// Creates a new documentation element.
		/// </summary>
        public XmlDocumentationElement(AXmlElement element, ISymbol declaringEntity)
		{
			if (element == null)
				throw new ArgumentNullException("element");
			this.element = element;
			this.xmlObject = element;
			this.declaringEntity = declaringEntity;
		}
		
		/// <summary>
		/// Creates a new documentation element.
		/// </summary>
		public XmlDocumentationElement(AXmlDocument document, ISymbol declaringEntity)
		{
			if (document == null)
				throw new ArgumentNullException("document");
			this.xmlObject = document;
			this.declaringEntity = declaringEntity;
		}
		
		/// <summary>
		/// Creates a new documentation element.
		/// </summary>
		public XmlDocumentationElement(string text, ISymbol declaringEntity)
		{
			if (text == null)
				throw new ArgumentNullException("text");
			this.declaringEntity = declaringEntity;
			this.textContent = text;
		}
		
		/// <summary>
		/// Gets the entity on which this documentation was originally declared.
		/// May return null.
		/// </summary>
		public ISymbol DeclaringEntity {
			get { return declaringEntity; }
		}
		
		/// <summary>
		/// Gets the element name.
		/// </summary>
		public string Name {
			get {
				return element != null ? element.Name : string.Empty;
			}
		}
		
		/// <summary>
		/// Gets the attribute value.
		/// </summary>
		public string GetAttribute(string name)
		{
			return element != null ? element.GetAttributeValue(name) : string.Empty;
		}
		
		/// <summary>
		/// Gets whether this is a pure text node.
		/// </summary>
		public bool IsTextNode {
			get { return xmlObject == null; }
		}
		
		/// <summary>
		/// Gets the text content.
		/// </summary>
		public string TextContent {
			get {
				if (textContent == null) {
					StringBuilder b = new StringBuilder();
					foreach (var child in this.Children)
						b.Append(child.TextContent);
					textContent = b.ToString();
				}
				return textContent;
			}
		}
		
		IList<XmlDocumentationElement> children;
		
		/// <summary>
		/// Gets the child elements.
		/// </summary>
		public IList<XmlDocumentationElement> Children {
			get {
				if (xmlObject == null)
					return new XmlDocumentationElement[0];
				return LazyInitializer.EnsureInitialized(
					ref this.children,
					() => CreateElements(xmlObject.Children, declaringEntity, nestingLevel));
			}
		}
		
		static readonly string[] doNotInheritIfAlreadyPresent = {
			"example", "exclude", "filterpriority", "preliminary", "summary",
			"remarks", "returns", "threadsafety", "value"
		};
		
		static List<XmlDocumentationElement> CreateElements(IEnumerable<AXmlObject> childObjects, ISymbol declaringEntity, int nestingLevel)
		{
			List<XmlDocumentationElement> list = new List<XmlDocumentationElement>();
			foreach (var child in childObjects) {
				var childText = child as AXmlText;
				var childTag = child as AXmlTag;
				var childElement = child as AXmlElement;
				if (childText != null) {
					list.Add(new XmlDocumentationElement(childText.Value, declaringEntity));
				} else if (childTag != null && childTag.IsCData) {
					foreach (var text in childTag.Children.OfType<AXmlText>())
						list.Add(new XmlDocumentationElement(text.Value, declaringEntity));
				} else if (childElement != null) {
					if (nestingLevel < 5 && childElement.Name == "inheritdoc") {
						string cref = childElement.GetAttributeValue("cref");
						ISymbol inheritedFrom = null;
                        string inheritedDocumentation = null;
						if (cref != null) {
							//inheritedFrom = crefResolver(cref);
							//if (inheritedFrom != null)
                            //    inheritedDocumentation = inheritedFrom.GetDocumentationCommentXml();
						} else {
       //                     foreach (var baseMember in InheritanceHelper.GetBaseMembers((IMember)declaringEntity, includeImplementedInterfaces: true)) {
							//	inheritedDocumentation = baseMember.Documentation;
							//	if (inheritedDocumentation != null) {
							//		inheritedFrom = baseMember;
							//		break;
							//	}
							//}
						}
						
                        if (inheritedDocumentation != null) {
                            var doc = new AXmlParser().Parse(SourceText.From (inheritedDocumentation));
							
							// XPath filter not yet implemented
							if (childElement.Parent is AXmlDocument && childElement.GetAttributeValue("select") == null) {
								// Inheriting documentation at the root level
								List<string> doNotInherit = new List<string>();
								doNotInherit.Add("overloads");
								doNotInherit.AddRange(childObjects.OfType<AXmlElement>().Select(e => e.Name).Intersect(
									doNotInheritIfAlreadyPresent));
								
								var inheritedChildren = doc.Children.Where(
									inheritedObject => {
										AXmlElement inheritedElement = inheritedObject as AXmlElement;
										return !(inheritedElement != null && doNotInherit.Contains(inheritedElement.Name));
									});
								
								list.AddRange(CreateElements(inheritedChildren, inheritedFrom, nestingLevel + 1));
							}
						}
					} else {
						list.Add(new XmlDocumentationElement(childElement, declaringEntity) { nestingLevel = nestingLevel });
					}
				}
			}
			if (list.Count > 0 && list[0].IsTextNode) {
				if (string.IsNullOrWhiteSpace(list[0].textContent))
					list.RemoveAt(0);
				else
					list[0].textContent = list[0].textContent.TrimStart();
			}
			if (list.Count > 0 && list[list.Count - 1].IsTextNode) {
				if (string.IsNullOrWhiteSpace(list[list.Count - 1].textContent))
					list.RemoveAt(list.Count - 1);
				else
					list[list.Count - 1].textContent = list[list.Count - 1].textContent.TrimEnd();
			}
			return list;
		}
		
		/// <inheritdoc/>
		public override string ToString()
		{
			if (element != null)
				return "<" + element.Name + ">";
			else
				return this.TextContent;
		}
	}
}
