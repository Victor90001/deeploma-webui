using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using webui.Models;
using System.Web;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace webui.Controllers
{
    [Route("/")]
    public class MainController : Controller
    {
        private const string UserCookieID = "ss_1900031310012sakdscccvq12";
        private CookieOptions loginCookieOptions = new() { Expires = DateTimeOffset.UtcNow.AddDays(1) };

        private readonly ILogger<MainController> _logger;
        private readonly IConfiguration _config;
        public MainController(ILogger<MainController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }
        [HttpGet("/")]
        public ActionResult Index()
        {
            var respCookie = Response.Cookies;
            var reqCookie = Request.Cookies;
            if (reqCookie["register"] != null && reqCookie["info"] != null)
            {
                HandleResponses("register", reqCookie["register"], reqCookie["info"]);
                respCookie.Delete("register");
                respCookie.Delete("info");
            }
            ViewLogin();
            return View();
        }

        [HttpGet("/detection")]
        public ActionResult Detection()
        {
            var respCookie = Response.Cookies;
            var reqCookie = Request.Cookies;
            if (reqCookie["register"] != null && reqCookie["info"] != null)
            {
                HandleResponses("register", reqCookie["register"], reqCookie["info"]);
                respCookie.Delete("register");
                respCookie.Delete("info");
            }
            ViewLogin();
            return View();
        }

        [HttpPost("/detection")]
        [AutoValidateAntiforgeryToken]
        public async Task<JsonNode> Detect(ImageDetection imageDetection)
        {
            if (imageDetection.uploadImage != null)
            {
                using (var target = new MemoryStream())
                {
                    await imageDetection.uploadImage.CopyToAsync(target);
                    HttpClient httpClient = new HttpClient();
                    var content = new MultipartFormDataContent();
                    content.Add(new ByteArrayContent(target.ToArray()), "file", "image.jpg");
                    content.Add(new StringContent(imageDetection.height.ToString()), "imgsz");
                    
                    var query = HttpUtility.ParseQueryString(string.Empty);
                    query["conf"] = imageDetection.conf;
                    query["iou"] = imageDetection.iou;
                    var address = string.Join("?", $"{_config["API_ADDRESS"]}/detect/{imageDetection.model}", query.ToString());

                    var response = await httpClient.PostAsync(address, content);
                    if (response != null)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var body = JsonObject.Parse(json);
                        return body;
                    }
                }
            }
            return null;
        }

        [HttpPost("/login")]
        public async Task<ActionResult> Login(User user)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var json = JsonSerializer.Serialize(new User { Username= user.Username, Password = user.Password });
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{_config["apiAddress"]}/login", content);
            if (response != null)
            {
                var body = await response.Content.ReadFromJsonAsync<ResponseApiMessage>();
                if (body.Result)
                {
                    Response.Cookies.Delete("logged");
                    AppendCookie(Response, "logged", true.ToString(), loginCookieOptions);
                    AppendCookie(Response, UserCookieID, user.Username, loginCookieOptions);
                }
                else
                {
                    Response.Cookies.Append("loginError", body.Result.ToString());
                    Response.Cookies.Append("info", body.Message);
                }
            }
            if (!string.IsNullOrEmpty(Request.Headers.Referer))
                return await Task.Run(() => Redirect(Request.Headers.Referer)); 
            return await Task.Run(() => RedirectToAction("Index"));
        }

        [HttpPost("/register")]
        public async Task<ActionResult> Register(RegisterUser registerUser)
        {
            HttpClient httpClient = new HttpClient();
            JsonContent jsonContent = JsonContent.Create(
                new RegisterUser { Username = registerUser.Username, Password = registerUser.Password, Email = registerUser.Email });
            var response = await httpClient.PostAsync($"{_config["API_ADDRESS"]}/signup", jsonContent);
            if (response != null)
            {
                var content = await response.Content.ReadFromJsonAsync<ResponseApiMessage>();
                Response.Cookies.Append("register", content.Result.ToString());
                Response.Cookies.Append("info", content.Message);
            }
            if (!string.IsNullOrEmpty(Request.Headers.Referer))
                return await Task.Run(() => Redirect(Request.Headers.Referer));
            return await Task.Run(() => RedirectToAction("Index"));
        }

        [HttpPost("/logout")]
        public async Task<ActionResult> Logout(string logout)
        {
            if (!string.IsNullOrEmpty(logout))
            {
                Response.Cookies.Delete("logged");
                Response.Cookies.Delete(UserCookieID);
            }
            if (!string.IsNullOrEmpty(Request.Headers.Referer))
                return await Task.Run(() => Redirect(Request.Headers.Referer));
            return await Task.Run(() => RedirectToAction("Index"));
        }

        private void AppendCookie(HttpResponse response, string cookieName, string value, CookieOptions? cookieOptions = null)
        {
            if (cookieOptions != null)
            {
                response.Cookies.Append(cookieName, value, cookieOptions);
            }
            else
            {
                response.Cookies.Append(cookieName, value);
            }
        }
        private void HandleResponses(string tag, string status, string message)
        {
            switch (tag)
            {
                case "login":
                    break;
                case "register":
                    ViewBag.RegisterStatus = bool.Parse(status);
                    ViewBag.RegisterInfo = message;
                    break;
                case "loginError":
                    ViewBag.LoginStatus = bool.Parse(status);
                    ViewBag.LoginError = message;
                    break;
            }
        }
        private void ViewLogin()
        {
            var reqCookies = Request.Cookies;
            if (reqCookies.ContainsKey(UserCookieID))
            {
                ViewBag.LoginName = reqCookies[UserCookieID];
                ViewBag.SignIn = true;
            }
            if (!reqCookies.ContainsKey("logged"))
            {
                if (!reqCookies.ContainsKey(UserCookieID))
                {
                    ViewBag.SignIn = false;
                    AppendCookie(Response, "logged", false.ToString());
                }
                else
                {
                    AppendCookie(Response, UserCookieID, reqCookies[UserCookieID], loginCookieOptions);
                    AppendCookie(Response, "logged", true.ToString(), loginCookieOptions);
                }
            }
        }
    }
}
