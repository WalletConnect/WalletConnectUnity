
# WalletConnectUnity
This project is an extension of WalletConnectSharp that brings WalletConnect to Unity. This project has been built using Unity 2021.3.22f1 (LTS). 

This project includes a simple demo scene for using the Sign Client. Two additional demo scenes will be added for using the Auth Client
and Web3Wallet Client.

#### :warning: **This is beta software**: This software is currently in beta and under development. Please proceed with caution, and open a new issue if you encounter a bug :warning:

## Installation

*To use WalletConnectUnity, you would need Unity 2021.3 or above.* 

After making a new project in Unity, you will need to download WalletConnectUnity from this repo by cloning it, forking it, or downloading as a zip file. Take the contents of the Assets folder in the repo and place it in your Unity Project.

Once imported in your Unity Project, create a game object in your scene named Wallet Connect and attach the `WCSignClient` component to the your new game object. You can configure both the connection settings and app details in the `WalletConnectUnity` component that comes
attached with `WCSignClient`.

## Usage

To use WalletConnect in your Unity project, simply create an empty GameObject in your Scene and attach the `WCSignClient` component to your GameObject. Attaching this script will automatically attach any required components as well, such
as `WalletConnectUnity`

![example](https://i.imgur.com/g6DsfoQ.png)

### Options
* Project Name - The name of your project. This will be used inside the relay server.
* Project Id - The id of your project. This will be used inside the relay server.
* Client Metadata
  * Name - The name of your app. This will be used inside the authentication request.
  * Description - The description of your app. This will be used inside the authentication request.
  * Url - The url of your app. This will be used inside the authentication request.
  * Icons - The icons of your app. This will be used inside the authentication request.
  * Very Url - The verification URL of your app. Currently used but not enforced
* Connect On Awake - If true, the client will automatically connect to the relay server on awake.
* Connect On Start - If true, the client will automatically connect to the relay server on start.
* Base Context - The base context string to use for logging.

## API

To access the current WalletConnect object in any Scene, you can do `WCSignClient.Instance`. This object will include a reference to the `WalletConnectSignClient` object, the Connection URL and all API functions. For reference on how to use the `WalletConnectSignClient` object, see [WalletConnectSharp](https://github.com/WalletConnect/WalletConnectSharp) for more details. 

