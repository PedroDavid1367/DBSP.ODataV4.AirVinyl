using System.ComponentModel.DataAnnotations;

namespace DBSP.ODataV4.AirVinyl.Model
{
  public class Rating
  {
    [Key]
    public int RatingId { get; set; }

    [Required]
    public int Value { get; set; }

    [Required]
    public Person RatedBy { get; set; }
  }
}
