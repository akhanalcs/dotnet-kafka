namespace Consumer.Domain;

public record Biometrics(Guid DeviceId, List<HeartRate> HeartRates, int MaxHeartRate);
public record HeartRate(DateTime DateTime, int Value);

public record HeartRateZoneReached(Guid DeviceId, HeartRateZone Zone, DateTime DateTime, int HeartRate, int MaxHeartRate);