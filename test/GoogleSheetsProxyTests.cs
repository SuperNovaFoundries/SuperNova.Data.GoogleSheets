using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using SuperNova.Coord;
using Xunit;

namespace SuperNova.Coord.Tests
{
    public class GoogleProxyTests
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
        static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        static string ApplicationName = "Google Sheets API .NET Quickstart";

        [Fact]
        public async Task GetCorpPriceTest()
        {
            UserCredential credential = null;
            using (var stream = new FileStream("client_id.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            var ds = new GoogleSheetsProxy(credential);
            CommodityInfo info = await ds.GetCorpCommodityInfoAsync("H2O");
            Assert.Equal("H2O", info.Ticker);
        }
    }
}
