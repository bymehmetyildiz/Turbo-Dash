using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuManager : MonoBehaviour
{
    [SerializeField] private Image Backgroundfill;
    [SerializeField] private Button carSkillButton;
    [SerializeField] private float refillDuration;
    private float refillTime;
    private Player player;
    private bool canUseSkill;

    void Start()
    {
        Backgroundfill.fillAmount = 1f;
        player = Player.instance;
        canUseSkill = true;
    }

    
    void Update()
    {
        
    }


    public void SwitchDriveState()
    {
        if (!player.isChangingLane && player.isStarted && canUseSkill)
        {
            player.stateMachine.ChangeState(player.driveState);
            Backgroundfill.fillAmount = 0f;
            refillTime = 0f;
            canUseSkill = false;            
            carSkillButton.interactable = false;
            StartCoroutine(RefillImage());
        }
    }

    private IEnumerator RefillImage()
    {
        while (refillTime < refillDuration)
        {
            refillTime += Time.deltaTime;
            Backgroundfill.fillAmount = Mathf.Lerp(0f, 1f, refillTime / refillDuration);
            yield return null;
        }

        // Ensure it's exactly full and re-enable at the end
        Backgroundfill.fillAmount = 1f;
        canUseSkill = true;
        carSkillButton.interactable = true;
    }

}
