using System;
using Newtonsoft.Json;
using UnityEngine;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Unity;
using WalletConnectSharp.Unity.Utils;

[RequireComponent(typeof(WalletConnect))]
public class WalletConnectSessionSaver : BindableMonoBehavior
{
    public const string SESSION_KEY = "__walletconnect__session__";
    
    [BindComponent]
    private WalletConnect _walletConnect;

    public bool saveOnDestroy = true;
    public bool saveOnUnfocus = true;
    public bool saveOnPause = true;
    public bool resumeOnStart = true;
    public bool resumeOnEnable = true;
    public bool resumeOnFocus = true;
    public bool resumeOnUnpause = true;
    public bool saveOnApplicationQuit = true;
    public bool saveOnDisable = true;

    public void Save(bool savePlayerPrefs = true)
    {
        var session = WalletConnect.Instance.SaveSession();
        if (session == null)
        {
            Debug.Log("[WalletConnect] No session to save, skipping");
            return; //Do nothing if no active session
        }

        var json = JsonConvert.SerializeObject(session);
        PlayerPrefs.SetString(SESSION_KEY, json);

        Debug.Log("[WalletConnect] Saved session: " + json);

        if (savePlayerPrefs)
        {
            Debug.Log("[WalletConnect] Saving PlayerPrefs");
            PlayerPrefs.Save();
        }
    }

    public void TryResume()
    {
        if (PlayerPrefs.HasKey(SESSION_KEY))
        {
            var json = PlayerPrefs.GetString(SESSION_KEY);
            var session = JsonConvert.DeserializeObject<SavedSession>(json);

            if (session == null)
            {
                Debug.LogWarning("[WalletConnect] Reading session from PlayerPrefs resulted in a null session, resume canceled");
                return; //Strange, but we'll ignore it
            }

            WalletConnect.Instance.ResumeSession(session);

            Debug.Log("[WalletConnect] Resumed session " + json);
        }
    }

    public void CLearSession()
    {
        if (PlayerPrefs.HasKey(SESSION_KEY))
        {
            PlayerPrefs.DeleteKey(SESSION_KEY);
            
            PlayerPrefs.Save();

            Debug.Log("[WalletConnect] Session cleared");
        }
        else
        {
            Debug.Log("[WalletConnect] No session to clear");
        }
    }

    private void Start()
    {
        if (resumeOnStart) 
            TryResume();
    }

    private void OnEnable()
    {
        if (resumeOnEnable)
            TryResume();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && saveOnPause)
        {
            Save();
        } 
        else if (!pauseStatus && resumeOnUnpause)
        {
            TryResume();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && resumeOnFocus)
        {
            TryResume();
        }
        else if (!hasFocus && saveOnUnfocus)
        {
            Save();
        }
    }

    private void OnDestroy()
    {
        if (saveOnDestroy)
        {
            Save();
        }
    }

    private void OnApplicationQuit()
    {
        if (saveOnApplicationQuit)
        {
            Save();
        }
    }

    private void OnDisable()
    {
        if (saveOnDisable)
        {
            Save();
        }
    }
}