namespace Aareon.Api.Utilities
{
    public static class AuditLogHelper
    {
        public static string AuditCreate (string ctlrName, string model, string userName) => 
            $"{ctlrName}: Create called with model: {model} by User: {userName}";
        
        public static string AuditUpdate (string ctlrName, int id, string model, string userName) => 
            $"{ctlrName}: Update called with id: {id} and model: {model} by User: {userName}";
        
        public static string AuditDelete (string ctlrName, int id, string userName) => 
            $"{ctlrName}: Delete called with id: {id} by User: {userName}";
    }
}