$targetFolder = Join-Path (Get-Location) "runtimes"
Write-Output "Removing Runtimes"

# Check if the folder exists
if (Test-Path $targetFolder) {
    # Get all subdirectories except 'win', 'win-x64', and 'win-x86', then delete them
    Get-ChildItem -Path $targetFolder -Directory | Where-Object {
        $_.Name -notin @("win", "win-x64", "win-x86")
    } | ForEach-Object {
        Remove-Item -Path $_.FullName -Recurse -Force
    }
    Write-Output "All subdirectories in '$targetFolder' except 'win', 'win-x64', and 'win-x86' have been deleted."
} else {
    Write-Output "The 'runtimes' folder does not exist."
}

$TargetDir = Get-Location
$languages = @(
    "cs", "da", "de", "fa", "fi", "fr", "it", "ko", "mk", "nl", "pl", "pt", "ru", "sv", "tr", "zh-CN", "zh-TW",
    "vi-VN", "af-ZA", "uz-Latn-UZ", "ur-PK", "uk-UA", "ug-CN", "tt-RU", "tr-TR", "th-TH", "te-IN", "ta-IN",
    "sv-SE", "sr-Latn-RS", "sr-Cyrl-RS", "sr-Cyrl-BA", "am-ET", "as-IN", "az-Latn-AZ", "bg-BG", "bn-IN",
    "ar-SA", "bs-Latn-BA", "ca-ES", "ca-Es-VALENCIA", "cs-CZ", "cy-GB", "da-DK", "de-DE", "el-GR", "et-EE",
    "eu-ES", "fa-IR", "fi-FI", "fil-PH", "fr-CA", "fr-FR", "ga-IE", "gd-gb", "gl-ES", "gu-IN", "he-IL",
    "hi-IN", "hr-HR", "hu-HU", "hy-AM", "id-ID", "is-IS", "it-IT", "ja-JP", "ka-GE", "kk-KZ", "km-KH",
    "kn-IN", "ko-KR", "kok-IN", "lb-LU", "lo-LA", "lt-LT", "lv-LV", "mi-NZ", "mk-MK", "ml-IN", "mr-IN",
    "ms-MY", "mt-MT", "nb-NO", "ne-NP", "nl-NL", "nn-NO", "or-IN", "pa-IN", "pl-PL", "pt-BR", "pt-PT",
    "quz-PE", "ro-RO", "ru-RU", "sk-SK", "sl-SI", "sq-AL"
)
foreach ($lang in $languages) {
    $path = Join-Path -Path $TargetDir -ChildPath $lang
    if (Test-Path $path) {
        Remove-Item -Path $path -Recurse -Force -ErrorAction SilentlyContinue
    }
}