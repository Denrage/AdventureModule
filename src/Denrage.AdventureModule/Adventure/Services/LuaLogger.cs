namespace Denrage.AdventureModule.Adventure.Services
{
    public class LuaLogger
    {
        public void Log(object message) => System.Diagnostics.Debug.WriteLine(message.ToString());
    }
}

