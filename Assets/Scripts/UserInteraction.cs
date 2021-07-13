using System;
using Managers.EventManager;
using UI;
using UnityEngine;

public class UserInteraction : MonoBehaviour
{
    private Vector3 _initalPosition = new Vector3();
    [SerializeField] private LineRenderer lr = new();
    [SerializeField] private Node selectedNode;
    [SerializeField] private Node endNode;
    [SerializeField] private Node contextMenuNode;
    private Camera _mainCam;
    [SerializeField] private bool hoveringOverContextMenu;

    public bool invertedCameraZoom;
    public bool invertedCamera;
    public float cameraIncrement;

    //drag controls
    public float cameraDragSpeed = 2;
    private Vector3 _origin;

    [SerializeField] private GameObject[] MenuCanvasUIs;
    [SerializeField] private GameObject NodeUICanvas;

    private void OnEnable()
    {
        _mainCam = Camera.main;
        EventManager.Listen("NodeMouseDown", (Action<Node>) OnNodeMouseDown);
        EventManager.Listen("NodeMouseUp", (Action<Node>) OnNodeMouseUp);
        EventManager.Listen("NodeMouseRightClick", (Action<Node>) OnNodeMouseRightClick);
        EventManager.Listen("OnContextMenuHover", (Action<bool>) ToggleContextMenu);
        EventManager.Listen("OnNodeSelectFromContextMenu", (Action<Node>) ChangeSelectedNode);
    }

    private void ChangeSelectedNode(Node node)
    {
        contextMenuNode = node;
    }

    public void ToggleContextMenu(bool toggle)
    {
        hoveringOverContextMenu = toggle;
    }

    private void OnNodeMouseRightClick(Node node)
    {
        NodeUICanvas.SetActive(true);
        contextMenuNode = node;
    }

    public void NodePanelDeleteNode()
    {
        //TODO Send a second prompt to confirm
        Destroy(contextMenuNode.gameObject); //TODO put into a deactivated menu where CTRL-Z can bring back the Node.
    }

    public void ViewSelectedNodeConnections()
    {
        UIManager.Instance.PopulateConnections(contextMenuNode.Connections);
        UIManager.Instance.AddNodeButtonsToConnectionContextMenu();
    }

    private void OnNodeMouseUp(Node node)
    {
        if (selectedNode != node)
        {
            endNode = node;
        }
    }

    private void OnNodeMouseDown(Node node)
    {
        selectedNode = node;
    }

    void Update()
    {
        CameraControl();
        if (selectedNode is not null)
        {
            NodeControl();
            return;
        }

        if (!hoveringOverContextMenu)
        {
            ClickAndDrag();
        }
    }

    private void ClickAndDrag()
    {
        if (Input.GetMouseButton(0))
        {
            if (Input.GetMouseButtonDown(0))
            {
                _origin = Input.mousePosition;
                return;
            }

            NodeUICanvas.SetActive(false);
            foreach (var element in MenuCanvasUIs)
            {
                element.SetActive(false);
            }

            Cursor.lockState = CursorLockMode.Locked;
            if (!invertedCamera)
            {
                Vector3 pos = _mainCam.ScreenToViewportPoint(Input.mousePosition - _origin);
                Vector3 translation = new Vector3(pos.x * cameraDragSpeed, pos.y * cameraDragSpeed, 0);

                transform.Translate(translation, Space.World);
                _origin = Input.mousePosition;
            }
            else
            {
                Vector3 pos = _mainCam.ScreenToViewportPoint(_origin - Input.mousePosition);
                Vector3 translation = new Vector3(pos.x * cameraDragSpeed, pos.y * cameraDragSpeed, 0);

                transform.Translate(translation, Space.World);
                _origin = Input.mousePosition; // so it feels less sticky
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
            lr.enabled = true;
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            lr.startWidth = .2f;
            lr.endWidth = .2f;
            lr.positionCount = 2;
            lr.SetPosition(0, _initalPosition);
            lr.SetPosition(1, worldSpacemousePos);
            //send ray, if a node is hit then do the following
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (endNode is not null)
            {
                if (selectedNode != endNode)
                {
                    selectedNode.AddNodeConnection(endNode);
                }
            }

            selectedNode = null;
            endNode = null;
            lr.enabled = false;
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