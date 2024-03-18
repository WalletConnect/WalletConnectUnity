using System.Collections.Generic;

namespace WalletConnectUnity.Core
{
    public class Chain
    {
        public virtual string Name { get; }
        public virtual Currency NativeCurrency { get; }
        public virtual bool IsTestnet { get; }
        public virtual string ImageUrl { get; }

        // --- CAIP-2
        public virtual string ChainNamespace { get; }
        public virtual string ChainReference { get; }

        public virtual string ChainId => $"{ChainNamespace}:{ChainReference}";
        // ---

        public Chain(string chainNamespace, string chainReference, string name, Currency nativeCurrency, bool isTestnet, string imageUrl)
        {
            ChainNamespace = chainNamespace;
            ChainReference = chainReference;
            Name = name;
            NativeCurrency = nativeCurrency;
            IsTestnet = isTestnet;
            ImageUrl = imageUrl;
        }
    }

    public readonly struct Currency
    {
        public readonly string name;
        public readonly string symbol;
        public readonly int decimals;

        public Currency(string name, string symbol, int decimals)
        {
            this.name = name;
            this.symbol = symbol;
            this.decimals = decimals;
        }
    }

    public static class ChainConstants
    {
        internal const string ChainImageUrl = "https://api.web3modal.com/public/getAssetImage";

        public static class Namespaces
        {
            public const string Evm = "eip155";
            public const string Algorand = "algorand";
            public const string Solana = "sol";
        }

        public static class References
        {
            public const string Ethereum = "1";
            public const string EthereumGoerli = "5";
            public const string Optimism = "10";
            public const string Ronin = "2020";
            public const string Base = "8453";
            public const string BaseGoerli = "84531";
            public const string OptimismGoerli = "420";
            public const string Arbitrum = "42161";
            public const string ArbitrumRinkeby = "421611";
            public const string Celo = "42220";
            public const string CeloAlfajores = "44787";
            public const string Solana = "5eykt4UsFv8P8NJdTREpY1vzqKqZKvdp";
            public const string SolanaDevNet = "EtWTRABZaYq6iMfeYKouRu166VU2xqa1";
            public const string SolanaTestNet = "4uhcVJyU9pJkvQyS88uRDiswHXSCkY3z";

            public const string Algorand = "wGHE2Pwdvd7S12BL5FaOP20EGYesN73k";
            public const string AlgorandTestnet = "SGO1GKSzyE7IEPItTxCByw9x8FmnrCDe";
        }

        // https://specs.walletconnect.com/2.0/specs/meta-clients/web3modal/api#known-static-asset-ids
        public static Dictionary<string, string> ImageIds { get; } = new()
        {
            // Ethereum
            { References.Ethereum, "692ed6ba-e569-459a-556a-776476829e00" },
            // Ethereum Goerli
            { References.EthereumGoerli, "692ed6ba-e569-459a-556a-776476829e00" },
            // Optimism
            { References.Optimism, "ab9c186a-c52f-464b-2906-ca59d760a400" },
            // Optimism Goerli
            { References.OptimismGoerli, "ab9c186a-c52f-464b-2906-ca59d760a400" },
            // Ronin
            { References.Ronin, "b8101fc0-9c19-4b6f-ec65-f6dfff106e00" },
            // Arbitrum
            { References.Arbitrum, "600a9a04-c1b9-42ca-6785-9b4b6ff85200" },
            // Arbitrum Rinkeby
            { References.ArbitrumRinkeby, "600a9a04-c1b9-42ca-6785-9b4b6ff85200" },
            // Celo
            { References.Celo, "ab781bbc-ccc6-418d-d32d-789b15da1f00" },
            // Celo Alfajores
            { References.CeloAlfajores, "ab781bbc-ccc6-418d-d32d-789b15da1f00" },
            // Base
            { References.Base, "7289c336-3981-4081-c5f4-efc26ac64a00" },
            // Base Goerli
            { References.BaseGoerli, "7289c336-3981-4081-c5f4-efc26ac64a00" },
            // Solana
            { References.Solana, "a1b58899-f671-4276-6a5e-56ca5bd59700" },
            // Solana DevNet
            { References.SolanaDevNet, "a1b58899-f671-4276-6a5e-56ca5bd59700" },
            // Solana TestNet
            { References.SolanaTestNet, "a1b58899-f671-4276-6a5e-56ca5bd59700" }
        };

        public static class Chains
        {
            public static readonly Chain Ethereum = new(
                Namespaces.Evm,
                References.Ethereum,
                "Ethereum",
                new Currency("Ether", "ETH", 18),
                false,
                $"{ChainImageUrl}/{ImageIds[References.Ethereum]}"
            );

            public static readonly Chain EthereumGoerli = new(
                Namespaces.Evm,
                References.EthereumGoerli,
                "Ethereum Goerli",
                new Currency("Ether", "ETH", 18),
                true,
                $"{ChainImageUrl}/{ImageIds[References.EthereumGoerli]}"
            );

            public static readonly Chain Optimism = new(
                Namespaces.Evm,
                References.Optimism,
                "Optimism",
                new Currency("Ether", "ETH", 18),
                false,
                $"{ChainImageUrl}/{ImageIds[References.Optimism]}"
            );

            public static readonly Chain OptimismGoerli = new(
                Namespaces.Evm,
                References.OptimismGoerli,
                "Optimism Goerli",
                new Currency("Ether", "ETH", 18),
                true,
                $"{ChainImageUrl}/{ImageIds[References.OptimismGoerli]}"
            );

            public static readonly Chain Ronin = new(
                Namespaces.Evm,
                References.Ronin,
                "Ronin",
                new Currency("Ether", "ETH", 18),
                false,
                $"{ChainImageUrl}/{ImageIds[References.Ronin]}"
            );

            public static readonly Chain Arbitrum = new(
                Namespaces.Evm,
                References.Arbitrum,
                "Arbitrum",
                new Currency("Ether", "ETH", 18),
                false,
                $"{ChainImageUrl}/{ImageIds[References.Arbitrum]}"
            );

            public static readonly Chain ArbitrumRinkeby = new(
                Namespaces.Evm,
                References.ArbitrumRinkeby,
                "Arbitrum Rinkeby",
                new Currency("Ether", "ETH", 18),
                true,
                $"{ChainImageUrl}/{ImageIds[References.ArbitrumRinkeby]}"
            );

            public static readonly Chain Celo = new(
                Namespaces.Evm,
                References.Celo,
                "Celo",
                new Currency("Celo", "CELO", 18),
                false,
                $"{ChainImageUrl}/{ImageIds[References.Celo]}"
            );

            public static readonly Chain CeloAlfajores = new(
                Namespaces.Evm,
                References.CeloAlfajores,
                "Celo Alfajores",
                new Currency("Celo", "CELO", 18),
                true,
                $"{ChainImageUrl}/{ImageIds[References.CeloAlfajores]}"
            );

            public static readonly Chain Base = new(
                Namespaces.Evm,
                References.Base,
                "Base",
                new Currency("Ether", "ETH", 18),
                false,
                $"{ChainImageUrl}/{ImageIds[References.Base]}"
            );

            public static readonly Chain BaseGoerli = new(
                Namespaces.Evm,
                References.BaseGoerli,
                "Base Goerli",
                new Currency("Ether", "ETH", 18),
                true,
                $"{ChainImageUrl}/{ImageIds[References.BaseGoerli]}"
            );

            public static readonly Chain Solana = new(
                Namespaces.Solana,
                References.Solana,
                "Solana",
                new Currency("Sol", "SOL", 9),
                false,
                $"{ChainImageUrl}/{ImageIds[References.Solana]}"
            );

            public static readonly Chain SolanaDevNet = new(
                Namespaces.Solana,
                References.SolanaDevNet,
                "Solana DevNet",
                new Currency("Sol", "SOL", 9),
                false,
                $"{ChainImageUrl}/{ImageIds[References.SolanaDevNet]}"
            );

            public static readonly Chain SolanaTestNet = new(
                Namespaces.Solana,
                References.SolanaTestNet,
                "Solana TestNet",
                new Currency("Sol", "SOL", 9),
                true,
                $"{ChainImageUrl}/{ImageIds[References.SolanaTestNet]}"
            );

            public static readonly IReadOnlyCollection<Chain> All = new HashSet<Chain>
            {
                Ethereum,
                EthereumGoerli,
                Optimism,
                OptimismGoerli,
                Ronin,
                Arbitrum,
                ArbitrumRinkeby,
                Celo,
                CeloAlfajores,
                Base,
                BaseGoerli,
                Solana,
                SolanaDevNet,
                SolanaTestNet
            };
        }
    }
}