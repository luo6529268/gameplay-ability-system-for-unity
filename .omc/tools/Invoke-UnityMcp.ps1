param(
    [Parameter(Mandatory = $true)]
    [string]$Type,

    [string]$ParamsJson = "{}",

    [int]$Port = 6402,

    [int]$TimeoutMilliseconds = 30000
)

$paramsObject = $ParamsJson | ConvertFrom-Json
$command = @{
    type = $Type
    params = $paramsObject
} | ConvertTo-Json -Compress -Depth 20

$client = [System.Net.Sockets.TcpClient]::new()
$client.ReceiveTimeout = $TimeoutMilliseconds
$client.SendTimeout = $TimeoutMilliseconds

try
{
    $client.Connect("127.0.0.1", $Port)
    $stream = $client.GetStream()
    $reader = [System.IO.StreamReader]::new(
        $stream,
        [System.Text.Encoding]::UTF8,
        $false,
        1024,
        $true)
    $handshake = $reader.ReadLine()
    if ($handshake -notmatch "FRAMING=1")
    {
        throw "UnityMCP bridge did not negotiate framed transport: '$handshake'"
    }

    $payload = [System.Text.Encoding]::UTF8.GetBytes($command)
    $header = [BitConverter]::GetBytes([uint64]$payload.Length)
    [Array]::Reverse($header)
    $stream.Write($header, 0, $header.Length)
    $stream.Write($payload, 0, $payload.Length)
    $stream.Flush()

    $responseHeader = New-Object byte[] 8
    $headerRead = 0
    while ($headerRead -lt $responseHeader.Length)
    {
        $count = $stream.Read(
            $responseHeader,
            $headerRead,
            $responseHeader.Length - $headerRead)
        if ($count -le 0)
        {
            throw "UnityMCP bridge closed before the response header completed."
        }
        $headerRead += $count
    }

    [Array]::Reverse($responseHeader)
    $responseLength = [BitConverter]::ToUInt64($responseHeader, 0)
    if ($responseLength -gt 64MB)
    {
        throw "UnityMCP response exceeds the 64 MB protocol limit."
    }

    $response = New-Object byte[] ([int]$responseLength)
    $responseRead = 0
    while ($responseRead -lt $response.Length)
    {
        $count = $stream.Read(
            $response,
            $responseRead,
            $response.Length - $responseRead)
        if ($count -le 0)
        {
            throw "UnityMCP bridge closed before the response body completed."
        }
        $responseRead += $count
    }

    [System.Text.Encoding]::UTF8.GetString($response)
}
finally
{
    $client.Dispose()
}
