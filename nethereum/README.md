# WalletConnectUnity
This project is an extension of WalletConnectSharp that brings WalletConnect to Unity. This project has been built using Unity 2019.4.28f1 (LTS), and has been tested using 2020.1.14f1. 

This project includes a working demo scene (**TODO**: needs to show sample transactions)

#### :warning: **This is beta software**: This software is currently in beta and under development. Please proceed with caution, and open a new issue if you encounter a bug :warning:

## Usage

To use WalletConnect in your Unity project, simply create an empty GameObject in your Scene and attach the `WalletConnect` component to your GameObject. Attaching this script will automatically attach any required components as well

![example](https://i.imgur.com/nlpZx5l.png)

### Options

* Persist Through Scenes
    - This makes the WalletConnect GameObject persist through Scene changes. This is the same as calling `DontDestroyOnLoad`
* Wait For Wallet On Start
    - Whether you would like WalletConnect to automatically start listening for a Wallet connection on start. The user will still need to initiate a connection either by scanning a QR code or through deep linking
* ConnectedEvent
    - A Unity Event that is triggered when a Wallet seession has started. Nothing is passed to this event listener
* ConnectedEventSession
    - A Unity Event that is triggered when a Wallet session has started. The Session data is passed to this event listener
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
