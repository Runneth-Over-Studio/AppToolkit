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
    /// Gets or sets the application information used for branding and display in views.
    /// This property can be set at application startup to provide a consistent title, font, and color scheme
    /// for all derived views when rendering headers or clearing the console.
    /// </summary>
    public static ViewBranding? ViewBranding { get; set; }

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

        if (!string.IsNullOrEmpty(ViewBranding?.AppTitle))
        {
            // Display ASCII header if FIGlet font is provided, otherwise use a simple panel.
            if (ViewBranding.AppTitleFigletFont != null)
            {
                FigletText figlet = new FigletText(ViewBranding.AppTitleFigletFont, ViewBranding.AppTitle).Color(ViewBranding.PrimaryColor);
                AnsiConsole.Write(figlet);
            }
            else
            {
                AnsiConsole.Write(new Panel($"[{ViewBranding.PrimaryColor}]{ViewBranding.AppTitle}[/]")
                {
                    Border = BoxBorder.Heavy,
                    BorderStyle = new Style(ViewBranding.PrimaryColor)
                });
            }

            AnsiConsole.Write(Environment.NewLine);
        }
    }
}

/// <summary>
/// Represents application branding information, including the title, optional FIGlet font, and primary color.
/// Used by views to display consistent application headers and styling.
/// </summary>
public record ViewBranding
{
    /// <summary>
    /// The application title to display in the console header.
    /// </summary>
    public string? AppTitle { get; init; }

    /// <summary>
    /// The FIGlet font used to render the application title as ASCII art.
    /// If null, the app title is displayed as plain text within a styled panel.
    /// </summary>
    public FigletFont? AppTitleFigletFont { get; init; }

    /// <summary>
    /// The primary color used for application branding in the console UI.
    /// </summary>
    public Color PrimaryColor { get; init; } = Color.Green;
}
