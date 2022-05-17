<h1 align="center">SmartOpenHH | <a href="https://mars.haw-hamburg.de">Website</a></h1>

The SmartOpenHamburg model is an agent-based simulation model for the representation of a digital twin. It calculates multi-modal path dynamics and simulates a defined daily routine of a citizen.

## Contents

The repository contains three projects and a number of libraries to support them. For more information on each model and the libraries, please check out the README.md in the respective subdirectory.

## Quick Start

Start and adjusting the model requires the following steps.

Clone the Git Repo:

```
git clone https://git.haw-hamburg.de/mars/model-smart-open-hamburg-po.git
```

Download and install the SDK for NetCore from the official [website](https://dotnet.microsoft.com/download/dotnet-core/).  Navigate into the cloned directory and make sure that all required dependencies are installed automatically by building the model in the directory where the SOHModel.sln file is located:

```
dotnet build
```

We have prepared a scenario in the project ``SOHTravellingBox`` for the entry with agents, travelling within the area of Hamburg Dammtor, which you can start immediately. 

Navigate to the folder and start the model:

```
cd SOHTravellingBox
dotnet run
```

This runs the simulation and creates a file call `trips.geojson`. Open [kepler.gl](https://kepler.gl/demo) and import the file via drag & drop. See the trajectories which were computed by the simulation.


## Update local Model Development

Since we cannot update your own model development with this as baseline. We recommend to use this Git repository as *second* Git source. Therefore

Create your own git repo (*if not already done*) in your desired local folder.
``bash
git init
``

Add this repository as an Git remote URL beside your own repo address.

``bash
git remote add soh git@git.haw-hamburg.de:mars/model-smart-open-hamburg-po.git
``

This command will add one more remote source to your local git repo called ``soh``.

In case of any updates, use the the ``git pull`` command to fetch and load any remote changes into your local repository area:

```bash
git pull soh master
```

This will pull the `master` branch and all its changes into your own repository. In case that something has a conflict with your own model code, resolve this my creating an own project with unique name as recommendation.
