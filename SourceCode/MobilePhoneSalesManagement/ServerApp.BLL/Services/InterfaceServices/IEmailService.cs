namespace ServerApp.BLL.Services.InterfaceServices
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body);
    }
}
