public class Experience
{
    public interface IExpProvider
    {
        int Exp { get; set; }
        int Level { get; set; }
    }

    private IExpProvider expProvider_;
    private int[] expTable_;

    public System.Action<int> OnLevelUp
    { set; private get; }

    public Experience(IExpProvider provider, int[] expTable)
    {
        expProvider_ = provider;
        expTable_ = expTable;
    }

    public void AddExperience(int value)
    {
        int expGenerated = value;
        if (expProvider_.Level - 1 < expTable_.Length)
        {
            int expNeed = expTable_[expProvider_.Level - 1];

            while (expProvider_.Level - 1 < expTable_.Length && expGenerated > 0)
            {
                expNeed = expTable_[expProvider_.Level - 1];
                expProvider_.Exp += expGenerated;
                expGenerated = expProvider_.Exp > expNeed ? expProvider_.Exp - expNeed : 0;

                if (expProvider_.Exp >= expNeed)
                {
                    expProvider_.Exp = 0;
                    expProvider_.Level++;
                    if (OnLevelUp != null)
                        OnLevelUp(expProvider_.Level);
                }
            }
        }
    }

    public int Level
    {
        get { return expProvider_.Level; }
    }

    public float ExpProgress
    {
        get
        {
            return expProvider_.Level - 1 < expTable_.Length ? (float)expProvider_.Exp / (float)expTable_[expProvider_.Level - 1] : 1.0f;
        }
    }
}
