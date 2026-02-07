using System;
using UnityEngine;
using UnityEngine.Assertions;

public class ItemController : TriggerController
{
    private static readonly int INVALID_ID = 0;

    [SerializeField] private GameObject m_Item;
    [SerializeField] private bool m_IsRevealer = false;

    public int UniqueID { get; private set; } = INVALID_ID;

    private void Awake()
    {
        Assert.IsNotNull(m_Item, "Please assign a valid GameObject to the item member.");

        UniqueID = m_Item.GetInstanceID();
    }

    protected override void Interact()
    {
        PickItem();

        CanInteract = false;
    }

    private void PickItem()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.StoreItem(UniqueID);
            UISystem.Instance.HidePlayerTip();
        }

        if (m_Item != null)
        {
            if (m_IsRevealer) 
            {
                // niveau 4 : On active l'objet (le pont)
                m_Item.SetActive(true);
            }
            else 
            {
                // niveau 1-3 : On désactive l'objet (la clé disparaît)
                m_Item.SetActive(false);
            }
        }


        // On désactive le trigger lui-même pour ne pas cliquer 2 fois
        this.gameObject.SetActive(false);
    }
}
