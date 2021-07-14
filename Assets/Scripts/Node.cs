using System;
using System.Collections.Generic;
using Managers.EventManager;
using UI;
using UnityEngine;

public class Node : MonoBehaviour
{
    private string _name;
    [SerializeField] private List<Node> connections;
    public List<Node> Connections => connections;

    [SerializeField] private LineRenderer lr;

    [SerializeField] private GameObject prefabTemp;
    [SerializeField] private float lineSize;

    [SerializeField] private SpriteRenderer mat;
    private Color userSetColour = new Color();

    public void SetName(string str)
    {
        _name = str;
    }

    public void SetColour(Color color)
    {
        mat.color = color;
        userSetColour = color;
    }

    public void SetSize(float size)
    {
        transform.localScale = new Vector3(size, size, size); //TODO support dynamic size scaling of node.
    }

    public void AddNodeConnection(Node node, bool updateOtherNode = true)
    {
        if (node == this)
        {
            Debug.LogError("Node was given as itself. Returning");
            return;
        }

        foreach (var n in connections) // TODO make connections a dictionary
        {
            if (n == node)
            {
                return;
            }
        }

        if (updateOtherNode)
        {
            // tell the other node that we're now connected. //TODO render line one way -- Every node renders a line
            node.AddNodeConnection(this, false);
        }

        connections.Add(node);
        Vector3 startNode = this.transform.position;
        Vector3 endNode = node.transform.position;
        var positionCount = lr.positionCount;
        positionCount += 2;
        lr.positionCount = positionCount;
        var posCount = positionCount; // rider IDE likes it like this, not sure why this is better.
        lr.SetPosition(posCount - 2, startNode);
        lr.SetPosition(posCount - 1, endNode);
        lr.enabled = true;
        lr.startWidth = lineSize / 100;
        lr.endWidth = lineSize / 100;
    }

    public void RemoveNodeConnection(Node node, bool informOtherNode = true)
    {
        if (informOtherNode)
        {
            node.RemoveNodeConnection(this, false);
        }
        connections.Remove(node); //TODO remove line renderer once destroyed
        RedrawLineRendererConnections(false);
    }

    public void Highlight(bool toggle)
    {
        if (toggle)
        {
            mat.color = Color.red;
            return;
        }
        mat.color = userSetColour;
    }
    
    public void RedrawLineRendererConnections(bool allNodes = true) //TODO:probably more expensive than it should be
    {
        lr.positionCount = 0;
        lr.enabled = false;
        lr.startWidth = lineSize / 100;
        lr.endWidth = lineSize / 100;
        foreach (var n in connections)
        {
            if (allNodes)
            {
                n.RedrawLineRendererConnections(false);
            }

            var positionCount = lr.positionCount;
            positionCount += 2;
            lr.positionCount = positionCount;
            var posCount = positionCount; // rider IDE likes it like this, not sure why this is better.
            lr.SetPosition(posCount - 2, this.transform.position);
            lr.SetPosition(posCount - 1, n.transform.position);
        }

        lr.enabled = true;
    }

    private void OnDestroy()
    {
        var TempList = new List<Node>(); //TODO:fix this shit 
        foreach (var n in connections)
        {
            TempList.Add(n);
        }
        foreach (var nodeConnection in TempList)
        {
            nodeConnection.RemoveNodeConnection(this);
        }

        foreach (var connection in connections)
        {
            connection.RedrawLineRendererConnections();
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

    private void OnMouseExit()
    {
        EventManager.RaiseEvent("OnMouseExitNode");
    }
}