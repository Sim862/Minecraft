using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DayUI : MonoBehaviour
{
    TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "Day : " + GameManager.instance.day;
    }
}
