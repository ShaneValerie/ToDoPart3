using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ToDoMaui_Listview;

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
    private readonly string _baseUrl = "https://todo-list.dcism.org";
    private readonly HttpClient _client = new HttpClient(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
    });

    // --- SIGN UP ---
    public async Task<ApiResponse> SignUpAsync(string fName, string lName, string email, string password, string confirmPassword)
    {
        var payload = new
        {
            first_name = fName,
            last_name = lName,
            email = email,
            password = password,
            confirm_password = confirmPassword
        };

        var response = await _client.PostAsJsonAsync($"{_baseUrl}/signup_action.php", payload);
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiResponse>(json);
    }

    // --- SIGN IN ---
    public async Task<ApiResponse> SignInAsync(string email, string password)
    {
        var url = $"{_baseUrl}/signin_action.php?email={email}&password={password}";
        var response = await _client.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiResponse>(json);
    }

    // --- GET ITEMS (active or inactive) ---
    public async Task<List<ToDoClass>> GetItemsAsync(int userId, string status = "active")
    {
        var url = $"{_baseUrl}/getItems_action.php?status={status}&user_id={userId}";
        nvar response = await _client.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();

        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var items = new List<ToDoClass>();

        if (root.GetProperty("status").GetInt32() == 200)
        {
            foreach (var prop in root.GetProperty("data").EnumerateObject())
            {
                var v = prop.Value;
                items.Add(new ToDoClass
                {
                    item_id = v.GetProperty("item_id").GetInt32(),
                    item_name = v.GetProperty("item_name").GetString() ?? "",
                    item_description = v.GetProperty("item_description").GetString() ?? "",
                    status = v.GetProperty("status").GetString() ?? "active",
                    user_id = v.GetProperty("user_id").GetInt32()
                });
            }
        }

        return items;
    }

    // --- ADD ITEM ---
    public async Task<bool> AddItemAsync(string itemName, string itemDescription, int userId)
    {
        var payload = new
        {
            item_name = itemName,
            item_description = itemDescription,
            user_id = userId
        };

        var response = await _client.PostAsJsonAsync($"{_baseUrl}/addItem_action.php", payload);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("status").GetInt32() == 200;
    }

    // --- UPDATE ITEM ---
    public async Task<bool> UpdateItemAsync(int itemId, string itemName, string itemDescription)
    {
        var payload = new
        {
            item_id = itemId,
            item_name = itemName,
            item_description = itemDescription
        };

        var request = new HttpRequestMessage(HttpMethod.Put, $"{_baseUrl}/editItem_action.php")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        var response = await _client.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("status").GetInt32() == 200;
    }

    // --- CHANGE STATUS (active <-> inactive) ---
    public async Task<bool> ChangeStatusAsync(int itemId, string newStatus)
    {
        var payload = new
        {
            item_id = itemId,
            status = newStatus
        };

        var request = new HttpRequestMessage(HttpMethod.Put, $"{_baseUrl}/statusItem_action.php")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        var response = await _client.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("status").GetInt32() == 200;
    }

    // --- DELETE ITEM ---
    public async Task<bool> DeleteItemAsync(int itemId)
    {
        var url = $"{_baseUrl}/deleteItem_action.php?item_id={itemId}";
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        var response = await _client.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("status").GetInt32() == 200;
    }
}