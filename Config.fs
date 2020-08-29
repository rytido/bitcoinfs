module Config

open System

type Settings = {
    BaseDir: string
    ConnectTimeout: float
    CommandTimeout: float
    HandshakeTimeout: float
    MinGetdataBatchSize: int
    MaxGetdataBatchSize: int
    MyExtIp: string
    ServerPort: int
    TestNet: bool
    MaxGetblockRetry: int
    UseTor: bool
    UseSocks: bool
}

let settings = {
    BaseDir = "/home/bitcoin"
    ConnectTimeout = 5.
    CommandTimeout = 15.
    HandshakeTimeout = 10.
    MinGetdataBatchSize = 10
    MaxGetdataBatchSize = 20
    MyExtIp = "192.168.0.101"
    ServerPort = 8333
    TestNet = false
    MaxGetblockRetry = 3
    UseTor = true
    UseSocks = true
}

let logger1 s = Console.WriteLine("{0:s} {1}", DateTime.Now, s)
let logger2 a b = Console.WriteLine("{0:s} {1} {2}", DateTime.Now, a, b)
let logger3 a b c = Console.WriteLine("{0:s} {1} {2} {3}", DateTime.Now, a, b, c)
