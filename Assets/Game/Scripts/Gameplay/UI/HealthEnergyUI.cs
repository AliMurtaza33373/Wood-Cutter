using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthEnergyUI : MonoBehaviour
{
    [SerializeField]
    private Image energyFill;


    private void Update()
    {
        energyFill.fillAmount = PlayerInteractions.Instance.CurrentPlayerEnergy / Player.MAX_ENERGY;
    }
}
