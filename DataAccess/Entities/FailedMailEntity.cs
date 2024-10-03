namespace EmailPOC.DataAccess.Entities
{
	public class FailedMailEntity
    {
        public int Id { get; set; }
        public int NewsletterMailId { get; set; }
        public required string RecepientEmail { get; set; }
        public DateTime FailDate { get; set; }
        public int Retries { get; set; } = 1;

        public override string ToString()
        {
            return 
                $"\n -> Mail Id: {Id} \n" +
                $"-> Newsletter Mail Id: {Id} \n" +
                $"-> Recipient Email: {Id} \n" +
                $"-> Last Fail Date: {Id} \n" +
                $"-> Mail failed after {Id} retries \n";
        }
    }
}
