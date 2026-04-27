using UnityEngine;

[CreateAssetMenu(fileName = "New TaskDetails", menuName = "DesignerStuff/TaskDetails_SO")]
public class TaskDetails_SO : ScriptableObject
{
    // override this to set taskname, taskdesc, target, tasktype, condition track and tag.
    [SerializeField] private string taskName;
    [SerializeField] private string taskDescription;
    [SerializeField] private int target;
    [SerializeField] private TaskType taskLevel;
    [SerializeField] private ConditionTrack taskConditionTrack;
    [SerializeField] private string taskConditionTag;
    public string TaskName => taskName;
    public string TaskDescription => taskDescription;
    public int Target => target;
    public TaskType TaskLevel => taskLevel;
    public ConditionTrack TaskConditionTrack => taskConditionTrack;
    public string TaskConditionTag => taskConditionTag;
}
