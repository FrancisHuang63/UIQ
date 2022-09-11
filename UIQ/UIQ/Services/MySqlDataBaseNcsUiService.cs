using UIQ.Enums;

namespace UIQ.Services
{
    public class MySqlDataBaseNcsUiService : MySqlDataBaseService
    {
        public MySqlDataBaseNcsUiService(IConfiguration _configuration): base(_configuration, DataBaseEnum.NcsUi)
        {
        }
    }
}
