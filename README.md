# Rastix

**Rastix** is an unofficial Windows Terminal-based project focused on improving the display of Persian and other right-to-left (RTL) text in command-line applications. It enables bidirectional text support by default and provides a separate `rx` launch alias.

Rastix is not an official Microsoft product or release. RTL rendering and editing behavior can vary depending on the application, font, input method, and text being displayed; this project does not promise to solve every shaping, ordering, or editing issue.

## Features

- Bidirectional text rendering is enabled by default.
- The development package is branded **Rastix** and uses the `rx` command, avoiding a conflict with the official Windows Terminal `wt` alias.
- The default PowerShell and Command Prompt profiles start in the current working directory. In File Explorer, enter `rx` in the address bar to open Rastix in the displayed folder.
- The Explorer context menu includes **Open in Rastix here**.

## Download and install

The project builds an **unsigned MSIX** with GitHub Actions. To get the latest successful build:

1. Open [Rastix GitHub Actions](https://github.com/Tiredb0y/Rastix/actions/workflows/rtl-build.yml), select a successful run, and download the `Rastix-unsigned-MSIX` artifact.
2. Extract the downloaded artifact ZIP. If an earlier Rastix development package is installed, close it first. The package identity is `WindowsTerminalDev`, separate from the official Windows Terminal package. Remove only this development package before registering the new build (some builds may have a lower package version):

   ```powershell
   Get-AppxPackage WindowsTerminalDev | Remove-AppxPackage
   ```

   Back up any settings you want to keep before removal. The package's user data is under `%LOCALAPPDATA%\Packages\WindowsTerminalDev_8wekyb3d8bbwe\LocalState`. Do not use the command above to remove the official Microsoft Windows Terminal.
3. In PowerShell, from the extracted folder, unpack the MSIX and register its manifest:

   ```powershell
   $msix = Get-ChildItem . -Filter *.msix -Recurse | Select-Object -First 1
   if (-not $msix) { throw 'No MSIX found in the extracted artifact.' }
   makeappx.exe unpack /p $msix.FullName /d .\Rastix-unpacked
   Add-AppxPackage -Register .\Rastix-unpacked\AppxManifest.xml
   ```

   Windows Developer Mode and the Windows SDK (`makeappx.exe`) are required. If `makeappx.exe` is not on `PATH`, run it from the Windows SDK installation directory. This is an unsigned development package: install it only if you trust the source and build.

After installation, launch Rastix with `rx`. In File Explorer, typing `rx` in the address bar opens it at the folder currently shown. The Explorer context-menu option can also open the selected folder.

**Requirements:** Windows 10 version 2004 (build 19041) or later. GitHub Actions artifacts are temporary build outputs, not signed releases or automatic updates.

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

This repository has a GitHub Actions workflow that builds an unsigned MSIX. See [the workflow runs](https://github.com/Tiredb0y/Rastix/actions/workflows/rtl-build.yml) for build status and artifacts. For local source builds, the upstream [Windows Terminal build guide](https://github.com/microsoft/terminal/blob/main/doc/building.md) describes the general requirements; this fork's workflow is the reference for its CI build configuration.

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
- بستهٔ توسعه با نام **Rastix** و فرمان `rx` ارائه می‌شود و با نام `wt` در Windows Terminal رسمی تداخل ندارد.
- پروفایل‌های پیش‌فرض PowerShell و Command Prompt از پوشهٔ جاری شروع می‌شوند. در نوار نشانی File Explorer بنویسید `rx` تا برنامه در پوشهٔ نمایش‌داده‌شده باز شود.
- گزینهٔ **Open in Rastix here** در منوی زمینهٔ File Explorer وجود دارد.

## دریافت و نصب

GitHub Actions یک بستهٔ **MSIX امضانشده** می‌سازد. برای دریافت آخرین ساخت موفق:

1. به [اجرای GitHub Actions راستیکس](https://github.com/Tiredb0y/Rastix/actions/workflows/rtl-build.yml) بروید، یک اجرای موفق را باز کنید و artifact با نام `Rastix-unsigned-MSIX` را دریافت کنید.
2. فایل ZIP دریافت‌شده را استخراج کنید. اگر نسخهٔ توسعهٔ قبلی راستیکس نصب است، ابتدا برنامه را ببندید. هویت بسته `WindowsTerminalDev` است و از بستهٔ رسمی Windows Terminal جداست. پیش از ثبت build جدید، نسخهٔ توسعهٔ قبلی را حذف کنید (ممکن است شمارهٔ نسخهٔ بعضی buildها پایین‌تر باشد):

   ```powershell
   Get-AppxPackage WindowsTerminalDev | Remove-AppxPackage
   ```

   اگر تنظیماتی را لازم دارید، پیش از حذف از آن‌ها پشتیبان بگیرید. داده‌های کاربری بسته در مسیر `%LOCALAPPDATA%\Packages\WindowsTerminalDev_8wekyb3d8bbwe\LocalState` قرار دارد. این فرمان را برای حذف Windows Terminal رسمی Microsoft به‌کار نبرید.
3. در PowerShell و از پوشهٔ استخراج‌شده، MSIX را باز و manifest آن را ثبت کنید:

   ```powershell
   $msix = Get-ChildItem . -Filter *.msix -Recurse | Select-Object -First 1
   if (-not $msix) { throw 'No MSIX found in the extracted artifact.' }
   makeappx.exe unpack /p $msix.FullName /d .\Rastix-unpacked
   Add-AppxPackage -Register .\Rastix-unpacked\AppxManifest.xml
   ```

   Developer Mode ویندوز و Windows SDK (ابزار `makeappx.exe`) لازم است. اگر `makeappx.exe` در `PATH` نیست، آن را از پوشهٔ نصب Windows SDK اجرا کنید. این بستهٔ توسعه امضا نشده است؛ فقط در صورتی نصبش کنید که به منبع و فایل ساخت اعتماد دارید.

پس از نصب، راستیکس را با `rx` اجرا کنید. در File Explorer، نوشتن `rx` در نوار نشانی برنامه را در پوشهٔ فعلی باز می‌کند. همچنین می‌توانید از گزینهٔ منوی زمینه برای بازکردن پوشهٔ انتخاب‌شده استفاده کنید.

**پیش‌نیاز:** Windows 10 نسخهٔ 2004 (ساخت 19041) یا جدیدتر. artifactهای GitHub Actions فایل‌های موقت ساخت هستند؛ انتشار امضاشده یا به‌روزرسانی خودکار محسوب نمی‌شوند.

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

این مخزن workflow مربوط به ساخت MSIX امضانشده را در GitHub Actions دارد. وضعیت ساخت و artifactها در [اجرای workflow](https://github.com/Tiredb0y/Rastix/actions/workflows/rtl-build.yml) در دسترس‌اند. برای ساخت محلی از کد منبع، [راهنمای ساخت Windows Terminal](https://github.com/microsoft/terminal/blob/main/doc/building.md) پیش‌نیازهای عمومی را توضیح می‌دهد؛ workflow این مخزن مرجع تنظیمات ساخت CI راستیکس است.

## گزارش مشکل

هنگام گزارش مشکل، نسخهٔ ویندوز، برنامه‌ای که متن را تولید می‌کند، فونت، نمونه‌ای از متن (در صورت امکان قابل کپی)، و نوع مشکل—ترتیب RTL، شکل‌دهی حروف، ورودی/ویرایش یا کدگذاری—را ذکر کنید. رمز عبور، token یا اطلاعات خصوصی نفرستید.

## پروژه و مجوز

راستیکس بر پایهٔ [Windows Terminal مایکروسافت](https://github.com/microsoft/terminal) ساخته شده است. این پروژه مستقل و غیررسمی است؛ نام‌ها و علامت‌های تجاری بالادستی متعلق به صاحبان مربوطه هستند. برای اطلاعات مجوز و انتساب، فایل‌های [LICENSE](LICENSE) و [NOTICE.md](NOTICE.md) را ببینید.
