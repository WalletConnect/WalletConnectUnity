using Common.Logging;
using Nethereum.JsonRpc.Client.RpcMessages;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.JsonRpc.Client;

namespace WalletConnectSharp.NEthereum.Client.Shims
{
  public class RpcClient : ClientBase
  {
    private const int NUMBER_OF_SECONDS_TO_RECREATE_HTTP_CLIENT = 60;
    private readonly AuthenticationHeaderValue _authHeaderValue;
    private readonly Uri _baseUrl;
    private readonly HttpClientHandler _httpClientHandler;
    private readonly ILog _log;
    private readonly JsonSerializerSettings _jsonSerializerSettings;
    private volatile bool _firstHttpClient;
    private HttpClient _httpClient;
    private HttpClient _httpClient2;
    private bool _rotateHttpClients = true;
    private DateTime _httpClientLastCreatedAt;
    private readonly object _lockObject = new object();

    public static int MaximumConnectionsPerServer { get; set; } = 20;

    public RpcClient(
      Uri baseUrl,
      AuthenticationHeaderValue authHeaderValue = null,
      JsonSerializerSettings jsonSerializerSettings = null,
      HttpClientHandler httpClientHandler = null,
      ILog log = null)
    {
      this._baseUrl = baseUrl;
      if (authHeaderValue == null)
        authHeaderValue = UserAuthentication.FromUri(baseUrl)?.GetBasicAuthenticationHeaderValue();
      this._authHeaderValue = authHeaderValue;
      if (jsonSerializerSettings == null)
        jsonSerializerSettings = BuildDefaultJsonSerializerSettings();
      this._jsonSerializerSettings = jsonSerializerSettings;
      this._httpClientHandler = httpClientHandler;
      this._log = log;
      this.CreateNewRotatedHttpClient();
    }

    private static HttpMessageHandler GetDefaultHandler()
    {
      try
      {
        return (HttpMessageHandler) null;
      }
      catch
      {
        return (HttpMessageHandler) null;
      }
    }
    
    public static JsonSerializerSettings BuildDefaultJsonSerializerSettings()
    {
      JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
      serializerSettings.NullValueHandling = 0;
      serializerSettings.MissingMemberHandling = (MissingMemberHandling)1;
      return serializerSettings;
    }

    public RpcClient(
      Uri baseUrl,
      HttpClient httpClient,
      AuthenticationHeaderValue authHeaderValue = null,
      JsonSerializerSettings jsonSerializerSettings = null,
      ILog log = null)
    {
      this._baseUrl = baseUrl;
      if (authHeaderValue == null)
        authHeaderValue = UserAuthentication.FromUri(baseUrl)?.GetBasicAuthenticationHeaderValue();
      this._authHeaderValue = authHeaderValue;
      if (jsonSerializerSettings == null)
        jsonSerializerSettings = BuildDefaultJsonSerializerSettings();
      this._jsonSerializerSettings = jsonSerializerSettings;
      this._log = log;
      this.InitialiseHttpClient(httpClient);
      this._httpClient = httpClient;
      this._rotateHttpClients = false;
    }

    protected override async Task<RpcResponseMessage> SendAsync(
      RpcRequestMessage request,
      string route = null)
    {
      RpcLogger logger = new RpcLogger(this._log);
      RpcResponseMessage rpcResponseMessage;
      try
      {
        HttpClient httpClient = this.GetOrCreateHttpClient();
        string str = JsonConvert.SerializeObject((object) request, this._jsonSerializerSettings);
        StringContent stringContent1 = new StringContent(str, Encoding.UTF8, "application/json");
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(ClientBase.ConnectionTimeout);
        logger.LogRequest(str);
        string requestUri = route;
        StringContent stringContent2 = stringContent1;
        CancellationToken token = cancellationTokenSource.Token;
        HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(requestUri, (HttpContent) stringContent2, token).ConfigureAwait(false);
        httpResponseMessage.EnsureSuccessStatusCode();
        using (StreamReader streamReader = new StreamReader(await httpResponseMessage.Content.ReadAsStreamAsync()))
        {
          using (JsonTextReader jsonTextReader = new JsonTextReader((TextReader) streamReader))
          {
            RpcResponseMessage responseMessage = JsonSerializer.Create(this._jsonSerializerSettings).Deserialize<RpcResponseMessage>((JsonReader) jsonTextReader);
            logger.LogResponse(responseMessage);
            rpcResponseMessage = responseMessage;
          }
        }
      }
      catch (TaskCanceledException ex)
      {
        RpcClientTimeoutException timeoutException = new RpcClientTimeoutException(string.Format("Rpc timeout after {0} milliseconds", (object) ClientBase.ConnectionTimeout.TotalMilliseconds), (Exception) ex);
        logger.LogException((Exception) timeoutException);
        throw timeoutException;
      }
      catch (Exception ex)
      {
        RpcClientUnknownException unknownException = new RpcClientUnknownException("Error occurred when trying to send rpc requests(s)", ex);
        logger.LogException((Exception) unknownException);
        throw unknownException;
      }
      logger = (RpcLogger) null;
      return rpcResponseMessage;
    }

    private HttpClient GetOrCreateHttpClient()
    {
      if (!this._rotateHttpClients)
        return this.GetClient();
      lock (this._lockObject)
      {
        if ((DateTime.UtcNow - this._httpClientLastCreatedAt).TotalSeconds > 60.0)
          this.CreateNewRotatedHttpClient();
        return this.GetClient();
      }
    }

    private HttpClient GetClient()
    {
      if (!this._rotateHttpClients)
        return this._httpClient;
      lock (this._lockObject)
        return this._firstHttpClient ? this._httpClient : this._httpClient2;
    }

    private void CreateNewRotatedHttpClient()
    {
      HttpClient newHttpClient = this.CreateNewHttpClient();
      this._httpClientLastCreatedAt = DateTime.UtcNow;
      if (this._firstHttpClient)
      {
        lock (this._lockObject)
        {
          this._firstHttpClient = false;
          this._httpClient2 = newHttpClient;
        }
      }
      else
      {
        lock (this._lockObject)
        {
          this._firstHttpClient = true;
          this._httpClient = newHttpClient;
        }
      }
    }

    private HttpClient CreateNewHttpClient()
    {
      HttpClient httpClient = new HttpClient();
      if (this._httpClientHandler != null)
      {
        httpClient = new HttpClient((HttpMessageHandler) this._httpClientHandler);
      }
      else
      {
        HttpMessageHandler defaultHandler = RpcClient.GetDefaultHandler();
        if (defaultHandler != null)
          httpClient = new HttpClient(defaultHandler);
      }
      this.InitialiseHttpClient(httpClient);
      return httpClient;
    }

    private void InitialiseHttpClient(HttpClient httpClient)
    {
      httpClient.DefaultRequestHeaders.Authorization = this._authHeaderValue;
      httpClient.BaseAddress = this._baseUrl;
    }
  }
}