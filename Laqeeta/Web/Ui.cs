namespace Laqeeta.Api.Web;

public static class Ui
{
    /// <summary>Bootstrap colour class for any status enum.</summary>
    public static string BadgeClass(Enum status) => status.ToString() switch
    {
        "Open" or "InStorage" => "text-bg-primary",
        "Matched" or "Pending" => "text-bg-warning",
        "Suggested" or "Verified" => "text-bg-info",
        "Confirmed" or "Returned" or "HandedOver" => "text-bg-success",
        "Rejected" or "Disposed" => "text-bg-danger",
        _ => "text-bg-secondary"
    };
}
