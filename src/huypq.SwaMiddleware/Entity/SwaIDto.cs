namespace huypq.SwaMiddleware
{
    public interface SwaIDto<T> where T : class
    {
        int Ma { get; set; }
        void FromEntity(T entity);
        T ToEntity();
    }
}
