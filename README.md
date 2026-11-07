# 🔍 Laqeeta | لقيطة 

[![Flutter](https://img.shields.io/badge/Framework-Flutter-02569B?logo=flutter)](https://flutter.dev/)
[![Firebase](https://img.shields.io/badge/Backend-Firebase-FFCA28?logo=firebase)](https://firebase.google.com/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

**لقيطة (Laqeeta)** هو تطبيق جوال ذكي ومجتمعي يهدف إلى الربط بين الأشخاص الذين فقدوا ممتلكاتهم أو وثائقهم الهامة، وبين من عثروا عليها. يعتمد التطبيق على خوارزميات البحث الذكية والنطاق الجغرافي لتسهيل واسترجاع المفقودات بأسرع وقت ممكن وبأعلى قدر من الأمان والخصوصية.

---

## 🌟 الميزات الرئيسية (Key Features)

- **📝 نشر المفقودات والموجودات (Report Lost/Found):** إمكانية رفع تفاصيل العنصر المفقود/الموجود مع الصور، الوصف، والموقع الجغرافي.
- **🗺️ البحث القائم على الموقع (Location-Based Search):** استعراض المفقودات بناءً على الخريطة الجغرافية أو تحديد نطاق المسافة.
- **🔔 التنبيهات الذكية (Smart Notifications):** إرسال إشعارات فورية للمستخدمين عند إدراج غرض يطابق مواصفات ما فقدوه قريبًا منهم.
- **🔍 نظام مطابقة ذكي (Smart Matching System):** مطابقة تلقائية بين البلاغات المتقاربة في الوصف والموقع.
- **💬 المحادثات الفورية (In-App Chat):** إمكانية التواصل الأمني المباشر بين العاثر وصاحب الغرض المفقود دون الحاجة لمشاركة أرقام الهواتف الشخصية.
- **🏷️ التجميع حسب الفئات (Categories):** تصنيف الأشياء (إلكترونيات، وثائق ومستندات، مفاتيح، حيوانات أليفة، أمتعة... إلخ).
- **🛡️ الخصوصية والأمان:** إخفاء البيانات الشخصية للحسابات وحمايتها حتى يتم التأكد من هوية صاحب المفقودات عبر أسئلة تأكيدية.

---

## 🛠️ التكنولوجيا المستخدمة (Tech Stack)

### Frontend (تطبيق الجوال)
- **[Flutter](https://flutter.dev/)** - إطار عمل لبناء التطبيق ليعمل على نظامي Android و iOS بنفس الكود.
- **[Dart](https://dart.dev/)** - لغة البرمجة الأساسية لتطوير التطبيق.

### Backend & Cloud Services (الخدمات السحابية)
- **[Firebase Authentication](https://firebase.google.com/docs/auth)** - إدارة تسجيل الدخول (بريد إلكتروني، Google، ورقم الهاتف).
- **[Cloud Firestore](https://firebase.google.com/docs/firestore)** - قاعدة بيانات غير علائقية (NoSQL) لحظية لتخزين بيانات المفقودات والمحادثات.
- **[Firebase Storage](https://firebase.google.com/docs/storage)** - لتخزين الصور والمرفقات الخاصة بالبلاغات.
- **[Firebase Cloud Messaging (FCM)](https://firebase.google.com/docs/cloud-messaging)** - لإرسال الإشعارات التنبيهية الفورية.

---

## 📦 المكتبات والباكجات المستخدمة (Packages & Dependencies)

فيما يلي أهم المكتبات الحالية والمقترحة في ملف `pubspec.yaml`:

| Package | الوصف / الاستخدام |
| :--- | :--- |
| `flutter_bloc` / `provider` | إدارة حالة التطبيق (State Management). |
| `firebase_core` & `cloud_firestore` | الربط بقواعد بيانات Firebase والخدمات السحابية. |
| `firebase_auth` | التوثيق وإدارة الحسابات وتسجيل الدخول. |
| `google_maps_flutter` | عرض خرائط جوجل وتحديد مواقع المفقودات والموجودات. |
| `geolocator` & `geocoding` | الحصول على الموقع الجغرافي الحالي وتحويل الإحداثيات إلى عناوين. |
| `image_picker` | التقاط الصور من الكاميرا أو اختيارها من معرض الصور عند إضافة بلاغ. |
| `cached_network_image` | تحميل الصور وعرضها بكفاءة مع خاصية التخزين المؤقت (Caching). |
| `flutter_local_notifications` | إدارة الإشعارات المحلية على جهاز المستخدم. |
| `intl` | دعم تنسيق التواريخ والوقت واللغات (Multi-language Support). |
| `http` / `dio` | لإجراء طلبات الـ API والاتصالات الخارجية عند الحاجة. |

---

## 🚀 كيفية تشغيل المشروع محلياً (Getting Started)

### المتطلبات الأساسية (Prerequisites)
- تثبيت [Flutter SDK](https://docs.flutter.dev/get-started/install) (الإصدار stable).
- تثبيت [Android Studio](https://developer.android.com/studio) أو [VS Code](https://code.visualstudio.com/).
- محاكي (Emulator) أو جهاز فرضي/حقيقي للاختبار.

### خطوات التشغيل:

1. **استنساخ المستودع (Clone the repository):**
   ```bash
   git clone https.github.com/AliElbehery/Laqeeta.git
   cd Laqeeta
   ```

2. **تثبيت الباكجات (Install dependencies):**
   ```bash
   flutter pub get
   ```

3. **إعداد Firebase:**
   - قم بإنشاء مشروع جديد على [Firebase Console](https://console.firebase.google.com/).
   - أضف تطبيقي Android و iOS وحمّل ملفات الإعداد (`google-services.json` لـ Android و `GoogleService-Info.plist` لـ iOS).
   - ضع الملفات في المسارات المخصصة لها داخل المشروع (`android/app/` و `ios/Runner/`).

4. **تشغيل التطبيق (Run the app):**
   ```bash
   flutter run
   ```

---

## 📁 هيكلية المشروع (Project Structure)

```text
lib/
├── core/                   # الأكواد والخدمات المشتركة (Theme, Utils, Constants)
├── data/                   # الموديلات (Models) والمزودات (Repositories/Providers)
├── logic/                  # إدارة الحالات (Cubit / BLoC / Controllers)
├── presentation/           # واجهات المستخدم (Screens & Widgets)
│   ├── screens/            # الشاشات الأساسية (Home, Details, Add Item, Profile)
│   └── widgets/            # العناصر المكونة للواجهة (Custom Buttons, Cards)
└── main.dart               # نقطة الانطلاق الأساسية للتطبيق
```

---

## 🤝 المساهمة (Contributing)

المساهمات والتطويرات مرحب بها دائماً! 
1. قم بعمل Fork للمشروع.
2. أنشئ فرع جديد للميزة (`git checkout -b feature/AmazingFeature`).
3. احفظ التغييرات (`git commit -m 'Add some AmazingFeature'`).
4. ارفع الفرع (`git push origin feature/AmazingFeature`).
5. افتح Pull Request.

---

## 📄 الترخيص (License)

هذا المشروع مرخص بموجب رخصة **MIT** - راجع ملف [LICENSE](LICENSE) لمزيد من التفاصيل.
