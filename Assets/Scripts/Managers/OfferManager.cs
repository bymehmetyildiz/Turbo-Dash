using System.Collections;
using UnityEngine;
using CrazyGames;

public class OfferManager : MonoBehaviour
{
    private Player player;
    [SerializeField] private GameObject jet, tank, plane;
    [SerializeField] private bool requireRewardedAdForOffers = false;

    // Offers come one at a time, in rotation, after a stretch of running on foot.
    // Time spent inside a vehicle does not count, so power-ups can no longer be chained back to back.
    [Header("Offer Timing (seconds on foot)")]
    [SerializeField] private float firstOfferDelay = 30f;
    [SerializeField] private float offerCooldown = 40f;
    [SerializeField] private float offerVisibleDuration = 5f;

    private GameObject[] offerOrder;
    private int nextOfferIndex;
    private float offerTimer;
    private Coroutine offerRoutine;

    void Start()
    {
        player = Player.instance;

        jet.SetActive(false);
        plane.SetActive(false);
        tank.SetActive(false);

        offerOrder = new[] { jet, tank, plane };
        offerTimer = firstOfferDelay;
    }

    void Update()
    {
        if (!player.isStarted || !player.IsOnFoot || offerRoutine != null)
            return;

        offerTimer -= Time.deltaTime;
        if (offerTimer > 0f)
            return;

        offerRoutine = StartCoroutine(ActivateOffer(offerOrder[nextOfferIndex]));
        nextOfferIndex = (nextOfferIndex + 1) % offerOrder.Length;
        offerTimer = offerCooldown;
    }

    private IEnumerator ActivateOffer(GameObject offer)
    {
        offer.SetActive(true);
        yield return new WaitForSeconds(offerVisibleDuration);
        offer.SetActive(false);
        offerRoutine = null;
    }

    private void HideOffers()
    {
        if (offerRoutine != null)
        {
            StopCoroutine(offerRoutine);
            offerRoutine = null;
        }

        jet.SetActive(false);
        plane.SetActive(false);
        tank.SetActive(false);
        offerTimer = offerCooldown;
    }

    // State switchers
    public void SwitchToPlaneState()
    {
        ActivateOfferPower(() => player.stateMachine.ChangeState(player.planeState));
    }

    public void SwitchToJetState()
    {
        ActivateOfferPower(() =>
        {
            player.stateMachine.ChangeState(player.jetState);
            AudioManager.instance.PlaySound(20);
        });
    }

    public void SwitchToTankState()
    {
        ActivateOfferPower(() => player.stateMachine.ChangeState(player.tankState));
    }

    private void ActivateOfferPower(System.Action activate)
    {
        if (!player.isStarted || player.stateMachine.currentstate != player.moveState)
            return;

        if (!requireRewardedAdForOffers || !CrazySDK.IsAvailable)
        {
            HideOffers();
            activate?.Invoke();
            return;
        }

        CrazySDK.Ad.RequestAd(
            CrazyAdType.Rewarded,
            () => Debug.Log("Rewarded ad started"),
            (error) => Debug.Log("Rewarded ad error: " + error),
            () =>
            {
                if (player.isStarted && player.stateMachine.currentstate == player.moveState)
                {
                    HideOffers();
                    activate?.Invoke();
                }
            }
        );
    }
}
