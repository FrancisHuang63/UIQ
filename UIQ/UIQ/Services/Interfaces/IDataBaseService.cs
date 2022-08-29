using System.Data;

namespace UIQ.Services.Interfaces
{
    public interface IDataBaseService
    {
        string ConnectionString { get; set; }

        public Task<int> DeleteAsync(string tableName, object parameter = null);

        public Task<IEnumerable<T>> GetAllAsync<T>(string tableName, object parameter = null, string[] selectColumns = null);

        public Task<int> InsertAsync<T>(string tableName, T model);

        public Task<int> UpdateAsync<T>(string tableName, T model, object parameter = null);

        public Task<int> ExecuteWithTransactionAsync(string sql, object parameter = null, CommandType commandType = CommandType.Text);

        public Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameter = null, CommandType commandType = CommandType.Text);
    }
}
