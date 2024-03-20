using Il2CppGB.Networking.Utils;
using Il2CppInterop.Runtime;
using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CinematicML
{
    public static class InputActionExtensions
    {
        public static bool WasPressedThisFrame(this InputAction inputAction)
        {
            return inputAction.triggered && inputAction.ReadValue<float>() != 0f;
        }

        public static bool WasReleasedThisFrame(this InputAction inputAction)
        {
            return inputAction.triggered && inputAction.ReadValue<float>() == 0f;
        }
    }

    public static class BuildInfo
    {
        public const string Name = "Cinematic";
        public const string Author = "HueSamai/dotpy, TheUltimateNuke";
        public const string Description = null;
        public const string Company = null;
        public const string Version = "2.0.0";
        public const string DownloadLink = null;
    }

    public class Mod : MelonMod
    {
        public static GameObject? CamGameObject { get; private set; }
        
        public static bool CamEnabled
        {
            get
            {
                if (CamGameObject == null) CamEnabled = false;
                return _camEnabled;
            }
            set
            {
                if (!value) 
                { 
                    if (CamGameObject != null) UnityEngine.Object.Destroy(CamGameObject);
                    GBNetUtils.UnlockActorInputs();
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    CamGameObject = InstantiateCineCam();
                    if (CamGameObject == null || DefaultCamera == null) return;
                    GBNetUtils.LockActorInputs();
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;

                    CamGameObject.transform.position = DefaultCamera.transform.position;
                    CamGameObject.transform.rotation = DefaultCamera.transform.rotation;
                }
                _camEnabled = value;
            }
        }
        private static bool _camEnabled;

        private static Keyboard? CurrentKeyboard => Keyboard.current;
        private static Mouse? CurrentMouse => Mouse.current;
        private static Camera? DefaultCamera => Camera.main;

        private static Vector3 InputVector => CurrentKeyboard != null ? new(CurrentKeyboard[Key.D].ReadValue() - CurrentKeyboard[Key.A].ReadValue(), CurrentKeyboard[Key.Space].ReadValue() - CurrentKeyboard[Key.LeftCtrl].ReadValue(), CurrentKeyboard[Key.W].ReadValue() - CurrentKeyboard[Key.S].ReadValue()) : Vector3.zero;
        private static Vector2 MouseVector => CurrentMouse != null && CurrentMouse.rightButton.isPressed ? CurrentMouse.delta.ReadValue() : Vector2.zero;
        private static float RollAxis => CurrentKeyboard != null ? CurrentKeyboard[Key.Q].ReadValue() - CurrentKeyboard[Key.E].ReadValue() : 0f;

        private static GameObject? InstantiateCineCam()
        {
            var newCam = new GameObject("CinematicCam", Il2CppType.Of<Camera>());
            if (newCam == null) return null;

            newCam.tag = "CinematicCam";

            return newCam;
        }

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            
            LoggerInstance.Msg("Mod " + BuildInfo.Name + " has initialized.");
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (CurrentKeyboard == null || CurrentMouse == null) return;

            if (CurrentKeyboard[Key.Y].wasPressedThisFrame)
                CamEnabled = !CamEnabled;

            if (CamGameObject == null) return;

            CamGameObject.transform.position += CamGameObject.transform.forward * InputVector.z * Time.deltaTime;
            CamGameObject.transform.position += CamGameObject.transform.up * InputVector.y * Time.deltaTime;
            CamGameObject.transform.position += CamGameObject.transform.right * InputVector.x * Time.deltaTime;
            CamGameObject.transform.Rotate(new Vector3(-MouseVector.y * 0.5f, MouseVector.x * 0.5f, RollAxis));
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasUnloaded(buildIndex, sceneName);
        }
    }
}