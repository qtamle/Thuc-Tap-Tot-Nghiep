using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SwipeController : MonoBehaviour
{
    //[SerializeField] int maxWeapon;
    //[SerializeField] GameObject weaponPrefab;
    //[SerializeField] RectTransform panelDisplayWeapon; // Container chứa vũ khí

    //int currentWeapon;
    //Vector3 targetPos;
    //[SerializeField] Vector3 weaponStep;
    //[SerializeField] RectTransform WeaponRect;

    //[SerializeField] float tweenTime;
    //[SerializeField] LeanTweenType tweenType;

    //[SerializeField] RectTransform snapWeapon;

    //private void Awake()
    //{
    //    currentWeapon = 1;
    //    targetPos = WeaponRect.localPosition;

    //    //Tạo động các vũ khí
    //    for (int i = 0; i < maxWeapon; i++)
    //    {
    //        GameObject newWeapon = Instantiate(weaponPrefab, panelDisplayWeapon);
    //        newWeapon.name = "Weapon " + (i + 1);
    //        newWeapon.transform.localPosition = i * weaponStep; // Đặt vị trí theo thứ tự
    //    }
    //}

    //public void Next()
    //{
    //    if (currentWeapon < maxWeapon)
    //    {
    //        currentWeapon++;
    //        targetPos += weaponStep;
    //        ChangeWeapon();
    //    }
    //}

    //public void Previous()
    //{
    //    if (currentWeapon > 1)
    //    {
    //        currentWeapon--;
    //        targetPos -= weaponStep;
    //        ChangeWeapon();
    //    }
    //}

    //void ChangeWeapon()
    //{
    //    WeaponRect.LeanMoveLocal(targetPos, tweenTime).setEase(tweenType);
    //}

    //public void SnapWeapon()
    //{
    //    // Tính toán vị trí gần nhất
    //    int closestIndex = Mathf.RoundToInt(snapWeapon.localPosition.x / weaponStep.x);
    //    closestIndex = Mathf.Clamp(closestIndex, 0, maxWeapon - 1);

    //    // Cập nhật vị trí và vũ khí hiện tại
    //    currentWeapon = closestIndex + 1;
    //    targetPos = closestIndex * weaponStep;

    //    snapWeapon.LeanMoveLocal(targetPos, tweenTime).setEase(tweenType);
    //}
}
