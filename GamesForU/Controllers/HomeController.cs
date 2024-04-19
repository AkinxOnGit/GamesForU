using GamesForU.Data;
using GamesForU.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Diagnostics;

namespace GamesForU.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ApplicationDbContext _context;


        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {

			List<DataPoint> dataPoints = new List<DataPoint>();

			string connectionString = "Server=(localdb)\\mssqllocaldb;Database=aspnet-GamesForU-f2042bb1-8e6b-4f9f-a160-120a23df4704;Trusted_Connection=True;MultipleActiveResultSets=true";

			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				string query = @"
                SELECT Games.Name,
                       GamesOrders.GamesId,
                       SUM(GamesOrders.Amount) AS TotalAmount
                FROM GamesOrders
                INNER JOIN Games ON GamesOrders.GamesId = Games.Id 
                INNER JOIN Orders ON GamesOrders.OrdersId = Orders.Id
                WHERE Orders.Invoice IS NOT NULL
                GROUP BY Games.Name, GamesOrders.GamesId
                ORDER BY TotalAmount DESC";

				SqlCommand command = new SqlCommand(query, connection);

				connection.Open();
				SqlDataReader reader = command.ExecuteReader();

				while (reader.Read())
				{
					GameInfo gameInfo = new GameInfo
					{
						Name = reader.GetString(0), // Assuming Name is the first column
						GamesId = reader.GetInt32(1), // Assuming GamesId is the second column
						TotalAmount = Convert.ToDecimal(reader.GetInt32(2)) // Convert Int32 to Decimal
					};

					dataPoints.Add(new DataPoint(gameInfo.Name, gameInfo.TotalAmount));
				}


				reader.Close();
			}

			ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);

			return View();













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

		private class GameInfo
		{
			public string Name { get; set; }
			public int GamesId { get; set; }
			public decimal TotalAmount { get; set; }
		}
	}
}