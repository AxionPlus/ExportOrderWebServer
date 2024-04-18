using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ExportOrderWebServer.Service;

public interface IHttpCustomMethods
{
    Task<AppObjectResponse> POST_Report(string url, object data, string? remarks = null);
    //Task<IActionResult> POST_FileReport(string url, object data, string? remarks = null);
}

public class HttpCustomMethods : IHttpCustomMethods
{
    private readonly IHttpClientFactory _http;
    private readonly NavigationManager Navigation;

    public HttpCustomMethods(IHttpClientFactory httpClient, NavigationManager navigation)
    {
        _http = httpClient;
        Navigation = navigation;
    }

    public async Task<AppObjectResponse> POST_Report(string url, object data, string? remarks = null)
    {
        var obj = new { Object = data, Remarks = remarks };
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

        var appObjectResponse = new AppObjectResponse();

        using (var Http = _http.CreateClient())
        {
            Http.Timeout = new TimeSpan(0, 10, 0);
            var stringContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            //var response = await Http.PostAsync($"{ApplicationParameter.UploadFileUrl}{url}", stringContent).ConfigureAwait(false);
            var response = await Http.PostAsync($"{Navigation.BaseUri}file/UploadFile/{url}", stringContent).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                //var res = response.Content.Headers.ContentDisposition;
                var resultString = await response.Content.ReadAsStringAsync();
                appObjectResponse = JsonConvert.DeserializeObject<AppObjectResponse>(resultString);
            }
            else
            {
                appObjectResponse.ErrorAdd($"StatusCode: {response.StatusCode}");
            }
        }
        return appObjectResponse;
    }

    //public async Task<IActionResult> POST_FileReport(string url, object data, string? remarks = null)
    //{
    //    var obj = new { Object = data, Remarks = remarks };
    //    var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

    //    var appObjectResponse = new AppObjectResponse();

    //    using (var Http = _http.CreateClient())
    //    {
    //        Http.Timeout = new TimeSpan(0, 10, 0);
    //        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    //        //var response = await Http.PostAsync($"{ApplicationParameter.UploadFileUrl}{url}", stringContent).ConfigureAwait(false);
    //        var response = await Http.PostAsync($"{Navigation.BaseUri}file/UploadFile/{url}", stringContent).ConfigureAwait(false);

    //        if (response.IsSuccessStatusCode)
    //        {
    //            var res = response.Content.Headers.ContentDisposition;
    //            //var resultString = await response.Content.ReadAsStringAsync();
    //            //appObjectResponse = JsonConvert.DeserializeObject<AppObjectResponse>(resultString);
    //        }
    //        else
    //        {
    //            //appObjectResponse.ErrorAdd($"StatusCode: {response.StatusCode}");
    //        }

    //    }
    //    //return appObjectResponse;
    //    return (IActionResult)Results.Empty;
    //}

    //public Task<AppObjectResponse> POST_Report(string url, object data, string? remarks = null)
    //{
    //    throw new NotImplementedException();
    //}
}
