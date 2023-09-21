namespace Core.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CORE_AuthorizationAttribute : Attribute
    {

    }

    // NOTE: var meta = HttpContext.GetEndpoint().Metadata.OfType<CORE_AuthorizationAttribute>();
}