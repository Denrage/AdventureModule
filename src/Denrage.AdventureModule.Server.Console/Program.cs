using Autofac;
using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Server.MessageHandlers;
using Denrage.AdventureModule.Server.Services;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        private readonly IContainer container;

        public Program()
        {
            var builder = new ContainerBuilder();
            builder
                .RegisterType<TcpService>()
                .As<ITcpService>()
                .SingleInstance()
                .OnActivated(e => this.RegisterMessageHandler(e.Instance, e.Context));
            builder
                .RegisterType<DrawObjectService>()
                .As<IDrawObjectService>()
                .SingleInstance()
                .OnActivated(e => this.RegisterDrawObjects(e.Instance));
            builder.RegisterType<SynchronizationService>().As<ISynchronizationService>().SingleInstance();
            builder.RegisterType<UserManagementService>().As<IUserManagementService>().SingleInstance();
            builder.RegisterType<PlayerMumbleService>().As<IPlayerMumbleService>().SingleInstance();
            this.RegisterMessageHandler(builder);
            this.container = builder.Build();
        }

        private void RegisterDrawObject<T>(ContainerBuilder builder, bool withUpdate = false)
            where T : DrawObject
        {
            builder.RegisterType<AddDrawObjectMessageHandler<T>>().As<IMessageHandler>().SingleInstance();
            builder.RegisterType<RemoveDrawObjectMessageHandler<T>>().As<IMessageHandler>().SingleInstance();
            if (withUpdate)
            {
                builder.RegisterType<UpdateDrawObjectMessageHandler<T>>().As<IMessageHandler>().SingleInstance();
            }
        }

        public void Start()
        {
            var tcpService = this.container.Resolve<ITcpService>();

            Task.Run(async () => await tcpService.Initialize());

        }

        private void RegisterMessageHandler(ContainerBuilder builder)
        {
            this.RegisterDrawObject<Line>(builder);
            this.RegisterDrawObject<MapMarker>(builder);
            this.RegisterDrawObject<Image>(builder, true);
            builder.RegisterType<PlayerMumbleMessageHandler>().As<IMessageHandler>().SingleInstance();
            builder.RegisterType<PingMessageHandler>().As<IMessageHandler>().SingleInstance();
            builder.RegisterType<LoginMessageHandler>().As<IMessageHandler>().SingleInstance();
            builder.RegisterType<StateChangedMessageHandler<DialogState>>().As<IMessageHandler>().SingleInstance();
            builder.RegisterType<StateChangedMessageHandler<LuaVariablesState>>().As<IMessageHandler>().SingleInstance();
            builder.RegisterType<StateChangedMessageHandler<AdventureState>>().As<IMessageHandler>().SingleInstance();
            builder.RegisterType<GetStatesMessageHandler>().As<IMessageHandler>().SingleInstance();

        }

        private void RegisterMessageHandler(TcpService instance, IComponentContext context)
        {
            var messageHandler = context.Resolve<IEnumerable<IMessageHandler>>();
            foreach (var handler in messageHandler)
            {
                instance.RegisterMessage(handler);
            }
        }

        private void RegisterDrawObjects(DrawObjectService drawObjectService)
        {

            drawObjectService.Register<Line, AddDrawObjectMessage<Line>, RemoveDrawObjectMessage<Line>, UpdateDrawObjectMessage<Line>>(
                lines => new AddDrawObjectMessage<Line>() { DrawObjects = lines.ToArray() },
                ids => new RemoveDrawObjectMessage<Line>() { Ids = ids.ToArray() },
                lines => new UpdateDrawObjectMessage<Line>() { DrawObjects = lines.ToArray() },
                null);

            drawObjectService.Register<MapMarker, AddDrawObjectMessage<MapMarker>, RemoveDrawObjectMessage<MapMarker>, UpdateDrawObjectMessage<MapMarker>>(
                markers => new AddDrawObjectMessage<MapMarker>() { DrawObjects = markers.ToArray() },
                ids => new RemoveDrawObjectMessage<MapMarker>() { Ids = ids.ToArray() },
                markers => new UpdateDrawObjectMessage<MapMarker>() { DrawObjects = markers.ToArray() },
                null);

            drawObjectService.Register<Image, AddDrawObjectMessage<Image>, RemoveDrawObjectMessage<Image>, UpdateDrawObjectMessage<Image>>(
                images => new AddDrawObjectMessage<Image>() { DrawObjects = images.ToArray() },
                ids => new RemoveDrawObjectMessage<Image>() { Ids = ids.ToArray() },
                images => new UpdateDrawObjectMessage<Image>() { DrawObjects = images.ToArray() },
                (oldObject, newObject) =>
                {
                    if (newObject.Location != null)
                    {
                        oldObject.Location = newObject.Location;
                    }

                    if (newObject.Data != null && newObject.Data.Length > 0)
                    {
                        oldObject.Data = newObject.Data;
                    }

                    if (newObject.Opacity.HasValue)
                    {
                        oldObject.Opacity = newObject.Opacity;
                    }

                    if (newObject.Rotation.HasValue)
                    {
                        oldObject.Rotation = newObject.Rotation;
                    }

                    if (newObject.Size != null)
                    {
                        oldObject.Size = newObject.Size;
                    }
                });

        }

        static void Main(string[] args)
        {
            var program = new Program();
            program.Start();
            Console.WriteLine("Server started");
            Console.ReadLine();

        }
    }
}