namespace blogpessoal.Security
{
    public class Settings
    {
        private static string secret = " cb852897d5830ab73226f1ab747500578e800a542d76d3ea31e98d5e4dc68044";

        public static string Secret { get => secret; set => secret = value; }  

    }
}
