﻿namespace Avalonia.FuncUI.Components

open System
open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Templates
open Avalonia.FuncUI.Library
open Avalonia.FuncUI.Types
open Avalonia.FuncUI.Components.Hosts
open Avalonia.Data
open Avalonia.Data.Core
open System.Linq.Expressions

type DataTemplateView<'data>(
                             viewFunc: 'data -> IView,
                             matchFunc: ('data -> bool) voption,
                             itemsSource: Expression<Func<'data, 'data seq>> voption,
                             supportsRecycling: bool) = 
    
    member this.ViewFunc = viewFunc
    member this.MatchFunc = matchFunc
    member this.ItemsSource = itemsSource
    member this.SupportsRecycling = supportsRecycling
    
    override this.Equals (other: obj) : bool =
        match other with
        | :? DataTemplateView<'data> as other ->
            this.ViewFunc.GetType() = other.ViewFunc.GetType() &&
            this.MatchFunc.GetType() = other.MatchFunc.GetType() &&
            this.SupportsRecycling = other.SupportsRecycling
        | _ -> false
        
    override this.GetHashCode () =
        (this.ViewFunc.GetType(), this.SupportsRecycling).GetHashCode()
    
    interface ITreeDataTemplate with
        member this.SupportsRecycling =
            this.SupportsRecycling
            
        member this.ItemsSelector (item: obj) : InstancedBinding =
            match this.ItemsSource with
            | ValueNone -> null
            | ValueSome expression -> 
                match item with
                | :? 'data as data ->
                    InstancedBinding.OneWay(ExpressionObserver.Create(data, expression), BindingPriority.Style)
                | _ -> null

        member this.Match (data: obj) : bool =
            match data with
            | :? 'data as data -> true
            | _ -> false
            
        member this.Build (data: obj) : IControl =
            let host = HostControl()

            let update (data: 'data) : unit =
                let view = Some (this.ViewFunc data)
                (host :> IViewHost).Update view
            
            host
                .GetObservable(Control.DataContextProperty)
                .SubscribeWeakly<obj>((fun data ->
                    match data with
                    | :? 'data as t -> update(t)
                    | _ -> ()
                ), this) |> ignore
                
            host :> IControl

    /// <summary>
    /// Create a DataTemplateView for data matching type ('data)
    /// </summary>
    /// <typeparam name="'data">The Type of the data.</typeparam>
    /// <param name="viewFunc">The function that creates a view from the passed data.</param>
    static member create(viewFunc: 'data -> IView<'view>) : DataTemplateView<'data> =
        DataTemplateView<'data>(viewFunc = (fun a -> (viewFunc a) :> IView),
                                matchFunc = ValueNone,
                                itemsSource = ValueNone,
                                supportsRecycling = true)

    /// <summary>
    /// Create a DataTemplateView for data matching type ('data)
    /// </summary>
    /// <typeparam name="'data">The Type of the data.</typeparam>
    /// <param name="viewFunc">The function that creates a view from the passed data.</param>
    /// <param name="matchFunc">The function that decides if this template is capable of creating a view from the passed data.</param>     
    static member create(viewFunc: 'data -> IView<'view>, matchFunc: 'data -> bool) : DataTemplateView<'data> =
        DataTemplateView<'data>(viewFunc = (fun a -> (viewFunc a) :> IView),
                                matchFunc = ValueSome matchFunc,
                                itemsSource = ValueNone,
                                supportsRecycling = true)

    /// <summary>
    /// Create a DataTemplateView for data matching type ('data)
    /// </summary>
    /// <typeparam name="'data">The Type of the data.</typeparam>
    /// <param name="viewFunc">The function that creates a view from the passed data.</param>
    static member create(itemsSelector, viewFunc: 'data -> IView<'view>) : DataTemplateView<'data> =
        DataTemplateView<'data>(viewFunc = (fun a -> (viewFunc a) :> IView),
                                matchFunc = ValueNone,
                                itemsSource = ValueSome itemsSelector,
                                supportsRecycling = true)
    
    /// <summary>
    /// Create a DataTemplateView for data matching type ('data)
    /// </summary>
    /// <typeparam name="'data">The Type of the data.</typeparam>
    /// <param name="viewFunc">The function that creates a view from the passed data.</param>
    /// <param name="matchFunc">The function that decides if this template is capable of creating a view from the   passed data.</param>     
    static member create(itemsSelector, viewFunc: 'data -> IView<'view>, matchFunc: 'data -> bool) : DataTemplateView<'data> =
        DataTemplateView<'data>(viewFunc = (fun a -> (viewFunc a) :> IView),
                                matchFunc = ValueSome matchFunc,
                                itemsSource = ValueSome itemsSelector,
                                supportsRecycling = true)