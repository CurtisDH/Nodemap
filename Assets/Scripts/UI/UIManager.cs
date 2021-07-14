using System;
using System.Collections.Generic;
using Managers.EventManager;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;
        [SerializeField] private GameObject scrollContent;
        [SerializeField] private GameObject NodeButtonPrefab;
        private List<Node> _connections = new List<Node>();
        private List<GameObject> populatedButtons = new List<GameObject>();
        [SerializeField] private GameObject[] MenuCanvasUIs;
        [SerializeField] private GameObject nodePrefab;
        [SerializeField] private TMP_InputField nameInputField, sizeInputField;
        [SerializeField] private GameObject nodeContainer;
        [SerializeField] private UserInteraction userInt;

        public static UIManager Instance
        {
            get
            {
                if (_instance is null)
                {
                    var obj = new GameObject("UIManager");
                    obj.AddComponent<UIManager>();
                    _instance = obj.GetComponent<UIManager>();
                }

                return _instance;
            }
        }

        public void Awake()
        {
            _instance = this;
        }

        public void PopulateConnections(List<Node> nodeConnections)
        {
            _connections.Clear();
            foreach (var n in nodeConnections)
            {
                _connections.Add(n);
            }
        }

        public void AddNodeButtonsToConnectionContextMenu()
        {
            foreach (var obj in populatedButtons)
            {
                Destroy(obj);
            }

            foreach (var node in _connections)
            {
                //TODO Fix button placement in the vertical layout -- button placement is really odd
                var obj = Instantiate(NodeButtonPrefab, parent: scrollContent.transform);
                obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = node.name;
                var button = obj.transform.GetComponent<Button>();
                var buttonDelete = obj.transform.GetChild(1).GetComponent<Button>();


                var eventTrigger = button.GetComponent<EventTrigger>();
                EventTrigger.Entry eventtype = new();
                eventtype.eventID = EventTriggerType.PointerEnter;
                eventtype.callback.AddListener(delegate { node.Highlight(true); });
                eventTrigger.triggers.Add(eventtype);
                EventTrigger.Entry eventType = new();
                eventType.eventID = EventTriggerType.PointerExit;
                eventType.callback.AddListener(delegate { node.Highlight(false); });
                eventTrigger.triggers.Add(eventType);



                buttonDelete.onClick.AddListener(delegate { userInt.RemoveNodeConnectFromContextMenuNode(node, obj); });
                button.onClick.AddListener(delegate { ChangePanel(0, node); });
                populatedButtons.Add(obj);
            }
        }

        public void ChangePanel(int index, Node node)
        {
            node.Highlight(false);
            foreach (var panel in MenuCanvasUIs)
            {
                panel.SetActive(false);
            }

            MenuCanvasUIs[index].SetActive(true);
            EventManager.RaiseEvent("OnNodeSelectFromContextMenu", node);
        }

        public void CreateNode()
        {
            var obj = Instantiate(nodePrefab, parent: nodeContainer.transform);
            var node = obj.GetComponent<Node>();
            string name = nameInputField.text;
            obj.name = name;
            node.SetName(name);
            node.SetColour(Color.white); // TODO: implement a colour wheel or hex translation -- prefer a wheel.
            try
            {
                var val = float.Parse(sizeInputField.text);
                node.SetSize(val);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                node.SetSize(1);
                return;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CreateNode();
            }
        }

        public List<Node> GetConnections()
        {
            return _connections;
        }
        
    }

}
