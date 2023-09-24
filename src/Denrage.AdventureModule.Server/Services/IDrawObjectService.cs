using Denrage.AdventureModule.Libs.Messages.Data;

namespace Denrage.AdventureModule.Server.Services;

public interface IDrawObjectService
{
    Task Add<T>(IEnumerable<T> drawObjects, Guid clientId, CancellationToken ct) where T : DrawObject;
    Task Remove<T>(IEnumerable<Guid> drawObject, Guid clientId, CancellationToken ct) where T : DrawObject;
    Task Update<T>(IEnumerable<T> drawObjects, Guid clientId, CancellationToken ct) where T : DrawObject;
}