using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/Map Layout Definition")]
public class MapLayoutDefinition : ScriptableObject
{
    [Serializable]
    public class Node
    {
        public string id;
        public Vector3 position;
        public bool isStart;
        public bool isEnd;          // boss / last fight
        public bool isPOI;          // randomized content placed here at run start
    }

    [Serializable]
    public class Edge
    {
        public string fromId;
        public string toId;
    }

    public List<Node> nodes = new();
    public List<Edge> edges = new();

    public Node GetNode(string id)
    {
        for (int i = 0; i < nodes.Count; i++)
            if (nodes[i].id == id) return nodes[i];
        return null;
    }

    public string GetStartNodeId()
    {
        for (int i = 0; i < nodes.Count; i++)
            if (nodes[i].isStart) return nodes[i].id;
        return null;
    }

    public string GetEndNodeId()
    {
        for (int i = 0; i < nodes.Count; i++)
            if (nodes[i].isEnd) return nodes[i].id;
        return null;
    }

    public List<string> GetOutgoing(string fromId)
    {
        var list = new List<string>();
        for (int i = 0; i < edges.Count; i++)
            if (edges[i].fromId == fromId) list.Add(edges[i].toId);
        return list;
    }

    public List<string> GetIncoming(string toId)
    {
        var list = new List<string>();
        for (int i = 0; i < edges.Count; i++)
            if (edges[i].toId == toId) list.Add(edges[i].fromId);
        return list;
    }
}