using System;
using System.Linq;

namespace RunnethOverStudio.AppToolkit.Presentation.MVP;

/// <summary>
/// Provides navigation between presenters in the Model-View-Presenter (MVP) pattern. 
/// Manages the life-cycle of presenters and their associated views, allowing transitions between different presenters using dependency injection.
/// </summary>
/// <remarks>
/// Inspired by <see href="https://stackoverflow.com/a/13030590">this answer</see> by tcarvin.
/// </remarks>
public class Navigator(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private Presenter? _current;

    /// <summary>
    /// Navigates to the specified presenter type by resolving it from the dependency injection container,
    /// dismissing the current presenter (if any), and displaying the associated view.
    /// </summary>
    /// <param name="nextPresenterType">
    /// The <see cref="Type"/> of the next presenter to navigate to. Must derive from <see cref="Presenter"/>.
    /// </param>
    public void NavigateTo(Type nextPresenterType)
    {
        if (typeof(Presenter).IsAssignableFrom(nextPresenterType) &&
            _serviceProvider.GetService(nextPresenterType) is Presenter nextPresenter)
        {
            NavigateTo(nextPresenter);
        }
    }

    private void NavigateTo(Presenter nextPresenter)
    {
        _current?.Dismiss();

        _current = nextPresenter;
        _current.Display(FindView(nextPresenter));
    }

    private static View FindView(Presenter presenter)
    {
        string? name = presenter.GetType().FullName!.Replace(nameof(Presenter), nameof(View));

        // Look for the view type in all assemblies in-case the view is not in the same assembly as the presenter.
        Type? type = AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(a => a.GetType(name))
            .FirstOrDefault(t => t != null);

        if (type != null && Activator.CreateInstance(type) is View view)
        {
            return view;
        }

        throw new Exception("View not found for presenter. Presenters and Views must be named identically, except for their respective \"Presenter\" and \"View\" suffixes.");
    }
}
