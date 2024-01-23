using System.Collections.Generic;
using UnityEngine;

namespace WalletConnectUnity.Modal.Sample
{
    public class Chain
    {
        private static Dictionary<int, string> Eip155NetworkImageIds { get; } = new()
        {
            // Ethereum
            { 1, "692ed6ba-e569-459a-556a-776476829e00" },
            // Arbitrum
            { 42161, "600a9a04-c1b9-42ca-6785-9b4b6ff85200" },
            // Avalanche
            { 43114, "30c46e53-e989-45fb-4549-be3bd4eb3b00" },
            // Binance Smart Chain
            { 56, "93564157-2e8e-4ce7-81df-b264dbee9b00" },
            // Fantom
            { 250, "06b26297-fe0c-4733-5d6b-ffa5498aac00" },
            // Optimism
            { 10, "ab9c186a-c52f-464b-2906-ca59d760a400" },
            // Polygon
            { 137, "41d04d42-da3b-4453-8506-668cc0727900" },
            // Gnosis
            { 100, "02b53f6a-e3d4-479e-1cb4-21178987d100" },
            // EVMos
            { 9001, "f926ff41-260d-4028-635e-91913fc28e00" },
            // ZkSync
            { 324, "b310f07f-4ef7-49f3-7073-2a0a39685800" },
            // Filecoin
            { 314, "5a73b3dd-af74-424e-cae0-0de859ee9400" },
            // Iotx
            { 4689, "34e68754-e536-40da-c153-6ef2e7188a00" },
            // Metis
            { 1088, "3897a66d-40b9-4833-162f-a2c90531c900" },
            // Moonbeam
            { 1284, "161038da-44ae-4ec7-1208-0ea569454b00" },
            // Moonriver
            { 1285, "f1d73bb6-5450-4e18-38f7-fb6484264a00" },
            // Zora
            { 7777777, "845c60df-d429-4991-e687-91ae45791600" },
            // Celo
            { 42220, "ab781bbc-ccc6-418d-d32d-789b15da1f00" },
            // Base
            { 8453, "7289c336-3981-4081-c5f4-efc26ac64a00" },
            // Aurora
            { 1313161554, "3ff73439-a619-4894-9262-4470c773a100" }
        };

        public static readonly string EvmNamespace = "eip155";

        public static readonly Chain
            Ethereum = new(EvmNamespace, "1", nameof(Ethereum), new Color(0.38f, 0.49f, 0.92f));

        public static readonly Chain Optimism = new(EvmNamespace, "10", nameof(Optimism), new Color(0.91f, 0f, 0f));

        public static readonly Chain
            xDai = new(EvmNamespace, "100", nameof(xDai), new Color(0.28f, 0.66f, 0.65f));

        public static readonly Chain Polygon = new(EvmNamespace, "137", nameof(Polygon), new Color(0.51f, 0.28f, 0.9f));

        public static readonly Chain Arbitrum =
            new(EvmNamespace, "42161", nameof(Arbitrum), new Color(0.17f, 0.22f, 0.29f));

        public static readonly Chain Celo = new(EvmNamespace, "42220", nameof(Celo),
            new Color(0.21f, 0.82f, 0.5f));

        public static readonly Chain EthereumGoerli =
            new(EvmNamespace, "5", "Ethereum Goerli", new Color(0.38f, 0.49f, 0.92f), true, Eip155NetworkImageIds[1]);

        public static readonly Chain OptimismGoerli =
            new(EvmNamespace, "420", "Optimism Goerli", new Color(0.91f, 0f, 0f), true, Eip155NetworkImageIds[10]);

        public static readonly Chain ArbitrumRinkeby =
            new(EvmNamespace, "421611", "Arbitrum Rinkeby", new Color(0.17f, 0.22f, 0.29f), true,
                Eip155NetworkImageIds[42161]);

        public static readonly Chain CeloAlfajores =
            new(EvmNamespace, "44787", "Celo Alfajores", new Color(0.21f, 0.82f, 0.5f), true,
                Eip155NetworkImageIds[42220]);

        public static readonly Chain Base =
            new(EvmNamespace, "8453", "Base", new Color(0.28f, 0.39f, 0.98f), true);

        public static readonly Chain[] All =
        {
            Ethereum,
            EthereumGoerli,
            Optimism,
            OptimismGoerli,
            Polygon,
            Arbitrum,
            ArbitrumRinkeby,
            Celo,
            CeloAlfajores,
            xDai,
            Base
        };

        public Chain(string chainNamespace,
            string chainId,
            string name,
            Color primaryColor,
            bool testnet = false,
            string overrideImageId = null)
        {
            ChainNamespace = chainNamespace;
            Name = name;
            PrimaryColor = primaryColor;
            ChainId = chainId;
            IsTestnet = testnet;

            try
            {
                ImageId = overrideImageId ?? Eip155NetworkImageIds[int.Parse(chainId)];
            }
            catch (KeyNotFoundException e)
            {
                Debug.LogError($"[WalletConnectUnity] Chain image not found for chain {chainId}");
            }
        }

        public Color PrimaryColor { get; }

        public string ChainId { get; }

        public string Name { get; }

        public string IconUrl => $"https://api.web3modal.com/public/getAssetImage/{ImageId}";

        public string ChainNamespace { get; }

        public bool IsTestnet { get; }
        public string FullChainId => $"{ChainNamespace}:{ChainId}";

        public string ImageId { get; }
    }
}