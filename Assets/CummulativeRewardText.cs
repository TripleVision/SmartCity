using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CummulativeRewardText : MonoBehaviour
{
    TextMeshProUGUI tmp;
    string exampleText;
    private void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        exampleText = tmp.text;
    }

    private void Update()
    {
        tmp.text = String.Format(exampleText, Globals.CummulativeReward, "0.00");
    }
}
