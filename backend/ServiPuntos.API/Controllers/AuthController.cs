﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

/// <summary>
/// Controlador que maneja la autenticación con Google y la gestión de tokens JWT
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    // Servicio para la generación y gestión de tokens JWT
    private readonly JwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Constructor que inyecta las dependencias necesarias
    /// </summary>
    /// <param name="jwtTokenService">Servicio para generar tokens JWT</param>
    public AuthController(JwtTokenService jwtTokenService, IConfiguration configuration)
    {
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
    }

    [HttpGet("google-login")]
    public IActionResult GoogleLogin()
    {
        // Generar un estado aleatorio
        var state = Guid.NewGuid().ToString();

        // Guardar el estado en una variable de sesión en lugar de una cookie
        try
        {
            // Intentar usar la sesión
            HttpContext.Session.SetString("GoogleOAuthState", state);
            Console.WriteLine($"[GoogleAuth] Estado guardado en sesión: {state}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GoogleAuth] Error al usar sesión: {ex.Message}");

            // Alternativa: usar una cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Path = "/",
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromMinutes(15)
            };
            Response.Cookies.Append("GoogleAuthState", state, cookieOptions);
            Console.WriteLine($"[GoogleAuth] Estado guardado en cookie: {state}");
        }


        // Parámetros de OAuth
        // cargar client ID y client secret desde appsettings.json
        //var clientId = _configuration["Authentication:Google:ClientId"];
        var clientId = _configuration["Authentication:Google:ClientId"];
        var redirectUri = "https://localhost:5019/api/auth/google-callback";
        var scope = "email profile openid";

        // Construir la URL de autorización de Google
        var googleAuthUrl =
            "https://accounts.google.com/o/oauth2/v2/auth" +
            $"?client_id={Uri.EscapeDataString(clientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            $"&response_type=code" +
            $"&scope={Uri.EscapeDataString(scope)}" +
            $"&state={Uri.EscapeDataString(state)}" +
            $"&include_granted_scopes=true";

        Console.WriteLine($"[GoogleAuth] Estado generado: {state}");
        Console.WriteLine($"[GoogleAuth] URL de redirección: {redirectUri}");

        return Redirect(googleAuthUrl);
    }


    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string state, [FromQuery] string? error)
    {
        Console.WriteLine($"[GoogleCallback] Recibido - Estado: {state}");
        Console.WriteLine($"[GoogleCallback] Código: {(code != null ? code.Substring(0, Math.Min(10, code.Length)) + "..." : "null")}");
        Console.WriteLine($"[GoogleCallback] Error: {error}");

        Console.WriteLine($"[GoogleCallback] Recibido - Estado: {state}");
        Console.WriteLine($"[GoogleCallback] Código: {(code != null ? code.Substring(0, Math.Min(10, code.Length)) + "..." : "null")}");

        // Variable para almacenar el estado guardado
        string savedState = null;

        try
        {
            // Intentar recuperar de la sesión
            savedState = HttpContext.Session.GetString("GoogleOAuthState");
            Console.WriteLine($"[GoogleCallback] Estado recuperado de sesión: {savedState}");
        }
        catch
        {
            // Si falla, intentar recuperar de la cookie
            if (Request.Cookies.TryGetValue("GoogleAuthState", out savedState))
            {
                Console.WriteLine($"[GoogleCallback] Estado recuperado de cookie: {savedState}");
            }
        }

        // Verificar el estado
        if (string.IsNullOrEmpty(savedState) || state != savedState)
        {
            return Content($@"
            <html>
                <body>
                    <h1>Error de autenticación</h1>
                    <p>El estado no coincide o no está presente.</p>
                    <p>Estado recibido: {state}</p>
                    <p>Estado guardado: {savedState ?? "No encontrado"}</p>
                </body>
            </html>", "text/html");
        }
        // Limpiar el estado de la sesión
        HttpContext.Session.Remove("GoogleOAuthState");

        try
        {
            // Intercambiar el código por tokens
            var clientId = _configuration["Authentication:Google:ClientId"];
            var clientSecret = _configuration["Authentication:Google:ClientSecret"];
            var redirectUri = "https://localhost:5019/api/auth/google-callback";

            using var httpClient = new HttpClient();
            var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code"
            });

            var tokenResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequest);
            var responseContent = await tokenResponse.Content.ReadAsStringAsync();

            if (!tokenResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"[GoogleCallback] Error en token: {responseContent}");
                return Content($"<h1>Error al obtener token</h1><pre>{responseContent}</pre>", "text/html");
            }

            // Extraer el access_token y el id_token
            var responseJson = System.Text.Json.JsonDocument.Parse(responseContent);
            var accessToken = responseJson.RootElement.GetProperty("access_token").GetString();

            // Obtener información del usuario
            var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v3/userinfo");
            userInfoRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var userInfoResponse = await httpClient.SendAsync(userInfoRequest);
            var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();

            if (!userInfoResponse.IsSuccessStatusCode)
            {
                return Content($"<h1>Error al obtener datos del usuario</h1><pre>{userInfoContent}</pre>", "text/html");
            }

            // Procesar la información del usuario
            var userInfoJson = System.Text.Json.JsonDocument.Parse(userInfoContent);
            var claims = new List<Claim>();

            // Extraer claims comunes
            if (userInfoJson.RootElement.TryGetProperty("sub", out var subElement))
                claims.Add(new Claim("sub", subElement.GetString()));

            if (userInfoJson.RootElement.TryGetProperty("email", out var emailElement))
                claims.Add(new Claim("email", emailElement.GetString()));

            if (userInfoJson.RootElement.TryGetProperty("name", out var nameElement))
                claims.Add(new Claim("name", nameElement.GetString()));
            
            claims.Add(new Claim("google_access_token", accessToken)); //Agregamos esto para tener este dato en el JWT cuando se haga logout
            claims.Add(new Claim("auth_provider", "google"));
            // Agregar rol predeterminado
            claims.Add(new Claim("role", "user"));

            // Generar token JWT
            var token = _jwtTokenService.GenerateJwtToken(claims);

            // Redireccionar al frontend con el token
            Console.WriteLine($"[GoogleCallback] Token JWT generado exitosamente");
            return Redirect($"http://localhost:3000/auth-callback?token={Uri.EscapeDataString(token)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GoogleCallback] Error: {ex.Message}");
            Console.WriteLine($"[GoogleCallback] Stack: {ex.StackTrace}");
            return Content($"<h1>Error en el proceso</h1><p>{ex.Message}</p><pre>{ex.StackTrace}</pre>", "text/html");
        }
    }

    /// <summary>
    /// Obtiene la información del usuario autenticado mediante cookies de sesión
    /// </summary>
    /// <remarks>
    /// Frontend: Útil inmediatamente después del login para verificar los datos del usuario
    /// Ejemplo: fetch('api/auth/session-userinfo').then(res => res.json())
    /// </remarks>
    /// <returns>Información de claims del usuario o error 401 si no está autenticado</returns>
    [HttpGet("session-userinfo")]
    public async Task<IActionResult> GetSessionUserInfo()
    {
        // Intenta autenticar al usuario actual usando el esquema de cookies
        // Esto es útil justo después del callback de Google, antes de que el cliente use el JWT
        var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authenticateResult.Succeeded)
            return Unauthorized();

        // Extrae y formatea los claims para devolverlos al cliente
        var userClaims = authenticateResult.Principal.Claims.Select(c => new { c.Type, c.Value });
        return Ok(userClaims);
    }

    [HttpGet("userinfo")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]  // Requiere token JWT válido
    public IActionResult GetUserInfo()
    {
        // Extrae los claims del usuario desde el token JWT proporcionado en el header
        var userClaims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(userClaims);
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            // 1. Primero intentamos obtener el token desde el header de autorización
            string jwtToken = null;
            if (HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var authHeaderValue = authHeader.FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeaderValue) && authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    jwtToken = authHeaderValue.Substring("Bearer ".Length).Trim();
                    Console.WriteLine($"[Logout] Token JWT recibido: {jwtToken.Substring(0, Math.Min(20, jwtToken.Length))}...");
                }
            }

            // 2. Si tenemos un token JWT, intentamos obtener información del usuario
            if (!string.IsNullOrEmpty(jwtToken))
            {
                try
                {
                    // Decodificar el token para obtener información
                    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadToken(jwtToken) as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;

                    if (jsonToken != null)
                    {
                        // Buscar si hay claims que indiquen que fue un login con Google
                        var subClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "sub");
                        var emailClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "email");
                        var googleAccessTokenClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "google_access_token");

                        if (googleAccessTokenClaim != null)
                        {
                            Console.WriteLine("[Logout] Detectado token de acceso de Google, revocando sesión...");

                            // Llamar al endpoint de revocación de Google con el token de acceso
                            using (var httpClient = new HttpClient())
                            {
                                var revocationUrl = $"https://oauth2.googleapis.com/revoke?token={googleAccessTokenClaim.Value}";
                                Console.WriteLine($"[Logout] Llamando a URL de revocación: {revocationUrl}");

                                var revocationResponse = await httpClient.PostAsync(
                                    "https://oauth2.googleapis.com/revoke",
                                    new FormUrlEncodedContent(new Dictionary<string, string>
                                    {
                                        ["token"] = googleAccessTokenClaim.Value
                                    })
                                );

                                var responseContent = await revocationResponse.Content.ReadAsStringAsync();
                                Console.WriteLine($"[Logout] Respuesta de revocación: {revocationResponse.StatusCode}, Contenido: {responseContent}");

                                if (revocationResponse.IsSuccessStatusCode)
                                {
                                    Console.WriteLine("[Logout] Token de Google revocado exitosamente");
                                }
                                else
                                {
                                    Console.WriteLine($"[Logout] Error al revocar token de Google: {responseContent}");
                                }
                            }
                        }
                        else if (subClaim != null || (emailClaim != null && emailClaim.Value.EndsWith("@gmail.com")))
                        {
                            Console.WriteLine("[Logout] Detectado login con Google, pero no se encontró token de acceso en el JWT");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Logout] Error al procesar token JWT: {ex.Message}");
                    Console.WriteLine($"[Logout] Stack trace: {ex.StackTrace}");
                }
            }

            // 3. Como backup, también intentamos el método original con cookies
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (authenticateResult.Succeeded)
            {
                var accessToken = authenticateResult.Properties?.GetTokenValue("access_token");
                if (!string.IsNullOrEmpty(accessToken))
                {
                    // Llamamos al endpoint de revocación de Google para invalidar el token
                    using (var httpClient = new HttpClient())
                    {
                        await httpClient.GetAsync($"https://oauth2.googleapis.com/revoke?token={accessToken}");
                        Console.WriteLine("[Logout] Token de Google revocado exitosamente");
                    }
                }
            }

            // 4. Limpiamos las cookies de autenticación en cualquier caso
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Ok(new { message = "Sesión cerrada exitosamente" });
        }
        catch (Exception ex)
        {
            // Registramos el error pero devolvemos una respuesta de éxito de todos modos
            Console.WriteLine($"[Logout] Error al revocar el token: {ex.Message}");
            return Ok(new { message = "Sesión cerrada con advertencias", warning = ex.Message });
        }
    }
}
