public class PlayerProfileExperience : Experience
{
    public static int[] Values = new int[]
    {
        100, //1
        200, //2
        400, //3 
        400, //4
        400, //5
        400, //6
        400, //7
        400, //8
        400, //9
        400 //10
    };

    public PlayerProfileExperience(IExpProvider provider)
        : base(provider, Values)
    { }
}
