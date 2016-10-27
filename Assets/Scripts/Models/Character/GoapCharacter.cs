using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GoapCharacter : MonoBehaviour, IGoap
{
    void Start()
    {
        Init();
    }

    public Dictionary<string, bool> createGoalState()
    {
        //var goalState = Brain.NextGoal();
        Dictionary<string, bool> goalState = new Dictionary<string, bool>();
        goalState.Add("hasRaw Iron", true);

        return goalState;
    }

    Dictionary<string, bool> worldData = new Dictionary<string, bool>();
    /**
    * Key-Value data that will feed the GOAP actions and system while planning.
    */

    public Dictionary<string, bool> getWorldState()
    {
        foreach (string key in PrototypeManager.Inventory.Keys)
        {
            worldData["has" + key] = false;
        }


        if (character.inventory != null)
        {
            worldData["has" + character.inventory.GetName ()] = character.inventory.StackSize > 0;
        }


        return worldData;
    }

    BlackBoard bb = new BlackBoard();

    public BlackBoard GetBlackBoard()
    {
        return bb;
    }

    public void planFailed(Dictionary<string, bool> failedGoal)
    {
        // Not handling this here since we are making sure our goals will always succeed.
        // But normally you want to make sure the world state has changed before running
        // the same goal again, or else it will just fail.
    }

    public void planFound(KeyValuePair<string, bool> goal, Queue<GoapAction> actions)
    {
        // Yay we found a plan for our goal
        Debug.ULogChannel("Character", "<color=green>Plan found: </color> " + GoapAgent.prettyPrint(actions));
    }

    public void actionsFinished ()
    {
        // Everything is done, we completed our actions for this gool. Hooray!
        Debug.ULogChannel("Character", "<color=blue>Actions completed</color>");
    }

    public void planAborted (GoapAction aborter)
    {
        // An action bailed out of the plan. State has been reset to plan again.
        // Take note of what happened and make sure if you run the same goal again
        // that it can succeed.
        Debug.ULogChannel("Character", "<color=red>Plan Aborted: </color> " + GoapAgent.prettyPrint(aborter));
    }

    private List<Tile> path;
    private float movementPercentage;
    private float distToTravel;
    private Tile nextTile;
    private bool entered;

    public bool moveAgent(GoapAction nextAction, float deltaTime) {
        if (path == null && !entered)
        {
            entered = true;
            path = ProjectPorcupine.Pathfinding.Pathfinder.FindPathToTile(character.CurrTile, nextAction.getTargetTile(), true);

            character.IsWalking = true;

            if (character.IsSelected)
            {
                VisualPath.Instance.SetVisualPoints(character.name, new List<Tile>(path));
            }

            if (path == null || path.Count == 0)
            {
                Exit();
                planAborted(nextAction);
                return false;
            }

            // The starting tile might be included, so we need to get rid of it
            while (path[0].Equals(character.CurrTile))
            {
                path.RemoveAt(0);

                if (path.Count == 0)
                {
                    Debug.Log(" - Ran out of path to walk");

                    // We've either arrived, or we need to find a new path to the target
                    Exit();
                    return true;
                }
            }

            AdvanceNextTile();

            distToTravel = Mathf.Sqrt(
                Mathf.Pow(character.CurrTile.X - nextTile.X, 2) +
                Mathf.Pow(character.CurrTile.Y - nextTile.Y, 2));

            return false;
        }

        if (nextTile == null)
            return true;

        if (nextTile.IsEnterable() == Enterability.Soon)
        {
            // We can't enter the NOW, but we should be able to in the
            // future. This is likely a DOOR.
            // So we DON'T bail on our movement/path, but we do return
            // now and don't actually process the movement.
            return false;
        }

        // How much distance can be travel this Update?
        float distThisFrame = character.MovementSpeed / nextTile.MovementCost * deltaTime;

        // How much is that in terms of percentage to our destination?
        float percThisFrame = distThisFrame / distToTravel;

        // Add that to overall percentage travelled.
        movementPercentage += percThisFrame;

        if (movementPercentage >= 1f)
        {
            // We have reached the next tile
            character.CurrTile = nextTile;
            character.CurrTile.OnEnter();

            float overshotMovement = Mathf.Clamp01(movementPercentage - 1f);
            movementPercentage = 0f;

            // Arrived at the destination or run out of path.
            if (hasReachedDestination(character.CurrTile, nextAction.getTargetTile()) || path.Count == 0)
            {
                Exit();
                nextAction.setInRange(true);
                return true;
            }

            AdvanceNextTile();

            distToTravel = Mathf.Sqrt(
                Mathf.Pow(character.CurrTile.X - nextTile.X, 2) +
                Mathf.Pow(character.CurrTile.Y - nextTile.Y, 2) +
                Mathf.Pow(character.CurrTile.Z - nextTile.Z, 2));

            if (nextTile.IsEnterable() == Enterability.Yes)
            {
                movementPercentage = overshotMovement;
            }
            else if (nextTile.IsEnterable() == Enterability.Never)
            {
                // Most likely a wall got built, so we just need to reset our pathfinding information.
                // FIXME: Ideally, when a wall gets spawned, we should invalidate our path immediately,
                //            so that we don't waste a bunch of time walking towards a dead end.
                //            To save CPU, maybe we can only check every so often?
                //            Or maybe we should register a callback to the OnTileChanged event?
                // Debug.ULogErrorChannel("FIXME", "A character was trying to enter an unwalkable tile.");

                // Should the character show that he is surprised to find a wall?
                Exit();
                planAborted(nextAction);
                return false;
            }
        }

        character.TileOffset = new Vector3(
            (nextTile.X - character.CurrTile.X) * movementPercentage,
            (nextTile.Y - character.CurrTile.Y) * movementPercentage,
            (nextTile.Z - character.CurrTile.Z) * movementPercentage);

        return false;
    }

    public void Exit()
    {
        entered = false;
        character.IsWalking = false;
        VisualPath.Instance.RemoveVisualPoints(character.name);
    }

    private void AdvanceNextTile()
    {
        nextTile = path[0];
        path.RemoveAt(0);

        character.FaceTile(nextTile);
    }

    private bool hasReachedDestination(Tile currTile, Tile goalTile)
    {
        int minX = goalTile.X - 1;
        int maxX = goalTile.X + 1;
        int minY = goalTile.Y - 1;
        int maxY = goalTile.Y + 1;
        int minZ = goalTile.Z - 1;
        int maxZ = goalTile.Z + 1;

        // Tile is either adjacent on the same level, or directly above/below, and if above, is empty
        return (currTile.X >= minX && currTile.X <= maxX &&
            currTile.Y >= minY && currTile.Y <= maxY &&
            currTile.Z == goalTile.Z &&
            goalTile.IsClippingCorner(currTile) == false) || 
            ((currTile.Z >= minZ && currTile.Z <= maxZ &&
                currTile.X == goalTile.X &&
                currTile.Y == goalTile.Y) && 
                (currTile.Z >= goalTile.Z ||
                    currTile.Type == TileType.Empty));
    }

    public void Init()
    {
        if (Brain == null)
            Brain = gameObject.GetComponent<Brain>();
        Brain.Init();

        //init world data
        foreach (string key in PrototypeManager.Inventory.Keys)
        {
            //Debug.LogWarning("KEY: " + key);
            worldData.Add("has" + key, false);
        }
        
        //init blackboard
        bb.AddData("brain", Brain);
        bb.AddData("character", character);
    }

    public void Tick(float deltaTime)
    {
        Brain.Tick(this, deltaTime);

        if (NeedAbort)
        {
            Agent.AbortFsm();
            NeedAbort = false;
        }
    }

    public void Release()
    {
        Brain.Release();
    }

    public bool NeedAbort;

    public IAgent Agent { get; set; }

    public IBrain Brain { get; set; }

    public Character character { get; set; }
}