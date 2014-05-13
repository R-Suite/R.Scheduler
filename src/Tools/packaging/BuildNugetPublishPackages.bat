SET OUTDIR=C:\GitHub\R.Scheduler\src\

@ECHO === === === === === === === ===

@ECHO ===NUGET Publishing ....

del *.nupkg

NuGet pack "%OUTDIR%R.Scheduler\R.Scheduler.nuspec"


nuget.exe push R.Scheduler.0.0.0.6.nupkg
           
@ECHO === === === === === === === ===

PAUSE
