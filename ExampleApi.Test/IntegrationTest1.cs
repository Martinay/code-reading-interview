using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Reflection;
using System.Net;
using System.IO.Compression;
using Xunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExampleApi;
using ExampleApi.Models;
using Microsoft.Extensions.DependencyInjection;
using Code_reading;

namespace Example.Api.IntegrationTests.Controlers;


public sealed class V0RestorativeOrderTest : IDisposable
{
    private HttpClient _httpClient;
    private WebApplicationFactory<Program> _webAppFactory;

    public V0RestorativeOrderTest()
    {
        _webAppFactory = new WebApplicationFactory<Program>();

        _httpClient = _webAppFactory.CreateDefaultClient();

    }

    [Fact]
    public async void GetDownloadLink_ShouldReturns200()
    {
		//Arrange
		string token;

		using (var scope = _webAppFactory.Services.CreateScope())
        {
            using (CancellationTokenSource source = new CancellationTokenSource())
            {
                var identityClientService = scope.ServiceProvider.GetRequiredService<IIdentityClient>();
                CancellationToken cancellationToken = source.Token;
                token = await identityClientService.GetServiceBearerToken(cancellationToken);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            }
        }
		int headerId = 0;
		var assembly = Assembly.GetExecutingAssembly();
		var resourceName = "Code_reading.Rx.json";

		using var stream = assembly.GetManifestResourceStream(resourceName);
		if (stream == null)
		{
			throw new ArgumentException("stream should not be null");
		}
		using var reader = new StreamReader(stream);
		var resultJson = reader.ReadToEnd();

		var rxObject = JsonConvert.DeserializeObject<RxModel>(resultJson);
		rxObject.ID = Guid.NewGuid();
		rxObject.ScanDate = DateTime.Now;
		rxObject.DueDate = DateTime.Now.AddDays(30);

		var rxJson = JsonConvert.SerializeObject(rxObject);

		var data = new RxGenRequest()
		{
			CaseType = 1,
			CompanyId = rxObject.CompanyID,
			ContactId = rxObject.ContactID,
			DoctorNotesString = JsonConvert.SerializeObject(new string[]
			{
				"Tooth 1, good",
				"Tooth 2, bridge"
			}),
			RxJson = rxJson,
			PatientFirstName = "John",
			PatientLastName = "Doe"
		};

		using (var httpClient = new HttpClient())
		{
			httpClient.Timeout = TimeSpan.FromSeconds(300);
			httpClient.DefaultRequestHeaders.Clear();
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var rxGenUrl = new Uri("http://rx-generation-service.cloud/create/new");
			using var strContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

			var rxGenResponse = await httpClient.PostAsync(rxGenUrl, strContent);

			var responseString = await rxGenResponse.Content.ReadAsStringAsync();

			var responseObject = JsonConvert.DeserializeObject<RxGenServiceResponse>(responseString);

			if (responseObject.CaseInfo == null)
			{
				throw new ArgumentException("CaseInfo should not be null");
			}
			headerId = responseObject.CaseInfo.Value;

			Console.WriteLine(headerId);

			rxGenResponse.EnsureSuccessStatusCode();
		}

		var downloadRequest = new DownloadRequest
		{
			CompanyID = rxObject.CompanyID,
			RxID = headerId,
			ContactID = rxObject.ContactID,
			ShouldAnonymize = true
		};
		_httpClient.BaseAddress = new Uri("https://deployed-servicce-to-test.cloud/");
		var endpointUrl = new Uri("/v1.0/Download", UriKind.Relative);
		await Task.Delay(TimeSpan.FromSeconds(120));
		//Act

		using (var httpClient = new HttpClient())
		{
			httpClient.BaseAddress = _httpClient.BaseAddress;
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(downloadRequest));
			var downloadResponse = await httpClient.PostAsync(endpointUrl, jsonContent);
			using (var fileStream = File.Create(headerId + ".zip"))
			{
				var zipStream = await downloadResponse.Content.ReadAsStreamAsync();
				zipStream.Seek(0, SeekOrigin.Begin);
				zipStream.CopyTo(fileStream);
			}
			Xunit.Assert.Equal(HttpStatusCode.OK, downloadResponse.StatusCode);
		}

		//Assert

		await validateZipContent(headerId);
	}

	public static async Task validateZipContent(int orderId)
	{
		await Task.Delay(TimeSpan.FromSeconds(30));
		var headerId = "" + orderId;
		var zipPath = headerId + ".zip";
		var extractPath = headerId + "/";
		bool pdfPresent = false, plyPresent = false, htmlPresent = false, pngPresent = false, xmlPresent = false;


		// Normalizes the path.
		zipPath = Path.GetFullPath(zipPath);
		using (var archive = ZipFile.OpenRead(zipPath))
		{
			foreach (var entry in archive.Entries)
			{
				switch (entry)
				{
					case var temp when entry.FullName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase):
						StringAssert.Contains(entry.FullName, headerId, StringComparison.Ordinal);
						StringAssert.Contains(entry.FullName, "Rx", StringComparison.Ordinal);
						pdfPresent = true;
						break;
					case var temp when entry.FullName.EndsWith(".ply", StringComparison.OrdinalIgnoreCase):
						StringAssert.Contains(entry.FullName, headerId, StringComparison.Ordinal);
						plyPresent = true;
						break;
					case var temp when entry.FullName.EndsWith(".html", StringComparison.OrdinalIgnoreCase):
						StringAssert.Contains(entry.FullName, headerId, StringComparison.Ordinal);
						StringAssert.Contains(entry.FullName, "Rx", StringComparison.Ordinal);
						htmlPresent = true;
						break;
					case var temp when entry.FullName.EndsWith(".png", StringComparison.OrdinalIgnoreCase):
						StringAssert.Contains(entry.FullName, "logo", StringComparison.Ordinal);
						pngPresent = true;
						break;
					case var temp when entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase):
						StringAssert.Contains(entry.FullName, headerId, StringComparison.Ordinal);
						StringAssert.Contains(entry.FullName, "data", StringComparison.Ordinal);
						xmlPresent = true;
						break;
					default:
						break;
				}
			}
			Console.WriteLine("pdfPresent" + pdfPresent + "plyPresent" + plyPresent + "htmlPresent" + htmlPresent + "pngPresent" + pngPresent + "xmlPresent" + xmlPresent);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(pdfPresent);
			Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(plyPresent);
			Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(htmlPresent);
			Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(pngPresent);
			Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(xmlPresent);
		}
	}

	public async void Dispose()
    {
        _httpClient.Dispose();
        await _webAppFactory.DisposeAsync();
    }
}