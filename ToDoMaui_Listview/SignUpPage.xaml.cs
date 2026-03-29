namespace ToDoMaui_Listview;

public partial class SignUpPage : ContentPage
{
    private ApiService _apiService = new ApiService();

    public SignUpPage()
    {
        InitializeComponent();
    }

    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fNameEntry.Text) || string.IsNullOrWhiteSpace(lNameEntry.Text) ||
                string.IsNullOrWhiteSpace(emailEntry.Text) || string.IsNullOrWhiteSpace(passEntry.Text))
            {
                await DisplayAlert("Wait", "Please fill out all fields.", "OK");
                return;
            }

            if (passEntry.Text != confirmPassEntry.Text)
            {
                await DisplayAlert("Error", "Passwords do not match.", "OK");
                return;
            }

            // Call the Live API!
            var response = await _apiService.SignUpAsync(fNameEntry.Text, lNameEntry.Text, emailEntry.Text, passEntry.Text, confirmPassEntry.Text);

            // API Docs state success status is 200
            if (response != null && response.status == 200)
            {
                await DisplayAlert("Success!", response.message, "OK");
                await Navigation.PopAsync(); // Go back to login
            }
            else
            {
                await DisplayAlert("Error", response?.message ?? "Failed to create account.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Connection Error", $"Could not connect to server: {ex.Message}", "OK");
        }
    }

    private async void GoBackToLogin(object sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}