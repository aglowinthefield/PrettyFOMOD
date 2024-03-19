using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Glow.PrettyFOMOD.Library;
using ReactiveUI;

namespace Glow.PrettyFOMOD.GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        // this.WhenAnyValue(o => o.Fomod)
        //     .Subscribe(o => this.RaisePropertyChanged(nameof(Fomod)));
        // this.WhenAnyValue(o => o.Fomod!.Info)
        //     .Subscribe(o => this.RaisePropertyChanged(nameof(Fomod.Info)));
        // this.WhenAnyValue(o => o.Fomod!.Info.Author)
        //     .Subscribe(o => this.RaisePropertyChanged(nameof(Fomod.Info.Author)));
    }

    public string Greeting => "Welcome to PrettyFOMOD! Select your mod folder to continue.";

    private string? _SelectedFolder;

    #region Info XML

    private string? _name;
    private string? _author;
    private string? _version;
    private string? _website;
    private string? _description;

    #endregion

    private FOMOD _fomod = new();

    public string? SelectedFolder
    {
        get => _SelectedFolder;
        set
        {
            this.RaiseAndSetIfChanged(ref _SelectedFolder, value);
            if (value == null) return;
            Console.WriteLine($"Instantiating FOMOD with selected folder {value}");
            Fomod = new FOMOD(value);
            
            // Set info
            Author      = Fomod.Info.Author;
            Name        = Fomod.Info.Name;
            Description = Fomod.Info.Description;
            Version     = Fomod.Info.Version;
            Website     = Fomod.Info.Website;
        }
    }

    public FOMOD? Fomod
    {
        get => _fomod;
        set
        {
            Console.WriteLine("Updating FOMOD info");
            this.RaiseAndSetIfChanged(ref _fomod, value);
        }
    }

    public string? Author
    {
        get => _author;
        set => this.RaiseAndSetIfChanged(ref _author, value);
    }

    public string? Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public string? Version
    {
        get => _version;
        set => this.RaiseAndSetIfChanged(ref _version, value);
    }

    public string? Website
    {
        get => _website;
        set => this.RaiseAndSetIfChanged(ref _website, value);
    }

    public string? Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }
}