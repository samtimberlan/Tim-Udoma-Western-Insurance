namespace Tim_Udoma_Western_Insurance.Services.Interfaces
{
    public interface INotificationService : INotify
    {
        new void Notify(string userId, string message);
    }
}
