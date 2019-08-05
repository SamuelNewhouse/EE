using UnityEngine;

public class ElementBox : MonoBehaviour
{
    enum Element
    {
        Void, Balance, Order, Chaos, Stone, Lightning, Ice, Fire, Water, Wind, Shadow, Light, Death, Life
    }

    enum Category
    {
        Void, Balance, Order, Chaos
    }

    enum Location
    {
        Void, Point, Material, Force
    }

    enum LocationSubtype
    {
        None, Base, Transcendent
    }

    [SerializeField] Element element;
    [SerializeField] Category category;
    [SerializeField] Location location;
    [SerializeField] LocationSubtype locationSubtype;    

    delegate void ElementBoxCollisionHandler(ElementBox thisBox, ElementBox otherBox);
    ElementBoxCollisionHandler handler;

    static void VoidHandler(ElementBox thisBox, ElementBox otherBox)
    {
        if (otherBox.category != Category.Void && otherBox.category != Category.Balance)
            Object.Destroy(otherBox.gameObject);
    }

    static void BalanceHandler(ElementBox thisBox, ElementBox otherBox)
    {
        // -- Balance intentionally does nothing.
    }

    static void OrderHandler(ElementBox thisBox, ElementBox otherBox)
    {
        if (otherBox.element == Element.Chaos)
            Object.Destroy(otherBox.gameObject);
    }

    static void ChaosHandler(ElementBox thisBox, ElementBox otherBox)
    {
        if (otherBox.element == Element.Order)
            Object.Destroy(otherBox.gameObject);
    }

    static void StoneHandler(ElementBox thisBox, ElementBox otherBox) {
        if (otherBox.element == Element.Lightning)
            Object.Destroy(otherBox.gameObject);
    }

    static void LightningHandler(ElementBox thisBox, ElementBox otherBox)
    {
        if (otherBox.element == Element.Stone)
            Object.Destroy(otherBox.gameObject);
    }

    static void IceHandler(ElementBox thisBox, ElementBox otherBox)
    {
        if (otherBox.element == Element.Fire)
            Object.Destroy(otherBox.gameObject);
    }

    static void FireHandler(ElementBox thisBox, ElementBox otherBox)
    {
        if (otherBox.element == Element.Ice)
            Object.Destroy(otherBox.gameObject);
    }

    static void WaterHandler(ElementBox thisBox, ElementBox otherBox)
    {
        if (otherBox.element == Element.Wind)
            Object.Destroy(otherBox.gameObject);
    }

    static void WindHandler(ElementBox thisBox, ElementBox otherBox)
    {
        if (otherBox.element == Element.Water)
            Object.Destroy(otherBox.gameObject);
    }

    static void ShadowHandler(ElementBox thisBox, ElementBox otherBox)
    {
        if (otherBox.element == Element.Light)
            Object.Destroy(otherBox.gameObject);
    }

    static void LightHandler(ElementBox thisBox, ElementBox otherBox)
    {
        if (otherBox.element == Element.Shadow)
            Object.Destroy(otherBox.gameObject);
    }

    static void DeathHandler(ElementBox thisBox, ElementBox otherBox)
    {
        if (otherBox.element == Element.Life)
            Object.Destroy(otherBox.gameObject);
    }

    static void LifeHandler(ElementBox thisBox, ElementBox otherBox)
    {
        if (otherBox.element == Element.Death)
            Object.Destroy(otherBox.gameObject);
    }

    void Start()
    {
        switch (element)
        {
            case Element.Void:
                handler = VoidHandler;
                break;
            case Element.Balance:
                handler = BalanceHandler;
                break;
            case Element.Order:
                handler = OrderHandler;
                break;
            case Element.Chaos:
                handler = ChaosHandler;
                break;
            case Element.Stone:
                handler = StoneHandler;
                break;
            case Element.Lightning:
                handler = LightningHandler;
                break;
            case Element.Ice:
                handler = IceHandler;
                break;
            case Element.Fire:
                handler = FireHandler;
                break;
            case Element.Water:
                handler = WaterHandler;
                break;
            case Element.Wind:
                handler = WindHandler;
                break;
            case Element.Shadow:
                handler = ShadowHandler;
                break;
            case Element.Light:
                handler = LightHandler;
                break;
            case Element.Death:
                handler = DeathHandler;
                break;
            case Element.Life:
                handler = LifeHandler;
                break;
            default: // @TODO Default to avoid NullReferenceException while creating. Might change/remove later?
                handler = BalanceHandler; 
                break;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        ElementBox elementBox = collision.gameObject.GetComponent<ElementBox>();
        if (elementBox)
            handler(this, elementBox);
    }
}
