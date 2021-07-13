using System;
using System.Collections;
using System.Collections.Generic;
using Managers.EventManager;
using UnityEngine;
using UnityEngine.Serialization;

public class Node : MonoBehaviour
{
    private string _name;
    [SerializeField]
    private List<Node> connections;
    [SerializeField]
    private LineRenderer _lr;

    [SerializeField] private float lineSize;

    public Node(List<Node> connections, string name, LineRenderer lr)
    {
        this.connections = connections;
        _name = name;
        _lr = lr;
    }

    public void AddNodeConnection(Node node)
    {
        foreach (var n in connections) // TODO make connections a dictionary
        {
            if (n == node)
            {
                return;
            }
        }
        connections.Add(node);
        Vector3 startNode = this.transform.position;
        Vector3 endNode = node.transform.position;
        var positionCount = _lr.positionCount;
        positionCount += 2;
        _lr.positionCount = positionCount;
        var posCount = positionCount;
        _lr.SetPosition(posCount-2,startNode);
        _lr.SetPosition(posCount-1,endNode);
        _lr.enabled = true;
        _lr.startWidth = lineSize/100;
        _lr.endWidth = lineSize/100;
    }

    public void RemoveNodeConnection(Node node)
    {
        connections.Remove(node);
    }

    private void OnMouseDown()
    {
        EventManager.RaiseEvent("NodeMouseDown",this);
    }

    private void OnMouseOver()
    {
        EventManager.RaiseEvent("NodeMouseUp",this);
        if (Input.GetMouseButtonDown(1))
        {
            EventManager.RaiseEvent("NodeMouseRightClick",this);
        }
    }
}
