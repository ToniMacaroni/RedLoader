namespace RedLoader
{
    public sealed class ResolvedMelons // This class only exists because I can't use Tuples
    {
        public readonly ModBase[] loadedMelons;
        public readonly RottenMelon[] rottenMelons;

        public ResolvedMelons(ModBase[] loadedMelons, RottenMelon[] rottenMelons)
        {
            this.loadedMelons = loadedMelons ?? new ModBase[0];
            this.rottenMelons = rottenMelons ?? new RottenMelon[0];
        }
    }
}
