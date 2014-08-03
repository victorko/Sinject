using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Sinject.Runtime;

namespace Sinject.Patcher
{
    public sealed class Patcher
    {
        private AssemblyDefinition assembly;

        public Patcher(string assemblyPath)
        {
            this.assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
        }

        public void Patch(string typeName, string methodName)
        {
            var type = this.assembly.GetType(typeName);
            var method = type.GetMethod(methodName);

            var callStubMethodRef = this.assembly.MethodRef(typeof(Stubs), "CallStub");
            var isPatched = method.Body.Instructions.Any(
                instr => instr.OpCode == OpCodes.Call && instr.Operand.ToString() == callStubMethodRef.ToString());
            if (isPatched) return;

            var argsCount = method.Parameters.Count + (method.IsStatic ? 0 : 1);

            var argsVar = method.AddVariable(this.assembly.TypeRef(typeof(object[])));
            var index = method.InsertIL(0,
                Instruction.Create(OpCodes.Ldc_I4, argsCount),
                Instruction.Create(OpCodes.Newarr, this.assembly.TypeRef(typeof(object))),
                Instruction.Create(OpCodes.Stloc, argsVar));

            if (!method.IsStatic)
            {
                index = method.InsertIL(index,
                    Instruction.Create(OpCodes.Ldloc, argsVar),
                    Instruction.Create(OpCodes.Ldc_I4, 0),
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Stelem_Ref));
            }
            for (var i = 0; i < method.Parameters.Count; i++)
            {
                index = method.InsertIL(index,
                    Instruction.Create(OpCodes.Ldloc, argsVar),
                    Instruction.Create(OpCodes.Ldc_I4, i + (method.IsStatic ? 0 : 1)),
                    Instruction.Create(OpCodes.Ldarg, method.Parameters[i]),
                    Instruction.Create(OpCodes.Stelem_Ref));
            }

            var resultVar = method.AddVariable(this.assembly.TypeRef(typeof(object)));
            index = method.InsertIL(index,
                Instruction.Create(OpCodes.Ldstr, type.FullName),
                Instruction.Create(OpCodes.Ldstr, method.Name),
                Instruction.Create(OpCodes.Ldloc, argsVar),
                Instruction.Create(OpCodes.Ldloca_S, resultVar),
                Instruction.Create(OpCodes.Call, callStubMethodRef));

            index = method.InsertIL(index,
                Instruction.Create(OpCodes.Brfalse, method.Body.Instructions[index]));

            if (method.ReturnType.FullName != "System.Void")
            {
                index = method.InsertIL(index,
                    Instruction.Create(OpCodes.Ldloc, resultVar));

                if (method.ReturnType.IsValueType)
                {
                    index = method.InsertIL(index,
                        Instruction.Create(OpCodes.Unbox_Any, method.ReturnType));
                }
            }

            index = method.InsertIL(index,
                Instruction.Create(OpCodes.Ret));

        }


        public void Save(string assemblyPath)
        {
            this.assembly.Write(assemblyPath);
        }
    }
}
