using Microsoft.Data.SqlClient;

namespace MafiaPickem.Api.Data;

public interface IDbConnectionFactory
{
    SqlConnection CreateConnection();
}
