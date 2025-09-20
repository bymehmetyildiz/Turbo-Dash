using UnityEngine;
using TMPro;
using DG.Tweening;
using Cinemachine;


public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    private Player player;
    [SerializeField] private GameObject startButton;

    [Header("Menus")]
    public RectTransform upgradeMenu;
    public RectTransform gameMenu;
    public RectTransform garageMenu;

    [Header("Upgrade Menu")]
    [SerializeField] private TMP_Text[] upgradeText;
    [SerializeField] private TMP_Text[] upgradeCostText;
    [SerializeField] private string[] upgradeNames;
    [SerializeField] private int[] upgradeCost;
    private bool isUpgradeMenuOpen = false;
    [SerializeField] private float firstPos, lastPos;

    [Header("Shop Menu")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform camLookAt;
    [SerializeField] private GameObject car;
    [SerializeField] private Transform garageCamPos;
    private bool isGarageOpen = false;
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;

    [Header("Garage Menu")]
    [SerializeField] private RectTransform carDetails;
    [SerializeField] private float carFirstPos, carLastPos;

    [Header("Gesture Menu")]
    [SerializeField] private RectTransform gestureMenu;
    [SerializeField] private float gestureFirstPos, gestureLastPos;

    [Header("Gameplay Menu")]
    public TMP_Text currentCoinText;
    public TMP_Text distanceText;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    void Start()
    {
        player = Player.instance;
        garageMenu.gameObject.SetActive(false);
        carDetails.anchoredPosition = new Vector2(carFirstPos, carDetails.anchoredPosition.y);
        currentCoinText.text = 0.ToString();
        distanceText.text = 0 + " m";
    }

    private void OnValidate()
    {
        for (int i = 0; i < upgradeText.Length; i++)
        {
            upgradeText[i].text = upgradeNames[i];
            upgradeCostText[i].text = upgradeCost[i].ToString();
        }
    }

    public void StartGame()
    {        
        if (player.isStarted == false)
            player.isStarted = true;

        if(startButton.activeSelf)
            startButton.SetActive(false);

        gameMenu.gameObject.SetActive(false);
    }

    public void OpenUpgradeMenu()
    {
        if(isUpgradeMenuOpen == false)
        {
            upgradeMenu.DOAnchorPosX(firstPos, 0.5f).SetEase(Ease.OutBack);
            isUpgradeMenuOpen = true;
            gameMenu.gameObject.SetActive(false);
        }
        else
        {
            upgradeMenu.DOAnchorPosX(lastPos, 0.5f).SetEase(Ease.InBack);
            isUpgradeMenuOpen = false;
            gameMenu.gameObject.SetActive(true);
        }

    }

    public void OpenGarage()
    {
        if (!isGarageOpen)
        {
            garageMenu.gameObject.SetActive(true);
            carDetails.DOAnchorPosX(carLastPos, 1f).SetEase(Ease.OutBack);
            // Save the original camera position & rotation
            originalCamPos = virtualCamera.transform.position;
            originalCamRot = virtualCamera.transform.rotation;

            virtualCamera.Follow = null;
            virtualCamera.LookAt = car.transform;

            // Smooth move into garage view
            virtualCamera.transform.DOMove(garageCamPos.position, 0.5f);
            virtualCamera.transform.DORotateQuaternion(garageCamPos.rotation, 0.5f);

            gameMenu.gameObject.SetActive(false);
            isGarageOpen = true;
        }
        else
        {
            virtualCamera.LookAt = camLookAt;
            carDetails.DOAnchorPosX(carFirstPos, 0.5f).SetEase(Ease.InBack);
            

            // Smooth move back to original position & rotation
            virtualCamera.transform.DOMove(originalCamPos, 0.5f);
            virtualCamera.transform.DORotateQuaternion(originalCamRot, 0.5f);

            // Delay restoring Follow until after tween
            DOVirtual.DelayedCall(0.5f, () =>
            {
                virtualCamera.Follow = player.transform;
                gameMenu.gameObject.SetActive(true);
                isGarageOpen = false;
                garageMenu.gameObject.SetActive(false);
            });
        }
    }

    public void OpenGestureMenu()
    {
        if (gestureMenu.anchoredPosition.x != gestureLastPos)
        {
            gestureMenu.DOAnchorPosX(gestureLastPos, 0.5f).SetEase(Ease.InBack);
            gameMenu.gameObject.SetActive(true);            
        }
        else
        {
            gestureMenu.DOAnchorPosX(gestureFirstPos, 0.5f).SetEase(Ease.OutBack);
            gameMenu.gameObject.SetActive(false);
        }
    }

    public void MoveCoin()
    {

    }





}

