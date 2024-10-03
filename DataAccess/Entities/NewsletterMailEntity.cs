namespace EmailPOC.DataAccess.Entities
{
	public class NewsletterMailEntity
	{
        public int Id { get; set; }
        public Guid UmbracoContentKey { get; set; }
        public DateTime ShceduledSendDate { get; set; }
        public DateTime? SentDate { get; set; }
        public required string AdminResponseEn { get; set; }
        public required string AdminResponseAr {  get; set; }
        public string? FileNameEn { get; set; }
        public string? FileNameAr { get; set; }
        public string? AttachementPath { get; set; }
        public required string UnsubscribeUrlEn { get; set; }
        public required string UnsubscribeUrlAr { get; set; }
    }
}
