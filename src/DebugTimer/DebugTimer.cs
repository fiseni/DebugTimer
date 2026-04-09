namespace DebugTimer
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Measures elapsed time between <see cref="Start"/> and <see cref="Stop"/> calls when the <c>DEBUG_TIMER</c> symbol is defined.
    /// </summary>
    public static class DebugTimer
    {
        private static readonly ConcurrentDictionary<string, Stopwatch> _timers = new ConcurrentDictionary<string, Stopwatch>();
        private static Action<string, string, long, string> _logger;

        /// <summary>
        /// Configures a custom logger used to write timer results.
        /// </summary>
        /// <param name="logger">The logger callback that receives message template, source, elapsed milliseconds, and timer key.</param>
        [Conditional("DEBUG_TIMER")]
        public static void Initialize(Action<string, string, long, string> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Starts a timer using an integer tag combined with caller file and member name.
        /// </summary>
        /// <param name="tag">A numeric identifier appended to the generated timer key.</param>
        /// <param name="filePath">The caller file path, provided automatically by the compiler.</param>
        /// <param name="caller">The caller member name, provided automatically by the compiler.</param>
        [Conditional("DEBUG_TIMER")]
        public static void Start(int tag = 1, [CallerFilePath] string filePath = "", [CallerMemberName] string caller = "")
            => GenerateKeyAndStart(Path.GetFileNameWithoutExtension(filePath), caller, tag.ToString());

        /// <summary>
        /// Stops a timer started with the same integer tag and caller context.
        /// </summary>
        /// <param name="tag">A numeric identifier appended to the generated timer key.</param>
        /// <param name="filePath">The caller file path, provided automatically by the compiler.</param>
        /// <param name="caller">The caller member name, provided automatically by the compiler.</param>
        [Conditional("DEBUG_TIMER")]
        public static void Stop(int tag = 1, [CallerFilePath] string filePath = "", [CallerMemberName] string caller = "")
            => GenerateKeyAndStop(Path.GetFileNameWithoutExtension(filePath), caller, tag.ToString());


        /// <summary>
        /// Starts a timer using a string tag combined with caller file and member name.
        /// </summary>
        /// <param name="tag">An optional text identifier appended to the generated timer key.</param>
        /// <param name="filePath">The caller file path, provided automatically by the compiler.</param>
        /// <param name="caller">The caller member name, provided automatically by the compiler.</param>
        [Conditional("DEBUG_TIMER")]
        public static void Start(string tag = null, [CallerFilePath] string filePath = "", [CallerMemberName] string caller = "")
            => GenerateKeyAndStart(Path.GetFileNameWithoutExtension(filePath), caller, tag);

        /// <summary>
        /// Stops a timer started with the same string tag and caller context.
        /// </summary>
        /// <param name="tag">An optional text identifier appended to the generated timer key.</param>
        /// <param name="filePath">The caller file path, provided automatically by the compiler.</param>
        /// <param name="caller">The caller member name, provided automatically by the compiler.</param>
        [Conditional("DEBUG_TIMER")]
        public static void Stop(string tag = null, [CallerFilePath] string filePath = "", [CallerMemberName] string caller = "")
            => GenerateKeyAndStop(Path.GetFileNameWithoutExtension(filePath), caller, tag);


        /// <summary>
        /// Starts or restarts a timer for the specified global key.
        /// </summary>
        /// <param name="key">The unique timer key.</param>
        [Conditional("DEBUG_TIMER")]
        public static void StartGlobal(string key)
        {
            _timers.AddOrUpdate(key, Stopwatch.StartNew(), (s, x) => { x.Restart(); return x; });
        }

        /// <summary>
        /// Stops a timer by global key and writes the elapsed time using the configured logger or <see cref="Debug.Print(string, object[])"/>.
        /// </summary>
        /// <param name="key">The unique timer key.</param>
        [Conditional("DEBUG_TIMER")]
        public static void StopGlobal(string key)
        {

            if (_timers.TryRemove(key, out var stopWatch))
            {
                stopWatch.Stop();
                if (_logger != null)
                {
                    const string messageTemplate = "{DebugTimer}: Elapsed: {DebugTimerElapsed,8} ms | {DebugTimerKey}";
                    _logger(messageTemplate, "DebugTimer", stopWatch.ElapsedMilliseconds, key);
                }
                else
                {
                    const string messageTemplate = "{0}: Elapsed: {1,8} ms | {2}";
                    Debug.Print(messageTemplate, "DebugTimer", stopWatch.ElapsedMilliseconds, key);
                }
            }
        }

        private static void GenerateKeyAndStart(string callerTypeName, string caller, string tag = null)
            => StartGlobal(GenerateKey(callerTypeName, caller, tag));

        private static void GenerateKeyAndStop(string callerTypeName, string caller, string tag = null)
            => StopGlobal(GenerateKey(callerTypeName, caller, tag));

        private static string GenerateKey(string typeName, string caller, string tag = null)
        {
            tag = tag != null ? $" - {tag}" : string.Empty;
            return $"{typeName} - {caller}{tag}";
        }
    }
}
