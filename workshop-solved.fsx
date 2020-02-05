// In this hands-on session, we will explore a classic machine learning
// technique, k-means clustering. The goal is to identify if there are pattens
// in the data. Patterns in this case are clusters, that is, groups of 
// observation that have a similar profile.

// In our case, we will look into a classic dataset, the Iris dataset, from
// the UC Irvine machine learning repository:
// https://archive.ics.uci.edu/ml/datasets/Iris/

// The dataset has observations for 3 different species of Iris flowers, 
// setosa, versicolor and virginica (50 of each). We will use observable
// measurements for flowers, to try and group them into clusters, and check
// if the method actually finds group that correspond to the 3 species.

// The workshop is broken down in parts:
// - loading and visualizing data
// - writing the k-means clustering algorithm
// - potentially improving and generalizing the code



// -----------------------------------------------------------------------------
// 0. Getting set up
// -----------------------------------------------------------------------------

// Check the readme.md file for setup instructions

// Then go through the script, running it piece by piece
// You can execute snippets of code by selecting the code, and pressing 
// Alt+Enter (Cmd+Enter on a Mac).
// To validate that things work, try to run the following:

printfn "Hello, world!"



// -----------------------------------------------------------------------------
// 1. Loading the data in memory
// -----------------------------------------------------------------------------

// we can read all data in memory using File.ReadAllLines:

open System
open System.IO

let dataPath = Path.Combine (__SOURCE_DIRECTORY__, "iris.data")
let rawData = File.ReadAllLines dataPath

// we create a data structure (a Record) to represent each observation
type Flower = {
    Species: string
    SepalLength: float
    SepalWidth: float
    PetalLength: float
    PetalWidth: float
    }

// instantiating a Record is done by simply filling in its fields:
let myFlower = {
    Species = "Unknown"
    SepalLength = 10.0
    SepalWidth = 20.0
    PetalLength = 30.0
    PetalWidth = 40.0
    }

// We want to read the raw data, and convert every line into a record.
// To do that, we will use two things: Array.map, and Split

let exampleStringToSplit = "this,is,comma,separated"
let splitResult = exampleStringToSplit.Split ','

let originalArray = [| 1; 2; 3; 4; 5 |]
let arrayAfterMapping = originalArray |> Array.map (fun item -> item * 2)


// TODO: using Array.map and Split, read the file and convert it
// into an array of Flower(s):

let read file = 
    File.ReadAllLines file
    |> Array.map (fun line -> 
        let items = line.Split ','
        {
            SepalLength = items.[0] |> float
            SepalWidth = items.[1] |> float
            PetalLength = items.[2] |> float
            PetalWidth = items.[3] |> float
            Species = items.[4]
        }
        )

let data = 
    Path.Combine (__SOURCE_DIRECTORY__, "iris.data")
    |> read



// -----------------------------------------------------------------------------
// 2. Visualize the data with XPlot.GoogleCharts
// -----------------------------------------------------------------------------

#r "./packages/Google.DataTable.Net.Wrapper/lib/netstandard2.0/Google.DataTable.Net.Wrapper.dll"
#r "./packages/XPlot.GoogleCharts/lib/netstandard2.0/XPlot.GoogleCharts.dll"
#r "./packages/Newtonsoft.Json/lib/netstandard2.0/Newtonsoft.Json.dll"

open XPlot.GoogleCharts

// example usage

// line chart
[ 1; 3; 2; 8; 5; 8; 3 ]
|> Chart.Line
|> Chart.Show

// scatterplot
[ (1, 1); (3, 2); (2, 7); (3, 4); (1, 2) ]
|> Chart.Scatter
|> Chart.Show

// overlayed scatterplots
[
    [ (1, 1); (2, 2); (3, 3); (4, 4); (5, 5) ]
    [ (1, 2); (2, 3); (3, 4); (4, 5); (5, 6) ]
]
|> Chart.Scatter
|> Chart.Show

// using chart options
[ 1; 3; 2; 8; 5; 8; 3 ]
|> Chart.Line 
|> Chart.WithLabels [ "Series 1" ]
|> Chart.WithTitle "The title"
|> Chart.WithSize (800, 640)
|> Chart.WithXTitle "X Axis legend"
|> Chart.WithYTitle "Y Axis legend"
|> Chart.WithLegend true
|> Chart.Show


// TODO: scatterplot Petal Length against Petal Width
data
|> Array.map (fun x -> x.PetalLength, x.PetalWidth)
|> Chart.Scatter
|> Chart.Show


// TODO: same, with each flower species in a different series
// Array.groupBy should be useful here
data
|> Array.groupBy (fun flower -> flower.Species)
|> Array.map (fun (label, group) ->
    group
    |> Array.map (fun flower -> flower.PetalLength, flower.PetalWidth)
    )
|> Chart.Scatter
|> Chart.WithSize (800, 640)
|> Chart.Show



// -----------------------------------------------------------------------------
// 3. K-Means clustering
// -----------------------------------------------------------------------------

// K-Means finds K clusters in a dataset by performing these operations:
// At each step we have K centroids; each centroid is an average version of
// a group of flowers (a cluster).
// We assign each flower to the closest of the K centroid, forming K clusters
// We recompute / update the centroids for each of these new clusters
// If the centroids have not changed, we stop, otherwise, we repeat.

// First, we will code 2 components we need:
// computing the centroid of a group of flowers,
// computing the distance between 2 flowers, using Petal Length and Petal Width

// TODO: given a group of flowers, compute their centroid
let centroid (group: Flower []) =
    {
        Species = "Centroid"
        SepalLength = group |> Array.averageBy (fun x -> x.SepalLength)
        SepalWidth = group |> Array.averageBy (fun x -> x.SepalWidth)
        PetalLength = group |> Array.averageBy (fun x -> x.PetalLength)
        PetalWidth = group |> Array.averageBy (fun x -> x.PetalWidth)
    }

// test it out!
let datasetCentroid = centroid data

// TODO: given 2 flowers, compute their distance, using Petal Length and Petal Width
let distance flower1 flower2 = 
    pown (flower1.PetalLength - flower2.PetalLength) 2
    +
    pown (flower1.PetalWidth - flower2.PetalWidth) 2

// test it out!
distance data.[0] data.[1]

// We can now write a cluster function: given centroids and a dataset,
// we assign each flower to its closest centroid.
let clusters (centroids: Flower []) (data: Flower []) =
    data
    |> Array.groupBy (fun flower ->
        centroids 
        |> Array.minBy (fun centroid -> distance centroid flower)
        )

// TODO: complete the clusterize function, to recursively refine clusters
// until they do not change any more.
let clusterize (n: int) (data: Flower []) =

    let rec update currentClusters =
        printfn "Trying to refine clusters"
        let updatedCentroids = 
            currentClusters 
            |> Array.map (fun (_, cluster) -> centroid cluster)
        if 
            updatedCentroids 
            |> Array.forall (fun centroid -> 
                currentClusters |> Array.exists (fun (c, _) -> c = centroid)
                )
        then updatedCentroids
        else update (clusters updatedCentroids data) 

    let rng = Random 0

    let initCentroids = 
        data 
        |> Array.sortBy (fun _ -> rng.Next()) 
        |> Array.take n
    let initClusters = clusters initCentroids data
    update initClusters


// test it out!
let finalCentroids = clusterize 3 data


// TODO: count how many different species we have in each cluster.
data 
|> Array.map (fun flower -> 
    flower.Species, 
    finalCentroids |> Array.minBy (distance flower)
    )
|> Array.groupBy snd
|> Array.map (fun (centroid, group) -> centroid, group |> Seq.countBy fst)


// We can also plot our species, and the 3 clusters the algorithm found:
data
|> Array.groupBy (fun flower -> flower.Species)
|> Array.map (fun (label, group) ->
    group
    |> Array.map (fun flower -> flower.PetalLength, flower.PetalWidth)
    )
|> Array.append 
    [| 
        finalCentroids 
        |> Array.map (fun centroid -> centroid.PetalLength, centroid.PetalWidth)
    |]
|> Chart.Scatter
|> Chart.WithSize (800, 640)
|> Chart.Show



// -----------------------------------------------------------------------------
// 4. Bonus questions / food for thought
// -----------------------------------------------------------------------------

// Do we gain anything by including other characteristics (features)?
//   Problem: characteristics are on different scales: normalization

// Can we make the code more general?
//   Operate on Arrays instead of Records
//   Use different distances

// How do we know how many clusters there are? Why 3?
