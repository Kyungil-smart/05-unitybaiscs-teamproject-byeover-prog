using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyTwHelpUI : MonoBehaviour
{
    private void Update()
    {
        if (UIController.toBuyTwID.Value > 0)
        {
            gameObject.SetActive(false);
        }
    }
}
