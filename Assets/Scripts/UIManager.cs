using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Cinemachine;

public class UIManager : MonoBehaviour
{
    private Player player;
    [SerializeField] private GameObject startButton;

    [Header("Menus")]
    [SerializeField] private RectTransform upgradeMenu;
    [SerializeField] private RectTransform gameMenu;

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


    void Start()
    {
        player = Player.instance;
    }

    private void OnValidate()
    {
        for (int i = 0; i < upgradeText.Length; i++)
        {
            upgradeText[i].text = upgradeNames[i];
            upgradeCostText[i].text = upgradeCost[i].ToString();
        }
    }

    void Update()
    {
        
    }

    public void StartGame()
    {
        
        if (player.isStarted == false)
            player.isStarted = true;

        if(startButton.activeSelf)
            startButton.SetActive(false);
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
            // Save the original camera position & rotation
            originalCamPos = virtualCamera.transform.position;
            originalCamRot = virtualCamera.transform.rotation;

            virtualCamera.Follow = null;
            virtualCamera.LookAt = car.transform;

            // Smooth move into garage view
            virtualCamera.transform.DOMove(garageCamPos.position, 0.5f);
            virtualCamera.transform.DORotateQuaternion(garageCamPos.rotation, 0.5f);

            startButton.SetActive(false);
            isGarageOpen = true;
        }
        else
        {
            virtualCamera.LookAt = camLookAt;

            // Smooth move back to original position & rotation
            virtualCamera.transform.DOMove(originalCamPos, 0.5f);
            virtualCamera.transform.DORotateQuaternion(originalCamRot, 0.5f);

            // Delay restoring Follow until after tween
            DOVirtual.DelayedCall(0.5f, () =>
            {
                virtualCamera.Follow = player.transform;
                startButton.SetActive(true);
                isGarageOpen = false;
            });
        }
    }




}
