using System.Collections.Generic;

public static class ChaosDictionary
{
    private static Dictionary<string, int> dict = 
    new Dictionary<string, int>() 
    {
        {"ObjectType.Small", 10},
        {"ObjectType.Medium", 25},
        {"ObjectType.Large", 50},
        {"CatSave", 50}
    };

    public static int GetPointsForEvent(ObjectType objectType)
    {
        string key = "ObjectType." + objectType.ToString();
        int points;
        dict.TryGetValue(key, out points);
        return points;
    }

    public static int GetPointsForEvent()
    {
        return 0;
    }
}