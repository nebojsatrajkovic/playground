using System.ComponentModel;

namespace Core.Shared.Database.Generator.Enumerations
{
    public enum CORE_DB_TABLE_TEMPLATE_PLACEHOLDER
    {
        [Description("${NAMESPACE}")]
        NAMESPACE,

        [Description("${TABLE_NAME}")]
        TABLE_NAME,

        [Description("${MODEL_ATTRIBUTES}")]
        MODEL_ATTRIBUTES,

        [Description("${QUERY_ATTRIBUTES}")]
        QUERY_ATTRIBUTES
    }
}