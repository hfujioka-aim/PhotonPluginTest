using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Beam : MonoBehaviour
{
    [SerializeField]
    public GameObject owner;

    private Renderer renderer;

    public bool IsHit { get; private set; }

    public bool IsActive { get; private set; }

    private void Awake()
    {
        this.renderer = this.GetComponent<Renderer>();
        this.IsActive = false;
        this.IsHit = false;
        this.renderer.material.color = this.getColor();
    }

    private void OnEnable()
    {
        this.IsActive = false;
        this.IsHit = false;
        this.renderer.material.color = this.getColor();
    }

    private void OnDisable()
    {
        this.IsActive = false;
        this.IsHit = false;
        this.renderer.material.color = this.getColor();
    }

    private Color getColor()
    {
        var color = this.IsHit ? Color.red : Color.green;
        if (!this.IsActive) {
            color.a = 0.25f;
        }
        return color;
    }

    public void SetActive(bool isActive)
    {
        this.IsActive = isActive;
        this.renderer.material.color = this.getColor();
    }

    private void localHit(bool isHit)
    {
        if (!isHit && this.IsHit) {
            this.IsHit = false;
            this.renderer.material.color = this.getColor();
        } else if (isHit && !this.IsHit) {
            this.IsHit = true;
            this.renderer.material.color = this.getColor();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || other.gameObject == this.owner) {
            return;
        }

        this.localHit(true);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player") || other.gameObject == this.owner) {
            return;
        }

        this.localHit(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player") || other.gameObject == this.owner) {
            return;
        }

        this.localHit(false);
    }
}
