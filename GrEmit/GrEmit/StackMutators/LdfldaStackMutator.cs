using GrEmit.InstructionParameters;
using GrEmit.Utils;

namespace GrEmit.StackMutators
{
    internal class LdfldaStackMutator : StackMutator
    {
        public override void Mutate(GroboIL il, ILInstructionParameter parameter, ref EvaluationStack stack)
        {
            var field = ((FieldILInstructionParameter)parameter).Field;
            if(!field.IsStatic)
            {
                var declaringType = field.DeclaringType;
                CheckNotEmpty(il, stack);

                var instance = stack.Pop().ToType();
                if(instance.IsValueType)
                    ThrowError(il, string.Format("In order to load the field '{0}' of a value type '{1}' load an instance by ref", field, Formatter.Format(instance)));
                else if(instance.IsByRef)
                {
                    var elementType = instance.GetElementType();
                    if(elementType.IsValueType)
                    {
                        if(declaringType != elementType)
                            ThrowError(il, string.Format("Cannot load the field '{0}' of an instance of type '{1}'", field, elementType));
                    }
                    else
                        ThrowError(il, string.Format("Cannot load the field '{0}' of an instance of type '{1}'", field, instance));
                }
                else CheckCanBeAssigned(il, declaringType, instance);
            }
            stack.Push(field.FieldType.MakeByRefType());
        }
    }
}