using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Alice.SDK.Core;

namespace HelloCSharp;

public class HelloPlugin : AlicePlugin
{
    public override void OnLoad(AliceHost host)
    {
        host.Log.Info("========================================");
        host.Log.Info("  Hello C# Plugin - Core API Demo");
        host.Log.Info("========================================");

        int passed = 0, failed = 0;

        void Ok(string name) { passed++; host.Log.Info($"  OK: {name}"); }
        void Fail(string name, string reason) { failed++; host.Log.Error($"  FAIL: {name}: {reason}"); }
        void Test(string name, Action fn)
        {
            try { fn(); }
            catch (Exception e) { Fail(name, e.Message); }
        }

        // 1. log
        host.Log.Info("");
        host.Log.Info("[1] Logging");
        Test("log", () =>
        {
            host.Log.Info("  info message");
            host.Log.Warn("  warn message");
            host.Log.Debug("  debug message");
            Ok("log (info/warn/debug)");
        });

        // 2. fs
        host.Log.Info("");
        host.Log.Info("[2] FileSystem");
        Test("fs.write+read", () =>
        {
            host.Files.WriteText("_test_core.txt", "Hello from Core!");
            var content = host.Files.ReadText("_test_core.txt");
            if (content != "Hello from Core!") throw new Exception($"mismatch: {content}");
            Ok($"fs.write+read = {content}");
        });
        Test("fs.exists", () =>
        {
            if (!host.Files.Exists("_test_core.txt")) throw new Exception("should exist");
            if (host.Files.Exists("_no_such_file.txt")) throw new Exception("should not exist");
            Ok("fs.exists");
        });

        // 3. kv (strongly typed)
        host.Log.Info("");
        host.Log.Info("[3] KvStore (typed)");
        Test("kv.set+get<int>", () =>
        {
            host.Kv.Set("test_number", 42);
            var v = host.Kv.Get<int>("test_number");
            if (v != 42) throw new Exception($"expected 42, got {v}");
            Ok($"kv.set+get<int> = {v}");
        });
        Test("kv.set+get<object>", () =>
        {
            host.Kv.Set("test_obj", new { name = "Alice", version = 3 });
            var v = host.Kv.GetRaw("test_obj");
            if (v == null || !v.Contains("Alice")) throw new Exception($"mismatch: {v}");
            Ok($"kv.set+get<object> = {v}");
        });

        // 4. service (typed registration + call)
        host.Log.Info("");
        host.Log.Info("[4] Services (typed)");
        Test("service.register+call", () =>
        {
            host.Services.Register("test.core.math", new MathService());
            var result = host.Services.Call<AddResult>("test.core.math", "add", new { a = 10, b = 20 });
            if (!result.Success) throw new Exception($"call failed: {result.Error}");
            if (result.Value?.Sum != 30) throw new Exception($"expected 30, got {result.Value?.Sum}");
            Ok($"service typed call = {result.Value?.Sum}");
        });

        // 5. events (typed)
        host.Log.Info("");
        host.Log.Info("[5] Events (typed)");
        Test("event.emit+on", () =>
        {
            ChatEvent? received = null;
            var handle = host.Events.On<ChatEvent>("test.core.event", e => { received = e; });
            host.Events.Emit("test.core.event", new ChatEvent { Message = "hello", From = "test" });
            if (received == null) throw new Exception("not received");
            if (received.Message != "hello") throw new Exception($"mismatch: {received.Message}");
            host.Events.Off(handle);
            Ok($"event typed = {received.Message} from {received.From}");
        });

        // 6. platform
        host.Log.Info("");
        host.Log.Info("[6] Platform");
        Test("platform", () =>
        {
            var name = host.Platform.Name;
            var dataDir = host.Platform.DataDir;
            if (string.IsNullOrEmpty(dataDir)) throw new Exception("dataDir empty");
            Ok($"platform = {name}, data = {dataDir}");
        });

        // 7. timer
        host.Log.Info("");
        host.Log.Info("[7] Timer");
        Test("timer.set+remove", () =>
        {
            var id = host.Timers.Set("10s", "core test", new { source = "core" });
            if (string.IsNullOrEmpty(id)) throw new Exception("id empty");
            host.Timers.Remove(id);
            Ok($"timer = {id} then removed");
        });

        // 8. http
        host.Log.Info("");
        host.Log.Info("[8] HTTP");
        Test("http.get", () =>
        {
            var resp = host.Http.Get("https://httpbin.org/get");
            if (!resp.Success) throw new Exception($"failed: {resp.Error}");
            if (!resp.Value!.IsSuccess) throw new Exception($"status: {resp.Value.Status}");
            Ok($"http.get status = {resp.Value.Status}");
        });

        // summary
        host.Log.Info("");
        host.Log.Info("========================================");
        host.Log.Info($"  Result: {passed} passed, {failed} failed");
        host.Log.Info("========================================");
    }

    public override void OnUnload()
    {
        Host.Log.Info("Hello C# Core plugin unloaded");
    }
}

// 测试用的 service 类
public class MathService
{
    public AddResult Add(int a, int b) => new() { Sum = a + b };
}

public class AddResult { public int Sum { get; set; } }
public class ChatEvent { public string Message { get; set; } = ""; public string From { get; set; } = ""; }

// PluginLoader 需要的静态入口 (保持兼容)
public static class PluginEntry
{
    private static HelloPlugin? _plugin;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public static void Initialize(nint hostBridgePtr)
    {
        _plugin = new HelloPlugin();
        _plugin.InternalLoad(hostBridgePtr);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public static void Shutdown()
    {
        _plugin?.InternalUnload();
        _plugin = null;
    }
}
