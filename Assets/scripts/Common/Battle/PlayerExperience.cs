public class PlayerExperience : Experience
{
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
        580, //20
        700,
        850,
        1000,
        1150,
        1300 //25
    };

    public PlayerExperience(IExpProvider provider)
        : base(provider, Values)
    { }
}
