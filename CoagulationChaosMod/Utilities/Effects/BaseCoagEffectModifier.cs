namespace CoagulationChaosMod.Utilities.Effects
{
    public class BaseCoagEffectModifier
    {
        public string adjectiveName = "untitled";

        public string PrefixName => adjectiveName;
        public string SuffixName => "of " + adjectiveName + "-ness";

        public readonly CoagEffect? parent = null;
        public readonly bool isPrefix = true;

        public BaseCoagEffectModifier(string adjectiveName, bool isPrefix = true)
        {
            this.adjectiveName = adjectiveName;
            this.isPrefix = isPrefix;
        }

        public BaseCoagEffectModifier(string adjectiveName, CoagEffect toApplyTo, bool isPrefix = true) 
        {
            this.adjectiveName = adjectiveName;
            this.isPrefix = isPrefix;
            parent = toApplyTo;

            toApplyTo.TryCombineWithModifier(this, isPrefix);
        }
    }
}
