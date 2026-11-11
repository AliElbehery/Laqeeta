using Laqeeta.Core.Exceptions;

namespace Laqeeta.Core.Localization;

/// <summary>Error codes carried by <see cref="DomainException"/>.</summary>
public static class Err
{
    // not found
    public const string ReportNotFound = "err.reportNotFound";

    public const string ItemNotFound = "err.itemNotFound";
    public const string MatchNotFound = "err.matchNotFound";
    public const string ClaimNotFound = "err.claimNotFound";
    public const string UserNotFound = "err.userNotFound";
    public const string StationNotFound = "err.stationNotFound";
    public const string FoundStationNotFound = "err.foundStationNotFound";
    public const string TargetStationNotFound = "err.targetStationNotFound";
    public const string NotificationNotFound = "err.notificationNotFound";
    public const string LineNotFound = "err.lineNotFound";

    // forbidden / auth
    public const string NotYourReport = "err.notYourReport";

    public const string NotYourNotification = "err.notYourNotification";
    public const string NotYourMatch = "err.notYourMatch";
    public const string WrongStation = "err.wrongStation";
    public const string EmployeeRequired = "err.employeeRequired";
    public const string AccountDisabled = "err.accountDisabled";
    public const string InvalidCredentials = "err.invalidCredentials";
    public const string InvalidToken = "err.invalidToken";
    public const string CurrentPasswordWrong = "err.currentPasswordWrong";

    // conflicts
    public const string UsernameTaken = "err.usernameTaken";

    public const string EmailTaken = "err.emailTaken";
    public const string UserExists = "err.userExists";
    public const string ConcurrencyConflict = "err.concurrencyConflict";

    // business rules
    public const string PasswordSame = "err.passwordSame";

    public const string PasswordWeak = "err.passwordWeak";
    public const string ConfirmNeedsSuggested = "err.confirmNeedsSuggested";
    public const string RejectNeedsSuggested = "err.rejectNeedsSuggested";
    public const string ReportNotOpen = "err.reportNotOpen";
    public const string ItemNotInStorage = "err.itemNotInStorage";
    public const string LostDateFuture = "err.lostDateFuture";
    public const string ReportAlreadyClosed = "err.reportAlreadyClosed";
    public const string StationNoItems = "err.stationNoItems";
    public const string FoundTimeFuture = "err.foundTimeFuture";
    public const string TransferNeedsStorage = "err.transferNeedsStorage";
    public const string TargetNoItems = "err.targetNoItems";
    public const string ItemAlreadyThere = "err.itemAlreadyThere";
    public const string MatchNotConfirmed = "err.matchNotConfirmed";
    public const string ClaimExists = "err.claimExists";
    public const string ClaimNotPending = "err.claimNotPending";
    public const string ApproveNeedsChecks = "err.approveNeedsChecks";
    public const string ClaimNotVerified = "err.claimNotVerified";
}

/// <summary>Codes stored in <c>Notification.Code</c>; the text is translated when it is displayed.</summary>
public static class NotifCode
{
    public const string MatchFoundForReport = "notif.matchFoundForReport";
    public const string MatchFoundForItem = "notif.matchFoundForItem";
    public const string MatchConfirmed = "notif.matchConfirmed";
    public const string ClaimApproved = "notif.claimApproved";
    public const string ClaimRejected = "notif.claimRejected";
    public const string ItemReturned = "notif.itemReturned";
}

/// <summary>Arabic / English texts for error codes and notification codes. Shared by the website and the API.</summary>
public static class Messages
{
    public const string Ar = "ar";
    public const string En = "en";

    /// <summary>"ar-EG,ar;q=0.9" (Accept-Language) or "ar" (cookie) => "ar"; anything else => "en".</summary>
    public static string Normalize(string? lang)
        => lang is not null && lang.TrimStart().StartsWith("ar", StringComparison.OrdinalIgnoreCase) ? Ar : En;

    // ------------------------------------------------------------------ errors
    public static string Error(DomainException ex, string lang)
        => ex.Code is not null && ErrorTable.TryGetValue(ex.Code, out var t) ? Pick(t.Ar, t.En, lang) : ex.Message;

    // ------------------------------------------------------------------ notifications
    /// <summary>Localized title; falls back to the stored (English) title for old rows without a code.</summary>
    public static string Title(string? code, string stored, string lang)
        => code is not null && NotificationTable.TryGetValue(code, out var t) ? Pick(t.TitleAr, t.TitleEn, lang) : stored;

    /// <summary>Localized body; <paramref name="param"/> (e.g. a station name) fills {0}.</summary>
    public static string Body(string? code, string stored, string? param, string lang)
    {
        if (code is null || !NotificationTable.TryGetValue(code, out var t)) return stored;
        var text = Pick(t.BodyAr, t.BodyEn, lang);
        var arg = param ?? (Normalize(lang) == Ar ? "المحطة" : "the station");
        return text.Replace("{0}", arg);
    }

    private static string Pick(string ar, string en, string lang) => Normalize(lang) == Ar ? ar : en;

    // ------------------------------------------------------------------ tables (public for tests)
    public static readonly IReadOnlyDictionary<string, (string Ar, string En)> ErrorTable =
        new Dictionary<string, (string Ar, string En)>
        {
            [Err.ReportNotFound] = ("البلاغ غير موجود.", "Report not found."),
            [Err.ItemNotFound] = ("الحاجة غير موجودة.", "Item not found."),
            [Err.MatchNotFound] = ("التطابق غير موجود.", "Match not found."),
            [Err.ClaimNotFound] = ("الطلب غير موجود.", "Claim not found."),
            [Err.UserNotFound] = ("المستخدم غير موجود.", "User not found."),
            [Err.StationNotFound] = ("المحطة غير موجودة.", "Station not found."),
            [Err.FoundStationNotFound] = ("محطة اللقاء غير موجودة.", "Found-at station not found."),
            [Err.TargetStationNotFound] = ("المحطة المطلوب النقل إليها غير موجودة.", "Target station not found."),
            [Err.NotificationNotFound] = ("الإشعار غير موجود.", "Notification not found."),
            [Err.LineNotFound] = ("الخط غير موجود.", "Metro line not found."),

            [Err.NotYourReport] = ("ده مش بلاغك.", "Not your report."),
            [Err.NotYourNotification] = ("ده مش إشعارك.", "Not your notification."),
            [Err.NotYourMatch] = ("التطابق ده مش خاص ببلاغك.", "This match is not for your report."),
            [Err.WrongStation] = ("الحاجة محفوظة في محطة تانية.", "The item is held at a different station."),
            [Err.EmployeeRequired] = ("الإجراء ده يحتاج موظف نشط تابع لمحطة.", "This action requires an active employee assigned to a station."),
            [Err.AccountDisabled] = ("الحساب معطّل.", "Account is disabled."),
            [Err.InvalidCredentials] = ("اسم المستخدم أو الإيميل أو كلمة المرور غير صحيحة.", "Invalid username/email or password."),
            [Err.InvalidToken] = ("التوكن غير صالح.", "Invalid token."),
            [Err.CurrentPasswordWrong] = ("كلمة المرور الحالية غير صحيحة.", "Current password is incorrect."),

            [Err.UsernameTaken] = ("اسم المستخدم مستخدم قبل كده.", "Username is already taken."),
            [Err.EmailTaken] = ("الإيميل مسجّل قبل كده.", "Email is already registered."),
            [Err.UserExists] = ("اسم المستخدم أو الإيميل مسجّل قبل كده.", "Username or email is already registered."),
            [Err.ConcurrencyConflict] = ("حد تاني عدّل نفس البيانات في نفس اللحظة. حدّث الصفحة وحاول تاني.", "Someone else changed the same data at the same time. Refresh and try again."),

            [Err.PasswordSame] = ("كلمة المرور الجديدة لازم تختلف عن الحالية.", "New password must be different from the current one."),
            [Err.PasswordWeak] = ("كلمة المرور لازم تكون 8 حروف على الأقل وتحتوي على حرف ورقم.", "Password must be at least 8 characters and contain a letter and a digit."),
            [Err.ConfirmNeedsSuggested] = ("التطابق المقترح بس هو اللي ينفع يتأكد.", "Only suggested matches can be confirmed."),
            [Err.RejectNeedsSuggested] = ("التطابق المقترح بس هو اللي ينفع يترفض.", "Only suggested matches can be rejected."),
            [Err.ReportNotOpen] = ("البلاغ مش مفتوح (عنده تطابق متأكد بالفعل أو مغلق).", "This report is not open (it already has a confirmed match, or it is closed)."),
            [Err.ItemNotInStorage] = ("الحاجة مبقتش في المخزن.", "The item is no longer in storage."),
            [Err.LostDateFuture] = ("تاريخ الفقد مينفعش يكون في المستقبل.", "Lost date cannot be in the future."),
            [Err.ReportAlreadyClosed] = ("البلاغ مغلق بالفعل.", "Report is already closed."),
            [Err.StationNoItems] = ("محطتك مش بتستقبل المفقودات.", "Your station does not accept lost items."),
            [Err.FoundTimeFuture] = ("وقت اللقاء مينفعش يكون في المستقبل.", "Found time cannot be in the future."),
            [Err.TransferNeedsStorage] = ("الحاجات اللي في المخزن بس هي اللي ينفع تتنقل.", "Only items in storage can be transferred."),
            [Err.TargetNoItems] = ("المحطة المطلوب النقل إليها مش بتستقبل المفقودات.", "Target station does not accept items."),
            [Err.ItemAlreadyThere] = ("الحاجة موجودة في المحطة دي بالفعل.", "Item is already there."),
            [Err.MatchNotConfirmed] = ("لازم موظف المحطة يؤكد التطابق الأول.", "The match must be confirmed by station staff first."),
            [Err.ClaimExists] = ("فيه طلب استلام موجود بالفعل للتطابق ده.", "A claim already exists for this match."),
            [Err.ClaimNotPending] = ("الطلبات قيد المراجعة بس هي اللي ينفع تتراجع.", "Only pending claims can be verified."),
            [Err.ApproveNeedsChecks] = ("مينفعش تقبل الطلب قبل فحص الهوية والتأكد من الإجابات المميزة.", "Cannot approve before checking ID and the secret answer."),
            [Err.ClaimNotVerified] = ("الطلبات اللي تم التحقق منها بس هي اللي ينفع تتسلّم.", "Only verified claims can be handed over."),
        };

    public static readonly IReadOnlyDictionary<string, (string TitleAr, string TitleEn, string BodyAr, string BodyEn)> NotificationTable =
        new Dictionary<string, (string, string, string, string)>
        {
            [NotifCode.MatchFoundForReport] = ("تطابق محتمل مع حاجتك المفقودة", "Possible match for your lost item",
                "فيه حاجة شبه اللي بلّغت عنها اتلاقت. شوف التطابقات.", "An item similar to your report was found. Check your matches."),
            [NotifCode.MatchFoundForItem] = ("تطابق محتمل مع حاجتك المفقودة", "Possible match for your lost item",
                "حاجة جديدة اتسجلت شبه بلاغك. شوف التطابقات.", "A newly found item looks like your report. Check your matches."),
            [NotifCode.MatchConfirmed] = ("حاجتك جاهزة للاستلام", "Your item is ready to claim",
                "روح {0} ومعاك إثبات شخصية عشان تستلمها.", "Please visit {0} with your ID to claim it."),
            [NotifCode.ClaimApproved] = ("تم قبول طلبك", "Claim approved",
                "تم التحقق من ملكيتك. تقدر تستلم حاجتك.", "Your ownership was verified. You can collect your item."),
            [NotifCode.ClaimRejected] = ("تم رفض طلبك", "Claim rejected",
                "مقدرناش نتحقق من ملكيتك للحاجة دي.", "We could not verify ownership of this item."),
            [NotifCode.ItemReturned] = ("تم تسليم الحاجة", "Item returned",
                "تم تسليم حاجتك وإغلاق البلاغ.", "Your item was handed over. This report is now closed."),
        };
}