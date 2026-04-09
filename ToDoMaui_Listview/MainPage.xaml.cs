using System.Collections.ObjectModel;

namespace ToDoMaui_Listview;

public partial class MainPage : ContentPage
{
    private ApiService _api = new ApiService();
    public ObservableCollection<ToDoClass> ToDos { get; set; } = new ObservableCollection<ToDoClass>();
    private ToDoClass? _selectedToDo;
    private int _currentUserId;

    public MainPage(int userId)
    {
        InitializeComponent();
        _currentUserId = userId;
        todoLV.ItemsSource = ToDos;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var items = await _api.GetItemsAsync(_currentUserId, "active");
        ToDos.Clear();
        foreach (var item in items) ToDos.Add(item);
    }

    private async void AddToDoItem(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(nameEntry.Text) || string.IsNullOrWhiteSpace(descEntry.Text))
        {
            await DisplayAlert("Wait!", "Please enter both a tag and a detail.", "OK");
            return;
        }

        bool success = await _api.AddItemAsync(nameEntry.Text, descEntry.Text, _currentUserId);

        if (success)
        {
            // Refresh list from API so we get the real item_id back
            var items = await _api.GetItemsAsync(_currentUserId, "active");
            ToDos.Clear();
            foreach (var item in items) ToDos.Add(item);
            ClearInputs();
        }
        else
        {
            await DisplayAlert("Error", "Failed to add task. Please try again.", "OK");
        }
    }

    private void TriggerEditMode(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is ToDoClass item)
        {
            _selectedToDo = item;
            nameEntry.Text = item.item_name;
            descEntry.Text = item.item_description;

            addBtn.IsVisible = false;
            editBtn.IsVisible = true;
            cancelBtn.IsVisible = true;
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
        ClearInputs();
        addBtn.IsVisible = true;
        editBtn.IsVisible = false;
        cancelBtn.IsVisible = false;
    }

    private async void DeleteToDoItem(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is ToDoClass itemToDelete)
        {
            bool success = await _api.DeleteItemAsync(itemToDelete.item_id);

            if (success)
            {
                ToDos.Remove(itemToDelete);
                if (_selectedToDo == itemToDelete) CancelEdit(null, null);
            }
            else
            {
                await DisplayAlert("Error", "Failed to delete task. Please try again.", "OK");
            }
        }
    }

    // Checking the box = mark as inactive (done) via API
    private async void OnTaskCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox cb && cb.BindingContext is ToDoClass task)
        {
            if (e.Value)
            {
                bool success = await _api.ChangeStatusAsync(task.item_id, "inactive");

                if (success)
                {
                    ToDos.Remove(task);
                }
                else
                {
                    // Revert the checkbox visually
                    task.status = "active";
                    await DisplayAlert("Error", "Failed to update task status.", "OK");
                }
            }
        }
    }

    private void ClearInputs()
    {
        nameEntry.Text = string.Empty;
        descEntry.Text = string.Empty;
        _selectedToDo = null;
    }
}