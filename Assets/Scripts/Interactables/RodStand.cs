using Interfaces;
using UnityEngine;

namespace Interactables
{
    public class RodStand : MonoBehaviour, IInteractable
    {
        public Transform rodPosition;
        public Item fishingRod;

        public void Interact(Player player)
        {
            // Check if the held item has a FishingRodController
            if (player.activeItem && player.activeItem.itemType == ItemType.FishingRod
                                  && !fishingRod)
            {
                fishingRod = player.activeItem;
                // Move the fishing rod to the stand’s position
                fishingRod.transform.SetParent(rodPosition);
                fishingRod.transform.position = rodPosition.position;
                fishingRod.transform.rotation = rodPosition.rotation;

                // Player is no longer holding the fishing rod
                
                player.activeItem = null;
                
            }
            else if (!player.activeItem && fishingRod)
            {
                player.Equip(fishingRod);
                fishingRod = null;
            }
        }
    }
}