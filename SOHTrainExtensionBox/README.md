<h1 align="center">S-Bahn Extension Hamburg</h1>


<span style="color:red">All data and created content of these data, as well as the results of this simulation are subject to a confidentiality agreement with Deutsche Bahn.</span>

The S-Bahn Hamburg MARS Model is a model which attempts to create a realistic simulation environment of the S-Bahn network of Hamburg. In this network, the individual line utilization is to be measured and optimization possibilities are to be built in.
## Contents

Data from Deutsche Bahn (DB), as well as data from the Transparency Portal Hamburg, was used to create the content contained in this report.

---
## Quick Start

Start and adjusting the model requires the following steps.

Download and install the SDK for NetCore from the official [website](https://dotnet.microsoft.com/download/dotnet-core/).  Navigate into the cloned directory and make sure that all required dependencies are installed automatically by building the model in the directory where the SOHModel.sln file is located:

```
dotnet build
```

We have prepared a scenario in the project ``SOHTrainExtensionBox`` for the entry with agents, travelling within the area of Hamburg with trains to various location in Hamburg, which you can start immediately. 

Navigate to the folder and start the model:

```
dotnet run
```

This runs the simulation and creates a file call `trips.geojson`. Open [kepler.gl](https://kepler.gl/demo) and import the file via drag & drop. See the trajectories which were computed by the simulation.

### Simulation duration

Startpoint and endpoint of simulation can be changed in **SOHTrainBox/config.json**.
```
"startPoint": "2022-09-01T06:00:00",
"endPoint": "2022-09-01T07:00:00",
```

### PassengerTraveller GeoJSON

Instead of GeoJSON from TrainDriver you can get GeoJSON from PassengerTraveller.<br>
Remove _outputs_ and _kind_ in **SOHTrainExtensionBox/config.json** in **TrainDriver** section and add to **PassengerTraveller** section.
```
    {
      "name": "PassengerTraveler",
        "outputs": [
            {
                "kind": "trips"
            },
            {
                "kind": "csv"
            }
        ]
    },
```

### Expand passenger traveller schedule

Expanding the passenger traveller schedule needs an execution of the script **SOHTrainExtensionBox/setSpawnGoal.py** with parameters starttime, endtime, intervall in minutes, amount passengers, source station and destination station.


```
python setSpawnGoal.py [STARTTIME] [ENDTIME] [INTERVALL_MINUTES] [SPAWN_COUNT] [SOURCE] [DESTINATION]
```

#### _Examples:_
```
python setSpawnGoal.py 06:00 10:00 1 10 Nord Süd
python setSpawnGoal.py 08:00 09:00 5 1 Harburg Altona
```
Every one minute from 6:00 to 10:00 AM ten passengers spawn from "Nord"-Area to "Süd"-Area.<br>
Every five minutes from 8:00 to 9:00 AM one passenger spawns from "Harburg" to "Altona. [Hamburg S-Bahn stations (HVV)](https://www.hvv.de/resource/blob/22142/79d42a5dee6a01d769384dbac8c50fa2/usar-plan-data.pdf)

![alt text](SOHTrainBox/images/hamburg_spawnareas.png "Hamburg Spawn Areas")
(_Nord = North, Mitte = Center, Ost = East, West = West_)


## Results

For better understanding of the data, we generated heatmaps with Tableau using the generated .csv files. Through the PassengerCount Variable in TrainDriver, we were able to track how many passengers were on the train at each tick at the current time. This allowed us to monitor the
the driving behavior of the passengers with respect to the new line very well.

![alt text](SOHTrainBox/images/HeatmapBeforeOpti.png "Heatmap before optimization")
(Heatmap before improvement)

![alt text](SOHTrainBox/images/HeatmapAfterOpti.png "Heatmap after optimization")
(Heatmap after inserting the new S5 route)

The heat map shows very clearly that passengers coming from the south-west with destination Altona take the new S5 route.
This relieves the entire Hamburg-Mitte route network, since the S3/S31 no longer has to transport so many passengers.

Improving the utilization of the S3/S31 route would also have a positive side effect, which would be that the frequency of the S31
could be slowly reduced over time.










