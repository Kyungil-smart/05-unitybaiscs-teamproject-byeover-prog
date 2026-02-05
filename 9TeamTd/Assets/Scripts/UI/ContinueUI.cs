using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ContinueUI : MonoBehaviour
{
    [SerializeField] GameObject continueButton;
    [SerializeField] GameObject startButton;
    [SerializeField] Button stage2_startButton;
    [SerializeField] Button stage3_startButton;
    [SerializeField] GameObject Lock1;
    [SerializeField] GameObject Lock2;

    public GameObject selectPanel;

    void Start()
    {
        bool hasSave = SaveManager.instance.LoadDataForPreview(SaveManager.instance.nowSlot);
        if (hasSave)
        {
            continueButton.SetActive(true);
            startButton.SetActive(false);

            SaveData _data = SaveManager.instance.LoadData();
            if (_data.lastOpenStageNum > 1)
            {
                stage2_startButton.interactable = true;

                Lock1.SetActive(false);

                stage3_startButton.interactable = true;

                Lock2.SetActive(false);

            }
            /*
            if (_data.lastOpenStageNum > 2)
            {

                stage3_startButton.interactable = true;

                Lock2.SetActive(false);
            }
            */
        }
    }

    public void OpenPanel()
    {
        selectPanel.SetActive(true);
        StartCoroutine(delaySetting());
    }

    IEnumerator delaySetting()
    {
        yield return new WaitForEndOfFrame();
        GameManager.Instance.SelectedBaseID.Value = 1000;
    }
}
