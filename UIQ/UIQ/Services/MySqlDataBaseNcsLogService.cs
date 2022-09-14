using Microsoft.Extensions.Options;
using UIQ.Enums;

namespace UIQ.Services
{
    public class MySqlDataBaseNcsLogService : MySqlDataBaseService
    {
        public MySqlDataBaseNcsLogService(IOptions<ConnectoinStringOption> connectoinStringOption) : base(connectoinStringOption, DataBaseEnum.NcsLog)
        {
        }
    }
}