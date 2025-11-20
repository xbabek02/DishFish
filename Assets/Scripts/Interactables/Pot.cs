using Interfaces;
using UnityEngine;
using System.Collections;

namespace Interactables
{
    public class Pot : MonoBehaviour, IInteractable
    {
        [Header("Cooking Settings")]
        public float cookTime = 20f;
        public float burnTime = 20f;   // how long before food becomes burned
        public AudioClip doneSound;
        public AudioSource audioSource;

        private ItemType? itemType = null;
        private Coroutine timerCoroutine;
        private bool isCooked = false;
        private bool isBurned = false;

        [Header("Output Prefabs")]
        public Item CrabDishPrefab;
        public Item FishDishPrefab;
        public Item BurnedDishPrefab;

        void Start()
        {
            cookTime = DifficultyManager.Instance.cookTime;
            burnTime = DifficultyManager.Instance.overcookTime;
        }
        
        public void Interact(Player player)
        {
            if (!player.activeItem)
            {
                if (isCooked || isBurned)
                {
                    // Give correct result
                    Item prefabToGive = null;

                    if (isBurned)
                    {
                        prefabToGive = BurnedDishPrefab;
                    }
                    else if (itemType == ItemType.CrabBowl)
                    {
                        prefabToGive = CrabDishPrefab;
                    }
                    else if (itemType == ItemType.FishBowl)
                    {
                        prefabToGive = FishDishPrefab;
                    }

                    if (prefabToGive != null)
                        player.EquipNew(prefabToGive);

                    // Reset pot
                    ResetPotState();
                }
                return;
            }

            // --- PLAYER TRYING TO PLACE FOOD IN POT ---
            if (itemType == null && (player.activeItem.itemType == ItemType.CrabBowl || player.activeItem.itemType == ItemType.FishBowl))
            {
                itemType = player.activeItem.itemType;

                // Start cooking
                if (timerCoroutine != null)
                    StopCoroutine(timerCoroutine);

                timerCoroutine = StartCoroutine(CookTimer());

                Destroy(player.activeItem.gameObject);
                player.activeItem = null;
                return;
            }
        }

        private IEnumerator CookTimer()
        {
            // Wait for cooking
            yield return new WaitForSeconds(cookTime);

            isCooked = true;

            // Play done sound
            if (audioSource && doneSound)
                audioSource.PlayOneShot(doneSound);

            // Wait before burning
            yield return new WaitForSeconds(burnTime);

            if (isCooked)  // still not taken
                isBurned = true;
        }

        private void ResetPotState()
        {
            itemType = null;
            isCooked = false;
            isBurned = false;

            if (timerCoroutine != null)
                StopCoroutine(timerCoroutine);

            timerCoroutine = null;
        }
    }
}
