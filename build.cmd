:: the windows shell, so amazing

:: options
@echo Off
cd %~dp0
setlocal

:: determine cache dir
set NUGET_CACHE_DIR=%LocalAppData%\NuGet





:: download nuget to cache dir
set NUGET_URL="https://www.nuget.org/nuget.exe"
if not exist %NUGET_CACHE_DIR%\NuGet.exe (
  if not exist %NUGET_CACHE_DIR% md %NUGET_CACHE_DIR%
  echo Downloading latest version of NuGet.exe...
  @powershell -NoProfile -ExecutionPolicy unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest '%NUGET_URL%' -OutFile '%NUGET_CACHE_DIR%\NuGet.exe'"
)

:: copy nuget locally
if not exist .nuget\NuGet.exe (
  if not exist .nuget md .nuget
  copy %NUGET_CACHE_DIR%\NuGet.exe .nuget\NuGet.exe > nul
)

:: restore packages
.nuget\NuGet.exe restore ScriptCs.Testing.ScriptPacks.sln

:: build solution
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild ScriptCs.Testing.ScriptPacks.sln /property:Configuration=Release /nologo /maxcpucount /verbosity:minimal %* /fileLogger /fileloggerparameters:LogFile=artifacts\msbuild.log;Verbosity=normal;Summary /nodeReuse:false
