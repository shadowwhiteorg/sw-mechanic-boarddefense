using _Game.Interfaces;

namespace _Game.Core.Events
{
    public readonly struct GameStartedEvent : IGameEvent
    {
        public string Message { get; }
        public GameStartedEvent(string message) => Message = message;
        public override string ToString() => $"GameStartedEvent(Message={Message})";
        
        
    }
    
    public readonly struct GameWonEvent : IGameEvent { }

    public readonly struct GameLostEvent : IGameEvent { }
}