using System;
using System.Collections;
using System.Collections.Generic;
using Managers.EventManager;
using UnityEngine;

public class UserInteraction : MonoBehaviour
{
    private Vector3 _initalPosition = new Vector3();
    [SerializeField]
    private LineRenderer _lr = new();
    [SerializeField]
    private Node _selectedNode;
    [SerializeField]
    private Node _endNode;
    private Camera _mainCam;

    private void OnEnable()
    {
       _mainCam = Camera.main;
       EventManager.Listen("NodeMouseDown", (Action<Node>) OnNodeMouseDown);
       EventManager.Listen("NodeMouseUp",(Action<Node>) OnNodeMouseUp);
       EventManager.Listen("NodeMouseRightClick",(Action<Node>) OnNodeMouseRightClick);
    }

    private void OnNodeMouseRightClick(Node node)
    {
        Debug.Log("NODE:"+node);
        if(_selectedNode != node)
        {
            _endNode = node;
        }
    }
    private void OnNodeMouseUp(Node node)
    {
        Debug.Log("NODE:"+node);
        if(_selectedNode != node)
        {
            _endNode = node;
        }
    }
    private void OnNodeMouseDown(Node node)
    {
        _selectedNode = node;
    }
    void Update()
    {
        if (_selectedNode is not null)
        {


            var mousePosition = (Input.mousePosition);
            Vector3 worldSpacemousePos = _mainCam.ScreenToWorldPoint(new Vector3(
                mousePosition.x, mousePosition.y, 10));
            if (Input.GetMouseButtonDown(0))
            {

                _initalPosition = _mainCam.ScreenToWorldPoint(new Vector3(
                    mousePosition.x, mousePosition.y, 10));
                _lr.enabled = true;

            }

            if (Input.GetKey(KeyCode.Mouse0))
            {
                _lr.startWidth = .2f;
                _lr.endWidth = .2f;
                _lr.positionCount = 2;
                _lr.SetPosition(0, _initalPosition);
                _lr.SetPosition(1, worldSpacemousePos);
                //send ray, if a node is hit then do the following
            }

            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                if(_endNode is not null)
                {
                    Debug.Log("EndNode:" + _endNode);
                    _selectedNode.AddNodeConnection(_endNode);
                    if (_selectedNode != _endNode)
                    {
                        Debug.Log("Test123123");

                    }
                }
                _selectedNode = null;
                _endNode = null;
                _lr.enabled = false;
            }
        }
    }
}
