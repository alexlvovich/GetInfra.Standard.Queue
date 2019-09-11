# Infra.Standard.Queue

[![Build Status](https://dev.azure.com/alexlvovich/GetInfra/_apis/build/status/Build%20GetInfra.Standard.Queue?branchName=master)](https://dev.azure.com/alexlvovich/GetInfra/_build/latest?definitionId=11&branchName=master)

### General

Infra.Standard.Queue is .NET Standard library designed to add Queue and Publish/Subscribe functionality to .NET project.

### Implementation

* RabbitMQ
* Azure Service Bus


#### Running with Azure Service Bus

Tell you DI what implementation you are going to use

```
services.AddSingleton<IQueueConsumer, AzureSBTopicConsumer>();

```

Or

```
services.AddSingleton<IQueuePublisher, AzureSBTopicPublisher>();

```

Set serialization settings

```
services.AddSingleton<IJsonSerializer, DefaultJsonSerializer>();

```

Create following config in appsettings.json. note "name" to be used for the constuctor

```
 "AzureServiceBus": {
    "name": {
      "Endpoint": "amqps://<your url>",
      "EntityPath": "<Topic>",
      "SasKeyName": "<Policy name>",
      "SasKey": "<SAS Key>",
      "SubscriptionName": ""
    }
  }

```
