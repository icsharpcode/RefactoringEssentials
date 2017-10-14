namespace RefactoringEssentials.CSharp.Diagnostics
{
    /// <summary>
    /// The issue marker is used to set how an issue should be marked inside the text editor.
    /// </summary>
    public enum DiagnosticAnalyzerMarker
    {
        /// <summary>
        /// The issue is not shown inside the text editor. (But in the task bar)
        /// </summary>
        None,

        /// <summary>
        /// The region is marked as underline in the severity color.
        /// </summary>
        WavedLine,

        /// <summary>
        /// The region is marked as dotted line in the severity color.
        /// </summary>
        DottedLine,

        /// <summary>
        /// The text is grayed out.
        /// </summary>
        GrayOut
    }

}

