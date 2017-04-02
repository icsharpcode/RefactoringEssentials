using System;
using System.IO;

namespace RefactoringEssentials.BuildTools
{
    enum BuildToolsMode
    {
        None,
        VersionPlaceholders
    }

    class BuildToolsParameters
    {
        public BuildToolsMode Mode { get; set; } = BuildToolsMode.None;
        public string SettingsFile { get; set; }
        public string TemplateFile { get; set; }
        public string TargetFile { get; set; }
        public bool IsDebug { get; set; }
        public string NuGetVersion { get; set; }
        public string FullVersion { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var parameters = ReadParameters(args);
            switch (parameters.Mode)
            {
                case BuildToolsMode.VersionPlaceholders:
                    ReplaceVersionPlaceholders(parameters);
                    break;

                default:
                    ShowHelp();
                    break;
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine();
            Console.WriteLine("Build Tool Options:");
            Console.WriteLine("    -version");
            Console.WriteLine("          -> Replaces version placeholders");
            Console.WriteLine("    -settings:<file>");
            Console.WriteLine("          -> Settings file with version numbers to replace placeholders.");
            Console.WriteLine("    -template:<file>");
            Console.WriteLine("          -> Template file (with placeholders)");
            Console.WriteLine("    -target:<file>");
            Console.WriteLine("          -> Target file to write result to.");
            Console.WriteLine();
            Console.WriteLine("Supported version placeholders in template files:");
            Console.WriteLine("    %%nuget_version%% - Value of 'nuget_version=<version>' setting in settings file.");
            Console.WriteLine("    %%full_version%% - Value of 'full_version=<version>' setting in settings file.");
            Console.WriteLine("    %%shorter_version%% - Same as %%full_version%%, but without 4th version part (revision)");
            Console.WriteLine();
        }

        static BuildToolsParameters ReadParameters(string[] args)
        {
            BuildToolsParameters parameters = new BuildToolsParameters();

            if (args != null)
            {
                foreach (string arg in args)
                {
                    string[] argParts = arg.Split(new char[] { ':' }, 2);

                    switch (argParts[0])
                    {
                        case "-version":
                            parameters.Mode = BuildToolsMode.VersionPlaceholders;
                            break;

                        case "-Debug":
                        case "-debug":
                            parameters.IsDebug = true;
                            break;

                        case "-settings":
                            if (argParts.Length > 1)
                            {
                                parameters.SettingsFile = argParts[1];
                            }
                            break;

                        case "-template":
                            if (argParts.Length > 1)
                            {
                                parameters.TemplateFile = argParts[1];
                            }
                            break;

                        case "-target":
                            if (argParts.Length > 1)
                            {
                                parameters.TargetFile = argParts[1];
                            }
                            break;
                    }
                }
            }

            if (parameters.Mode != BuildToolsMode.None)
            {
                // Read settings file
                if (File.Exists(parameters.SettingsFile))
                {
                    try
                    {
                        string[] settings = File.ReadAllLines(parameters.SettingsFile);
                        foreach (string setting in settings)
                        {
                            string[] settingParts = setting.Split(new char[] { '=' }, 2);

                            switch (settingParts[0])
                            {
                                case "nuget_version":
                                    if (settingParts.Length > 1)
                                    {
                                        parameters.NuGetVersion = settingParts[1];
                                    }
                                    break;

                                case "full_version":
                                    if (settingParts.Length > 1)
                                    {
                                        parameters.FullVersion = settingParts[1];
                                    }
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("BuildTool ERROR: Couldn't read '{0}'. Reason:{1}{1}{2}", parameters.SettingsFile, Environment.NewLine, ex.Message);
                    }
                }
            }

            return parameters;
        }

        static void ReplaceVersionPlaceholders(BuildToolsParameters parameters)
        {
            if (string.IsNullOrEmpty(parameters.TemplateFile))
            {
                Console.WriteLine("BuildTool ERROR: No template file specified.");
                return;
            }
            if (!File.Exists(parameters.TemplateFile))
            {
                Console.WriteLine("BuildTool ERROR: Template file '{0}' does not exist.", parameters.TemplateFile);
                return;
            }

            int version = parameters.IsDebug ? new Random().Next(65534) + 1 : 0;
            File.WriteAllText(parameters.TargetFile, File.ReadAllText(parameters.TemplateFile)
                .Replace("%%nuget_version%%", parameters.NuGetVersion ?? "%%nuget_version%%")
                .Replace("%%full_version%%", parameters.FullVersion ?? "%%full_version%%")
                .Replace("%%shorter_version%%", ExtractShorterVersion(parameters.FullVersion) ?? "%%shorter_version%%")
                .Replace("%%debug_version%%", version.ToString()));
        }

        static string ExtractShorterVersion(string fullVersion)
        {
            if (fullVersion == null)
                return null;

            Version version = new Version(fullVersion);
            return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
        }
    }
}
