using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;

[RequireComponent(typeof(ParticleSystem))]
public class GetWeatherData : MonoBehaviour
{
    private string requestUrl = "https://maps.googleapis.com/maps/api/geocode/json";
    private string geocode_apikey = "AIzaSyB8atCJJPW1nddKKHS4XWwuHKRaHbJ-llU";
    private string wb_apikey = "8660093900b24664a3d30561337fc1de";
    private string lat;
    private string lng;
    public static string address = "Atlanta, GA";
    public string day = "0";
    public Slider slider;
    public TMP_Dropdown dropdown;
    public TextMeshProUGUI sliderText;
    public TextMeshProUGUI buttonText;
    private ParticleSystemRenderer psRenderer;
    public Vector3 newPosition;
    public float brightness = 10.0f;
    public float windStrength = 5.0f;
    public float windTurbulence = 0.5f;
    public Material partlyCloudy_Sunset;
    public Material overcast;
    public Material nightMoon;
    public Material pinkCloud_sunrise;
    public Material deepDusk;
    public Material coldSunset;
    public Material nightNoClouds;
    public Material dayNoClouds;
    public ParticleSystem ps;


    // Start is called before the first frame update
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        var emmm = ps.emission;
        emmm.rateOverTime = 0f;
        GameObject.Find("Update").GetComponent<Button>().onClick.AddListener(GetData);
    }


    public void Update()
    {
        sliderText.text = slider.value.ToString("0");
        day = slider.value.ToString("0");
    }

    public void setAddress(int val)
    {
        if (val == 0)
        {
            address = "Atlanta, GA";
        }
        else if (val == 1)
        {
            address = "New York City, NY";
        }
    }


    void GetData() => StartCoroutine(GetData_Coroutine());

        IEnumerator GetData_Coroutine()
        {
            using (UnityWebRequest request = UnityWebRequest.Get($"{requestUrl}?address={address}&key={geocode_apikey}"))
            {
                yield return request.SendWebRequest();
                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.Log(request.error);
                }
                else
                {
                    string json = request.downloadHandler.text;
                    string searchSubstring = "location";
                    string[] lines = json.Split("\n");
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].Contains(searchSubstring))
                        {
                            char[] characters_lat = lines[i + 1].ToCharArray();
                            char[] characters_lng = lines[i + 2].ToCharArray();
                            string lat_str = "";
                            string lng_str = "";
                            foreach (char c in characters_lat)
                            {
                                if (char.IsDigit(c) || c == '.' || c == '-')
                                {
                                    lat_str += c.ToString();
                                }
                            }
                            lat = lat_str;
                            foreach (char c in characters_lng)
                            {
                                if (char.IsDigit(c) || c == '.' || c == '-')
                                {
                                    lng_str += c.ToString();
                                }
                            }
                            lng = lng_str;
                            break;
                        }
                    }
                }
            }
            using (UnityWebRequest request = UnityWebRequest.Get($"https://api.weatherbit.io/v2.0/forecast/daily?lat={lat}&lon={lng}&key={wb_apikey}"))
            {
                yield return request.SendWebRequest();
                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.Log(request.error);
                }
                else
                {
                    string json = request.downloadHandler.text;
                    json_data myData = JsonUtility.FromJson<json_data>(json);
                    // add in rain logic below
                    if (Convert.ToInt32(float.Parse(myData.data[int.Parse(day)].precip)) != 0 || Convert.ToInt32(float.Parse(myData.data[int.Parse(day)].snow)) != 0)
                    {
                        ParticleSystem.CollisionModule collisionModule = ps.collision;
                        ParticleSystem.VelocityOverLifetimeModule velocityModule = ps.velocityOverLifetime;
                        ParticleSystem.MainModule mainModule = ps.main;
                        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
                        ParticleSystem.MainModule main_ss = ps.main;

                        var main = ps.main;
                        var ext = ps.externalForces;
                        var sh = ps.shape;
                        var coll = ps.collision;
                        var em = ps.emission;

                        main.duration = 10f;
                        main.startLifetime = 1f;
                        main.startDelay = 0f;
                        main.startColor = new Color(255f, 255f, 255f, 125f);
                        main.maxParticles = 100000;
                        ps.transform.position = new Vector3(0,25);
                        sh.shapeType = ParticleSystemShapeType.Box;
                        sh.scale = new Vector3(200f, 1f, 180f);
                        coll.type = ParticleSystemCollisionType.World;
                        collisionModule.enabled = true;
                        coll.bounce = 0f;
                        coll.dampen = 1f;
                        velocityModule.enabled = true;
                        velocityModule.space = ParticleSystemSimulationSpace.World;
                        velocityModule.x = new ParticleSystem.MinMaxCurve(0, 0);
                        velocityModule.y = new ParticleSystem.MinMaxCurve(-35, -25);
                        velocityModule.z = new ParticleSystem.MinMaxCurve(0, 0);
                        mainModule.startSpeed = 0f;
                        ext.enabled = true;
                        renderer.renderMode = ParticleSystemRenderMode.Stretch;
                        RenderSettings.ambientIntensity = 0f;
                        main_ss.startSize3D = true;
                        main_ss.startSizeXMultiplier = .1f;
                        main_ss.startSizeYMultiplier = 1.5f;
                        main_ss.startSizeZMultiplier = .1f;
                        em.rateOverTime = Convert.ToInt32(float.Parse(myData.data[int.Parse(day)].precip)) * 2500f;
                        if (Convert.ToInt32(float.Parse(myData.data[int.Parse(day)].uv)) > 8)
                        {
                            RenderSettings.skybox = partlyCloudy_Sunset;
                        } else
                        {
                            RenderSettings.skybox = overcast;
                        }
                        ps.Play();
                    }
                    else
                    {
                        ParticleSystem.CollisionModule collisionModule = ps.collision;
                        ParticleSystem.VelocityOverLifetimeModule velocityModule = ps.velocityOverLifetime;
                        ParticleSystem.MainModule mainModule = ps.main;
                        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
                        ParticleSystem.MainModule main_ss = ps.main;

                        var main = ps.main;
                        var ext = ps.externalForces;
                        var sh = ps.shape;
                        var coll = ps.collision;
                        var em = ps.emission;

                        main.duration = 10f;
                        main.startLifetime = 1f;
                        main.startDelay = 0f;
                        main.startColor = new Color(255f, 255f, 255f, 125f);
                        main.maxParticles = 100000;
                        ps.transform.position = newPosition;
                        sh.shapeType = ParticleSystemShapeType.Box;
                        sh.scale = new Vector3(200f, 1f, 180f);
                        coll.type = ParticleSystemCollisionType.World;
                        collisionModule.enabled = true;
                        coll.bounce = 0f;
                        coll.dampen = 1f;
                        velocityModule.enabled = true;
                        velocityModule.space = ParticleSystemSimulationSpace.World;
                        velocityModule.x = new ParticleSystem.MinMaxCurve(0, 0);
                        velocityModule.y = new ParticleSystem.MinMaxCurve(-35, -25);
                        velocityModule.z = new ParticleSystem.MinMaxCurve(0, 0);
                        mainModule.startSpeed = 0f;
                        ext.enabled = true;
                        renderer.renderMode = ParticleSystemRenderMode.Stretch;
                        RenderSettings.ambientIntensity = 0f;
                        main_ss.startSize3D = true;
                        main_ss.startSizeXMultiplier = .1f;
                        main_ss.startSizeYMultiplier = 1.5f;
                        main_ss.startSizeZMultiplier = .1f;
                        em.rateOverTime = 0f;
                        // Cloudy and sunny
                        if (Convert.ToInt32(float.Parse(myData.data[int.Parse(day)].clouds)) > 50 && Convert.ToInt32(float.Parse(myData.data[int.Parse(day)].uv)) > 5)
                        {
                            RenderSettings.skybox = partlyCloudy_Sunset;
                        } // Cloudy and not sunny
                        else if (Convert.ToInt32(float.Parse(myData.data[int.Parse(day)].clouds)) > 50 && Convert.ToInt32(float.Parse(myData.data[int.Parse(day)].uv)) < 5)
                        {
                            RenderSettings.skybox = overcast;
                        } // Not cloudy and sunny
                        else if (Convert.ToInt32(float.Parse(myData.data[int.Parse(day)].clouds)) < 50 && Convert.ToInt32(float.Parse(myData.data[int.Parse(day)].uv)) > 5)
                        {
                            RenderSettings.skybox = dayNoClouds;
                        }
                        ps.Play();
                }
                }
            }
        }
    }


[System.Serializable]
public class json_data
{
    public string city_name;
    public string country_code;
    public DailyWeatherData[] data;
    public string lat;
    public string lon;
    public string state_code;
    public string timezone;
}

[System.Serializable]
public class DailyWeatherData
{
    public string app_max_temp;
    public string app_min_temp;
    public string clouds;
    public string clouds_hi;
    public string clouds_low;
    public string clouds_mid;
    public string datetime;
    public string dewpt;
    public string high_temp;
    public string low_temp;
    public string max_dhi;
    public string max_temp;
    public string min_temp;
    public string moon_phase;
    public string moon_phase_lunation;
    public string moonrise_ts;
    public string moonset_ts;
    public string ozone;
    public string pop;
    public string precip;
    public string pres;
    public string rh;
    public string slp;
    public string snow;
    public string snow_depth;
    public string sunrise_ts;
    public string sunset_ts;
    public string temp;
    public string ts;
    public string uv;
    public string valid_date;
    public string vis;
    public string wind_cdir;
    public string wind_cdir_full;
    public string wind_dir;
    public string wind_gust_spd;
    public string wind_spd;
}