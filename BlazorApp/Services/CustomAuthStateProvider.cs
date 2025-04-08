using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Nodes;
using BlazorApp.Models;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorApp.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private readonly ISyncLocalStorageService _localStorage;

    public CustomAuthStateProvider(HttpClient httpClient, ISyncLocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        
        var accessToken = _localStorage.GetItem<string>("accessToken");
        if (accessToken != null)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        try
        {
              var response = await _httpClient.GetAsync("manage/info");
              if (response.IsSuccessStatusCode)
              {
                  var strResponse = await response.Content.ReadAsStringAsync();
                  var jsonResponse = JsonNode.Parse(strResponse);
                  var email = jsonResponse!["email"]!.ToString();

                  var claims = new List<Claim>
                  {
                      new (ClaimTypes.Name, email ),
                      new (ClaimTypes.Email, email)
                  };
                  
                   var identity = new ClaimsIdentity(claims, "Token");
                   user = new ClaimsPrincipal(identity);
                   return new AuthenticationState(user);
              }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return new AuthenticationState(user);
    }
   
    public async Task<FormResult> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("login", new { email, password });
            if (response.IsSuccessStatusCode)
            {
                var strResponse = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonNode.Parse(strResponse);
                var accessToken = jsonResponse?["accessToken"]?.ToString();
                var refreshToken = jsonResponse?["refreshToken"]?.ToString();
                
                _localStorage.SetItem("accessToken", accessToken);
                _localStorage.SetItem("refreshToken", refreshToken);
                
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                
                //need to refresh auth state
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                
                //success
                return new FormResult
                {
                    Succeeded = true
                };
            }
            else
            {
                return new FormResult { Succeeded = false, Errors = ["Bad Email our Password"] };
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            //throw;
        }

        return new FormResult
        {
            Succeeded = false,
            Errors = ["Connection Error"]
        };
    }
    
    public void Logout()
    {
        _localStorage.RemoveItem("accessToken");
        _localStorage.RemoveItem("refreshToken");
        _httpClient.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
    
    public async Task<FormResult> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("register", registerDto);
            if (response.IsSuccessStatusCode)
            {
                //return new FormResult { Succeeded = true };
                 return await LoginAsync(registerDto.Email, registerDto.Password);
            }
            
            //register errors
            var strResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine(strResponse);
            var jsonResponse = JsonNode.Parse(strResponse);
            var errorsObject = jsonResponse!["errors"]!.AsObject();
            
            var errorsList = new List<string>();
            errorsList.AddRange(errorsObject.Select(error => 
                error.Value![0]!.ToString()));

            var formResult = new FormResult
            {
                Succeeded = false,
                Errors = errorsList.ToArray()
            };
            return formResult;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return new FormResult { Succeeded = false, Errors = ["Connection Error"] };
    }
}

public class FormResult
{
    public bool Succeeded { get; set; }
    public string[] Errors { get; set; } = [];
}