using BepInEx;
using UnityEngine;
using System;

namespace FieldEquipmentMod
{
    [BepInPlugin("com.shafin.fieldequip", "Ultimate Field Unlocker", "1.4.0")]
    public class Plugin : BaseUnityPlugin
    {
        private bool _active = true;

        void OnGUI()
        {
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1));
            
            if (GUI.Button(new Rect(50, 50, 380, 100), _active ? "<color=cyan>BYPASS: ACTIVE</color>" : "<color=red>BYPASS: OFF</color>"))
            {
                _active = !_active;
            }
        }

        void Update()
        {
            if (!_active) return;

            // Target the core game systems every frame to maintain the 'lie'
            GameObject pd = GameObject.Find("PlayerData");
            GameObject hero = GameObject.FindGameObjectWithTag("Player");

            if (pd != null)
            {
                pd.SendMessage("SetBool", new object[] { "atBench", true }, SendMessageOptions.DontRequireReceiver);
                pd.SendMessage("SetBool", new object[] { "canEquip", true }, SendMessageOptions.DontRequireReceiver);
            }

            if (hero != null)
            {
                // This mimics the PC mod's patch by forcing the hero's state
                hero.SendMessage("SetAtBench", true, SendMessageOptions.DontRequireReceiver);
            }

            // Target the specific UI logic components
            GameObject[] menus = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var m in menus)
            {
                if (m == null) continue;
                string name = m.name;

                // Silk Skills, Crests, and Tools all share these internal keywords
                if (name.Contains("Inventory") || name.Contains("Skill") || name.Contains("Crest") || name.Contains("Tool"))
                {
                    // Force the FSM variables directly
                    m.SendMessage("SetCanEquip", true, SendMessageOptions.DontRequireReceiver);
                    
                    // Force a UI redraw (The PC mod does this via Harmony)
                    m.SendMessage("UpdateStatus", SendMessageOptions.DontRequireReceiver);
                    m.SendMessage("SendEvent", "BENCH ON", SendMessageOptions.DontRequireReceiver);
                    
                    // Unblock the Button's visual lock
                    CanvasGroup cg = m.GetComponent<CanvasGroup>();
                    if (cg != null) { cg.interactable = true; cg.alpha = 1f; }
                }
            }
        }
    }
}
