namespace AuthService.Domain.Constants;

//Clase que contiene constantes relacionadas con el servicio de autenticación, como los nombres de los roles de usuario y otros valores que se utilizan en la lógica de autenticación y autorización.
public static class RoleConstants
{
    public const string ADMIN_ROLE = "ADMIN_ROLE";
    public const string USER_ROLE = "USER_ROLE";
    public static readonly string[] AllowedRoles = { ADMIN_ROLE, USER_ROLE };

}