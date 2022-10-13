using Microsoft.Xna.Framework;
using Neo.IronLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Adventure
{
    public class Step
    {
        public LuaChunk Chunk { get; set; }

        public string Name { get; set; }

        public string LuaFile { get; set; }

        public StepState State { get; set; }

        public AdventureGlobal Environment { get; set; }
    }

    public enum StepState
    {
        NotStarted,
        Completed,
        Failed,
        Running,
    }

    public class StepLogic
    {
        private readonly Adventure adventure;

        public StepLogic(Adventure adventure)
        {
            this.adventure = adventure;
        }

        public void NextStep(string name)
        {
            this.adventure.ActivateStep(this.adventure.Steps[name]);
        }

    }

    public class AdventureGlobal : LuaGlobal
    {
        public Step Step { get; set; }

        public AdventureGlobal(Lua lua) 
            : base(lua)
        {
        }

        [LuaMember]
        public CharacterInformation Character { get; set; }

        [LuaMember]
        public LuaLogger Logger { get; set; }

        [LuaMember]
        public LogicBuilderCreator LogicCreator { get; set; }

        [LuaMember]
        public StepLogic StepLogic { get; set; }

        [LuaMember]
        public DialogBuilder Dialog { get; set; }

        [LuaMember]
        public Vector3 CreateVector(float x, float y, float z) 
            => new Vector3(x, y, z);

        public void ExecuteStep()
        {

        }

        //public Step InitializeStep(string luaFile)
        //{
        //    this.Step = new Step
        //    {
        //        Environment = this,
        //        LuaFile = luaFile,
        //        Chunk = this.Lua.CompileChunk(luaFile, new LuaCompileOptions() { ClrEnabled = true }),
        //        State = StepState.NotStarted,
        //    };

        //    _ = this.DoChunk(this.Step.Chunk);

        //    var result = this.CallMethod(InitializationMethodName);
        //    this.Step.Name = (string)result[0];
            
        //    return this.Step;
        //}

        public LuaResult CallMethod(string methodName, params object[] args)
        {
            return this.WrapCall(() => this.CallMemberDirect(methodName, args));
        }

        public LuaResult WrapCall(Func<LuaResult> scriptMethod)
        {
            try
            {
                return scriptMethod();
            }
            catch (Exception ex)
            {
                var stackTrace = LuaExceptionData.GetData(ex).FormatStackTrace(0, false);
                
                throw;
            }
        }
    }
}
