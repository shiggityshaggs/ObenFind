using System;
using UnityEngine;

namespace ObenFind
{
    internal class InputHandler : MonoBehaviour
    {
        public static bool PressedOpen { get; private set; }
        public static bool PressedClose { get; private set; }

        private static KeyCode MenuKey => InputManager.instance?.keyBindings.menu ?? KeyCode.None;
        private static KeyCode PauseKey => InputManager.instance?.keyBindings.pausemenu ?? KeyCode.None;

        private static bool m_PressedOpen()
        {
            if (Plugin.GUIComponent.enabled) return false;
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C)) return true;
            return false;
        }

        private static bool m_PressedClose()
        {
            if (!Plugin.GUIComponent.enabled) return false;
            if (Event.current.keyCode == MenuKey || Event.current.keyCode == PauseKey) return true;
            if (Input.GetKeyDown(MenuKey) || Input.GetKeyDown(PauseKey)) return true;
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C)) return true;
            return false;
        }

        static bool busy;

        void Update()
        {
            PressedOpen = m_PressedOpen();
            PressedClose = m_PressedClose();

            if (!busy)
            {
                if (PressedClose)
                    Close();
                else if (PressedOpen)
                    Open();
            }
        }

        System.Collections.IEnumerator ControlsEnabled(bool enable)
        {
            yield return new WaitForEndOfFrame();

            if (enable)
                GameController.instance?.ControlsEnabled(this.gameObject);
            else
                GameController.instance?.ControlsDisabled(gameObject: this.gameObject, ShowCursor: true);

            busy = false;
        }

        void Open()
        {
            if (GameController.instance != null && GameController.instance.keysDisabled)
                return;

            busy = true;
            Plugin.GUIComponent.enabled = true;
            StartCoroutine(ControlsEnabled(false));
        }

        void Close()
        {
            busy = true;
            Plugin.GUIComponent.enabled = false;
            StartCoroutine(ControlsEnabled(true));
        }
    }
}
