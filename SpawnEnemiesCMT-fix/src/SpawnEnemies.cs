using CementTools;
using CoreNet.Model;
using CoreNet.Objects;
using Costumes;
using Femur;
using GB.Core;
using GB.Game.Data;
using GB.Networking.Objects;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace SpawnEnemiesFixed
{
    public class SpawnEnemies : CementMod
    {
        private int enemyCount = 0;

        // Token: 0x04000005 RID: 5
        private WavesData waveInformation;

        // Token: 0x04000006 RID: 6
        private KeybindManager keybindManager;

        // Token: 0x04000007 RID: 7
        private bool inGame = false;

        // Token: 0x04000008 RID: 8
        private GameObject localPlayer;

        private void Start()
        {
            this.keybindManager = new KeybindManager(this.modFile);
            this.keybindManager.BindLarge(new Action(this.SpawnLarge), false);
            this.keybindManager.BindMedium(new Action(this.SpawnMedium), false);
            this.keybindManager.BindSmall(new Action(this.SpawnSmall), false);
            string text = "Waves/Grind";
            this.waveInformation = UnityEngine.Resources.Load(text) as WavesData;
            SceneManager.sceneLoaded += this.SceneLoaded;
            Cement.Log($"Cement Mod {modFile.GetString("Name")} has loaded!");
        }

        private void Update()
        {
            bool flag = this.localPlayer == null;
            if (flag)
            {
                foreach (Actor actor in UnityEngine.Object.FindObjectsOfType<Actor>())
                {
                    bool isLocalPlayer = actor.isLocalPlayer;
                    if (isLocalPlayer)
                    {
                        this.localPlayer = actor.gameObject;
                        actor.gangID = 1;
                    }
                }
            }
            bool flag2 = this.inGame;
            if (flag2)
            {
                this.keybindManager.CheckInputs();
            }
        }

        public void SceneLoaded(Scene scene, LoadSceneMode _)
        {
            bool flag = scene.name != Global.MENU_SCENE_NAME;
            this.inGame = flag;
        }

        private Vector3 GetMousePositionInWorld()
        {
            Vector3 vector = Mouse.current.position.ReadValue();
            vector.z = 2f;
            Ray ray = Camera.main.ScreenPointToRay(vector);
            RaycastHit raycastHit;
            bool flag = Physics.Raycast(ray, out raycastHit);
            Vector3 vector2;
            if (flag)
            {
                vector2 = raycastHit.point;
            }
            else
            {
                vector2 = new Vector3(0f, -1000f, 0f);
            }
            return vector2;
        }

        private void SpawnSmall()
        {
            Vector3 mousePositionInWorld = this.GetMousePositionInWorld();
            Vector3 vector = mousePositionInWorld + Vector3.up;
            this.SpawnEnemy(2, vector);
        }

        private void SpawnMedium()
        {
            Vector3 mousePositionInWorld = this.GetMousePositionInWorld();
            Vector3 vector = mousePositionInWorld + Vector3.up;
            this.SpawnEnemy(0, vector);
        }

        private void SpawnLarge()
        {
            Vector3 mousePositionInWorld = this.GetMousePositionInWorld();
            Vector3 vector = mousePositionInWorld + Vector3.up;
            this.SpawnEnemy(1, vector);
        }

        public void SpawnEnemy(int type, Vector3 position)
        {
            Cement.Log("SPAWNING ENEMY");
            bool flag = this.waveInformation != null;
            if (!flag)
            {
                Wave wave = this.waveInformation.levelWaves[0];
                CostumeSaveEntry costumeByPresetName = MonoSingleton<Global>.Instance.Costumes.CostumePresetDatabase.GetCostumeByPresetName(this.waveInformation.GetRandomCostume());
                NetCostume netCostume = new(costumeByPresetName);
                Color color = new(Random.value, Random.value, Random.value);
                int gangID = wave.beasts[0].gangID;
                NetBeast netBeast = new(200 + this.enemyCount, netCostume, color, color, gangID, NetPlayer.PlayerType.AI, false);
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.waveInformation.GetSpawnObject(type), position, Quaternion.identity);
                bool flag2 = gameObject != null;
                if (flag2)
                {
                    netBeast.Instance = gameObject;
                    Actor component = gameObject.GetComponent<Actor>();
                    bool flag3 = component != null;
                    if (flag3)
                    {
                        component.ControlledBy = Actor.ControlledTypes.AI;
                        component.playerID = -1;
                        component.IsAI = true;
                        component.controllerID = netBeast.ControllerId;
                        component.gangID = gangID;
                        MonoSingleton<Global>.Instance.GetComponentInChildren<NetModel>().Add<NetBeast>("NET_PLAYERS", netBeast);
                        component.DressBeast();
                    }
                    MonoSingleton<Global>.Instance.GetComponentInChildren<NetModel>().Remove<NetBeast>("NET_PLAYERS", netBeast);
                    this.enemyCount++;
                }
            }
        }
    }

    public class KeybindManager
    {
        public KeybindManager(ModFile file)
        {
            this._file = file;
        }

        private bool WasPressedLastFrame(string p)
        {
            bool flag = !this._pressedLastFrame.ContainsKey(p);
            bool flag2;
            if (flag)
            {
                this._pressedLastFrame[p] = false;
                flag2 = false;
            }
            else
            {
                flag2 = this._pressedLastFrame[p];
            }
            return flag2;
        }

        public void CheckInputs()
        {
            foreach (InputDevice inputDevice in InputSystem.devices)
            {
                foreach (string text in this._keybinds.Keys)
                {
                    InputControl inputControl = inputDevice.TryGetChildControl(text);
                    bool flag = inputControl == null;
                    if (!flag)
                    {
                        bool flag2 = inputControl.IsPressed(0f);
                        if (flag2)
                        {
                            bool flag3 = !this._keybinds[text].held && this.WasPressedLastFrame(text);
                            if (!flag3)
                            {
                                this._keybinds[text].action();
                                this._pressedLastFrame[text] = true;
                            }
                        }
                        else
                        {
                            this._pressedLastFrame[text] = false;
                        }
                    }
                }
            }
        }

        public void BindSmall(Action a, bool held = true)
        {
            this._keybinds[this._file.GetString("KeybindSpawnSmall")] = new KeybindManager.KeybindData(a, held);
        }

        public void BindMedium(Action a, bool held = true)
        {
            this._keybinds[this._file.GetString("KeybindSpawnMedium")] = new KeybindManager.KeybindData(a, held);
        }

        public void BindLarge(Action a, bool held = true)
        {
            this._keybinds[this._file.GetString("KeybindSpawnLarge")] = new KeybindManager.KeybindData(a, held);
        }

        private ModFile _file;

        private Dictionary<string, KeybindManager.KeybindData> _keybinds = new();

        private Dictionary<string, bool> _pressedLastFrame = new();

        private class KeybindData
        {
            public KeybindData(Action a, bool h)
            {
                this.action = a;
                this.held = h;
            }

            public readonly Action action;

            public readonly bool held;
        }
    }
}
