using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Framerate : MonoBehaviour
{
    //Declare these in your class
    int m_frameCounter = 0;
    float m_timeCounter = 0.0f;
    float m_lastFramerate = 0.0f;
    public float m_refreshTime = 0.5f;
    public TextMeshProUGUI m_text;

    void Update()
    {
        if( m_timeCounter < m_refreshTime )
        {
            m_timeCounter += Time.deltaTime;
            m_frameCounter++;
        }
        else
        {
            //This code will break if you set your m_refreshTime to 0, which makes no sense.
            m_lastFramerate = (float)m_frameCounter/m_timeCounter;
            m_frameCounter = 0;
            m_timeCounter = 0.0f;
            m_text.text = "FPS: " + m_lastFramerate.ToString("F2");
        }
    }
}