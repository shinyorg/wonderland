namespace ShinyWonderland.Features.AI;

public enum AiPhase
{
    Idle,
    Prompting,
    Listening,
    Thinking,
    Speaking
}

public record AiPhaseChanged(AiPhase Phase) : IEvent;
