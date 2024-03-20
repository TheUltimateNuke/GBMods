using Il2Cpp;
using Il2CppGB.Game;
using MelonLoader;
using System.Text;
using UnityEngine;

namespace CoagulationChaosMod.Utilities.Effects
{
    public class CoagEffect
    {
        public const int maxEffects = 5;

        private event Action? OnEffectEnable;
        private event Action? OnEffectDisable;

        public Func<bool>? CustomEffectEnableAction;
        public Func<bool>? CustomEffectUpdateAction;
        public Action? CustomEffectDisableAction;

        public string baseName = "untitled effect";

        private float curDuration = maxEffectTime;

        private const int minEffectTime = 10;
        private const int maxEffectTime = 25;

        private System.Random? randInstance = null;

        private readonly int maxPrefixModifiers = 1;
        private readonly int maxSuffixModifiers = 1;

        public bool Enabled { 
            get 
            {
                return _enabled;
            }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    if (_enabled)
                        OnEffectEnable?.Invoke();
                    else
                        OnEffectDisable?.Invoke();
                }
            }
        }
        private bool _enabled = false;

        public BaseCoagEffectModifier[] PrefixModifiers => _prefixModifiers.ToArray();
        public BaseCoagEffectModifier[] SuffixModifiers => _suffixModifiers.ToArray();

        private readonly List<BaseCoagEffectModifier> _prefixModifiers = new();
        private readonly List<BaseCoagEffectModifier> _suffixModifiers = new();

        private readonly bool canBeModified = true;

        public CoagEffect(string baseName, bool canBeModified=true)
        {
            randInstance = new(Mod.RandomSeed);

            this.baseName = baseName;
            this.canBeModified = canBeModified;
            curDuration = randInstance.Next(minEffectTime, maxEffectTime);

            Mod.OnSceneChanged += Internal_CoagEffect_OnSceneChanged;

            OnEffectEnable += Internal_CoagEffect_OnEnable;
            OnEffectDisable += Internal_CoagEffect_OnDisable;
            Mod.OnUpdateEvent += Internal_CoagEffect_OnUpdate;
        }

        public override string ToString()
        {
            StringBuilder titleBuilder = new();

            foreach (var modifier in PrefixModifiers)
            {
                titleBuilder.Append(modifier.PrefixName + " ");
            }

            titleBuilder.Append(baseName);

            foreach (var modifier in SuffixModifiers)
            {
                titleBuilder.Append(" " + modifier.SuffixName + " ");
            }

            return titleBuilder.ToString();
        }

        public bool HasModifier(string modifierName)
        {
            return _prefixModifiers.Find(mod => mod.adjectiveName == modifierName) != null || _suffixModifiers.Find(mod => mod.adjectiveName == modifierName) != null;
        }

        public bool TryCombineWithModifier(BaseCoagEffectModifier modifier, bool isPrefix = true)
        {
            if ((PrefixModifiers.Length >= maxPrefixModifiers && isPrefix) || (SuffixModifiers.Length >= maxSuffixModifiers && !isPrefix)) return false;
            if (!canBeModified) return false;

            if (isPrefix) _prefixModifiers.Add(modifier); else _suffixModifiers.Add(modifier);
            return true;
        }

        public bool TryRemoveModifier(BaseCoagEffectModifier modifier, bool removeFromPrefix = true)
        {
            if (!canBeModified) return false;

            return removeFromPrefix ? _prefixModifiers.Remove(modifier) : _suffixModifiers.Remove(modifier);
        }

        private void Internal_CoagEffect_OnSceneChanged(UnityEngine.SceneManagement.Scene _scene, UnityEngine.SceneManagement.LoadSceneMode _loadSceneMode)
        {
            try
            {
                if (GameManagerNew.Instance == null)
                {
                    randInstance = new System.Random(Mod.RandomSeed);
                }
            }
            catch (NullReferenceException e)
            {
                Melon<Mod>.Logger.Warning("NullReferenceException in Internal_CoagEffect_OnSceneChanged(). This can be safely ignored, as long as you weren't in a game when this message was thrown. " + e);
            }
            
        }

        private void Internal_CoagEffect_OnEnable()
        {
            Melon<Mod>.Logger.Msg("Effect \"" + ToString() + "\" enabled!");

            if (CustomEffectEnableAction?.Invoke() == false) Enabled = false;
        }

        private void Internal_CoagEffect_OnDisable()
        {
            CustomEffectDisableAction?.Invoke();

            Melon<Mod>.Logger.Msg("Effect \"" + ToString() + "\" disabled!");
        }

        private void Internal_CoagEffect_OnUpdate()
        {
            if (!Enabled) return;

            CustomEffectUpdateAction?.Invoke();
            if (curDuration > 0) curDuration -= Time.deltaTime; 
            else 
            {
                if (randInstance != null) curDuration = randInstance.Next(minEffectTime, maxEffectTime);
                Enabled = false;
            }
        }

        public static CoagEffect? operator +(CoagEffect left, BaseCoagEffectModifier right)
        {
            left.TryCombineWithModifier(right);
            return left;
        }

        public static CoagEffect? operator +(BaseCoagEffectModifier left, CoagEffect right)
        {
            right.TryCombineWithModifier(left);
            return right;
        }

        public static CoagEffect? operator -(CoagEffect left, BaseCoagEffectModifier right)
        {
            left.TryRemoveModifier(right);
            return left;
        }
    }
}
