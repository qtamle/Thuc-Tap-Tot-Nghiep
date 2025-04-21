using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlot : MonoBehaviour
{
    [SerializeField]
    GameObject Virtual;
    [SerializeField]
    private Image PlayerImgNull;

    [SerializeField]
    private Image Player1Img;

    [SerializeField]
    private Image Player2Img;

    [SerializeField]
    private TMP_Text PlayerName;

    [SerializeField]
    private TMP_Text PlayerSelectedWeapon;

    public void UpdatePlayerSlotDisplay(WeaponSelectState state)
    {
        Virtual.SetActive(true);
        if (state.ClientId == 0)
        {
            if (PlayerImgNull != null)
            {
                PlayerImgNull.enabled = false;
                if (Player1Img != null)
                    Player1Img.enabled = true;
                if (Player2Img != null)
                    Player2Img.enabled = false;
            }
        }

        if (state.ClientId == 1)
        {
            if (PlayerImgNull != null)
            {
                PlayerImgNull.enabled = false;
                if (Player2Img != null)
                    Player2Img.enabled = true;
                if (Player1Img != null)
                    Player1Img.enabled = false;
            }
        }

        if (PlayerName != null)
            PlayerName.text = $"Player {state.ClientId + 1}";
        if (int.TryParse(state.WeaponID.ToString(), out int weaponIdInt))
        {
            var weaponName = WeaponSelectState.GetWeaponNameById(weaponIdInt);
            PlayerSelectedWeapon.text = $"Weapon: {weaponName}";
        }

        else
        {
            PlayerImgNull.enabled = true;
            Player1Img.enabled = false;
            Player2Img.enabled = false;
        }
    }

    public void DisableDisplay()
    {
        PlayerImgNull.enabled = true;
        Player1Img.enabled = false;
        Player2Img.enabled = false;
        PlayerName.text = $"Player Null";
        PlayerSelectedWeapon.text = $"Weapon: No Selected";
        Virtual.SetActive(false);
    }
}
