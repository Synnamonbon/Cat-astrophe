using System;
using UnityEngine;

public class Task
{
    public string taskName;
    public string taskDescription;
    public int playerID;                           // Owner
    public int target;
    private int progress = 0;
    private bool complete = false;
    public TaskType taskType;                      // Level of task for rewards
    public ConditionTrack conditionTrack;          // Event condition type
    public string conditionTag;                    // Tag of condition type
    public event Action<string, int> UpdateProgressEvent;
    public event Action<Task> CompleteTaskEvent;

    public void InitTask(int pid, TaskDetails_SO taskDetails)
    {
        AssignToPlayer(pid);
        SetTaskFromScriptable(taskDetails);
    }

    private void AssignToPlayer(int pid)
    {
        playerID = pid;
    }

    private void SetTaskFromScriptable(TaskDetails_SO taskDetails)
    {
        taskName = taskDetails.TaskName;
        taskDescription = taskDetails.TaskDescription;
        target = taskDetails.Target;
        taskType = taskDetails.TaskLevel;
        conditionTrack = taskDetails.TaskConditionTrack;
        conditionTag = taskDetails.TaskConditionTag;
    }

    public void IncrementCondition()
    {
        if(complete){return;}
        progress++;
        //Debug.Log(taskName + ": " + progress + "/" + target + " for " + playerID);
        // Replace with updating UI layer.
        UpdateProgressEvent?.Invoke(taskName, progress);
        if (progress >= target)
        {
            complete = true;
            CompleteTaskEvent?.Invoke(this);
        }
    }
}
