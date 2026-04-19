using BepInEx;
using UnityEngine;
using System;

namespace FieldEquipmentMod
{
    [BepInPlugin("com.shafin.fieldequip", "Field Equipment Unlocker", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private bool _active = true;
        private float _timer = 0f;

        void OnGUI()
        {
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1));
            
            string status = _active ? "<color=green>BENCH BYPASS: ACTIVE</color>" : "<color=red>BENCH BYPASS: OFF</color>";
            if (GUI.Button(new Rect(50, 50, 350, 90), $"<size=28>{status}</size>"))
            {
                _active = !_active;
            }
        }

        void Update()
        {
            if (!_active) return;

            _timer += Time.deltaTime;
            if (_timer >= 1.0f) // Check every second
            {
                UnlockEquipment();
                _timer = 0;
            }
        }

        private void UnlockEquipment()
        {
            // Forces the Player State to 'Resting' so menus work anywhere
            GameObject hero = GameObject.FindGameObjectWithTag("Player");
            if (hero != null)
            {
                hero.SendMessage("SetAtBench", true, SendMessageOptions.DontRequireReceiver);
            }

            // Forces Global Variables to allow equipping
            GameObject pd = GameObject.Find("PlayerData");
            if (pd != null)
            {
                pd.SendMessage("SetBool", new object[] { "atBench", true }, SendMessageOptions.DontRequireReceiver);
                pd.SendMessage("SetBool", new object[] { "canEquip", true }, SendMessageOptions.DontRequireReceiver);
            }

            // Target the Inventory UI directly
            GameObject[] uiElements = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var ui in uiElements)
            {
                if (ui != null && (ui.name.Contains("Inventory") || ui.name.Contains("Crest")))
                {
                    ui.SendMessage("SetCanEquip", true, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}
