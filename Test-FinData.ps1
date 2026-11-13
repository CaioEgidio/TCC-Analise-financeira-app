#requires -Version 7.0
[CmdletBinding()]
param(
    [string]$BaseUrl = 'http://localhost:5068',
    [string]$EmailA = 'teste@exemplo.com',
    [string]$EmailB = 'segundo@exemplo.com'
)

$ErrorActionPreference = 'Stop'
$BaseUrl = $BaseUrl.TrimEnd('/')
$address = [uri]$BaseUrl
if (!$address.IsLoopback -or $address.Scheme -notin @('http', 'https')) {
    throw 'Use somente o endereço local da API (localhost ou 127.0.0.1).'
}
if ($EmailA -eq $EmailB) { throw 'Informe duas contas diferentes.' }
$script:checks = 0
$fixtures = [System.Collections.Generic.List[object]]::new()

function Assert-Check([bool]$Condition, [string]$Label) {
    if (!$Condition) { throw "FALHOU: $Label" }
    $script:checks++
    Write-Host "OK - $Label" -ForegroundColor Green
}

function Send-Api {
    param(
        [string]$Method,
        [string]$Path,
        [string]$Token = '',
        [object]$Body = $null,
        [int]$Expected = 200
    )
    $parameters = @{
        Method = $Method
        Uri = "$BaseUrl$Path"
        SkipHttpErrorCheck = $true
        MaximumRedirection = 0
        TimeoutSec = 20
        ErrorAction = 'Stop'
    }
    if ($Token) { $parameters.Headers = @{ Authorization = "Bearer $Token" } }
    if ($null -ne $Body) {
        $parameters.ContentType = 'application/json'
        $parameters.Body = $Body | ConvertTo-Json -Depth 8
    }
    $response = Invoke-WebRequest @parameters
    # A resposta de login contém segredos e nunca é impressa.
    if ([int]$response.StatusCode -ne $Expected) {
        throw "$Method $Path retornou $($response.StatusCode), esperado $Expected."
    }
    Assert-Check $true "$Method $Path -> $Expected"
    $content = $response.Content
    if ($content -is [byte[]]) {
        $content = [System.Text.Encoding]::UTF8.GetString($content)
    }
    if (![string]::IsNullOrWhiteSpace([string]$content)) {
        return ConvertFrom-Json -InputObject ([string]$content) -NoEnumerate
    }
}

function Get-Login([string]$Email) {
    $secret = Read-Host "Senha de $Email" -AsSecureString
    try {
        $password = [System.Net.NetworkCredential]::new('', $secret).Password
        $login = Send-Api -Method POST -Path '/api/auth/login' -Body @{
            email = $Email
            password = $password
        }
        if (!$login.accessToken) { throw 'Login sem accessToken.' }
        return [string]$login.accessToken
    }
    finally {
        $password = $null
        $secret.Dispose()
    }
}

function Get-TestCategory([string]$Token) {
    $name = 'FinData - testes automatizados'
    $categories = Send-Api -Method GET -Path '/api/categories' -Token $Token
    $category = $categories | Where-Object name -eq $name | Select-Object -First 1
    if ($category) { return [string]$category.id }
    $created = Send-Api -Method POST -Path '/api/categories' -Token $Token -Body @{
        name = $name
        description = 'Categoria reutilizada pelo teste automatizado local.'
    } -Expected 201
    if (!$created.id) { throw 'Categoria criada sem id.' }
    Write-Host 'Categoria de testes criada. Ela será mantida para as próximas execuções.'
    return [string]$created.id
}

function New-TestTransaction([string]$Token, [string]$CategoryId, [decimal]$Amount, [int]$Type) {
    $created = Send-Api -Method POST -Path '/api/transactions' -Token $Token -Body @{
        description = "Teste automatizado $runId"
        amount = $Amount
        date = $date
        type = $Type
        categoryId = $CategoryId
    } -Expected 201
    $id = [guid]::Parse($created.id).ToString()
    $fixtures.Add([pscustomobject]@{ Id = $id; Token = $Token; Deleted = $false })
    return $id
}

try {
    Write-Host 'Mantenha a API e o PostgreSQL em execução. Use as duas contas de teste.'
    $null = Send-Api -Method GET -Path '/api/auth/me' -Expected 401
    $tokenA = Get-Login $EmailA
    $tokenB = Get-Login $EmailB
    $userA = Send-Api -Method GET -Path '/api/auth/me' -Token $tokenA
    $userB = Send-Api -Method GET -Path '/api/auth/me' -Token $tokenB
    Assert-Check ($userA.id -and $userB.id -and $userA.id -ne $userB.id) 'Tokens identificam contas diferentes'

    $today = Get-Date
    $date = $today.ToString('yyyy-MM-dd')
    $query = "year=$($today.Year)&month=$($today.Month)"
    $runId = [guid]::NewGuid().ToString('N')
    $categoryA = Get-TestCategory $tokenA
    $categoryB = Get-TestCategory $tokenB
    Assert-Check ($categoryA -ne $categoryB) 'Categorias pertencem a contas distintas'
    $categoriesB = Send-Api -Method GET -Path '/api/categories' -Token $tokenB
    Assert-Check ($categoryA -notin @($categoriesB | ForEach-Object id)) 'B não lista a categoria de A'
    $categoriesA = Send-Api -Method GET -Path '/api/categories' -Token $tokenA
    Assert-Check ($categoryB -notin @($categoriesA | ForEach-Object id)) 'A não lista a categoria de B'

    $baselineA = Send-Api -Method GET -Path "/api/dashboard/summary?$query" -Token $tokenA
    $baselineB = Send-Api -Method GET -Path "/api/dashboard/summary?$query" -Token $tokenB
    $incomeA = New-TestTransaction $tokenA $categoryA 100 1
    $expenseA = New-TestTransaction $tokenA $categoryA 25 2
    $expenseB = New-TestTransaction $tokenB $categoryB 40 2

    $body = @{ description = 'Teste'; amount = 10; date = $date; type = 2; categoryId = $categoryA }
    $null = Send-Api -Method POST -Path '/api/transactions' -Token $tokenB -Body $body -Expected 400
    $body.categoryId = $categoryB
    $null = Send-Api -Method PUT -Path "/api/transactions/$expenseA" -Token $tokenB -Body $body -Expected 404
    $null = Send-Api -Method DELETE -Path "/api/transactions/$expenseA" -Token $tokenB -Expected 404

    $listA = Send-Api -Method GET -Path "/api/transactions?$query" -Token $tokenA
    $listB = Send-Api -Method GET -Path "/api/transactions?$query" -Token $tokenB
    $idsA = @($listA | ForEach-Object id)
    $idsB = @($listB | ForEach-Object id)
    Assert-Check ($expenseA -in $idsA -and $incomeA -in $idsA) 'A mantém seus lançamentos após a tentativa de exclusão por B'
    Assert-Check ($expenseB -notin $idsA -and $expenseB -in $idsB -and $expenseA -notin $idsB -and $incomeA -notin $idsB) 'Listagens isoladas entre contas'

    $summaryA = Send-Api -Method GET -Path "/api/dashboard/summary?$query" -Token $tokenA
    $summaryB = Send-Api -Method GET -Path "/api/dashboard/summary?$query" -Token $tokenB
    Assert-Check (([decimal]$summaryA.totalIncome - [decimal]$baselineA.totalIncome) -eq 100 -and ([decimal]$summaryA.totalExpense - [decimal]$baselineA.totalExpense) -eq 25 -and ([decimal]$summaryA.balance - [decimal]$baselineA.balance) -eq 75) 'Resumo de A considera somente seus novos lançamentos'
    Assert-Check (([decimal]$summaryB.totalIncome - [decimal]$baselineB.totalIncome) -eq 0 -and ([decimal]$summaryB.totalExpense - [decimal]$baselineB.totalExpense) -eq 40 -and ([decimal]$summaryB.balance - [decimal]$baselineB.balance) -eq -40) 'Resumo de B considera somente seus novos lançamentos'
    $breakdownA = Send-Api -Method GET -Path "/api/dashboard/expenses-by-category?$query" -Token $tokenA
    $breakdownB = Send-Api -Method GET -Path "/api/dashboard/expenses-by-category?$query" -Token $tokenB
    Assert-Check ($categoryB -notin @($breakdownA | ForEach-Object categoryId) -and $categoryA -notin @($breakdownB | ForEach-Object categoryId)) 'Dashboard não expõe categorias alheias'

    $body.categoryId = $categoryA
    $body.amount = 30
    $null = Send-Api -Method PUT -Path "/api/transactions/$expenseA" -Token $tokenA -Body $body -Expected 204
    $updated = Send-Api -Method GET -Path "/api/transactions?$query" -Token $tokenA
    $entry = $updated | Where-Object id -eq $expenseA | Select-Object -First 1
    Assert-Check ($null -ne $entry -and [decimal]$entry.amount -eq 30) 'Dono consegue editar a própria transação'
    $body.amount = 0
    $null = Send-Api -Method POST -Path '/api/transactions' -Token $tokenA -Body $body -Expected 400

    $null = Send-Api -Method DELETE -Path "/api/transactions/$expenseA" -Token $tokenA -Expected 204
    ($fixtures | Where-Object Id -eq $expenseA).Deleted = $true
    $null = Send-Api -Method DELETE -Path "/api/transactions/$expenseA" -Token $tokenA -Expected 404
    foreach ($path in @('/api/categories', "/api/transactions?$query", "/api/dashboard/summary?$query", "/api/dashboard/expenses-by-category?$query")) {
        $null = Send-Api -Method GET -Path $path -Expected 401
    }
    Write-Host "PASSOU: $script:checks verificações concluídas." -ForegroundColor Green
}
finally {
    foreach ($fixture in $fixtures) {
        if ($fixture.Deleted) { continue }
        try {
            $null = Send-Api -Method DELETE -Path "/api/transactions/$($fixture.Id)" -Token $fixture.Token -Expected 204
        }
        catch {
            Write-Warning "Não foi possível limpar a transação de teste $($fixture.Id). Confira com a conta dona."
        }
    }
    $tokenA = $null
    $tokenB = $null
    $login = $null
    Write-Host 'As categorias de teste ficam disponíveis para reutilização. Os tokens não foram gravados em arquivo.'
}
