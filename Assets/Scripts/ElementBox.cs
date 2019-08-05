using UnityEngine;

public class ElementBox : MonoBehaviour
{
    public enum Element
    {
        Void, Balance, Order, Chaos, Stone, Lightning, Ice, Fire, Water, Wind, Shadow, Light, Death, Life
    }

    public enum Category
    {
        Void, Balance, Order, Chaos
    }

    public enum Location
    {
        Void, Point, Material, Force
    }

    public enum LocationSubtype
    {
        None, Base, Transcendent
    }

    [SerializeField] public Element element;
    [SerializeField] public Category category;
    [SerializeField] public Location location;
    [SerializeField] public LocationSubtype locationSubtype;
    [SerializeField] public GameObject manifestObject;

    private delegate void ElementBoxCollisionHandler(ElementBox thisBox, ElementBox otherBox, GameObject manifestObject = null);
    private ElementBoxCollisionHandler handler;

    private static float groundScanDistance = 1f;
    private static int ElementBoxesLayer = 1 << 8;

    private static void spawnElementManifest(ElementBox thisBox, ElementBox otherBox, GameObject manifestObject)
    {
        // -- Start at average position between boxes.
        Vector3 startPosition = (thisBox.transform.position + otherBox.transform.position) / 2;
        Vector3 thisVelocity = thisBox.GetComponentInParent<Rigidbody>().velocity;
        Vector3 otherVelocity = otherBox.GetComponentInParent<Rigidbody>().velocity;

        // -- Use highest magnitude velocity to decide on direction.
        Vector3 manifestDirection = (thisVelocity.magnitude >= otherVelocity.magnitude) ? thisVelocity : otherVelocity;
        manifestDirection = Vector3.Normalize(manifestDirection);

        // -- Align along any nearby ground surfaces
        RaycastHit hit;
        if (Physics.Raycast(startPosition, Vector3.down, out hit, groundScanDistance, ~ElementBoxesLayer))
            manifestDirection = Vector3.Normalize(manifestDirection + Vector3.Reflect(manifestDirection, hit.normal));

        Quaternion manifestRotation = Quaternion.LookRotation(manifestDirection, hit.normal);
        Instantiate(manifestObject, startPosition, manifestRotation);

        Object.Destroy(otherBox.gameObject);
        Object.Destroy(thisBox.gameObject);
    }

    private static void VoidHandler(ElementBox thisBox, ElementBox otherBox, GameObject manifestObject)
    {
        if (otherBox.category != Category.Void && otherBox.category != Category.Balance)
            Object.Destroy(otherBox.gameObject);
    }

    private static void BalanceHandler(ElementBox thisBox, ElementBox otherBox, GameObject manifestObject)
    {
        // -- Balance intentionally does nothing.
    }

    private static void OrderHandler(ElementBox thisBox, ElementBox otherBox, GameObject manifestObject)
    {
        if (otherBox.element == Element.Chaos)
            Object.Destroy(otherBox.gameObject);
    }

    private static void ChaosHandler(ElementBox thisBox, ElementBox otherBox, GameObject manifestObject)
    {
        if (otherBox.element == Element.Order)
            Object.Destroy(otherBox.gameObject);
    }

    private static void StoneHandler(ElementBox thisBox, ElementBox otherBox, GameObject manifestObject) {
        if (otherBox.element == Element.Lightning)
            Object.Destroy(otherBox.gameObject);
        else if(otherBox.element == Element.Order)
            spawnElementManifest(thisBox, otherBox, manifestObject);
    }

    private static void LightningHandler(ElementBox thisBox, ElementBox otherBox, GameObject manifestObject)
    {
        if (otherBox.element == Element.Stone)
            Object.Destroy(otherBox.gameObject);
    }

    private static void IceHandler(ElementBox thisBox, ElementBox otherBox, GameObject manifestObject)
    {
        if (otherBox.element == Element.Fire)
            Object.Destroy(otherBox.gameObject);
    }

    private static void FireHandler(ElementBox thisBox, ElementBox otherBox, GameObject manifestObject)
    {
        if (otherBox.element == Element.Ice)
            Object.Destroy(otherBox.gameObject);
    }

    private static void WaterHandler(ElementBox thisBox, ElementBox otherBox, GameObject manifestObject)
    {
        if (otherBox.element == Element.Wind)
            Object.Destroy(otherBox.gameObject);
    }

    private static void WindHandler(ElementBox thisBox, ElementBox otherBox, GameObject manifestObject)
    {
        if (otherBox.element == Element.Water)
            Object.Destroy(otherBox.gameObject);
    }

    private static void ShadowHandler(ElementBox thisBox, ElementBox otherBox, GameObject manifestObject)
    {
        if (otherBox.element == Element.Light)
            Object.Destroy(otherBox.gameObject);
    }

    private static void LightHandler(ElementBox thisBox, ElementBox otherBox, GameObject manifestObject)
    {
        if (otherBox.element == Element.Shadow)
            Object.Destroy(otherBox.gameObject);
    }

    private static void DeathHandler(ElementBox thisBox, ElementBox otherBox, GameObject manifestObject)
    {
        if (otherBox.element == Element.Life)
            Object.Destroy(otherBox.gameObject);
    }

    private static void LifeHandler(ElementBox thisBox, ElementBox otherBox, GameObject manifestObject)
    {
        if (otherBox.element == Element.Death)
            Object.Destroy(otherBox.gameObject);
    }

    private void Awake()
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

    private void OnCollisionEnter(Collision collision)
    {
        ElementBox elementBox = collision.gameObject.GetComponent<ElementBox>();
        if (elementBox)
            handler(this, elementBox, manifestObject);
    }
}
