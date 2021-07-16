using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    private static SceneManager _instance;
    public static SceneManager Instance
    {
        get
        {
            if (_instance is null)
            {
                var obj = new GameObject("UIManager");
                obj.AddComponent<SceneManager>();
                _instance = obj.GetComponent<SceneManager>();
            }

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    private Dictionary<GameObject, Node> _activeNodes = new Dictionary<GameObject, Node>();
    public List<Node> VisualNodeIndicator = new();

    public Dictionary<GameObject, Node> GetActiveNodes()
    {
        return _activeNodes;
    }

    public void AddNodeToDictionary(Node node)
    {
        _activeNodes.Add(node.gameObject,node);
        VisualNodeIndicator.Add(node);
    }

    public void RemoveNodeFromDictionary(Node node)
    {
        _activeNodes.Remove(node.gameObject);
        VisualNodeIndicator.Remove(node);
    }
}
