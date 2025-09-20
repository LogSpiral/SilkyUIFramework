namespace SilkyUIFramework.Extensions;

internal static class BaseBodyExtensions
{
    extension(BaseBody baseBody)
    {
        public RegisterUIAttribute GetRegisterUI()
        {
            return baseBody.GetType().GetCustomAttribute<RegisterUIAttribute>();
        }

        public RegisterGlobalUIAttribute GetRegisterGlobalUI()
        {
            return baseBody.GetType().GetCustomAttribute<RegisterGlobalUIAttribute>();
        }
    }
}