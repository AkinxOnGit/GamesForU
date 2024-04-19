using System.Runtime.Serialization;

namespace GamesForU.Models
{
    //DataContract for Serializing Data - required to serve in JSON format
    [DataContract]
    public class DataPoint
    {
        public DataPoint(string name, decimal totalAmount)
        {
            this.Label = name;
            this.Y = Convert.ToDouble(totalAmount);
        }

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "label")]
        public string Label = "";

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "y")]
        public Nullable<double> Y = null;
        private string name;
        private decimal totalAmount;
    }
}
