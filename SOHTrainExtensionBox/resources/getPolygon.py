# Python program to read
# json file
  
  
import json
  
# Opening JSON file
f = open('spawn_polygon.geojson', encoding='utf-8')
  
# returns JSON object as 
# a dictionary
data = json.load(f)
cleanData={}
for feature in data["features"]:
    poligyon ="MULTIPOLYGON ((("
    for coordinate in feature["geometry"]["coordinates"][0][0]:
        poligyon+=str(coordinate[0])+" "+str(coordinate[1])+","
    poligyon = poligyon[:-1]
    poligyon+=")))"
    cleanData[feature["properties"]["Name"]] = poligyon

print(cleanData)

# Closing file
f.close()
with open('spawnares_polygon.json', 'w', encoding='utf-8') as f:
    json.dump(cleanData, f, ensure_ascii=False, indent=4)