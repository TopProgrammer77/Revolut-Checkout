using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RestSharp;
using RevolutIntegration.Models;
using System.Diagnostics;

namespace RevolutIntegration.Controllers
{
	public class ShopController : Controller
	{
		private readonly ILogger<ShopController> _logger;
        private readonly IConfiguration _configuration;

        public ShopController(IConfiguration configuration, ILogger<ShopController> logger)
		{
			_logger = logger;
			_configuration = configuration;
		}

        public IActionResult Index()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		[HttpPost]
		public async Task<IActionResult> Order([Bind("Description, Price, Currency")] OrderRequest order)
		{
			var mode = _configuration["Mode"]; // "prod", "sandbox"
			var options = new RestClientOptions
			{
				MaxTimeout = -1,
				BaseUrl = new Uri((mode == "sandbox") ? "https://sandbox-merchant.revolut.com" : "https://merchant.revolut.com")
			};
			var client = new RestClient(options);
			var request = new RestRequest("/api/orders", Method.Post);
			request.AddHeader("Content-Type", "application/json");
			request.AddHeader("Accept", "application/json");
			request.AddHeader("Revolut-Api-Version", "2023-09-01");

			var secretKey = (mode == "sandbox") ? _configuration["Sandbox:SecretKey"] : _configuration["Prod:SecretKey"];
			if (secretKey == null)
			{
				Console.Error.WriteLine("Api secret key is incorrect!");
				return BadRequest("Creating Order was failed!");
			}

			request.AddHeader("Authorization", $"Bearer {secretKey}");
			var body = @"{" + "\n" +
			@$"  ""amount"": {order.Price}," + "\n" +
			@$"  ""currency"": ""{order.Currency}""" + "\n" +
			@"}";
			request.AddStringBody(body, DataFormat.Json);
			RestResponse response = await client.ExecuteAsync(request);

			if (response.IsSuccessful && response.Content != null)
			{
				var data = JObject.Parse(response.Content);
				string? checkoutUrl = (string?)data["checkout_url"];
				Console.WriteLine(data.ToString());
				if (checkoutUrl != null)
					return Redirect(checkoutUrl);
			}

			return BadRequest("Creating Order was failed!");
		}
	}
}
