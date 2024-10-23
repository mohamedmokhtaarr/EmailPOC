namespace EmailPOC.DataAccess.Entities
{
    public class EmailLog
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? EmailType { get; set; } // ResetPassword, ValidationEmail, etc.
        public bool IsSent { get; set; } // To mark if the email was successfully sent
        public int RetryCount { get; set; } // For retry mechanism
        public DateTime SentDate { get; set; }
        public DateTime? RetryDate { get; set; } // Nullable for retries
    }

}
