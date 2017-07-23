public class PlayerExperience : Experience
{
    static public int[] Values = new int[]
    {
        100, //1
        200, 
        300,
        400,
        500, //5
        600,
        700,
        800,
        900,
        1000, //10
        1100,
        1200,
        1300,
        1400,
        1500, //15
        1600,
        1700,
        1800,
        1900,
        2000, //20
        2100,
        2200,
        2300,
        2400,
        2500, //25
        2600,
        2700,
        2800,
        2900,
        3000,
        3100,
        3200,
        3300,
        3400,
        3500 //35
    };

    public PlayerExperience(IExpProvider provider)
        : base(provider, Values)
    { }
}
