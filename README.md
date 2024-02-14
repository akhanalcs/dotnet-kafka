# dotnet-kafka
Apache Kafka is an event streaming platform used to collect, store and process real time data streams at scale.  
It has numerous use cases, including distributed logging, stream processing and Pub-Sub Messaging.

<img width="700" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/19978d3c-3f9c-4474-9a3b-995b5ea437ed">

## Helpful Links
1. [Data streaming with Apache Kafka](https://developer.confluent.io/)
2. [Kafka 101](https://developer.confluent.io/courses/apache-kafka/events/)
3. [How Kafka works](https://www.confluent.io/blog/apache-kafka-intro-how-kafka-works/)(Great!)
4. [confluent kafka dotnet examples](https://github.com/confluentinc/confluent-kafka-dotnet/tree/master/examples)

## Terminology
### Event
An event is any type of action, incident, or change that's identified or recorded by software or applications. For example, a payment, a website click, or a temperature reading, along with a description of what happened.

Kafka encourages you to see the world as sequences of events, which it models as key-value pairs.

Events are immutable, as it is (sometimes tragically) impossible to change the past.

### Topic (category of messages)
Because the world is filled with so many events, Kafka gives us a means to organize them and keep them in order: topics.  
A topic is an ordered log of events.

Topics are properly logs, not queues; they are durable, replicated, fault-tolerant records of the events stored in them. 

The simplicity of the log as a data structure and the immutability of the contents in it are keys to Kafka's success as a critical component in modern data infrastructure.

### Partitioning
Partitioning takes the single topic log (remember topic is just a log), and breaks it into multiple logs each of which can live on a separate node in the Kafka cluster.

<img width="450" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/d54600ad-49c8-4a4b-9983-33b4f366600c">

Deciding which messages to write to which partition:

1. If the message has no key (remember event is key-value pair)
   
   <img width="450" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/d889e3aa-6a51-413a-9ca3-f35f48f11eb3">

   The messages are distributed round-robin among the topic partitions.
  
2. If the message has key
   
   <img width="450" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/5c7866d6-26f6-4e09-a44c-cc5c32ce4949">

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
### Clone this repo down
### Create a new sln file
```bash
Ashishs-MacBook-Pro:dotnet-kafka ashishkhanal$ dotnet new sln
```
### Add a web api project as Producer
<img width="450" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/e309d0df-7de6-4fce-a199-467cdcd5bf50">

### Add a console app as Consumer
<img width="450" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/25394bc5-984c-4eaf-a071-3cb20a8fec59">

#### Setup appsettings.json
[Reference](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration#alternative-hosting-approach)

Install `Microsoft.Extensions.Hosting` package.

Add `appsettings.json`, and set these options:
- Build action: Content  
- Copy to output directory: Copy if newer

And use it
https://github.com/akhanalcs/dotnet-kafka/blob/2fb1f8c16f5b5fbee85caa1645b42940d8f670fb/Consumer/Program.cs#L3-L10

#### Use user-secrets to store API key and secret
[Reference](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0&tabs=windows#enable-secret-storage)

```bash
dotnet user-secrets init
```

This command adds a `UserSecretsId` element, populated with a GUID, to the project file.

If you want the Producer to also access secrets pointed by this Id, copy this element into the Producer's project file as well.

Now you can store API keys and secrets in there without it being checked into source control.

Right click the project -> Tools -> [.NET User Secrets](https://plugins.jetbrains.com/plugin/10183--net-core-user-secrets)

<img width="350" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/507002f2-4517-45f6-b285-8b87a30e981f">

### Install dependencies
Manage Nuget Packages

<img width="400" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/7ba0251a-8099-4a36-aed4-b2de91b089d2">

Install it in both projects

<img width="850" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/b85f2175-ac38-4a30-9588-190d444171ff">

## Install Java
Go to [Java Downloads](https://www.oracle.com/java/technologies/downloads/) and install the latest JDK. (JDK 21 as of Feb 2024).

[TODO: Will come back to this later]

## Local Kafka cluster setup
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

I gave up on local setup at this time as I could not make it work even after scouring the interent. So now on to Confluent cloud.
## Run Kafka in Confluent Cloud
### Signup
[Go to Signup page](https://www.confluent.io/confluent-cloud/tryfree/)

I signed up through my Azure account as Pasy As You Go

<img width="450" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/14b0a2ef-e70f-4b33-b879-50d08f882d86">

Created Confluent org in my Azure account

<img width="750" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/bb0f35be-f95f-461d-abd6-0c2ae5029b1c">

It appears under the RG I created it under

<img width="650" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/a3f31fd2-b305-4b0c-b2b8-c86da7c2aafc">

Now go to Confluent cloud by clicking Launch

<img width="300" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/672d9b73-cd46-4a59-9f23-a5c613362b00">

### Create cluster
Go to Home

<img width="450" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/23081af9-9797-4b86-8e59-6ebe5d686162">

Add Cluster -> Create cluster -> Choose "Basic" cluster type

<img width="600" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/8b188706-57f3-410e-8af9-de32b2aba91b">

Click "Launch cluster"

<img width="350" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/ea734e4b-1f4a-4bea-992c-2d03d8d7d021">

### Grab Bootstrap server address
Home -> Environments -> default -> cluster_0 -> Cluster Settings -> Endpoints

<img width="750" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/f520ce07-ac02-46fe-818b-b682d456a10a">

## Configuration
Home -> Environments -> default -> cluster_0 -> API Keys -> Create key

<img width="550" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/06aa5e29-ea4f-4727-abdf-4b00d92b0474">

## Create Topic
[Reference](https://developer.confluent.io/courses/apache-kafka-for-dotnet/producing-messages-hands-on/#create-a-new-topic)

A topic is an immutable, append-only log of events. Usually, a topic is comprised of the same kind of events, e.g., in this guide we create a topic for retail purchases.

Create a new topic, purchases, which you will use to produce and consume events.

Home -> Environments -> default -> cluster_0 -> Topics -> Create topic

<img width="450" alt="image" src="https://github.com/akhanalcs/dotnet-kafka/assets/30603497/12acfdb3-20a2-4a30-b781-cfe15512e318">


- Truth: That comports to reality.
- Maybe true there's a diamond shaped exactly like my head on MARS, but there's no way for us to know that. so we can't really say "oh it's true that there's diamon shaped my head in MARS"

