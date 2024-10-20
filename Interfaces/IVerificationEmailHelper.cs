namespace EmailPOC.Interfaces
{
    public interface IVerificationEmailHelper
    {
        Task TriggerVerificationEmailEventAsync(string email);

    }
}
