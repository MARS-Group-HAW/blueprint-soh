###
### Disclamer: Dieser Code ist nicht für fremde Augen gedacht. ACHTUNG! Gefahr auf unreperable Schäden
###

import csv
import json
import re

trip = open('../HVV_GTFS/trips.txt', encoding="utf-8")
trip_neu = open('trips_neu.txt', encoding="utf-8")
routes = open('../HVV_GTFS/routes.txt', encoding="utf-8")
stop = open('../HVV_GTFS/stops.txt', encoding="utf-8")
stop_times = open('../HVV_GTFS/stop_times.txt', encoding="utf-8")
stop_times_neu = open('stoptimes_neu.txt', encoding="utf-8")

#tripreader = csv.reader(trip)

tripreader = csv.reader(trip)
trip_neureader = csv.reader(trip_neu)
routesreader = csv.reader(routes)
stopreader = csv.reader(stop)
stop_timesreader = csv.reader(stop_times)
stop_times_neureader = csv.reader(stop_times_neu)




tripdata = []
routesdata = []
stopdata = []
stop_timesdata = []
regex = 'S[0-9][0-9]?'


s_trip = ['9476_109', '9474_109', '9475_109', '9473_109', '9472_109', '5111_109']

tripheader = []
tripheader = next(tripreader)

trip_neuheader = []
trip_neuheader = next(trip_neureader)

routesheader = []
routesheader = next(routesreader)

stopheader = []
stopheader = next(stopreader)

stop_timesheader = []
stop_timesheader = next(stop_timesreader)

stop_times_neuheader = []
stop_times_neuheader = next(stop_times_neureader)

#for row in routesreader:
 #    if re.match(regex, row[2]):
 #        routesdata.append(row)
# 
# filename = 'routes_neu.txt'
# 
# with open(filename, 'w', newline="", encoding="utf-8") as file:
#     csvwriter = csv.writer(file, quoting=csv.QUOTE_NONNUMERIC)
#     csvwriter.writerow(routesheader)
#     csvwriter.writerows(routesdata)

# filename = 'trips_neu.txt'
# with open(filename, 'w', newline="", encoding="utf-8") as file:
#     csvwriter = csv.writer(file, quoting=csv.QUOTE_NONNUMERIC)
#     csvwriter.writerow(tripheader)
#     for row in tripreader:
#         if row[0] in s_trip:
#             tripdata.append(row)
#             csvwriter.writerow(row)
stoptimes = []
for row in stop_times_neureader:
    stoptimes.append(row[3])


with open('stops_neu.txt', 'w', newline="", encoding="utf-8") as file:
     csvwriter = csv.writer(file, quoting=csv.QUOTE_NONNUMERIC)
     csvwriter.writerow(stopheader)
     for row in stopreader:
        if row[0] in stoptimes:
             csvwriter.writerow(row)


#print(json.dumps(tripdata, ident=4))