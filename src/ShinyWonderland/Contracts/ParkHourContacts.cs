namespace ShinyWonderland.Contracts;

public record GetCurrentParkHours : IRequest<ParkHours>;
public record GetUpcomingParkHours : IRequest<ParkHours[]>;

public record ParkHours(
    DateOnly Date,
    TimeRange? Hours
)
{
    public bool IsOpen => this.Hours != null;
    public bool IsClosed => this.Hours == null;
};
public record TimeRange(TimeOnly Open, TimeOnly Closed);