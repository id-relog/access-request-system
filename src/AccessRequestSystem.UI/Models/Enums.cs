namespace AccessRequestSystem.UI.Models;

public enum RequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Expired = 3
}

public enum DurationPreset
{
    Custom = 0,
    Week = 1,
    Month = 2,
    Quarter = 3,
    HalfYear = 4,
    Year = 5
}
