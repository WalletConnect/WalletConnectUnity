
# WalletConnectUnity
This repository is a monorepo of packages that extend [WalletConnectSharp](https://github.com/WalletConnect/WalletConnectSharp) and brings WalletConnect to Unity.

## Packages
| Package | Description                                                                                                                                                                                                                                                                                                                                                                              | OpenUPM |
|---------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------|
| Core | High-level, Unity-friendly extension of [WalletConnectSharp](https://github.com/WalletConnect/WalletConnectSharp)<br>- Automatic active session management<br>- Option to resume session from storage<br>- Deep linking support<br>- IL2CPP support<br>- Lightweight `IJsonRpcConnection` implementation<br>- QR Code generation utility<br>- API to load wallets data and visual assets | [![openupm](https://img.shields.io/npm/v/com.walletconnect.core?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.walletconnect.core/) |
| Modal | Simplest and most minimal way to connect your players with WalletConnect                                                                                                                                                                                                                                                                                                                 | [![openupm](https://img.shields.io/npm/v/com.walletconnect.modal?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.walletconnect.modal/) |
| UI | This is a technical package that provides UI for WalletConnect Modal. It is not intended to be used directly, but rather as a dependency of WalletConnect Modal.                                                                                                                                                                                                                         | [![openupm](https://img.shields.io/npm/v/com.walletconnect.ui?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.walletconnect.ui/) |
| Nethereum |This Unity package provides a simple way to integrate WalletConnect with [Nethereum](https://nethereum.com) library.                                                                                                                                                                                                                         | [![openupm](https://img.shields.io/npm/v/com.walletconnect.nethereum?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.walletconnect.nethereum/) |

Older versions of  WalletConnectUnity are available under `legacy/*` branches

### Prerequisites
* Unity 2021.3 or above
* IL2CPP managed code stripping level: Minimal (or lower)

### Supported Platforms
* Android
* iOS
* macOS
* Windows
* WebGL ([experimental](#webgl-usage))

### Documentation
* [WalletConnect Modal](https://docs.walletconnect.com/advanced/walletconnectmodal/about?platform=unity)
* [Sign API](https://docs.walletconnect.com/api/sign/overview?platform=unity)
* [Core API](https://docs.walletconnect.com/api/core/pairing?platform=unity)

## Installation
<details>
  <summary>Install via OpenUPM CLI</summary>

To install packages via OpenUPM, you need to have [Node.js](https://nodejs.org/en/) and [openupm-cli](https://openupm.com/docs/getting-started.html#installing-openupm-cli) installed. Once you have them installed, you can run the following commands:

- **WalletConnect Modal**:
  ```bash
  openupm add com.walletconnect.modal
  ```
- **WalletConnectUnity Core**:
  ```bash
  openupm add com.walletconnect.core
  ```
</details>

<details>
  <summary>Install via Package Manager with OpenUPM</summary>

0. Open `Advanced Project Settings` from the gear ⚙ menu located at the top right of the Package Manager’s toolbar
1. Add a new scoped registry with the following details:
   - Name: `OpenUPM`
   - URL: `https://package.openupm.com`
   - Scope(s): `com.walletconnect`
2. Press plus ➕ and then `Save` buttons
3. In the Package Manager windows open the add ➕  menu from the toolbar
4. Select `Add package by name...`
5. Enter the name of the package you want to install:
   - **WalletConnectUnity Modal**: `com.walletconnect.modal`
   - **WalletConnectUnity Core**: `com.walletconnect.core`
6. Press `Add` button

</details>

<details>
  <summary>Install via Package Manager with Git URL</summary>
 
  0. Open the add ➕  menu in the Package Manager’s toolbar
  1. Select `Add package from git URL...`
  2. Enter the package URL. Note that when installing via a git URL, the package manager won't install git dependencies automatically. Follow the error messages from the console and add all necessary packages manually
     - **WalletConnectUnity Modal**: `https://github.com/WalletConnect/WalletConnectUnity.git?path=Packages/com.walletconnect.modal`
     - **WalletConnectUnity UI**: `https://github.com/WalletConnect/WalletConnectUnity.git?path=Packages/com.walletconnect.ui`
     - **WalletConnectUnity Core**: `https://github.com/WalletConnect/WalletConnectUnity.git?path=Packages/com.walletconnect.core`
  3. Press `Add` button

  It's possible to lock the version of the package by adding `#{version}` at the end of the git URL, where `#{version}` is the git tag of the version you want to use. 
  For example, to install version `1.0.0` of WalletConnectUnity Modal, use the following URL: 
  ```
  https://github.com/WalletConnect/WalletConnectUnity.git?path=Packages/com.walletconnect.modal#modal/1.0.0
  ```
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

    // Create QR code texture
    var texture = WalletConnectUnity.Core.Utils.QRCode.EncodeTexture(connectedData.Uri);
    
    // ... Display QR code texture

    // Wait for wallet approval
    await connectedData.Approval;                                                            
}                                                                                            
```

### WalletConnectProjectConfig Fields
* Id - The id of your project. This will be used inside the relay server.
* Client Metadata
  * Name - The name of your app. This will be used inside the authentication request.
  * Description - The description of your app. This will be used inside the authentication request.
  * Url - The url of your app. This will be used inside the authentication request.
  * Icons - The icons of your app. This will be used inside the authentication request.
  * Very Url - The verification URL of your app. Currently used but not enforced

### WebGL Usage
Due to WebGL's single-threaded nature, certain asynchronous operations like `Task.Run`, `Task.ContinueWith`, `Task.Delay`, and `ConfigureAwait(false)` are not natively supported. 

To enable these operations in WebGL builds, an additional third-party package, [WebGLThreadingPatcher](https://github.com/VolodymyrBS/WebGLThreadingPatcher), is required. This package modifies the Unity WebGL build to delegate work to the `SynchronizationContext`, allowing these operations to be executed on the same thread without blocking the main application. Please note that all tasks are still executed on a single thread, and any blocking calls will freeze the entire application.

The [WebGLThreadingPatcher](https://github.com/VolodymyrBS/WebGLThreadingPatcher) package can be added via git URL:
```
https://github.com/VolodymyrBS/WebGLThreadingPatcher.git
```

## Sample
* [WalletConnect Modal Sample](https://github.com/WalletConnect/WalletConnectUnity/tree/main/Packages/com.walletconnect.modal/Samples~/Modal%20Sample#readme)
