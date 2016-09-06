**Version 1.2.4 adds support for:**
- **Token-based authentication** (No default implementation provided, you must implement ```R.Scheduler.Interfaces.IAuthorize``` in your custom assembly and register the assembly with the Scheduler at start-up. _See R.Scheduler.TestHost.Program.cs_)
- ConnectionString **encryption** for SqlJob and username/password **encryption** for FtpDownloadJob. (Add ```<add key="FeatureToggle.EncryptionFeatureToggle" value="true" />``` and ```<add key="SchedulerEncryptionKey" value="{Convert.ToBase64String(R.Scheduler.Core.AESGCM.NewKey())}" />``` to your app settings.

# R.Scheduler
An experimental, easy to use job execution engine built on top of Quartz Enterprise Scheduler .NET. 
R.Scheduler is API driven. Actions can be performed using a simple RESTful API using JSON over HTTP.

## Project Maturity

R.Scheduler is used in a production environment with a couple of thousands of jobs running daily. It is relatively matured but may not yet be suitable for the most demanding and conservative projects.

Public (Web) API is relatively stable but minor changes are likely in future versions.

## Getting Started

#### Setup Database

Create a set of database **tables for Quartz.NET**. Use table-creation SQL scripts in   
https://github.com/R-Suite/R.Scheduler/blob/master/database/quartz/tables_postgres.sql or   
https://github.com/R-Suite/R.Scheduler/blob/master/database/quartz/tables_sqlServer.sql  

Create a set of database **tables for R.Scheduler**. Use table-creation SQL scripts in  
https://github.com/R-Suite/R.Scheduler/blob/master/database/rscheduler/tables_postgres.sql or     
https://github.com/R-Suite/R.Scheduler/blob/master/database/rscheduler/tables_sqlServer.sql  


#### Simple Configuration

Calling initialize with no parameters will **create and start** an instance of the Scheduler with default configuration options.

```c#
public class Program
{
    private static void Main(string[] args)
    {
        R.Scheduler.Scheduler.Initialize();
    }
}
```

#### Custom Configuration

Initialize also takes a single lambda/action parameter for custom configuration. In this case we choose not to start the Scheduler automatically. Instead, we create a scheduler instance and start the instance explicitly after the Scheduler initialization.

```c#
public class Program
{
    private static void Main(string[] args)
    {
        R.Scheduler.Scheduler.Initialize(config =>
        {
            config.AutoStart = false;
            config.CustomFtpLibraryAssemblyName = "MyFtpLib.dll";
            config.PersistanceStoreType = PersistanceStoreType.Postgre;
            config.ConnectionString = "Server=localhost;Port=5432;Database=Scheduler;User Id=xxx;Password=xxx;";
        });

        IScheduler sched = R.Scheduler.Scheduler.Instance();
        sched.Start();
    }
}
```

#### Create New [WebRequest] Job

```c#
POST /api/webRequests
{
    "JobName": "MyJob",
    "ActionType": "HTTP",
    "Method": "POST",
    "Uri": "http://localhost:6001/api/myEndpoint",
    "Body": "",
    "ContentType": "application/json"
}
```
The result of the above operation is:
```c#
{
    "Id": "207379FE-9F7F-483C-8D26-A5369F073369",
    "Valid": "True",
    "Errors": []
}
```

#### Get Job

```c#
GET /api/jobs/207379FE-9F7F-483C-8D26-A5369F073369
```
The result of the above operation is:
```c#
{
    "Id": "207379FE-9F7F-483C-8D26-A5369F073369",
    "JobName": "MyJob",
    "JobGroup": "DEFAULT",
    "JobType": "WebRequestJob",
    "Description": ""
}
```

#### Schedule Job with Simple Trigger

```c#
POST /api/simpleTriggers
{
    "Name": "MyTrigger",
    "JobName": "MyJob",
    "RepeatCount": 5,
    "RepeatInterval": "0:00:01:00"
}
```
***
```c#
{
    "Id": "C0BD2811-6AD5-4120-90F2-900AA668FDCC",
    "Valid": "True",
    "Errors": []
}
```

#### Execute Job

```c#
POST /api/jobs/207379FE-9F7F-483C-8D26-A5369F073369
{}
```
***
```c#
{
    "Valid": "True",
}
```

#### Delete Job

```c#
DELETE /api/jobs/207379FE-9F7F-483C-8D26-A5369F073369
{}
```

#### Supported Quartz.net Functionality

- Jobs: 
  - SendMailJob
  - NativeJob
  - DirectoryScanJob (with callback url parameter)
- Triggers:
  - Simple Trigger
  - Cron Trigger
- Calendars:
  - Holiday Calendar
  - Cron Calendar
- Misfire Instructions
- DataStore:
  - SqlServer
  - Postgres

#### What does R.Scheduler add on top of top of Quartz.net?

- Jobs:
  - AssemblyPluginJob
  - FtpDownloadJob (Use Default Ftp library or inject your own)
  - WebRequestJob
  - DirectoryScanJob callback
  - SqlJob
- WebApi
- Auditing
- Support for Token-based authentication



#### Downloads

Download from NuGet 'R.Scheduler' and install into your Host application.

Download from NuGet 'R.Scheduler.Contracts' and install into your Client application(s).

[Search NuGet for R.Scheduler](http://nuget.org/packages?q=R.Scheduler)
