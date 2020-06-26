MassTransit
===========

MassTransit is a _free, open-source_ distributed application framework for .NET. MassTransit makes it easy to create applications and services that leverage message-based, loosely-coupled asynchronous communication for higher availability, reliability, and scalability.

![Mass Transit](https://avatars2.githubusercontent.com/u/317796?s=200&v=4 "Mass Transit")

MassTransit is Apache 2.0 licensed.

Build Status
------------

Branch | Status
--- | :---:
master | [![master](https://ci.appveyor.com/api/projects/status/hox8dhh5eyy7jsf4/branch/master?svg=true)](https://ci.appveyor.com/project/phatboyg/masstransit/branch/master)
develop | [![develop](https://ci.appveyor.com/api/projects/status/hox8dhh5eyy7jsf4/branch/develop?svg=true)](https://ci.appveyor.com/project/phatboyg/masstransit/branch/develop)

MassTransit Nuget Packages
---------------------------

| Package Name | .NET Standard | .NET Core App |
| ------------ | :-----------: | :----------: |
| **Main** |
| [MassTransit][MassTransit.nuget] | 2.0 |
| **Other** |
| [MassTransit.Analyzers][Analyzers.nuget] | 2.0 |
| [MassTransit.SignalR][SignalR.nuget] | 2.0 |
| [MassTransit.TestFramework][TestFramework.nuget] | 2.0 |
| **Containers** |
| [MassTransit.Autofac][Autofac.nuget] | 2.0 |
| [MassTransit.Extensions.DependencyInjection][CoreDI.nuget] | 2.0 |
| [MassTransit.SimpleInjector][SimpleInjector.nuget] | 2.0 |
| [MassTransit.StructureMap][StructureMap.nuget] | 2.0 |
| [MassTransit.CastleWindsor][Windsor.nuget] | 2.0 |
| **ASP.NET Core** |
| [MassTransit.AspNetCore][AspNetCore.nuget] | - | 3.1 |
| **Monitoring** |
| [MassTransit.Prometheus][Prometheus.nuget] | 2.0 |
| **Persistence** |
| [MassTransit.Azure.Cosmos][Cosmos.nuget] | 2.0 |
| [MassTransit.Dapper][Dapper.nuget] | 2.0 |
| [MassTransit.EntityFrameworkCore][EFCore.nuget] | 2.0 |
| [MassTransit.EntityFramework][EF.nuget] | 2.1 |
| [MassTransit.Marten][Marten.nuget] | 2.0 |
| [MassTransit.MongoDb][MongoDb.nuget] | 2.0 |
| [MassTransit.NHibernate][NHibernate.nuget] | 2.0 |
| [MassTransit.Redis][Redis.nuget] | 2.0 |
| **Scheduling** |
| [MassTransit.Hangfire][Hangfire.nuget] | 2.0 |
| [MassTransit.Quartz][Quartz.nuget] | 2.0 |
| **Transports** |
| [MassTransit.ActiveMQ][ActiveMQ.nuget] | 2.0 |
| [MassTransit.AmazonSQS][AmazonSQS.nuget] | 2.0 |
| [MassTransit.Azure.ServiceBus.Core][AzureSbCore.nuget] | 2.0 |
| [MassTransit.RabbitMQ][RabbitMQ.nuget] | 2.0 |
| [MassTransit.WebJobs.EventHubs][EventHubs.nuget] | 2.0 |
| [MassTransit.WebJobs.ServiceBus][AzureFunc.nuget] | 2.0 |


## Getting started with MassTransit

In order to get started with MassTransit, you can have a look at the
documentation, which is located at [https://masstransit-project.com/](https://masstransit-project.com/).

### Downloads

Download from NuGet 'MassTransit' [Search NuGet for MassTransit](https://nuget.org/packages?q=masstransit)

Download the continuously integrated Nuget packages from [AppVeyor](https://ci.appveyor.com/project/phatboyg/masstransit/build/artifacts).

### Supported transports

We support RabbitMQ and Azure Service Bus message brokers.

## Mailing list

[MassTransit Discuss](https://groups.google.com/group/masstransit-discuss)

## Discord 

Get help live at the MassTransit Discord server.

[![alt Join the conversation](https://img.shields.io/discord/682238261753675864.svg "Discord")](https://discord.gg/rNpQgYn)

## GitHub Issues

**Pay attention**

Please do not open an issue on github, unless you have spotted an actual bug in MassTransit. 
If you are unsure, ask on the mailing list, and if we confirm it's a bug, we'll ask you to create the issue. 
Issues are not the place for questions, and they'll likely be closed.

This policy is in place to avoid bugs being drowned out in a pile of sensible suggestions for future 
enhancements and calls for help from people who forget to check back if they get it and so on.

## Building from Source

 1. Install the latest [.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1) SDK.
 1. Clone the source down to your machine.<br/>
    ```bash
    git clone git://github.com/MassTransit/MassTransit.git
    ```
 1. Run `build.ps1` or `build.sh`.

## Contributing

 1. Turn off `autocrlf`.
    ```bash
    git config core.autocrlf false
    ```
 1. Hack!
 1. Make a pull request.

## Builds

MassTransit is built on [AppVeyor](https://ci.appveyor.com/project/phatboyg/masstransit)
 
# REQUIREMENTS
* .NET Core SDK v3.1

# CREDITS
Logo Design by _The Agile Badger_

[MassTransit.nuget]: https://www.nuget.org/packages/MassTransit
[Analyzers.nuget]: https://www.nuget.org/packages/MassTransit.Analyzers
[SignalR.nuget]: https://www.nuget.org/packages/MassTransit.SignalR
[TestFramework.nuget]: https://www.nuget.org/packages/MassTransit.TestFramework

[Autofac.nuget]: https://www.nuget.org/packages/MassTransit.Autofac
[CoreDI.nuget]: https://www.nuget.org/packages/MassTransit.Extensions.DependencyInjection
[SimpleInjector.nuget]: https://www.nuget.org/packages/MassTransit.SimpleInjector
[StructureMap.nuget]: https://www.nuget.org/packages/MassTransit.StructureMap
[Windsor.nuget]: https://www.nuget.org/packages/MassTransit.CastleWindsor

[AspNetCore.nuget]: https://www.nuget.org/packages/MassTransit.AspNetCore
[Prometheus.nuget]: https://www.nuget.org/packages/MassTransit.Prometheus

[Cosmos.nuget]: https://www.nuget.org/packages/MassTransit.Azure.Cosmos
[Dapper.nuget]: https://www.nuget.org/packages/MassTransit.Dapper
[EFCore.nuget]: https://www.nuget.org/packages/MassTransit.EntityFrameworkCore
[EF.nuget]: https://www.nuget.org/packages/MassTransit.EntityFramework
[Marten.nuget]: https://www.nuget.org/packages/MassTransit.Marten
[MongoDb.nuget]: https://www.nuget.org/packages/MassTransit.MongoDb
[NHibernate.nuget]: https://www.nuget.org/packages/MassTransit.NHibernate
[Redis.nuget]: https://www.nuget.org/packages/MassTransit.Redis

[Hangfire.nuget]: https://www.nuget.org/packages/MassTransit.Hangfire
[Quartz.nuget]: https://www.nuget.org/packages/MassTransit.Quartz

[ActiveMQ.nuget]: https://www.nuget.org/packages/MassTransit.ActiveMQ
[AmazonSQS.nuget]: https://www.nuget.org/packages/MassTransit.AmazonSQS
[AzureSbCore.nuget]: https://www.nuget.org/packages/MassTransit.Azure.ServiceBus.Core
[RabbitMQ.nuget]: https://www.nuget.org/packages/MassTransit.RabbitMQ
[EventHubs.nuget]: https://www.nuget.org/packages/MassTransit.WebJobs.EventHubs
[AzureFunc.nuget]: https://www.nuget.org/packages/MassTransit.WebJobs.ServiceBus
