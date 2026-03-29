using System.Net.Http.Json;
using System.Text.Json;

namespace ToDoMaui_Listview;

// These classes map perfectly to the JSON responses in your API documentation
public class ApiResponse
{
    public int status { get; set; }
    public string message { get; set; }
    public UserData data { get; set; }
}

public class UserData
{
    public int id { get; set; }
    public string fname { get; set; }
    public string lname { get; set; }
    public string email { get; set; }
}

public class ApiService
{
    // The root URL provided in your documentation
    private readonly string _baseUrl = "https://todo-list.dcism.org";
    private readonly HttpClient _client = new HttpClient();

    // --- SIGN UP API ---
    public async Task<ApiResponse> SignUpAsync(string fName, string lName, string email, string password, string confirmPassword)
    {
        var url = $"{_baseUrl}/signup_action.php"; // (Note: fixed the line-break typo from the docs)

        var payload = new
        {
            first_name = fName,
            last_name = lName,
            email = email,
            password = password,
            confirm_password = confirmPassword
        };

        var response = await _client.PostAsJsonAsync(url, payload);
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiResponse>(jsonResponse);
    }

    // --- SIGN IN API ---
    public async Task<ApiResponse> SignInAsync(string email, string password)
    {
        // The documentation specifies this is a GET request with query parameters
        var url = $"{_baseUrl}/signin_action.php?email={email}&password={password}";

        var response = await _client.GetAsync(url);
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiResponse>(jsonResponse);
    }
}