using RunnethOverStudio.AppToolkit.Core;
using Spectre.Console;
using System;

namespace RunnethOverStudio.AppToolkit.Presentation.MVP;

/// <summary>
/// Provides a base class for views that use Spectre.Console for rich CLI presentation in the MVP pattern.
/// Handles displaying process failures and clearing the console with application branding.
/// </summary>
public abstract class View
{
    /// <summary>
    /// Represents the stylized ASCII art text for the application title.
    /// </summary>
    /// <remarks>
    /// Optionally set this field once at application startup (e.g., <c>SpectreView.AppTitleFiglet = new FigletText("My App");</c>)
    /// so that all derived views will display this title when clearing the console.
    /// </remarks>
    public static readonly FigletText? AppTitleFiglet;

    /// <summary>
    /// The format string used for displaying prompts in a CLI application.
    /// </summary>
    protected const string PROMPT_FORMAT = "{0}: ";

    /// <summary>
    /// Presents the view to the user. Must be implemented by derived classes.
    /// </summary>
    public abstract void Show();

    /// <summary>
    /// Displays information about a failed process to the user using Spectre.Console formatting.
    /// Can be overridden to provide custom failure handling or messaging.
    /// </summary>
    public virtual void ShowProcessFail(string processName, ProcessResult processResult)
    {
        if (!processResult.Errors.IsEmpty)
        {
            string errorMessage = processResult.ToStringBulleted('*');

            AnsiConsole.MarkupLineInterpolated($"[red]{processName} failed[/]: {errorMessage}");
        }
        else
        {
            AnsiConsole.MarkupLineInterpolated($"[red]{processName} failed[/].");
        }

        AnsiConsole.Write("Press any key to return...");

        Console.ReadKey();
    }

    /// <summary>
    /// Clears the console and displays the application title using Spectre.Console.
    /// Can be overridden to provide custom clearing logic.
    /// </summary>
    protected virtual void Clear()
    {
        AnsiConsole.Clear();

        if (AppTitleFiglet != null)
        {
            AnsiConsole.Write(AppTitleFiglet);
            AnsiConsole.Write(Environment.NewLine);
        }
    }
}
