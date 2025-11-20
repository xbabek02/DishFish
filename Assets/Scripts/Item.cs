using UnityEngine;


public enum ItemType
{
    Fish,
    Crab,
    FishBowl,
    CrabBowl,
    FishingRod,
    FishDish,
    CrabDish,
    BurnedFood
}

public class Item : MonoBehaviour
{
    public ItemType itemType;
    public Vector3 handPosition;
    public Quaternion handRotation;
}
