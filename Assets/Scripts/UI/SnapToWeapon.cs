using UnityEngine;
using UnityEngine.UI;

public class SnapToWeapon : MonoBehaviour
{

    public ScrollRect scrollRect;
    public RectTransform contentWeaponPanel;
    public RectTransform sampleListItem;

    public HorizontalLayoutGroup HLG;
    public string[] ItemNames;

    bool isSnapped;

    public float snapForce;
    float snapSpeed;

    void Start()
    {
        isSnapped = false;
    }

  
    void Update()
    {
        int currentItem = Mathf.RoundToInt((0 - contentWeaponPanel.localPosition.x / (sampleListItem.rect.width + HLG.spacing)));
        Debug.Log(currentItem);

        if(scrollRect.velocity.magnitude < 200 && !isSnapped )
        {
            snapSpeed += snapForce * Time.deltaTime;
            contentWeaponPanel.localPosition = new Vector3(
                Mathf.MoveTowards(contentWeaponPanel.localPosition.x,
                                  0 - (currentItem * (sampleListItem.rect.width + HLG.spacing)), snapSpeed),
                contentWeaponPanel.localPosition.y,
                contentWeaponPanel.localPosition.z);
            if(contentWeaponPanel.localPosition.x == 0 - (currentItem *sampleListItem.rect.width + HLG.spacing))
            {
                isSnapped = true;

            }
        }
        if (scrollRect.velocity.magnitude > 200)
        {
            isSnapped = false;
            snapSpeed = 0;
        }

    }
}
