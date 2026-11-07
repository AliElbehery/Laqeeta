namespace Laqeeta.Core.Enums;

public enum UserRole { User = 1, Employee = 2 }

public enum ItemCategory
{
    Phone = 1, Wallet, Bag, Keys, Documents, Electronics,
    Clothing, Jewelry, Glasses, Other
}

public enum LostReportStatus { Open = 1, Matched, Closed }

public enum ClosureReason { Returned = 1, CancelledByUser, Expired, Duplicate }

public enum FoundItemStatus { InStorage = 1, Matched, Returned, Transferred, Disposed }

public enum MatchStatus { Suggested = 1, Confirmed, Rejected }

public enum ClaimStatus { Pending = 1, Verified, Rejected, HandedOver }

public enum NotificationType { MatchFound = 1, ClaimUpdate, ItemReturned, ReportClosed, System }
