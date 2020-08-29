module Config

open System

type Settings = {
    BaseDir: string
    BlocksDir: string
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
    MaxPeers: int
    PruneDepth: int
    ReadTimeout: int
    BloomFilterSize: int
    SocksHost: string
    SocksPort: int
}

let settings = {
    BaseDir = "/home/bitcoin"
    BlocksDir = "/home/bitcoin/blocks"
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
    MaxPeers = 5
    PruneDepth = 2000000
    ReadTimeout = 30000
    BloomFilterSize = 10000
    SocksHost = "127.0.0.1"
    SocksPort = 9150
}

let logger1 s = Console.WriteLine("{0:s} {1}", DateTime.Now, s)
let logger2 a b = Console.WriteLine("{0:s} {1} {2}", DateTime.Now, a, b)
let logger3 a b c = Console.WriteLine("{0:s} {1} {2} {3}", DateTime.Now, a, b, c)
