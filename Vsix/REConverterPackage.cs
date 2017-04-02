﻿//------------------------------------------------------------------------------
// <copyright file="ConvertCSToVBCommandPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace RefactoringEssentials.VsExtension
{
	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the
	/// IVsPackage interface and uses the registration attributes defined in the framework to
	/// register itself and its components with the shell. These attributes tell the pkgdef creation
	/// utility what data to put into .pkgdef file.
	/// </para>
	/// <para>
	/// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
	/// </para>
	/// </remarks>
	[PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0")] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string)]
    [ProvideOptionPage(typeof(GeneralOptionsDialogPage), "Refactoring Essentials", "General", 0, 0, true)]
    [Guid(REConverterPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class REConverterPackage : Package
    {
        /// <summary>
        /// ConvertCSToVBCommandPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "2bc6f609-6f6e-4c54-a908-791dd169911d";

        /// <summary>
        /// Initializes a new instance of package class.
        /// </summary>
        public REConverterPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            ConvertCSToVBCommand.Initialize(this);
            ConvertVBToCSCommand.Initialize(this);
            base.Initialize();
        }

        public bool DisableConverterInContextMenu
        {
            get
            {
                GeneralOptionsDialogPage page = (GeneralOptionsDialogPage)GetDialogPage(typeof(GeneralOptionsDialogPage));
                return page.HideConverterFromContextMenu;
            }
        }

        #endregion
    }
}
