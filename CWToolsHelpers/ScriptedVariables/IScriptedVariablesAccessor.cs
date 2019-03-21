using System;
using System.Collections.Generic;
using CWToolsHelpers.FileParsing;

namespace CWToolsHelpers.ScriptedVariables {
    public interface IScriptedVariablesAccessor : IDisposable{
        /// <summary>
        /// If the raw value refers to a paradox variable, attempts to retrieve it from the available variables, or returns the raw value.
        /// </summary>
        /// <remarks>
        /// It is always safe to call this method, as if the raw value is a variable it will return the variable value, otherwise it will return the raw value itself.
        /// </remarks>
        /// <param name="rawValue">The value extracted from a paradox file, which may or may not be a variable</param>
        /// <returns>The looked up scripted variable value if the <c>rawValue</c> is a variable, otherwise the <c>rawValue</c></returns>
        string GetPotentialValue(string rawValue);

        IScriptedVariablesAccessor CreateNew(IEnumerable<CWKeyValue> keyValues);

        void AddAdditionalFileVariables(CWNode node);
    }
}