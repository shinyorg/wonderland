namespace ShinyWonderland.Contracts;

// AppSettings [Bind] properties don't raise PropertyChanged - publish this when parking is changed outside the parking page
public record ParkingLocationChangedEvent(Position? Position) : IEvent;
