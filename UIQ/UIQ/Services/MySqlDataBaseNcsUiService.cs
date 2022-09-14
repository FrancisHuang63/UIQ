using Microsoft.Extensions.Options;
using UIQ.Enums;

namespace UIQ.Services
{
    public class MySqlDataBaseNcsUiService : MySqlDataBaseService
    {
        public MySqlDataBaseNcsUiService(IOptions<ConnectoinStringOption> connectoinStringConfigure) : base(connectoinStringConfigure, DataBaseEnum.NcsUi)
        {
        }
    }
}
