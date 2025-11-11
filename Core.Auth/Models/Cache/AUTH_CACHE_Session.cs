namespace Core.Auth.Models.Cache
{
    internal class AUTH_CACHE_Session
    {
        public AUTH_CACHE_SessionInfo SessionInfo { get; set; }
        public DateTime ValidUntil_UTC { get; set; }
    }

    internal struct AUTH_CACHE_SessionInfo
    {
        public int SessionID { get; set; }
        public int AccountID { get; set; }
        public int TenantID { get; set; }
    }
}