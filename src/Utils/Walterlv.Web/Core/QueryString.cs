using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;

namespace Walterlv.Web.Core
{
    internal class QueryString
    {
        [return: NotNullIfNotNull("query")]
        public static string? Serialize(object? query, string? prefix = "?")
        {
            if (query is null)
            {
                return null;
            }

            var isContractedType = query.GetType().IsDefined(typeof(DataContractAttribute));
            var properties = from property in query.GetType().GetProperties()
                             where property.CanRead && (isContractedType ? property.IsDefined(typeof(DataMemberAttribute)) : true)
                             let memberName = isContractedType ? property.GetCustomAttribute<DataMemberAttribute>().Name : property.Name
                             let value = property.GetValue(query, null)
                             where value != null && !string.IsNullOrWhiteSpace(value.ToString())
                             select memberName + "=" + HttpUtility.UrlEncode(value.ToString());
            var queryString = string.Join("&", properties);
            return string.IsNullOrWhiteSpace(queryString) ? "" : prefix + queryString;
        }
    }
}
