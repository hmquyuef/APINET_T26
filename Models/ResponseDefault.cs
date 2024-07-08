namespace APINET_T26.Models
{
    public class ResponseDefault<TEntity>
    {
        public TEntity Items { get; set; }
        public string Filter { get; set; }
    }
}
