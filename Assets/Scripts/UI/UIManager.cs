using UnityEngine;
using TMPro;
using DG.Tweening;
using Cinemachine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CrazyGames;
using System.Runtime.InteropServices;


public class UIManager : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
[DllImport("__Internal")]
private static extern int IsMobileBrowser();
#endif

    public static UIManager instance;

    private Player player;
    [SerializeField] private GameObject startButton;

    [Header("Menus")]
    public RectTransform upgradeMenu;
    public RectTransform startMenu;
    public RectTransform gameMenu;
    public RectTransform garageMenu;
    public RectTransform pauseMenu;
    public RectTransform settingsMenu;
    public CanvasGroup FadePanel;

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
    [SerializeField] private TMP_Text endGameButtonText;
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private GameObject adButton;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private RectTransform coinSpawnPoint;
    [SerializeField] private RectTransform targetPoint;
    [SerializeField] private ParticleSystem confetti;
    private int tempCoin;

    [Header("Controls Menu")]
    public RectTransform controlsMenu;
    public GameObject mobileControls;
    public GameObject desktopControls;
    public bool isControlsShown;

    [Header("Settings Menu")]
    public RectTransform deleteSaveMenu;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    void Start()
    {
        if (CrazySDK.IsAvailable)
        {
            CrazySDK.Init(() =>
            {
                Debug.Log("CrazySDK initialized");
            });
        }


        SaveManager.instance.LoadGame();

        FadePanel.alpha = 1f;
        FadePanel.DOFade(0f, 1f);

        player = Player.instance;
        garageMenu.gameObject.SetActive(false);
        carDetails.anchoredPosition = new Vector2(carFirstPos, carDetails.anchoredPosition.y);
        gestureMenu.anchoredPosition = new Vector2(gestureFirstPos, gestureMenu.anchoredPosition.y);
        currentCoinText.text = 0.ToString();
        distanceText.text = 0 + " m";
        gameMenu.gameObject.SetActive(false);
        endGamePanel.SetActive(false);
        totalCoinBG.SetActive(true);
        adButton.SetActive(false);
        restartButton.SetActive(false);
        timerImg.gameObject.SetActive(false);
        pauseMenu.gameObject.transform.localScale = Vector3.zero;
        settingsMenu.transform.localPosition = new Vector3(0, 1000, 0);
        settingsMenu.gameObject.SetActive(false);
        controlsMenu.anchoredPosition = new Vector2(0, -1510);

        deleteSaveMenu.localScale = Vector2.zero;

        isControlsShown = PlayerPrefs.GetInt("IsControlShown", 0) == 1;

        if (!isControlsShown)
        {
            OpenControls();                // show controls only once
            MarkControlsAsShown();         // mark them as shown so they won't open again automatically
        }
        // Detect device type (mobile or desktop)
        tempCoin = 0;

        UpdateTotalCoin();
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

    public void DoubleCoins()
    {
        CrazySDK.Ad.RequestAd(CrazyAdType.Rewarded, () =>
        {
            // ad started
        }, (error) =>
        {
            // ad error
        }, () =>
        {
            player.currentCoinAmount = tempCoin;
            StartCoroutine(DoubleCoinsRoutine());
        });
    }

    private IEnumerator DoubleCoinsRoutine()
    {
        adButton.SetActive(false);
        restartButton.SetActive(false);
        player.currentCoinAmount = tempCoin;
        UpdateTotalCoin();
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
                AudioManager.instance.PlaySound(16);
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
        yield return new WaitForSeconds(2f);
        RestartGame();
    }


    private IEnumerator EndGameCoinCounter()
    {
        yield return new WaitForSeconds(3f);
        tempCoin = player.currentCoinAmount;
        endGamePanel.SetActive(true);
        endGamePanel.SetActive(true);
        UpdateTotalCoin();
        endGameScoreText.text = distanceText.text;
        yield return new WaitForSeconds(1f);

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
                AudioManager.instance.PlaySound(16);
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

        yield return new WaitForSeconds(0.5f);      
        CrazySDK.Game.HappyTime();
        AudioManager.instance.PlaySound(23);
        //yield return new WaitForSeconds(0.5f);
        //if (tempCoin > 0)
        //{
        //    adButton.SetActive(true);
        //    yield return new WaitForSeconds(0.5f);
        //    restartButton.SetActive(true);
        //    endGameButtonText.text = "No Thanks";
        //}
        //else
        //{
            adButton.SetActive(false);            
            restartButton.SetActive(true);
            endGameButtonText.text = "Return";
        //}

            
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

        CrazySDK.Game.GameplayStart();
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
            bestScoreMenu
                .DOAnchorPosX(scoreLastPos, 0.5f)
                .SetEase(Ease.InBack);

            startMenu.gameObject.SetActive(true);            
        }
        else
        {
            bestScoreMenu
                .DOAnchorPosX(scoreFirstPos, 0.5f)
                .SetEase(Ease.OutBack);

            startMenu.gameObject.SetActive(false);
        }
    }

    public void OpenPauseMenu()
    {
        if (pauseMenu.localScale == Vector3.zero)
        {
            pauseMenu.DOScale(Vector3.one, 0.5f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
            CrazySDK.Game.GameplayStop();

            Time.timeScale = 0f;
        }
        else
        {
            pauseMenu.DOScale(Vector3.zero, 0.5f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    Time.timeScale = 1f;
                    CrazySDK.Game.GameplayStart();
                });
        }
    }

    public void RestartGame()
    {
        FadePanel.DOFade(1f, 1f)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                //int chance = Random.Range(0, 100); // 0-99
                //bool showAd = (chance < 50);       // 50% chance

                //if (showAd)
                //{
                //    CrazySDK.Ad.RequestAd(CrazyAdType.Midgame, () =>
                //    {
                //        // Ad started
                //    },
                //    (error) =>
                //    {
                //        SaveManager.instance.SaveGame();
                //        Time.timeScale = 1f;
                //        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                //    },
                //    () =>
                //    {
                //        SaveManager.instance.SaveGame();
                //        Time.timeScale = 1f;
                //        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                //    });
                //}
                //else
                //{
                    // No ad
                    SaveManager.instance.SaveGame();
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                //}
            });
    }

    public void OpenSettings()
    {
        if (settingsMenu.anchoredPosition.y != 0)
        {
            settingsMenu.DOAnchorPosY(0, 0.5f)
                .SetUpdate(true)
                .SetEase(Ease.OutBack);
            settingsMenu.gameObject.SetActive(true);
        }
        else
        {
            settingsMenu.DOAnchorPosY(1000, 0.5f)
                .SetUpdate(true)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    settingsMenu.gameObject.SetActive(false);
                });
        }
    }

    public void DeletePanel()
    {
        if(deleteSaveMenu.localScale == Vector3.zero)
        {
            deleteSaveMenu.DOScale(Vector3.one, 0.5f)
                .SetUpdate(true)
                .SetEase(Ease.OutBack);
        }
        else
        {
            deleteSaveMenu.DOScale(Vector3.zero, 0.5f)
                .SetUpdate(true)
                .SetEase(Ease.InBack);
        }
    }


    public void OpenControls()
    {
        bool isMobile = false;

#if UNITY_WEBGL && !UNITY_EDITOR
isMobile = IsMobileBrowser() == 1;
#else
        isMobile = (UnityEngine.SystemInfo.deviceType == DeviceType.Handheld);

#endif

        mobileControls.SetActive(isMobile);
        desktopControls.SetActive(!isMobile);

        if (controlsMenu.anchoredPosition.y != 0)
        {
            controlsMenu.DOAnchorPosY(0, 0.5f)
                .SetUpdate(true)
                .SetEase(Ease.OutBack);
        }
        else
        {
            controlsMenu.DOAnchorPosY(-1510, 0.5f)
               .SetUpdate(true)
               .SetEase(Ease.InBack);
        }
    }

    public void MarkControlsAsShown()
    {
        isControlsShown = true;
        PlayerPrefs.SetInt("IsControlShown", 1);
        PlayerPrefs.Save();
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
            int reward = player.GetCoinRewardAmount();
            player.currentCoinAmount += reward;
            currentCoinText.text = NumberFormatter.FormatNumber(player.currentCoinAmount);

            if (player.combo >= 8)
                currentCoinText.text += "  x" + reward;

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
        FadePanel.DOFade(1f, 1f)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                Time.timeScale = 1f;
                PlayerPrefs.DeleteAll();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });
    }
}

