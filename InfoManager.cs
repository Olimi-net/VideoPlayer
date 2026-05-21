using System;
/**
 * @Author Lada 
 * @Created 2018.08.29 
 */
namespace VideoPlayer
{
    public class InfoManager
    {
        InfoModule _info;

        public InfoManager()
        {
            _info = new InfoModule();
        }

        internal InfoModule GetInfoModule()
        {
            return _info;
        }

        internal void Exit()
        {
            _info.SaveInfo();
        }
    }
}
