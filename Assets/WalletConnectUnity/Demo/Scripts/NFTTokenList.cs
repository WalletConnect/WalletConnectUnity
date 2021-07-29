using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using DefaultNamespace;
using ERC721ContractLibrary.Contracts.ERC721PresetMinterPauserAutoId.ContractDefinition;
using NativeWebSocket;
using Nethereum.Web3;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using WalletConnectSharp.NEthereum;
using WalletConnectSharp.Unity;
using WalletConnectSharp.Unity.Models;

public class NFTTokenList : MonoBehaviour
{
    public WalletConnect walletConnect;
    public string[] nftTokenAddresses = new string[0];

    public GameObject tokenPrefab;

    public string infuraId = "";

    public void RebuildTokenList()
    {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        
        StartCoroutine(CoroutineRebuildTokenList(tokenPrefab, transform));
    }

    private IEnumerator CoroutineRebuildTokenList(GameObject prefab, Transform parent)
    {
        var dataTask = Task.Run(AsyncRebuildTokenList);
        
        var coroutineInstruction = new WaitForTaskResult<List<NFTTokenData>>(dataTask);
        yield return coroutineInstruction;

        var task = coroutineInstruction.Source;

        if (task.Exception != null)
        {
            throw task.Exception;
        }

        foreach (var token in task.Result)
        {
            var tokenObj = Instantiate(prefab, parent, true);

            var imageUI = tokenObj.GetComponentInChildren<Image>();
            var textUI = tokenObj.GetComponentInChildren<Text>();

            yield return token.DownloadImageSprite();

            imageUI.sprite = token.imageSprite;
            textUI.text = token.name;
        }
    }

    private IEnumerator CoroutineWebRequest<T>(string url, TaskCompletionSource<T> task)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(url);

        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            task.SetException(new IOException(uwr.error));
        }
        else
        {
            var json = uwr.downloadHandler.text;

            var result = JsonConvert.DeserializeObject<T>(json);
            
            task.SetResult(result);
        }
    }

    private async Task<T> AsyncWebRequest<T>(string url)
    {
        TaskCompletionSource<T> dataSource = new TaskCompletionSource<T>(TaskCreationOptions.None);

        MainThreadUtil.Run(delegate
        {
            StartCoroutine(CoroutineWebRequest(url, dataSource));
        });

        await dataSource.Task;

        return dataSource.Task.Result;
    }

    public async Task<List<NFTTokenData>> AsyncRebuildTokenList()
    {
        if (!walletConnect.Connected)
            return new List<NFTTokenData>();

        var provider = walletConnect.Protocol.CreateProviderWithInfura(infuraId);

        var address = walletConnect.Protocol.Accounts[0];
        var response = await WalletConnect.Instance.Protocol.EthSignTypedData(address, "This is a test message");
        
        var web3 = new Web3(provider);
        var owner = walletConnect.Protocol.Accounts[0];
        List<Task<NFTTokenData>> tokenTasks = new List<Task<NFTTokenData>>();

        foreach (var nftToken in nftTokenAddresses)
        {
            var tokenCountCall = new BalanceOfFunction()
            {
                Owner = owner
            };

            var tokenCountHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();
            var tokenCount = await tokenCountHandler.QueryAsync<BigInteger>(nftToken, tokenCountCall);

            var index = BigInteger.Zero;

            while (index.CompareTo(tokenCount) != 0)
            {
                var tokenIdCall = new TokenOfOwnerByIndexFunction()
                {
                    Owner = owner,
                    Index = index
                };

                var task = web3.Eth.GetContractQueryHandler<TokenOfOwnerByIndexFunction>()
                    .QueryAsync<BigInteger>(nftToken, tokenIdCall).ContinueWith(
                        tokenIdTask => web3.Eth.GetContractQueryHandler<TokenURIFunction>().QueryAsync<string>(nftToken,
                            new TokenURIFunction()
                            {
                                TokenId = tokenIdTask.Result
                            })).Unwrap().ContinueWith(tokenUri => AsyncWebRequest<NFTTokenData>(tokenUri.Result)).Unwrap();
                
                tokenTasks.Add(task);

                index += BigInteger.One;
            }
        }

        await Task.WhenAll(tokenTasks);

        return tokenTasks.Where(task => task.IsCompleted).Select(task => task.Result).ToList();
    }
}
