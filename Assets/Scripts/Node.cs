using System;
using System.Collections.Generic;
using Managers.EventManager;
using UnityEngine;

public class Node : MonoBehaviour
{
    private string _name;
    [SerializeField] private List<Node> connections;
    public List<Node> Connections => connections;

    [SerializeField] private LineRenderer lr;

    [SerializeField] private GameObject prefabTemp;
    [SerializeField] private float lineSize;

    public Node(List<Node> connections, string name, LineRenderer lr)
    {
        this.connections = connections;
        _name = name;
        this.lr = lr;
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
        var positionCount = lr.positionCount;
        positionCount += 2;
        lr.positionCount = positionCount;
        var posCount = positionCount;
        lr.SetPosition(posCount - 2, startNode);
        lr.SetPosition(posCount - 1, endNode);
        lr.enabled = true;
        lr.startWidth = lineSize / 100;
        lr.endWidth = lineSize / 100;
    }

    private Vector3 GetConnectionPointFromNode(Node node)
    {
        //TODO Figure out direction
        var scale = node.transform.localScale;
        var pos = node.transform.position;
        float posX;
        float posY;
        var distance = Vector3.Distance(this.transform.position, pos);
        var angle = Vector3.Angle(this.transform.position, pos);
        Debug.Log(distance);
        return new Vector3(pos.x, pos.y + (scale.y/2), pos.z);
    }
    
    public void RemoveNodeConnection(Node node)
    {
        connections.Remove(node); //TODO remove line renderer once destroyed
    }

    private void OnDestroy()
    {
        foreach (var nodeConnection in connections)
        {
            nodeConnection.RemoveNodeConnection(this);
        }
    }

    private void OnMouseDown()
    {
        EventManager.RaiseEvent("NodeMouseDown", this);
    }

    private void OnMouseOver()
    {
        EventManager.RaiseEvent("NodeMouseUp", this);
        if (Input.GetMouseButtonDown(1))
        {
            EventManager.RaiseEvent("NodeMouseRightClick", this);
        }
    }
}