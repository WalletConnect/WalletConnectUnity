
# WalletConnectUnity
This project is an extension of WalletConnectSharp that brings WalletConnect to Unity. This project has been built using Unity 2019.4.28f1 (LTS), and has been tested using 2020.1.14f1. 

This project includes a working demo scene (**TODO**: needs to show sample transactions)

#### :warning: **This is beta software**: This software is currently in beta and under development. Please proceed with caution, and open a new issue if you encounter a bug :warning:

## Installation

*To use WalletConnectUnity, you would need Unity2019.4.28f1 or above.* 

After making a new project in Unity, you will need to download WalletConnectUnity from this repo by cloning it, forking it, or downloading as a zip file. Take the contents of the Assets folder in the repo and place it in your Unity Project.

Once imported in your Unity Project, create a game object in your scene named Wallet Connect and attach the Wallet Connect component to the your new game object. You can configure both the connection settings and app details in the Wallet Connect Component.

## Usage

To use WalletConnect in your Unity project, simply create an empty GameObject in your Scene and attach the `WalletConnect` component to your GameObject. Attaching this script will automatically attach any required components as well

![example](https://i.imgur.com/nlpZx5l.png)

### Options
* Default Wallet
    - The default wallet to open up on iOS when no when no wallet is specified in `OpenMobileWallet`
* Auto Save and Resume
    - Automatically saves the session when the application pauses or quits and resumes the session when the app resumes or starts.
* Connect On Awake
    - Whether you would like WalletConnect to automatically start listening for a Wallet connection on awake. The user will still need to initiate a connection either by scanning a QR code or through deep linking
* Connect On Start
    - Whether you would like WalletConnect to automatically start listening for a Wallet connection on start. The user will still need to initiate a connection either by scanning a QR code or through deep linking
* Create New Session On Session Disconnect
	- WalletConnect will create a new session every time the current session ends. 
* Connect Session Retry Count
	 - How many times a session should attempt to reconnect to the bridge server before failing.
* Custom Bridge Url
	 - Set a custom bridge to connect to. Leave this blank to use WalletConnect default bridges.
* Chain Id
	 - **Unused** will be used to specify chain to interact with.
* Connected Event
    - A Unity Event that is triggered when a Wallet session has started. Nothing is passed to this event listener.
* Connected Event Session
    - A Unity Event that is triggered when a Wallet session has started. The Session data is passed to this event listener
* Disconnected Event Session
    - A Unity Event that is triggered when a Wallet session has disconnected. The Session data is passed to this event listener.
* Connection Failed Event
    - A Unity Event that is triggered when a Wallet session has failed to connect either due to transport errors, network errors, or user declining to create session. The Session data is passed to this event listener
* New Session Connected
	 -  A Unity Event that is triggered when a new Wallet session has been successfully created and connected. The Session data is passed to this event listener. This event will only be called once in a session's lifetime.
* Resumed Session Connected
	 -  A Unity Event that is triggered when a Wallet session has been successfully resumed and connected. The Session data is passed to this event listener. 
* App Data
    - This is the Session data that is given and shown in the Wallet

## QRCode Generation

This project comes with a component to automitcally generate a QR Code image and place it onto a UI Image. To do this, simply attach the `WalletConnect QR Image` component to your `Image` and select your `WalletConnect` GameObject reference

![example2](https://i.imgur.com/vgH5Hvv.png)

## API

To access the current WalletConnect object in any Scene, you can do `WalletConnect.Instance`. This object will include a reference to the `WalletConnectProtocol` object, the Connection URL and some helper functions. For reference on how to use the `WalletConnectProtocol` object, see [WalletConnectSharp](https://github.com/WalletConnect/WalletConnectSharp) for more details. 

## NEthereum

Currently, this project **DOES NOT** include the NEthereum Extension. To add the extension, simply add the [WalletConnect.NEthereum](https://github.com/WalletConnect/WalletConnectSharp/tree/main/WalletConnectSharp.NEthereum) folder from WalletConnectSharp and then include the proper NEthereum Unity libraries in your project.

Once you have done this, to create a new NEthereum Web3 instance, you can simply do

```
var wcProtocol = WalletConnect.Instance.Protocol
var web3 = new Web3(wcProtocol.CreateProvider(new Uri("https://mainnet.infura.io/v3/<infruaId>"));
```
