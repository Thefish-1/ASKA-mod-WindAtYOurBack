using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using SSSGame.Water;
using UnityEngine.SceneManagement;

namespace WindAtYourBack
{
    [BepInPlugin("windatyourback", "WindAtYourBack", "1.5.1")]
    public class WindAtYourBackPlugin : BasePlugin
    {
        public override void Load()
        {
            // Register the component for IL2CPP compatibility
            ClassInjector.RegisterTypeInIl2Cpp<WindUpdaterComponent>();

            // Create a persistent object to run the mod logic
            var windObj = new GameObject("WindAtYourBack_Controller");
            UnityEngine.Object.DontDestroyOnLoad(windObj);
            windObj.AddComponent<WindUpdaterComponent>();
        }
    }

    public class WindUpdaterComponent : MonoBehaviour
    {
        private BoatMotor _motor;
        private float _nextSearch;

        public WindUpdaterComponent(IntPtr handle) : base(handle) { }

        private void FixedUpdate()
        {
            // Optimization: Do nothing if we are in the Main Menu (index 0)
            if (SceneManager.GetActiveScene().buildIndex == 0) return;

            // If motor reference is missing, search for it every 2 seconds
            if (_motor == null)
            {
                if (Time.time < _nextSearch) return;
                _nextSearch = Time.time + 2f;
                _motor = UnityEngine.Object.FindObjectOfType<BoatMotor>();
                
                if (_motor == null) return;
            }

            // Calculate direction: Use the boat's forward heading on a flat plane
            Vector3 forward = _motor.transform.forward;
            forward.y = 0;

            // Apply wind: Force direction to forward and strength to 100% (1.0)
            _motor._weatherWindDirection = forward.normalized;
            _motor._weatherWindValue = 1.0f;
        }
    }
}