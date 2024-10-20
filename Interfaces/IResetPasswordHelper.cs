namespace EmailPOC.Interfaces
{
    public interface IResetPasswordHelper
    {
        Task TriggerResetPasswordEventAsync(string email);
    }
}
