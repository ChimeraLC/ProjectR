
// Latent action delegate, that gives delta since last tick and total time spent
using System.Collections.Generic;
using Godot;

public delegate void LatentDelegate(float delta, float currentTick, float proportion);
public delegate void BasicDelegate();
public class LatentAction
{
    public float duration; // Duration of action
    public float currentTime; // How long the action has been running
    public LatentDelegate latentDel; // Delegate to call each tick
    public BasicDelegate basicDel;  // Delegate to call at the end

    public LatentAction(float duration, LatentDelegate latentDel, BasicDelegate basicDel)
    {
        this.duration = duration;
        this.latentDel = latentDel;
        this.basicDel = basicDel;
        currentTime = 0;
    }
}

public class LatentManager
{
    public static LatentManager Instance {get; private set;}

    private HashSet<LatentAction> latentActions;
    private HashSet<LatentAction> finishedActions;

    public LatentManager()
    {
        Instance = this;
    
        latentActions = new HashSet<LatentAction>();
        finishedActions = new HashSet<LatentAction>();
    }

    public void Tick(float delta)
    {
        finishedActions.Clear();

        // Run each of the latent actions
        foreach (LatentAction action in latentActions)
        {
            action.currentTime += delta;
            if (action.latentDel != null)
            {
                action.latentDel(delta, action.currentTime, 
                    Mathf.Min(1, action.currentTime / action.duration));
            }

            if (action.currentTime >= action.duration)
            {
                // Call end function
                if (action.basicDel != null)
                {
                    action.basicDel();
                }
                
                finishedActions.Add(action);
            }
        }

        foreach(LatentAction action in finishedActions)
        {
            latentActions.Remove(action);
        }
    }

    // Runs a function every tick for a duration
    public static void RunLatentAction(float duration, LatentDelegate del)
    {
        if (Instance != null)
        {
            Instance.latentActions.Add(new LatentAction(duration, del, null));
        }
        else
        {
            Logger.Log("Attempted to add latent action before latent manager was initialized",
                LogLevel.error);
        }
    }

    // Runs a function at the end of a duration
    public static void RunBasicAction(float duration, BasicDelegate del)
    {
        if (Instance != null)
        {
            Instance.latentActions.Add(new LatentAction(duration, null, del));
        }
        else
        {
            Logger.Log("Attempted to add latent action before latent manager was initialized",
                LogLevel.error);
        }
    }
}