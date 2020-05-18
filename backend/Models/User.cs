using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ClimateDataAnalyticsApi.Models
{
	public class User
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		[BsonElement("Name")]
		public string Nome { get; set; }

		public string Email { get; set; }
        public string Password { get; set; }
        public string Birthday { get; set; }
		public string Power { get; set; }
    }
}    