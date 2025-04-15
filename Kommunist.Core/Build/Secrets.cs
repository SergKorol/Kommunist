namespace Kommunist.Core.Build
{
    public class Secrets
    {
#if HAS_BLOB_SECRET
        public const string ConnectionString = "__BLOB_CONNECTION_STRING__";
#else
        public const string ConnectionString = null;
#endif
    }
}
