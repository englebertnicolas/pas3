using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace PAS.AspNetCore;

public static class ConfigurationExtensions {

    /// <summary>
    /// Builds a complete SQL connection string by enriching the base connection string from <c>ConnectionStrings:{name}</c> 
    /// with optional credentials retrieved from <c>SqlUsers:{name}:Name</c> and <c>SqlUsers:{name}:Password</c>.
    /// </summary>
    public static string? BuildConnectionString(this IConfiguration configuration, string name, bool overwriteConfiguration = false) {
    var cncString = configuration.GetConnectionString(name);
        if (cncString == null) return null;

        var userName = configuration[$"SqlUsers:{name}:Name"];
        var password = configuration[$"SqlUsers:{name}:Password"];

        SqlConnectionStringBuilder builder;
        try {
            builder = new SqlConnectionStringBuilder(cncString);
        } catch {
            // Connection string is not a SQL connection string
            return cncString;
        }

        if (!string.IsNullOrEmpty(userName)) builder.UserID = userName;
        if (!string.IsNullOrEmpty(password)) builder.Password = password;

        if (overwriteConfiguration && builder.ConnectionString != cncString) {
            configuration[$"ConnectionStrings:{name}"] = builder.ConnectionString;
        }

        return builder.ConnectionString;
    }
}
