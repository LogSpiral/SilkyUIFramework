namespace SilkyUIFramework.Extensions;

static class BasicBodyExtensions
{
    extension(BasicBody basicBody)
    {
        public RegisterUIAttribute GetRegisterUI()
        {
            return basicBody.GetType().GetCustomAttribute<RegisterUIAttribute>();
        }

        public RegisterGlobalUIAttribute GetRegisterGlobalUI()
        {
            return basicBody.GetType().GetCustomAttribute<RegisterGlobalUIAttribute>();
        }
    }
}
