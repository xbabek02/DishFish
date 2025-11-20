using Interfaces;
using UnityEngine;

namespace Interactables
{
    public class RodBarell : MonoBehaviour, IInteractable
    {
        public Item fishingRodPrefab;
        
        public void Interact(Player player)
        {
            player.EquipNew(fishingRodPrefab);
        }
    }
}