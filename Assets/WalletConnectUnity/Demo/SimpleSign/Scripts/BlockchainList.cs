using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using WalletConnect;

namespace WalletConnectUnity.Demo.SimpleSign
{
    public class BlockchainList : MonoBehaviour
    {
        public GameObject blockchainListTransform;
        public GameObject blockchainRowPrefab;
        public Toggle testnetToggle;

        public Chain[] SelectedChains
        {
            get
            {
                return blockchainListTransform.GetComponentsInChildren<BlockchainRowItem>().Where(br => br.SelectedState)
                    .Select(br => br.Blockchain).ToArray();
            }
        }
        
        private void Start()
        {
            testnetToggle.onValueChanged.AddListener(_ => UpdateList());
            UpdateList();
        }

        private void UpdateList()
        {
            foreach (Transform child in blockchainListTransform.transform)
            {
                Destroy(child.gameObject);
            }

            var useTests = testnetToggle.isOn;

            foreach (var chain in useTests ? Chain.All.Where(c => c.IsTestnet) : Chain.All)
            {
                var row = Instantiate(blockchainRowPrefab, blockchainListTransform.transform);
                var rowData = row.GetComponent<BlockchainRowItem>();
                if (rowData == null)
                    rowData = row.AddComponent<BlockchainRowItem>();

                rowData.Blockchain = chain;
            }   
        }
    }
}