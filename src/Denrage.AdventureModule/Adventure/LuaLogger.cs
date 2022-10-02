namespace Denrage.AdventureModule.Adventure
{
    public class LuaLogger
    {
        public void Log(object message) => System.Diagnostics.Debug.WriteLine(message.ToString());
    }


}


