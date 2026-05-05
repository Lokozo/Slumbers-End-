using UnityEngine;
using TMPro;

public class UIRegister : MonoBehaviour
{
    public enum UIType
    {
        InventoryMenu,
        ResourcePanel,
        ResourceContentPanel,
        MagnifyingGlass,
        CheckIcon,
        LootableNameText,
        PushPullIcon,
        BreakableIcon
    }

    public UIType type;

    private void Awake()
    {
        var ui = UIManager.Instance;

        switch (type)
        {
            case UIType.InventoryMenu:
                ui.inventoryMenu = gameObject;
                break;

            case UIType.ResourcePanel:
                ui.resourcePanel = gameObject;
                break;

            case UIType.ResourceContentPanel:
                ui.resourceContentPanel = transform;
                break;

            case UIType.MagnifyingGlass:
                ui.magnifyingGlassIcon = gameObject;
                break;

            case UIType.CheckIcon:
                ui.checkIcon = gameObject;
                break;

            case UIType.LootableNameText:
                ui.lootableNameText = GetComponent<TextMeshProUGUI>();
                break;
            case UIType.PushPullIcon:
                ui.pushPullIcon = gameObject;
                break;
            case UIType.BreakableIcon:
                ui.breakableIcon = gameObject;
                break;
        }
    }
}