public class PlayerExperience 
{
    public interface IExpProvider
    {
        int Exp { get; set; }
        int Level { get; set; }
    }

    static public int[] Values = new int[]
    {
        10, //1
        20, 
        40,
        60,
        80, //5
        100,
        120,
        140,
        160,
        180, //10
        220,
        260,
        300,
        340,
        380, //15
        420,
        460,
        500,
        540,
        580 //20
    };

    private IExpProvider expProvider_;

    public System.Action<int> OnLevelUp
    { set; private get; }

    public PlayerExperience(IExpProvider provider)
    {
        expProvider_ = provider;
    }

    public void AddExperience(int value)
    {
        int expGenerated = value;
        if (expProvider_.Level - 1 < PlayerExperience.Values.Length)
        {
            int expNeed = PlayerExperience.Values[expProvider_.Level - 1];

            while (expProvider_.Level - 1 <= PlayerExperience.Values.Length && expGenerated > 0)
            {
                expProvider_.Exp += expGenerated;
                expGenerated = expProvider_.Exp > expNeed ? expProvider_.Exp - expNeed : 0;

                if (expProvider_.Exp >= expNeed)
                {
                    expProvider_.Exp = 0;
                    expProvider_.Level++;
                    if (OnLevelUp != null)
                        OnLevelUp(expProvider_.Level);
                }

                expNeed = PlayerExperience.Values[expProvider_.Level - 1];
            }
        }
    }

    public float ExpProgress
    {
        get
        {
            return expProvider_.Level - 1 < Values.Length ? (float)expProvider_.Exp / (float) Values[expProvider_.Level-1] : 1.0f;
        }
    }
}
