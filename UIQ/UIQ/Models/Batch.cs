using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UIQ.Models
{
	public class Batch
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[JsonPropertyName("batch_id")]
		public int Batch_Id { get; set; }

		[JsonPropertyName("batch_position")]
		public int Batch_Position { get; set; }

		[JsonPropertyName("member_id")]
		public int Member_Id { get; set; }

		[JsonPropertyName("batch_name")]
		public string Batch_Name { get; set; }

		[JsonPropertyName("batch_type")]
		public string Batch_Type { get; set; }

		[JsonPropertyName("batch_dtg")]
		public string Batch_Dtg { get; set; }

		[JsonPropertyName("batch_time")]
		public int Batch_Time { get; set; }

		[JsonPropertyName("batch_relay")]
		public int Batch_Relay { get; set; }
	}
}