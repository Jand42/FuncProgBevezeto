let f x = x + 1

let iter act s =
    for x in s do
        act x

let printNum = printfn "%d"

let (|>) x f = f x

[ 1 .. 5 ] |> iter printNum

// ---

type Type1 = { A : Type2 option } // option<Type2>
and Type2 = { B : Type1 option }

let xx = 
    let rec f1 x =
        printNum x
        x / 2 |> f2
    and f2 x =
        printNum x
        if x > 1 then
            x + 1 |> f1

    f1 10

    let f1 x = x + 1 // shadowing

    ()

// ---

let toPigLatin (word: string) =
    let isVowel (c: char) =
        match c with
        | 'a' | 'e' | 'i' |'o' |'u'
        | 'A' | 'E' | 'I' | 'O' | 'U' -> true
        |_ -> false
    
    if isVowel word.[0] then
        word + "yay"
    else
        word.[1..] + string (word.[0]) + "ay"

toPigLatin "fsharp" 

// ---

type Person = { Name: string; Age : int }

let andras34 = { Name = "András"; Age = 34 }
let andras35 = { andras34 with Age = andras34.Age + 1 }

let andras34anon = {| Name = "András"; Age = 34 |}
let andras35anon = {| andras34 with Age = andras34.Age + 1; Awake = true |}
     
module Mutable =
    type Person = { Name: string; mutable Age : int }

    let andras34 = { Name = "András"; Age = 34 }
    
    do andras34.Age <- andras34.Age + 1
     
// ---

[ 1 .. 10 ] |> List.reduce (*)

type Math =
    static member Factorial n =
        if n <= 1 then
            1 
        else
            n * Math.Factorial (n - 1)

module Math =
    let factorial n =
        let rec f n acc =
            if n <= 1 then
                acc
            else
                f (n - 1) (n * acc)
        f n 1

//let mutable x = 2

// ---

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
    //| Dog d -> 
    //    feedDog d

// ---

open System
open System.Net

// Fetch the contents of a web page
let fetchUrl callback url =
    let req = WebRequest.Create(Uri(url))
    use resp = req.GetResponse()
    use stream = resp.GetResponseStream()
    use reader = new IO.StreamReader(stream)
    callback reader url

// ---

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

// ---



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

let comps = 
    [
        combined
        fakeTask "third"
    ]
Async.Parallel(comps, 4) // max párhozamos szálak, újdonság az FSharp 4.7-ben
|> Async.RunSynchronously

Async.FromContinuations (fun (ok, err, canc) ->
    ok 3
)

let c = System.Threading.SynchronizationContext.Current
async {
    do! Async.SwitchToThreadPool()
    // work
    do! Async.SwitchToContext c
    // UI work
}

// ---

let primesSquaredSmall =
    query { 
        for p in primes |> Seq.take 10 do
        where (p < 10)
        select (p * p)
    }

primesSquaredSmall |> Seq.iter printNum

// ---

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


type MaybeBuilder() =

    member this.Bind(x, cont) = 
        match x with
        | None -> None
        | Some a -> cont a

    member this.Return(x) = 
        Some x

let maybe = new MaybeBuilder()

let z1 =
    match Some 3 with
    | None -> 0
    | Some x ->
    match Some 2 with 
    | None -> 0
    | Some y ->
    x + y

let z2 =
    maybe {
        let! x = Some 3
        let! y = Some 2
        return x + y
    }