using System.Collections.Generic;
using System.IO;
using Mars.Interfaces.Environments;
using ServiceStack;

namespace SOHFloodBox;

public class HeightModel
{


    List<height_data> list_height_data= new();
    
    public void read()
    {
        using(var reader = new StreamReader("resources/veddel_height.csv"))
        {
            var line = reader.ReadLine();
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                var values = line.Split(',');

                list_height_data.Add(new height_data(values[2].ToDouble(), values[3].ToDouble(), values[4].ToDouble()));
            }
        }
    }

    public double detect_best_height(Position position)
    {
        ;
        double distance_alt = 1000;
        height_data part_nearest = new height_data(0,0,0);
        foreach (height_data part in list_height_data)
        {
            double distance = position.DistanceInMTo(part.lon, part.lat);
            if (distance < distance_alt)
            {
                distance_alt = distance;
                part_nearest = part;
            }
        }

        return part_nearest.height;
    }

    public struct height_data
    {
        public double lat { get; set; }
        public double lon { get; set; }
        public double height { get; set; }
        public height_data(double lat, double lon, double height)
        {
            this.lat = lat;
            this.lon = lon;
            this.height = height;
        }

    }
    

    
    
}