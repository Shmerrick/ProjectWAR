using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameWork
{
    public class CircularBuffer<T>
    {
        T[] _buffer;
        int _head;
        int _tail;
        int _length;
        int _bufferSize;


        public CircularBuffer(int bufferSize)
        {
            _buffer = new T[bufferSize];
            _bufferSize = bufferSize;
            _head = bufferSize - 1;
        }

        public bool IsEmpty
        {
            get { return _length == 0; }
        }

        public bool IsFull
        {
            get { return _length == _bufferSize; }
        }

        public T Dequeue()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Queue exhausted");

            T dequeued = _buffer[_tail];
            _tail = NextPosition(_tail);
            _length--;
            return dequeued;
        }

        private int NextPosition(int position)
        {
            return (position + 1) % _bufferSize;
        }

        public void Enqueue(T toAdd)
        {
            try
            {
                _head = NextPosition(_head);
                _buffer[_head] = toAdd;
                if (IsFull)
                    _tail = NextPosition(_tail);
                else
                    _length++;
            }
            catch (Exception)
            {

            }

        }
    }
}
