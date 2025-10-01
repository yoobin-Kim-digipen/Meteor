using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [SerializeField] private Texture2D skyboxNight;
    [SerializeField] private Texture2D skyboxSunrise;
    [SerializeField] private Texture2D skyboxDay;
    [SerializeField] private Texture2D skyboxSunset;

    [SerializeField] private Gradient graddientNightToSunrise;
    [SerializeField] private Gradient graddientSunriseToDay;
    [SerializeField] private Gradient graddientDayToSunset;
    [SerializeField] private Gradient graddientSunsetToNight;

    [SerializeField] private Light globalLight;
    [SerializeField] private Transform MagneticFieldTransform;

    public int morningHour = 6;
    public int dayHour = 8;
    public int eveningHour = 18;
    public int nightHour = 22;

    private int minutes;
    private int hours = 6;
    private int days;

    private float tempSecond;

    private Coroutine skyboxTransitionCoroutine;

    public Vector3 shrinkSpeed = new Vector3(0.1f, 0.0f, 0.1f);

    private void Awake()
    {
        // 싱글톤 인스턴스 초기화
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 이동 시에도 유지하고 싶으면 활성화
        }
        else
        {
            Destroy(gameObject); // 이미 인스턴스가 있으면 중복 제거
        }
    }

    void Start()
    {
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

        CheckForTransition();
        MagneticfieldAdjestment();
    }

    private void CheckForTransition()
    {
        if (skyboxTransitionCoroutine != null)
        {
            return;
        }

        if (hours == morningHour)
        {
            skyboxTransitionCoroutine = StartCoroutine(LerpSkybox(skyboxNight, skyboxSunrise, 10f));
            StartCoroutine(LerpLight(graddientNightToSunrise, 10f));
        }
        else if (hours == dayHour)
        {
            skyboxTransitionCoroutine = StartCoroutine(LerpSkybox(skyboxSunrise, skyboxDay, 10f));
            StartCoroutine(LerpLight(graddientSunriseToDay, 10f));
        }
        else if (hours == eveningHour)
        {
            skyboxTransitionCoroutine = StartCoroutine(LerpSkybox(skyboxDay, skyboxSunset, 10f));
            StartCoroutine(LerpLight(graddientDayToSunset, 10f));
        }
        else if (hours == nightHour)
        {
            skyboxTransitionCoroutine = StartCoroutine(LerpSkybox(skyboxSunset, skyboxNight, 10f));
            StartCoroutine(LerpLight(graddientSunsetToNight, 10f));
        }
    }

    private IEnumerator LerpSkybox(Texture2D a, Texture2D b, float time)
    {
        RenderSettings.skybox.SetTexture("_Texture2", b);
        RenderSettings.skybox.SetFloat("_Blend", 0);

        for (float i = 0; i < time; i += Time.deltaTime)
        {
            float t = i / time;
            RenderSettings.skybox.SetFloat("_Blend", t);
            yield return null;
        }

        RenderSettings.skybox.SetTexture("_Texture1", b);
        RenderSettings.skybox.SetFloat("_Blend", 0);

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

    private void MagneticfieldAdjestment()
    {
        if ((hours >= 18 || hours < 6) && MagneticFieldTransform.localScale.x > 50)
        {
            Vector3 newScale = MagneticFieldTransform.localScale - shrinkSpeed * Time.deltaTime * 16;
            newScale.x = Mathf.Max(newScale.x, 0f);
            newScale.z = Mathf.Max(newScale.z, 0f);
            MagneticFieldTransform.localScale = newScale;
        }
    }

    public int GetHour()
    {
        return hours;
    }
}
