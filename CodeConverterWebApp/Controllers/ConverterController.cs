using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CodeConverterWebApp.Models;
using RefactoringEssentials.Converter;

namespace CodeConverterWebApp.Controllers
{
    public class ConverterController : ApiController
    {
        [HttpPost]
        public IHttpActionResult Post([FromBody]ConvertRequest todo)
        {
            var languages = todo.requestedConversion.Split('2');

            string fromLanguage = "C#";
            string toLanguage = "Visual Basic";
            int fromVersion = 6;
            int toVersion = 14;

            if (languages.Length == 2)
            {
                fromLanguage = ParseLanguage(languages[0]);
                fromVersion = GetDefaultVersionForLanguage(languages[0]);
                toLanguage = ParseLanguage(languages[1]);
                toVersion = GetDefaultVersionForLanguage(languages[1]);
            }

            var codeWithOptions = new CodeWithOptions(todo.code)
                .WithDefaultReferences()
                .SetFromLanguage(fromLanguage, fromVersion)
                .SetToLanguage(toLanguage, toVersion);
            var result = CodeConverter.Convert(codeWithOptions);

            var response = new ConvertResponse()
            {
                conversionOk = result.Success,
                convertedCode = result.ConvertedCode,
                errorMessage = result.GetExceptionsAsString()
            };

            return Ok(response);
        }

        string ParseLanguage(string language)
        {
            if ("cs".Equals(language, StringComparison.OrdinalIgnoreCase))
                return "C#";
            return "Visual Basic";
        }

        int GetDefaultVersionForLanguage(string language)
        {
            if ("cs".Equals(language, StringComparison.OrdinalIgnoreCase))
                return 6;
            // Visual Basic
            return 14;
        }
    }
}
