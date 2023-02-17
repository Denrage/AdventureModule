namespace Denrage.AdventureModule.Adventure
{
    public class LogicBuilderCreator
    {
        public object CreateButtonOrderLogic()
        {
            return new ButtonOrderBuilder();
        }
    }
}

