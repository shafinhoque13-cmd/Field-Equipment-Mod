using BepInEx;
using UnityEngine;
using System;

namespace FieldEquipmentMod
{
    [BepInPlugin("com.shafin.fieldequip", "Ultimate Field Unlocker", "1.6.0")]
    public class Plugin : BaseUnityPlugin
    {
        private bool _active = true;

        void OnGUI()
        {
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1));
            
            string label = _active ? "<color=cyan>BYPASS: ON</color>" : "<color=red>BYPASS: OFF</color>";
            if (GUI.Button(new Rect(50, 50, 380, 100), $"<size=30>{label}</size>"))
            {
                _active = !_active;
            }
        }

        void Update()
        {
            if (!_active) return;

            // 1. Force the 'Memory' of the save file
            GameObject pd = GameObject.Find("PlayerData");
            if (pd != null)
            {
                pd.SendMessage("SetBool", new object[] { "atBench", true }, SendMessageOptions.DontRequireReceiver);
                pd.SendMessage("SetBool", new object[] { "canEquip", true }, SendMessageOptions.DontRequireReceiver);
            }

            // 2. Force the Player's physical state
            GameObject hero = GameObject.FindGameObjectWithTag("Player");
            if (hero != null)
            {
                hero.SendMessage("SetAtBench", true, SendMessageOptions.DontRequireReceiver);
            }

            // 3. THE UI FIX: Brute force the buttons
            // This targets the "Dark/Grey" look from your screenshots
            GameObject[] allUI = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var ui in allUI)
            {
                if (ui == null) continue;
                string name = ui.name.ToLower();

                // Target Silk Skills, Crests, Tools, and the Equip Slots
                if (name.Contains("skill") || name.Contains("crest") || name.Contains("tool") || name.Contains("slot") || name.Contains("equip"))
                {
                    // Force Visuals (Makes them bright/visible)
                    CanvasGroup cg = ui.GetComponent<CanvasGroup>();
                    if (cg != null)
                    {
                        cg.interactable = true;
                        cg.blocksRaycasts = true;
                        cg.alpha = 1f;
                    }

                    // Force Logic (Tricks the PlayMaker FSM)
                    ui.SendMessage("SetCanEquip", true, SendMessageOptions.DontRequireReceiver);
                    ui.SendMessage("SendEvent", "BENCH ON", SendMessageOptions.DontRequireReceiver);
                    ui.SendMessage("SendEvent", "ACTIVATE", SendMessageOptions.DontRequireReceiver);
                    ui.SendMessage("SendEvent", "READY", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}
