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

    private delegate void ElementBoxCollisionHandler(ElementBox otherBox);
    private ElementBoxCollisionHandler handler;

    private float groundScanDistance = 1f;
    private float manifestGroundSpacing = .01f; // -- Place slightly away from floor to prevent flicker with transparent surfaces.
    private int ElementBoxesLayer = 1 << 8;

    private void spawnElementManifest(ElementBox otherBox)
    {
        // -- Start at average position between boxes.
        Vector3 startPosition = (transform.position + otherBox.transform.position) / 2;

        // -- Use highest magnitude velocity to decide on direction.
        Vector3 thisVelocity = GetComponentInParent<Rigidbody>().velocity;
        Vector3 otherVelocity = otherBox.GetComponentInParent<Rigidbody>().velocity;

        Vector3 manifestDirection = (thisVelocity.magnitude >= otherVelocity.magnitude) ? thisVelocity : otherVelocity;

        // -- Prevent manifestObject from spawning at a weird angle if ElementBoxes are falling, rising, or at different heights.
        manifestDirection.y = 0;
        manifestDirection = Vector3.Normalize(manifestDirection);

        // -- Align parallel to any nearby ground surface
        RaycastHit hit;
        Vector3 hitNormal = Vector3.up;

        if (Physics.Raycast(startPosition, Vector3.down, out hit, groundScanDistance, ~ElementBoxesLayer) && hit.normal != Vector3.up)
        {
            hitNormal = hit.normal;
            manifestDirection = Vector3.Normalize(manifestDirection + Vector3.Reflect(manifestDirection, hitNormal));
        }

        Quaternion manifestRotation = Quaternion.LookRotation(manifestDirection, hitNormal);

        // -- The center of one bottom-corner of manifestObject should start at startPosition.
        Vector3 manifestSize = manifestObject.GetComponent<Renderer>().bounds.size;
        Vector3 elementSize = GetComponentInParent<Renderer>().bounds.size;

        // -- Move away from ground the proper amount.
        startPosition += hitNormal * (manifestSize.y / 2 - elementSize.y / 2 + manifestGroundSpacing);
        // --  Move away from collision point the proper amount.
        startPosition += (manifestDirection * manifestSize.z / 2);

        Instantiate(manifestObject, startPosition, manifestRotation);

        Object.Destroy(otherBox.gameObject);
        Object.Destroy(gameObject);
    }

    private void VoidHandler(ElementBox otherBox)
    {
        if (otherBox.category != Category.Void && otherBox.category != Category.Balance)
            Object.Destroy(otherBox.gameObject);
    }

    private void BalanceHandler(ElementBox otherBox)
    {
        // -- Balance intentionally does nothing.
    }

    private void OrderHandler(ElementBox otherBox)
    {
        if (otherBox.element == Element.Chaos)
            Object.Destroy(otherBox.gameObject);
    }

    private void ChaosHandler(ElementBox otherBox)
    {
        if (otherBox.element == Element.Order)
            Object.Destroy(otherBox.gameObject);
    }

    private void StoneHandler(ElementBox otherBox)
    {
        if (otherBox.element == Element.Lightning)
            Object.Destroy(otherBox.gameObject);
        else if (otherBox.element == Element.Order)
            spawnElementManifest(otherBox);
    }

    private void LightningHandler(ElementBox otherBox)
    {
        if (otherBox.element == Element.Stone)
            Object.Destroy(otherBox.gameObject);
        else if (otherBox.element == Element.Chaos)
            spawnElementManifest(otherBox);
    }

    private void IceHandler(ElementBox otherBox)
    {
        if (otherBox.element == Element.Fire)
            Object.Destroy(otherBox.gameObject);
        else if (otherBox.element == Element.Order)
            spawnElementManifest(otherBox);
    }

    private void FireHandler(ElementBox otherBox)
    {
        if (otherBox.element == Element.Ice)
            Object.Destroy(otherBox.gameObject);
        else if (otherBox.element == Element.Chaos)
            spawnElementManifest(otherBox);
    }

    private void WaterHandler(ElementBox otherBox)
    {
        if (otherBox.element == Element.Wind)
            Object.Destroy(otherBox.gameObject);
        else if (otherBox.element == Element.Order)
            spawnElementManifest(otherBox);
    }

    private void WindHandler(ElementBox otherBox)
    {
        if (otherBox.element == Element.Water)
            Object.Destroy(otherBox.gameObject);
        else if (otherBox.element == Element.Chaos)
            spawnElementManifest(otherBox);
    }

    private void ShadowHandler(ElementBox otherBox)
    {
        if (otherBox.element == Element.Light)
            Object.Destroy(otherBox.gameObject);
        else if (otherBox.element == Element.Order)
            spawnElementManifest(otherBox);
    }

    private void LightHandler(ElementBox otherBox)
    {
        if (otherBox.element == Element.Shadow)
            Object.Destroy(otherBox.gameObject);
        else if (otherBox.element == Element.Chaos)
            spawnElementManifest(otherBox);
    }

    private void DeathHandler(ElementBox otherBox)
    {
        if (otherBox.element == Element.Life)
            Object.Destroy(otherBox.gameObject);
        else if (otherBox.element == Element.Order)
            spawnElementManifest(otherBox);
    }

    private void LifeHandler(ElementBox otherBox)
    {
        if (otherBox.element == Element.Death)
            Object.Destroy(otherBox.gameObject);
        else if (otherBox.element == Element.Chaos)
            spawnElementManifest(otherBox);
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
                // -- @Check: For now at least, leave null to trigger an error if something else is used.
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        ElementBox box = collision.gameObject.GetComponent<ElementBox>();        
        if (box)
            handler(box);
    }
}