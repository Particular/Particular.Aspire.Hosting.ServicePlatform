namespace Particular.Aspire.Hosting.ServicePlatform.Transport;

using System.Data.Common;
using global::Aspire.Hosting.ApplicationModel;

static class ConnectionStringExpressionBuilderExtensions
{
    extension(ReferenceExpressionBuilder builder)
    {
        /// <summary>
        /// Appends a ";keyword=value" pair to the connection string. It will apply the quoting/escaping 
        /// rules so that values containing ';' or '=' don't corrupt the connection string.
        /// </summary>
        public void AppendKeyword(string keyword, string? value)
        {
            if (value is null)
            {
                return;
            }

            var pair = new DbConnectionStringBuilder { [keyword] = value };
            builder.Append($";{pair.ConnectionString}");
        }
    }
}
