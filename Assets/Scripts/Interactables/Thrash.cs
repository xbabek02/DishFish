using Interfaces;
using UnityEngine;

namespace Interactables
{
    public class Thrash : MonoBehaviour, IInteractable
    {
        public void Interact(Player player)
        {
            if (!player.activeItem)
            {
                return;
            }
            Destroy(player.activeItem.gameObject);
            player.activeItem = null;
        }
    }
}