using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;

namespace Wyam.Core.Modules.Metadata
{
    /// <summary>
    /// Creates documents from the results of a SQL query. Each row is output
    /// as a new document and every column is used as the metadata (or content) of
    /// the new document. Input documents are ignored.
    /// </summary>
    /// <category>Metadata</category>
    public class Sql : ReadDataModule<Sql, DataRow>
    {
        private readonly string _connectionString;
        private readonly string _sql;

        /// <summary>
        /// Creates documents from a SQL query given the specified connection string and query.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="sql">The SQL query.</param>
        public Sql(string connectionString, string sql)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException(nameof(connectionString));
            }
            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentException(nameof(sql));
            }

            _connectionString = connectionString;
            _sql = sql;
        }

        /// <inheritdoc />
        protected override IEnumerable<DataRow> GetItems(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlDataAdapter adapter = new SqlDataAdapter(_sql, conn))
                {
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable.AsEnumerable();
                }
            }
        }

        /// <inheritdoc />
        protected override IDictionary<string, object> GetDictionary(DataRow row) =>
            row.Table.Columns.Cast<DataColumn>().ToDictionary(col => col.ColumnName, col => row[col]);
    }
}
