using Signature.Data;
using Signature.Utils.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Signature.SignatureService
{
    public class SignatureBuilder
    {
        #region Members        

        /// <summary>
        /// Поток для чтения файла.
        /// </summary>
        private readonly FileStream _fsReader;

        /// <summary>
        /// Адрес файла для чтения.
        /// </summary>
        private readonly string _sourceFile;

        /// <summary>
        /// Признак отмены обработки.
        /// </summary>
        private bool _isCancelled;

        /// <summary>
        /// Признак окончания чтения файла.
        /// </summary>
        private bool _isReaded;

        /// <summary>
        /// Количество потоков хеширования.
        /// </summary>
        private readonly static int _encryptThreadsCount = Environment.ProcessorCount;

        /// <summary>
        /// Количество завершенных потоков хеширования.
        /// </summary>
        private int _finishedEncryptThreads = 0;

        /// <summary>
        /// Событие для сигнала об окончании записи всех хешей.
        /// </summary>
        static readonly AutoResetEvent autoEvent = new AutoResetEvent(false);

        /// <summary>
        /// Потоки для хеширования.
        /// </summary>
        private readonly Thread[] _encryptThreads = new Thread[_encryptThreadsCount];

        /// <summary>
        /// Очередь для чтения.
        /// </summary>
        private readonly QueueManager _queueReader = new QueueManager();

        /// <summary>
        /// Очередь для записи.
        /// </summary>
        private readonly QueueManager _queueWriter = new QueueManager();

        /// <summary>
        /// Мьютекс для чтения.
        /// </summary>
        private readonly static Mutex _mutexReader = new Mutex();

        /// <summary>
        /// Класс для обертки прочитанных блоков.
        /// </summary>
        private readonly BlockManager _blockManager = new BlockManager();

        /// <summary>
        /// Сервис для записи хешей.
        /// </summary>
        private readonly IWriter _writer;

        /// <summary>
        /// Сервис для хеширования.
        /// </summary>
        private readonly IEncrypter _encrypter;

        #endregion

        #region Properties

        public bool IsSucceeded { get; set; }

        public int BlockSize { get; set; }

        #endregion

        #region Constructors

        public SignatureBuilder(string sourceFile, int blockSize, IEncrypter encrypter, IWriter writer)
        {
            _sourceFile = sourceFile;
            _fsReader = new FileStream(_sourceFile, FileMode.Open);
            BlockSize = blockSize;
            _encrypter = encrypter;
            _writer = writer;
        }

        #endregion

        #region Methods

        public void Launch()
        {
            var readThread = new Thread(() => Read());
            readThread.Start();

            for (var i = 0; i < _encryptThreadsCount; i++)
            {
                _encryptThreads[i] = new Thread(() => Encrypt(i));
                _encryptThreads[i].Start();              
            }      

            ThreadPool.QueueUserWorkItem(new WaitCallback(Write), autoEvent);
            autoEvent.WaitOne();

            IsSucceeded = !_isCancelled;
        }

        public int CallBackResult() => !_isCancelled && IsSucceeded ? 0 : 1;

        private void Read()
        {
            try
            {
                int bytesRead;
                byte[] lastBuffer;

                while (_fsReader.Position < _fsReader.Length && !_isCancelled)
                {
                    if (_fsReader.Length - _fsReader.Position <= BlockSize)
                    {
                        bytesRead = (int)(_fsReader.Length - _fsReader.Position);
                    }
                    else
                    {
                        bytesRead = BlockSize;
                    }

                    lastBuffer = new byte[bytesRead];
                    _fsReader.Read(lastBuffer, 0, bytesRead);
                    
                    var block = _blockManager.CreateByteBlock(lastBuffer);
                    _queueReader.Enqueue(block);
                }
                _fsReader.Close();
                _isReaded = true;
            }
            catch (Exception)
            {
                _isCancelled = true;
                throw;
            }
        }

        private void Encrypt(int i)
        {
            try
            {
                while (true && !_isCancelled)
                {
                    _mutexReader.WaitOne();
                    var block = _queueReader.Dequeue();
                    _mutexReader.ReleaseMutex();

                    if (block != null)
                    {
                        block.Hash = _encrypter.Encrypt(block.Buffer);
                        _queueWriter.Enqueue(block);
                    }
                    else
                    {
                        if (_queueReader.IsEmpty && _isReaded)
                        {
                            SetDone(i);
                            return;
                        }
                    }                    
                }
            }
            catch (Exception ex)
            {
                _isCancelled = true;
                throw new Exception($"Error in thread number {i}. \n Error description: { ex.Message}", ex);
            }
        }

        private void Write(object stateInfo)
        {
            try
            {
                while (true && !_isCancelled)
                {
                    var block = _queueWriter.Dequeue();
                    if (block != null)
                        _writer.Write(block);
                    else
                    {
                        if (_finishedEncryptThreads == _encryptThreadsCount && _queueWriter.IsEmpty)
                        {
                            ((AutoResetEvent)stateInfo).Set();
                            return;
                        }
                    }
                }
                ((AutoResetEvent)stateInfo).Set();
            }
            catch (Exception)
            {
                _isCancelled = true;
                throw;
            }
        }

        public void Cancel()
        {
            _isCancelled = true;
        }

        public void SetDone(int i)
        {
            _finishedEncryptThreads++;
        }

        #endregion
    }
}
