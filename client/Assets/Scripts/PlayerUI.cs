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

    [Tooltip("Pixel offset from the player target")]
    [SerializeField]
    private Vector3 screenOffset = new Vector3(0f, 30f, 0f);

    private PlayerManager target;

    float characterControllerHeight = 0f;

    Transform targetTransform;

    Renderer targetRenderer;

    CanvasGroup _canvasGroup;

    Vector3 targetPosition;

    #endregion

    #region MonoBehaviour Callbacks

    void Awake()
    {
        this._canvasGroup = this.GetComponent<CanvasGroup>();
        this.transform.SetParent(GameObject.Find("Canvas").transform, false);
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
    }

    #endregion

    #region Public Methods

    public void SetTarget(PlayerManager _target)
    {
        if (_target == null) {
            Debug.LogError("<Color=Red><a>Missing</a></Color> PlayMakerManager target for PlayerUI.SetTarget.", this);
            return;
        }

        // Cache references for efficiency
        this.target = _target;

        this.targetTransform = _target.transform;
        this.targetRenderer = _target.GetComponent<Renderer>();
        var characterController = _target.GetComponent<CharacterController>();

        // Get data from the Player that won't change during the lifetime of this Component
        if (characterController != null) {
            this.characterControllerHeight = characterController.height;
        }

        if (this.playerNameText != null) {
            this.playerNameText.text = _target.photonView.Owner.NickName;
        }
    }

    #endregion
}
