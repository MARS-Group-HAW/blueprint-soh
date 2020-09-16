<h1 align="center">SmartOpenHH | <a href="https://mars.haw-hamburg.de">Website</a></h1>

The SmartOpenHamburg model is an agent-based simulation model for the representation of a digital twin. It calculates multi-modal path dynamics and simulates a defined daily routine of a citizen.

## Quick Start

Start and adjusting the model requires the following steps.

Clone the Git Repo:

```
git clone https://git.haw-hamburg.de/mars/model-smart-open-hamburg-po.git
```

Download and install the SDK for NetCore 3.1 from the official [website] (https://dotnet.microsoft.com/download/dotnet-core/3.1).

Navigate into the cloned directory and make sure that all required dependencies are installed automatically by building the model in the directory where the LIFE.sln file is located:

```
dotnet build
```

We have prepared a scenario in the project SOHModelStarter for the entry with 10000 agents that you can start immediately. To be able to analyze results afterwards, we recommend the use of a relational database for queries via SQL. Other formats are also possible. We have set a local SQlite as default for you. For other output settings, please refer to the [Online Documentation].

Navigate to the folder and start the model:

```
dotnet run
```

The results of the model are stored after each simulated second for each agent involved. A new SQLite database with the file name *+mars.sqlite** was created in the folder **bin/Debug/netcoreapp3.1** where all data is stored.

Use your preferred tool for query or visualization. We recommend the tool [Falcon SQL] (https://github.com/plotly/falcon) with which you can analyze relational queries in different ways with diagrams.