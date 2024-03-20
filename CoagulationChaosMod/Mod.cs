using CoagulationChaosMod.Utilities.Effects;
using Il2CppFemur;
using Il2CppGB.Game;
using Il2CppGB.Game.Data;
using Il2CppGB.Game.Gameplay;
using Il2CppGB.Networking.Delegates;
using Il2CppGB.Networking.Objects;
using Il2CppGB.Networking.Utils;
using Il2CppInterop.Runtime;
using MelonLoader;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace CoagulationChaosMod
{
    public static class BuildInfo 
    {
        public const string Name = "Coagulation";
        public const string Description = "A meaty chaos mod for Gang Beasts.";
        public const string Author = "TheUltimateNuke";
        public const string Company = null;
        public const string Version = "1.0.0";
        public const string DownloadLink = null;
    }

    public class Mod : MelonMod
    {
        private const int noModifChance = 60;

        public static event Action? OnRoundStart;
        public static event Action? OnRoundEnd;
        public static event Action? OnUpdateEvent;
        public static event Action<Scene, LoadSceneMode>? OnSceneChanged;

        public static int RandomSeed => _seedPref.Value;
        public static float TimeBetweenEffects => _delayBetweenEffects.Value;

        public static BaseCoagEffectModifier[] AllPossibleModifiers => _allPossibleModifiers.ToArray();
        public static CoagEffect[] AllPossibleEffects => _allPossibleEffects.ToArray();

        private static List<CoagEffect> _currentlyHandledEffects = new();

        private static readonly MelonPreferences_Category _mainPrefCategory = MelonPreferences.CreateCategory("Coagulation - Settings");
        private static readonly MelonPreferences_Entry<int> _seedPref = _mainPrefCategory.CreateEntry(nameof(_seedPref), Guid.NewGuid().GetHashCode(), "Random Seed", "The seed for the random generator. Typically used to share with others the order in which effects are picked.");
        private static readonly MelonPreferences_Entry<float> _delayBetweenEffects = _mainPrefCategory.CreateEntry(nameof(_delayBetweenEffects), 5f, "Delay Between Effects", "The time it takes for the mod to choose a new effect to spawn.");

        private static float _curTimeUntilNextEffect = TimeBetweenEffects;

        private static readonly System.Random random_pickeffects = new(RandomSeed);
        private static readonly System.Random random_gravity = new(RandomSeed);
        private static readonly System.Random random_modifiers = new(RandomSeed);
        private static readonly System.Random random_modifChance = new(RandomSeed);

        private static CoagEffect? PickRandomEffect()
        {
            if (AllPossibleEffects.Length == 0) return null;
            return AllPossibleEffects[random_pickeffects.Next(AllPossibleEffects.Length)];
        }

        private static readonly List<BaseCoagEffectModifier> _allPossibleModifiers = new()
        {

        };

        private static Vector3? _noGravity_originalGravity;
        //private static Dictionary<Rigidbody, float> _rb_masses = new();
        private static Actor? _chosenPlayerToBecomeAi = null;
        private static Actor? _chosenPlayerToBecomeJuggernaut = null;
        private static WavesData? _fallbackWavesData = null;

        public static bool playerIsBecomingJuggernaut = false;

        private static IEnumerator SpawnWave()
        {
            throw new NotImplementedException();
        }

        private static readonly List<CoagEffect> _allPossibleEffects = new()
        {
            new CoagEffect("big bodies", false)
            {
                CustomEffectEnableAction = new(() =>
                {
                    GameplayModifiers.bodyScaleMul += 0.1f;
                    GameplayModifiers.ApplyUpdates();
                    return true;
                }),
                CustomEffectDisableAction = new(() =>
                {
                    GameplayModifiers.bodyScaleMul -= 0.1f;
                    GameplayModifiers.ApplyUpdates();
                })
            },
            new CoagEffect("speedy mcSpeederson", false)
            {
                CustomEffectEnableAction = new(() =>
                {
                    GameplayModifiers.moveSpeedMul += 8;
                    GameplayModifiers.ApplyUpdates();
                    return true;
                }),
                CustomEffectDisableAction = new(() =>
                {
                    GameplayModifiers.moveSpeedMul -= 8;
                    GameplayModifiers.ApplyUpdates();
                })
            },
            new CoagEffect("no gravity", false)
            {
                CustomEffectEnableAction = new(() =>
                {
                    if (_noGravity_originalGravity == null) return false;
                    if (_noGravity_originalGravity != null) Physics.gravity = (Vector3)_noGravity_originalGravity * (float)random_gravity.NextDouble();
                    return true;
                }),
                CustomEffectDisableAction = new(() => {
                    if (_noGravity_originalGravity != null) Physics.gravity = (Vector3)_noGravity_originalGravity;
                }),
            },
            /*
            new CoagEffect("decreased mass", false)
            {
                CustomEffectEnableAction = new(() =>
                {
                    foreach (Rigidbody rb in UnityEngine.Object.FindObjectsOfType<Rigidbody>())
                    {
                        _rb_masses.Add(rb, rb.mass);
                        rb.mass = 2;
                    }
                }),
                CustomEffectDisableAction = new(() =>
                {
                    foreach (Rigidbody rb in _rb_masses.Keys)
                    {
                        rb.mass = _rb_masses[rb];
                        _rb_masses.Remove(rb);
                    }
                })
            },
            */ // crashes game
            new CoagEffect("spawn wave", false)
            {
                CustomEffectEnableAction = new(() =>
                {
                    try
                    {
                        MelonCoroutines.Start(SpawnWave());
                    }
                    catch (NotImplementedException e)
                    {
                        Melon<Mod>.Logger.Error("Error occured spawning wave: " + e);
                        return false;
                    }
                    return true;
                })
            },
            new CoagEffect("random player becomes ai", false)
            {
                CustomEffectEnableAction = new(() =>
                {
                    if (_chosenPlayerToBecomeAi != null) return false;
                    var beasts = UnityEngine.Object.FindObjectsOfType<Actor>();
                    if (beasts.Length == 0) return false;
                    var playerMaybe = beasts[new System.Random().Next(0, beasts.Length)];
                    if (playerMaybe.IsAI) return false;
                    _chosenPlayerToBecomeAi = playerMaybe;
                    _chosenPlayerToBecomeAi.IsAI = true;
                    _chosenPlayerToBecomeAi.inputHandler.LockInput();
                    return true;
                }),
                CustomEffectDisableAction = new(() =>
                {
                    if (_chosenPlayerToBecomeAi != null)
                    {
                        _chosenPlayerToBecomeAi.IsAI = false;
                        _chosenPlayerToBecomeAi.inputHandler.UnlockInput();
                    }
                    _chosenPlayerToBecomeAi = null;
                })
            },
            new CoagEffect("random player becomes juggernaut", false)
            {
                CustomEffectEnableAction = new(() =>
                {
                    if (_chosenPlayerToBecomeJuggernaut != null || _fallbackWavesData == null) return false;

                    var beasts = UnityEngine.Object.FindObjectsOfType<Actor>();

                    if (beasts.Length == 0) return false;

                    var playerMaybe = beasts[new System.Random().Next(beasts.Length)];
                    _chosenPlayerToBecomeJuggernaut = playerMaybe;

                    int playerId = _chosenPlayerToBecomeJuggernaut.playerID;
                    int gangId = _chosenPlayerToBecomeJuggernaut.gangID;
                    NetBeast playerBeast = GBNetUtils.GetBeastForActor(_chosenPlayerToBecomeJuggernaut);
                    Vector3 playerPos = _chosenPlayerToBecomeJuggernaut.transform.position;

                    playerIsBecomingJuggernaut = true; // used to prevent OnDestroy from being called on juggernaut player, since hes still in the game
                    UnityEngine.Object.Destroy(_chosenPlayerToBecomeJuggernaut);

                    var newGo = UnityEngine.Object.Instantiate(_fallbackWavesData.GetSpawnObject(1), playerPos, Quaternion.identity);
                    playerIsBecomingJuggernaut = false;
                    var newActor = newGo.GetComponent<Actor>();
                    if (newActor == null) 
                    {
                        throw new NullReferenceException("newActor is nyull... ur gyame is nyow bwoken!! TwT");
                    }
                    playerBeast.Instance = newGo;

                    newActor.ControlledBy = Actor.ControlledTypes.Human;
                    newActor.playerID = playerId;
                    newActor.IsAI = false;
                    newActor.controllerID = playerBeast.ControllerId;
                    newActor.gangID = gangId;
                    newActor.DressBeast();
                    for (var i = 0; i < 20; i++)
                    {
                        newActor.Laugh(); // maniacal, devious, festering insanity
                    }

                    return true;
                }),
            }
        };

        private static BaseCoagEffectModifier? PickRandomModifierOrNull()
        {
            if (AllPossibleModifiers.Length == 0) return null;
            if (random_modifChance.Next(1, 100) < noModifChance) return null;
            return AllPossibleModifiers[random_modifiers.Next(AllPossibleModifiers.Length - 1)];
        }

        private static void ApplyRandomEffectWithModifiers()
        {
            CoagEffect? randomEffect = PickRandomEffect();
            if (randomEffect == null || _currentlyHandledEffects.Contains(randomEffect)) return;

            BaseCoagEffectModifier? randomModifier = PickRandomModifierOrNull();
            if (randomModifier != null) randomEffect.TryCombineWithModifier(randomModifier);

            _currentlyHandledEffects.Add(randomEffect);
            randomEffect.Enabled = true;
        }

        public override void OnInitializeMelon()
        {
            HarmonyInstance.PatchAll();

            GameManagerNew.add_OnRoundStart(DelegateSupport.ConvertDelegate<Handler>(new Action(() =>
            {
                OnRoundStart?.Invoke();
            })));

            GameManagerNew.add_OnRoundEnd(DelegateSupport.ConvertDelegate<Handler>(new Action(() =>
            {
                OnRoundEnd?.Invoke();
            })));

            SceneManager.add_sceneLoaded(DelegateSupport.ConvertDelegate<UnityAction<Scene, LoadSceneMode>>(new Action<Scene, LoadSceneMode>((scene, loadSceneMode) =>
            {
                OnSceneChanged?.Invoke(scene, loadSceneMode);
            })));

            OnRoundStart += Internal_Mod_OnRoundStart;
            OnSceneChanged += Internal_Mod_OnSceneChanged;

            LoggerInstance.Msg("Mod " + BuildInfo.Name + " v" + BuildInfo.Version + " loaded.");
        }

        public override void OnUpdate()
        {
            OnUpdateEvent?.Invoke();

            foreach (var effect in _currentlyHandledEffects.ToList())
            {
                if (!effect.Enabled) _currentlyHandledEffects.Remove(effect);
            }

            if (_curTimeUntilNextEffect > 0)
            {
                _curTimeUntilNextEffect -= Time.deltaTime;
                return;
            }

            _curTimeUntilNextEffect = TimeBetweenEffects;
            ApplyRandomEffectWithModifiers();
        }

        private void Internal_Mod_OnSceneChanged(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (_fallbackWavesData == null) _fallbackWavesData = Resources.Load("Waves/Grind").Cast<WavesData>();
            if (_noGravity_originalGravity == null) _noGravity_originalGravity = Physics.gravity;

            _curTimeUntilNextEffect = TimeBetweenEffects;
            _currentlyHandledEffects.Clear();
        }

        private void Internal_Mod_OnRoundStart()
        {
            
        }
    }
}