using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Localization;
using Laqeeta.Core.Models;

namespace Laqeeta.Api.Web;

/// <summary>Tiny bilingual (Arabic / English) localizer. Language is stored in the "lang" cookie.</summary>
public interface ILoc
{
    string this[string key] { get; }
    string Lang { get; }
    bool IsRtl { get; }
    /// <summary>Localized name of an enum value.</summary>
    string Of<TEnum>(TEnum value) where TEnum : struct, Enum;
    /// <summary>UTC date-time -> local time text.</summary>
    string Dt(DateTime? utc);
    /// <summary>Business error (Err code) in the current language.</summary>
    string Error(DomainException ex);
    /// <summary>Notification title / body in the current language (old rows without a code show as stored).</summary>
    string Title(Notification n);
    string Body(Notification n);
    /// <summary>Label of a form field by its property name (used in validation messages).</summary>
    string Field(string propertyName);
}

public class Loc(IHttpContextAccessor accessor) : ILoc
{
    public const string CookieName = "lang";

    public string Lang => accessor.HttpContext?.Request.Cookies[CookieName] == "en" ? "en" : "ar";
    public bool IsRtl => Lang == "ar";

    public string this[string key]
        => Text.TryGetValue(key, out var t) ? (IsRtl ? t.Ar : t.En) : key;

    public string Of<TEnum>(TEnum value) where TEnum : struct, Enum
        => this[$"{typeof(TEnum).Name}.{value}"];

    public string Dt(DateTime? utc)
        => utc is null ? "—" : utc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

    public string Error(DomainException ex) => Messages.Error(ex, Lang);
    public string Title(Notification n) => Messages.Title(n.Code, n.Title, Lang);
    public string Body(Notification n) => Messages.Body(n.Code, n.Message, n.Param, Lang);
    public string Field(string propertyName)
        => Text.ContainsKey($"field.{propertyName}") ? this[$"field.{propertyName}"] : propertyName;

    private static readonly Dictionary<string, (string Ar, string En)> Text = new()
    {
        // ---- validation messages (ErrorMessage keys on the view models)
        ["val.required"] = ("مطلوب.", "is required."),
        ["val.length"] = ("طوله غير مناسب.", "has an invalid length."),
        ["val.passwordLength"] = ("لازم يكون 8 حروف على الأقل.", "must be at least 8 characters."),
        ["val.username"] = ("مسموح فيه حروف إنجليزية وأرقام و . _ - فقط.", "may contain letters, digits and . _ - only."),
        ["val.email"] = ("مش إيميل صحيح.", "is not a valid email address."),
        ["val.phone"] = ("مش رقم صحيح (أرقام فقط من 8 لـ 15 رقم).", "is not a valid phone number (8-15 digits)."),
        ["val.compare"] = ("مش متطابق مع كلمة المرور.", "does not match the password."),
        ["val.select"] = ("لازم تختاره.", "must be selected."),

        // ---- field labels
        ["field.FullName"] = ("الاسم بالكامل", "Full name"),
        ["field.Username"] = ("اسم المستخدم", "Username"),
        ["field.Email"] = ("الإيميل", "Email"),
        ["field.Phone"] = ("رقم التليفون", "Phone"),
        ["field.Password"] = ("كلمة المرور", "Password"),
        ["field.ConfirmPassword"] = ("تأكيد كلمة المرور", "Confirm password"),
        ["field.UsernameOrEmail"] = ("اسم المستخدم أو الإيميل", "Username or email"),
        ["field.CurrentPassword"] = ("كلمة المرور الحالية", "Current password"),
        ["field.NewPassword"] = ("كلمة المرور الجديدة", "New password"),
        ["field.ConfirmNewPassword"] = ("تأكيد كلمة المرور الجديدة", "Confirm new password"),
        ["field.LineId"] = ("الخط", "Metro line"),
        ["field.ContactPhone"] = ("تليفون للتواصل", "Contact phone"),
        ["field.Color"] = ("اللون", "Color"),
        ["field.LostDate"] = ("تاريخ الفقد", "Date lost"),
        ["field.Direction"] = ("الاتجاه", "Direction"),
        ["field.Description"] = ("الوصف", "Description"),
        ["field.FoundStationId"] = ("محطة اللقاء", "Found at station"),
        ["field.FoundAt"] = ("وقت اللقاء", "Found at"),
        ["field.PrivateIdentifyingDetails"] = ("تفاصيل خاصة", "Private details"),
        ["field.ToStationId"] = ("المحطة الجديدة", "Destination station"),
        ["field.Notes"] = ("ملاحظات", "Notes"),

        // ---- app / nav
        ["app.name"] = ("لقيتها", "Laqeeta"),
        ["app.tagline"] = ("نظام المفقودات والموجودات بالمترو", "Metro Lost & Found"),
        ["nav.myReports"] = ("بلاغاتي", "My reports"),
        ["nav.newReport"] = ("بلاغ جديد", "New report"),
        ["nav.dashboard"] = ("لوحة التحكم", "Dashboard"),
        ["nav.items"] = ("الحاجات الموجودة", "Found items"),
        ["nav.matches"] = ("التطابقات", "Matches"),
        ["nav.claims"] = ("طلبات الاستلام", "Claims"),
        ["nav.notifications"] = ("الإشعارات", "Notifications"),
        ["nav.profile"] = ("حسابي", "My account"),
        ["nav.logout"] = ("تسجيل الخروج", "Log out"),
        ["nav.addEmployee"] = ("إضافة موظف", "Add employee"),

        // ---- add employee
        ["employee.title"] = ("إضافة موظف جديد", "New employee"),
        ["employee.hint"] = ("الموظف هيقدر يسجّل الحاجات الموجودة ويراجع التطابقات والطلبات في محطته.",
                             "The employee can log found items and review matches and claims at their station."),
        ["employee.station"] = ("المحطة اللي هيشتغل فيها", "Works at station"),
        ["employee.noItems"] = ("لا تستقبل المفقودات", "does not accept items"),
        ["employee.create"] = ("إنشاء الحساب", "Create account"),
        ["msg.employeeCreated"] = ("تم إنشاء حساب الموظف {0}.", "Employee account {0} was created."),
        ["field.StationId"] = ("المحطة", "Station"),

        // ---- common
        ["common.all"] = ("الكل", "All"),
        ["common.cancel"] = ("إلغاء", "Cancel"),
        ["common.save"] = ("حفظ", "Save"),
        ["common.details"] = ("التفاصيل", "Details"),
        ["common.home"] = ("الصفحة الرئيسية", "Home"),
        ["common.optional"] = ("اختياري", "optional"),
        ["common.select"] = ("اختر", "Select"),
        ["common.status"] = ("الحالة", "Status"),
        ["common.created"] = ("تاريخ الإنشاء", "Created"),

        // ---- home
        ["home.title"] = ("ضاع منك حاجة في المترو؟", "Lost something on the metro?"),
        ["home.subtitle"] = ("بلّغ عن الحاجة اللي ضاعت، ولما تتسجل في أي محطة هنقولك فورًا ونساعدك تستلمها.",
                             "Report what you lost. When it is logged at any station, we notify you right away and help you collect it."),
        ["home.start"] = ("ابدأ دلوقتي", "Get started"),
        ["home.step1.title"] = ("بلّغ", "Report"),
        ["home.step1.text"] = ("اكتب وصف الحاجة والخط والتاريخ.", "Describe the item, the line and the date."),
        ["home.step2.title"] = ("نطابق", "We match"),
        ["home.step2.text"] = ("النظام بيقارن بلاغك بالحاجات اللي لقاها موظفو المحطات.", "We compare your report with items found by station staff."),
        ["home.step3.title"] = ("استلم", "Collect"),
        ["home.step3.text"] = ("اثبت ملكيتك واستلم حاجتك من المحطة.", "Prove ownership and pick your item up at the station."),

        // ---- auth
        ["auth.login"] = ("تسجيل الدخول", "Log in"),
        ["auth.register"] = ("إنشاء حساب", "Sign up"),
        ["auth.usernameOrEmail"] = ("اسم المستخدم أو الإيميل", "Username or email"),
        ["auth.username"] = ("اسم المستخدم", "Username"),
        ["auth.fullName"] = ("الاسم بالكامل", "Full name"),
        ["auth.email"] = ("الإيميل", "Email"),
        ["auth.phone"] = ("رقم التليفون", "Phone"),
        ["auth.password"] = ("كلمة المرور", "Password"),
        ["auth.confirmPassword"] = ("تأكيد كلمة المرور", "Confirm password"),
        ["auth.currentPassword"] = ("كلمة المرور الحالية", "Current password"),
        ["auth.newPassword"] = ("كلمة المرور الجديدة", "New password"),
        ["auth.changePassword"] = ("تغيير كلمة المرور", "Change password"),
        ["auth.passwordHint"] = ("8 حروف على الأقل، وتحتوي على حرف ورقم.", "At least 8 characters, with a letter and a digit."),
        ["auth.remember"] = ("تذكرني", "Remember me"),
        ["auth.noAccount"] = ("معندكش حساب؟", "No account?"),
        ["auth.haveAccount"] = ("عندك حساب؟", "Already have an account?"),
        ["auth.role"] = ("نوع الحساب", "Account type"),
        ["denied.title"] = ("غير مسموح", "Access denied"),
        ["denied.text"] = ("مفيش عندك صلاحية لفتح الصفحة دي.", "You do not have permission to open this page."),
        ["error.title"] = ("حصل خطأ", "Something went wrong"),
        ["error.text"] = ("حاول تاني، ولو المشكلة فضلت كلّم الدعم.", "Please try again. If the problem continues, contact support."),

        // ---- passenger reports
        ["report.title"] = ("بلاغ", "Report"),
        ["report.empty"] = ("لسه معملتش أي بلاغ.", "You have not reported anything yet."),
        ["report.line"] = ("الخط", "Metro line"),
        ["report.category"] = ("النوع", "Category"),
        ["report.color"] = ("اللون", "Color"),
        ["report.lostDate"] = ("تاريخ الفقد", "Date lost"),
        ["report.time"] = ("الوقت التقريبي", "Approx. time"),
        ["report.direction"] = ("الاتجاه", "Direction"),
        ["report.contactPhone"] = ("تليفون للتواصل", "Contact phone"),
        ["report.description"] = ("الوصف", "Description"),
        ["report.descriptionHint"] = ("اكتب علامات مميزة تساعد في التعرف على الحاجة (من غير بيانات سرية).", "Add distinguishing marks that help identify the item (no secret data)."),
        ["report.submit"] = ("إرسال البلاغ", "Submit report"),
        ["report.cancel"] = ("إلغاء البلاغ", "Cancel report"),
        ["report.cancelConfirm"] = ("هل أنت متأكد من إلغاء البلاغ؟", "Cancel this report?"),
        ["report.closureReason"] = ("سبب الإغلاق", "Closure reason"),

        // ---- matches / claims
        ["match.title"] = ("التطابقات", "Matches"),
        ["match.none"] = ("مفيش تطابقات لحد دلوقتي. هنبلغك أول ما تظهر حاجة مناسبة.", "No matches yet. We will notify you as soon as something fits."),
        ["match.waiting"] = ("في انتظار تأكيد موظف المحطة.", "Waiting for the station employee to confirm."),
        ["match.empty"] = ("مفيش تطابقات في الحالة دي.", "No matches in this state."),
        ["match.report"] = ("البلاغ", "Report"),
        ["match.item"] = ("الحاجة", "Item"),
        ["match.confirm"] = ("تأكيد", "Confirm"),
        ["match.reject"] = ("رفض", "Reject"),
        ["claim.status"] = ("حالة الطلب", "Claim status"),
        ["claim.submit"] = ("دي حاجتي - قدّم طلب استلام", "It is mine - submit a claim"),
        ["claim.confirmedHint"] = ("الموظف أكد التطابق. لو دي حاجتك قدّم طلب استلام.", "The employee confirmed the match. If it is yours, submit a claim."),
        ["claim.pendingHint"] = ("طلبك قيد المراجعة من موظف المحطة.", "Your claim is being reviewed by the station employee."),
        ["claim.collectHint"] = ("تم التحقق. تقدر تستلم حاجتك من محطة", "Verified. You can collect your item at station"),
        ["claim.empty"] = ("مفيش طلبات.", "No claims."),
        ["claim.claimant"] = ("صاحب الطلب", "Claimant"),
        ["claim.claimedAt"] = ("وقت الطلب", "Claimed at"),
        ["claim.verify"] = ("مراجعة الطلب", "Verify claim"),
        ["claim.checklist"] = ("خطوات التحقق", "Verification checklist"),
        ["claim.idChecked"] = ("تم فحص إثبات الشخصية", "ID checked"),
        ["claim.secretPassed"] = ("أجاب على الأسئلة المميزة صح", "Answered the identifying questions correctly"),
        ["claim.notes"] = ("ملاحظات", "Notes"),
        ["claim.approve"] = ("قبول", "Approve"),
        ["claim.reject"] = ("رفض", "Reject"),
        ["claim.handOver"] = ("تسليم", "Hand over"),
        ["claim.handOverConfirm"] = ("تأكيد تسليم الحاجة لصاحبها؟", "Confirm handing the item over?"),

        // ---- notifications
        ["notif.empty"] = ("مفيش إشعارات.", "No notifications."),
        ["notif.unread"] = ("غير المقروءة", "Unread"),
        ["notif.markAll"] = ("تعليم الكل كمقروء", "Mark all as read"),
        ["notif.markRead"] = ("تم القراءة", "Mark read"),
        ["notif.openReport"] = ("فتح البلاغ", "Open report"),

        // ---- staff
        ["staff.station"] = ("المحطة", "Station"),
        ["staff.inStorage"] = ("حاجات في المخزن", "Items in storage"),
        ["staff.suggested"] = ("تطابقات مقترحة", "Suggested matches"),
        ["staff.pending"] = ("طلبات قيد المراجعة", "Pending claims"),
        ["staff.ready"] = ("جاهزة للتسليم", "Ready for hand-over"),
        ["items.title"] = ("حاجة", "Item"),
        ["items.log"] = ("تسجيل حاجة موجودة", "Log found item"),
        ["items.empty"] = ("مفيش حاجات.", "No items."),
        ["items.heldHere"] = ("الحاجة هتتحفظ في محطتك.", "The item will be held at your station."),
        ["items.foundStation"] = ("محطة اللقاء", "Found at station"),
        ["items.currentStation"] = ("محفوظة في", "Held at"),
        ["items.foundAt"] = ("وقت اللقاء", "Found at"),
        ["items.loggedAt"] = ("وقت التسجيل", "Logged at"),
        ["items.returnedAt"] = ("وقت التسليم", "Returned at"),
        ["items.publicDescription"] = ("الوصف العام", "Public description"),
        ["items.privateDetails"] = ("تفاصيل خاصة", "Private details"),
        ["items.privateHint"] = ("لا تظهر للمستخدمين؛ تُستخدم للتحقق من الملكية فقط.", "Never shown to passengers; used only to verify ownership."),
        ["items.transfer"] = ("نقل لمحطة تانية", "Transfer to another station"),
        ["items.toStation"] = ("المحطة الجديدة", "Destination station"),

        // ---- flash messages
        ["msg.welcome"] = ("أهلًا بيك! تم إنشاء حسابك.", "Welcome! Your account was created."),
        ["msg.passwordChanged"] = ("تم تغيير كلمة المرور.", "Password changed."),
        ["msg.reportCreated"] = ("تم إرسال البلاغ.", "Report submitted."),
        ["msg.reportCancelled"] = ("تم إلغاء البلاغ.", "Report cancelled."),
        ["msg.claimSubmitted"] = ("تم إرسال طلب الاستلام.", "Claim submitted."),
        ["msg.allRead"] = ("تم تعليم كل الإشعارات كمقروءة.", "All notifications marked as read."),
        ["msg.itemLogged"] = ("تم تسجيل الحاجة.", "Item logged."),
        ["msg.itemTransferred"] = ("تم نقل الحاجة.", "Item transferred."),
        ["msg.matchConfirmed"] = ("تم تأكيد التطابق.", "Match confirmed."),
        ["msg.matchRejected"] = ("تم رفض التطابق.", "Match rejected."),
        ["msg.claimApproved"] = ("تم قبول الطلب.", "Claim approved."),
        ["msg.claimRejected"] = ("تم رفض الطلب.", "Claim rejected."),
        ["msg.handedOver"] = ("تم تسليم الحاجة.", "Item handed over."),
        ["msg.claimNotFound"] = ("الطلب غير موجود في محطتك.", "Claim not found at your station."),
        ["msg.invalidInput"] = ("بيانات غير صالحة.", "Invalid input."),

        // ---- enums
        ["UserRole.User"] = ("مستخدم", "Passenger"),
        ["UserRole.Employee"] = ("موظف", "Employee"),
        ["ItemCategory.Phone"] = ("هاتف", "Phone"),
        ["ItemCategory.Wallet"] = ("محفظة", "Wallet"),
        ["ItemCategory.Bag"] = ("حقيبة", "Bag"),
        ["ItemCategory.Keys"] = ("مفاتيح", "Keys"),
        ["ItemCategory.Documents"] = ("مستندات", "Documents"),
        ["ItemCategory.Electronics"] = ("إلكترونيات", "Electronics"),
        ["ItemCategory.Clothing"] = ("ملابس", "Clothing"),
        ["ItemCategory.Jewelry"] = ("مجوهرات", "Jewelry"),
        ["ItemCategory.Glasses"] = ("نظارة", "Glasses"),
        ["ItemCategory.Other"] = ("أخرى", "Other"),
        ["LostReportStatus.Open"] = ("مفتوح", "Open"),
        ["LostReportStatus.Matched"] = ("تم التطابق", "Matched"),
        ["LostReportStatus.Closed"] = ("مغلق", "Closed"),
        ["ClosureReason.Returned"] = ("تم التسليم", "Returned"),
        ["ClosureReason.CancelledByUser"] = ("ألغاه المستخدم", "Cancelled by user"),
        ["ClosureReason.Expired"] = ("انتهت صلاحيته", "Expired"),
        ["ClosureReason.Duplicate"] = ("مكرر", "Duplicate"),
        ["FoundItemStatus.InStorage"] = ("في المخزن", "In storage"),
        ["FoundItemStatus.Matched"] = ("محجوزة", "Reserved"),
        ["FoundItemStatus.Returned"] = ("تم تسليمها", "Returned"),
        ["FoundItemStatus.Transferred"] = ("منقولة", "Transferred"),
        ["FoundItemStatus.Disposed"] = ("تم التخلص منها", "Disposed"),
        ["MatchStatus.Suggested"] = ("مقترح", "Suggested"),
        ["MatchStatus.Confirmed"] = ("مؤكد", "Confirmed"),
        ["MatchStatus.Rejected"] = ("مرفوض", "Rejected"),
        ["ClaimStatus.Pending"] = ("قيد المراجعة", "Pending"),
        ["ClaimStatus.Verified"] = ("تم التحقق", "Verified"),
        ["ClaimStatus.Rejected"] = ("مرفوض", "Rejected"),
        ["ClaimStatus.HandedOver"] = ("تم التسليم", "Handed over"),
        ["NotificationType.MatchFound"] = ("تطابق جديد", "Match found"),
        ["NotificationType.ClaimUpdate"] = ("تحديث الطلب", "Claim update"),
        ["NotificationType.ItemReturned"] = ("تم تسليم الحاجة", "Item returned"),
        ["NotificationType.ReportClosed"] = ("إغلاق البلاغ", "Report closed"),
        ["NotificationType.System"] = ("النظام", "System"),
    };
}
