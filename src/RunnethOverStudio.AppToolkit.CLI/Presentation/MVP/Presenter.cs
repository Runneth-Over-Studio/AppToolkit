using System;

namespace RunnethOverStudio.AppToolkit.Presentation.MVP;

/// <summary>
/// Represents the base class for all presenters in the Model-View-Presenter (MVP) pattern.
/// </summary>
public abstract class Presenter(Navigator navigator)
{
    /// <summary>
    /// Gets or sets the <see cref="MVP.Navigator"/> used to manage navigation between presenters.
    /// </summary>
    protected Navigator Navigator { get; set; } = navigator;

    /// <summary>
    /// Displays the specified <see cref="View"/>.
    /// </summary>
    /// <param name="view">The view to display.</param>
    public abstract void Display(View view);

    /// <summary>
    /// Dismisses the <see cref="View"/>.
    /// </summary>
    /// <remarks>
    /// Override to provide custom dismissal logic.
    /// </remarks>
    public virtual void Dismiss() { }
}
