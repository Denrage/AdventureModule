namespace Denrage.AdventureModule.UserInterface.Windows.DrawTools
{
    public abstract class Tool
    {
        public abstract void OnUpdate(DrawContext context);

        public virtual void Reset() { }
    }
}
