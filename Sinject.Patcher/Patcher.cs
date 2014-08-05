using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Sinject.Runtime;
using System.Xml.Linq;

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

            // If already patched return
            if (IsPatched(method)) return;

            // Create array with arguments
            var argsVar = method.AddVariable(this.assembly.TypeRef(typeof(object[])));
            var index = CreateArgsArray(method, argsVar, 0);

            // Call method Stubs.CallStub
            var resultVar = method.AddVariable(this.assembly.TypeRef(typeof(object)));
            index = CallCallStub(type, method, argsVar, index, resultVar);

            // Branch to original method code if Stubs.CallStub returns false
            index = method.InsertIL(index, Instruction.Create(OpCodes.Brfalse, method.Body.Instructions[index]));

            // Push return value
            index = PushReturnValue(method, index, resultVar);

            // Return
            method.InsertIL(index, Instruction.Create(OpCodes.Ret));
        }

        private static int PushReturnValue(MethodDefinition method, int index, VariableDefinition resultVar)
        {
            if (method.ReturnType.FullName != "System.Void")
            {
                index = method.InsertIL(index,
                    Instruction.Create(OpCodes.Ldloc, resultVar));

                if (method.ReturnType.IsValueType)
                {
                    index = method.InsertIL(index,
                        Instruction.Create(OpCodes.Unbox_Any, method.ReturnType)); //TODO: process outs and refs
                }
            }
            return index;
        }

        private int CallCallStub(TypeDefinition type, MethodDefinition method, VariableDefinition argsVar, int index, VariableDefinition resultVar)
        {
            var callStubMethodRef = this.assembly.MethodRef(typeof(Stubs), "CallStub");
            return method.InsertIL(index,
                Instruction.Create(OpCodes.Ldstr, type.FullName),
                Instruction.Create(OpCodes.Ldstr, method.Name),
                Instruction.Create(OpCodes.Ldloc, argsVar),
                Instruction.Create(OpCodes.Ldloca_S, resultVar),
                Instruction.Create(OpCodes.Call, callStubMethodRef));
        }

        private int CreateArgsArray(MethodDefinition method, VariableDefinition argsVar, int index)
        {
            // Create array
            var argsCount = method.Parameters.Count + (method.IsStatic ? 0 : 1);
            
            index = method.InsertIL(index,
                Instruction.Create(OpCodes.Ldc_I4, argsCount),
                Instruction.Create(OpCodes.Newarr, this.assembly.TypeRef(typeof(object))),
                Instruction.Create(OpCodes.Stloc, argsVar));

            // Fill array:
            if (!method.IsStatic)
            {
                // add this
                index = method.InsertIL(index,
                    Instruction.Create(OpCodes.Ldloc, argsVar),
                    Instruction.Create(OpCodes.Ldc_I4, 0),
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Stelem_Ref));
            }
            // add parameters
            for (var i = 0; i < method.Parameters.Count; i++)
            {
                index = method.InsertIL(index,
                    Instruction.Create(OpCodes.Ldloc, argsVar),
                    Instruction.Create(OpCodes.Ldc_I4, i + (method.IsStatic ? 0 : 1)),
                    Instruction.Create(OpCodes.Ldarg, method.Parameters[i]), //TODO: process refs
                    Instruction.Create(OpCodes.Stelem_Ref)); //TODO: boxing before add to array???
            }

            return index;
        }

        private bool  IsPatched(MethodDefinition method)
        {
            //TODO: Redo with attribute marking
            var callStubMethodRef = this.assembly.MethodRef(typeof(Stubs), "CallStub");
            return method.Body.Instructions.Any(
                instr => instr.OpCode == OpCodes.Call && 
                         instr.Operand.ToString() == callStubMethodRef.ToString());
        }


        public void Save(string assemblyPath)
        {
            this.assembly.Write(assemblyPath);
        }
        
        public static void ProcessPatchesFile(string filePath)
        {
            var root = XElement.Load(filePath);
            foreach (var assembly in root.Elements("assembly"))
            {
                var assemblyPath = assembly.Attribute("path").Value;
                var patcher = new Patcher(assemblyPath);
                foreach (var type in assembly.Elements("type"))
                {
                    var typeName = type.Attribute("name").Value;
                    var methods = type.Value.Split(',').Select(token => token.Trim());
                    foreach (var methodName in methods)
                    {
                        patcher.Patch(typeName, methodName);
                    }
                }
                patcher.Save(assemblyPath);
            }
        }
    }
}
