﻿(**
# Main
*)
module Main

(*** hide ***)
open System.IO
open System.Net
open System.Threading
open Org.BouncyCastle.Utilities.Encoders
open FSharpx
open FSharpx.Collections
open Protocol
open Peer
open Checks
open Db
open Script
open Config

(* The following stuff is for skipping validation during the import of a bootstrap file and for debugging purposes *)
let skipScript (script: byte[]) =
    script.Length = 25 && script.[0] = OP_DUP && script.[1] = OP_HASH160 && script.[24] = OP_CHECKSIG

let processUTXOFast (utxoAccessor: IUTXOAccessor) (height) (tx: Tx) (i: int) =
    tx.TxIns |> Seq.iteri (fun iTxIn txIn ->
        if i <> 0 then
            let scriptRuntime = ScriptRuntime(Script.computeTxHash tx iTxIn)
            utxoAccessor.GetUTXO txIn.PrevOutPoint |> Option.iter(fun txOut ->
                let inScript = txIn.Script
                let script = txOut.TxOut.Script
                (*
                if not (scriptRuntime.Verify(inScript, script)) then
                    logger.ErrorF "Script failed on tx %A for input %d" tx iTxIn
                *)
                utxoAccessor.DeleteUTXO txIn.PrevOutPoint
                )
        )
    tx.TxOuts |> Seq.iteri (fun iTxOut txOut ->
        let outpoint = OutPoint(tx.Hash, iTxOut)
        utxoAccessor.AddUTXO (outpoint, UTXO(txOut, 0))
        )

(**
*)
let readBootstrapFast (firstBlock: int) (stream: Stream) =
    use reader = new BinaryReader(stream)
    let mutable i = firstBlock
    let mutable tip: byte[] = null
    while(stream.Position <> stream.Length) do
        if i % 10000 = 0 then
            logger1 i
        let magic = reader.ReadInt32()
        let length = reader.ReadInt32()
        let block = ParseByteArray (reader.ReadBytes(length)) Block.Parse
        block.Header.Height <- i
        block.Header.IsMain <- true
        let prevBH = Db.readHeader block.Header.PrevHash
        if prevBH.Hash <> zeroHash
        then
            prevBH.NextHash <- block.Header.Hash
            Db.writeHeaders prevBH
        block.Txs |> Seq.iteri (fun idx tx -> processUTXOFast utxoAccessor block.Header.Height tx idx)
        Db.writeHeaders block.Header
        tip <- block.Header.Hash
        i <- i + 1
    logger2 "Last block" i
    Db.writeTip tip

let writeBootstrap (firstBlock: int) (lastBlock: int) (stream: Stream) =
    use writer = new BinaryWriter(stream)
    for i in firstBlock..lastBlock do
        logger2 "Writing block #" i
        let bh = Db.getHeaderByHeight i
        let block = Db.loadBlock bh
        writer.Write(magic)
        writer.Write(Db.getBlockSize bh)
        writer.Write(block.ToByteArray())

(*** hide ***)
let verifySingleTx (tx: Tx) (iTxIn: int) (outScript: byte[]) =
    let scriptRuntime = ScriptRuntime(Script.computeTxHash tx iTxIn)
    let txIn = tx.TxIns.[iTxIn]
    let inScript = txIn.Script
    scriptRuntime.Verify(inScript, outScript)

let decodeTx (s: string) =
    let hex = Hex.Decode(s)
    use ms = new MemoryStream(hex)
    use reader = new BinaryReader(ms)
    Tx.Parse(reader)

let readBootstrap (firstBlock: int) (stream: Stream) =
    use reader = new BinaryReader(stream)
    let mutable i = firstBlock
    while(stream.Position <> stream.Length) do
        if i % 10000 = 0 then
            logger1 i
        let magic = reader.ReadInt32()
        let length = reader.ReadInt32()
        let block = ParseByteArray (reader.ReadBytes(length)) Block.Parse
        block.Header.Height <- i
        updateBlockUTXO utxoAccessor block |> ignore
        i <- i + 1
    logger2 "Last block" i

let readBootstrapTest() =
    // Import a couple of bootstrap dat files
    use stream = new FileStream("J:/bootstrap-295000.dat", FileMode.Open, FileAccess.Read)
    readBootstrapFast 0 stream
    use stream = new FileStream("J:/bootstrap-332702.dat", FileMode.Open, FileAccess.Read)
    readBootstrapFast 295001 stream
    use stream = new FileStream("J:/bootstrap-341000.dat", FileMode.Open, FileAccess.Read)
    readBootstrapFast 332703 stream

let writeBootstrapFile() =
    // Write a bootstrap file from saved blocks

    use stream = new FileStream("D:/bootstrap-nnn.dat", FileMode.CreateNew, FileAccess.Write)
    writeBootstrap 345001 348000 stream

let addLocalNode() =
    let myNode = IPEndPoint(IPAddress.Loopback, 8333)
    trackerIncoming.OnNext(TrackerCommand.Connect myNode)

let runNode() =
    Peer.initPeers()

    Tracker.startTracker()
    Tracker.startServer()
    Mempool.startMempool()
    Blockchain.blockchainStart()

    addLocalNode()

    trackerIncoming.OnNext(TrackerCommand.GetPeers)
    Thread.Sleep(-1)

(**
The main function initializes the application and waits forever
*)
[<EntryPoint>]
let main argv =
    // Config.BasicConfigurator.Configure() |> ignore

    // Db.scanUTXO()
    runNode()
    // writeBootstrapFile()

    0 // return an integer exit code
