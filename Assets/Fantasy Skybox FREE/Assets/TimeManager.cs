using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private Texture2D skyboxNight;
    [SerializeField] private Texture2D skyboxSunrise;
    [SerializeField] private Texture2D skyboxDay;
    [SerializeField] private Texture2D skyboxSunset;

    [SerializeField] private Gradient graddientNightToSunrise;
    [SerializeField] private Gradient graddientSunriseToDay;
    [SerializeField] private Gradient graddientDayToSunset;
    [SerializeField] private Gradient graddientSunsetToNight;

    [SerializeField] private Light globalLight;

    private int minutes;
    private int hours = 6;
    private int days;

    private float tempSecond;

    private Coroutine skyboxTransitionCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        // Material 인스턴스화
        if (RenderSettings.skybox != null)
        {
            Material skyboxMaterial = new Material(RenderSettings.skybox);
            RenderSettings.skybox = skyboxMaterial;
        }
    }

    public void Update()
    {
        tempSecond += Time.deltaTime * 16f;

        if (tempSecond >= 1)
        {
            minutes++;
            tempSecond = 0;

            if (minutes >= 60)
            {
                hours++;
                minutes = 0;
            }

            if (hours >= 24)
            {
                hours = 0;
                days++;
            }
        }

        // 매 프레임마다 현재 시간과 전환 조건을 확인합니다.
        // 그리고 이미 전환 코루틴이 실행 중이 아닌지 체크합니다.
        CheckForTransition();
    }

    private void CheckForTransition()
    {
        if (skyboxTransitionCoroutine != null)
        {
            return;
        }

        if (hours == 6)
        {
            skyboxTransitionCoroutine = StartCoroutine(LerpSkybox(skyboxNight, skyboxSunrise, 10f));
            StartCoroutine(LerpLight(graddientNightToSunrise, 10f));
        }
        else if (hours == 8)
        {
            skyboxTransitionCoroutine = StartCoroutine(LerpSkybox(skyboxSunrise, skyboxDay, 10f));
            StartCoroutine(LerpLight(graddientSunriseToDay, 10f));
        }
        else if (hours == 18)
        {
            skyboxTransitionCoroutine = StartCoroutine(LerpSkybox(skyboxDay, skyboxSunset, 10f));
            StartCoroutine(LerpLight(graddientDayToSunset, 10f));
        }
        else if (hours == 22)
        {
            skyboxTransitionCoroutine = StartCoroutine(LerpSkybox(skyboxSunset, skyboxNight, 10f));
            StartCoroutine(LerpLight(graddientSunsetToNight, 10f));
        }
    }

    private IEnumerator LerpSkybox(Texture2D a, Texture2D b, float time)
    {
        RenderSettings.skybox.SetTexture("_Texture2", b);
        RenderSettings.skybox.SetFloat("_Blend", 0); // 시작 블렌드 값 초기화

        for (float i = 0; i < time; i += Time.deltaTime)
        {
            float t = i / time;
            RenderSettings.skybox.SetFloat("_Blend", t);
            yield return null;
        }

        RenderSettings.skybox.SetTexture("_Texture1", b);
        RenderSettings.skybox.SetFloat("_Blend", 0);
        
        // 코루틴 완료 후 핸들을 null로 초기화
        skyboxTransitionCoroutine = null;
    }

    private IEnumerator LerpLight(Gradient lightGradient, float time)
    {
        for (float i = 0; i < time; i += Time.deltaTime)
        {
            globalLight.color = lightGradient.Evaluate(i / time);
            RenderSettings.fogColor = globalLight.color;
            yield return null;
        }
    }
}