using BepInEx;
using UnityEngine;
using System;
using System.Reflection;

namespace FieldEquipmentMod
{
    [BepInPlugin("com.shafin.fieldequip", "Field Equipment Unlocker", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private bool _modEnabled = true;
        private float _checkTimer = 0f;

        // UI Scaling for Mobile
        private Rect _btnRect = new Rect(50, 50, 300, 90);

        void OnGUI()
        {
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1));

            string btnText = _modEnabled ? "<color=green>BENCH BYPASS: ON</color>" : "<color=red>BENCH BYPASS: OFF</color>";
            if (GUI.Button(_btnRect, $"<size=30>{btnText}</size>"))
            {
                _modEnabled = !_modEnabled;
            }
        }

        void Update()
        {
            if (!_modEnabled) return;

            _checkTimer += Time.deltaTime;
            if (_checkTimer >= 0.5f) // Run every half second to keep the "lie" alive
            {
                ForceEquipmentStates();
                _checkTimer = 0;
            }
        }

        private void ForceEquipmentStates()
        {
            // 1. Trick the PlayerData
            // This is where the game checks "atBench" before showing the 'Equip' button
            GameObject pdObj = GameObject.Find("PlayerData");
            if (pdObj != null)
            {
                // We send a direct message to set the boolean variable
                pdObj.SendMessage("SetBool", new object[] { "atBench", true }, SendMessageOptions.DontRequireReceiver);
                pdObj.SendMessage("SetBool", new object[] { "canEquip", true }, SendMessageOptions.DontRequireReceiver);
            }

            // 2. Trick the HeroController
            // This prevents the game from kicking you out of the menu when you move
            GameObject hero = GameObject.FindGameObjectWithTag("Player");
            if (hero != null)
            {
                hero.SendMessage("SetAtBench", true, SendMessageOptions.DontRequireReceiver);
            }

            // 3. Force the Inventory UI to wake up
            // This targets the specific "Crest" and "Tool" menu buttons
            GameObject[] menus = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var menu in menus)
            {
                if (menu == null) continue;
                string n = menu.name.ToLower();
                
                if (n.Contains("inventory") || n.Contains("equipment") || n.Contains("crest"))
                {
                    // Forces the FSM (PlayMaker) inside the menu to unlock the slots
                    menu.SendMessage("SetCanEquip", true, SendMessageOptions.DontRequireReceiver);
                    menu.SendMessage("UpdateStatus", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}

