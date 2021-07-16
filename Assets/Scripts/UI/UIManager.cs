using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Managers.EventManager;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
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
        [SerializeField] private TMP_InputField nameInputField;

        [FormerlySerializedAs("sizeInputField")] [SerializeField]
        private TMP_InputField sizeXInputField;

        [SerializeField] private TMP_InputField sizeYInputField;
        [SerializeField] private GameObject nodeContainer;
        [SerializeField] private UserInteraction userInt;
        [SerializeField] private string lastOpenedPathCfg = "LastOpenedPath.cfg";
        private Sprite selectedSprite = null;

        [Header("Advanced Configuration Panel")] [SerializeField]
        private Button ChangeName, ChangeColour, ViewConnections, ChangeSize;

        [FormerlySerializedAs("editInputX")] [SerializeField]
        private TMP_InputField editMenuInputX;

        [FormerlySerializedAs("editInputY")] [SerializeField]
        private TMP_InputField editMenuInputY;

        [SerializeField] private TMP_InputField editMenuInputName;


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
            selectedSprite = null;
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
            userInt.ToggleContextMenu(false);
            if (Camera.main is not null)
            {
                var camTransform = Camera.main.transform;
                var nodeTransform = node.transform;
                var position = nodeTransform.position;
                camTransform.position =
                    new Vector3(position.x, position.y, camTransform.position.z);
            }

            userInt.MoveMenusToNode(node);
        }

        public void CreateNode()
        {
            var obj = Instantiate(nodePrefab, parent: nodeContainer.transform);
            obj.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                Input.mousePosition.y, obj.transform.position.z));
            var node = obj.GetComponent<Node>();
            string name = nameInputField.text;
            obj.name = name;
            node.SetName(name);
            node.SetColour(Color.white); // TODO: implement a colour wheel or hex translation -- prefer a wheel.
            var collider = node.GetCollider;
            collider.size = node.transform.localScale;
            // I think unity handles != differently. So as far as im aware is not is faster
            if (selectedSprite is not null)
            {
                node.imageComp.sprite = selectedSprite;
                selectedSprite = null;
            }

            SetNodeSize(node, sizeXInputField, sizeYInputField);
        }

        private void SetNodeSize(Node node, TMP_InputField xInputField, TMP_InputField yInputField)
        {
            try
            {
                float x = 0;
                float y = 0;
                if (!string.IsNullOrEmpty(xInputField.text))
                {
                    x = float.Parse(xInputField.text);
                }

                if (!string.IsNullOrEmpty(yInputField.text))
                {
                    y = float.Parse(yInputField.text);
                }

                if (x == 0)
                {
                    x = y;
                    if (y == 0)
                    {
                        x = 1;
                        y = 1;
                    }
                }

                if (y == 0)
                {
                    y = x;
                    if (x == 0)
                    {
                        x = 1;
                        y = 1;
                    }
                }

                node.SetSize(x, y);
            }
            catch (Exception e)
            {
                float size = 1;
                Debug.LogWarning($"Exception returning {size}:{e.Message}");
                node.SetSize(size, size);
                return;
            }
        }

        public void ChangeContextMenuNodeImage()
        {
            OpenFileBrowser();
            if(selectedSprite is not null)
            {
                userInt.GetContextMenuNode.imageComp.sprite = selectedSprite;
            }
        }

        public void ChangeContextMenuNodeSize() //TODO Make this a draggable resizer instead of text based edit
        {
            SetNodeSize(userInt.GetContextMenuNode, editMenuInputX, editMenuInputY);
        }

        public void ChangeContextMenuNodeName()
        {
            userInt.GetContextMenuNode.SetName(editMenuInputName.text);
        }

        //Image selection
        public void OpenFileBrowser() //TODO rename this to something more descriptive and reattach script to button
        {
            var paths = StandaloneFileBrowser.OpenFilePanel("Select Image", GetLastOpenedPath(), "", false);
            if (paths.Length is not 0)
            {
                using FileStream fs = File.OpenWrite(Application.persistentDataPath + @"/" + lastOpenedPathCfg);

                Byte[] data = new UTF8Encoding(true).GetBytes(paths[0]);
                fs.Write(data, 0, data.Length);

                foreach (var path in paths)
                {
                    Debug.Log(path);
                }

                var image = new ImageDetails(paths[0]);
                selectedSprite = image.TextureSprite;
                //Draw onto the sprite
            }
        }

        private string GetLastOpenedPath()
        {
            try //TODO remove try statements in entire project -- They are more expensive than necessary
            {
                return Path.GetDirectoryName(
                    File.ReadAllText(Application.persistentDataPath + "/" + lastOpenedPathCfg));
            }
            catch (Exception e)
            {
                Debug.Log("Caught Exception:" + e.Message);
                return Application.persistentDataPath;
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

    public class ImageDetails
    {
        public string FileNameWithoutExtension;
        public string FileNameWithExtension;
        public string Directory;
        public string Extension;
        public string FullPath;
        public Sprite TextureSprite;

        public ImageDetails(string filePath)
        {
            Extension = Path.GetExtension(filePath);
            FileNameWithExtension = Path.GetFileName(filePath);
            FileNameWithExtension = Path.GetFileNameWithoutExtension(filePath);
            Directory = Path.GetDirectoryName(filePath);
            FullPath = filePath;
            TextureSprite = GetSpriteFromTexture2D(GetTexture2DFromPath(filePath));
        }

        private Texture2D GetTexture2DFromPath(string path)
        {
            var byteArray = File.ReadAllBytes(path);
            Texture2D txt2D = new(1, 1);
            txt2D.LoadImage(byteArray);
            return txt2D;
        }

        private Sprite GetSpriteFromTexture2D(Texture2D texture2D)
        {
            var sprite = Sprite.Create(texture2D,
                new Rect(0, 0, texture2D.width, texture2D.height),
                new Vector2(0, 0));
            Debug.Log(sprite.pivot);
            return sprite;
        }
    }
}