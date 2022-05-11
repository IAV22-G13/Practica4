using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace es.ucm.fdi.iav.rts
{
    struct City
    {
        public GameObject c;   //Elemento del bando
        public float strength;
        public static bool operator ==(City a, City b) => a.c == b.c;
        public static bool operator !=(City a, City b) => a.c != b.c;
    };
    class LocationRecord : IComparable<LocationRecord>
    {
        public Vertex location;
        public City nearestCity;
        public float strenght;

        public int CompareTo(LocationRecord b)
        {
            if (this.strenght < b.strenght) return -1;
            else if (this.strenght > b.strenght) return 1;
            return 0;
        }
    }
    public class MapaInfluencia : GraphGrid
    {
        //    # The strength function has this format.
        float strengthFunction(City c, Vector3 l)
        {
            Vector2 cityPos = IdToGrid(GetNearestVertexId(c.c.transform.position));
            Vector2 pos = IdToGrid(GetNearestVertexId(l));

            Vector2 dist = new Vector2(Math.Abs(cityPos.x - pos.x), Math.Abs(cityPos.y - pos.y));
            float d = c.strength - ((int)dist.magnitude * 0.2f);
            if (d < 0)
                return 0;
            return d;
        }

        List<LocationRecord> mapFloodDijkstra(Graph map, City[] cities, float strenghTreshold, Func<City, Vector3, float> strengthFunction)
        {
            List<LocationRecord> open = new List<LocationRecord>();
            List<LocationRecord> closed = new List<LocationRecord>();

            for (int i = 0; i < cities.Length; i++)
            {
                LocationRecord startRecord = new LocationRecord();
                startRecord.location = map.GetNearestVertex(cities[i].c.transform.position);
                startRecord.nearestCity = cities[i];
                startRecord.strenght = cities[i].strength;
                open.Add(startRecord);
            }

            while (open.Count > 0)
            {
                LocationRecord c = open[0];
                for (int i = 0; i < neighbors[c.location.id].Count; i++)
                {
                    LocationRecord act = new LocationRecord();
                    act.location = neighbors[c.location.id][i];
                    float strenght = strengthFunction(c.nearestCity, act.location.transform.position);
                    act.strenght = strenght;
                    act.nearestCity = c.nearestCity;

                    if (act.strenght < strenghTreshold)
                        continue;
                    else if (closed.Contains(act))
                    {
                        act = closed.Find(LocationRecord => LocationRecord.location == act.location);
                        if (act.nearestCity != c.nearestCity && act.strenght > strenght)
                            continue;
                    }
                    else if (open.Contains(act))
                    {
                        act = open.Find(LocationRecord => LocationRecord.location == act.location);
                        if (act.nearestCity != c.nearestCity && act.strenght > strenght)
                            continue;
                    }
                    else
                    {
                        act = new LocationRecord();
                        act.location = neighbors[c.location.id][i];
                    }

                    act.nearestCity = c.nearestCity;
                    act.strenght = strenght;

                    if (!open.Contains(act))
                        open.Add(act);
                }

                open.Remove(c);
                closed.Add(c);
            }
            return closed;
        }
    }
    //11 function mapfloodDijkstra(map: Map, cities: City[], strengthThreshold: float, strengthFunction: function) -> LocationRecord[]:
    //29 # Iterate through processing each node.

    //37 # Loop through each location in turn.
    //38 for location in locations:

    //46 # .. or if closed and we’ve found a worse route.
    //47 else if closed.contains(location):
    //48 # Find the record in the closed list.
    //49 neighborRecord = closed.find(location)
    //50 if neighborRecord.city != current.city and
    //51 neighborRecord.strength<strength:
    //52 continue
    //53
    //54 # .. or if it is open and we’ve found a worse
    //55 # route.
    //56 else if open.contains(location):
    //57 # Find the record in the open list.
    //58 neighborRecord = open.find(location)
    //59 if neighborRecord.strength<strength:
    //60 continue
    //61
    //62 # Otherwise we know we’ve got an unvisited
    //63 # node, so make a record for it.
    //64 else:
    //65 neighborRecord = new NodeRecord()
    //66 neighborRecord.location = location
    //67
    //68 # We’re here if we need to update the node
    //69 # Update the cost and connection.
    //70 neighborRecord.city = current.city
    //71 neighborRecord.strength = strength
    //72
    //73 # And add it to the open list.
    //74 if not open.contains(location):
    //75 open += neighborRecord
    //76
    //77 # We’ve finished looking at the neighbors for the current
    //78 # node, so add it to the closed list and remove it from the
    //79 # open list.
    //80 open -= current
    //81 closed += current
    //82
    //83 # The closed list now contains all the locations that belong to
    //84 # any city, along with the city they belong to.
    //85 return closed

}