using Interfaces;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    [CanBeNull] public Item activeItem;
    public Transform handTransform;
    public Bar throwBar;
    public TMP_Text moneyText;

    private int _money;
    public int Money
    {
        get => _money;
        set
        {
            _money = value;
            OnMoneyChanged();
        }
    }

    private void OnMoneyChanged()
    {
        if (moneyText != null)
            moneyText.text = _money.ToString();
    }

    public void EquipNew(Item prefab,  bool destroyPrev=true)
    {
        if (destroyPrev && activeItem)
        {
            Destroy(activeItem.gameObject);
        }
        Item item = Instantiate(
            prefab, 
            transform.position, 
            transform.rotation,
            handTransform
        );
        
        ApplyHandOffset(item);
        activeItem = item;
    }

    public void Equip(Item item, bool destroyPrev=true)
    {
        if (destroyPrev && activeItem)
        {
            Destroy(activeItem.gameObject);
        }
        
        item.gameObject.transform.SetParent(handTransform);
        item.transform.position = handTransform.position;
        item.transform.rotation = handTransform.rotation;
        
        ApplyHandOffset(item);
        activeItem = item;
    }
    
    private void ApplyHandOffset(Item item)
    {
        item.transform.localPosition = item.handPosition;
        item.transform.localRotation = item.handRotation;
    }
}