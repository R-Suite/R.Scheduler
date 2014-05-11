SET OUTDIR=C:\GIT\RSuite\R.Scheduler\src\

@ECHO === === === === === === === ===

@ECHO ===NUGET Publishing ....

del *.nupkg

NuGet pack "%OUTDIR%R.Scheduler.Contracts\R.Scheduler.Contracts.nuspec"


nuget.exe push R.Scheduler.Contracts.0.0.0.1.nupkg
           
@ECHO === === === === === === === ===

PAUSE
