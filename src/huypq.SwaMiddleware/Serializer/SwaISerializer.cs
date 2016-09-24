namespace huypq.SwaMiddleware
{
    public interface SwaISerializer
    {
        void Serialize(System.IO.Stream output, object data);
        T Deserialize<T>(object data);
    }
}
