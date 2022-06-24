
# Python program to read
# json file
  
import csv     
import json
import sys  
# Opening JSON file
f = open('spawnares_polygon.json', encoding='utf-8')
   
# returns JSON object as 
# a dictionary
data = json.load(f)
fields=[]
print(sys.argv[5])
#startTime
fields.append(sys.argv[1])
#endTime
fields.append(sys.argv[2])
#spawningIntervalInMinutes
fields.append(sys.argv[3])
#spawningAmount
fields.append(sys.argv[4])
#source,destination
fields.append(data[sys.argv[5]])

#destination
fields.append(data[sys.argv[6]])

with open(r'passenger_traveler_schedule.csv', 'a', encoding='utf-8', newline="") as f:
    writer = csv.writer(f)
    writer.writerow(fields)