# SAV Manager API - Quick Start Script
# Run this to start the API and test admin login

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "🚀 SAV Manager API - Quick Start" -ForegroundColor Yellow
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Admin Credentials
$adminEmail = "admin@savmanager.com"
$adminPassword = "Admin@123456"

Write-Host "📋 Admin Credentials:" -ForegroundColor Green
Write-Host "   Email:    $adminEmail"
Write-Host "   Password: $adminPassword"
Write-Host ""

# Navigate to API folder
$apiPath = "C:\Users\ayoub\Desktop\S2\.NET\SAVSolution\SAV.API"

if (Test-Path $apiPath) {
	Write-Host "📁 Found API project at: $apiPath" -ForegroundColor Green
	Push-Location $apiPath
} else {
	Write-Host "❌ API project not found at: $apiPath" -ForegroundColor Red
	Write-Host "Please update the path in this script." -ForegroundColor Yellow
	exit
}

Write-Host ""
Write-Host "🔨 Building solution..." -ForegroundColor Cyan
dotnet build

if ($LASTEXITCODE -ne 0) {
	Write-Host "❌ Build failed!" -ForegroundColor Red
	Pop-Location
	exit
}

Write-Host ""
Write-Host "✅ Build successful!" -ForegroundColor Green
Write-Host ""

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "🌐 Starting API..." -ForegroundColor Yellow
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "🔍 Watch for this message in the logs:" -ForegroundColor Yellow
Write-Host "   '✅ Default admin account created successfully.'" -ForegroundColor Green
Write-Host ""
Write-Host "📍 API will be available at: https://localhost:5001" -ForegroundColor Cyan
Write-Host "📍 Swagger UI: https://localhost:5001/swagger" -ForegroundColor Cyan
Write-Host ""

dotnet run

Pop-Location
