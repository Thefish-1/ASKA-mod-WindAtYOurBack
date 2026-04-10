using System;
using BepInEx;
using BepInEx.Configuration; // Required for Config
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using SSSGame.Water;
using UnityEngine.SceneManagement;

namespace WindAtYourBack
{
    [BepInPlugin("windatyourback", "WindAtYourBack", "1.7.0")]
    public class WindAtYourBackPlugin : BasePlugin
    {
        // Static reference to access the config from the component
        public static ConfigEntry<float> MaxBoatSpeedConfig;

        public override void Load()
        {
            // Bind the config setting: Category "General", Key "MaxBoatSpeed", Default 15.0f
            MaxBoatSpeedConfig = Config.Bind("General", "MaxBoatSpeed", 15.0f, "Sets the maximum speed for the boat. Game default is 15.0.");

            ClassInjector.RegisterTypeInIl2Cpp<WindUpdaterComponent>();

            var windObj = new GameObject("WindAtYourBack_Controller");
            UnityEngine.Object.DontDestroyOnLoad(windObj);
            windObj.AddComponent<WindUpdaterComponent>();
            
            Log.LogInfo("WindAtYourBack: Loaded with Config support!");
        }
    }

    public class WindUpdaterComponent : MonoBehaviour
    {
        private BoatMotor _motor;
        private float _nextSearch;

        public WindUpdaterComponent(IntPtr handle) : base(handle) { }

        private void FixedUpdate()
        {
            if (SceneManager.GetActiveScene().buildIndex == 0) return;

            if (_motor == null)
            {
                if (Time.time < _nextSearch) return;
                _nextSearch = Time.time + 2f;
                _motor = UnityEngine.Object.FindObjectOfType<BoatMotor>();
                
                if (_motor == null) return;
            }

            // Apply wind direction and base strength
            Vector3 forward = _motor.transform.forward;
            forward.y = 0;
            _motor._weatherWindDirection = forward.normalized;
            _motor._weatherWindValue = 1.0f;

            // Apply Max Speed from Config
            // This replaces the hardcoded speedBoost variable
            float speedLimit = WindAtYourBackPlugin.MaxBoatSpeedConfig.Value;
            _motor.maxTailSpeed = speedLimit;
            _motor.maxTailSpeedLateral = speedLimit;
        }
    }
}
