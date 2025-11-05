namespace Core.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CORE_AUTH_RequiredRight : Attribute
    {
        public string RequiredRight { get; set; }

        public CORE_AUTH_RequiredRight(string RequiredRight)
        {
            this.RequiredRight = RequiredRight;
        }
    }

    // NOTE: var meta = HttpContext.GetEndpoint().Metadata.OfType<CORE_AUTH_RequiredRight>();
}