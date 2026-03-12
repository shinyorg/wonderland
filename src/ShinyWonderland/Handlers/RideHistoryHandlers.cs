namespace ShinyWonderland.Handlers;


[MediatorSingleton]
public class RideHistoryHandlers(
    IDataService data,
    TimeProvider timeProvider
) : 
    ICommandHandler<AddRideCommand>, 
    IRequestHandler<GetRideHistory, List<RideHistoryRecord>>, 
    IRequestHandler<GetParkLastRiddenTimes, List<LastRideTime>>
{
    public Task Handle(AddRideCommand command, IMediatorContext context, CancellationToken cancellationToken)
        => data.AddRideHistory(new RideHistoryRecord
        {
            RideId = command.RideId,
            RideName = command.RideName,
            Timestamp = timeProvider.GetUtcNow()
        });


    public async Task<List<RideHistoryRecord>> Handle(GetRideHistory request, IMediatorContext context, CancellationToken cancellationToken)
    {
        var result = await data.GetRideTimeHistory();
        return result
            .Select(x =>
            {
                x.Timestamp = x.Timestamp.LocalDateTime;
                return x;
            })
            .ToList();
    }

    
    public Task<List<LastRideTime>> Handle(GetParkLastRiddenTimes request, IMediatorContext context, CancellationToken cancellationToken)
        => data.GetLastRideTimes();
}

public record AddRideCommand(string RideId, string RideName) : ICommand;
public record GetRideHistory(Guid? Ride) : IRequest<List<RideHistoryRecord>>;
public record GetParkLastRiddenTimes : IRequest<List<LastRideTime>>;