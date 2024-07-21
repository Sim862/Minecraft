using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndDayUI : MonoBehaviour
{
    TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = $"{GameManager.instance.day}일을 생존했습니다!!!";
    }
}
