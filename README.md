# R.Scheduler
An experimental, easy to use job execution engine built on top of Quartz Enterprise Scheduler .NET. 
R.Scheduler is API driven. Actions can be performed using a simple RESTful API using JSON over HTTP.

## Project Maturity

R.Scheduler is used in a production environment with a limited number of jobs. It is still in relatively early stages of development and hasn’t been officially released. It may not yet be suitable for the most demanding and conservative projects.

Public (Web) API is relatively stable but minor changes are likely in future versions.

## Getting Started


#### Simple Configuration

Calling initialize with no parameters will create an instance of the Scheduler with default configuration options.

```c#
R.Scheduler.Scheduler.Initialize();

IScheduler sched = R.Scheduler.Scheduler.Instance();
sched.Start();
```

#### Custom Configuration

Initialize also takes a single lambda/action parameter for custom configuration.

```c#
R.Scheduler.Scheduler.Initialize(config =>
{
    config.CustomFtpLibraryAssemblyName = "MyFtpLib.dll";
    config.PersistanceStoreType = PersistanceStoreType.Postgre;
    config.ConnectionString = "Server=localhost;Port=5432;Database=Scheduler;User Id=xxx;Password=xxx;";
});

IScheduler sched = R.Scheduler.Scheduler.Instance();
sched.Start();
```


#### Supported Quartz.net Functionality

- Jobs: 
  - SendMailJob
- Triggers:
  - Simple Trigger
  - Cron Trigger
- Calendars:
  - Holiday Calendar
- Misfire Instructions
- DataStore:
  - SqlServer
  - Postgres

#### What does R.Scheduler add on top of top of Quartz.net?

- Jobs:
  - AssemblyPluginJob
  - FtpDownloadJob (Use Default Ftp library or inject your own)
  - WebRequestJob
- WebApi
- Auditing
- [WebManagement](https://github.com/R-Suite/R.Scheduler.Web)



#### Downloads

Download from NuGet 'R.Scheduler' and install into your Host application.

Download from NuGet 'R.Scheduler.Contracts' and install into your Client application(s).

[Search NuGet for R.Scheduler](http://nuget.org/packages?q=R.Scheduler)
