using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HotFixUI : Window
{
    private HotFixPanel m_Panel;
    public override void Awake(params object[] paralist)
    {
        base.Awake(paralist);
        m_Panel = GameObject.GetComponent<HotFixPanel>();
        m_Panel.m_Image.fillAmount = 0;
        m_Panel.m_progress.text = string.Format("下载中。。。{0}M/s", 0);
        HotPatchManager.Instance.ServerInfoError += ServerInfoError;
        HotPatchManager.Instance.ItemError += ItemError;
        CheckVersion();
    }

    void HotFix()
    {
        if (Application.internetReachability==NetworkReachability.NotReachable)
        {
            //提示网络错误，检测网络链接是否正常
            //GameStart.open 
            GameStart.OpenCommonConfrim("网络链接失败", () =>
            {
                Application.Quit();
            },() =>
            {
                Application.Quit();
            });
        }
        else
        {
            
        }
    }

    void CheckVersion()
    {
        HotPatchManager.Instance.CheckVersion((hot) =>
        {
            if (hot)
            {
                //提示玩家确认是偶下载
                GameStart.OpenCommonConfrim(string.Format("当前版本：{0},有{1}大小版本热更包",
                        HotPatchManager.Instance.CurVersion,HotPatchManager.Instance.LoadSunSize/1024.0f),
                    OnClickStartDownLoad, () =>
                    {
                        Application.Quit();
                    });
            }
            else
            {
                StartOnFinish();
                //进入游戏
            }
        });
    }

    private void OnClickStartDownLoad()
    {
        if (Application.platform==RuntimePlatform.IPhonePlayer|| Application.platform==RuntimePlatform.Android)
        {
            //必须是收集流量
            if (Application.internetReachability==NetworkReachability.ReachableViaCarrierDataNetwork)
            {
                StartDownLoad();
            }
        }
        else
        {
            StartDownLoad();
        }
    }

    /// <summary>
    /// 正式开始下载
    /// </summary>
    void StartDownLoad()
    {
        m_Panel.m_InfoPanel.SetActive(true);
        m_Panel.m_HottextInfo.text = HotPatchManager.Instance.CurrentPatchs.Des;
        GameStart.Instance.StartCoroutine(HotPatchManager.Instance.StartDownLoadAB(StartOnFinish));
    }

    private void StartOnFinish()
    {
        GameStart.Instance.StartCoroutine(OnFinish());
    }

    IEnumerator OnFinish()
    {
        yield return GameStart.Instance.StartCoroutine(GameStart.Instance.StartGame(m_Panel.m_Image, m_Panel.m_progress));
        UIManager.Instance.CloseWnd(this);
    }
    
    private float m_SumTime = 0;
    public override void OnUpdate()
    {
        if (HotPatchManager.Instance.m_StartDownLoad)
        {
            m_SumTime += Time.deltaTime;
            m_Panel.m_Image.fillAmount = HotPatchManager.Instance.GetProgress();
            float speed = (HotPatchManager.Instance.GetLoadSize() / 1024.0f) / m_SumTime;
            m_Panel.m_progress.text = string.Format(string.Format("{0:F}M/s", speed));
        }
    }

    private void ServerInfoError()
    {
        GameStart.OpenCommonConfrim("服务器列表获取失败",CheckVersion,Application.Quit);
    }

    private void ItemError(string all)
    {
        GameStart.OpenCommonConfrim( string.Format("资源下载失败 资源列表：{0}",all),ANewDownLoad,Application.Quit);
    }

    private void ANewDownLoad()
    {
        HotPatchManager.Instance.CheckVersion((hot) =>
        {
            if (hot)
            {
                StartDownLoad();
            }
            else
            {
                StartOnFinish();
            }
        });
    }
    public override void OnClose()
    {
        base.OnClose();
        HotPatchManager.Instance.ServerInfoError -= ServerInfoError;
        HotPatchManager.Instance.ItemError -= ItemError;
        //加载场景
        GameMapManager.Instance.LoadScene(ConStr.MENUSCENE);
    }
}
