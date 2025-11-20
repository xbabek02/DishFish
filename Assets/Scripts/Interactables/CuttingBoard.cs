using Interfaces;
using UnityEngine;

namespace Interactables
{
    public class CuttingBoard : MonoBehaviour, IInteractable
    {
        public Item fishBowlPrefab;
        public Item crabBowlPrefab;
        
        public void Interact(Player player)
        {
            if (player.activeItem && player.activeItem.itemType == ItemType.Crab)
            {
                player.EquipNew(crabBowlPrefab);
            }
            
            if (player.activeItem && player.activeItem.itemType == ItemType.Fish)
            {
                player.EquipNew(fishBowlPrefab);
            }
        }
    }
}