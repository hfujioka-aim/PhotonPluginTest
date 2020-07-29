using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class PlayerUI: MonoBehaviour
{
    #region Private Fields

    [Tooltip("UI Text to display Player's Name")]
    [SerializeField]
    private Text playerNameText;

    [Tooltip("UI Slider to display Player's Health")]
    [SerializeField]
    private Slider playerHealthSlider;

    [SerializeField]
    private Text hitText;

    [Tooltip("Pixel offset from the player target")]
    [SerializeField]
    private Vector3 screenOffset = new Vector3(0f, 30f, 0f);

    private PlayerManager target;

    float characterControllerHeight;

    Transform targetTransform;

    Renderer targetRenderer;

    CanvasGroup _canvasGroup;

    Vector3 targetPosition;

    #endregion

    #region MonoBehaviour Callbacks

    void Awake()
    {
        this._canvasGroup = this.GetComponent<CanvasGroup>();
        var canvas = GameObject.Find("Canvas");
        if (canvas) {
            this.transform.SetParent(canvas.transform, false);
        }
    }

    void Update()
    {
        // Destroy itself if the target is null, It's a fail safe when Photon is destroying Instances of a Player over the network
        if (this.target == null) {
            Destroy(this.gameObject);
            return;
        }

        // Reflect the Player Health
        if (this.playerHealthSlider != null) {
            this.playerHealthSlider.value = target.Health;
        }
    }

    void LateUpdate()
    {
        // Do not show the UI if we are not visible to the camera, thus avoid potential bugs with seeing the UI, but not the player itself.
        if (this.targetRenderer != null) {
            this._canvasGroup.alpha = this.targetRenderer.isVisible ? 1f : 0f;
        }

        // #Critical
        // Follow the Target GameObject on screen.
        if (this.targetTransform != null) {
            this.targetPosition = this.targetTransform.position;
            this.targetPosition.y += this.characterControllerHeight;
            this.transform.position = Camera.main.WorldToScreenPoint(this.targetPosition) + this.screenOffset;
        }

        if (this.hitText != null) {
            this.hitText.enabled = this.target?.IsHit ?? false;
        }
    }

    #endregion

    #region Public Methods

    public void SetTarget(PlayerManager _target)
    {
        // Cache references for efficiency
        this.target = _target;

        this.targetTransform = _target?.transform;
        this.targetRenderer = _target?.GetComponent<Renderer>();
        var characterController = _target?.GetComponent<CharacterController>();

        this.characterControllerHeight = characterController?.height ?? 1.5f;

        if (this.playerNameText != null) {
            this.playerNameText.text = _target.photonView?.Owner?.NickName;
        }
    }

    #endregion
}
