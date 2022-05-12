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
        public delegate int Comparison<in T>(T x, T y);

        public int CompareTo(LocationRecord b)
        {
            if (this.strenght > b.strenght) return -1;
            else if (this.strenght < b.strenght) return 1;
            return 0;
        }

        public override bool Equals(object obj)
        {
            return obj is LocationRecord record &&
                   EqualityComparer<Vertex>.Default.Equals(location, record.location) &&
                   EqualityComparer<City>.Default.Equals(nearestCity, record.nearestCity) &&
                   strenght == record.strenght;
        }

        public override int GetHashCode()
        {
            int hashCode = -1012343204;
            hashCode = hashCode * -1521134295 + EqualityComparer<Vertex>.Default.GetHashCode(location);
            hashCode = hashCode * -1521134295 + nearestCity.GetHashCode();
            hashCode = hashCode * -1521134295 + strenght.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(LocationRecord a, LocationRecord b)
        {
            if (a is null)
                return b is null;
            if (b is null)
                return a is null;
            return a.location == b.location && a.nearestCity == b.nearestCity && a.strenght == b.strenght;
        }
        public static bool operator !=(LocationRecord a, LocationRecord b) => !(a == b);
    }
    public class MapaInfluencia : GraphGrid
    {

        private bool harkonnen = true;
        private bool fremen = true;
        private bool graben = false;

        [SerializeField]
        float actFloorPaint = 2.0f;
        float timer;
        List<GameObject> painted;

        //    # The strength function has this format.

        public void Awake()
        {
            painted = new List<GameObject>();
            timer = actFloorPaint;
        }
        float strengthFunction(City c, Vector3 l)
        {
            Vector2 cityPos = IdToGrid(GetNearestVertexId(c.c.transform.position));
            Vector2 pos = IdToGrid(GetNearestVertexId(l));

            Vector2 dist = new Vector2(Math.Abs(cityPos.x - pos.x), Math.Abs(cityPos.y - pos.y));
            float d = c.strength - ((int)dist.magnitude * 0.2f);
            if (d < 0)
                return 0;
            if (d > 1)
                return 1;
            return d;
        }

        List<LocationRecord> mapFloodDijkstra(City[] cities, float strenghTreshold, Func<City, Vector3, float> strengthFunction)
        {
            List<LocationRecord> open = new List<LocationRecord>();
            List<LocationRecord> closed = new List<LocationRecord>();

            for (int i = 0; i < cities.Length; i++)
            {
                LocationRecord startRecord = new LocationRecord();
                startRecord.location = GetNearestVertex(cities[i].c.transform.position - transform.position - new Vector3(cellSize / 2, 0, cellSize / 2));
                startRecord.nearestCity = cities[i];
                startRecord.strenght = cities[i].strength;
                open.Add(startRecord);
            }

            while (open.Count > 0)
            {
                open.Sort();
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
                        if ((act.nearestCity != c.nearestCity && act.strenght > strenght) || (act.nearestCity == c.nearestCity))
                            continue;
                    }
                    else if (open.Contains(act))
                    {
                        act = open.Find(LocationRecord => LocationRecord.location == act.location);
                        if ((act.nearestCity != c.nearestCity && act.strenght > strenght) || (act.nearestCity == c.nearestCity))
                            continue;
                    }

                    act.nearestCity = c.nearestCity;
                    act.strenght = strenght;

                    if (!open.Contains(act))
                        open.Add(act);
                }

                open.Remove(c);
                if (closed.Contains(c))
                    closed.Remove(c);
                closed.Add(c);
            }
            return closed;
        }

        private City[] getUnits(int i)
        {
            List<BaseFacility> facilyBase = RTSGameManager.Instance.GetBaseFacilities(i);
            List<ProcessingFacility> facilyProccess = RTSGameManager.Instance.GetProcessingFacilities(i);
            List<ExtractionUnit> extraction = RTSGameManager.Instance.GetExtractionUnits(i);
            List<ExplorationUnit> explorer = RTSGameManager.Instance.GetExplorationUnits(i);
            List<DestructionUnit> destruction = RTSGameManager.Instance.GetDestructionUnits(i);

            int sum = facilyBase.Count + facilyProccess.Count + extraction.Count + explorer.Count + destruction.Count;
            City[] c = new City[sum];

            int k = 0;
            for (int j = 0; j < facilyBase.Count; j++)
            {
                c[k].c = facilyBase[j].gameObject;
                c[k].strength = 1.0f;
                k++;
            }

            for (int j = 0; j < facilyProccess.Count; j++)
            {
                c[k].c = facilyProccess[j].gameObject;
                c[k].strength = 0.75f;
                k++;
            }

            for (int j = 0; j < extraction.Count; j++)
            {
                c[k].c = extraction[j].gameObject;
                c[k].strength = 0.34f;
                k++;
            }

            for (int j = 0; j < explorer.Count; j++)
            {
                c[k].c = explorer[j].gameObject;
                c[k].strength = 0.45f;
                k++;
            }

            for (int j = 0; j < destruction.Count; j++)
            {
                c[k].c = destruction[j].gameObject;
                c[k].strength = 0.61f;
                k++;
            }

            return c;
        }

        public void Update()
        {
            timer += Time.deltaTime;
            if (timer > actFloorPaint)
            {
                Colour();
                timer = 0;
            }
        }

        private void Colour()
        {
            for (int i = 0; i < painted.Count; i++)
            {
                GameObject o = painted[i];
                o.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 0);
            }
            painted.Clear();

            List<LocationRecord> har = new List<LocationRecord>();
            List<LocationRecord> fre = new List<LocationRecord>();
            List<LocationRecord> gra = new List<LocationRecord>();

            if (harkonnen)
            {
                City[] c = getUnits(0);
                har = mapFloodDijkstra(c, 0.09f, strengthFunction);
            }
            if (fremen)
            {
                City[] c = getUnits(1);
                fre = mapFloodDijkstra(c, 0.09f, strengthFunction);
            }
            if (graben)
            {

            }

            for (int i = 0; i < Math.Max(har.Count, fre.Count); i++)
            {
                if (i < har.Count)
                {
                    float diff = har[i].strenght;
                    GameObject o = vertexObjs[har[i].location.id];
                    o.GetComponent<MeshRenderer>().material.color += Color.red * diff;
                    painted.Add(o);
                }

                if (i < fre.Count)
                {
                    float diff = fre[i].strenght;
                    GameObject o = vertexObjs[fre[i].location.id];
                    Color act = o.GetComponent<MeshRenderer>().material.color;
                    act += Color.blue * diff;
                    if (act.r > 1) act.r = 1;
                    if (act.g > 1) act.g = 1;
                    if (act.b > 1) act.b = 1;
                    if (act.a > 1) act.a = 1;
                    o.GetComponent<MeshRenderer>().material.color = act;
                    painted.Add(o);
                }
            }
        }
    }
}