<h1 align="center">SmartOpenHH | <a href="https://mars.haw-hamburg.de">Website</a></h1>

The SmartOpenHamburg model is an agent-based simulation model for the representation of a digital twin. It calculates multi-modal path dynamics and simulates a defined daily routine of a citizen.

## Contents

The repository contains three projects and a number of libraries to support them. For more information on each model and the libraries, please check out the README.md in the respective subdirectory.

Models:

1. SOHModelStarter: a model that is geared towards determining optimal routes using several modes of travel (modalities). Per default, the model is set in the district of Altona, Hamburg to run for 24 simulation hours with one agent (type `Citizen`) who is able to travel by walking on foot, riding a bicycle, and driving a car. The agent has a predefined daily routine which includes activities like work, errands, and others.
2. SOHGreen4Bikes: a model that is focused on travel by bike (to ease network congestion in and around the simulation environment) in a small section of the central part of the district of Harburg, Hamburg. The model is set to run with 10 agents (type: `Citizen`) for 24 simulation hours. These agents have predefined daily routines which include activities like work, errands, and others.
3. SOHFerryTransferBox: 

## Quick Start

Start and adjusting the model requires the following steps.

Clone the Git Repo:

```
git clone https://git.haw-hamburg.de/mars/model-smart-open-hamburg-po.git
```

Download and install the SDK for NetCore 3.1 from the official [website](https://dotnet.microsoft.com/download/dotnet-core/3.1).

Navigate into the cloned directory and make sure that all required dependencies are installed automatically by building the model in the directory where the SOHModel.sln file is located:

```
dotnet build
```

We have prepared a scenario in the project SOHModelStarter for the entry with 10000 agents that you can start immediately. To be able to analyze results afterwards, we recommend the use of a relational database for queries via SQL. Other formats are also possible. We have set a local SQlite as default for you. For other output settings, please refer to the [Online Documentation].

Navigate to the folder and start the model:

```
cd SOHModelStarter
dotnet run
```

The results of the model are stored after each simulated second for each agent involved. A new SQLite database with the file name *+mars.sqlite** was created in the folder **bin/Debug/netcoreapp3.1** where all data is stored.

Use your preferred tool for query or visualization. We recommend the tool [Falcon SQL](https://github.com/plotly/falcon) with which you can analyze relational queries in different ways with diagrams.