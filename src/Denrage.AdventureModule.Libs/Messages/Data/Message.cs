using System;

namespace Denrage.AdventureModule.Libs.Messages.Data
{
    public abstract class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}