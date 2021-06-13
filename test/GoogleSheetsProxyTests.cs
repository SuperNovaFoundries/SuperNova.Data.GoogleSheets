using System.Composition;
using System.Threading.Tasks;
using SuperNova.Data.GoogleSheets.Contract;
using SuperNova.MEF.NetCore;
using Xunit;

namespace SuperNova.Data.GoogleSheets.Tests
{
    public class GoogleProxyTests
    {
        [Import] private IGoogleSheetsProxy _proxy { get; set; } = null;

        //apiKey removed for security. If a valid key is added here, these tests passes.
        static string apiKey = "";
        private string snfCorpSheet = "1tyYLfgAqD7Mm1Lv8-fc59RuPdPZ_pa0HYjY7TVI_KKo";



        [Fact]
        public async Task GetCorpPriceTest()
        {
            #region not in use
            //UserCredential credential = null;
            //using (var stream = new FileStream("client_id.json", FileMode.Open, FileAccess.Read))
            //{
            //    // The file token.json stores the user's access and refresh tokens, and is created
            //    // automatically when the authorization flow completes for the first time.
            //    string credPath = "token.json";
            //    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            //        GoogleClientSecrets.Load(stream).Secrets,
            //        Scopes,
            //        "user",
            //        CancellationToken.None,
            //        new FileDataStore(credPath, true)).Result;
            //    Console.WriteLine("Credential file saved to: " + credPath);

            //    await credential.RefreshTokenAsync(CancellationToken.None);
            //} 
            #endregion

            var ds = new GoogleSheetsProxy()
            {
                ApiKey = apiKey
            };
            CommodityInfo info = await ds.GetCorpCommodityInfoAsync(snfCorpSheet, "H2O");
            Assert.Equal("H2O", info.Ticker);
        }
        [Fact]
        public void ImportTest()
        {
            MEFLoader.SatisfyImportsOnce(this);
            Assert.NotNull(_proxy);
        }

    }
}
