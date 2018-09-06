﻿using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace DunGen.CLI
{
  internal class MenuItem
  {
    public static readonly string HelpOptionString = "-?|-h|--help";

    private List<MenuItem> _children = new List<MenuItem>();

    public bool IsRoot { get; set; }
    public MenuItem Parent { get; private set; } = null;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<MenuItem> Children
    {
      get
      {
        return _children;
      }
      set
      {
        _children = value;
        foreach (var item in _children)
        {
          item.Parent = this;
        }
      }
    }
    public Action<CommandLineApplication> Configure { get; set; }
    public Func<string> StatusMessage { get; set; } = null;

    public MenuItem(string Name, bool IsRoot = false)
    {
      this.Name = Name;
      this.IsRoot = IsRoot;
    }

    public void LoadChildPredicates(CommandLineApplication thisPred)
    {
      for (int i = 0; i < Children.Count; ++i)
      {
        thisPred.Command(Children[i].Name, (cmd) => { Children[i].SelfConfigure(cmd); });
      }
    }

    public void SelfConfigure(CommandLineApplication pred)
    {
      pred.Description = Description;
      pred.HelpOption(HelpOptionString);

      LoadChildPredicates(pred);

      pred.OnExecute(() =>
      {
        int statusVal = 0;
        bool quitInteractive = false;

        while (false == quitInteractive)
        {
          CommandLineApplication interactiveEditor = new CommandLineApplication();

          interactiveEditor.OnExecute(() =>
          {
            interactiveEditor.ShowHelp();
            return 0;
          });

          LoadChildPredicates(interactiveEditor);
          interactiveEditor.Command("<", (cmd) =>
          {
            cmd.Description = IsRoot ? "Quit the program" : "Go up one menu level";
            cmd.HelpOption(HelpOptionString);

            cmd.OnExecute(() =>
            {
              quitInteractive = true;
              return 0;
            });
          });

          interactiveEditor.Command("*", (cmd) =>
          {
            var fromRootOption = cmd.Option("-r|--root",
              "Print commands from the root of the menu",
              CommandOptionType.NoValue);
            cmd.Description = "Print available commands";
            cmd.ExtendedHelpText = "Defaults to printing only child commands. Use -r to print the whole menu";
            cmd.HelpOption(HelpOptionString);

            cmd.OnExecute(() =>
            {
              MenuItem printRoot = this;
              if (fromRootOption.HasValue())
              {
                while (printRoot.Parent != null) printRoot = printRoot.Parent;
              }
              printRoot.PrintPretty(Console.Out, " ", false, this);
              return 0;
            });
          });

          Console.WriteLine();
          Console.WriteLine("MENU BREADCRUMB: {0}", GetLocationString());
          interactiveEditor.ShowHelp();

          if (null != StatusMessage) Console.WriteLine(StatusMessage());
          Console.WriteLine("What do you want to do?");
          Console.Write(">");
          string input = Console.ReadLine();
          string[] inputArgs = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

          try
          {
            statusVal = interactiveEditor.Execute(inputArgs);
          }
          catch (CommandParsingException ex)
          {
            Console.WriteLine(ex.Message);
          }
          catch (ArgumentNullException ex)
          {
            Console.WriteLine(ex.Message);
          }
          catch (Exception ex)
          {
            Console.WriteLine("Something went wrong processing your command.");
            Console.WriteLine("Error: {0}", ex.Message);
          }
        }
        return statusVal;
      });

      // Overwrite defaults from above, if any are specified
      this.Configure?.Invoke(pred);
    }

    private string GetLocationString()
    {
      Stack<MenuItem> parents = new Stack<MenuItem>();
      MenuItem currentLoc = this;
      while (currentLoc != null)
      {
        parents.Push(currentLoc);
        currentLoc = currentLoc.Parent;
      }

      return String.Join(" > ", parents.Select((mi) => mi.Name));
    }
  }

  internal static class Extensions
  {
    internal static void PrintPretty(this MenuItem printRoot, TextWriter writer, string indent, bool last, MenuItem me = null)
    {
      string toWrite = indent;
      bool useStar = (null != me && printRoot == me);
      if (last)
      {
        toWrite += @"└─";
        indent += "  ";
      }
      else
      {
        toWrite += @"├─";
        indent += "│ ";
      }

      writer.WriteLine("{0,-15} {1} {2}", toWrite + printRoot.Name, (useStar ? "*" : "-"), printRoot.Description);

      for (int i = 0; i < printRoot.Children.Count; i++)
      {
        printRoot.Children[i].PrintPretty(writer, indent, i == printRoot.Children.Count - 1, me);
      }
    }
  }
}
