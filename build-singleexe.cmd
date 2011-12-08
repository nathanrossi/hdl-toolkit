@echo off

echo Setting up build environment
call "%VS100COMNTOOLS%\vsvars32.bat"

echo Building
msbuild hdltoolkit.sln /p:Configuration=Release

echo Copying to bin directory
mkdir bin
copy src\HDLToolkit.Console\bin\Release\hdltk.exe bin
copy src\HDLToolkit.Console\bin\Release\HDLToolkit.dll bin
copy src\HDLToolkit.Console\bin\Release\NConsole.dll bin

echo Creating Single Executable
echo Looking for ILMerge...
set ilmerge="%ProgramFiles(x86)%\Microsoft\Ilmerge\Ilmerge.exe"
if not exist %ilmerge% (
	echo ILMerge could not be located
	exit /b 1
)
echo Found ILMerge at %ilmerge%
%ilmerge% /out:bin\hdltk-se.exe bin\hdltk.exe bin\HDLToolkit.dll bin\NConsole.dll
echo Created Single Executable
