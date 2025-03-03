using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

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
        // Lấy PlayerMovement từ chính Player đang nhặt supply
        GameObject player = GameObject.FindGameObjectWithTag("Player"); // Tìm player hiện tại
        playerMovement = player.GetComponent<PlayerMovement>();

        if (playerMovement != null)
        {
            playerMovement.isMovementLocked = true; // Chỉ khoá di chuyển, không vô hiệu hoá script
        }

        currentSupply = supplyPickup;
        infoCanvas.SetActive(true);

        supplyNameText.text = supplyPickup.supplyData.supplyName.ToString();
        supplyTypeText.text = $"Type of Supply: {supplyPickup.supplyData.supplyType}";
        descriptionText.text = supplyPickup.supplyData.description;
        supplySpriteImage.sprite = supplyPickup.supplyData.supplySprite;
    }

    public void OnSelectSupply()
    {
        if (currentSupply != null)
        {
            currentSupply.PickupSupply();
            infoCanvas.SetActive(false);
            if (playerMovement != null)
            {
                playerMovement.isMovementLocked = false;
            }
        }
    }

    public void OnCancelSelection()
    {
        infoCanvas.SetActive(false);
        if (currentSupply != null)
        {
            if (playerMovement != null)
            {
                playerMovement.isMovementLocked = false;
            }
            currentSupply.StartDisableTriggerTimer();
        }
    }
}
