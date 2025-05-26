namespace ShinyWonderland.Contracts;

// don't put filters or sorts here
public record GetCurrentRideTimes : IRequest<List<RideTime>>;

public record RideTime(
    string Id,
    string Name,
    int? WaitTimeMinutes,
    int? PaidWaitTimeMinutes,
    Position? Position,
    bool IsOpen
);
