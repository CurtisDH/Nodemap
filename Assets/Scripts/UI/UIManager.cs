using System;
using System.Collections.Generic;
using Managers.EventManager;
using TMPro;
using UnityEngine;
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
                var obj = Instantiate(NodeButtonPrefab);
                obj.transform.parent = scrollContent.transform;
                obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = node.name;
                var button = obj.transform.GetComponent<Button>();
                button.onClick.AddListener(delegate {ChangePanel(0,node);});
                populatedButtons.Add(obj);
            }
        }

        public void ChangePanel(int index,Node node)
        {
            foreach (var panel in MenuCanvasUIs)
            {
                panel.SetActive(false);
            }
            MenuCanvasUIs[index].SetActive(true);
            EventManager.RaiseEvent("OnNodeSelectFromContextMenu",node);
        }
        

        public List<Node> GetConnections()
        {
            return _connections;
        }
    }
}