module Names
open EEEHelpers
open CommonTypes
open System.Text.RegularExpressions

//------------------------------------------------------------------------------------------------------------------------------//
//--------------------------------------------FUNCTIONS TO DO TYPE REFLECTION---------------------------------------------------//
//------------------------------------------------------------------------------------------------------------------------------//

let getUnionCaseName x =
    sprintf "%A" x
    |> Seq.takeWhile System.Char.IsLetterOrDigit
    |> Seq.map string
    |> String.concat ""

let testUnionNames() =
    let testCases = [
        And
        Mux2
        Custom {Name="Mytype"; InputLabels=[]; OutputLabels=[]}
        NbitsAdder 16
        ]
    testCases 
    |> List.map getUnionCaseName
    |> List.iter (printfn "%s")

//------------------------------------------------------------------------------------------------------------------------------//
//---------------------------------------------------------TYPES----------------------------------------------------------------//
//------------------------------------------------------------------------------------------------------------------------------//

type Symbol =
    {
        
        Pos: XYPos
        InWidth0: int option
        InWidth1: int option
        Id : ComponentId       
        Compo : Component                 
        Colour: string
        ShowInputPorts: bool
        ShowOutputPorts: bool
        Opacity: float
        Moving: bool
    }

/// This is the Elmish Model type for the Symbol module
type Model = {
    Symbols: Map<ComponentId, Symbol>
    CopiedSymbols: Map<ComponentId, Symbol>
    Ports: Map<string, Port>                            // string since it's for both input and output ports

    InputPortsConnected:  Set<InputPortId>              // we can use a set since we only care if an input port 
                                                        // is connected or not (if so it is included) in the set 

    OutputPortsConnected: Map<OutputPortId, int>        // map of output port id to number of wires connected to that port
    }

//------------------------------------------------------------------------------------------------------------------------------//
//----------------------------------------------DEFAULT NAME GENERATION---------------------------------------------------------//
//------------------------------------------------------------------------------------------------------------------------------//

///Generates a label prefix for a component type
let generatePrefix compType = 
    match compType with
    | Not | And | Or | Xor | Nand | Nor | Xnor -> "G"
    | Mux2 -> "MUX"
    | Demux2 -> "DM"
    | NbitsAdder _ -> "A"
    | NbitsXor _ -> "XOR"
    | DFF | DFFE -> "FF"
    | Register _ | RegisterE _ -> "REG"
    | AsyncROM1 _ -> "AROM"
    | ROM1 _ -> "ROM"
    | RAM1 _ -> "RAM"
    | AsyncRAM1 _ -> "ARAM"
    | Custom c -> 
        c.Name.ToUpper() + (if c.Name |> Seq.last |> System.Char.IsDigit then "." else "")
    | Constant1 _ -> "C"
    | BusCompare _ -> "EQ"
    | Decode4 -> "DEC"
    | BusSelection _ -> "SEL"
    | _ -> ""

/// Returns the number of the component label (i.e. the number 1 from IN1)
let getNumber (str : string) = 
    let index = Regex.Match(str, @"\d+$")
    match index with
    | null -> 0
    | _ -> int index.Value

/// Filters symbols for all that match a given compType
let filterSymbols compType symbols =
    match compType with 
       | Not | And | Or | Xor | Nand | Nor | Xnor -> 
            symbols
            |> List.filter (fun sym ->
                (sym.Compo.Type = Not || sym.Compo.Type = And 
                || sym.Compo.Type = Or || sym.Compo.Type = Xor
                || sym.Compo.Type = Nand || sym.Compo.Type = Nor
                || sym.Compo.Type = Xnor)
                )
       | DFF | DFFE -> 
            symbols
            |> List.filter (fun sym ->
                (sym.Compo.Type = DFF || sym.Compo.Type = DFFE))
       //The following components require this pattern matching in order to correctly identify all of the components in the circuit of that type
       //Normally this is because they are defined by a width as well as a type
       | Register _ | RegisterE _ ->
            symbols
            |> List.filter (fun sym ->
                match sym.Compo.Type with 
                | Register _ | RegisterE _ -> true
                | _ -> false)
       | Constant1 _ ->
            symbols
            |> List.filter (fun sym ->
                match sym.Compo.Type with 
                | Constant1 _ -> true
                | _ -> false)
       | Input _ ->
           symbols
           |> List.filter (fun sym ->
               match sym.Compo.Type with 
               | Input _ -> true
               | _ -> false)
       | Output _ ->
           symbols
           |> List.filter (fun sym ->
               match sym.Compo.Type with 
               | Output _ -> true
               | _ -> false)
       | Viewer _ ->
           symbols
           |> List.filter (fun sym ->
               match sym.Compo.Type with 
               | Viewer _ -> true
               | _ -> false)
       | BusSelection _ ->
           symbols
           |> List.filter (fun sym ->
               match sym.Compo.Type with 
               | BusSelection _ -> true
               | _ -> false)
       | BusCompare _ ->
           symbols
           |> List.filter (fun sym ->
               match sym.Compo.Type with 
               | BusCompare _ -> true
               | _ -> false)
       | NbitsAdder _ ->
           symbols
           |> List.filter (fun sym ->
               match sym.Compo.Type with 
               | NbitsAdder _ -> true
               | _ -> false)
       | NbitsXor _ ->
           symbols
           |> List.filter (fun sym ->
               match sym.Compo.Type with 
               | NbitsXor _ -> true
               | _ -> false)
       | AsyncROM1 _ ->
           symbols
           |> List.filter (fun sym ->
               match sym.Compo.Type with 
               | AsyncROM1 _ -> true
               | _ -> false)
       | ROM1 _ ->
           symbols
           |> List.filter (fun sym ->
               match sym.Compo.Type with 
               | ROM1 _ -> true
               | _ -> false)
       | RAM1 _ ->
           symbols
           |> List.filter (fun sym ->
               match sym.Compo.Type with 
               | RAM1 _ -> true
               | _ -> false)
       | AsyncRAM1 _ ->
           symbols
           |> List.filter (fun sym ->
               match sym.Compo.Type with 
               | AsyncRAM1 _ -> true
               | _ -> false) //sym -> sym.Compo.Type = AsyncRAM1

       | _ ->
            symbols
            |> List.filter (fun sym -> sym.Compo.Type = compType)

/// Generates a label index to use as suffix
let generateNumber symbols compType =
    let filteredSymbols = 
        filterSymbols compType symbols

    match compType with
    | MergeWires | SplitWire _ -> ""
    | _ ->
        if List.isEmpty filteredSymbols then 1 
        else symbolList
            |> List.map (fun sym -> getNumber sym.Compo.Label)
            |> List.max
            |> (+) 1
        |> string


//----------------------------TOP LEVEL FUNCTION-----------------------------//
//        This function generates a new Issie default label (component name) //
//        Names are a short prefic followed by a (consecutive from 0) number //
//        e.g. G1, G2, G3. The prefic depends on the ComponentType.          //

///Generates the label for a component type
let generateLabel (model: Model) (compType: ComponentType) : string =
    let (prefix: string) = generatePrefix compType
    let (number: string) =
        let symbols = List.map snd (Map.toList model.Symbols)
        generateNumber symbols compType
    match compType with
    | IOLabel -> prefix
    | _ -> prefix + number

