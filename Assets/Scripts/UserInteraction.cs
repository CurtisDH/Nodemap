using System;
using System.Collections;
using System.Collections.Generic;
using Managers.EventManager;
using UnityEngine;

public class UserInteraction : MonoBehaviour
{
    private Vector3 _initalPosition = new Vector3();
    [SerializeField] private LineRenderer _lr = new();
    [SerializeField] private Node _selectedNode;
    [SerializeField] private Node _endNode;
    private Camera _mainCam;

    public bool invertedCameraZoom;
    public bool invertedCamera;
    public float cameraIncrement;

    //drag controls
    public float cameraDragSpeed = 2;
    private Vector3 origin;

    private void OnEnable()
    {
        _mainCam = Camera.main;
        EventManager.Listen("NodeMouseDown", (Action<Node>) OnNodeMouseDown);
        EventManager.Listen("NodeMouseUp", (Action<Node>) OnNodeMouseUp);
        EventManager.Listen("NodeMouseRightClick", (Action<Node>) OnNodeMouseRightClick);
    }

    private void OnNodeMouseRightClick(Node node)
    {
        if (_selectedNode != node)
        {
            _endNode = node;
        }
    }

    private void OnNodeMouseUp(Node node)
    {
        if (_selectedNode != node)
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
        CameraControl();
        if (_selectedNode is not null)
        {
            NodeControl();
            return;
        }

        ClickAndDrag();
    }

    private void ClickAndDrag()
    {
        if(Input.GetMouseButton(0))
        {
            if (Input.GetMouseButtonDown(0))
            {
                origin = Input.mousePosition;
                return;
            }

            Cursor.lockState = CursorLockMode.Locked;
            if (!invertedCamera)
            {
                Vector3 pos = _mainCam.ScreenToViewportPoint(Input.mousePosition - origin);
                Vector3 translation = new Vector3(pos.x * cameraDragSpeed, pos.y * cameraDragSpeed, 0);

                transform.Translate(translation, Space.World);
                origin = Input.mousePosition;
            }
            else
            {
                Vector3 pos = _mainCam.ScreenToViewportPoint(origin - Input.mousePosition);
                Vector3 translation = new Vector3(pos.x * cameraDragSpeed, pos.y * cameraDragSpeed, 0);

                transform.Translate(translation, Space.World);
                origin = Input.mousePosition; // so it feels less sticky
            }
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void NodeControl()
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
            if (_endNode is not null)
            {
                if (_selectedNode != _endNode)
                {
                    _selectedNode.AddNodeConnection(_endNode);
                }
            }

            _selectedNode = null;
            _endNode = null;
            _lr.enabled = false;
        }
    }

    private void CameraControl()
    {
        if (invertedCameraZoom)
        {
            _mainCam.fieldOfView += cameraIncrement * Input.GetAxis("Mouse ScrollWheel");
            return;
        }

        _mainCam.fieldOfView -= cameraIncrement * Input.GetAxis("Mouse ScrollWheel");
    }
}