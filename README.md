# Rastix

**Rastix** is an unofficial Windows Terminal-based project focused on improving the display of Persian and other right-to-left (RTL) text in command-line applications. It enables bidirectional text support by default and provides a separate `rx` launch alias.

Rastix is not an official Microsoft product or release. RTL rendering and editing behavior can vary depending on the application, font, input method, and text being displayed; this project does not promise to solve every shaping, ordering, or editing issue.

## Features

- Bidirectional text rendering is enabled by default.
- Rastix is branded independently and provides an `rx` executable/command without replacing Windows Terminal's `wt` alias.
- The default PowerShell and Command Prompt profiles start in the current working directory. In File Explorer, enter `rx` in the address bar to open Rastix in the displayed folder.
- Packaged development builds include **Open in Rastix here** in the Explorer context menu. The setup EXE and portable ZIP do not register this menu entry.

## Download and install

When the `rastix-v0.1.1` release is published, download **Rastix-Setup-v0.1.1.exe** from [GitHub Releases](https://github.com/Tiredb0y/Rastix/releases). The setup installs the unpackaged portable layout for the current user under `%LOCALAPPDATA%\Programs\Rastix\0.1.1` and creates a Start menu shortcut. It does not register an MSIX, require Developer Mode, elevate, or install/trust a certificate. Settings are stored in the installation's `settings` folder; back up that folder before uninstalling. Use the **Uninstall Rastix** Start menu shortcut to remove the app, its settings folder, and the PATH entry. This release does not yet support in-place upgrades; uninstall an older version before installing a newer one.

A **Rastix-Portable-v0.1.1.zip** is planned for the same release; extract it and run `WindowsTerminal.exe`. Its `.portable` marker makes settings stay beside the app. The installer also adds its folder to your user `PATH`, so new terminals can launch `rx` from any directory; restart existing terminals after installation. If you remove the install manually, also remove its `%LOCALAPPDATA%\Programs\Rastix\0.1.1` entry from your user PATH.

Both the setup EXE and portable ZIP are **unsigned**. Windows SmartScreen may show an **Unknown publisher** warning, and reputation-based protection may block or warn about first-time downloads. No fake or self-signed certificate is used. This is separate from the prior unsigned MSIX installation issue: the public release no longer asks Windows to trust/register an MSIX publisher. Do not disable SmartScreen or security protections just to install Rastix; only continue if you trust the source and have verified the release hashes. To compare the published SHA-256 values in PowerShell, run this from the folder containing the downloaded files and `SHA256SUMS.txt`:

```powershell
Get-FileHash .\Rastix-Setup-v0.1.1.exe, .\Rastix-Portable-v0.1.1.zip -Algorithm SHA256
Get-Content .\SHA256SUMS.txt
```

**Requirements:** Windows 10 version 2004 (build 19041) or later, x64. The EXE is an installer, not a signed or self-updating package.

## Persian text and PowerShell

RTL display, Arabic/Persian letter shaping, cursor movement, selection, and command-line editing are related but distinct behaviors. Results may differ across shell applications and input methods. Windows PowerShell 5.1 may also use a legacy code page. To use UTF-8 in that shell, you can configure its profile or the relevant Terminal profile to run:

```powershell
$utf8 = [System.Text.UTF8Encoding]::new($false)
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8
```

This changes text encoding, not text direction. Do not weaken your system's PowerShell execution policy just to apply this setting.

## Building

GitHub Actions builds an unsigned package internally as an input to the repository's supported unpackaged-layout generation script, then packages that layout as a setup EXE and portable ZIP. The unsigned MSIX is not published. Tags matching `rastix-v*` trigger builds and attach the EXE, ZIP, and SHA-256 sums to the GitHub Release. See [workflow runs](https://github.com/Tiredb0y/Rastix/actions/workflows/rtl-build.yml) for CI results. For local builds, see the upstream [Windows Terminal build guide](https://github.com/microsoft/terminal/blob/main/doc/building.md).

## Reporting issues

When reporting an issue, include your Windows version, the application producing the text, the font, a copyable example if possible, and whether the problem concerns RTL ordering, letter shaping, input/editing, or encoding. Do not post passwords, access tokens, or other private information.

## Project and license

Rastix is based on [Microsoft's Windows Terminal](https://github.com/microsoft/terminal). It is an independent, unofficial project; upstream names and trademarks remain the property of their respective owners. See [LICENSE](LICENSE) and [NOTICE.md](NOTICE.md) for licensing and attribution details.

---

# راستیکس

**راستیکس** پروژه‌ای غیررسمی و مبتنی بر Windows Terminal است که بر بهبود نمایش متن فارسی و دیگر متن‌های راست‌به‌چپ (RTL) در برنامه‌های خط فرمان تمرکز دارد. پشتیبانی از متن دوجهته به‌طور پیش‌فرض فعال است و برنامه با نام فرمان جداگانهٔ `rx` اجرا می‌شود.

راستیکس محصول یا انتشار رسمی Microsoft نیست. نمایش و ویرایش متن RTL ممکن است بسته به برنامه، فونت، روش ورود متن و محتوای متن متفاوت باشد؛ این پروژه تضمین نمی‌کند که همهٔ مشکلات شکل‌دهی حروف، ترتیب متن یا ویرایش را برطرف کند.

## ویژگی‌ها

- نمایش دوجهتهٔ متن به‌طور پیش‌فرض فعال است.
- راستیکس با نام مستقل و فرمان `rx` ارائه می‌شود و نام `wt` در Windows Terminal رسمی را جایگزین نمی‌کند.
- پروفایل‌های پیش‌فرض PowerShell و Command Prompt از پوشهٔ جاری شروع می‌شوند. در نوار نشانی File Explorer بنویسید `rx` تا برنامه در پوشهٔ نمایش‌داده‌شده باز شود.
- گزینهٔ **Open in Rastix here** در منوی زمینهٔ File Explorer مربوط به ساخت توسعهٔ packaged است؛ نصب‌کنندهٔ EXE و ZIP قابل‌حمل این گزینه را ثبت نمی‌کنند.

## دریافت و نصب

پس از انتشار نسخهٔ `rastix-v0.1.1`، فایل **Rastix-Setup-v0.1.1.exe** را از [GitHub Releases](https://github.com/Tiredb0y/Rastix/releases) دریافت کنید. نصب‌کننده نسخهٔ unpackaged و قابل‌حمل برنامه را برای کاربر جاری در `%LOCALAPPDATA%\Programs\Rastix\0.1.1` نصب و میان‌بری در Start Menu ایجاد می‌کند. ثبت MSIX، Developer Mode، دسترسی مدیر یا نصب/اعتماد به گواهی لازم نیست. تنظیمات کنار برنامه در پوشهٔ `settings` نگه‌داری می‌شوند. پیش از حذف برنامه از تنظیمات پشتیبان بگیرید. میان‌بر **Uninstall Rastix** در Start Menu برنامه، پوشهٔ تنظیمات و ورودی PATH را حذف می‌کند. این نسخه ارتقای درجا را پشتیبانی نمی‌کند؛ برای نصب نسخهٔ جدید ابتدا نسخهٔ قبلی را حذف کنید.

فایل **Rastix-Portable-v0.1.1.zip** نیز برای همین انتشار در نظر گرفته شده است؛ آن را استخراج و `WindowsTerminal.exe` را اجرا کنید. نشانگر `.portable` تنظیمات را کنار برنامه نگه می‌دارد. نصب‌کننده مسیر برنامه را به `PATH` کاربر اضافه می‌کند تا در پنجره‌های ترمینال جدید، فرمان `rx` از هر پوشه‌ای اجرا شود؛ پس از نصب، پنجره‌های ترمینال باز را دوباره اجرا کنید. اگر نصب را دستی حذف می‌کنید، ورودی `%LOCALAPPDATA%\Programs\Rastix\0.1.1` را نیز از `PATH` کاربر پاک کنید.

نصب‌کننده و ZIP هر دو **امضانشده** هستند. Windows SmartScreen ممکن است هشدار **Unknown publisher** نمایش دهد یا بر اساس اعتبار دانلود هشدار/محدودیت ایجاد کند. هیچ گواهی جعلی یا خودامضایی استفاده نشده است. این موضوع با خطای نصب MSIX قبلی تفاوت دارد: انتشار جدید MSIX را برای نصب یا اعتماد به ناشر ثبت نمی‌کند. برای نصب Rastix، SmartScreen یا حفاظت‌های امنیتی را غیرفعال نکنید؛ فقط در صورتی ادامه دهید که به منبع اعتماد دارید و hashهای انتشار را بررسی کرده‌اید. برای مقایسهٔ hashهای SHA-256 در PowerShell، این فرمان‌ها را در پوشهٔ فایل‌های دریافت‌شده اجرا کنید:

```powershell
Get-FileHash .\Rastix-Setup-v0.1.1.exe, .\Rastix-Portable-v0.1.1.zip -Algorithm SHA256
Get-Content .\SHA256SUMS.txt
```

**پیش‌نیاز:** Windows 10 نسخهٔ 2004 (ساخت 19041) یا جدیدتر، x64. فایل EXE نصب‌کننده است، نه بستهٔ امضاشده یا دارای به‌روزرسانی خودکار.

## متن فارسی و PowerShell

نمایش RTL، شکل‌دهی حروف فارسی/عربی، حرکت مکان‌نما، انتخاب متن و ویرایش خط فرمان رفتارهای مرتبط اما جداگانه‌اند. نتیجه ممکن است در پوسته‌ها و روش‌های ورود متن مختلف فرق کند. Windows PowerShell 5.1 نیز ممکن است از code page قدیمی استفاده کند. برای فعال‌کردن UTF-8 در آن، می‌توانید در پروفایل PowerShell یا پروفایل مربوط در Terminal این دستورها را اجرا کنید:

```powershell
$utf8 = [System.Text.UTF8Encoding]::new($false)
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8
```

این تنظیم کدگذاری متن را تغییر می‌دهد، نه جهت نمایش آن. برای استفاده از این تنظیم، سیاست امنیتی اجرای PowerShell را تضعیف نکنید.

## ساخت

GitHub Actions یک بستهٔ بدون امضا را فقط به‌عنوان ورودی داخلی برای اسکریپت رسمی تولید چیدمان unpackaged می‌سازد، سپس آن را به فایل نصب EXE و ZIP قابل‌حمل تبدیل می‌کند؛ MSIX منتشر نمی‌شود. push کردن tagهایی با الگوی `rastix-v*` ساخت را اجرا کرده و EXE، ZIP و hashهای SHA-256 را به GitHub Release می‌افزاید. وضعیت CI در [اجرای workflow](https://github.com/Tiredb0y/Rastix/actions/workflows/rtl-build.yml) دیده می‌شود. برای ساخت محلی، [راهنمای ساخت Windows Terminal](https://github.com/microsoft/terminal/blob/main/doc/building.md) را ببینید.

## گزارش مشکل

هنگام گزارش مشکل، نسخهٔ ویندوز، برنامه‌ای که متن را تولید می‌کند، فونت، نمونه‌ای از متن (در صورت امکان قابل کپی)، و نوع مشکل—ترتیب RTL، شکل‌دهی حروف، ورودی/ویرایش یا کدگذاری—را ذکر کنید. رمز عبور، token یا اطلاعات خصوصی نفرستید.

## پروژه و مجوز

راستیکس بر پایهٔ [Windows Terminal مایکروسافت](https://github.com/microsoft/terminal) ساخته شده است. این پروژه مستقل و غیررسمی است؛ نام‌ها و علامت‌های تجاری بالادستی متعلق به صاحبان مربوطه هستند. برای اطلاعات مجوز و انتساب، فایل‌های [LICENSE](LICENSE) و [NOTICE.md](NOTICE.md) را ببینید.
