using Dapper;
using Microsoft.Extensions.Options;
using MySqlConnector;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using UIQ.Enums;
using UIQ.Services.Interfaces;

namespace UIQ.Services
{
    public class MySqlDataBaseService : IDataBaseService
    {
        public string ConnectionString { get; set; }
        public DataBaseEnum DataBase { get; }

        public MySqlDataBaseService(IOptions<ConnectoinStringOption> connectoinStringOption, DataBaseEnum dataBase)
        {
            DataBase = dataBase;
            ConnectionString = connectoinStringOption.Value.GetConnectoinString(dataBase);
        }

        public async Task<int> DeleteAsync(string tableName, object parameter = null)
        {
            var sql = $"DELETE FROM `{tableName}` {GetWhereSql(parameter)}";
            if (parameter == null) return await ExecuteWithTransactionAsync(sql);

            var actualParamter = new DynamicParameters();
            parameter.GetType().GetProperties().ToList().ForEach(prop =>
            {
                actualParamter.Add($"@_{prop.Name}", prop.GetValue(parameter));
            });

            return await ExecuteWithTransactionAsync(sql, actualParamter);
        }

        public async Task<int> ExecuteWithTransactionAsync(string sql, object parameter = null, CommandType commandType = CommandType.Text)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                try
                {
                    var result = conn.ExecuteAsync(sql, parameter).GetAwaiter().GetResult();
                    await transaction.CommitAsync();
                    return result;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>(string tableName, object parameter = null, string[] selectColumns = null)
        {
            var fieldNames = selectColumns?.Any() ?? false
                ? string.Join(", ", selectColumns.Select(x => $"`{x}`"))
                : string.Join(", ", typeof(T).GetProperties().Where(x => !x.CustomAttributes.Any(a => a.AttributeType == typeof(NotMappedAttribute) || a.AttributeType == typeof(DatabaseGeneratedAttribute)))
                    .Select(x => $"`{x.Name.ToLower()}`"));
            var sql = $@"SELECT {fieldNames}
                         FROM `{tableName}` {GetWhereSql(parameter)}";

            return await QueryAsync<T>(sql, parameter);
        }

        public async Task<int> InsertAsync<T>(string tableName, T model)
        {
            if (model == null) return 0;
            var props = model.GetType().GetProperties().Where(x => !x.CustomAttributes.Any(a => a.AttributeType == typeof(NotMappedAttribute) || a.AttributeType == typeof(DatabaseGeneratedAttribute)));
            var fieldNames = props.Select(x => $"`{x.Name.ToLower()}`");
            var fieldValues = props.Select(x => $"@{x.Name}");

            var sql = $@"INSERT INTO `{tableName}`({string.Join(", ", fieldNames)})
                         VALUES({string.Join(", ", fieldValues)})";

            return await ExecuteWithTransactionAsync(sql, model);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameter = null, CommandType commandType = CommandType.Text)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                try
                {
                    var result = conn.QueryAsync<T>(sql, parameter, transaction).GetAwaiter().GetResult();
                    await transaction.CommitAsync();
                    return result;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<int> UpdateAsync<T>(string tableName, T model, object parameter = null)
        {
            if (model == null) return 0;

            var props = model.GetType().GetProperties().Where(x => !x.CustomAttributes.Any(a => a.AttributeType == typeof(NotMappedAttribute) || a.AttributeType == typeof(DatabaseGeneratedAttribute)));
            var setSql = string.Join(", ", props.Select(prop => $"`{prop.Name.ToLower()}` = @{prop.Name}"));
            var whereSql = GetWhereSql(parameter);
            var sql = $@"UPDATE `{tableName}`
                         SET {setSql} {whereSql}";

            return await ExecuteWithTransactionAsync(sql, parameter);
        }

        #region Private Methods

        private bool IsEnumerableType(Type type)
        {
            return typeof(IEnumerable<dynamic>).IsAssignableFrom(type) || type.IsArray;
        }

        private string GetWhereSql(object whereParamter)
        {
            if (whereParamter == null || whereParamter.GetType().GetProperties().Any() == false) return string.Empty;

            var props = whereParamter.GetType().GetProperties().Where(x => !x.CustomAttributes.Any(a => a.AttributeType == typeof(NotMappedAttribute)));
            var whereConditionSqls = props.Select(prop =>
            {
                var isEunmerableType = IsEnumerableType(prop.PropertyType);
                return isEunmerableType
                    ? $"`{prop.Name.ToLower()}` IN {"@_" + prop.Name}"
                    : $"`{prop.Name.ToLower()}` = {"@_" + prop.Name}";
            });

            return whereConditionSqls.Any() ? $"WHERE {string.Join(" AND ", whereConditionSqls)}" : string.Empty;
        }

        #endregion Private Methods
    }
}