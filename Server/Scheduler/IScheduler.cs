using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;

namespace EmmyLua.LanguageServer.Framework.Server.Scheduler;

public interface IScheduler
{
    void Schedule(Func<Message, Task> action, Message message);
}
