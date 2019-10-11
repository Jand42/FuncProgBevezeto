- title : Funkcionális programozás F# nyelven
- author : Jankó András
- theme : night
- transition : default

***

### Funkcionális programozás F# nyelven

**Jankó András**

![IntelliFactory](images/iflogo.jpg)

![Talentera](images/Talentera.svg)

***

### F# nyelv alapjai

- Típus inferencia
- Lineáris projektstruktúra
- Utasítások helyett kifejezések
- Alapértelmezett immutabilitás
- Elsődlegesen funkcionális
- Öröklődés helyett kompozíció
- Teljes .NET kompatibilitás

---

#### Típus inferencia

    let f x = x + 1

    let iter act s =
        for x in s do
            act x

    let printNum = printfn "%d"

    [ 1 .. 5 ] |> iter printNum

--- 

#### Lineáris projektstruktúra

* File1.fsi
* File1.fs
* File2.fs


    type Type1 = { A : Type2 option } // option<Type2>
    and Type2 = { B : Type1 option }

    let rec f1 x =
        printNum x
        f2 (x / 2)
    and f2 x =
        printNum x
        if x > 1 then f1 (x + 1)

    f1 10

    let f1 x = x + 1 // shadowing

--- 

#### Utasítások helyett kifejezések

    let toPigLatin (word: string) =
        let isVowel (c: char) =
            match c with
            | 'a' | 'e' | 'i' |'o' |'u'
            | 'A' | 'E' | 'I' | 'O' | 'U' -> true
            | _ -> false
        
        if isVowel word.[0] then
            word + "yay"
        else
            word.[1..] + string (word.[0]) + "ay"

    toPigLatin "fsharp" 

---

#### Alapértelmezett immutabilitás

    type Person = { Name: string; Age : int }

    let andras34 = { Name = "András"; Age = 34 }

    let andras35 = { andras34 with Age = andras34.Age + 1 }

    module Mutable =
        type Person = { Name: string; mutable Age : int }

        let andras34 = { Name = "András"; Age = 34 }
        
        do andras34.Age <- andras34.Age + 1

---

#### Elsődlegesen funkcionális

    [ 1 .. 10 ] |> List.reduce (*)

    type Math =
        static member Factorial n =
            if n <= 1 then
                1 
            else
                n * Math.Factorial (n - 1)

    // tail recursive
    let factorial n =
        let rec f n acc =
            if n <= 1 then
                acc
            else
                f (n - 1) (n * acc)
        f n 1

---

#### Öröklődés helyett kompozíció

    type Cat = { Name : string; Kittens: Cat list }
    type Dog = { Name : string; Owner: Person }

    type Pet =
        | Cat of Cat
        | Dog of Dog

    let feedPet feedCat feedDog pet =
        match pet with
        | Cat c -> 
            feedCat c
            c.Kittens |> List.iter feedCat
        | Dog d -> 
            feedDog d

---

#### Teljes .NET kompatibilitás

    open System
    open System.IO
    open System.Net

    // Fetch the contents of a web page
    let fetchUrl callback url =
        let req = WebRequest.Create(Uri(url))
        use resp = req.GetResponse()
        use stream = resp.GetResponseStream()
        use reader = new IO.StreamReader(stream)
        callback reader url

***

### Egyedi képességek és az FSharp.Core

- Seq
- Async
- Query
- Computation expression
- Type provider

---

#### Seq

    let isPrime n =
        let rec check i =
            float i > sqrt (float n) || (n % i <> 0 && check (i + 1))
        check 2

    let primes = 
        seq {
            let mutable i = 2
            while true do
                if isPrime i then
                    yield i
                i <- i + 1
        }

    primes |> Seq.take 10 |> Seq.iter printNum

---

#### Async

    let random = System.Random()

    let fakeTask name =
        async {
            printfn "Starting %s" name
            do! Async.Sleep (random.Next(1000))
            printfn "Finished %s" name
            return "Done: " + name
        }

    let combined =
        async {
            let! r1 = fakeTask "first"
            let! r2 = fakeTask "second"
            return r1 + "; " + r2
        }   

    [ combined; fakeTask "third" ]
    |> Async.Parallel
    |> Async.RunSynchronously

---

#### Query

    let primesSquaredSmall =
        query { 
            for p in primes |> Seq.take 10 do
            where (p < 10)
            select (p * p)
        }

    primesSquaredSmall |> Seq.iter printNum

---

#### Computation expression

    type MaybeBuilder() =

        member this.Bind(x, f) = 
            match x with
            | None -> None
            | Some a -> f a

        member this.Return(x) = 
            Some x
    
    let maybe = new MaybeBuilder()

    maybe {
        let! x = Some 3
        return x + 3
    }

---

#### Type provider

    (*
    #r "../packages/FSharp.Data/lib/net45/FSharp.Data.dll"
    open FSharp.Data

    [<Literal>]
    let uri = "https://api.github.com/orgs/fsbolero/repos"
    type MyJson = JsonProvider<uri>

    let getRepos (uri: string) = async {
        use wc = new WebClient()
        wc.Headers.Add("User-Agent", "request")
        let! json = wc.DownloadStringTaskAsync(uri) |> Async.AwaitTask  
        printfn "Here are the repositories in the fsbolero organization:"
        for repo in MyJson.Parse(json) do
            printfn "* %s" repo.Name
    }

    getRepos uri |> Async.RunSynchronously
    *)

***

### Operátorok és DLS-ek

- Tetszőleges operátorok
- Duck typing
- FAKE
- FsUnit

---

#### Tetszőleges operátorok

    let (?+?) x y =
        match x, y with
        | Some x, Some y -> Some (x + y)
        | _ -> None

    (Some 1) ?+? (Some 2)
    (Some 3) ?+? None

---

#### Duck typing

    let inline ( ! ) (o: ^x) : ^a =
        (^x: (member Value: ^a with get) o)

    let inline ( := ) (o: ^x) (v: ^a) =
        (^x: (member Value: ^a with set) (o, v))

---

#### FAKE

    Build scriptek

    (*
    "Clean"
        ==> "BuildApp"
        ==> "Default"

    // start build
    Target.runOrDefault "Default"
    *)

----

#### FsUnit

    Unit tesztelés

    (*
    open FsUnit
    open NUnit.Framework

    [<Test>]
    let ``Addition works``() =
        1 + 1 |> should equal 2
        1 + 1 |> should not' equal 3

    [<Test>]
    let ``startWith results``() =
        "ships" |> should startWith "sh"
        "ships" |> should not' (startWith "ss")
    *)

***

### Applikáció fejlesztés

- Konzol
- Könyvtár
- Windows
- Web

---

#### WebSharper

websharper.com

try.webhsarper.com

---

#### Fable

fable.io

safe-stack.github.io

try.fsharp.org

---

#### WebSharper vs Fable

- WebSharper: .NET 4.5+/Core, F#+C#, ES5, NuGet, kliens-szerver, pontosabb szemantika
- Fable: .NET Core, F#, ES2015 - Babel, npm, magában csak kliens-oldali, nagyobb System könyvtár lefedettség, jobb TypeScript támogatás

---

#### Bolero

fsbolero.io

tryfsharp.fsbolero.io

***

### Köszönöm a figyelmet!

andras.janko@gmail.com

github.com/Jand42

hr@intellifactory.com
