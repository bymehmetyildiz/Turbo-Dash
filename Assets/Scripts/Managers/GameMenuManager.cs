using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuManager : MonoBehaviour
{
    [SerializeField] private Image Backgroundfill;
    [SerializeField] private Button carSkillButton;
    [SerializeField] private float refillDuration;
    // The car is no longer ready at second 0: the run starts with the button partly charged.
    [SerializeField] [Range(0f, 1f)] private float startCharge = 0.5f;

    private float refillTime;
    private Player player;
    private bool canUseSkill;

    void Start()
    {
        player = Player.instance;

        canUseSkill = false;
        carSkillButton.interactable = false;
        refillTime = refillDuration * startCharge;
        Backgroundfill.fillAmount = startCharge;

        StartCoroutine(RefillImage());
    }

    public void SwitchDriveState()
    {
        // Only from a normal run: not mid-jump, mid-slide or while another vehicle is active
        if (!canUseSkill || !player.isStarted || player.isChangingLane)
            return;

        if (player.stateMachine.currentstate != player.moveState)
            return;

        player.stateMachine.ChangeState(player.driveState);

        Backgroundfill.fillAmount = 0f;
        refillTime = 0f;
        canUseSkill = false;
        carSkillButton.interactable = false;

        StartCoroutine(RefillImage());
    }

    private IEnumerator RefillImage()
    {
        while (refillTime < refillDuration)
        {
            // Charge only while the run is on
            if (player.isStarted)
            {
                refillTime += Time.deltaTime;
                Backgroundfill.fillAmount = Mathf.Lerp(0f, 1f, refillTime / refillDuration);
            }
            yield return null;
        }

        // Ensure it's exactly full and re-enable at the end
        Backgroundfill.fillAmount = 1f;
        canUseSkill = true;
        carSkillButton.interactable = true;
    }
}
