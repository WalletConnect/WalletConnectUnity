
# WalletConnectUnity
This repository is a monorepo of packages that extend [WalletConnectSharp](https://github.com/WalletConnect/WalletConnectSharp) and brings WalletConnect to Unity.

### Packages
* **Core** - a high-level, Unity-friendly extension of [WalletConnectSharp](https://github.com/WalletConnect/WalletConnectSharp). While WalletConnectSharp can still be accessed for advanced use cases, the Core package offers several benefits.
  - Automatic active session management, option to resume session from storage
  - Deep linking support
  - IL2CPP support
  - Lightweight `IJsonRpcConnection` implementation
  - QR Code generation utility
  - API to load wallets data and visual assets
* **Modal** - a no-frills modal for wallet connections
* **UI** - collection of uGUI prefabs, sprites, and UI scripts used by the Modal

### Supported Platforms
* Unity Editor 2021.3 or above
* Android
* iOS
* macOS
* Windows
* WebGL (soon)

#### :warning: **This is beta software**: This software is currently in beta and under development. Please proceed with caution, and open a new issue if you encounter a bug. Older versions of  WalletConnectUnity are available under `legacy/*` branches :warning:

## Installation

<details open>
  <summary>Install via Git URL</summary>
 
  0. Open the add ➕  menu in the Package Manager’s toolbar
  1. Select `Add package from git URL...`
  2. Enter the package URL. Note that when installing via a git URL, the package manager won't install git dependencies automatically. Follow the error messages from the console and add all necessary packages manually
     - **WalletConnectModal**: `https://github.com/WalletConnect/WalletConnectUnity.git?path=Packages/com.walletconnect.modal`
     - **WalletConnectUnity UI**: `https://github.com/WalletConnect/WalletConnectUnity.git?path=Packages/com.walletconnect.ui`
     - **WalletConnectUnity Core**: `https://github.com/WalletConnect/WalletConnectUnity.git?path=Packages/com.walletconnect.core`
  3. Press `Add` button
</details>

<details>
  <summary>Install via OpenUPM</summary>
 
  COMING SOON
</details>

## Usage
0. Set up in  project id and metadata `WalletConnectProjectConfig` ScriptableAsset (created automatically located at `Assets/WalletConnectUnity/Resources/WalletConnectProjectConfig.asset`, do NOT move it outside of `Resources` directory).
1. Initialize `WalletConnect` and connect wallet:
```csharp
// Initialize singleton
await WalletConnect.Instance.InitializeAsync();

// Or handle instancing manually
var walletConnectUnity = new WalletConnect();
await walletConnectUnity.InitializeAsync();


// Try resume last session
var sessionResumed = await WalletConnect.Instance.TryResumeSessionAsync();              
if (!sessionResumed)                                                                         
{                                                                                            
    var connectedData = await WalletConnect.Instance.ConnectAsync(connectOptions);

    // Use connectedData.Uri to generate QR code

    // Wait for wallet approval
    await connectedData.Approval;                                                            
}                                                                                            
```

### WalletConnectProjectConfig Fields
* Project Id - The id of your project. This will be used inside the relay server.
* Client Metadata
  * Name - The name of your app. This will be used inside the authentication request.
  * Description - The description of your app. This will be used inside the authentication request.
  * Url - The url of your app. This will be used inside the authentication request.
  * Icons - The icons of your app. This will be used inside the authentication request.
  * Very Url - The verification URL of your app. Currently used but not enforced

