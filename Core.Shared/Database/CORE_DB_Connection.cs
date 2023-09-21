using System.Data;
using System.Data.SqlClient;

namespace Core.Shared.Database
{
    public class CORE_DB_Connection : IDisposable
    {
        readonly SqlConnection _Connection = null!;
        SqlTransaction _Transaction = null!;

        public SqlConnection Connection => _Connection;
        public SqlTransaction Transaction => _Transaction;

        public CORE_DB_Connection(string connectionString)
        {
            var connection = new SqlConnection(connectionString);

            connection.Open();

            _Connection = connection;

            _Transaction = connection.BeginTransaction();
        }

        public void Dispose()
        {
            CloseConnection();
            _Transaction.Dispose();

            GC.SuppressFinalize(this);
        }

        public void Commit()
        {
            if (_Connection.State == ConnectionState.Open && _Transaction != null)
            {
                _Transaction.Commit();
            }
        }

        public void CommitAndBeginNewTransaction()
        {
            if (_Connection.State == ConnectionState.Open && _Transaction != null)
            {
                _Transaction.Commit();
                _Transaction = _Connection.BeginTransaction();
            }
        }

        public void BeginNewTransaction()
        {
            if (_Connection.State == ConnectionState.Open && _Transaction != null)
            {
                _Transaction = _Connection.BeginTransaction();
            }
        }

        public void RollBack()
        {
            if (_Connection.State == ConnectionState.Open && _Transaction != null)
            {
                _Transaction.Rollback();
            }
        }

        public void RollBackAndBeginNewTransaction()
        {
            if (_Connection.State == ConnectionState.Open && _Transaction != null)
            {
                _Transaction.Rollback();
                _Transaction = _Connection.BeginTransaction();
            }
        }

        public void CloseConnection()
        {
            if (_Connection.State == ConnectionState.Open)
            {
                _Connection.Close();
            }
        }
    }
}