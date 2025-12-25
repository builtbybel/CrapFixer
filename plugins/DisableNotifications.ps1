#Requires -RunAsAdministrator

try {
    # Disable Windows notifications by modifying registry settings
    Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\PushNotifications" -Name "ToastEnabled" -Type DWord -Value 0 -ErrorAction Stop

    # Disable notifications in the Settings app (Action Center)
    Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Notifications\Settings" -Name "NOC_GLOBAL_SETTING_TOASTS_ENABLED" -Type DWord -Value 0 -ErrorAction Stop

    # Suppress notification banners for all apps
    $apps = Get-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\*" -ErrorAction SilentlyContinue
    foreach ($app in $apps) {
        Set-ItemProperty -Path $app.PSPath -Name "ShowInActionCenter" -Type DWord -Value 0 -ErrorAction SilentlyContinue
        Set-ItemProperty -Path $app.PSPath -Name "Enabled" -Type DWord -Value 0 -ErrorAction SilentlyContinue
    }

    # Attempt to disable Focus Assist (Quiet Hours), but skip if path doesn't exist
    $focusAssistPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\Cache\DefaultAccount\$$windows.data.notifications.quiethourssettings\Current"
    if (Test-Path $focusAssistPath) {
        Set-ItemProperty -Path $focusAssistPath -Name "Data" -Type Binary -Value ([byte[]](0x02,0x00,0x00,0x00,0x00,0x00,0x00,0x00)) -ErrorAction Stop
    } else {
        Write-Host "Focus Assist registry path not found. Skipping Focus Assist configuration."
    }

    Write-Host "Windows notifications have been disabled. Please restart your system for changes to take effect."
}
catch {
    Write-Host "An error occurred: $_"
    Write-Host "Some notification settings may not have been applied."
}