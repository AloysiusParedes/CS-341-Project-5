//
// F# library to analyze Chicago crime data
//
// <<Aloysius Paredes>>
// U. of Illinois, Chicago
// CS341, Fall 2016
// Homework 5
//

module FSAnalysis
#light
open System
open FSharp.Charting
open FSharp.Charting.ChartTypes
open System.Drawing
open System.Windows.Forms

// Parse one line of CSV data:
//   Date,IUCR,Arrest,Domestic,Beat,District,Ward,Area,Year
//   09/03/2015 11:57:00 PM,0820,true,false,0835,008,18,66,2015
//   ...
// Returns back a tuple with most of the information:
//   (date, iucr, arrested, domestic, area, year)
// as string*string*bool*bool*int*int.
let private ParseOneCrime (line : string) = 
  let elements = line.Split(',')
  let date = elements.[0]
  let iucr = elements.[1]
  let arrested = Convert.ToBoolean(elements.[2])
  let domestic = Convert.ToBoolean(elements.[3])
  let area = Convert.ToInt32(elements.[elements.Length - 2])
  let year = Convert.ToInt32(elements.[elements.Length - 1])
  (date, iucr, arrested, domestic, area, year)

// Parse file of crime data, where the format of each line 
// is discussed above; returns back a list of tuples of the
// form shown above.
// NOTE: the "|>" means pipe the data from one function to
// the next.  The code below is equivalent to letting a var
// hold the value and then using that var in the next line:
//  let LINES  = System.IO.File.ReadLines(filename)
//  let DATA   = Seq.skip 1 LINES
//  let CRIMES = Seq.map ParseOneCrime DATA
//  Seq.toList CRIMES
let private ParseCrimeData filename = 
  System.IO.File.ReadLines(filename)
  |> Seq.skip 1  // skip header row:
  |> Seq.map ParseOneCrime
  |> Seq.toList


let private ParseOneIUCR (line : string) =
  let elements = line.Split(',')
  let iucr = elements.[0]
  let primary = elements.[1]
  let secondary = elements.[2]
  (iucr, primary, secondary)

let private ParseCrimeIUCR filename2 =
  System.IO.File.ReadLines(filename2)
  |> Seq.skip 1  // skip header row:
  |> Seq.map ParseOneIUCR
  |> Seq.toList

let private ParseOneArea (line : string) =
  let elements = line.Split(',')
  let number = elements.[0]
  let community = elements.[1]
  (number, community)

let private ParseArea filename2 = 
  System.IO.File.ReadLines(filename2)
  |> Seq.skip 1  // skip header row:
  |> Seq.map ParseOneArea
  |> Seq.toList


// Given a list of crime tuples, returns a count of how many 
// crimes were reported for the given year:
let private CrimesThisYear crimes crimeyear = 
  let crimes2 = List.filter (fun (_, _, _, _, _, year) -> year = crimeyear) crimes
  let numCrimes = List.length crimes2
  numCrimes


// CrimesByYear:
// Given a CSV file of crime data, analyzes # of crimes by year, 
// returning a chart that can be displayed in a Windows desktop
// app:
let CrimesByYear(filename) = 
  // debugging:  print filename, which appears in Visual Studio's Output window
  printfn "Calling CrimesByYear: %A" filename
  let crimes = ParseCrimeData filename
  let (_, _, _, _, _, minYear) = List.minBy (fun (_, _, _, _, _, year) -> year) crimes
  let (_, _, _, _, _, maxYear) = List.maxBy (fun (_, _, _, _, _, year) -> year) crimes
  let range  = [minYear .. maxYear]

  let counts = List.map (fun year -> CrimesThisYear crimes year) range
  let countsByYear = List.map2 (fun year count -> (year, count)) range counts

  printfn "Counts: %A" counts
  printfn "Counts by Year: %A" countsByYear

  let myChart = 
    Chart.Line(countsByYear, Name="Total # of Crimes")
  let myChart2 = 
    myChart.WithTitle(filename).WithLegend();
  let myChartControl = 
    new ChartControl(myChart2, Dock=DockStyle.Fill)

  myChartControl

////////////////////////////////////////////////////////////
// Given a list of crime tuples, returns a count of how many 
// arrests were reported for the given year
let private ArrestsThisYear crimes crimeyear = 
  let crimes2 = List.filter (fun (_, _, _, _, _, year) -> year = crimeyear) crimes
  //find the crimes with arrests == true
  let arrests2 = List.filter (fun (_, _, arrest, _, _, _) -> arrest = true) crimes2
  let numArrests = List.length arrests2
  numArrests

////////////////////////////////////////////////////////////////
//Arrest %: total crimes per year vs. number of arrests per year
let CrimesVsArrests(filename) =
  printfn "Calling CrimesVsArrests: %A" filename    
  let crimes = ParseCrimeData filename
  let (_, _, _, _, _, minYear) = List.minBy (fun (_, _, _, _, _, year) -> year) crimes
  let (_, _, _, _, _, maxYear) = List.maxBy (fun (_, _, _, _, _, year) -> year) crimes
  let range  = [minYear .. maxYear]

  let counts = List.map (fun year -> CrimesThisYear crimes year) range
  let countsByYear = List.map2 (fun year count -> (year, count)) range counts

  let arrestscounts = List.map (fun year -> ArrestsThisYear crimes year) range
  let arrestsByYear = List.map2 (fun year count -> (year, count)) range arrestscounts

  printfn "Counts: %A" counts
  printfn "Counts by Year: %A" countsByYear
  printfn "Arrests: %A" arrestscounts
  printfn "Arrests by Year %A" arrestsByYear

  //plot:
  let myChart =
    //combine the two charts
    Chart.Combine([
                    Chart.Line(countsByYear, Name = "Total # of Crimes")
                    Chart.Line(arrestsByYear, Name = "# of Arrests")
                  ])
  let myChart2 =
    myChart.WithTitle(filename).WithLegend();
  let myChartControl =
    new ChartControl(myChart2, Dock = DockStyle.Fill)

  //return the chart for display
  myChartControl


////////////////////////////////////////////////////////////
// Given a list of crime tuples, returns a count of how many 
// arrests were reported for the given year
let private IUCRThisYear crimes crimeyear code = 
  let crimes2 = List.filter (fun (_, _, _, _, _, year) -> year = crimeyear) crimes
  //find the crimes with arrests == true
  let arrests2 = List.filter (fun (_, iucr, _, _, _, _) -> iucr = code) crimes2
  let numArrests = List.length arrests2
  numArrests

let private getIUCR iucrCodes iucrCode = 
  let crime = List.filter (fun (iucr, _, _) -> iucr = iucrCode) iucrCodes
  crime

///////////////////////////////////////////////////////////////////////////
//By Crime: total crimes per year vs. number of a particular crime per year
let GivenCrimeByYear(filename, filename2, iucr) =
  printfn "Calling GivenCrimeByYear: %A" filename    

  let crimes = ParseCrimeData filename
  let iucrCodes = ParseCrimeIUCR filename2

  //find the iucr
  let crimeIUCR = getIUCR iucrCodes iucr

  if List.length crimeIUCR = 1 then
      //get the name
      let description = crimeIUCR.Head
      let code, primary, secondary = description
      let crimeName = primary + ": " + secondary

      let (_, _, _, _, _, minYear) = List.minBy (fun (_, _, _, _, _, year) -> year) crimes
      let (_, _, _, _, _, maxYear) = List.maxBy (fun (_, _, _, _, _, year) -> year) crimes
      let range  = [minYear .. maxYear]

      let counts = List.map (fun year -> CrimesThisYear crimes year) range
      let countsByYear = List.map2 (fun year count -> (year, count)) range counts

      let iucrcounts = List.map (fun year -> IUCRThisYear crimes year code) range
      let iucrByYear = List.map2 (fun year count -> (year, count)) range iucrcounts

      printfn "Counts: %A" counts
      printfn "Counts by Year: %A" countsByYear
      printfn "IUCR: %A" iucrcounts
      printfn "IUCR by Year %A" iucrByYear
  
      //plot:
      let myChart =
        //combine the two charts
        Chart.Combine([
                        Chart.Line(countsByYear, Name = "Total # of Crimes")
                        Chart.Line(iucrByYear, Name = crimeName)
                      ])
      let myChart2 =
        myChart.WithTitle(filename).WithLegend();
      let myChartControl =
        new ChartControl(myChart2, Dock = DockStyle.Fill)

      //return the chart for display
      myChartControl
  else
      let zeroPlot = [(2013,0); (2014,0); (2015,0)]
      let (_, _, _, _, _, minYear) = List.minBy (fun (_, _, _, _, _, year) -> year) crimes
      let (_, _, _, _, _, maxYear) = List.maxBy (fun (_, _, _, _, _, year) -> year) crimes
      let range  = [minYear .. maxYear]

      let counts = List.map (fun year -> CrimesThisYear crimes year) range
      let countsByYear = List.map2 (fun year count -> (year, count)) range counts


      printfn "Counts: %A" counts
      printfn "Counts by Year: %A" countsByYear
      printfn "IUCR: %A" 0
      printfn "IUCR by Year %A" 0
  
      //plot:
      let myChart =
        //combine the two charts
        Chart.Combine([
                        Chart.Line(countsByYear, Name = "Total # of Crimes")
                        Chart.Line(zeroPlot, Name = "unknown crime code")
                      ])
      let myChart2 =
        myChart.WithTitle(filename).WithLegend();
      let myChartControl =
        new ChartControl(myChart2, Dock = DockStyle.Fill)

      //return the chart for display
      myChartControl
 


////////////////////////////////////////////////////////////
// Given a list of crime tuples, returns a count of how many 
// arrests were reported for the given year
let private AreaThisYear crimes crimeyear areaNum = 
  let crimes2 = List.filter (fun (_, _, _, _, _, year) -> year = crimeyear) crimes
  let area2 = List.filter (fun (_, _, _, _, area, _) -> area = areaNum) crimes2
  let numArea = List.length area2
  numArea

let private getArea area communityNum = 
  let area = List.filter (fun (_, community) -> community = communityNum) area
  area

///////////////////////////////////////////////////////////////////////////////////////////////
//By Area: total crimes per year vs. number of crimes that occurred in a particular area of the city
let CrimesByArea(filename, filename2, community) =
  let zeroPlot = [(2013,0); (2014,0); (2015,0)]
  printfn "Calling CrimesByArea: %A" filename    

  let crimes = ParseCrimeData filename
  let areas = ParseArea filename2


  //find the area
  let area = getArea areas community
  
  if List.length area = 1 then
      let areaNum = Convert.ToInt32(fst area.Head)
      let areaCommunity = snd area.Head
      let (_, _, _, _, _, minYear) = List.minBy (fun (_, _, _, _, _, year) -> year) crimes
      let (_, _, _, _, _, maxYear) = List.maxBy (fun (_, _, _, _, _, year) -> year) crimes
      let range  = [minYear .. maxYear]

      let counts = List.map (fun year -> CrimesThisYear crimes year) range
      let countsByYear = List.map2 (fun year count -> (year, count)) range counts

      let areacounts = List.map (fun year -> AreaThisYear crimes year areaNum) range
      let areaByYear = List.map2 (fun year count -> (year, count)) range areacounts

      printfn "Counts: %A" counts
      printfn "Counts by Year: %A" countsByYear
      printfn "Area code: %A" areaNum
      printfn "Area: %A" areacounts
      printfn "Area by Year %A" areaByYear

      //plot:
      let myChart =
        //combine the two charts
        Chart.Combine([
                      Chart.Line(countsByYear, Name = "Total # of Crimes")
                      Chart.Line(areaByYear, Name = community)
                      ])
      let myChart2 = myChart.WithTitle(filename).WithLegend();
      let myChartControl = new ChartControl(myChart2, Dock = DockStyle.Fill)

      //return the chart for display
      myChartControl
  else
      let (_, _, _, _, _, minYear) = List.minBy (fun (_, _, _, _, _, year) -> year) crimes
      let (_, _, _, _, _, maxYear) = List.maxBy (fun (_, _, _, _, _, year) -> year) crimes
      let range  = [minYear .. maxYear]

      let counts = List.map (fun year -> CrimesThisYear crimes year) range
      let countsByYear = List.map2 (fun year count -> (year, count)) range counts

      printfn "Counts: %A" counts
      printfn "Counts by Year: %A" countsByYear
      printfn "Area code: 0"
      printfn "Area: %A" 0
      printfn "Area by Year %A" 0

      //plot:
      let myChart =
        //combine the two charts
        Chart.Combine([
                      Chart.Line(countsByYear, Name = "Total # of Crimes")
                      Chart.Line(zeroPlot, Name = "unknown area")
                      ])
      let myChart2 = myChart.WithTitle(filename).WithLegend();
      let myChartControl = new ChartControl(myChart2, Dock = DockStyle.Fill)

      //return the chart for display
      myChartControl


////////////////////////////////////////////////////////////
// Given a list of crime tuples, returns a count of how many 
// arrests were reported for the given year
let private CrimesThisArea crimes areaNum = 
  let area2 = List.filter (fun (_, _, _, _, area, _) -> area = areaNum) crimes
  let numArea = List.length area2
  numArea


///////////////////////////////////////////////////////////////////////////////////////////////
//Chicago: total crimes per year vs. number of crimes that occurred in a particular area of the city
let CrimesChicago(filename, filename2) =
  printfn "Calling CrimesChicago: %A" filename    

  let crimes = ParseCrimeData filename
  let areas = ParseArea filename2

  let numAreas = List.length areas
  let areaNums = [(1) .. (numAreas)]


  let counts = List.map (fun area -> CrimesThisArea crimes area) areaNums
  let countsByArea = List.map2 (fun area count -> (area, count)) areaNums counts

  printfn "Counts: %A" counts
  printfn "Counts by Area: %A" countsByArea


  let myChart = 
    Chart.Line(countsByArea, Name = "Total Crimes by Chicago Area")
  let myChart2 = 
    myChart.WithTitle(filename).WithLegend();
  let myChartControl = 
    new ChartControl(myChart2, Dock=DockStyle.Fill)

  myChartControl

