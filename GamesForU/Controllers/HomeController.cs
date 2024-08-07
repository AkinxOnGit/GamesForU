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
            List<DataPoint> dataPointsBestSelling = new List<DataPoint>();
            List<DataPoint> dataPointsMostExpensive = new List<DataPoint>();
            List<DataPoint> dataPointsUserOrders = new List<DataPoint>();

            decimal totalOrderSum = 0; 

            string connectionString = "Server=(localdb)\\mssqllocaldb;Database=aspnet-GamesForU-f2042bb1-8e6b-4f9f-a160-120a23df4704;Trusted_Connection=True;MultipleActiveResultSets=true";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Abfrage für die meistverkauften Spiele
                string queryBestSelling = @"
                SELECT Games.Name,
                       GamesOrders.GamesId,
                       SUM(GamesOrders.Amount) AS TotalAmount
                FROM GamesOrders
                INNER JOIN Games ON GamesOrders.GamesId = Games.Id 
                INNER JOIN Orders ON GamesOrders.OrdersId = Orders.Id
                WHERE Orders.Invoice IS NOT NULL
                GROUP BY Games.Name, GamesOrders.GamesId
                ORDER BY TotalAmount DESC";

                SqlCommand commandBestSelling = new SqlCommand(queryBestSelling, connection);

                connection.Open();
                SqlDataReader readerBestSelling = commandBestSelling.ExecuteReader();

                while (readerBestSelling.Read())
                {
                    GameInfo gameInfo = new GameInfo
                    {
                        Name = readerBestSelling.GetString(0), // Assuming Name is the first column
                        GamesId = readerBestSelling.GetInt32(1), // Assuming GamesId is the second column
                        TotalAmount = Convert.ToDecimal(readerBestSelling.GetInt32(2)) // Convert Int32 to Decimal
                    };

                    dataPointsBestSelling.Add(new DataPoint(gameInfo.Name, gameInfo.TotalAmount));
                }

                readerBestSelling.Close();

                // Abfrage für die teuersten Spiele
                string queryMostExpensive = @"
                SELECT Games.Name, 
                       CAST(Games.Price AS decimal(18,2)) AS Price
                FROM Games
                ORDER BY Price DESC";

                SqlCommand commandMostExpensive = new SqlCommand(queryMostExpensive, connection);
                SqlDataReader readerMostExpensive = commandMostExpensive.ExecuteReader();

                while (readerMostExpensive.Read())
                {
                    string gameName = readerMostExpensive.GetString(0); // Assuming Name is the first column
                    decimal gamePrice = readerMostExpensive.GetDecimal(1); // Assuming Price is the second column

                    dataPointsMostExpensive.Add(new DataPoint(gameName, gamePrice));
                }

                readerMostExpensive.Close();
                // Abfrage für die Benutzer mit den meisten Bestellungen
                string queryUserOrders = @"
                SELECT Users.UserName,
                       COUNT(GamesOrders.Id) AS OrderCount
                FROM GamesOrders
                INNER JOIN Orders ON GamesOrders.OrdersId = Orders.Id
                INNER JOIN AspNetUsers AS Users ON Orders.IdentityUserId = Users.Id
                WHERE Orders.Invoice IS NOT NULL
                GROUP BY Users.UserName
                ORDER BY OrderCount DESC";

                SqlCommand commandUserOrders = new SqlCommand(queryUserOrders, connection);
                SqlDataReader readerUserOrders = commandUserOrders.ExecuteReader();

                while (readerUserOrders.Read())
                {
                    string userName = readerUserOrders.GetString(0); // Assuming UserName is the first column
                    int orderCount = readerUserOrders.GetInt32(1); // Assuming OrderCount is the second column

                    dataPointsUserOrders.Add(new DataPoint(userName, orderCount));
                }

                readerUserOrders.Close();

                // Berechnung der Gesamtsumme aller Bestellungen
                string queryTotalOrderSum = @"
                SELECT SUM(CAST(Games.Price AS decimal(18,2)) * GamesOrders.Amount) AS TotalOrderSum
                FROM GamesOrders
                INNER JOIN Games ON GamesOrders.GamesId = Games.Id
                INNER JOIN Orders ON GamesOrders.OrdersId = Orders.Id
                WHERE Orders.Invoice IS NOT NULL";

                SqlCommand commandTotalOrderSum = new SqlCommand(queryTotalOrderSum, connection);
                totalOrderSum = (decimal)commandTotalOrderSum.ExecuteScalar();
            }

            ViewBag.DataPointsBestSelling = JsonConvert.SerializeObject(dataPointsBestSelling);
            ViewBag.DataPointsMostExpensive = JsonConvert.SerializeObject(dataPointsMostExpensive);
            ViewBag.DataPointsUserOrders = JsonConvert.SerializeObject(dataPointsUserOrders);
            ViewBag.TotalOrderSum = totalOrderSum;

            return View();
        }

        private class GameInfo
        {
            public string Name { get; set; }
            public int GamesId { get; set; }
            public decimal TotalAmount { get; set; }
        }

        private class UserOrderInfo
        {
            public string UserName { get; set; }
            public int OrderCount { get; set; }
        }
    }
}
