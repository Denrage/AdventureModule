using Microsoft.Xna.Framework;
using Neo.IronLua;
using System;
using System.IO;

namespace Denrage.AdventureModule.Adventure
{
    public class AdventureScript
    {
        private AdventureElementCreator creator;
        private readonly LuaGlobal luaEnvironment;
        private readonly CharacterInformation characterInformation;
        private readonly LogicBuilderCreator logicCreator;
        private readonly LuaLogger logger;
        private readonly Lua luaEngine;
        private bool scriptValid = false;

        public void Update(GameTime gameTime)
        {
            if (scriptValid)
            {
                var elements = this.creator.Elements;
                foreach (var item in elements)
                {
                    item.Update(gameTime);
                }

                if (this.luaEnvironment.ContainsKey("update"))
                {
                    (this.luaEnvironment["update"] as Func<LuaResult>)();
                }
            }
        }

        public void EmoteUsed()
        {
            this.characterInformation.FireEmoteUsed();
        }

        public AdventureScript()
        {
            var file = @"D:\Repos\AdventureModule\test.lua";
            var fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(file), Path.GetFileName(file));
            fileWatcher.Changed += (s, e) =>
            {
                this.LoadScript(file);
            };
            fileWatcher.EnableRaisingEvents = true;
            this.creator = new AdventureElementCreator();
            this.characterInformation = new CharacterInformation();
            this.logicCreator = new LogicBuilderCreator();
            this.logger = new LuaLogger();
            this.luaEngine = new Lua();

            // create an environment, that is associated to the lua scripts
            this.luaEnvironment = this.luaEngine.CreateEnvironment<LuaGlobal>();


            this.luaEnvironment["Character"] = this.characterInformation;
            this.luaEnvironment["LogicCreator"] = this.logicCreator;
            this.luaEnvironment["Logger"] = this.logger;
            this.luaEnvironment["CreateVector"] = new Func<float, float, float, Vector3>((x, y, z) => new Vector3(x, y, z));

            this.LoadScript(file);
        }

        private void LoadScript(string path)
        {
            try
            {
                scriptValid = false;
                this.creator.Clear();
                var chunk = this.luaEngine.CompileChunk(path, new LuaCompileOptions() { DebugEngine = LuaStackTraceDebugger.Default });
                _ = this.luaEnvironment.DoChunk(chunk);



                if (this.luaEnvironment.ContainsKey("onStart"))
                {
                    (this.luaEnvironment["onStart"] as Func<AdventureElementCreator, LuaResult>)(this.creator);
                }

                if (this.luaEnvironment.ContainsKey("onStarted"))
                {
                    (this.luaEnvironment["onStarted"] as Func<LuaResult>)();
                }

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"Expception: {e.Message}");
                var d = LuaExceptionData.GetData(e); // get stack trace
                System.Diagnostics.Debug.WriteLine("StackTrace: {0}", d.FormatStackTrace(0, false));
            }
            scriptValid = true;
        }
    }


}


