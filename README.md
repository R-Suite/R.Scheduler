# R.Scheduler
An experimental, easy to use plugin execution engine built on top of Quartz Enterprise Scheduler .NET. 
R.Scheduler is API driven. Actions can be performed using a simple RESTful API using JSON over HTTP.

## Getting Started

### Configuration

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
    config.EnableWebApiSelfHost = true;
    config.PersistanceStoreType = PersistanceStoreType.Postgre;
    config.ConnectionString = "Server=localhost;Port=5432;Database=Scheduler;User Id=xxx;Password=xxx;";
});

IScheduler sched = R.Scheduler.Scheduler.Instance();
sched.Start();
```
