using BepInEx;
using UnityEngine;
using System;

namespace FieldEquipmentMod
{
    [BepInPlugin("com.shafin.fieldequip", "Ultimate Field Unlocker", "1.2.0")]
    public class Plugin : BaseUnityPlugin
    {
        private bool _active = true;
        private float _timer = 0f;

        void OnGUI()
        {
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1));
            
            string status = _active ? "<color=cyan>BYPASS: ACTIVE</color>" : "<color=red>BYPASS: OFF</color>";
            if (GUI.Button(new Rect(50, 50, 380, 100), $"<size=30>{status}</size>"))
            {
                _active = !_active;
            }
        }

        void Update()
        {
            if (!_active) return;

            _timer += Time.deltaTime;
            if (_timer >= 0.4f) 
            {
                UnlockEverything();
                _timer = 0;
            }
        }

        private void UnlockEverything()
        {
            // 1. Force PlayerData (The Logic Layer)
            GameObject pd = GameObject.Find("PlayerData");
            if (pd != null)
            {
                pd.SendMessage("SetBool", new object[] { "atBench", true }, SendMessageOptions.DontRequireReceiver);
                pd.SendMessage("SetBool", new object[] { "canEquip", true }, SendMessageOptions.DontRequireReceiver);
            }

            // 2. Force HeroController (The Physics/Character Layer)
            GameObject hero = GameObject.FindGameObjectWithTag("Player");
            if (hero != null)
            {
                hero.SendMessage("SetAtBench", true, SendMessageOptions.DontRequireReceiver);
                hero.SendMessage("SetCanEquip", true, SendMessageOptions.DontRequireReceiver);
            }

            // 3. Force UI Buttons (Crests, Tools, and Silk Skills)
            GameObject[] uiObjects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var ui in uiObjects)
            {
                if (ui == null) continue;
                string n = ui.name.ToLower();

                if (n.Contains("skill") || n.Contains("crest") || n.Contains("tool") || n.Contains("slot"))
                {
                    // Wake up PlayMaker FSMs
                    ui.SendMessage("SetCanEquip", true, SendMessageOptions.DontRequireReceiver);
                    ui.SendMessage("SendEvent", "BENCH ON", SendMessageOptions.DontRequireReceiver);
                    
                    // Force Visual Interactivity
                    var group = ui.GetComponent<CanvasGroup>();
                    if (group != null)
                    {
                        group.interactable = true;
                        group.blocksRaycasts = true;
                        group.alpha = 1f;
                    }
                }
            }
        }
    }
}
