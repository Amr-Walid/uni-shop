namespace FTD.Application.Interfaces
{
    /// <summary>
    /// Abstraction for raw cart storage (Key-Value string).
    /// Prevents ASP.NET Core Http dependencies from leaking into the Application layer.
    /// </summary>
    public interface ICartStorage
    {
        string? GetRaw();
        void SetRaw(string json);
        void Clear();
    }
}
