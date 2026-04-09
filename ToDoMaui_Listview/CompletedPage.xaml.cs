using System.Collections.ObjectModel;

namespace ToDoMaui_Listview;

public partial class CompletedPage : ContentPage
{
    private ApiService _api = new ApiService();
    public ObservableCollection<ToDoClass> CompletedToDos { get; set; } = new ObservableCollection<ToDoClass>();
    private ToDoClass? _selectedToDo;
    private int _currentUserId;

    public CompletedPage(int userId)
    {
        InitializeComponent();
        _currentUserId = userId;
        completedLV.ItemsSource = CompletedToDos;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var items = await _api.GetItemsAsync(_currentUserId, "inactive");
        CompletedToDos.Clear();
        foreach (var item in items) CompletedToDos.Add(item);
    }

    private void TriggerEditMode(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is ToDoClass item)
        {
            _selectedToDo = item;
            nameEntry.Text = item.item_name;
            descEntry.Text = item.item_description;

            editPanel.IsVisible = true;
        }
    }

    private async void SaveEditItem(object sender, EventArgs e)
    {
        if (_selectedToDo != null)
        {
            bool success = await _api.UpdateItemAsync(_selectedToDo.item_id, nameEntry.Text, descEntry.Text);

            if (success)
            {
                _selectedToDo.item_name = nameEntry.Text;
                _selectedToDo.item_description = descEntry.Text;
            }
            else
            {
                await DisplayAlert("Error", "Failed to update task. Please try again.", "OK");
            }

            CancelEdit(null, null);
        }
    }

    private void CancelEdit(object? sender, EventArgs e)
    {
        nameEntry.Text = string.Empty;
        descEntry.Text = string.Empty;
        _selectedToDo = null;
        editPanel.IsVisible = false;
    }

    private async void DeleteToDoItem(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is ToDoClass itemToDelete)
        {
            bool success = await _api.DeleteItemAsync(itemToDelete.item_id);

            if (success)
            {
                CompletedToDos.Remove(itemToDelete);
                if (_selectedToDo == itemToDelete) CancelEdit(null, null);
            }
            else
            {
                await DisplayAlert("Error", "Failed to delete task. Please try again.", "OK");
            }
        }
    }

    // Unchecking the box = mark as active again via API
    private async void OnTaskCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox cb && cb.BindingContext is ToDoClass task)
        {
            if (!e.Value)
            {
                bool success = await _api.ChangeStatusAsync(task.item_id, "active");

                if (success)
                {
                    CompletedToDos.Remove(task);
                }
                else
                {
                    // Revert visually
                    task.status = "inactive";
                    await DisplayAlert("Error", "Failed to update task status.", "OK");
                }
            }
        }
    }
}