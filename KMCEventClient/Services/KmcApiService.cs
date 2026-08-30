using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KMCEventClient.Models;

namespace KMCEventClient.Services;

public class KmcApiService
{
    private readonly HttpClient _httpClient;

    public KmcApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<EventViewModel>> GetEventsAsync(DateTime? date = null, string? type = null)
    {
        var query = new List<string>();
        if (date.HasValue)
        {
            query.Add($"date={Uri.EscapeDataString(date.Value.ToString("yyyy-MM-dd"))}");
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query.Add($"type={Uri.EscapeDataString(type.Trim())}");
        }

        var url = "api/events" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
        return await _httpClient.GetFromJsonAsync<List<EventViewModel>>(url) ?? new List<EventViewModel>();
    }

    public async Task<EventViewModel?> GetEventAsync(int eventId)
    {
        var response = await _httpClient.GetAsync($"api/events/{eventId}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<EventViewModel>();
    }

    public async Task<List<VenueViewModel>> GetVenuesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<VenueViewModel>>("api/venues")
            ?? new List<VenueViewModel>();
    }

    public async Task<ApiResult<AuthResponseViewModel>> LoginAsync(LoginViewModel model)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", model);
        if (!response.IsSuccessStatusCode)
        {
            return new ApiResult<AuthResponseViewModel>
            {
                Success = false,
                Message = await ReadErrorMessageAsync(response)
            };
        }

        return new ApiResult<AuthResponseViewModel>
        {
            Success = true,
            Data = await response.Content.ReadFromJsonAsync<AuthResponseViewModel>()
        };
    }

    public async Task<ApiResult<object>> RegisterOrganizerAsync(RegisterOrganizerViewModel model)
    {
        var request = new
        {
            model.OrganizerName,
            model.Email,
            model.PhoneNumber,
            model.OrganizationName,
            model.Password
        };

        var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
        return new ApiResult<object>
        {
            Success = response.IsSuccessStatusCode,
            Message = response.IsSuccessStatusCode
                ? "Organizer account created successfully. You can now log in."
                : await ReadErrorMessageAsync(response)
        };
    }

    public async Task<List<EventViewModel>> GetMyEventsAsync(string token)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, "api/events/mine", token);
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return new List<EventViewModel>();
        }

        return await response.Content.ReadFromJsonAsync<List<EventViewModel>>() ?? new List<EventViewModel>();
    }

    public async Task<ApiResult<EventViewModel>> CreateEventAsync(EventFormViewModel model, string token)
    {
        var requestBody = BuildEventRequest(model);
        using var request = CreateAuthorizedRequest(HttpMethod.Post, "api/events", token);
        request.Content = JsonContent.Create(requestBody);
        var response = await _httpClient.SendAsync(request);

        return await ReadEventResultAsync(response);
    }

    public async Task<ApiResult<EventViewModel>> UpdateEventAsync(EventFormViewModel model, string token)
    {
        var requestBody = BuildEventRequest(model);
        using var request = CreateAuthorizedRequest(HttpMethod.Put, $"api/events/{model.EventID}", token);
        request.Content = JsonContent.Create(requestBody);
        var response = await _httpClient.SendAsync(request);

        return await ReadEventResultAsync(response);
    }

    public async Task<ApiResult<object>> DeleteEventAsync(int eventId, string token)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Delete, $"api/events/{eventId}", token);
        var response = await _httpClient.SendAsync(request);

        return new ApiResult<object>
        {
            Success = response.IsSuccessStatusCode,
            Message = response.IsSuccessStatusCode
                ? "Event deleted successfully."
                : await ReadErrorMessageAsync(response)
        };
    }

    public async Task<ApiResult<RegistrationViewModel>> RegisterForEventAsync(RegistrationFormViewModel model)
    {
        var request = new
        {
            model.EventID,
            model.FullName,
            model.Email,
            model.PhoneNumber
        };

        var response = await _httpClient.PostAsJsonAsync("api/registrations", request);
        if (!response.IsSuccessStatusCode)
        {
            return new ApiResult<RegistrationViewModel>
            {
                Success = false,
                Message = await ReadErrorMessageAsync(response)
            };
        }

        return new ApiResult<RegistrationViewModel>
        {
            Success = true,
            Message = "Registration completed successfully.",
            Data = await response.Content.ReadFromJsonAsync<RegistrationViewModel>()
        };
    }

    public async Task<ApiResult<List<RegistrationViewModel>>> GetEventRegistrationsAsync(int eventId, string token)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, $"api/registrations/event/{eventId}", token);
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return new ApiResult<List<RegistrationViewModel>>
            {
                Success = false,
                Message = await ReadErrorMessageAsync(response)
            };
        }

        return new ApiResult<List<RegistrationViewModel>>
        {
            Success = true,
            Data = await response.Content.ReadFromJsonAsync<List<RegistrationViewModel>>() ?? new()
        };
    }

    private static object BuildEventRequest(EventFormViewModel model) => new
    {
        model.EventName,
        model.Description,
        model.EventDate,
        model.EventTime,
        model.EventType,
        model.Capacity,
        model.VenueID
    };

    private static HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url, string token)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static async Task<ApiResult<EventViewModel>> ReadEventResultAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            return new ApiResult<EventViewModel>
            {
                Success = false,
                Message = await ReadErrorMessageAsync(response)
            };
        }

        return new ApiResult<EventViewModel>
        {
            Success = true,
            Data = await response.Content.ReadFromJsonAsync<EventViewModel>()
        };
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body))
        {
            return $"The API returned {(int)response.StatusCode} {response.ReasonPhrase}.";
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("message", out var message))
            {
                return message.GetString() ?? "The request could not be completed.";
            }

            if (document.RootElement.TryGetProperty("title", out var title))
            {
                return title.GetString() ?? "The request could not be completed.";
            }
        }
        catch (JsonException)
        {
            // Fall back to the response body when the API did not return JSON.
        }

        return body;
    }
}
