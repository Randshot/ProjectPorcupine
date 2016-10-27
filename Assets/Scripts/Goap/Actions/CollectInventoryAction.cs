
using System;
using UnityEngine;

public class CollectInventoryAction : GoapAction
{
    private bool hasInventory = false;
    private Inventory targetInventory;
    private bool requireInRange = true;
    private string type;

    public float Cost = 1f;
	
    public CollectInventoryAction() {}
	
    public void Init(string type)
    {
        addEffect ("has" + type, true);
        this.type = type;
    }
	
	public override void reset ()
	{
		hasInventory = false;
		targetInventory = null;
	}
	
	public override bool isDone ()
	{
		return hasInventory;
	}
	
	public override bool requiresInRange ()
	{
        return requireInRange;
	}

    public override bool hasTarget()
    {
        return targetInventory != null;
    }

    public override Tile getTargetTile()
    {
        return targetInventory.Tile;
    }
	
	public override bool checkProceduralPrecondition (GameObject agent, BlackBoard bb)
	{
        GoapCharacter goapCharacter = agent.GetComponent<GoapCharacter>();
        targetInventory = World.Current.InventoryManager.GetClosestInventoryOfType(type, goapCharacter.character.CurrTile, true);

        if (targetInventory != null)
            return true;

        return false;
	}
	
	public override bool perform(GameObject agent, BlackBoard bb)
	{
        GoapCharacter goapCharacter = agent.GetComponent<GoapCharacter>();
        if (goapCharacter.character.inventory == null)
            goapCharacter.character.inventory = new Inventory(targetInventory.Type, targetInventory.StackSize, targetInventory.MaxStackSize);
        else
            goapCharacter.character.inventory.StackSize += targetInventory.StackSize;
        World.Current.InventoryManager.ConsumeInventory(targetInventory.Tile, targetInventory.StackSize);

        hasInventory = true;
        return true;
	}
}

