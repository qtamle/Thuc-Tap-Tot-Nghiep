using UnityEngine;

public abstract class SupplyBase : MonoBehaviour
{
    protected SpriteRenderer supplySprite;
    protected bool isActive = false;

    protected virtual void Awake () 
    { 
        supplySprite = GetComponentInChildren<SpriteRenderer>();
    }

    public abstract void Active ();
    public abstract void CanActive ();

}
