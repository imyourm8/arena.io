:: @echo off

set SCRIPT_DIR=%~dp0%
set PROTO_TOOLS=protobuf-tools
set PROTOGEN_PATH=%PROTO_TOOLS%\ProtoGen\
set PROTOFILES=ProtoFiles\
set PROTOLIB_PRJ=TapServer\ProtoLib
set MSBUILD = C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe

::compile .proto files to C# classes
for %%s in (%PROTOFILES%*.proto) do (
  Set "PROTO=%%~ns"
  %PROTOGEN_PATH%protogen.exe -i:%PROTOFILES%%%~ns.proto -o:%PROTOFILES%src\%%~ns.cs
)

set CONF=%1
::now copy every .cs file to ProtoLib project folder
xcopy /S /Y /i %SCRIPT_DIR%%PROTOFILES%src %SCRIPT_DIR%%PROTOLIB_PRJ%
::then run project build for Unity
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe %SCRIPT_DIR%%PROTOLIB_PRJ%\ProtoLib.csproj /p:Configuration=%CONF%
::copy built .dll to Precompiler
xcopy /S /Y /i %SCRIPT_DIR%%PROTOLIB_PRJ%\bin\%CONF% %SCRIPT_DIR%%PROTO_TOOLS%\Precompile\dll
::precompile serializer for unity 
pushd %PROTO_TOOLS%\Precompile
precompile.exe dll\ProtoLib.dll -o:ProtoSerializer.dll -t:ProtoSerializer /p:detectMissing
popd
::copy ProtoLib.dll and ProtoSerializer.dll to Unity Assets folder
echo %SCRIPT_DIR%\Assets\lib\Protobuf\%CONF%\ProtoLib.dll
copy %SCRIPT_DIR%%PROTO_TOOLS%\Precompile\dll\ProtoLib.dll %SCRIPT_DIR%\Assets\Plugins\Protobuf\ProtoLib.dll
copy %SCRIPT_DIR%%PROTO_TOOLS%\Precompile\ProtoSerializer.dll %SCRIPT_DIR%\Assets\Plugins\Protobuf\ProtoSerializer.dll
::remove everything
del %SCRIPT_DIR%%PROTO_TOOLS%\Precompile\ProtoSerializer.dll
pushd %SCRIPT_DIR%%PROTO_TOOLS%\Precompile\dll\
del /F /Q *.*
popd