using BepInEx;
using UnityEngine;
using System;

namespace FieldEquipmentMod
{
    [BepInPlugin("com.shafin.fieldequip", "Ultimate Field Unlocker", "1.7.0")]
    public class Plugin : BaseUnityPlugin
    {
        private bool _active = true;

        void OnGUI()
        {
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1));
            
            string status = _active ? "<color=cyan>BYPASS: ACTIVE</color>" : "<color=red>BYPASS: OFF</color>";
            if (GUI.Button(new Rect(50, 50, 400, 110), $"<size=28>{status}</size>"))
            {
                _active = !_active;
                if (_active) ForceUIRefresh(); // Jolt the UI when turned on
            }
        }

        void Update()
        {
            if (!_active) return;

            // Constantly feed 'True' to the underlying logic
            GameObject pd = GameObject.Find("PlayerData");
            if (pd != null) {
                pd.SendMessage("SetBool", new object[] { "atBench", true }, SendMessageOptions.DontRequireReceiver);
                pd.SendMessage("SetBool", new object[] { "canEquip", true }, SendMessageOptions.DontRequireReceiver);
            }
        }

        private void ForceUIRefresh()
        {
            GameObject[] all = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var obj in all)
            {
                if (obj == null) continue;
                string n = obj.name.ToLower();

                // Target the specific modules for Silk Skills, Tools, and Crests
                if (n.Contains("inventory") || n.Contains("skill") || n.Contains("crest") || n.Contains("tool") || n.Contains("slot"))
                {
                    // 1. Force visual transparency (Remove the grey)
                    CanvasGroup cg = obj.GetComponent<CanvasGroup>();
                    if (cg != null) { cg.interactable = true; cg.alpha = 1f; }

                    // 2. The 'Jolt' - We send the same events the PC mod uses
                    obj.SendMessage("SetCanEquip", true, SendMessageOptions.DontRequireReceiver);
                    obj.SendMessage("UpdateStatus", SendMessageOptions.DontRequireReceiver);
                    obj.SendMessage("SendEvent", "BENCH ON", SendMessageOptions.DontRequireReceiver);
                    obj.SendMessage("SendEvent", "RECHECK", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}
