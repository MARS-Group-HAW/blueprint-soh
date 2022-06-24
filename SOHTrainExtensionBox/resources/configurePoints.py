# Python program to read
# json file
  
  
import json
  
# Opening JSON file
f = open('bot_spawn_area2.geojson', encoding='utf-8')
fid=4
# returns JSON object as 
# a dictionary
data = json.load(f)

for feature in data["features"]:
    if (feature["geometry"]["type"]=="Point"):
        feature["geometry"]["type"]="MultiPoint"
        feature["properties"]["fid"] = fid
        feature["properties"]["y"] = feature["geometry"]["coordinates"][0]
        feature["properties"]["x"] = feature["geometry"]["coordinates"][1]
        feature["geometry"]["coordinates"] = [feature["geometry"]["coordinates"]]
    else:
        feature["properties"]["fid"] = fid
    
    
    
    fid+=1

print(data["features"])

# Closing file
f.close()
with open('bot_spawn_area3.geojson', 'w', encoding='utf-8') as f:
    json.dump(data, f, ensure_ascii=False, indent=4)
