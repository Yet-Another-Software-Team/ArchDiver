using ArchDiver.Core.Languages;

namespace ArchDiver.Core.Infrastructure
{
    /// <summary>
    /// Handles the initialization and registration of all plugins in the microkernel.
    /// </summary>
    public static class Bootstrapper
    {
        private static bool _isInitialized = false;

        public static void Initialize()
        {
            if (_isInitialized) return;

            // Register core language providers
            LanguageRegistry.Register(new CSharpLanguageProvider());
            LanguageRegistry.Register(new PythonLanguageProvider());
            LanguageRegistry.Register(new JavaLanguageProvider());

            _isInitialized = true;
        }
    }
}
