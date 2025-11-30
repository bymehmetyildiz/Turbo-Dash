using System.Collections;
using UnityEngine;
using CrazyGames;

public class OfferManager : MonoBehaviour
{
    private Player player;

    [SerializeField] private GameObject jet, tank, plane;
    

    [Header("Offer Distances (meters)")]
    [SerializeField] private float jetInterval = 83f;
    [SerializeField] private float tankInterval = 137f;
    [SerializeField] private float planeInterval = 177f;

    private float nextJetDistance;
    private float nextTankDistance;
    private float nextPlaneDistance;

    void Start()
    {
        player = Player.instance;

        jet.SetActive(false);
        plane.SetActive(false);
        tank.SetActive(false);

        // initialize next trigger points
        nextJetDistance = jetInterval;
        nextTankDistance = tankInterval;
        nextPlaneDistance = planeInterval;
    }

    void Update()
    {
        float traveled = player.distanceTraveled;

        if (traveled >= nextJetDistance)
        {
            StartCoroutine(ActivateOffer(jet));
            nextJetDistance += jetInterval; // schedule the next trigger
        }

        if (traveled >= nextTankDistance)
        {
            StartCoroutine(ActivateOffer(tank));
            nextTankDistance += tankInterval;
        }

        if (traveled >= nextPlaneDistance)
        {
            StartCoroutine(ActivateOffer(plane));
            nextPlaneDistance += planeInterval;
        }
    }

    private IEnumerator ActivateOffer(GameObject offer)
    {
        offer.SetActive(true);       
        yield return new WaitForSeconds(5f);
        offer.SetActive(false);
    }

    // State switchers
    public void SwitchToPlaneState()
    {
        if (player.isStarted && player.stateMachine.currentstate == player.moveState)
        {
            player.stateMachine.ChangeState(player.planeState);
            plane.SetActive(false);
        }

        /*
        CrazySDK.Ad.RequestAd(
                CrazyAdType.Rewarded,
                () =>
                {
                    Debug.Log("Rewarded ad started");
                },
                (error) =>
                {
                    Debug.Log("Rewarded ad error: " + error);
                },
                () =>
                {
                    if (player.isStarted && player.stateMachine.currentstate == player.moveState)
                        player.stateMachine.ChangeState(player.planeState);
                }
        );*/
    }

    public void SwitchToJetState()
    {
        if (player.isStarted && player.stateMachine.currentstate == player.moveState)
        {
            player.stateMachine.ChangeState(player.jetState);
            AudioManager.instance.PlaySound(20);
            jet.SetActive(false);
        }

        /*
        CrazySDK.Ad.RequestAd(
                CrazyAdType.Rewarded,
                () =>
                {
                    Debug.Log("Rewarded ad started");
                },
                (error) =>
                {
                    Debug.Log("Rewarded ad error: " + error);
                },
                () =>
                {
                    Debug.Log("Rewarded COMPLETED");
                    if (player.isStarted && player.stateMachine.currentstate == player.moveState)
                    {
                        player.stateMachine.ChangeState(player.jetState);
                        AudioManager.instance.PlaySound(20);
                    }
                }
        );
        */

    }

    public void SwitchToTankState()
    {
        if (player.isStarted && player.stateMachine.currentstate == player.moveState)
        {
            player.stateMachine.ChangeState(player.tankState);
            tank.SetActive(false);
        }


        /*
        CrazySDK.Ad.RequestAd(
               CrazyAdType.Rewarded,
               () =>
               {
                   Debug.Log("Rewarded ad started");
               },
               (error) =>
               {
                   Debug.Log("Rewarded ad error: " + error);
               },
               () =>
               {
                   Debug.Log("Rewarded COMPLETED");
                   if (player.isStarted && player.stateMachine.currentstate == player.moveState)
                       player.stateMachine.ChangeState(player.tankState);
               }
       );
        */
    }
}
