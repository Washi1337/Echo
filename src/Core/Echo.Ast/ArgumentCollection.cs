using System.Collections;
using System.Collections.Generic;

namespace Echo.Ast
{
    /// <summary>
    /// Responsible to keep <see cref="NodeBase{TInstruction}.Children"/> and
    /// <see cref="InstructionExpression{TInstruction}.Arguments"/> in sync
    /// </summary>
    public class ArgumentCollection<TInstruction> : IList<ExpressionBase<TInstruction>>
    {
        private readonly List<ExpressionBase<TInstruction>> _backingList;
        
        /// <summary>
        /// Creates a new instance of <see cref="ArgumentCollection{TInstruction}"/>
        /// </summary>
        /// <param name="owner">The <see cref="NodeBase{TInstruction}"/> that owns this instance</param>
        /// <param name="expressions">The expressions to initialize this instance with</param>
        public ArgumentCollection(NodeBase<TInstruction> owner, IEnumerable<ExpressionBase<TInstruction>> expressions)
        {
            _backingList = new List<ExpressionBase<TInstruction>>(expressions);
            Owner = owner;
            
            foreach (var item in _backingList)
                Owner.Children.Add(item);
        }

        /// <inheritdoc />
        public int Count => _backingList.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <summary>
        /// The <see cref="NodeBase{TInstruction}"/> that owns this <see cref="ArgumentCollection{TInstruction}"/>
        /// </summary>
        public NodeBase<TInstruction> Owner
        {
            get;
        }

        /// <inheritdoc />
        public ExpressionBase<TInstruction> this[int index]
        {
            get => _backingList[index];
            set
            {
                Owner.Children[index] = value;
                _backingList[index] = value;
            }
        }

        /// <inheritdoc />
        public int IndexOf(ExpressionBase<TInstruction> item) => _backingList.IndexOf(item);

        /// <inheritdoc />
        public void Insert(int index, ExpressionBase<TInstruction> item)
        {
            Owner.Children.Insert(index, item);
            _backingList.Insert(index, item);
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            Owner.Children.RemoveAt(index);
            _backingList.RemoveAt(index);
        }

        /// <inheritdoc />
        public IEnumerator<ExpressionBase<TInstruction>> GetEnumerator() => _backingList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _backingList.GetEnumerator();

        /// <inheritdoc />
        public void Add(ExpressionBase<TInstruction> item)
        {
            Owner.Children.Add(item);
            _backingList.Add(item);
        }

        /// <inheritdoc />
        public void Clear()
        {
            Owner.Children.Clear();
            _backingList.Clear();
        }

        /// <inheritdoc />
        public bool Contains(ExpressionBase<TInstruction> item) => _backingList.Contains(item);

        /// <inheritdoc />
        public void CopyTo(ExpressionBase<TInstruction>[] array, int arrayIndex) =>
            _backingList.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public bool Remove(ExpressionBase<TInstruction> item)
        {
            Owner.Children.Remove(item);
            return _backingList.Remove(item);
        }
    }
}