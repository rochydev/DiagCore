using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DiagCore.App.ViewModels;

/// <summary>
/// Common state for every section view model. Carries the three flags every
/// data-bound section needs (loading, error, has-data) and a shared
/// <see cref="RefreshAsync"/> entry point that derived classes implement.
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasLoadedOnce;

    /// <summary>True while there is no data to show (and no error) — useful for empty states.</summary>
    public bool IsEmpty => !IsLoading && string.IsNullOrEmpty(ErrorMessage) && !HasLoadedOnce;

    partial void OnIsLoadingChanged(bool value) => OnPropertyChanged(nameof(IsEmpty));
    partial void OnErrorMessageChanged(string? value) => OnPropertyChanged(nameof(IsEmpty));
    partial void OnHasLoadedOnceChanged(bool value) => OnPropertyChanged(nameof(IsEmpty));

    /// <summary>
    /// Runs <see cref="LoadAsync"/> guarded by the loading flag and surfaces
    /// exceptions through <see cref="ErrorMessage"/> rather than letting them
    /// reach the dispatcher.
    /// </summary>
    [RelayCommand]
    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        if (IsLoading) return;
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            await LoadAsync(cancellationToken).ConfigureAwait(true);
            HasLoadedOnce = true;
        }
        catch (OperationCanceledException)
        {
            // Caller will surface this as needed.
            throw;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Override to perform the data fetch. Implementations should populate
    /// observable state and propagate <paramref name="cancellationToken"/>
    /// to every awaited call.
    /// </summary>
    protected abstract Task LoadAsync(CancellationToken cancellationToken);
}
