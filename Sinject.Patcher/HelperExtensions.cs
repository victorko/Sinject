using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace Sinject.Patcher
{
    static class HelperExtensions
    {
        public static TypeDefinition GetType(this AssemblyDefinition assembly, string typeName)
        {
            return assembly.MainModule.Types.First(td => td.FullName == typeName);
        }

        public static MethodDefinition GetMethod(this TypeDefinition type, string methodName)
        {
            return type.Methods.First(md => md.Name == methodName);
        }

        public static VariableDefinition AddVariable(this MethodDefinition method, TypeReference varType)
        {
            var varIndex = method.Body.Variables.Count;
            method.Body.Variables.Add(new VariableDefinition(varType));
            return method.Body.Variables[varIndex];
        }

        public static TypeReference TypeRef(this AssemblyDefinition assembly, Type type)
        {
            return assembly.MainModule.Import(type);
        }

        public static MethodReference MethodRef(this AssemblyDefinition assembly, Type type, string methodName)
        {
            return assembly.MainModule.Import(type.GetMethod(methodName));
        }

        public static int InsertIL(this MethodDefinition method, int index, params Instruction[] instrs)
        {
            for (var i = 0; i < instrs.Length; i++)
            {
                method.Body.Instructions.Insert(index + i, instrs[i]);
            }
            return index + instrs.Length;
        }
    }
}
