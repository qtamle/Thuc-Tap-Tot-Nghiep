using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SupplyInfoDisplay : MonoBehaviour
{
    public GameObject infoCanvas;
    public TMP_Text supplyNameText;
    public TMP_Text supplyTypeText;
    public TMP_Text descriptionText;
    public Image supplySpriteImage;

    private SupplyPickup currentSupply;

    private PlayerMovement playerMovement;
    private void Start()
    {
        infoCanvas.SetActive(false);
    }

    public void DisplaySupplyInfo(SupplyPickup supplyPickup)
    {
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.isMovementLocked = true;
            playerMovement.enabled = false;
        }
        currentSupply = supplyPickup;
        infoCanvas.SetActive(true);

        supplyNameText.text = supplyPickup.supplyData.supplyName;
        supplyTypeText.text = "Type of Supply: " + supplyPickup.supplyData.supplyType.ToString();
        descriptionText.text = supplyPickup.supplyData.description;
        supplySpriteImage.sprite = supplyPickup.supplyData.supplySprite;
    }

    public void OnSelectSupply()
    {
        if (currentSupply != null)
        {
            currentSupply.PickupSupply();
            infoCanvas.SetActive(false);
            playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.enabled = true;
            }
        }
    }

    public void OnCancelSelection()
    {
        infoCanvas.SetActive(false);
        if (currentSupply != null)
        {
            playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.enabled = true;
            }
            currentSupply.StartDisableTriggerTimer();
        }
    }
}
