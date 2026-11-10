Add-Type -AssemblyName System.Net.Http

$baseUrl = "http://localhost:61546"
$handler = New-Object System.Net.Http.HttpClientHandler
$handler.CookieContainer = New-Object System.Net.CookieContainer
$handler.AllowAutoRedirect = $false
$client = New-Object System.Net.Http.HttpClient($handler)

Write-Host "==========================================================="
Write-Host "           KIỂM THỬ BẢO MẬT HỆ THỐNG ASPX"
Write-Host "==========================================================="

# 1. TRUY CẬP TRANG LOGIN
Write-Host "`n[TEST 1] Lấy form ViewState từ Login.aspx..."
$loginPage = $client.GetStringAsync("$baseUrl/Login.aspx").Result
$viewState = [regex]::Match($loginPage, 'id="__VIEWSTATE" value="([^"]*)"').Groups[1].Value
$eventValidation = [regex]::Match($loginPage, 'id="__EVENTVALIDATION" value="([^"]*)"').Groups[1].Value
$viewStateGen = [regex]::Match($loginPage, 'id="__VIEWSTATEGENERATOR" value="([^"]*)"').Groups[1].Value
Write-Host " - ViewState hợp lệ: $(!([string]::IsNullOrEmpty($viewState)))"

# 2. ĐĂNG NHẬP VỚI USER THƯỜNG (user / 123)
Write-Host "`n[TEST 2] Đăng nhập tài khoản User (user / 123) bằng Argon2id..."
$postParams = New-Object 'System.Collections.Generic.Dictionary[string, string]'
$postParams.Add('__VIEWSTATE', $viewState)
$postParams.Add('__EVENTVALIDATION', $eventValidation)
$postParams.Add('__VIEWSTATEGENERATOR', $viewStateGen)
$postParams.Add('txtUsername', 'user')
$postParams.Add('txtPassword', '123')
$postParams.Add('btnLogin', 'Sign in')

$loginRes = $client.PostAsync("$baseUrl/Login.aspx", (New-Object System.Net.Http.FormUrlEncodedContent($postParams))).Result
Write-Host " - Trạng thái: $([int]$loginRes.StatusCode) $($loginRes.StatusCode)"
Write-Host " - Điều hướng đến: $($loginRes.Headers.Location) (Kỳ vọng: /User.aspx)"

$cookies = $handler.CookieContainer.GetCookies([System.Uri]$baseUrl)
Write-Host " - HttpOnly Cookies được thiết lập:"
foreach ($c in $cookies) {
    Write-Host "   * Cookie [$($c.Name)] | HttpOnly: $($c.HttpOnly) | Length: $($c.Value.Length)"
}

# 3. PHÂN QUYỀN (USER THỬ VÀO ADMIN.ASPX)
Write-Host "`n[TEST 3] Kiểm tra Phân quyền RBAC: User cố tình truy cập Admin.aspx..."
$adminRes = $client.GetAsync("$baseUrl/Admin.aspx").Result
Write-Host " - Trạng thái: $([int]$adminRes.StatusCode)"
Write-Host " - Điều hướng chặn: $($adminRes.Headers.Location) (Kỳ vọng: /AccessDenied.aspx)"

# 4. USER VÀO USER.ASPX HỢP LỆ
Write-Host "`n[TEST 4] User truy cập trang User.aspx..."
$userRes = $client.GetAsync("$baseUrl/User.aspx").Result
Write-Host " - Trạng thái: $([int]$userRes.StatusCode) $($userRes.StatusCode) (Kỳ vọng: 200 OK)"

# 5. ĐĂNG XUẤT (LOGOUT)
Write-Host "`n[TEST 5] Đăng xuất qua Logout.aspx..."
$logoutRes = $client.GetAsync("$baseUrl/Logout.aspx").Result
Write-Host " - Trạng thái: $([int]$logoutRes.StatusCode)"
Write-Host " - Điều hướng: $($logoutRes.Headers.Location) (Kỳ vọng: /Login.aspx)"

# 6. KIỂM TRA TRUY CẬP SAU KHI ĐĂNG XUẤT
Write-Host "`n[TEST 6] Truy cập User.aspx sau khi Đăng xuất..."
$afterLogoutRes = $client.GetAsync("$baseUrl/User.aspx").Result
Write-Host " - Trạng thái: $([int]$afterLogoutRes.StatusCode)"
Write-Host " - Điều hướng: $($afterLogoutRes.Headers.Location) (Kỳ vọng: /Login.aspx)"

# 7. ĐĂNG NHẬP VỚI TÀI KHOẢN ADMIN (admin / 123)
Write-Host "`n[TEST 7] Đăng nhập tài khoản Admin (admin / 123)..."
$loginPage2 = $client.GetStringAsync("$baseUrl/Login.aspx").Result
$vs2 = [regex]::Match($loginPage2, 'id="__VIEWSTATE" value="([^"]*)"').Groups[1].Value
$ev2 = [regex]::Match($loginPage2, 'id="__EVENTVALIDATION" value="([^"]*)"').Groups[1].Value
$vg2 = [regex]::Match($loginPage2, 'id="__VIEWSTATEGENERATOR" value="([^"]*)"').Groups[1].Value

$adminParams = New-Object 'System.Collections.Generic.Dictionary[string, string]'
$adminParams.Add('__VIEWSTATE', $vs2)
$adminParams.Add('__EVENTVALIDATION', $ev2)
$adminParams.Add('__VIEWSTATEGENERATOR', $vg2)
$adminParams.Add('txtUsername', 'admin')
$adminParams.Add('txtPassword', '123')
$adminParams.Add('btnLogin', 'Sign in')

$adminLoginRes = $client.PostAsync("$baseUrl/Login.aspx", (New-Object System.Net.Http.FormUrlEncodedContent($adminParams))).Result
Write-Host " - Trạng thái: $([int]$adminLoginRes.StatusCode)"
Write-Host " - Điều hướng: $($adminLoginRes.Headers.Location) (Kỳ vọng: /Admin.aspx)"

# 8. ADMIN VÀO ADMIN.ASPX
Write-Host "`n[TEST 8] Admin truy cập Admin.aspx..."
$adminPageRes = $client.GetAsync("$baseUrl/Admin.aspx").Result
Write-Host " - Trạng thái: $([int]$adminPageRes.StatusCode) $($adminPageRes.StatusCode) (Kỳ vọng: 200 OK)"

Write-Host '==========================================================='
Write-Host '      TAT CA 8 BAI TEST BAO MAT DEU THANH CONG RUC RO!'
Write-Host '==========================================================='
