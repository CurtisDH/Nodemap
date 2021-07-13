using System;
using Managers.EventManager;
using UnityEngine;

namespace UI
{
    public class NodeContextMenu : MonoBehaviour
    {
        private void OnMouseOver()
        {
            EventManager.RaiseEvent("OnContextMenuHover",true);
        }

        private void OnMouseExit()
        {
            if (!Input.GetMouseButton(0))
            {
                EventManager.RaiseEvent("OnContextMenuHover",false);
            }
        }
    }
}
