# dotnet-kafka
This repo shows 2 microservices written in .NET 8 that produce and consume messages from an Apache Kafka® cluster.  
It follows [this Confluent guide](https://developer.confluent.io/courses/apache-kafka-for-dotnet/overview/) for the code and few other resources for the lessons.

## Introduction
Apache Kafka is an event streaming platform used to collect, store and process real time data streams at scale.  
It has numerous use cases, including distributed logging, stream processing and Pub-Sub Messaging.

<img width="900" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/19978d3c-3f9c-4474-9a3b-995b5ea437ed">

## Helpful Links
1. [Data streaming with Apache Kafka](https://developer.confluent.io/)
2. [Kafka 101](https://developer.confluent.io/courses/apache-kafka/events/)
3. [How Kafka works](https://www.confluent.io/blog/apache-kafka-intro-how-kafka-works/)(Great!)
4. [confluent kafka dotnet examples](https://github.com/confluentinc/confluent-kafka-dotnet/tree/master/examples)
5. [Apache Kafka for .NET developers](https://developer.confluent.io/courses/apache-kafka-for-dotnet/overview/)(Great!)
6. [Kafka Visualization](https://softwaremill.com/kafka-visualisation/)(Great!)

## Terminology
### Event
An event is any type of action, incident, or change that's identified or recorded by software or applications. For example, a payment, a website click, or a temperature reading, along with a description of what happened.

Kafka encourages you to see the world as sequences of events, which it models as key-value pairs.

Events are immutable, as it is (sometimes tragically) impossible to change the past.

### Topic (Think of it as category of messages, table, log etc.)
Because the world is filled with so many events, Kafka gives us a means to organize them and keep them in order: topics.  
A topic is an ordered log of events.

Topics are properly logs, not queues; they are durable, replicated, fault-tolerant records of the events stored in them. 

The simplicity of the log as a data structure and the immutability of the contents in it are keys to Kafka's success as a critical component in modern data infrastructure.

### Partitioning
Partitioning takes the single topic log (remember topic is just a log), and breaks it into multiple logs each of which can live on a separate node in the Kafka cluster.

<img width="350" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/d54600ad-49c8-4a4b-9983-33b4f366600c">
<br><br>
Deciding which messages to write to which partition:

1. If the message has no key (remember event is key-value pair)
   
   <img width="400" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/d889e3aa-6a51-413a-9ca3-f35f48f11eb3">

   The messages are distributed round-robin among the topic partitions.
  
2. If the message has key
   
   <img width="350" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/5c7866d6-26f6-4e09-a44c-cc5c32ce4949">

   Use that key to figure out which partition to put the message into.

   We run that key through a hash function, take that output and mod the number of partitions and the resulting number is just the partition number to write to.

   It guarantees that messages having the same key always land in the same partition and therefore are always in order.

### Kafka Brokers
Kafka is distributed data infrastructure, which implies that there is some kind of node that can be duplicated across a network such that the collection of all of those nodes functions together as a single Kafka cluster.

That node is called a broker.

A broker can run on bare metal hardware, a cloud instance, in a container managed by Kubernetes, in Docker on your laptop, or wherever JVM processes can run.

Kafka brokers are intentionally kept very simple, maintaining as little state as possible. They are responsible for writing new events to partitions, serving reads on existing partitions, and replicating partitions among themselves. 

They don’t do any computation over messages or routing of messages between topics.

---

Kafka brokers are servers with special jobs to do: managing the load balancing, replication, and stream decoupling within the Kafka cluster.

In Kafka, data (individual pieces of data are events) are stored within logical groupings called topics. Those topics are split into partitions, the underlying structure of which is a log.

<img width="600" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/23ac25ca-e062-4c98-bad3-ef3d50f1a1bf">

Imagine running those broker 1,2,3 pods inside a Kubernetes cluster. Now you have Kafka cluster inside a Kubernetes cluster!

### Offset
Offset represents the position of a record within a partition of a topic, similar to how an index works in an array. Any particular message in a topic partition is identified by this unique offset.

<img width="500" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/5c2a7c9a-2374-4c07-ab33-fc8468fecf03">

### Committed Offsets
The concept of committed comes only when there is a consumer group.

The committed offset points to the next message that will be processed in the future.  
For eg: When you commit offset 3, you're telling Kafka that your consumer has successfully processed the record upto offset 2 and is ready to consume the record at offset 3.

Each partition of a Kafka topic has its own set of offsets, which indicate the last message that was successfully processed by the consumer group for that partition.

So in essence, "committing an offset" is indicating that we've successfully processed all records up to that point.

### Cluster
A cluster in Kafka is a group of servers (nodes) working together for three reasons:
1. Speed (low latency)  
    Several data streams can be processed by separate servers, which decreases the latency of data delivery.
2. Durability  
   Data is replicated across multiple servers, so if one fails, another server has the data backed up.
3. Scalability  
   Kafka also balances the load across multiple servers to provide scalability.  

### Replication
Kafka provides replicated storage of topic partitions.

A single node (also known as a broker) can be both a leader for some partitions and a follower for others.

Each node in a cluster has one of two roles when it comes to replication: leader or follower. Followers stay in sync with the leader in order to keep up to date on the newest data. 

The diagram below shows what might happen if you had a Topic A with a replication factor of two (N). This means that the leader node is responsible for the first instance of each partition, and the follower node ensures that it is replicated on each server. 

<img width="450" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/dc4ae999-e0a0-4f07-ba60-e78f5618e117">

broker1 is the leader of Partition 1 and and broker 2 is follower of Partition 1.

There's 1 lead partition and N-1 followers. N is the replication factor.

## Create projects
### Create a new Git repo and clone it down to your local
For example, I created this one for putting the hands on exercises.

### Create a new sln file
```bash
Ashishs-MacBook-Pro:dotnet-kafka akhanal$ dotnet new sln
```
### Add a web api project as Producer
<img width="425" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/e309d0df-7de6-4fce-a199-467cdcd5bf50">

### Add a console app as Consumer
<img width="425" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/25394bc5-984c-4eaf-a071-3cb20a8fec59">

#### Setup appsettings.json in your console app
[Reference](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration#alternative-hosting-approach)

Install `Microsoft.Extensions.Hosting` package.

Add `appsettings.json`, and set these options:
- Build action: Content  
- Copy to output directory: Copy if newer

And use it
https://github.com/akhanalcs/dotnet-kafka/blob/7313b07edd0d5e2a947b813aae2598ab596f298b/Consumer/Program.cs#L4-L10

#### Use user-secrets to store API key and secret
[Reference](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0&tabs=windows#enable-secret-storage)

```bash
dotnet user-secrets init
```

This command adds a `UserSecretsId` element, populated with a GUID, to the `.csproj` file.  
If you want the Producer to also access secrets pointed by this Id, copy this element into the Producer's project file as well.

Now you can store API keys and secrets in there without it being checked into source control.

Right click the project -> Tools -> [.NET User Secrets](https://plugins.jetbrains.com/plugin/10183--net-core-user-secrets)

<img width="400" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/507002f2-4517-45f6-b285-8b87a30e981f">

Put your secrets here

<img width="350" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/9168a6f6-5fc2-4a64-82ac-6db5ff2f3262">

### Install dependencies
Manage Nuget Packages

<img width="350" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/7ba0251a-8099-4a36-aed4-b2de91b089d2">

Install it in both projects

<img width="900" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/b85f2175-ac38-4a30-9588-190d444171ff">

## Install Java
Go to [Java Downloads](https://www.oracle.com/java/technologies/downloads/) and install the latest JDK. (JDK 21 as of Feb 2024).

**[Not required, so not doing it now]**

## Local Kafka cluster setup [Not doing it now, so skip this]
### Install confluent cli
```bash
brew install confluentinc/tap/cli
```

Check it was installed
```bash
Ashishs-MacBook-Pro:dotnet-kafka ashishkhanal$ confluent version
confluent - Confluent CLI

Version:     v3.48.1
Git Ref:     e86e352ee
Build Date:  2024-01-25T23:40:47Z
Go Version:  go1.21.5 X:boringcrypto (darwin/amd64)
Development: false
```

### Start the Kafka broker
```bash
confluent local kafka start
```

#### Troubleshooting
You'll get this error
```bash
Error: Cannot connect to the Docker daemon at unix:///var/run/docker.sock. Is the docker daemon running?
```

It went away after I checked the option of allowing docker socket to be used.

<img width="850" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/1313b420-df4f-4981-a72d-88ef5cff3b61">

Like this:

<img width="450" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/cea97d6a-30cd-4388-8b28-6bd11a223881">

---

Now, it doesn't print any ports, just shows this:
```bash
Ashishs-MacBook-Pro:dotnet-kafka ashishkhanal$ confluent local kafka start
The local commands are intended for a single-node development environment only, NOT for production usage. See more: https://docs.confluent.io/current/cli/index.html
```

Set environment variable `CONFLUENT_HOME`:

Check where the `confluent` cli is installed
```bash
Ashishs-MacBook-Pro:dotnet-kafka ashishkhanal$ brew info confluentinc/tap/cli
/usr/local/Cellar/cli/3.48.1 (4 files, 57.5MB) *
```

Open your shell profile
```bash
vim ~/.bash_profile
```

Add this to the file
```bash
# For Confluent
export CONFLUENT_HOME="/usr/local/Cellar/cli/3.48.1"
export PATH=$PATH:$CONFLUENT_HOME/bin
```

Hit `Esc`, type `:wq` and hit enter to save and quit.

Reload the bash profile file using the source command:
```bash
source ~/.bash_profile
```

It works at this point. For more details, check out my [StackOverflow question](https://stackoverflow.com/q/77985757/8644294).

<img width="950" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/9a9b15c5-4a0f-4d5c-8249-3883810c1f7c">

I gave Confluent Cloud a try instead of local cluster at this time.
## Run Kafka in Confluent Cloud
### Signup
[Go to Signup page](https://www.confluent.io/confluent-cloud/tryfree/)

I signed up through my Azure account as 'Pay As You Go' plan.

<img width="450" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/14b0a2ef-e70f-4b33-b879-50d08f882d86">

Created Confluent org in my Azure account

<img width="600" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/bb0f35be-f95f-461d-abd6-0c2ae5029b1c">

It appears under the RG I created it under

<img width="650" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/a3f31fd2-b305-4b0c-b2b8-c86da7c2aafc">

Now go to Confluent cloud by clicking Launch

<img width="300" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/672d9b73-cd46-4a59-9f23-a5c613362b00">

You'll go to this url often, bookmark it if you'd like:  
https://confluent.cloud/environments

### Create environment
Go to Home

Environments -> Add cloud environment

<img width="350" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/a569d5b1-58e8-411a-8984-b281a69bcb6f">
<br><br>

Stream Governance Packages -> Essentials -> Begin configuration

#### Select which cloud and region you want to create your Schema Registry and Stream Catalog in
Tell it where you will be storing the metadata.

<img width="550" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/2c57f793-801d-4f67-a07c-07a233ec53a0">

#### View it using CLI
```bash
confluent login
```

<img width="225" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/653fcab1-692d-4c27-b8ae-42ae9561d94f">

CLI shows the successful login

<img width="1050" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/6646fbe7-56c9-4e2c-a913-00c0e1e861b3">
<br><br>

```bash
confluent environment list
```

<img width="550" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/8efab508-ae07-44d3-9d59-bba678c8630f">
<br><br>

Set the new environment I just created as the active environment:
```bash
confluent environment use env-19vow5
```

Notice that the `*` has changed

<img width="550" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/3f73b8fa-4c31-40dd-90aa-5260aec5400c">

### Create cluster
<img width="550" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/f1070fff-b389-409f-baf9-27078918b2b2">

-> Create cluster on my own
<br><br>
Create cluster -> Basic

<img width="190" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/de308359-313f-4346-a51b-0caf12e2470e">

-> Begin configuration
<br><br>
<img width="600" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/8b188706-57f3-410e-8af9-de32b2aba91b">
<br>
<img width="350" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/ea734e4b-1f4a-4bea-992c-2d03d8d7d021">

-> Launch cluster

### Billing and payment
Payment details

Add coupon code: `DOTNETKAFKA101`

<img width="750" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/b47bd8f5-af4e-43a1-9a24-58a48f6fdc90">

### Add an API key for Kafka cluster (Cluster level)
We will need an API Key to allow applications to access our cluster.

<img width="600" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/55e85853-d27f-4e01-b5c2-c9c76995f0bc">

-> Create key
<br><br>
<img width="400" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/f94892bd-6027-49d7-aa6e-53702ced8a40">

Global access -> Next

Download and save the key somewhere for future use.

### Add an API key for Schema Registry (It's at Environment level)
1. From the main menu (top right) or the breadcrumb navigation (top) select **Environments**.
2. Select the **kafka-with-dotnet** environment.
3. In the right-hand menu there should be an option to **Add key**. Select it and create a new API Key.

<p align="left">
  <img width="250" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/63252f75-54ab-4166-9902-f3635e86ba8f">
&nbsp;
  <img width="400" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/f4e92617-719a-42d2-a464-99a603094ae7">
</p>

## Create Topic
[Reference](https://developer.confluent.io/courses/apache-kafka-for-dotnet/producing-messages-hands-on/#create-a-new-topic)

A topic is an immutable, append-only log of events. Usually, a topic is comprised of the same kind of events.

<img width="450" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/8338369a-158d-4932-8d82-3b301a85a628">
<br><br>

Create a new topic, `RawBiometricsImported`, which you will use to produce and consume events.

<img width="450" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/324d25d8-a71f-4485-86b0-5470d45370d6">

-> Create with defaults

When asked to **Define a data contract** select **Skip** for now.

## Populate config file using API Key file you downloaded earlier
- The Kafka.BootstrapServers is the Bootstrap server in the file.
- The Kafka.SaslUsername is the key value in the file.
- The Kafka.SaslPassword is the secret value in the file.
- SchemaRegistry.URL is the Stream Governance API endpoint url.<br>
  <img width="250" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/6a7404d5-b3ec-47b7-9a03-883b53c4ea01">
- SchemaRegistry.BasicAuthUserInfo is `<key>:<secret>` from the API Key file you downloaded for the Schema Registry.

Store user name and passwords inside `secrets.json` and other details in `appsettings.json`.

## Kafka Messages
<img width="500" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/2c56d72f-9788-437f-9715-2de12d2b3731">

### Event
A domain event signals something that has happened in the outside world that is of interest to the application.

Events are something that happened in the past. So they are immutable.

Use past tense when naming events. For eg: `UserCreated`, `UserAddressChanged` etc.

### Kafka Message Example
```cs
var message = new Message<string, Biometrics>
{
  Key = metrics.DeviceId,
  Value = metrics
};
```
If you care about message ordering, provide key, otherwise it's optional.  
In above example, because we're using `DeviceId` as key, all messages of that specific device are handled in order.

Value can be a primitive type such as string or some object that can be serialized into formats such as JSON, Avro or Protobuf.

## Producing messages to a topic
<img width="500" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/1d0706c6-5b6e-465f-ba32-2aef3628dd8d">

You can consider the messages being produced by your system to be just another type of API.

Some APIs will be consumed through HTTP while others might be consumed through Kafka.

### Producer Config
```json
  "KafkaProducer": {
    // One or more Kafka brokers each specified by a host and port if necessary.
    // It will be used to establish the initial connection to the Kafka cluster.
    // Once connected, additional brokers may become available.
    "BootstrapServers": "pkc-4rn2p.canadacentral.azure.confluent.cloud:9092",
    "SecurityProtocol": "SaslSsl",
    "SaslMechanism": "PLAIN",
    "SaslUsername": "comes from user-secrets' secrets.json",
    "SaslPassword": "comes from user-secrets' secrets.json",
    // Used to identify the producer.
    // In other words, to give it a name.
    // Although it's not strictly required, providing a ClientId will make debugging a lot easier.
    "ClientId": "ClientGateway"
  }
```

Grab the config
```cs
builder.Services.Configure<ProducerConfig>(builder.Configuration.GetSection("KafkaProducer")); // OR var producerConfig = builder.Configuration.GetSection("KafkaProducer").Get<ProducerConfig>();
```

## Produce messages in your Web API app
Go to the web api you created earlier.

It will work as a simple REST endpoint that accepts data from a fitness tracker in the form of strings and pushes it to Kafka with no intermediate processing.

In the long run, this may be dangerous because it could allow a malfunctioning device to push invalid data into our stream. We probably want to perform a minimal amount of validation, prior to pushing the data. We'll do that later.

Register an instance of `IProducer<string, string>`. We use a singleton because the producer maintains connections that we want to reuse.
```cs
builder.Services.AddSingleton<IProducer<string, string>>(sp =>
{
    var config = sp.GetRequiredService<IOptions<ProducerConfig>>();
    return new ProducerBuilder<string, Biometrics>(config.Value)
        .Build();
});
```

Send the message
```cs
var result = await producer.ProduceAsync(biometricsImportedTopicName, message);
// Synchronous method, so it will wait for acknowledgement from broker before continuing
// It's often better to produce multiple messages into a batch prior to calling Flush.
producer.Flush();
```

The messages aren't necessarily sent immediately.  
They may be buffered in memory so that multiple messages can be sent as a batch.  
Once we're sure we want the messages to be sent, it's a good idea to call the `.Flush()` method.

### Test it
#### Start the app
<img width="300" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/f7250701-7d04-4477-bead-6c9c5f83db7e">

#### Send a message to the endpoint through Swagger
<img width="500" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/a83c9991-e5b9-4256-863a-a9461490aec6">

#### Verify it in the cluster
Home -> Environments -> kafka-with-dotnet -> cluster_0 -> Topics

<img width="500" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/974b3b19-fa64-4402-8e40-8b2b3b48cfc1">

## Serialization & Deserialization
The message producer is created by providing two types. 
```cs
new ProducerBuilder<TypeofKey, TypeofValue>(config)
```
The first type represents the Key of the message while the second is for the Value.  
These types can be simple types, such as strings or integers, but they can also be more complex types like a custom class or struct. 

However, Kafka itself doesn't directly understand these types, it just operates with blobs of data in byte form. This implies that we need to convert our data into an array of bytes before sending it to Kafka if it is a complex object.

<img width="650" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/528a11e9-a910-4ffa-b6ae-afe2e4997195">

To register a serializer for the message **value**, we use the `SetValueSerializer` method on the ProducerBuilder.  
For eg:
```cs
new ProducerBuilder<string, Biometrics>(producerConfig)
    .SetValueSerializer(new JsonSerializer<Biometrics>(schemaRegistry))
    .Build();
```
## Schemas and the Schema Registry
The Confluent Kafka client includes support for three major message formats, including Protobuf, Avro and JSON.

Each of these three formats supports a message schema. Schema is basically a set of rules that outline the exact structure of a message.

These schema can be stored in an external service known as a Schema Registry. The Confluent Cloud built-in Schema Registry is a good choice here.

To make use of the schemas, we can connect our serializers to the Schema Registry.

The serializer will have its own version of the schema that it will use to serialize each message.  
The first time we try to serialize an object we look for a matching schema in the registry.

If a matching schema is found,the current message and any future ones that use the same schema will be sent to Kafka.

<img width="550" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/0448c387-860b-4e1b-863a-5808c65ecd93">

However, if no matching schema is found,then any messages that use that schema will be rejected. Essentially, an exception is thrown. This ensures that each message going to Kafka matches the required format.

## Schemas & Serialization
### Create a new topic
Create a new topic named `BiometricsImported` (use the default partitions).

Define a data contract -> Create a schema for message values

<img width="400" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/bf734225-8385-4252-b79a-6c9334a41ebb">

-> Create Schema
<br><br>

<img width="600" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/04a1e110-e193-44c0-b6da-fc3c4f8fae09">

Copy paste the above JSON into https://codebeautify.org/jsonviewer and add your fields into it (to ensure you're not messing up the JSON), and paste it back to this page and hit **Create**.
```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "$id": "http://example.com/myURI.schema.json",
  "title": "Biometrics",
  "description": "Biometrics collected from the fitness tracker.",
  "type": "object",
  "additionalProperties": false,
  "definitions": {
    "HeartRate": {
      "type": "object",
      "additionalProperties": false,
      "properties": {
        "DateTime": {
          "format": "date-time",
          "type": "string"
        },
        "Value": {
          "format": "int32",
          "type": "integer"
        }
      }
    }
  },
  "properties": {
    "DeviceId": {
      "format": "guid",
      "type": "string",
      "description": "The string type is used for strings of text."
    },
    "HeartRates": {
      "items": {
        "$ref": "#/definitions/HeartRate"
      },
      "type": "array"
    },
    "MaxHeartRate": {
      "format": "int32",
      "type": "integer"
    }
  }
}
```

In above step, essentially you were just mapping these records into a JSON format
```cs
record Biometrics(Guid DeviceId, List<HeartRate> HeartRates, int MaxHeartRate);
record HeartRate(DateTime DateTime, int Value);
```

### Connect the app to Schema Registry
Add 2 packages:
https://github.com/akhanalcs/dotnet-kafka/blob/df7850306df39f57b7f89be05a84077d6a773577/Producer/Producer.csproj#L13-L14

Register an instance of a `ISchemaRegistryClient` using a `new CachedSchemaRegistryClient`
https://github.com/akhanalcs/dotnet-kafka/blob/df7850306df39f57b7f89be05a84077d6a773577/Producer/Program.cs#L29-L34

### Test it
Navigate to Swagger page

<img width="190" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/dc04d7e6-0e43-4be8-a929-c570e29a2d77">

Click on `/biometrics` and click "Try it out".

Send the request. It succeeds.

<img width="250" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/a842fba4-eaac-4ab2-acd1-a3155e0848a7">
<br><br>

Check it out in Confluent Cloud.

<img width="700" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/b546a032-9a6b-4997-a7ba-d06e786e1d03">

## Consuming Messages from a Topic
The job of the consumer is to process each message in a topic. Usually, they're processed one at a time in the order they're received.

<img width="450" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/e41e8ed4-9ccb-4bf6-8f40-8003e57693eb">

It's like a reader (consumer) reading a book.

The reader gets to decide which books they read and how they read them.  
Some people might read a book end-to-end while others might jump around, only looking at the sections that interest them.

In the same way, a Kafka consumer can decide what topics they will pay attention to.  
Within each topic, they often look at every message, perform some processing, and potentially update a database or some other state.  
However, they may decide that some messages aren't relevant and discard them rather than wasting time on processing.  

<img width="400" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/a84a7f8f-2190-47e0-8147-238eefd8935c">

Remember that when messages are produced to a topic, they're assigned a key. That key is used to separate the topic into multiple partitions.  
Within each partition, the order of the messages is guaranteed. However, it's not guaranteed across multiple partitions.

<img width="400" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/10acb37d-0e46-42b2-8ae3-5ca67f311b02">

When we create a consumer, we assign it a group ID. This ID indicates that multiple instances of the consumer all belong to the same group. Kafka will distribute the partitions among the group members.  

This means that in a topic with 30 partitions, you could have up to 30 instances of your consumer all working in parallel.

<img width="500" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/75b87e44-f7ce-45b4-92dc-adb07a975477">

Each member of the group processes separate partitions.

### Configuration
```json
  "KafkaConsumer": {
    "BootstrapServers": "pkc-4rn2p.canadacentral.azure.confluent.cloud:9092",
    "SecurityProtocol": "SaslSsl",
    "SaslMechanism": "PLAIN",
    "ClientId": "BiometricsService",
    // To identify multiple consumers that all belong to a single group. (Eg: multiple instances of same microservice)
    "GroupId": "BiometricsService",
    // Determines where the consumer should start processing messages if it has no record of a previously committed offset.
    // In other words, if the consumer is unsure where to start processing messages, it will use this setting.
    // Setting it to Earliest will cause the consumer to start from the earliest message it hasn't seen yet.
    // This is a good value to use if you want to ensure that all messages get processed.
    "AutoOffsetReset": "earliest",
    // EnableAutoCommit determines whether or not the consumer will automatically commit its offsets.
    // Committing an offset indicates that the consumer has finished processing a message and doesn't want to receive it again.
    // If we choose to auto-commit, then the offsets are automatically managed by the consumer.
    "EnableAutoCommit": true,
    "SaslUsername": "comes from user-secrets' secrets.json",
    "SaslPassword": "comes from user-secrets' secrets.json"
  }
```

### Subscribing to a topic
When we are ready to begin consuming messages, we need to subscribe to a topic. This is done using the Subscribe Method on the consumer.  

It takes either a single topic name or a collection of multiple topic names. This informs the client which topics we want to consume but it doesn't actually start processing messages.

```cs
consumer.Subscribe(BiometricsImportedTopicName);
```

### Consuming a message
To start processing messages, we use the Consume Method. It will grab the next available message from the topic and return it as the result.
```cs
  consumer.Subscribe(BiometricsImportedTopicName);

  while(!stoppingToken.IsCancellationRequested)
  {
      var result = consumer.Consume(stoppingToken);
      // Do something with the message. DeviceId: result.Message.Key, Biometrics: result.Message.Value
  }

  // Close the consumer after you're done
  consumer.Close();
```

### Create a background service to work on the message in Consumer project
[Reference](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-8.0&tabs=visual-studio)

### Setup the Consumer in Program.cs
[How to register services in Console app](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-usage#register-services-for-di)

Add necessary packages
https://github.com/akhanalcs/dotnet-kafka/blob/54cda76ddb80ab3228bc400e46ec418cd3d7b639/Consumer/Consumer.csproj#L14-L15

And the setup in Program.cs
https://github.com/akhanalcs/dotnet-kafka/blob/54cda76ddb80ab3228bc400e46ec418cd3d7b639/Consumer/Program.cs#L21-L30

### Test it
We have 1 message in the Topic

<img width="500" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/14e5c215-a0b6-4e5f-a58a-74a2e569d6c8">

The Consumer read it successfully

<img width="650" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/231b46ac-ab86-403d-b2ab-199f50b89a9b">

## Delivery Guarantees & Transactions
### At least once
The default behavior of a Kafka consumer is to automatically commit offsets at a given interval.
```json
  "KafkaConsumer": {
    // Other stuffs here
    // Means that the consumer will receive every message from a topic, but may see duplicates.
    "EnableAutoCommit": true, // Default
    "AutoCommitIntervalMs": 5000,
    "EnableAutoOffsetStore": true
  },
```

However, the behavior in .NET is quite different.  
Let's take a look at a simple example.

**Scenario 1**
```cs
    while (!stoppingToken.IsCancellationRequested)
    {
        // When we call consume, the offset for the current message is stored in memory.
        // A background thread periodically commits the most recently stored offset to Kafka, according to the AutoCommitInterval.
        var result = consumer.Consume(stoppingToken);
        Process(result); // CRASH!!
        // Background commit would have occured here.
        // 👆If our application crashes BEFORE we commit the offsets, then we'll see the messages again once we recover.
        // In this scenario, we see all of the messages, but we get duplicates.
    }
```

**Scenario 2**
```cs
    while (!stoppingToken.IsCancellationRequested)
    {
        var result = consumer.Consume(stoppingToken);
        // Background commit occurs here.
        // 👆If our application crashes AFTER we commit the offsets, then the message is lost and won't be recovered.
        Process(result); // CRASH!!
    }
```

If we want to achieve at-least-once processing, we need to manually store the offsets.  
To do this, we start by disabling automatic offset storage.
```json
  "KafkaConsumer": {
    // Other stuffs here
    "EnableAutoCommit": true, // The auto-commit thread will still commit the offsets to Kafka according to the specified interval
    "AutoCommitIntervalMs": 5000,
    "EnableAutoOffsetStore": false, // <-- This guy
  },
```
Now the code looks like this
```cs
    while (!stoppingToken.IsCancellationRequested)
    {
        var result = consumer.Consume(stoppingToken);
        Process(result); // CRASH!!
        // When the message is fully processed, we call store offset on the consumer to explicitly store the offset in memory.
        consumer.StoreOffset(result);
    }
```

In the event of a crash, the offset won't have been stored. When we recover, we'll see the message again, giving us the at-least-once guarantee we were looking for.

### Effectively once (using the combination of Idempotency and Transaction)
In order to achieve effectively-once processing, we need to enable some settings on our producer.

Normally, when the producer experiences a write failure, it will retry the message.  
This can result in duplicates. However, if we enable **idempotence** then a unique sequence number is included with each write.
If the same sequence number is sent twice, then it will be de-duplicated.

```json
  // Consumer/appsettings.json
  "KafkaProducer": {
    // Other stuffs here
    "ClientId": "BiometricsService",
    "EnableIdempotence": "true" // <-- This guy
    // Config added for Transactional commits
    // Identify the instance of our application with a TransactionId.
    "TransactionalId": "BiometricsService" // <-- This guy
  },
  "KafkaConsumer": {
    // Other stuffs here
    "ClientId": "BiometricsService",
    "GroupId": "BiometricsService",
    // Config changed for Transactional commits. Changed from default true to false.
    "EnableAutoCommit": false, // <-- This guy
    // Default is ReadCommitted. Ensures that the consumers are only reading fully committed message in a Transaction.
    "IsolationLevel": "ReadCommitted" // <-- This guy
  }
```

## Transactional Commits
If we are consuming data from Kafka and producing new records to Kafka with no other external storage, then we can rely on Kafka transactions.

In the previous exercise, we implemented a basic consumer for our topic. One key aspect of that **consumer** is that it was set up to do **automatic** commits (after consuming the message). As a result, if something failed to process, it may be committed anyway, and the message may be lost.

We are going to switch from automatic commits to **transactional** commits (`"EnableAutoCommit": false`).

As we process the messages, we will produce multiple new messages to a separate topic. We want this to happen in a transactional fashion. Either all of the messages get published, or none of them do.

### Create a new topic
In Confluent Cloud, create a new topic named `HeartRateZoneReached`.

The type of message it will use is
```cs
public record HeartRateZoneReached(Guid DeviceId, HeartRateZone Zone, DateTime DateTime, int HeartRate, int MaxHeartRate);
```

So use this Schema
```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "$id": "http://example.com/myURI.schema.json",
  "title": "HeartRateZoneReached",
  "type": "object",
  "description": "Schema for HeartRateZoneReached.",
  "additionalProperties": false,
  "definitions": {
    "HeartRateZone": {
      "description": "It's an enum that represents HeartRateZone",
      "type": "integer",
      "enum": [
        0,
        1,
        2,
        3,
        4,
        5
      ],
      "x-enumNames": [
        "None",
        "Zone1",
        "Zone2",
        "Zone3",
        "Zone4",
        "Zone5"
      ]
    },
    "properties": {
      "DateTime": {
        "format": "date-time",
        "type": "string"
      },
      "DeviceId": {
        "format": "guid",
        "type": "string"
      },
      "HeartRate": {
        "format": "int32",
        "type": "integer"
      },
      "MaxHeartRate": {
        "format": "int32",
        "type": "integer"
      },
      "Zone": {
        "$ref": "#/definitions/HeartRateZone"
      }
    }
  }
}
```

Now our Consumer will also produce `HeartRateZoneReached` messages, so we need to add Producer configuration in Consumer project.
```json
  // Consumer/appsettings.json
  "KafkaProducer": {
    // Other stuffs here
    "ClientId": "BiometricsService",
    "EnableIdempotence": "true" // <-- This guy
    // Config added for Transactional commits
    // Identify the instance of our application with a TransactionId.
    "TransactionalId": "BiometricsService" // <-- This guy
  }
```

### Implement Heart Rate Zones
https://github.com/akhanalcs/dotnet-kafka/blob/82c4b0888b68897e6288155cc4fa88436c069a40/Consumer/Domain/HeartRateExtensions.cs#L1-L24

### Test 1
Now send this data to the `/biometrics` endpoint in Producer.

```json
{
  "deviceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "heartRates": [
    {
      "dateTime": "2022-12-02T13:50:00.000Z",
      "value": 90
    },
   {
      "dateTime": "2022-12-02T13:51:00.000Z",
      "value": 110
    },
   {
      "dateTime": "2022-12-02T13:52:00.000Z",
      "value": 130
    },
   {
      "dateTime": "2022-12-02T13:53:00.000Z",
      "value": 150
    },
   {
      "dateTime": "2022-12-02T13:54:00.000Z",
      "value": 170
    },
   {
      "dateTime": "2022-12-02T13:55:00.000Z",
      "value": 190
    }
  ],
  "maxHeartRate": 200
}
```

<img width="450" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/b8c70396-e15c-434e-86c0-fb77a36de2ae">
<br><br>

Also you can look at the Topics in Confluent cloud

<img width="500" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/9a7c0e4f-539a-4b6f-a8a3-8b7317ea2e6d">
<br><br>

As soon as above call succeeds, you'll get here in Consumer project

<img width="750" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/66bc6d3f-b2d8-404f-acc5-dc551a943d2b">
<br><br>

After `HandleMessage` method in `Consumer/Workers/HeartRateZoneWorker.cs` runs to completion, you'll see 5 messages in `HeartRateZoneReached` topic

<img width="550" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/3f670057-ad1f-476f-938f-c08692ee97d1">

So, in essence, this worker ensures that processing each `Biometrics` message and producing `HeartRateZoneReached` messages is an atomic operation/ transaction: either everything happens or nothing happens.

### Test 2
Send this over
```json
{
  "deviceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "heartRates": [
    {
      "dateTime": "2022-12-02T13:50:00.000Z",
      "value": 90
    },
   {
      "dateTime": "2022-12-02T13:51:00.000Z",
      "value": 110
    }
  ],
  "maxHeartRate": 200
}
```

It'll go to `BiometricsImported`

<img width="300" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/f43846c5-0f0e-4f3c-8e8d-76481acd1f36">

The offset says 2 (offset).

Now when this message appears at the Consumer, you'll also see that the Offset says 2.

<img width="500" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/7a9b0684-800e-430d-a47a-15ead4a8a1db">
<br><br>

The `offsets` inside `HandleMessage` shows 3 (to be committed offset).

<img width="700" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/7bc3a70a-0190-40d4-a989-068d56541d82">
<br><br>

More details in the comments:
https://github.com/akhanalcs/dotnet-kafka/blob/17203b0417f9106884a805e90eeb83a67cf9918f/Consumer/Workers/HeartRateZoneWorker.cs#L41-L53

## [Delete Confluent organization](https://learn.microsoft.com/en-us/azure/partner-solutions/apache-kafka-confluent-cloud/manage?tabs=azure-portal#delete-confluent-organization)
<img width="800" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/37eeb0e3-f3c9-4b38-aabb-9b3c79143720">
