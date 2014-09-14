SET OUTDIR=C:\GIT\RSuite\R.Scheduler\src\

@ECHO === === === === === === === ===

@ECHO ===NUGET Publishing ....

del *.nupkg

NuGet pack "%OUTDIR%R.Scheduler\R.Scheduler.nuspec"
NuGet pack "%OUTDIR%R.Scheduler.Contracts\R.Scheduler.Contracts.nuspec"
NuGet pack "%OUTDIR%R.Scheduler.AssemblyPlugin\R.Scheduler.AssemblyPlugin.nuspec"


nuget.exe push R.Scheduler.0.0.0.26.nupkg
nuget.exe push R.Scheduler.Contracts.0.0.0.14.nupkg
nuget.exe push R.Scheduler.AssemblyPlugin.0.0.0.1.nupkg

           
@ECHO === === === === === === === ===

PAUSE
