using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace es.ucm.fdi.iav.rts
{
    struct City { };
    struct Location { };
    struct Map { };
    struct LocationRecord
    {
        Location location;
        City nearestCity;
        float strenght;
    }
    public class MapaInfluencia : MonoBehaviour
    {
        //    # The strength function has this format.
        float strengthFunction(City c, Location l)
        {

            return 0;
        }

        LocationRecord[] mapFloodDijkstra(Map map, City[] cities, float strenghTreshold, Func<City, Location, float> strengthFunction)
        {

            return null;
        }
    }
    //11 function mapfloodDijkstra(map: Map, cities: City[], strengthThreshold: float, strengthFunction: function) -> LocationRecord[]:
    //16
    //17 # Initialize the open and closed lists.
    //18 open = new PathfindingList()
    //19 closed = new PathfindingList()
    //20
    //21 # Initialize the record for the start nodes.
    //22 for city in cities:
    //23 startRecord = new LocationRecord()
    //24 startRecord.location = city.getLocation()
    //25 startRecord.city = city
    //26 startRecord.strength = city.getStrength()
    //27 open += startRecord
    //28
    //29 # Iterate through processing each node.
    //30 while open:
    //31 # Find the largest element in the open list.
    //32 current = open.largestElement()
    //33
    //34 # Get its neighboring locations.
    //35 locations = map.getNeighbors(current.location)
    //36
    //37 # Loop through each location in turn.
    //38 for location in locations:
    //39 # Get the strength for the end node.
    //40 strength = strengthFunction(current.city, location)
    //41
    //42 # Skip if the strength is too low.
    //43 if strength<strengthThreshold:
    //6.2 Tactical Analyses 531
    //44 continue
    //45
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