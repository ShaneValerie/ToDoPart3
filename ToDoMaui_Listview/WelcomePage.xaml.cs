//dli nani mogamit ug databasehelperr
namespace ToDoMaui_Listview;

public partial class WelcomePage : ContentPage
{
    private ApiService _apiService = new ApiService();

    public WelcomePage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(emailEntry.Text) || string.IsNullOrWhiteSpace(passwordEntry.Text))
            {
                await DisplayAlert("Wait", "Please enter email and password.", "OK");
                return;
            }

            // Call the Live API!
            var response = await _apiService.SignInAsync(emailEntry.Text, passwordEntry.Text);

            // API Docs state success status is 200
            if (response != null && response.status == 200)
            {
                passwordEntry.Text = string.Empty;

                // Pass the LIVE User ID and their first name into the app
                Application.Current.MainPage = new MainTabbedPage(response.data.id, response.data.fname);
            }
            else
            {
                // API Docs state error status is 400
                await DisplayAlert("Error", response?.message ?? "Invalid credentials.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Connection Error", $"Could not connect to server: {ex.Message}", "OK");
        }
    }

    private async void GoToSignUp(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new SignUpPage());
    }
}