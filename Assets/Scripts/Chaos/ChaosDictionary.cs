using System.Collections.Generic;

public static class ChaosDictionary
{
    private static Dictionary<string, int> dict = 
    new Dictionary<string, int>() 
    {
        {"ObjectType.Small", 10},
        {"ObjectType.Medium", 25},
        {"ObjectType.Large", 50},
        {"CatSave", 50},
        {"TaskType.Level1", 75},
        {"TaskType.Level2", 100},
        {"TaskType.Level3", 150},
        {"TaskType.Level4", 250}
    };

    public static int GetPointsForEvent(ObjectType objectType)
    {
        string key = "ObjectType." + objectType.ToString();
        int points;
        if (dict.TryGetValue(key, out points))
        {
            return points;
        }
        else
        {
            return 0;
        }
    }

    public static int GetPointsForEvent(string specific)
    {
        int points;
        if (dict.TryGetValue(specific, out points))
        {
            return points;
        }
        else
        {
            return 0;
        }
    }

    public static int GetPointsForEvent(TaskType taskType)
    {
        string key = "TaskType." + taskType.ToString();
        int points;
        if (dict.TryGetValue(key, out points))
        {
            return points;
        }
        else
        {
            return 0;
        }
    }
}