using JavaResolver.Class.Code;

namespace Echo.Platforms.JavaResolver
{
    /// <summary>
    ///     Provides extension methods to JavaResolver.
    /// </summary>
    public static class JavaResolverExtensions
    {
        /// <summary>
        ///     Determines whether the instruction is a load opCode.
        /// </summary>
        /// <param name="opCode">The opCode.</param>
        /// <returns><c>true</c> if its a store opCode, <c>false</c> otherwise.</returns>
        public static bool IsALoad(this ByteOpCode opCode)
        {
            return opCode.Code switch
            {
                ByteCode.ALoad => true,
                ByteCode.ALoad_0 => true,
                ByteCode.ALoad_1 => true,
                ByteCode.ALoad_2 => true,
                ByteCode.ALoad_3 => true,
                _ => false
            };
        }
        
        /// <summary>
        ///     Determines whether the instruction is a store opCode.
        /// </summary>
        /// <param name="opCode">The opCode.</param>
        /// <returns><c>true</c> if its a store opCode, <c>false</c> otherwise.</returns>
        public static bool IsIStore(this ByteOpCode opCode)
        {
            return opCode.Code switch
            {
                ByteCode.IStore => true,
                ByteCode.IStore_0 => true,
                ByteCode.IStore_1 => true,
                ByteCode.IStore_2 => true,
                ByteCode.IStore_3 => true,
                _ => false
            };
        }
    }
}