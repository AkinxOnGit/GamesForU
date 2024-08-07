
using GamesForU.Models;

public class HomeViewModel
{
    public List<DataPoint> DataPointsBestSelling { get; set; }
    public List<DataPoint> DataPointsMostExpensive { get; set; }
    public List<UserOrderInfo> UserOrderInfos { get; set; }
}

public class UserOrderInfo
{
    public string UserName { get; set; }
    public int OrderCount { get; set; }
}