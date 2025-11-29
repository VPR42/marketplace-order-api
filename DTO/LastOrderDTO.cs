using MarketPlace.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketPlace.DTO
{
    public class LastOrderDTO
    {
        public string OrderName { get; set; }
        public string Status { get; set; }

        public Decimal OrderPrice { get; set; }
        public DateTime OrderedAt { get; set; }

    }
}
