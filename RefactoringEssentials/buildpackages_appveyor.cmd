rem @echo off

cd bin\Release

echo Building Analyzers NuGet package...
buildpackage.cmd
echo Building Library NuGet package...
buildlibrarypackage.cmd
