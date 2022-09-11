using UIQ.Enums;

namespace UIQ.Services
{
    public class MySqlDataBaseNcsLogService : MySqlDataBaseService
    {
        public MySqlDataBaseNcsLogService(IConfiguration _configuration) : base(_configuration, DataBaseEnum.NcsLog)
        {
        }
    }
}
