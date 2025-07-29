using Villagers.GameServer.Domain.Commands;

namespace Villagers.GameServer.Domain;

public class CommandQueue
{
    private readonly Queue<ICommand> _commands = new();
    private readonly object _lock = new();

    public void EnqueueCommand(ICommand command)
    {
        lock (_lock)
        {
            _commands.Enqueue(command);
        }
    }

    public IReadOnlyList<ICommand> GetCommandsAndClear()
    {
        lock (_lock)
        {
            var commandsCopy = _commands.ToList();
            _commands.Clear();
            return commandsCopy;
        }
    }
}