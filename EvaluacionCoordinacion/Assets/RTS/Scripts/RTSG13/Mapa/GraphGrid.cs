/*    
    Obra original:
        Copyright (c) 2018 Packt
        Unity 2018 Artificial Intelligence Cookbook - Second Edition, by Jorge Palacios
        https://github.com/PacktPublishing/Unity-2018-Artificial-Intelligence-Cookbook-Second-Edition
        MIT License

    Modificaciones:
        Copyright (C) 2020-2022 Federico Peinado
        http://www.federicopeinado.com

        Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
        Esta asignatura se imparte en la Facultad de Informática de la Universidad Complutense de Madrid (España).
        Contacto: email@federicopeinado.com
*/

namespace es.ucm.fdi.iav.rts
{

    using UnityEngine;
    using System;
    using System.Collections.Generic;

    public class GraphGrid : Graph
    {
        public bool get8Vicinity = true;
        public float cellSize = 1f;

        int numCols;
        int numRows;
        GameObject[] vertexObjs;
        public bool[,] mapVertices;
        int endCass;

        protected int GridToId(int x, int y)
        {
            return Math.Max(numRows, numCols) * y + x;
        }

        public Vector2 IdToGrid(int id)
        {
            Vector2 location = Vector2.zero;
            location.y = Mathf.Floor(id / numCols);
            location.x = Mathf.Floor(id % numCols);
            return location;
        }

        public GameObject getEndCass()
        {
            return vertices[endCass].gameObject;
        }

        private void LoadMap()
        {
            Vector3 position = Vector3.zero;
            Vector3 scale = Vector3.zero;
            int j = 0;
            int i = 0;
            int id = 0;

            vertices = new List<Vertex>(numRows * numCols);
            neighbors = new List<List<Vertex>>(numRows * numCols);
            costs = new float[numRows * numCols];
            vertexObjs = new GameObject[numRows * numCols];

            for (i = 0; i < numRows; i++)
            {
                for (j = 0; j < numCols; j++)
                {
                    position.x = i * cellSize;
                    position.z = j * cellSize;
                    id = GridToId(j, i);
                    vertexObjs[id] = Instantiate(vertexPrefab, position + transform.position + new Vector3(cellSize/2, 0.01f, cellSize/2), Quaternion.identity) as GameObject;

                    vertexObjs[id].name = "(Clone)" + id.ToString();
                    Vertex v = vertexObjs[id].AddComponent<Vertex>();
                    v.id = id;
                    vertices.Add(v);
                    neighbors.Add(new List<Vertex>());
                    float y = vertexObjs[id].transform.localScale.y;
                    scale = vertexObjs[id].transform.localScale * cellSize;

                    vertexObjs[id].transform.localScale = scale;
                    vertexObjs[id].transform.parent = gameObject.transform;
                }
            }

            // now onto the neighbours
            for (i = 0; i < numRows; i++)
            {
                for (j = 0; j < numCols; j++)
                {
                    SetNeighbours(j, i, get8Vicinity);
                }
            }
        }

        public override void Load()
        {
            Vector3 terrainSize = RTSGameManager.Instance.getMap().terrainData.size;
            numRows = (int)(terrainSize.x / cellSize);
            numCols = (int)(terrainSize.z / cellSize);
            LoadMap();
        }

        protected void SetNeighbours(int x, int y, bool get8 = true)
        {
            int col = x;
            int row = y;
            int i, j;
            int vertexId = GridToId(x, y);
            neighbors[vertexId] = new List<Vertex>();   //??????????
            costs[vertexId] = new float();
            Vector2[] pos = new Vector2[0];             //??????????
            if (get8)
            {
                pos = new Vector2[8];
                int c = 0;
                for (i = row - 1; i <= row + 1; i++)
                {
                    for (j = col - 1; j <= col + 1; j++)    //Añadida 3ª columna
                    {
                        if (i == row && j == col)
                            continue;
                        pos[c] = new Vector2(j, i);
                        c++;
                    }
                }
            }
            else
            {
                pos = new Vector2[4];
                pos[0] = new Vector2(col, row - 1);
                pos[1] = new Vector2(col - 1, row);
                pos[2] = new Vector2(col + 1, row);
                pos[3] = new Vector2(col, row + 1);
            }
            foreach (Vector2 p in pos)
            {
                i = (int)p.y;
                j = (int)p.x;
                if (i < 0 || j < 0 || i >= numRows || j >= numCols || (i == row && j == col) || !mapVertices[i, j])
                    continue;
                int id = GridToId(j, i);
                neighbors[vertexId].Add(vertices[id]);
            }
        }

        public override float[] GetNeighboursCosts(int vertId)
        {
            Vector2[] pos = new Vector2[4];
            Vector2 p = IdToGrid(vertId);
            pos[0] = new Vector2(p.x, p.y - 1);
            pos[1] = new Vector2(p.x - 1, p.y);
            pos[2] = new Vector2(p.x + 2, p.y);
            pos[3] = new Vector2(p.x, p.y + 1);
            int tam = neighbors[vertId].Count;
            float[] n = new float[tam];
            for (int i = 0; i < tam; i++)
            {
                int x = (int)pos[i].y;
                int y = (int)pos[i].x;
                if (x < 0 || y < 0 || x >= numRows || y >= numCols || !mapVertices[x, y])
                    continue;
                n[i] = costs[GridToId(x, y)];
            }
            return n;
        }

        public override Vertex GetNearestVertex(Vector3 position)
        {
            int col = (int)Mathf.Round(position.x / cellSize);
            int row = (int)Mathf.Round(position.z / cellSize);
            int id = GridToId(col, row);
            return vertices[id];
        }
        public int GetNearestVertexId(Vector3 position)
        {
            int col = (int)Mathf.Round(position.x / cellSize);
            int row = (int)Mathf.Round(position.z / cellSize);
            
            return GridToId(col, row);
        }

        public GameObject randCass()
        {
            int cass = 0;
            Vector2 pos;
            do
            {
                cass = UnityEngine.Random.Range(0, vertices.Count - 1);
                pos = IdToGrid(cass);
            }
            while (!mapVertices[(int)pos.x, (int)pos.y]);
            return vertices[cass].gameObject;
        }

    }
}
