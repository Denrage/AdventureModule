﻿using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.MessageHandlers
{
    public class AddDrawObjectMessageHandler<TDrawObject> : MessageHandler<AddDrawObjectMessage<TDrawObject>>
        where TDrawObject : DrawObject
    {
        private readonly Func<DrawObjectService> getDrawObjectService;

        public AddDrawObjectMessageHandler(Func<DrawObjectService> getDrawObjectService)
        {
            this.getDrawObjectService = getDrawObjectService;
        }

        protected override async Task Handle(Guid clientId, AddDrawObjectMessage<TDrawObject> message, CancellationToken ct)
        {
            await this.getDrawObjectService().Add(message.DrawObjects, true, ct);
        }
    }
}
