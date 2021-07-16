using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Managers.EventManager;
using UI;
using UnityEngine;
using UnityEngine.Serialization;

public class UserInteraction : MonoBehaviour
{
    private Vector3 _initalPosition = new Vector3();

    [Header("Components")] [SerializeField]
    private LineRenderer lr = new();

    [Header("Node Visual Display")]
    [Tooltip("Displays SelectedNode on a left click event, i.e when we're creating a link between the nodes")]
    [SerializeField]
    private Node selectedNode;

    [Tooltip("If we release left click over a node, the end node is selected")] [SerializeField]
    private Node endNode;

    [Tooltip("If we right click, the ContextMenuNode is set to the one over the mouse position.")] [SerializeField]
    private Node contextMenuNode;

    public Node GetContextMenuNode => contextMenuNode;

    private Camera _mainCam;

    [Tooltip("If we're hovering over a ContextMenu this is set to true.")] [SerializeField]
    private bool hoveringOverContextMenu;

    [Header("Camera Controls")] public bool invertedCameraZoom;
    public bool invertedCamera;

    public float cameraIncrement;

    //drag controls
    public float cameraDragSpeed = 2;
    [Header("Context Menu Offsets")] public float contextMenuYOffset = 5;

    [Tooltip("This is the scale of the menu. It's rendered in world space.")]
    public float contextMenuZOffset = 10;

    private Vector3 _origin;

    [Header("ContextMenus")] [FormerlySerializedAs("NodeContextMenus")] [SerializeField]
    private GameObject[] nodeContextMenus;

    [FormerlySerializedAs("NodeUICanvas")] [SerializeField]
    private GameObject nodeUICanvas;

    [SerializeField] private GameObject[] emptyRightClickContextMenus;
    private bool _bConnectionTool = true, _bMoveTool, _bHoveringOverNode, _bSelectTool;
    [SerializeField] private float zoomTowardsMouseFactor;

    [SerializeField] private List<Node> selectedNodes;
//TODO add ctrl C, ctrl V copy and paste.
    //TODO: Add a selection tool in which you can click and drag to select multiple nodes and interact with them


    private void OnEnable()
    {
        _mainCam = Camera.main;
        EventManager.Listen("NodeMouseDown", (Action<Node>) OnNodeMouseDown);
        EventManager.Listen("NodeMouseUp", (Action<Node>) OnNodeMouseUp);
        EventManager.Listen("NodeMouseRightClick", (Action<Node>) OnNodeMouseRightClick);
        EventManager.Listen("OnContextMenuHover", (Action<bool>) ToggleContextMenu);
        EventManager.Listen("OnNodeSelectFromContextMenu", (Action<Node>) ChangeSelectedNode);
        EventManager.Listen("OnMouseExitNode", OnMouseExitNode);
    }

    //TODO make all methods that are interacted with by ui end with UI

    private void OnMouseExitNode()
    {
        _bHoveringOverNode = false;
    }

    private void ChangeSelectedNode(Node node)
    {
        contextMenuNode = node;
    }

    public void ToggleContextMenu(bool toggle)
    {
        hoveringOverContextMenu = toggle;
    }

    public void ConnectionTool()
    {
        DisableAllTools();
        _bConnectionTool = true;
    }

    public void MoveTool() // Single Node
    {
        DisableAllTools();
        _bMoveTool = true;
    }

    public void SelectTool() // Multiple Node Selection
    {
        DisableAllTools();
        _bSelectTool = true;
    }

    private void DisableAllTools()
    {
        foreach (var node in selectedNodes)
        {
            node.Highlight(false);
        }

        selectedNodes.Clear();
        hoveringOverContextMenu = false;
        _bSelectTool = false;
        _bConnectionTool = false;
        _bMoveTool = false;
    }

    public void MoveMenusToNode(Node node)
    {
        foreach (var menu in nodeContextMenus)
        {
            menu.transform.position =
                new Vector3(node.transform.position.x,
                    node.transform.position.y + contextMenuYOffset, contextMenuZOffset);
        }

        foreach (var m in emptyRightClickContextMenus)
        {
            m.transform.position =
                new Vector3(node.transform.position.x,
                    node.transform.position.y + contextMenuYOffset, contextMenuZOffset);
        }
    }

    private void DisableNodeContextMenus()
    {
        foreach (var menu in nodeContextMenus)
        {
            menu.SetActive(false);
        }
    }

    private void DisableEmptyRightClickContextMenu()
    {
        foreach (var menu in emptyRightClickContextMenus)
        {
            menu.SetActive(false);
        }
    }

    private void OnNodeMouseRightClick(Node node)
    {
        DisableEmptyRightClickContextMenu();
        nodeUICanvas.SetActive(true);
        var nodePos = node.transform.position;
        foreach (var contextMenu in nodeContextMenus)
        {
            Debug.Log(new Vector3(nodePos.x,
                nodePos.y - contextMenuYOffset, contextMenuZOffset));
            contextMenu.transform.position = new Vector3(nodePos.x,
                nodePos.y - contextMenuYOffset, contextMenuZOffset);
        }

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
        _bHoveringOverNode = true;
        if (selectedNode != node)
        {
            endNode = node;
        }
    }

    private bool _bMultiSelectNodeSelected;
    [SerializeField] private Node OnMultiSelectNode;

    private void OnNodeMouseDown(Node node)
    {
        if (!_bSelectTool)
        {
            selectedNode = node;
            return;
        }

        foreach (var n in selectedNodes)
        {
            if (n == node)
            {
                OnMultiSelectNode = n;
                _bMultiSelectNodeSelected = true;
            }
        }
    }

    void Update() //TODO: Figure out a better way to handle input.. i.e keybindings.
    {
        //TODO: Fix scrollrect issue when hovering over button UI -- if hovering over a button user is unable to scroll.
        if (selectedNode is not null)
        {
            NodeControl();
            return;
        }

        //TODO: Add keybinds for tool shortcuts -- e.g V=Move/Selection Tool, C=Connections,N=Create new node etc..
        if (!hoveringOverContextMenu)
        {
            SelectionTool();
            //TODO: Allow user to drag camera around while menu is open -- Annoying to edit nodes 
            //TODO: Implement Escape key to back out of all menus
            CameraControl();
            ClickAndDrag(2);
            if (!_bSelectTool)
            {
                ClickAndDrag(0);
            }

            if (!_bHoveringOverNode)
            {
                RightClickContextMenu();
            }
        }
    }

    [SerializeField] private bool bSelecting;
    [SerializeField] private Vector2 startPos;
    [SerializeField] private RectTransform selectionBox;
    [SerializeField] private Vector2 posMouse;

    public void SelectionTool()
    {
        if (_bMultiSelectNodeSelected)
        {
            if (Input.GetMouseButton(0))
            {
                Vector3 pos = _mainCam.ScreenToViewportPoint(Input.mousePosition -
                                                             _mainCam.WorldToScreenPoint(OnMultiSelectNode.transform
                                                                 .position));
                Vector3 translation = new Vector3(pos.x * cameraDragSpeed, pos.y * cameraDragSpeed, 0);
                foreach (var node in selectedNodes)
                {
                    node.RedrawLineRendererConnections();
                    node.transform.Translate(translation, Space.World);
                }

                _origin = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                _bMultiSelectNodeSelected = false;
                OnMultiSelectNode = null;
            }

            return;
        }

        if (_bSelectTool)
        {
            if (Input.GetMouseButtonDown(0) && !bSelecting)
            {
                bSelecting = true;
                UpdateSelectionBox(Input.mousePosition);
                startPos = (Input.mousePosition);
            }

            if (Input.GetMouseButton(0))
            {
                if (bSelecting)
                {
                    UpdateSelectionBox(Input.mousePosition);
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                foreach (var activeNode in selectedNodes)
                {
                    activeNode.Highlight(false);
                }

                selectedNodes.Clear();
                bSelecting = false;
                selectionBox.gameObject.SetActive(false);
                Vector2 min = selectionBox.anchoredPosition - (selectionBox.sizeDelta / 2);
                Vector2 max = selectionBox.anchoredPosition + (selectionBox.sizeDelta / 2);
                foreach (var keyValuePair in SceneManager.Instance.GetActiveNodes())
                {
                    Vector3 screenPos = _mainCam.WorldToScreenPoint(keyValuePair.Key.transform.position);
                    if (screenPos.x > min.x && screenPos.x < max.x && screenPos.y > min.y && screenPos.y < max.y)
                    {
                        selectedNodes.Add(keyValuePair.Value);
                        keyValuePair.Value.Highlight(true);
                    }
                }
            }
        }
    }

    void UpdateSelectionBox(Vector2 curMousePos)
    {
        posMouse = curMousePos;
        if (!selectionBox.gameObject.activeInHierarchy)
            selectionBox.gameObject.SetActive(true);
        float width = curMousePos.x - startPos.x;
        float height = curMousePos.y - startPos.y;
        selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
        selectionBox.anchoredPosition = startPos + new Vector2(width / 2, height / 2);
    }

    public void RemoveNodeConnectFromContextMenuNode(Node nodeConnectionToRemove, GameObject objToDestroy)
    {
        contextMenuNode.RemoveNodeConnection(nodeConnectionToRemove);
        nodeConnectionToRemove.Highlight(false);
        Destroy(objToDestroy);
    }

    private void RightClickContextMenu()
    {
        if (Input.GetMouseButtonDown(1))
        {
            DisableNodeContextMenus();
            var mousePosition = (Input.mousePosition);
            Vector3 worldSpacemousePos = _mainCam.ScreenToWorldPoint(new Vector3(
                mousePosition.x, mousePosition.y, 20));
            foreach (var menu in emptyRightClickContextMenus)
            {
                menu.SetActive(false); //deactivate all previous menus
                menu.transform.position =
                    new Vector3(worldSpacemousePos.x, worldSpacemousePos.y, contextMenuZOffset);
            }

            emptyRightClickContextMenus[0].SetActive(true); // this should always be the primary menu.
        }
    }

    private void ClickAndDrag(int mouseButton)
    {
        if (Input.GetMouseButton(mouseButton))
        {
            if (Input.GetMouseButtonDown(mouseButton))
            {
                _origin = Input.mousePosition;
                return;
            }

            if (mouseButton == 0)
            {
                nodeUICanvas.SetActive(false);
                foreach (var element in nodeContextMenus)
                {
                    foreach (var menu in emptyRightClickContextMenus)
                    {
                        menu.SetActive(false);
                    }

                    element.SetActive(false);
                }
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
            mousePosition.x, mousePosition.y, 20));

        if (_bConnectionTool)
        {
            if (Input.GetMouseButtonDown(0))
            {
                _initalPosition = _mainCam.ScreenToWorldPoint(new Vector3(
                    mousePosition.x, mousePosition.y, contextMenuZOffset));
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

            return;
        }

        if (_bMoveTool)
        {
            if (Input.GetMouseButton(0))
            {
                selectedNode.RedrawLineRendererConnections();
                var position = new Vector3(worldSpacemousePos.x, worldSpacemousePos.y, contextMenuZOffset);
                selectedNode.transform.position = position;
            }

            if (Input.GetMouseButtonUp(0))
            {
                selectedNode = null;
            }

            return;
        }
    }

    private void CameraControl()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos = _mainCam.ScreenToWorldPoint(mousePos);
            var position = this.transform.position;
            position = Vector3.MoveTowards(position,
                new Vector3(mousePos.x, mousePos.y, position.z), zoomTowardsMouseFactor);
            this.transform.position = position;
        }

        if (invertedCameraZoom)
        {
            _mainCam.orthographicSize += cameraIncrement * Input.GetAxis("Mouse ScrollWheel");
            return;
        }

        _mainCam.orthographicSize -= cameraIncrement * Input.GetAxis("Mouse ScrollWheel");
    }
}