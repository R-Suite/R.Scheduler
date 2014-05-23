SET OUTDIR=C:\GIT\RSuite\R.Scheduler\src\

@ECHO === === === === === === === ===

@ECHO ===NUGET Publishing ....

del *.nupkg

NuGet pack "%OUTDIR%R.Scheduler\R.Scheduler.nuspec"


nuget.exe push R.Scheduler.0.0.0.8.nupkg
           
@ECHO === === === === === === === ===

PAUSE
