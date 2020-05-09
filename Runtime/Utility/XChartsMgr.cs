
/******************************************/
/*                                        */
/*     Copyright (c) 2018 monitor1394     */
/*     https://github.com/monitor1394     */
/*                                        */
/******************************************/

using System.Text;

using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace XCharts
{
    class XChartsVersion
    {
        public string version = "";
        public int date = 0;
        public int checkdate = 0;
        public string desc = "";
        public string homepage = "";
    }

    [ExecuteInEditMode]
    public class XChartsMgr : MonoBehaviour
    {
        public const string version = "1.4.0";
        public const int date = 20200411;

        [SerializeField] private string m_NowVersion;
        [SerializeField] private string m_NewVersion;
        private Dictionary<string, BaseChart> m_ChartDic = new Dictionary<string, BaseChart>();
        private static XChartsMgr m_XCharts;

        public static XChartsMgr Instance
        {
            get
            {
                if (m_XCharts == null)
                {
                    var go = GameObject.Find("_xcharts_");
                    if (go == null)
                    {
                        go = new GameObject();
                        go.name = "_xcharts_";
                        if (Application.isPlaying)
                        {
                            DontDestroyOnLoad(go);
                        }
                        m_XCharts = go.AddComponent<XChartsMgr>();
                    }
                    else
                    {
                        m_XCharts = go.GetComponent<XChartsMgr>();
                        if (m_XCharts == null)
                        {
                            m_XCharts = go.AddComponent<XChartsMgr>();
                        }
                    }
                    m_XCharts.m_NowVersion = version + "_" + date;
                }
                return m_XCharts;
            }
        }

        private XChartsMgr() { }

        private void Awake()
        {
            SerieLabelPool.ClearAll();
        }

        public string changeLog { get; private set; }
        public string newVersion { get { return m_NewVersion; } }
        public string nowVersion { get { return m_NowVersion; } }
        public string desc { get; private set; }
        public string homepage { get; private set; }
        public int newDate { get; private set; }
        public int newCheckDate { get; private set; }
        public bool isCheck { get; private set; }
        public bool isNetworkError { get; private set; }
        public string networkError { get; private set; }

        public bool needUpdate
        {
            get
            {
                return date < newCheckDate;
            }
        }

        public void CheckVersion()
        {
            isCheck = true;
            isNetworkError = false;
            networkError = "";
            StartCoroutine(GetVersion());
            if (date < newCheckDate)
            {
                StartCoroutine(GetChangeLog());
            }
        }

        IEnumerator GetVersion()
        {
            var url = "https://raw.githubusercontent.com/monitor1394/unity-ugui-XCharts/master/Assets/XCharts/version.json";
            var web = UnityWebRequest.Get(url);
            yield return web;
            if (IsNetworkError(web))
            {
                isNetworkError = true;
                networkError = web.error;
                m_NewVersion = "-";
            }
            else if (web.responseCode == 200)
            {
                var cv = JsonUtility.FromJson<XChartsVersion>(web.downloadHandler.text);
                m_NewVersion = cv.version + " (" + cv.date + ")";
                newDate = cv.date;
                newCheckDate = cv.checkdate;
                desc = cv.desc;
                homepage = cv.homepage;
                web.Dispose();
                isCheck = false;
            }
            else
            {
                isCheck = false;
                isNetworkError = true;
                if (web.responseCode > 0)
                    networkError = web.responseCode.ToString();
                else if (!string.IsNullOrEmpty(web.error))
                    networkError = web.error;
                else
                    networkError = "-";
                m_NewVersion = "-";
            }
        }

        IEnumerator GetChangeLog()
        {
            isCheck = true;
            var url = "https://raw.githubusercontent.com/monitor1394/unity-ugui-XCharts/master/Assets/XCharts/CHANGELOG.md";
            var web = new UnityWebRequest(url);
            yield return web;
            if (IsNetworkError(web))
            {
                Debug.LogError(web.error);
            }
            else if (web.responseCode == 200)
            {
                CheckLog(web.downloadHandler.text);
                web.Dispose();
                isCheck = false;
            }
            else
            {
                isCheck = false;
            }
        }

        private void CheckLog(string text)
        {
            StringBuilder sb = new StringBuilder();
            var temp = text.Split('\n');
            var regex = new Regex(".*(\\d{4}\\.\\d{2}\\.\\d{2}).*");
            var checkDate = XChartsMgr.date;
            foreach (var t in temp)
            {
                if (regex.IsMatch(t))
                {
                    var mat = regex.Match(t);
                    var date = mat.Groups[1].ToString().Replace(".", "");
                    int logDate;
                    if (int.TryParse(date, out logDate))
                    {
                        if (logDate >= checkDate)
                        {
                            sb.Append(t).Append("\n");
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    sb.Append(t).Append("\n");
                }
            }
            changeLog = sb.ToString();
        }

#if UNITY_5 || UNITY_2017_1
        public bool IsNetworkError(UnityWebRequest request)
        {
            return request.isError && !IsHttpError(request);
        }
#else
        public bool IsNetworkError(UnityWebRequest request)
        {
            return request.isNetworkError;
        }
#endif

#if UNITY_5
        public bool IsHttpError(UnityWebRequest request)
        {
            return request.responseCode >= 400;
        }
#else
        public bool IsHttpError(UnityWebRequest request)
        {
            return request.isHttpError;
        }
#endif

        void OnEnable()
        {
            SceneManager.sceneUnloaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene)
        {
            SerieLabelPool.ClearAll();
        }

        public void AddChart(BaseChart chart)
        {
            //TODO:
        }

        public BaseChart GetChart(string uuid)
        {
            return m_ChartDic[uuid];
        }

        public void RemoveChart(string uuid)
        {
            if (m_ChartDic.ContainsKey(uuid))
            {
                m_ChartDic.Remove(uuid);
            }
        }

        public bool ContainsChart(string uuid)
        {
            return m_ChartDic.ContainsKey(uuid);
        }
    }
}