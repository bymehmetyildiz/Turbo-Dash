using UnityEngine;
using TMPro;
using DG.Tweening;
using Cinemachine;
using System.Collections;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    private Player player;
    [SerializeField] private GameObject startButton;

    [Header("Menus")]
    public RectTransform upgradeMenu;
    public RectTransform startMenu;
    public RectTransform gameMenu;
    public RectTransform garageMenu;

    [Header("Upgrade Menu")]
    [SerializeField] private TMP_Text[] upgradeText;
    [SerializeField] private TMP_Text[] upgradeCostText;
    [SerializeField] private string[] upgradeNames;
    [SerializeField] private int[] upgradeCost;
    private bool isUpgradeMenuOpen = false;
    [SerializeField] private float firstPos, lastPos;

    [Header("Start Menu")]
    [SerializeField] private TMP_Text totalCoinText;
    [SerializeField] private GameObject totalCoinBG;

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
    public GameObject coinImgPrefab;
    public RectTransform target;
    public GameObject coinParent;
    [SerializeField] private Image timerImg;
    private Coroutine driveCounterCoroutine;

    [Header("BestScore Menu")]
    [SerializeField] public RectTransform bestScoreMenu;
    [SerializeField] private float scoreFirstPos, scoreLastPos;
    public ScoreBoard[] scoreBoard;

    [Header("EndGame Menu")]
    [SerializeField] private TMP_Text endGameCoinText;
    [SerializeField] private TMP_Text endGameScoreText;
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private GameObject adButton;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private RectTransform coinSpawnPoint;
    [SerializeField] private RectTransform targetPoint;
    [SerializeField] private ParticleSystem confetti;

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
        gestureMenu.anchoredPosition = new Vector2(gestureFirstPos, gestureMenu.anchoredPosition.y);
        currentCoinText.text = 0.ToString();
        distanceText.text = 0 + " m";
        gameMenu.gameObject.SetActive(false);
        endGamePanel.SetActive(false);
        totalCoinBG.SetActive(true);
        UpdateTotalCoin();
        adButton.SetActive(false);
        restartButton.SetActive(false);
        timerImg.gameObject.SetActive(false);
    }

    // Update CoinText
    public void UpdateTotalCoin()
    {
        totalCoinText.text = NumberFormatter.FormatNumber(player.totalCoinAmount);
        endGameCoinText.text = NumberFormatter.FormatNumber(player.totalCoinAmount);
        currentCoinText.text = NumberFormatter.FormatNumber(player.currentCoinAmount);
    }

    public void GameOverPanel()
    {
        StartCoroutine(EndGameCoinCounter());
    }

    private IEnumerator EndGameCoinCounter()
    {
        yield return new WaitForSeconds(3f);

        endGamePanel.SetActive(true);
        UpdateTotalCoin();
        endGameScoreText.text = distanceText.text;
        yield return new WaitForSeconds(2f);

        int target = player.totalCoinAmount + player.currentCoinAmount;
        int totalToAdd = player.currentCoinAmount;
        int step = Mathf.Max(1, totalToAdd / 100); // each step adds at least 1 coin

        // How many animated coins we actually show
        int maxAnimatedCoins = 30;
        int animatedCoins = 0;

        while (player.totalCoinAmount < target)
        {
            player.totalCoinAmount += step;
            player.currentCoinAmount -= step;

            if (player.totalCoinAmount > target)
                player.totalCoinAmount = target;

            if (player.currentCoinAmount < 0)
                player.currentCoinAmount = 0;
          
            if (animatedCoins < maxAnimatedCoins)
            {
                RectTransform coin = Instantiate(coinImgPrefab, endGamePanel.transform).GetComponent<RectTransform>();
                coin.localPosition = coinSpawnPoint.localPosition;

                coin.DOMove(targetPoint.position, 0.5f)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() => Destroy(coin.gameObject));

                animatedCoins++;
                yield return new WaitForSeconds(0.05f);
            }
            else
            {         
                break;
            }

            UpdateTotalCoin();
        }
       
        player.totalCoinAmount = target;
        player.currentCoinAmount = 0;
        UpdateTotalCoin();

        currentCoinText.text = "0";

        yield return new WaitForSeconds(1f);
        confetti.Play();
        yield return new WaitForSeconds(1f);
        adButton.SetActive(true);
        yield return new WaitForSeconds(2f);
        restartButton.SetActive(true);
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

        startMenu.gameObject.SetActive(false);
        gameMenu.gameObject.SetActive(true);
        totalCoinBG.SetActive(false);
    }

    public void OpenUpgradeMenu()
    {
        if(isUpgradeMenuOpen == false)
        {
            upgradeMenu.DOAnchorPosX(firstPos, 0.5f).SetEase(Ease.OutBack);
            isUpgradeMenuOpen = true;
            startMenu.gameObject.SetActive(false);
        }
        else
        {
            upgradeMenu.DOAnchorPosX(lastPos, 0.5f).SetEase(Ease.InBack);
            isUpgradeMenuOpen = false;
            startMenu.gameObject.SetActive(true);
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

            startMenu.gameObject.SetActive(false);
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
                startMenu.gameObject.SetActive(true);
                isGarageOpen = false;
                garageMenu.gameObject.SetActive(false);
            });
        }
    }

    public void OpenGestureMenu()
    {
        if (gestureMenu.anchoredPosition.x != gestureLastPos)
        {
            gestureMenu.DOAnchorPosX(gestureLastPos, 0.5f).SetEase(Ease.OutBack);
            startMenu.gameObject.SetActive(false);            
        }
        else
        {
            gestureMenu.DOAnchorPosX(gestureFirstPos, 0.5f).SetEase(Ease.InBack);
            startMenu.gameObject.SetActive(true);
        }
    }

    public void OpenScoreMenu()
    {
        if(bestScoreMenu.anchoredPosition.x != scoreLastPos)
        {
            bestScoreMenu.DOAnchorPosX(scoreLastPos, 0.5f).SetEase(Ease.InBack);
            startMenu.gameObject.SetActive(true);            
        }
        else
        {
            bestScoreMenu.DOAnchorPosX(scoreFirstPos, 0.5f).SetEase(Ease.OutBack);
            startMenu.gameObject.SetActive(false);
        }
    }

    public void MoveCoinImg(Vector3 worldPos)
    {
        // Convert world position to screen space
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // Convert screen space to UI local position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            coinParent.GetComponent<RectTransform>(),
            screenPos,
            Camera.main,
            out Vector2 localPos
        );

        // Instantiate the coin at this position
        RectTransform coin = Instantiate(coinImgPrefab, coinParent.transform).GetComponent<RectTransform>();
        coin.localPosition = localPos;

        // Animate it to target (coin counter UI in top-left)
        coin.DOMove(target.position, 0.5f).SetEase(Ease.InQuad).OnComplete(() =>
        {
            player.currentCoinAmount++;
            currentCoinText.text = NumberFormatter.FormatNumber(player.currentCoinAmount);
            Destroy(coin.gameObject);
        });
    }


    public void UpdateScoreBoard(int newScore)
    {
        // Check if new score qualifies for the board
        for (int i = 0; i < scoreBoard.Length; i++)
        {
            if (newScore > scoreBoard[i].score)
            {
                // Shift scores down from the end to make space
                for (int j = scoreBoard.Length - 1; j > i; j--)
                {
                    scoreBoard[j].score = scoreBoard[j - 1].score;
                }

                // Insert new score at position i
                scoreBoard[i].score = newScore;
                break; // stop after inserting
            }
        }

        // Save and update UI for all entries
        foreach (var entry in scoreBoard)
        {
            entry.SaveScore();
            entry.UpdateScore();
        }
    }

    // Skill Timer

    public void StartDriveStateCounter(float _timer)
    {
        // If one is already running, stop it first
        if (driveCounterCoroutine != null)
            StopCoroutine(driveCounterCoroutine);

        driveCounterCoroutine = StartCoroutine(DriveStateCounter(_timer));
    }

    public void StopDriveStateCounter()
    {
        if (driveCounterCoroutine != null)
        {
            StopCoroutine(driveCounterCoroutine);
            driveCounterCoroutine = null;
        }

        timerImg.gameObject.SetActive(false);
        timerImg.fillAmount = 1f;
    }



    private IEnumerator DriveStateCounter(float _timer)
    {
        timerImg.gameObject.SetActive(true);
        timerImg.fillAmount = 1f;

        float totalTime = _timer;

        while (_timer > 0)
        {
            _timer -= Time.deltaTime;
            timerImg.fillAmount = _timer / totalTime;
            yield return null;
        }

        timerImg.fillAmount = 0f;
        timerImg.gameObject.SetActive(false);
        driveCounterCoroutine = null;
    }


    [ContextMenu("Delete All Scores")]
    public void DeleteAllSaves()
    {
        PlayerPrefs.DeleteAll();        
    }

}

